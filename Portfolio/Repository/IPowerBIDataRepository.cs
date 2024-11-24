using Portfolio.Models;
using System.Net.Http;

namespace Portfolio.Repository
{
    public interface IPowerBIDataRepository
    {
        Task<HttpResponseMessage> CreateUserURLAudit(string userName, string url);
        Task<string> Ping();
    }
}
