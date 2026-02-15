using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace AppIt.Api.Tests;

public class TestApiFactory : WebApplicationFactory<AppIt.Api.Program>
{
    private readonly string _databaseName = $"AppItTests_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:UseInMemory"] = "true",
                ["Database:InMemoryName"] = _databaseName,
                ["ConnectionStrings:DefaultConnection"] = $"Server=(localdb)\\mssqllocaldb;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True"
            });
        });
    }
}
