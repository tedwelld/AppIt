using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.DTOs
{
    public  class AccountCategoryDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public  class CreateAccountCategoryDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
    public class UpdateAccountCategoryDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class DeleteAccountCategoryDto 
    {
        public int Id { get; set; }
    }
}
