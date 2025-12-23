using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Desafio.Umbler.Models;
using Desafio.Umbler.Repositories;
using Desafio.Umbler.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Whois.NET;

namespace Desafio.Umbler.Services
{
    public class DomainService : IDomainService
    {
        private readonly IDnsService _dnsService;
        private readonly IWhoisService _whoisService;
        private readonly IDomainRepository _domainRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly DomainSettings _settings;
        private readonly ILogger<DomainService> _logger;

        private const string CacheKeyPrefix = "domain_info_";

        public DomainService(
            IDnsService dnsService,
            IWhoisService whoisService,
            IDomainRepository domainRepository,
            IMemoryCache memoryCache,
            DomainSettings settings,
            ILogger<DomainService> logger)
        {
            _dnsService = dnsService;
            _whoisService = whoisService;
            _domainRepository = domainRepository;
            _memoryCache = memoryCache;
            _settings = settings;
            _logger = logger;
        }

        public async Task<DomainViewModel> GetDomainInfoAsync(string domainName)
        {
            var cacheKey = $"{CacheKeyPrefix}{domainName.ToLowerInvariant()}";

            // Tentar obter do cache em memória primeiro (reduz consultas ao banco)
            if (_memoryCache.TryGetValue<DomainViewModel>(cacheKey, out var cachedViewModel))
            {
                _logger.LogDebug("Domínio encontrado no cache em memória: {DomainName}", domainName);
                return cachedViewModel;
            }

            _logger.LogDebug("Buscando domínio no banco de dados: {DomainName}", domainName);
            var domain = await _domainRepository.GetByNameAsync(domainName);

            if (domain == null)
            {
                _logger.LogInformation("Domínio não encontrado no banco. Consultando serviços externos: {DomainName}", domainName);
                domain = await QueryDomainInfoAsync(domainName);
                
                if (domain == null)
                {
                    _logger.LogWarning("Não foi possível obter informações do domínio: {DomainName}", domainName);
                    return null;
                }

                _logger.LogInformation("Domínio consultado com sucesso. IP: {Ip}, HostedAt: {HostedAt}", domain.Ip, domain.HostedAt);
                await _domainRepository.AddAsync(domain);
            }
            else
            {
                _logger.LogDebug("Domínio encontrado no banco. Verificando TTL...");
                var timeSinceUpdate = DateTime.Now.Subtract(domain.UpdatedAt).TotalSeconds;
                
                // Aplicar TTL mínimo configurável para evitar consultas muito frequentes
                var effectiveTtl = Math.Max(domain.Ttl, _settings.MinimumTtlSeconds);
                _logger.LogDebug("Tempo desde última atualização: {TimeSinceUpdate}s, TTL: {Ttl}s, TTL Efetivo: {EffectiveTtl}s", 
                    timeSinceUpdate, domain.Ttl, effectiveTtl);

                if (timeSinceUpdate > effectiveTtl)
                {
                    _logger.LogInformation("TTL expirado. Atualizando informações do domínio: {DomainName}", domainName);
                    var updatedDomain = await QueryDomainInfoAsync(domainName);
                    
                    if (updatedDomain != null)
                    {
                        domain.Name = updatedDomain.Name;
                        domain.Ip = updatedDomain.Ip;
                        domain.UpdatedAt = updatedDomain.UpdatedAt;
                        domain.WhoIs = updatedDomain.WhoIs;
                        domain.Ttl = updatedDomain.Ttl;
                        domain.HostedAt = updatedDomain.HostedAt;
                        _logger.LogInformation("Domínio atualizado com sucesso. Novo IP: {Ip}", domain.Ip);
                        await _domainRepository.UpdateAsync(domain);
                        
                        // Limpar cache em memória após atualização
                        _memoryCache.Remove(cacheKey);
                    }
                    else
                    {
                        _logger.LogWarning("Falha ao atualizar domínio. Mantendo dados do cache: {DomainName}", domainName);
                    }
                }
                else
                {
                    _logger.LogDebug("TTL ainda válido. Retornando dados do cache");
                }
            }

            _logger.LogDebug("Salvando alterações no banco de dados");
            await _domainRepository.SaveChangesAsync();

            // Buscar Name Servers
            _logger.LogDebug("Consultando Name Servers para: {DomainName}", domain.Name);
            var nameServers = await _dnsService.GetNameServersAsync(domain.Name);
            if (nameServers != null && nameServers.Count > 0)
            {
                _logger.LogDebug("Name Servers encontrados: {NameServers}", string.Join(", ", nameServers));
            }
            else
            {
                _logger.LogDebug("Nenhum Name Server encontrado para: {DomainName}", domain.Name);
            }

            // Parsear dados estruturados do WHOIS
            WhoisData whoisData = null;
            if (!string.IsNullOrWhiteSpace(domain.WhoIs))
            {
                try
                {
                    whoisData = await _whoisService.ParseWhoisDataAsync(domain.WhoIs);
                    _logger.LogDebug("Dados WHOIS parseados com sucesso para: {DomainName}", domain.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao parsear dados WHOIS para: {DomainName}", domain.Name);
                }
            }

            // Mapear para ViewModel
            var viewModel = new DomainViewModel
            {
                Name = domain.Name,
                Ip = domain.Ip,
                HostedAt = domain.HostedAt,
                NameServers = nameServers ?? new List<string>(),
                UpdatedAt = domain.UpdatedAt,
                Ttl = domain.Ttl,
                Id = domain.Id,
                WhoIs = domain.WhoIs,
                WhoisData = whoisData
            };

            // Adicionar ao cache em memória para reduzir consultas ao banco
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.MemoryCacheExpirationMinutes),
                Priority = CacheItemPriority.Normal,
                Size = 1
            };

            _memoryCache.Set(cacheKey, viewModel, cacheOptions);
            _logger.LogDebug("Domínio adicionado ao cache em memória por {Minutes} minutos: {DomainName}", 
                _settings.MemoryCacheExpirationMinutes, domainName);

            _logger.LogInformation("Consulta de domínio concluída com sucesso: {DomainName}", domainName);
            return viewModel;
        }

        private async Task<Domain> QueryDomainInfoAsync(string domainName)
        {
            try
            {
                _logger.LogDebug("Iniciando consulta WHOIS para: {DomainName}", domainName);
                var response = await _whoisService.QueryAsync(domainName);
                _logger.LogDebug("Consulta WHOIS concluída. Organization: {Organization}", response.OrganizationName);

                _logger.LogDebug("Iniciando consulta DNS para: {DomainName}", domainName);
                var dnsResult = await _dnsService.QueryAsync(domainName);
                
                if (!dnsResult.HasRecord || string.IsNullOrWhiteSpace(dnsResult.IpAddress))
                {
                    _logger.LogWarning("Nenhum registro A encontrado para o domínio: {DomainName}", domainName);
                    return null;
                }

                _logger.LogDebug("Registro A encontrado. IP: {Ip}, TTL: {Ttl}", dnsResult.IpAddress, dnsResult.Ttl);

                _logger.LogDebug("Consultando WHOIS do IP: {Ip}", dnsResult.IpAddress);
                var hostResponse = await _whoisService.QueryAsync(dnsResult.IpAddress);
                _logger.LogDebug("Consulta WHOIS do IP concluída. Organization: {Organization}", hostResponse.OrganizationName);

                // Aplicar TTL mínimo configurável para evitar consultas muito frequentes
                var effectiveTtl = Math.Max(dnsResult.Ttl, _settings.MinimumTtlSeconds);
                _logger.LogDebug("TTL aplicado: {OriginalTtl}s -> {EffectiveTtl}s (mínimo: {MinimumTtl}s)", 
                    dnsResult.Ttl, effectiveTtl, _settings.MinimumTtlSeconds);

                var domain = new Domain
                {
                    Name = domainName.ToLowerInvariant(), // Normalizar para lowercase antes de salvar
                    Ip = dnsResult.IpAddress,
                    UpdatedAt = DateTime.Now,
                    WhoIs = response.Raw,
                    Ttl = effectiveTtl, // Usar TTL efetivo (mínimo aplicado)
                    HostedAt = hostResponse.OrganizationName
                };

                return domain;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar informações do domínio: {DomainName}", domainName);
                throw;
            }
        }
    }
}

