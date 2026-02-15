using AppIt.Core.Services;

namespace AppIt.Api.Tests;

public class AuthServiceSecurityTests
{
    [Fact]
    public void HashPassword_StoresNonPlaintextAndVerifies()
    {
        var password = "Sup3rSecure!";
        var hash = AuthService.HashPassword(password);

        Assert.NotNull(hash);
        Assert.NotEqual(password, hash);
        Assert.StartsWith("APPIT$1$", hash);
        Assert.True(AuthService.VerifyPassword(password, hash));
        Assert.False(AuthService.VerifyPassword("wrong-password", hash));
    }
}
