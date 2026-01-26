using System;
using System.Collections.Generic;

namespace AppIt.Data.EntityModels
{
    public class Role
    {
        public int RoleId { get; set; }
        public string Name { get; set; } = string.Empty;

        // Optional: navigation property if accounts are linked to roles
        public List<Account>? Accounts { get; set; }
    }
}
