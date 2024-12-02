using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Portfolio.Api.Models;

namespace Portfolio.Api.Repository
{
    public interface IMyIdentityRepository
    {
        Task<IEnumerable<IdentityRoleModel>> GetRoles();
        Task<IEnumerable<IdentityUserModel>> GetUsers();
        Task<IEnumerable<IdentityUserInRoleModel>> GetUsersInRoles();

        //not implemented as done via identity sevices but leaving as code example
        Task<IEnumerable<SQLRetMessageModel>> CreateRole(string roleName);

        Task<IEnumerable<IdentityUserInRoleModel>> GetUsersNotInRole(string roleID);
        Task<IEnumerable<IdentityRoleModel>> GetRolesNotInUser(string userName);
        Task<IEnumerable<IdentityRoleModel>> GetRolesInUser(string userName);

    }
}
