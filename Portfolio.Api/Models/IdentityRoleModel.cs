using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portfolio.Api.Models
{
    public class IdentityRoleModel
    {
        public string RoleId { get; set; }
        public string Name { get; set; }
        public string UsersInRole { get; set; }
    }
}
