using AppIt.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceReadDto> CreateAsync(CreateInvoiceDto dto);
        Task<IEnumerable<InvoiceReadDto>> GetAllAsync();
    }

}
