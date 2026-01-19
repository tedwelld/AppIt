using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Data.EntityDtos
{
    public class FullAuditedEntityDto<T> : AuditedEntityDto<T>
    {
        public bool? IsDeleted { get; set; }
        public DateTime? DeletionTime { get; set; }
        public int? DeleterId { get; set; }
    }
}
