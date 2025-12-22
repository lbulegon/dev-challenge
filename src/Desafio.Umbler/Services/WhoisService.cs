using System.Threading.Tasks;
using Desafio.Umbler.Helpers;
using Desafio.Umbler.Models;
using Whois.NET;

namespace Desafio.Umbler.Services
{
    public class WhoisService : IWhoisService
    {
        public async Task<WhoisResponse> QueryAsync(string query)
        {
            return await WhoisClient.QueryAsync(query);
        }

        public Task<WhoisData> ParseWhoisDataAsync(string whoisRaw)
        {
            return Task.FromResult(WhoisParser.Parse(whoisRaw));
        }
    }
}

