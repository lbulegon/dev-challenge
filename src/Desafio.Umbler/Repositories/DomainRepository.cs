using System;
using System.Linq;
using System.Threading.Tasks;
using Desafio.Umbler.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Desafio.Umbler.Repositories
{
    public class DomainRepository : IDomainRepository
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<DomainRepository> _logger;

        public DomainRepository(DatabaseContext context, ILogger<DomainRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Domain> GetByNameAsync(string domainName)
        {
            try
            {
                // Busca case-insensitive para garantir que encontre mesmo com diferentes capitalizações
                // Normaliza para lowercase para garantir consistência
                if (string.IsNullOrWhiteSpace(domainName))
                {
                    _logger.LogWarning("GetByNameAsync chamado com domainName nulo ou vazio");
                    return null;
                }

                var normalizedName = domainName.ToLowerInvariant().Trim();
                _logger.LogDebug("Buscando domínio no banco: '{NormalizedName}' (original: '{DomainName}')", normalizedName, domainName);

                // Primeiro, verificar quantos registros existem no banco (para debug)
                var totalCount = await _context.Domains.CountAsync();
                _logger.LogDebug("Total de domínios no banco: {TotalCount}", totalCount);

                if (totalCount > 0)
                {
                    // Listar alguns domínios para debug (primeiros 5)
                    var sampleDomains = await _context.Domains.Take(5).Select(d => d.Name).ToListAsync();
                    _logger.LogDebug("Exemplos de domínios no banco: {SampleDomains}", string.Join(", ", sampleDomains));
                }

                // Buscar usando ToLower() - EF Core traduz para LOWER() no SQL
                var domain = await _context.Domains
                    .Where(d => d.Name != null && d.Name.ToLower() == normalizedName)
                    .FirstOrDefaultAsync();

                if (domain != null)
                {
                    _logger.LogInformation("Domínio encontrado no banco: '{Name}', IP: {Ip}", domain.Name, domain.Ip);
                }
                else
                {
                    _logger.LogWarning("Domínio não encontrado no banco: '{NormalizedName}'", normalizedName);
                    
                    // Tentar busca alternativa: buscar todos e comparar em memória (para casos extremos)
                    var allDomains = await _context.Domains.Where(d => d.Name != null).ToListAsync();
                    var foundDomain = allDomains.FirstOrDefault(d => 
                        string.Equals(d.Name, normalizedName, StringComparison.OrdinalIgnoreCase));
                    
                    if (foundDomain != null)
                    {
                        _logger.LogWarning("Domínio encontrado com busca alternativa: '{Name}' (diferente capitalização)", foundDomain.Name);
                        return foundDomain;
                    }
                }

                return domain;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar domínio no banco: '{DomainName}'", domainName);
                throw;
            }
        }

        public async Task<Domain> AddAsync(Domain domain)
        {
            await _context.Domains.AddAsync(domain);
            return domain;
        }

        public Task UpdateAsync(Domain domain)
        {
            _context.Domains.Update(domain);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

