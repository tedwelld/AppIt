using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AppIt.Api.Tests;

public class AuthAndIsolationApiTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public AuthAndIsolationApiTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ReturnsEnvelopeWithUser()
    {
        var client = CreateHttpsClient();

        var registerPayload = new
        {
            firstName = "Jane",
            lastName = "Tester",
            email = $"jane.login.{Guid.NewGuid():N}@test.local",
            password = "Password!123",
            preferredCurrency = "USD"
        };

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerPayload);
        await AssertStatusCodeAsync(registerResponse, HttpStatusCode.OK);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = registerPayload.email,
            password = registerPayload.password
        });

        await AssertStatusCodeAsync(loginResponse, HttpStatusCode.OK);

        using var json = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());

        var data = json.RootElement.GetProperty("data");
        Assert.True(data.TryGetProperty("user", out var user));
        Assert.False(data.TryGetProperty("tokens", out _));
        Assert.False(string.IsNullOrWhiteSpace(user.GetProperty("email").GetString()));
    }

    [Fact]
    public async Task ReservationsMine_ReturnsDataWithoutToken()
    {
        var client = CreateHttpsClient();

        var userA = await RegisterAndLoginAsync(client, $"alice.{Guid.NewGuid():N}@test.local");
        var userB = await RegisterAndLoginAsync(client, $"bob.{Guid.NewGuid():N}@test.local");

        await CreateReservationAsync(client, userA.UserId, "RES-A");
        await CreateReservationAsync(client, userB.UserId, "RES-B");

        var mineResponse = await client.GetAsync("/api/reservations/mine?page=1&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, mineResponse.StatusCode);
        using var json = JsonDocument.Parse(await mineResponse.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.GetProperty("success").GetBoolean());

        var items = json.RootElement
            .GetProperty("data")
            .GetProperty("items")
            .EnumerateArray()
            .ToList();

        Assert.NotEmpty(items);
        Assert.Contains(items, row => row.GetProperty("reference").GetString() == "RES-A");
        Assert.Contains(items, row => row.GetProperty("reference").GetString() == "RES-B");
    }

    [Fact]
    public async Task ReservationsMine_ResponseContract_IsPagedEnvelope()
    {
        var client = CreateHttpsClient();
        var auth = await RegisterAndLoginAsync(client, $"contract.{Guid.NewGuid():N}@test.local");
        await CreateReservationAsync(client, auth.UserId, "RES-CONTRACT");

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/reservations/mine?page=1&pageSize=10&sortBy=reference&sortDirection=asc");

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;
        Assert.True(root.GetProperty("success").GetBoolean());
        Assert.True(root.TryGetProperty("timestampUtc", out _));

        var data = root.GetProperty("data");
        Assert.True(data.TryGetProperty("items", out var items));
        Assert.True(items.ValueKind == JsonValueKind.Array);
        Assert.True(data.TryGetProperty("page", out _));
        Assert.True(data.TryGetProperty("pageSize", out _));
        Assert.True(data.TryGetProperty("totalCount", out _));
        Assert.True(data.TryGetProperty("totalPages", out _));
    }

    private static async Task<AuthResult> RegisterAndLoginAsync(HttpClient client, string email)
    {
        var password = "Password!123";
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test",
            lastName = "User",
            email,
            password,
            preferredCurrency = "USD"
        });

        await AssertStatusCodeAsync(registerResponse, HttpStatusCode.OK);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password
        });

        await AssertStatusCodeAsync(loginResponse, HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var data = json.RootElement.GetProperty("data");
        return new AuthResult(data.GetProperty("user").GetProperty("id").GetInt32());
    }

    private static async Task CreateReservationAsync(HttpClient client, int accountId, string reference)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/reservations");
        request.Content = JsonContent.Create(new
        {
            reference,
            voucherCode = $"VCH-{reference}",
            accountId,
            currency = "USD",
            totalAmount = 120.50m,
            status = "Pending",
            customerEmail = "customer@test.local"
        });

        var response = await client.SendAsync(request);
        await AssertStatusCodeAsync(response, HttpStatusCode.Created);
    }

    private readonly record struct AuthResult(int UserId);

    private HttpClient CreateHttpsClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }

    private static async Task AssertStatusCodeAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        if (response.StatusCode == expected)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        Assert.Fail($"Expected {(int)expected} but received {(int)response.StatusCode}. Body: {body}");
    }
}
