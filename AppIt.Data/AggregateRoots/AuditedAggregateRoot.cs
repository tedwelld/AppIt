using AppIt.Data.EntityModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Data.AggregateRoots
{
    public class AuditedAggregateRoot<T> : BasicAggregateRoot<T>
    {
        public string Name { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public int? CreatorId { get; set; }
        // Navigation to Account is omitted to avoid ambiguous self-referencing relationships in EF
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletionTime { get; set; }
        public int? DeleterId { get; set; }
        // Navigation to Account is omitted to avoid ambiguous self-referencing relationships in EF


        public void MarkAsDeleted(Account account)
        {
            IsDeleted = true;
            DeletionTime = DateTime.Now;
            DeleterId = account.Id;
        }
         public void MarkAsCreated(Account account)
        {
            CreationTime = DateTime.Now;
            CreatorId = account.Id;
        }

    }
}
