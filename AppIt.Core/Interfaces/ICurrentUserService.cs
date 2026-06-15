namespace AppIt.Core.Interfaces
{
    /// <summary>
    /// Resolves the authenticated caller from JWT claims.
    /// </summary>
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string? Email { get; }
        bool IsStaff { get; }

        /// <summary>
        /// For /mine endpoints: staff may pass an explicit account id; regular users always get their own.
        /// </summary>
        int? ResolveMineAccountId(int? requestedAccountId);

        /// <summary>
        /// True when the caller may view data owned by <paramref name="resourceAccountId"/>.
        /// </summary>
        bool CanAccessAccount(int? resourceAccountId);
    }
}
