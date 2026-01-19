using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using AppIt.Data.EntityModels;

namespace AppIt.Data.AggregateRoots
{
    public class FullAuditedAggregateRoot<T> : AuditedAggregateRoot<T>
    {
        public DateTime? LastModificationTime { get; set; } = DateTime.Now;
        public int? LastModifierId { get; set; }

        public void MarkAsModified(Account account)
        {
            LastModificationTime = DateTime.Now;
            LastModifierId = account.Id;
        }

        public void MarkAsCreatedAndDeleted(Account account)
        {
            MarkAsCreated(account);
            MarkAsDeleted(account);
        }
    }
}
