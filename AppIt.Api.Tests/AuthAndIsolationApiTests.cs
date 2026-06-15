using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AppIt.Api.Controllers;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces.Services;
using AppIt.Data.EntityModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AppIt.Api.Tests;

public class AuthAndIsolationApiTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public AuthAndIsolationApiTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ReturnsEnvelopeWithUserAndToken()
    {
        var client = CreateHttpsClient();
        var auth = await RegisterAndLoginAsync(client, $"jane.login.{Guid.NewGuid():N}@test.local");

        Assert.NotEqual(0, auth.UserId);
        Assert.False(string.IsNullOrWhiteSpace(auth.Token));
    }

    [Theory]
    [InlineData("/api/payments")]
    [InlineData("/api/reservations")]
    [InlineData("/api/invoices")]
    [InlineData("/api/vouchers")]
    [InlineData("/api/support/messages/mine")]
    public async Task ProtectedEndpoints_RejectAnonymousRequests(string path)
    {
        var client = CreateHttpsClient();

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_RejectsRegularUser()
    {
        var client = CreateHttpsClient();
        var auth = await RegisterAndLoginAsync(client, $"regular.{Guid.NewGuid():N}@test.local");
        SetBearer(client, auth.Token);

        var response = await client.GetAsync("/api/admin/stats");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminStats_CapturesBookingCustomerInvoiceAndEarnings()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppItDbContext>();
        var before = await GetAdminStatsAsync(db, "daily");

        var customer = new Customer
        {
            FirstName = "Dashboard",
            Surname = "Guest",
            Email = $"dashboard.{Guid.NewGuid():N}@test.local",
            PhoneNumber = "+263 77 000 0000",
            DateUpdated = DateTime.UtcNow
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var reservation = new Reservation
        {
            Reference = $"RES-{Guid.NewGuid():N}",
            VoucherCode = $"VCH-{Guid.NewGuid():N}",
            CurrencyCode = "USD",
            TotalAmount = 149.50m,
            Status = "Pending",
            CreatedDate = DateTime.UtcNow,
            CustomerId = customer.Id,
            CustomerEmail = customer.Email
        };
        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();

        db.Invoices.Add(new Invoice
        {
            ReservationId = reservation.ReservationId,
            TotalAmount = 149.50m,
            CurrencyCode = "USD",
            Status = "Pending",
            IsPaid = false,
            IssuedDate = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var after = await GetAdminStatsAsync(db, "daily");

        Assert.Equal(before.TotalCustomers + 1, after.TotalCustomers);
        Assert.Equal(before.TotalBookings + 1, after.TotalBookings);
        Assert.Equal(before.TotalSales + 1, after.TotalSales);
        Assert.Equal(before.TotalEarnings + 149.50m, after.TotalEarnings);
    }

    [Fact]
    public async Task ReservationsMine_ResponseContract_IsPagedEnvelope()
    {
        var client = CreateHttpsClient();
        var auth = await RegisterAndLoginAsync(client, $"contract.{Guid.NewGuid():N}@test.local");
        SetBearer(client, auth.Token);
        var catalog = await SeedCheckoutCatalogAsync();
        await client.PostAsJsonAsync("/api/bookings/checkout", new
        {
            reservation = new
            {
                reference = "RES-CONTRACT",
                accountId = auth.UserId,
                currency = "USD",
                totalAmount = 120.50m,
                status = "Confirmed",
                customerEmail = "customer@test.local"
            },
            invoice = new { totalAmount = 120.50m, currency = "USD", status = "Pending" },
            payment = new { method = "Manual", amount = 120.50m, currencyCode = "USD", idempotencyKey = Guid.NewGuid().ToString("N") },
            serviceItems = new[]
            {
                new { serviceType = "Product", serviceId = catalog.ProductId, serviceName = "Contract Product", quantity = 1, unitPrice = 120.50m, totalPrice = 120.50m, currency = "USD" }
            }
        });

        var response = await client.GetAsync("/api/reservations/mine?page=1&pageSize=10&sortBy=reference&sortDirection=asc");
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

    [Fact]
    public async Task BookingCheckout_WithoutAccountId_ReturnsBadRequest()
    {
        var client = CreateHttpsClient();
        var auth = await RegisterAndLoginAsync(client, $"booking.{Guid.NewGuid():N}@test.local");
        SetBearer(client, auth.Token);

        var response = await client.PostAsJsonAsync("/api/bookings/checkout", new
        {
            reservation = new
            {
                reference = "RES-NO-ACCOUNT",
                voucherCode = "VCH-NO-ACCOUNT",
                currency = "USD",
                totalAmount = 100m,
                status = "Pending",
                customerEmail = "customer@test.local"
            },
            invoice = new { totalAmount = 100m, currency = "USD", status = "Pending" },
            payment = new { method = "Manual", amount = 100m, currencyCode = "USD" },
            voucher = new { code = "VCH-NO-ACCOUNT", reference = "RES-NO-ACCOUNT", type = "Reservation" }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task BookingCheckout_CreatesCustomerAndPersistsServiceItems()
    {
        var client = CreateHttpsClient();
        var auth = await RegisterAndLoginAsync(client, $"booking.customer.{Guid.NewGuid():N}@test.local");
        SetBearer(client, auth.Token);
        var catalog = await SeedCheckoutCatalogAsync();

        var response = await client.PostAsJsonAsync("/api/bookings/checkout", new
        {
            customer = new
            {
                firstName = "Ada",
                surname = "Client",
                email = $"ada.{Guid.NewGuid():N}@test.local",
                phoneNumber = "+263 77 123 4567",
                durationOfStayDays = 2
            },
            reservation = new
            {
                accountId = auth.UserId,
                currency = "USD",
                status = "Pending"
            },
            invoice = new { currency = "USD", status = "Pending" },
            payment = new { method = "Manual", currencyCode = "USD" },
            serviceItems = new[]
            {
                new { serviceType = "Product", serviceId = catalog.ProductId, serviceName = "Guided Tour", quantity = 2, unitPrice = 30m, currency = "USD" },
                new { serviceType = "Activity", serviceId = catalog.ActivityId, serviceName = "Sunset Cruise", quantity = 1, unitPrice = 40m, currency = "USD" }
            }
        });

        await AssertStatusCodeAsync(response, HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = UnwrapData(json.RootElement);
        Assert.Equal(100m, root.GetProperty("invoice").GetProperty("totalAmount").GetDecimal());
        Assert.Equal("Pending", root.GetProperty("payment").GetProperty("status").GetString());
        Assert.Equal(2, root.GetProperty("serviceItems").GetArrayLength());
        Assert.True(root.GetProperty("customer").GetProperty("id").GetInt32() > 0);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppItDbContext>();
        var reservationId = root.GetProperty("reservation").GetProperty("reservationId").GetInt32();
        Assert.Equal(2, db.ReservationServiceItems.Count(item => item.ReservationId == reservationId));
        Assert.Contains(db.ReservationServiceItems, item => item.ReservationId == reservationId && item.ServiceName == "Guided Tour" && item.TotalPrice == 60m);
    }

    [Fact]
    public async Task BookingCheckout_LinksExistingCustomer()
    {
        var client = CreateHttpsClient();
        var auth = await RegisterAndLoginAsync(client, $"booking.existing.{Guid.NewGuid():N}@test.local");
        SetBearer(client, auth.Token);
        var catalog = await SeedCheckoutCatalogAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppItDbContext>();
        var customer = new Customer
        {
            FirstName = "Existing",
            Surname = "Guest",
            Email = $"existing.{Guid.NewGuid():N}@test.local",
            PhoneNumber = "+263 77 765 4321",
            LastSavedBy = auth.UserId,
            DateUpdated = DateTime.UtcNow
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var response = await client.PostAsJsonAsync("/api/bookings/checkout", new
        {
            customerId = customer.Id,
            reservation = new
            {
                accountId = auth.UserId,
                currency = "USD",
                status = "Pending"
            },
            invoice = new { currency = "USD", status = "Pending" },
            payment = new { method = "Manual", currencyCode = "USD" },
            serviceItems = new[]
            {
                new { serviceType = "Accommodation", serviceId = catalog.AccommodationId, serviceName = "Standard Room", quantity = 1, unitPrice = 80m, currency = "USD" }
            }
        });

        await AssertStatusCodeAsync(response, HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var reservation = UnwrapData(json.RootElement).GetProperty("reservation");
        Assert.Equal(customer.Id, reservation.GetProperty("customerId").GetInt32());
        Assert.Equal(80m, reservation.GetProperty("totalAmount").GetDecimal());
    }

    [Fact]
    public async Task ManualPayment_RemainsPendingAndUnprocessed()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppItDbContext>();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        var invoice = await CreateInvoiceAsync(db);

        var result = await paymentService.ProcessAsync(new ProcessPaymentDto
        {
            InvoiceId = invoice.Id,
            Method = "Manual",
            Amount = invoice.TotalAmount,
            CurrencyCode = invoice.CurrencyCode
        });

        Assert.True(result.Success);
        Assert.Equal("Pending", result.Status);

        var payment = await db.Payments.FindAsync(result.PaymentId!.Value);
        Assert.NotNull(payment);
        Assert.Equal("Pending", payment!.Status);
        Assert.Null(payment.ProcessedAt);
    }

    [Fact]
    public async Task UnconfiguredStripe_ReturnsFailedResultWithoutManualFallback()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppItDbContext>();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        var invoice = await CreateInvoiceAsync(db);

        var result = await paymentService.ProcessAsync(new ProcessPaymentDto
        {
            InvoiceId = invoice.Id,
            Method = "Stripe",
            Amount = invoice.TotalAmount,
            CurrencyCode = invoice.CurrencyCode
        });

        Assert.False(result.Success);
        Assert.Equal("Failed", result.Status);
        Assert.Equal("Stripe", result.Provider);
        Assert.Empty(db.Payments.Where(p => p.InvoiceId == invoice.Id));
    }

    [Fact]
    public async Task PaymentProcessing_IsIdempotentForSameKey()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppItDbContext>();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        var invoice = await CreateInvoiceAsync(db);

        var request = new ProcessPaymentDto
        {
            InvoiceId = invoice.Id,
            Method = "Manual",
            Amount = invoice.TotalAmount,
            CurrencyCode = invoice.CurrencyCode,
            IdempotencyKey = $"idem-{Guid.NewGuid():N}"
        };

        var first = await paymentService.ProcessAsync(request);
        var second = await paymentService.ProcessAsync(request);

        Assert.True(first.Success);
        Assert.Equal(first.PaymentId, second.PaymentId);
        Assert.Single(db.Payments.Where(p => p.InvoiceId == invoice.Id));
    }

    [Fact]
    public async Task Cleanup_DeletesOnlyExpiredPendingPayments()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppItDbContext>();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        var oldInvoice = await CreateInvoiceAsync(db, DateTime.UtcNow.AddDays(-2));
        var freshInvoice = await CreateInvoiceAsync(db);
        var paidInvoice = await CreateInvoiceAsync(db, DateTime.UtcNow.AddDays(-2));

        db.Payments.AddRange(
            new Payment { InvoiceId = oldInvoice.Id, Method = "Manual", Status = "Pending", Amount = 10m, CurrencyCode = "USD" },
            new Payment { InvoiceId = freshInvoice.Id, Method = "Manual", Status = "Pending", Amount = 10m, CurrencyCode = "USD" },
            new Payment { InvoiceId = paidInvoice.Id, Method = "Manual", Status = "Paid", Amount = 10m, CurrencyCode = "USD", ProcessedAt = DateTime.UtcNow.AddDays(-2) });
        await db.SaveChangesAsync();

        var deleted = await paymentService.DeleteExpiredPendingPaymentsAsync(TimeSpan.FromHours(24));

        Assert.Equal(1, deleted);
        Assert.DoesNotContain(db.Payments, p => p.InvoiceId == oldInvoice.Id);
        Assert.Contains(db.Payments, p => p.InvoiceId == freshInvoice.Id);
        Assert.Contains(db.Payments, p => p.InvoiceId == paidInvoice.Id);
    }

    [Fact]
    public async Task FailedLoginAttempts_LockAccountTemporarily()
    {
        var client = CreateHttpsClient();
        var email = $"lockout.{Guid.NewGuid():N}@test.local";
        await RegisterAndLoginAsync(client, email);

        for (var i = 0; i < 5; i++)
        {
            var badResponse = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Wrong!123" });
            Assert.Equal(HttpStatusCode.Unauthorized, badResponse.StatusCode);
        }

        var lockedResponse = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Password!123" });
        Assert.Equal(HttpStatusCode.Unauthorized, lockedResponse.StatusCode);
        var body = await lockedResponse.Content.ReadAsStringAsync();
        Assert.Contains("locked", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeneratedReservationReferencesAndVouchers_AreUnique()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IReservationService>();
        var references = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var vouchers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < 100; i++)
        {
            var row = await service.CreateAsync(new CreateReservationDto
            {
                AccountId = null,
                Currency = "USD",
                TotalAmount = 25m,
                Status = "Pending",
                CustomerEmail = $"unique.{i}@test.local"
            });

            Assert.True(references.Add(row.Reference));
            Assert.True(vouchers.Add(row.VoucherCode));
        }
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
        var user = data.GetProperty("user");
        return new AuthResult(user.GetProperty("id").GetInt32(), data.GetProperty("token").GetString() ?? string.Empty);
    }

    private static async Task CreateReservationAsync(HttpClient client, int accountId, string reference)
    {
        var response = await client.PostAsJsonAsync("/api/reservations", new
        {
            reference,
            voucherCode = $"VCH-{reference}",
            accountId,
            currency = "USD",
            totalAmount = 120.50m,
            status = "Pending",
            customerEmail = "customer@test.local"
        });

        await AssertStatusCodeAsync(response, HttpStatusCode.Created);
    }

    private static async Task<Invoice> CreateInvoiceAsync(AppItDbContext db, DateTime? issuedDate = null)
    {
        var reservation = new Reservation
        {
            Reference = $"RES-{Guid.NewGuid():N}",
            VoucherCode = $"VCH-{Guid.NewGuid():N}",
            CurrencyCode = "USD",
            TotalAmount = 100m,
            Status = "Pending",
            CreatedDate = issuedDate ?? DateTime.UtcNow
        };
        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();

        var invoice = new Invoice
        {
            ReservationId = reservation.ReservationId,
            TotalAmount = 100m,
            CurrencyCode = "USD",
            Status = "Pending",
            IsPaid = false,
            IssuedDate = issuedDate ?? DateTime.UtcNow
        };
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();
        return invoice;
    }

    private async Task<(int ProductId, int ActivityId, int AccommodationId)> SeedCheckoutCatalogAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppItDbContext>();

        var product = new Product
        {
            Name = "Guided Tour",
            Category = "Tour",
            BasePriceUsd = 30m,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        var activity = new Activity
        {
            Name = "Sunset Cruise",
            BasePriceUsd = 40m,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        var accommodation = new Accommodation
        {
            Type = "Standard Room",
            Capacity = 12,
            GuestCapacity = 2,
            BasePriceUsd = 80m,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        db.Products.Add(product);
        db.Activities.Add(activity);
        db.Accommodations.Add(accommodation);
        await db.SaveChangesAsync();

        return (product.ProductId, activity.Id, accommodation.Id);
    }

    private static async Task<AdminStatsDto> GetAdminStatsAsync(AppItDbContext db, string range)
    {
        var result = await new AdminStatsController(db).Get(range);
        var ok = Assert.IsType<OkObjectResult>(result);
        return Assert.IsType<AdminStatsDto>(ok.Value);
    }

    [Fact]
    public async Task MineEndpoint_RejectsSpoofedAccountId_ForRegularUser()
    {
        var client = CreateHttpsClient();
        var victim = await RegisterAndLoginAsync(client, $"victim.{Guid.NewGuid():N}@test.local");
        var attacker = await RegisterAndLoginAsync(client, $"attacker.{Guid.NewGuid():N}@test.local");

        SetBearer(client, attacker.Token);
        var response = await client.GetAsync($"/api/reservations/mine?accountId={victim.UserId}");

        await AssertStatusCodeAsync(response, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllReservations_RejectsRegularUser()
    {
        var client = CreateHttpsClient();
        var auth = await RegisterAndLoginAsync(client, $"regular.list.{Guid.NewGuid():N}@test.local");
        SetBearer(client, auth.Token);

        var response = await client.GetAsync("/api/reservations");

        await AssertStatusCodeAsync(response, HttpStatusCode.Forbidden);
    }

    private readonly record struct AuthResult(int UserId, string Token);

    private HttpClient CreateHttpsClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }

    private static void SetBearer(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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

    private static JsonElement UnwrapData(JsonElement root)
    {
        return root.TryGetProperty("data", out var data) ? data : root;
    }
}
