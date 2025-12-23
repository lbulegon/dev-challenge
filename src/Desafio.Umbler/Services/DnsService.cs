using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DnsClient;

namespace Desafio.Umbler.Services
{
    public class DnsService : IDnsService
    {
        public async Task<DnsQueryResult> QueryAsync(string domain)
        {
            var lookupOptions = new LookupClientOptions
            {
                Timeout = TimeSpan.FromSeconds(10) // Timeout de 10 segundos para consulta DNS completa
            };
            var lookup = new LookupClient(lookupOptions);
            
            // Tentar primeiro com QueryType.A (mais confiável para obter IP)
            IDnsQueryResponse result;
            try
            {
                result = await lookup.QueryAsync(domain, QueryType.A);
            }
            catch
            {
                // Se falhar, tentar com ANY como fallback
                result = await lookup.QueryAsync(domain, QueryType.ANY);
            }
            
            var record = result.Answers.ARecords().FirstOrDefault();

            if (record == null)
            {
                return new DnsQueryResult
                {
                    HasRecord = false,
                    IpAddress = null,
                    Ttl = 0
                };
            }

            var address = record.Address;
            var ip = address?.ToString();

            return new DnsQueryResult
            {
                HasRecord = !string.IsNullOrWhiteSpace(ip),
                IpAddress = ip,
                Ttl = record.TimeToLive
            };
        }

        public async Task<List<string>> GetNameServersAsync(string domain)
        {
            try
            {
                var lookupOptions = new LookupClientOptions
                {
                    Timeout = TimeSpan.FromSeconds(5) // Timeout de 5 segundos para consulta de Name Servers
                };
                var lookup = new LookupClient(lookupOptions);
                var nsResult = await lookup.QueryAsync(domain, QueryType.NS);
                return nsResult.Answers.NsRecords().Select(ns => ns.NSDName.Value).ToList();
            }
            catch
            {
                // Em caso de erro, retorna lista vazia (não deve bloquear a resposta)
                return new List<string>();
            }
        }
    }
}

