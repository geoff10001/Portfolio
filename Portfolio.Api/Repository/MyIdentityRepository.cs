using Dapper;
using Microsoft.Data.SqlClient;
using Portfolio.Api.Models;
using System.Data;

namespace Portfolio.Api.Repository
{
    public class MyIdentityRepository : IMyIdentityRepository
    {
        private readonly ConnectionString _connectionString;

        //public MyProjectRepository()
        //{
        //}

        public MyIdentityRepository(ConnectionString connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<IdentityRoleModel>> GetRoles()
        {
            const string query = @"SELECT b.ID as 'RoleID', b.[Name] as 'Name',count(a.[RoleId]) 'UsersInRole' FROM [AspNetRoles] b  left outer join [AspNetUserRoles] a on a.RoleId = b.Id group by b.ID,b.[Name] order by b.[Name] ";
            //const string query = @"SELECT top 10 'J' + [Job_Code] as 'Namex' FROM [ppsData].[JobCodes] ";

            using (var conn = new SqlConnection(_connectionString.Value))
            {
                var result = await conn.QueryAsync<IdentityRoleModel>(query);
                return result;
            }
        }

        public async Task<IEnumerable<IdentityUserModel>> GetUsers()
        {
            const string query = @"SELECT b.ID as 'UserID', b.[UserName] as 'UserName' FROM  [AspNetUsers] b   order by b.[UserName] ";
            //const string query = @"SELECT top 10 'J' + [Job_Code] as 'Namex' FROM [ppsData].[JobCodes] ";

            using (var conn = new SqlConnection(_connectionString.Value))
            {
                var result = await conn.QueryAsync<IdentityUserModel>(query);
                return result;
            }
        }

        public async Task<IEnumerable<IdentityUserInRoleModel>> GetUsersInRoles()
        {
            const string query = @"SELECT b.ID as 'RoleID', b.[Name] as 'RoleName',c.ID as 'UserID',isnull(c.UserName,'') as 'UserName' From [AspNetRoles] b left outer join [AspNetUserRoles] a on a.RoleId = b.Id left outer join [AspNetUsers] c on c.Id = a.UserId where c.id is not null order by b.[Name],c.UserName ";
            //const string query = @"SELECT top 10 'J' + [Job_Code] as 'Namex' FROM [ppsData].[JobCodes] ";
            //, isnull(c.FirstName,'') as 'FirstName' , isnull(c.LastName,'') as 'LastName' 
            using (var conn = new SqlConnection(_connectionString.Value))
            {
                var result = await conn.QueryAsync<IdentityUserInRoleModel>(query);
                return result;
            }
        }

        public async Task<IEnumerable<IdentityUserInRoleModel>> GetUsersNotInRole(string roleID)
        {

            const string query = @"SELECT distinct [UserName] as 'UserName' FROM [AspNetUsers] where [UserName] not in (SELECT distinct b.[UserName] as 'UserName' FROM [AspNetUsers] b left outer join [AspNetUserRoles] a on a.UserId = b.Id where isnull(a.RoleId,'') = @RoleID ) order by [UserName] ";
            var values = new { RoleID = roleID };
            using (var conn = new SqlConnection(_connectionString.Value))
            {
                var result = await conn.QueryAsync<IdentityUserInRoleModel>(query, values);
                return result;
            }
        }


        //not implemented as done via identity sevices but leaving as code example
        public async Task<IEnumerable<SQLRetMessageModel>> CreateRole(string roleName)
        {
            const string procedure = "[SP_My_CreateRole]";
            //var values = new { RoleName = roleName, Ending_Date = "2017.12.31" };
            var values = new { RoleName = roleName };
            using (var conn = new SqlConnection(_connectionString.Value))
            {
                var result = await conn.QueryFirstOrDefaultAsync(procedure, values, commandType: CommandType.StoredProcedure);
                return result;
            }

            //same thing but calling without saying a sp so treating lke a normal sql query, means all code will be the same

            //const string query = @"exec [SP_My_CreateRole] @RoleName ";
            //var values = new { RoleName = roleName};
            //using (var conn = new SqlConnection(_connectionString.Value))
            //{
            //    var result = await conn.QueryFirstOrDefaultAsync(query, values);
            //    return result;
            //}
        }

        public async Task<IEnumerable<IdentityRoleModel>> GetRolesNotInUser(string userName)
        {
            const string query = @"SELECT b.ID as 'RoleID', b.[Name] as 'Name',count(a.[RoleId]) 'UsersInRole' FROM [AspNetRoles] b " +
            "left outer join [AspNetUserRoles] a on a.RoleId = b.Id left outer join [AspNetUsers] c on c.Id = a.UserId " +
            "where b.ID not in  " +
            "(SELECT b.ID as 'RoleID' FROM [AspNetRoles] b " +
            "left outer join [AspNetUserRoles] a on a.RoleId = b.Id left outer join [AspNetUsers] c on c.Id = a.UserId " +
            "where isnull(c.[UserName],'') = @UserName) " +
            "group by b.ID,b.[Name] order by b.[Name] ";
            var values = new { UserName = userName };
            using (var conn = new SqlConnection(_connectionString.Value))
            {
                var result = await conn.QueryAsync<IdentityRoleModel>(query, values);
                return result;
            }

        }

        public async Task<IEnumerable<IdentityRoleModel>> GetRolesInUser(string userName)
        {
            const string query = @"SELECT b.ID as 'RoleID', b.[Name] as 'Name',count(a.[RoleId]) 'UsersInRole' FROM [AspNetRoles] b " +
            "left outer join [AspNetUserRoles] a on a.RoleId = b.Id left outer join [AspNetUsers] c on c.Id = a.UserId " +
            "where isnull(c.[UserName],'') = @UserName " +
            "group by b.ID,b.[Name] order by b.[Name] ";
            var values = new { UserName = userName };
            using (var conn = new SqlConnection(_connectionString.Value))
            {
                var result = await conn.QueryAsync<IdentityRoleModel>(query, values);
                return result;
            }

        }

    }
}
