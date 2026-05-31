using AppIt.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppIt.Core.Interfaces.Services
{
    public interface IProofOfPaymentService
    {
        Task<IEnumerable<ProofOfPaymentReadDto>> GetAllAsync();
        Task<ProofOfPaymentReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<ProofOfPaymentReadDto>> GetByPaymentAsync(int paymentId);
        Task<ProofOfPaymentReadDto> CreateAsync(CreateProofOfPaymentDto dto);
        Task<ProofOfPaymentReadDto?> UpdateAsync(UpdateProofOfPaymentDto dto);
        Task<ProofOfPaymentReadDto?> VerifyAsync(int id, string verifiedBy);
        Task<bool> DeleteAsync(int id);
    }
}
