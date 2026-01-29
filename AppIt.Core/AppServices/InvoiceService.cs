using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.AppServices
{
    public class InvoiceService : IInvoiceService
    {
        private readonly AppItDbContext _context;

        public InvoiceService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<InvoiceReadDto> CreateAsync(CreateInvoiceDto dto)
        {
            var invoice = new Invoice
            {
                ReservationId = dto.ReservationId,
                TotalAmount = dto.TotalAmount
            };

            _context.Add(invoice);
            await _context.SaveChangesAsync();

            return new InvoiceReadDto(invoice.Id, invoice.ReservationId, invoice.TotalAmount, invoice.IsPaid);
        }

        public async Task<IEnumerable<InvoiceReadDto>> GetAllAsync()
        {
            return await _context.Set<Invoice>()
                .Select(i => new InvoiceReadDto(i.Id, i.ReservationId, i.TotalAmount, i.IsPaid))
                .ToListAsync();
        }
    }

}
