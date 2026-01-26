using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class ReservationService : IReservationService
    {
        private readonly AppItDbContext _context;

        public ReservationService(AppItDbContext context)
        {
            _context = context;
        }

        private DbSet<Reservation> Reservations => _context.Set<Reservation>();

        public async Task<ReservationReadDto> CreateAsync(CreateReservationDto dto)
        {
            var reservation = new Reservation
            {
                CustomerFirstName = dto.CustomerFirstName,
                CustomerLastName = dto.CustomerLastName,
                CustomerIdNumber = dto.CustomerIdNumber,
                AgencyId = dto.AgencyId,
                AgencyConsultantId = dto.AgencyConsultantId,
                AgencyVoucherReference = dto.AgencyVoucherReference,
                NumberOfPeople = dto.NumberOfPeople,
                CurrencyId = dto.CurrencyId,
                CurrencyExchangeRate = dto.CurrencyExchangeRate,
                Country = dto.Country,
                Vat = dto.Vat,
                IsInvoiced = dto.IsInvoiced,
                Notes = dto.Notes,
                AnalysisId = dto.AnalysisId,
                CustomerEmail = dto.CustomerEmail,
                ClosingByUserId = dto.ClosingByUserId,
                ClosingByUserName = dto.ClosingByUserName,
                ClosingDate = dto.ClosingDate
            };

            Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return ToReadDto(reservation);
        }

        public async Task<ReservationReadDto?> UpdateAsync(UpdateReservationDto dto)
        {
            var reservation = await Reservations.FindAsync(dto.ReservationId);
            if (reservation == null) return null;

            reservation.CustomerFirstName = dto.CustomerFirstName;
            reservation.CustomerLastName = dto.CustomerLastName;
            reservation.CustomerIdNumber = dto.CustomerIdNumber;
            reservation.AgencyId = dto.AgencyId;
            reservation.AgencyConsultantId = dto.AgencyConsultantId;
            reservation.AgencyVoucherReference = dto.AgencyVoucherReference;
            reservation.NumberOfPeople = dto.NumberOfPeople;
            reservation.CurrencyId = dto.CurrencyId;
            reservation.CurrencyExchangeRate = dto.CurrencyExchangeRate;
            reservation.Country = dto.Country;
            reservation.Vat = dto.Vat;
            reservation.IsInvoiced = dto.IsInvoiced;
            reservation.Notes = dto.Notes;
            reservation.AnalysisId = dto.AnalysisId;
            reservation.CustomerEmail = dto.CustomerEmail;
            reservation.ClosingByUserId = dto.ClosingByUserId;
            reservation.ClosingByUserName = dto.ClosingByUserName;
            reservation.ClosingDate = dto.ClosingDate;

            await _context.SaveChangesAsync();
            return ToReadDto(reservation);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var reservation = await Reservations.FindAsync(id);
            if (reservation == null) return false;

            Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ReservationReadDto?> GetByIdAsync(int id)
        {
            var reservation = await Reservations
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.CustomerId == id);
            return reservation == null ? null : ToReadDto(reservation);
        }

        public async Task<IEnumerable<ReservationReadDto>> GetAllAsync()
        {
            var reservations = await Reservations
                .AsNoTracking()
                .ToListAsync();
            return reservations.Select(ToReadDto);
        }

        private ReservationReadDto ToReadDto(Reservation r) => new()
        {
            ReservationId = r.CustomerId ?? 0,
            CustomerFirstName = r.CustomerFirstName,
            CustomerLastName = r.CustomerLastName,
            CustomerIdNumber = r.CustomerIdNumber,
            AgencyId = r.AgencyId,
            AgencyConsultantId = r.AgencyConsultantId,
            AgencyVoucherReference = r.AgencyVoucherReference,
            NumberOfPeople = r.NumberOfPeople,
            CurrencyId = r.CurrencyId,
            CurrencyExchangeRate = r.CurrencyExchangeRate,
            Country = r.Country,
            Vat = r.Vat,
            IsInvoiced = r.IsInvoiced,
            Notes = r.Notes,
            AnalysisId = r.AnalysisId,
            CustomerEmail = r.CustomerEmail,
            ClosingByUserId = r.ClosingByUserId,
            ClosingByUserName = r.ClosingByUserName,
            ClosingDate = r.ClosingDate
        };
    }
}
