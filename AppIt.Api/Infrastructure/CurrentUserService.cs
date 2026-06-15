using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AppIt.Core.Interfaces;

namespace AppIt.Api.Infrastructure
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public int? UserId
        {
            get
            {
                var value = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
                return int.TryParse(value, out var id) ? id : null;
            }
        }

        public string? Email =>
            User?.FindFirstValue(ClaimTypes.Email)
            ?? User?.FindFirstValue(JwtRegisteredClaimNames.Email);

        public bool IsStaff =>
            User?.IsInRole("super") == true || User?.IsInRole("admin") == true;

        public int? ResolveMineAccountId(int? requestedAccountId)
        {
            if (IsStaff && requestedAccountId is > 0)
            {
                return requestedAccountId;
            }

            return UserId;
        }

        public bool CanAccessAccount(int? resourceAccountId)
        {
            if (IsStaff) return true;
            if (!UserId.HasValue || !resourceAccountId.HasValue) return false;
            return resourceAccountId.Value == UserId.Value;
        }
    }
}
