using Microsoft.AspNetCore.Mvc;

namespace AppIt.Api.Infrastructure
{
    public static class AuthorizationResultExtensions
    {
        public static ActionResult? ForbidIfNot(bool allowed) =>
            allowed ? null : new ForbidResult();
    }
}
