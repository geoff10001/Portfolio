using Newtonsoft.Json;
using Portfolio.Models;
using System.Text;

namespace Portfolio.Repository
{
    public class PowerBIDataRepository : IPowerBIDataRepository
    {
        private readonly HttpClient httpClient;

        public PowerBIDataRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> CreateUserURLAudit(string userName, string uri)
        {
            try
            {
                var urlAudit = new URLAuditModel
                {
                    UA_User = userName,
                    UA_URL = uri
                };
                   
                var json = JsonConvert.SerializeObject(urlAudit);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                return await httpClient.PostAsync("api/PowerBIData/CreateUserURLAudit", data);
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Error in CreateUserURLAudit: {ex.Message}");

                // I could throw the exception again or handle it appropriately
                //throw;

                return new HttpResponseMessage();
            }
        }

        public async Task<string> Ping()
        {
            try
            {
                return await httpClient.GetFromJsonAsync<string>($"api/MyData/Ping");
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
