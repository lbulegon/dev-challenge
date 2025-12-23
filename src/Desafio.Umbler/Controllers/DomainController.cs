using System;
using System.Threading.Tasks;
using Desafio.Umbler.Helpers;
using Desafio.Umbler.Services;
using Desafio.Umbler.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Desafio.Umbler.Controllers
{
    [Route("api")]
    public class DomainController : Controller
    {
        private readonly IDomainService _domainService;
        private readonly ILogger<DomainController> _logger;

        public DomainController(IDomainService domainService, ILogger<DomainController> logger)
        {
            _domainService = domainService;
            _logger = logger;
        }

        [HttpGet, Route("domain/{domainName}")]
        public async Task<IActionResult> Get(string domainName)
        {
            _logger.LogInformation("Iniciando consulta de domínio: {DomainName}", domainName);

            try
            {
                if (string.IsNullOrWhiteSpace(domainName))
                {
                    _logger.LogWarning("Tentativa de consulta com domínio vazio ou nulo");
                    return BadRequest(new { error = "Nome do domínio é obrigatório" });
                }

                // Validar formato do domínio
                var validationResult = DomainValidator.ValidateDomain(domainName);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Domínio com formato inválido: {DomainName}", domainName);
                    return BadRequest(new { error = validationResult.ErrorMessage });
                }

                // Normalizar domínio (remover protocolo, www, etc.)
                domainName = validationResult.NormalizedDomain;

                // Obter informações do domínio através do serviço
                var domainViewModel = await _domainService.GetDomainInfoAsync(domainName);

                if (domainViewModel == null)
                {
                    _logger.LogWarning("Não foi possível obter informações do domínio: {DomainName}", domainName);
                    return NotFound(new { error = $"Domínio '{domainName}' não encontrado" });
                }

                _logger.LogInformation("Consulta de domínio concluída com sucesso: {DomainName}", domainName);
                return Ok(domainViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar consulta do domínio: {DomainName}", domainName);
                
                // Detectar erros específicos de MySQL/Database
                var errorMessage = GetErrorMessage(ex);
                
                return StatusCode(500, new { error = errorMessage, message = ex.Message });
            }
        }

        private string GetErrorMessage(Exception ex)
        {
            // Verificar se é erro de conexão com banco de dados
            var exceptionMessage = ex.ToString().ToLowerInvariant();
            var innerException = ex.InnerException?.ToString().ToLowerInvariant() ?? "";
            var fullMessage = exceptionMessage + " " + innerException;

            // Verificar se é erro de MySQL
            if (fullMessage.Contains("unable to connect to any of the specified mysql hosts") ||
                fullMessage.Contains("unable to connect") && fullMessage.Contains("mysql"))
            {
                return "❌ Erro de Conexão com Banco de Dados MySQL: O servidor MySQL não está acessível. Verifique se o servidor está online e se a conexão de rede está funcionando.";
            }

            if (fullMessage.Contains("connection timeout") && fullMessage.Contains("mysql"))
            {
                return "❌ Timeout de Conexão MySQL: O servidor MySQL não respondeu a tempo. Verifique se o servidor está acessível e se não há problemas de rede ou firewall bloqueando a conexão.";
            }

            // Verificar se é erro de Entity Framework relacionado a banco
            if (ex is DbUpdateException || ex is InvalidOperationException)
            {
                if (fullMessage.Contains("mysql") || fullMessage.Contains("database") || fullMessage.Contains("connection"))
                {
                    return "❌ Erro de Banco de Dados: Não foi possível conectar ao banco de dados MySQL. Verifique se o servidor está online e acessível.";
                }
            }

            // Verificar se é erro de timeout genérico
            if (fullMessage.Contains("timeout") || fullMessage.Contains("timed out"))
            {
                return "⏱️ Timeout: A requisição demorou muito para ser processada. Isso pode ocorrer se o servidor MySQL ou serviços externos estiverem lentos ou indisponíveis.";
            }

            // Erro genérico
            return "❌ Erro ao processar a consulta. Tente novamente mais tarde.";
        }
    }
}
