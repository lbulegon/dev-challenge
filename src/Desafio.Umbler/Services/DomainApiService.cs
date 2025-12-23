using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Desafio.Umbler.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Desafio.Umbler.Services
{
    public class DomainApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DomainApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DomainResult> GetDomainAsync(string domainName)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30); // Timeout de 30 segundos
                var request = _httpContextAccessor.HttpContext?.Request;
                var baseUrl = request != null 
                    ? $"{request.Scheme}://{request.Host}" 
                    : "http://localhost:65453";
                
                httpClient.BaseAddress = new System.Uri(baseUrl);
                
                var response = await httpClient.GetAsync($"/api/domain/{domainName}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var result = JsonSerializer.Deserialize<DomainResult>(content, options);
                    return result ?? new DomainResult();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonElement>(content);
                    var error = errorResponse.TryGetProperty("error", out var errorProp) 
                        ? errorProp.GetString() 
                        : "Domínio inválido";
                    return new DomainResult 
                    { 
                        Error = error,
                        RequestStatus = 400 
                    };
                }
                else
                {
                    // Tentar extrair mensagem de erro do conteúdo se disponível
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<JsonElement>(content);
                        if (errorResponse.TryGetProperty("error", out var errorProp))
                        {
                            var errorMsg = errorProp.GetString();
                            // Mensagem já deve estar formatada pelo controller
                            return new DomainResult { Error = errorMsg ?? "Erro ao consultar domínio" };
                        }
                    }
                    catch
                    {
                        // Se não conseguir parsear, usar mensagem genérica
                    }
                    return new DomainResult { Error = "Erro ao consultar domínio. Verifique se o servidor está respondendo corretamente." };
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || ex.CancellationToken.IsCancellationRequested == false)
            {
                return new DomainResult { Error = "⏱️ Timeout: A requisição demorou mais de 30 segundos. O servidor pode estar indisponível ou muito lento. Isso pode ser causado por problemas de conexão com o banco de dados MySQL ou serviços externos." };
            }
            catch (HttpRequestException ex)
            {
                var errorMsg = ex.Message.ToLowerInvariant();
                if (errorMsg.Contains("timeout") || errorMsg.Contains("timed out"))
                {
                    return new DomainResult { Error = "⏱️ Timeout de Conexão: O servidor não respondeu a tempo. Verifique se o servidor MySQL está acessível." };
                }
                return new DomainResult { Error = "❌ Erro de Comunicação: Não foi possível comunicar com o servidor. " + ex.Message };
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.ToLowerInvariant();
                if (errorMsg.Contains("mysql") || errorMsg.Contains("database") || errorMsg.Contains("connection"))
                {
                    return new DomainResult { Error = "❌ Erro de Banco de Dados MySQL: Não foi possível conectar ao banco de dados. Verifique se o servidor MySQL está online e acessível." };
                }
                return new DomainResult { Error = "❌ Erro inesperado ao consultar domínio: " + ex.Message };
            }
        }
    }
}

