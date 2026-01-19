using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
namespace AppIt.Data.Enums
{
    
    
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum JobTitle
        {
            None,
            ManagingDirector,
            CEO,
            Accountant,
            Consultant,
            
        }
    
}
