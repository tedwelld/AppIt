using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Data.EntityDtos
{
    public class AuditedEntityDto<T> : EntityDto<T>
    {
        public DateTime? CreationTime { get; set; }
        public int? CreatorId { get; set; }
        public string? CreatorName { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public int? LastModifierUserId { get; set; }
    }
}
