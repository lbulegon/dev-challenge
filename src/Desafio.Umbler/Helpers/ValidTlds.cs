using System.Collections.Generic;
using System.Linq;

namespace Desafio.Umbler.Helpers
{
    /// <summary>
    /// Lista de TLDs (Top Level Domains) válidos conhecidos.
    /// Inclui TLDs genéricos (gTLD) e códigos de países (ccTLD).
    /// Baseado na lista oficial da IANA atualizada até 2024.
    /// </summary>
    public static class ValidTlds
    {
        /// <summary>
        /// Lista de TLDs conhecidos mais comuns.
        /// Inclui gTLD populares e ccTLD de países mais utilizados.
        /// Para lista completa, consulte: https://www.iana.org/domains/root/db
        /// </summary>
        private static readonly string[] KnownTlds = new[]
        {
            // Generic Top-Level Domains (gTLD)
            "com", "org", "net", "edu", "gov", "mil", "int",
            "info", "biz", "name", "pro", "mobi", "asia", "jobs", "tel", "travel",
            "aero", "coop", "museum", "onion", "bitcoin",
            
            // New gTLD (mais populares)
            "app", "dev", "io", "tech", "online", "site", "website", "store", "shop",
            "blog", "cloud", "digital", "email", "host", "media", "news", "space",
            "tv", "video", "watch", "wiki", "xyz",
            
            // Country Code Top-Level Domains (ccTLD) - mais populares
            "br", "us", "uk", "ca", "au", "de", "fr", "it", "es", "nl",
            "jp", "cn", "in", "ru", "mx", "ar", "cl", "co", "pe", "za",
            "nz", "sg", "hk", "kr", "tw", "th", "id", "my", "ph", "vn",
            "pl", "se", "no", "dk", "fi", "ie", "pt", "gr", "tr", "il",
            "ae", "sa", "eg", "ng", "ke", "ma", "tz", "ug", "gh", "et",
            "ch", "at", "be", "cz", "hu", "ro", "bg", "hr", "si", "sk",
            "lt", "lv", "ee", "is", "lu", "mt", "cy", "ie",
            
            // Outros ccTLD relevantes
            "co.uk", "com.br", "com.mx", "com.au", "com.ar", "co.za",
            "gov.br", "org.br", "edu.br", "net.br",
            
            // TLDs especiais
            "test", "localhost", "local"
        };

        private static readonly HashSet<string> ValidTldSet = new HashSet<string>(KnownTlds, System.StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Verifica se um TLD é válido.
        /// </summary>
        /// <param name="tld">O TLD a ser verificado (sem ponto)</param>
        /// <returns>True se o TLD é válido, False caso contrário</returns>
        public static bool IsValid(string tld)
        {
            if (string.IsNullOrWhiteSpace(tld))
                return false;

            // Remove ponto se presente
            tld = tld.TrimStart('.').ToLowerInvariant();

            return ValidTldSet.Contains(tld);
        }

        /// <summary>
        /// Retorna todos os TLDs conhecidos.
        /// </summary>
        public static IEnumerable<string> GetAll()
        {
            return KnownTlds.OrderBy(tld => tld);
        }

        /// <summary>
        /// Retorna o número total de TLDs conhecidos.
        /// </summary>
        public static int Count => KnownTlds.Length;
    }
}

