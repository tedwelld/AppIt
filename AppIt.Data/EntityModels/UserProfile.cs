using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Data.EntityModels
{
    public class UserProfile
    {
        public int Id { get; set; }
   
       
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        
    }

}
