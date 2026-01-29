using AppIt.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditLogReadDto>> GetAllAsync();
    }

}
