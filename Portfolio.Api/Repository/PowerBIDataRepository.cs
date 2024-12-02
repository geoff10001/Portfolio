using Dapper;
using Microsoft.Data.SqlClient;
using Portfolio.Api.Models;
using System.Data;

namespace Portfolio.Api.Repository
{
    public class PowerBIDataRepository : IPowerBIDataRepository
    {

        //private readonly IDbConnection _connection;

        //public PowerBIDataRepository(IDbConnection connection)
        //{
        //    _connection = connection;
        //}



        private readonly ConnectionString _connectionString;

        public PowerBIDataRepository(ConnectionString connectionStringPB)
        {
            _connectionString = connectionStringPB;
        }

        public async Task<SQLRetMessageModel> CreateUserURLAudit(string userName, string URL)
        {
            try
            {
                const string procedure = "[SP_CreateUserURLAudit]";

                DynamicParameters param = new DynamicParameters();
                param.Add("@username", userName);
                param.Add("@url", URL);

                using (var conn = new SqlConnection(_connectionString.Value))
                {
                    // Assuming SP_CreateUserURLAudit returns a single result of type SQLRetMessageModel
                    var result = await conn.QueryFirstOrDefaultAsync<SQLRetMessageModel>(procedure, param, commandType: CommandType.StoredProcedure);

                    return result;
                }
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Error in CreateUserURLAudit: {ex.Message}");

                // Return a special result indicating an error
                return new SQLRetMessageModel
                {
                    Message = "An error occurred while processing the request.",
                    //Success = false
                    // You might include additional information in the error message or properties
                };
            }
        }




    }
}
