using System.Text.RegularExpressions;

namespace Desafio.Umbler.Helpers
{
    public static class DomainValidator
    {
        public static (bool IsValid, string ErrorMessage, string NormalizedDomain) ValidateDomain(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                return (false, "Nome do domínio é obrigatório", null);
            }

            var trimmedDomain = domain.Trim();

            // Remover protocolo se presente
            trimmedDomain = Regex.Replace(trimmedDomain, @"^https?://", "", RegexOptions.IgnoreCase);

            // Remover www. se presente (opcional)
            trimmedDomain = Regex.Replace(trimmedDomain, @"^www\.", "", RegexOptions.IgnoreCase);

            // Validar se não contém espaços ou tabs
            if (trimmedDomain.Contains(' ') || trimmedDomain.Contains('\t'))
            {
                return (false, "O domínio não pode conter espaços", null);
            }

            // Validar se não começa ou termina com ponto
            if (trimmedDomain.StartsWith(".") || trimmedDomain.EndsWith("."))
            {
                return (false, "O domínio não pode começar ou terminar com ponto", null);
            }

            // Validar se não contém dois pontos consecutivos
            if (trimmedDomain.Contains(".."))
            {
                return (false, "O domínio não pode conter pontos consecutivos", null);
            }

            // Validar que não começa ou termina com hífen
            if (trimmedDomain.StartsWith("-") || trimmedDomain.EndsWith("-"))
            {
                return (false, "O domínio não pode começar ou terminar com hífen", null);
            }

            // Regex para validar formato de domínio completo
            // Deve ter pelo menos: dominio.tld
            var domainPattern = @"^([a-z0-9]([a-z0-9-]*[a-z0-9])?\.)+[a-z]{2,}$";

            if (!Regex.IsMatch(trimmedDomain, domainPattern, RegexOptions.IgnoreCase))
            {
                return (false, "Formato de domínio inválido. Por favor, digite um domínio completo (ex: umbler.com)", null);
            }

            // Validar que cada parte do domínio tem formato válido
            var parts = trimmedDomain.Split('.');
            if (parts.Length < 2)
            {
                return (false, "Formato de domínio inválido. O domínio deve ter pelo menos um ponto separando o nome do TLD", null);
            }

            // TLD deve ter pelo menos 2 caracteres
            var tld = parts[parts.Length - 1];
            if (tld.Length < 2)
            {
                return (false, "Formato de domínio inválido. A extensão do domínio deve ter pelo menos 2 caracteres", null);
            }

            // Validar cada parte do domínio
            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];
                
                // Validar se não está vazio
                if (string.IsNullOrWhiteSpace(part))
                {
                    return (false, "Formato de domínio inválido. Cada parte do domínio não pode estar vazia", null);
                }
                
                // Validar se não é apenas hífen
                if (part == "-")
                {
                    return (false, "Formato de domínio inválido. Cada parte do domínio não pode conter apenas hífen", null);
                }
                
                // Validar caracteres permitidos
                if (!Regex.IsMatch(part, @"^[a-z0-9-]+$", RegexOptions.IgnoreCase))
                {
                    return (false, "Formato de domínio inválido. Cada parte do domínio deve conter apenas letras, números e hífens", null);
                }
                
                // Detectar padrões suspeitos comuns de erros de digitação ANTES da validação de comprimento
                var lowerPart = part.ToLowerInvariant();
                if (lowerPart == "ww" || lowerPart == "w" || lowerPart == "wwww")
                {
                    return (false, "Endereço inconsistente. Parece haver um erro de digitação. Você quis dizer 'www'? Se sim, remova o 'www' do início ou corrija a digitação (ex: terra.com.br em vez de ww.terra.com.br)", null);
                }
                
                // Validar comprimento mínimo: cada parte deve ter pelo menos 2 caracteres
                // (domínios com 1 caractere são extremamente raros e geralmente erros de digitação)
                if (part.Length < 2)
                {
                    return (false, $"Endereço inconsistente. A parte '{part}' é muito curta. Verifique se há erros de digitação no domínio", null);
                }
                
                // Validar que não termina ou começa com hífen em cada parte
                if (part.StartsWith("-") || part.EndsWith("-"))
                {
                    return (false, $"Formato de domínio inválido. A parte '{part}' não pode começar ou terminar com hífen", null);
                }
            }

            return (true, null, trimmedDomain);
        }
    }
}

