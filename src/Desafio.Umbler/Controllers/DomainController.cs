using System;
using System.Threading.Tasks;
using Desafio.Umbler.Helpers;
using Desafio.Umbler.Services;
using Desafio.Umbler.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
                return StatusCode(500, new { error = "Erro interno ao processar a requisição", message = ex.Message });
            }
        }
    }
}
