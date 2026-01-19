using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using AppIt.Data.AggregateRoots;

namespace AppIt.Data.EntityModels
{
   public class Account : FullAuditedAggregateRoot<int>
    {
      public int Id { get; set; }
        public string? Title { get; set; }
        public string FirstName { get; set; } = string.Empty;
      public string LastName { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public string AccountType { get; set; }
      public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }

}
