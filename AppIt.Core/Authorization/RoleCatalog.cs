using System;
using System.Collections.Generic;
using System.Linq;

namespace AppIt.Core.Authorization
{
    /// <summary>
    /// Canonical system roles and their access tier. Back-office roles receive an
    /// "admin" tier claim at token issuance so existing role-gated controllers
    /// (Roles = "super,admin") continue to authorize them, while the specific role
    /// name drives finer-grained menu/feature visibility on the client.
    /// </summary>
    public static class RoleCatalog
    {
        public const string SuperRole = "super";
        public const string AdminRole = "admin";
        public const string RegularRole = "regular";

        /// <summary>Back-office staff roles seeded into the system.</summary>
        public static readonly string[] BackOfficeRoles =
        {
            "General Manager",
            "Reservation Manager/Supervisor",
            "Driver Guide",
            "Accountant",
            "Consultant/Trainee",
            "Finance Director",
            "IT Devs",
            "admin",
            "IT Engineer/Manager",
            "Cashier",
            "Coordinator",
            "Accounts Clerk",
            "Operations Manager",
            "Marketing Director"
        };

        /// <summary>All roles that should exist in the system (includes super).</summary>
        public static IEnumerable<string> AllSeededRoles =>
            new[] { SuperRole, RegularRole }.Concat(BackOfficeRoles);

        private static readonly HashSet<string> BackOfficeSet =
            new(BackOfficeRoles.Select(r => r.ToLowerInvariant()), StringComparer.OrdinalIgnoreCase);

        /// <summary>True when the role should be granted back-office (admin tier) access.</summary>
        public static bool IsBackOffice(string? roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName)) return false;
            return BackOfficeSet.Contains(roleName.Trim().ToLowerInvariant());
        }

        /// <summary>True when the role is the top-level super administrator.</summary>
        public static bool IsSuper(string? roleName) =>
            !string.IsNullOrWhiteSpace(roleName) && roleName.Trim().Equals(SuperRole, StringComparison.OrdinalIgnoreCase);
    }
}
