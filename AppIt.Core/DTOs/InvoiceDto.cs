using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.DTOs
{
    public record CreateInvoiceDto(int ReservationId, decimal TotalAmount);
    public record InvoiceReadDto(int Id, int ReservationId, decimal TotalAmount, bool IsPaid);

}
