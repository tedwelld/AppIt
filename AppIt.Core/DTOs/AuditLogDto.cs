using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.DTOs
{
    public record AuditLogReadDto(
     int Id,
     string EntityName,
     string EntityId,
     string Action,
     string? Changes,
     int PerformedBy,
     DateTime PerformedAt
 );

}
