namespace Desafio.Umbler.Models
{
    public class DomainSettings
    {
        public int MinimumTtlSeconds { get; set; } = 60;
        public int MemoryCacheExpirationMinutes { get; set; } = 5;
    }
}

