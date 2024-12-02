using Portfolio.Api.Models;

namespace Portfolio.Api.Repository
{
    public interface IPowerBIDataRepository
    {
        //just a simple example of a method to get data from a database
        //so i can test conntection from portal to Portfolio.Api
        Task<SQLRetMessageModel> CreateUserURLAudit(string userName, string URL);
    }
}
