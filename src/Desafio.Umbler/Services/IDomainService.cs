using System.Threading.Tasks;
using Desafio.Umbler.Models;
using Desafio.Umbler.ViewModels;

namespace Desafio.Umbler.Services
{
    public interface IDomainService
    {
        Task<DomainViewModel> GetDomainInfoAsync(string domainName);
    }
}

