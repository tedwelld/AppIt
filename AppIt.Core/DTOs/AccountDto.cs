using System;

namespace AppIt.Core.DTOs
{
    public class AccountDto
    {
        public int Id { get; set; }
   
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public string PreferredCurrency { get; set; } = "USD";
        public string Role { get; set; } = "regular";
        public int? RoleId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class CreateAccountDto
    {
       
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public string PreferredCurrency { get; set; } = "USD";
        public string Role { get; set; } = "regular";
        public int? RoleId { get; set; }
        
    }

    public class UpdateAccountDto
    {
    
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? AvatarUrl { get; set; }
        public string PreferredCurrency { get; set; } = "USD";
        public string Role { get; set; } = "regular";
        public int? RoleId { get; set; }
  
        public bool IsActive { get; set; }
    }
}
