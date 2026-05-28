using System.Security.Cryptography;
using System.Text;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Data;
using AppIt.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace AppIt.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppItDbContext _context;

        public AuthService(AppItDbContext context)
        {
            _context = context;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, string? ipAddress = null)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var exists = await _context.Accounts.AnyAsync(a => a.Email.ToLower() == email);
            if (exists)
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            var regularRoleId = await ResolveOrCreateRoleIdAsync("regular");

            var account = new Account
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = email,
                Phone = dto.Phone?.Trim(),
                AvatarUrl = dto.AvatarUrl?.Trim(),
                PreferredCurrency = string.IsNullOrWhiteSpace(dto.PreferredCurrency) ? "USD" : dto.PreferredCurrency.Trim().ToUpperInvariant(),
                PasswordHash = HashPassword(dto.Password),
                RoleId = regularRoleId,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return await BuildAuthResponseAsync(account);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, string? ipAddress = null)
        {
            const int maxAttempts = 5;
            const int lockoutMinutes = 15;

            var email = dto.Email.Trim().ToLowerInvariant();
            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.Email.ToLower() == email);

            if (account == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (account.LockoutEnd.HasValue && account.LockoutEnd.Value > DateTime.UtcNow)
            {
                var remaining = (int)Math.Ceiling((account.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                throw new UnauthorizedAccessException($"Account is locked. Try again in {remaining} minute(s).");
            }

            var passwordVerified = VerifyPassword(dto.Password, account.PasswordHash);
            if (!passwordVerified && TryMigrateLegacyPasswordHash(account, dto.Password))
            {
                await _context.SaveChangesAsync();
                passwordVerified = true;
            }

            if (!passwordVerified)
            {
                account.FailedLoginAttempts++;
                if (account.FailedLoginAttempts >= maxAttempts)
                {
                    account.LockoutEnd = DateTime.UtcNow.AddMinutes(lockoutMinutes);
                    account.FailedLoginAttempts = 0;
                }
                await _context.SaveChangesAsync();
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (!account.IsActive)
            {
                throw new UnauthorizedAccessException("Account is deactivated.");
            }

            account.FailedLoginAttempts = 0;
            account.LockoutEnd = null;
            await _context.SaveChangesAsync();

            return await BuildAuthResponseAsync(account);
        }

        public async Task<PasswordResetRequestResponseDto> RequestPasswordResetAsync(PasswordResetRequestDto dto, bool includeRawToken = false, string? ipAddress = null)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email.ToLower() == email);
            if (account == null)
            {
                return new PasswordResetRequestResponseDto();
            }

            var now = DateTime.UtcNow;
            var stale = await _context.PasswordResetTokens
                .Where(t => t.AccountId == account.Id && (t.UsedAtUtc != null || t.ExpiresAtUtc <= now))
                .ToListAsync();

            if (stale.Count > 0)
            {
                _context.PasswordResetTokens.RemoveRange(stale);
            }

            var rawToken = GenerateSecureToken();
            _context.PasswordResetTokens.Add(new PasswordResetToken
            {
                AccountId = account.Id,
                TokenHash = HashToken(rawToken),
                CreatedAtUtc = now,
                ExpiresAtUtc = now.AddMinutes(30),
                CreatedByIp = ipAddress
            });

            await _context.SaveChangesAsync();

            return new PasswordResetRequestResponseDto
            {
                Message = "If the account exists, reset instructions have been issued.",
                ResetToken = includeRawToken ? rawToken : null
            };
        }

        public async Task ResetPasswordAsync(PasswordResetConfirmDto dto, string? ipAddress = null)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var tokenHash = HashToken(dto.Token.Trim());
            var now = DateTime.UtcNow;

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email.ToLower() == email);
            if (account == null)
            {
                throw new UnauthorizedAccessException("Invalid or expired reset token.");
            }

            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t =>
                    t.AccountId == account.Id
                    && t.TokenHash == tokenHash
                    && t.UsedAtUtc == null
                    && t.ExpiresAtUtc > now);

            if (resetToken == null)
            {
                throw new UnauthorizedAccessException("Invalid or expired reset token.");
            }

            account.PasswordHash = HashPassword(dto.NewPassword.Trim());
            account.UpdatedDate = now;
            resetToken.UsedAtUtc = now;
            resetToken.UsedByIp = ipAddress;

            await _context.SaveChangesAsync();
        }

        private async Task<AuthResponseDto> BuildAuthResponseAsync(Account account)
        {
            if (account.Role == null)
            {
                account.Role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == account.RoleId);
            }

            return new AuthResponseDto
            {
                User = MapToDto(account)
            };
        }

        private async Task<int> ResolveOrCreateRoleIdAsync(string roleName)
        {
            var normalized = roleName.Trim().ToLowerInvariant();
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == normalized);
            if (role != null)
            {
                return role.RoleId;
            }

            role = new Role { Name = normalized };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role.RoleId;
        }

        private static AccountDto MapToDto(Account account)
        {
            return new AccountDto
            {
                Id = account.Id,
                FirstName = account.FirstName,
                LastName = account.LastName,
                Email = account.Email,
                Phone = account.Phone,
                AvatarUrl = account.AvatarUrl,
                PreferredCurrency = account.PreferredCurrency,
                Role = account.Role?.Name ?? "regular",
                RoleId = account.RoleId,
                IsActive = account.IsActive,
                CreatedDate = account.CreatedDate,
                UpdatedDate = account.UpdatedDate
            };
        }

        public static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            const int iterations = 100_000;
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);
            return $"APPIT$1${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            var parts = storedHash.Split('$');
            if (parts.Length != 5 || !string.Equals(parts[0], "APPIT", StringComparison.Ordinal))
            {
                return false;
            }

            if (!int.TryParse(parts[2], out var iterations) || iterations < 10_000)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[3]);
            var expectedHash = Convert.FromBase64String(parts[4]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);
            return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }

        private static bool TryMigrateLegacyPasswordHash(Account account, string enteredPassword)
        {
            if (string.IsNullOrWhiteSpace(account.PasswordHash))
            {
                return false;
            }

            // Only migrate plain legacy values once; modern APPIT hashes already use PBKDF2.
            if (account.PasswordHash.StartsWith("APPIT$", StringComparison.Ordinal))
            {
                return false;
            }

            var enteredBytes = Encoding.UTF8.GetBytes(enteredPassword);
            var storedBytes = Encoding.UTF8.GetBytes(account.PasswordHash);
            if (enteredBytes.Length != storedBytes.Length || !CryptographicOperations.FixedTimeEquals(enteredBytes, storedBytes))
            {
                return false;
            }

            account.PasswordHash = HashPassword(enteredPassword);
            account.UpdatedDate = DateTime.UtcNow;
            return true;
        }

        public static string GenerateSecureToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public static string HashToken(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(bytes);
        }
    }
}
