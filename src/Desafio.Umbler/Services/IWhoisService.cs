using System.Threading.Tasks;
using Desafio.Umbler.Models;
using Whois.NET;

namespace Desafio.Umbler.Services
{
    public interface IWhoisService
    {
        Task<WhoisResponse> QueryAsync(string query);
        Task<WhoisData> ParseWhoisDataAsync(string whoisRaw);
    }
}

