using AppIt.Api.Services;
using AppIt.Core.AppServices;
using AppIt.Core.Configuration;
using AppIt.Core.DTOs;
using AppIt.Core.Interfaces;
using AppIt.Core.Interfaces.Services;
using AppIt.Core.Services;
using AppIt.Api.SeedData;
using AppIt.Api.Infrastructure;
using AppIt.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Net;
using System.Text;
using System.Text.Json.Serialization.Metadata;
namespace AppIt.Api
{
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void Main(string[] args)
        {
            try
            {
                // ?? REQUIRED for MVC ApiExplorer (fixes your exception)
                AppContext.SetSwitch(
                    "Microsoft.AspNetCore.Mvc.ApiExplorer.IsEnhancedModelMetadataSupported",
                    true
                );

                // If a cert path is configured via environment (launchSettings), but the file doesn't exist,
                // unset the env vars so Kestrel won't attempt to load a missing certificate and crash at startup.
                try
                {
                    var certPathEnv = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path");
                    if (!string.IsNullOrEmpty(certPathEnv))
                    {
                        var candidate = Path.IsPathRooted(certPathEnv)
                            ? certPathEnv
                            : Path.Combine(Directory.GetCurrentDirectory(), certPathEnv);

                        if (!File.Exists(candidate))
                        {
                            Console.WriteLine($"[Warning] Kestrel certificate not found at '{candidate}'. Unsetting certificate environment variables to avoid startup failure.");
                            Environment.SetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path", null);
                            Environment.SetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password", null);
                        }
                    }
                }
                catch
                {
                    // swallow any IO errors here to avoid masking startup failures
                }

                var builder = WebApplication.CreateBuilder(args);

                // Kestrel will use certificates configured via environment or launchSettings.
                // Removed the no-op certificate selector so Kestrel can load an installed PFX.

                #region Configuration & Infrastructure

                var useInMemoryDatabase = builder.Configuration.GetValue<bool>("Database:UseInMemory");
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                if (!useInMemoryDatabase && string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("DefaultConnection is not configured.");
                }

                #endregion

                #region Database

                builder.Services.AddDbContext<AppItDbContext>(options =>
                {
                    if (useInMemoryDatabase)
                    {
                        var dbName = builder.Configuration["Database:InMemoryName"] ?? "AppIt.InMemory";
                        options.UseInMemoryDatabase(dbName);
                    }
                    else
                    {
                        options.UseSqlServer(connectionString);
                    }
                });

                #endregion
                //Account Services

                #region Application Services

                builder.Services.AddScoped<IFeatureService, FeatureService>();
                builder.Services.AddScoped<IFeaturePermissionService, FeaturePermissionService>();
                builder.Services.AddScoped<IPermissionService, PermissionService>();
                builder.Services.AddScoped<IProductService, ProductService>();
                builder.Services.AddScoped<IAccommodationService, AccommodationService>();
                builder.Services.AddScoped<IActivityService, ActivityService>();
                builder.Services.AddScoped<ITransferService, TransferService>();
                builder.Services.AddScoped<ITourService, TourService>();
                builder.Services.AddScoped<IServicePriceService, ServicePriceService>();
                builder.Services.AddScoped<IPricingService, PricingService>();
                builder.Services.AddScoped<IRoleService, RoleService>();
                builder.Services.AddScoped<IRoleFeatureService, RoleFeatureService>();
                builder.Services.AddScoped<ICompanyService, CompanyService>();
                builder.Services.AddScoped<IDepartmentService, DepartmentService>();
                builder.Services.AddScoped<IAccountService, AccountService>();
                builder.Services.AddScoped<ISupplierService, SupplierService>();
                builder.Services.AddScoped<ICustomerTypeService, CustomerTypeService>();
                builder.Services.AddScoped<IReservationService, ReservationService>();
                builder.Services.AddScoped<ICustomerService, CustomerService>();
                builder.Services.AddScoped<IInvoiceService, InvoiceService>();
                builder.Services.AddScoped<IPaymentService, PaymentService>();
                builder.Services.AddScoped<IPaymentProvider, StripePaymentProvider>();
                builder.Services.AddHttpClient<PayPalPaymentProvider>();
                builder.Services.AddScoped<IPaymentProvider>(sp => sp.GetRequiredService<PayPalPaymentProvider>());
                builder.Services.AddScoped<IPaymentProvider, ManualPaymentProvider>();
                builder.Services.AddScoped<IBookingService, BookingService>();
                builder.Services.AddScoped<IVoucherService, VoucherService>();
                builder.Services.AddScoped<ISupportMessageService, SupportMessageService>();
                builder.Services.AddScoped<INotificationService, NotificationService>();
                builder.Services.AddScoped<IReportSnapshotService, ReportSnapshotService>();
                builder.Services.AddScoped<IUserProfileService, UserProfileService>();
                builder.Services.AddScoped<IAuthService, AuthService>();
                builder.Services.AddScoped<IAuditLogService, AuditLogService>();
                builder.Services.AddScoped<ICurrencyService, CurrencyService>();
                builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();
                builder.Services.AddScoped<ICreditNoteService, CreditNoteService>();
                builder.Services.AddScoped<IRefundService, RefundService>();
                builder.Services.AddScoped<IProofOfPaymentService, ProofOfPaymentService>();
                builder.Services.AddScoped<ICommissionService, CommissionService>();
                builder.Services.AddScoped<IDayEndService, DayEndService>();
                builder.Services.AddScoped<IConsultantService, ConsultantService>();
                builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
                builder.Services.AddScoped<IProductSubCategoryService, ProductSubCategoryService>();
                builder.Services.AddScoped<ISpecialProductPriceService, SpecialProductPriceService>();
                builder.Services.AddScoped<IComboService, ComboService>();
                builder.Services.AddScoped<ICreditAgentService, CreditAgentService>();
                builder.Services.AddScoped<IProductPriceAgentService, ProductPriceAgentService>();
                builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();
                builder.Services.AddScoped<INightAuditService, NightAuditService>();
                builder.Services.AddScoped<ITrialBalanceService, TrialBalanceService>();
                builder.Services.AddScoped<IDebtorReportService, DebtorReportService>();
                builder.Services.AddScoped<ICreditMemoService, CreditMemoService>();
                builder.Services.AddScoped<IFiscalService, AppIt.Core.AppServices.Fiscalization.FiscalService>();
                builder.Services.AddScoped<IHConnectService, AppIt.Core.AppServices.Integrations.HConnectService>();
                builder.Services.AddScoped<IBeds24Service, AppIt.Core.AppServices.Integrations.Beds24Service>();
                builder.Services.AddScoped<ISimunyeService, AppIt.Core.AppServices.Integrations.SimunyeService>();
                builder.Services.AddHostedService<PendingPaymentCleanupHostedService>();
                builder.Services.AddAppItQuartz(builder.Configuration);
                builder.Services.ConfigureHttpJsonOptions(options =>
                {
                    // Ensure a runtime TypeInfoResolver is available so source-generation is not required
                    // This prevents JsonTypeInfo metadata errors when serializing DTOs at runtime.
                    options.SerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
                });
                builder.Services.Configure<PaymentProviderOptions>(builder.Configuration.GetSection("Payments"));
                builder.Services.AddScoped<JwtTokenService>();
                builder.Services.AddHttpContextAccessor();
                builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
                builder.Services.AddScoped<IResourceAuthorizationService, ResourceAuthorizationService>();

                #endregion

                #region Authentication & Authorization

                var jwtKey = builder.Configuration["Auth:JwtKey"]
                    ?? throw new InvalidOperationException("Auth:JwtKey is not configured.");
                if (!builder.Environment.IsDevelopment()
                    && (jwtKey.Contains("REPLACE_WITH", StringComparison.OrdinalIgnoreCase)
                        || jwtKey.Length < 32))
                {
                    throw new InvalidOperationException("Auth:JwtKey must be set to a secure value (32+ chars) outside Development.");
                }
                var jwtIssuer = builder.Configuration["Auth:JwtIssuer"] ?? "AppIt";
                var jwtAudience = builder.Configuration["Auth:JwtAudience"] ?? "AppItClient";

                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = jwtIssuer,
                            ValidAudience = jwtAudience,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                            ClockSkew = TimeSpan.Zero
                        };
                    });

                builder.Services.AddAuthorization();

                builder.Services.AddHealthChecks()
                    .AddDbContextCheck<AppItDbContext>("database");

                #endregion

                #region API

                builder.Services.AddControllers(options =>
                    {
                        options.Filters.Add<ApiEnvelopeFilter>();
                    })
                    .AddJsonOptions(opts =>
                    {
                        // Ensure runtime TypeInfoResolver for controller JSON serialization
                        opts.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
                    });
                builder.Services.Configure<ApiBehaviorOptions>(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var problem = new ValidationProblemDetails(context.ModelState)
                        {
                            Status = StatusCodes.Status400BadRequest,
                            Title = "Validation failed.",
                            Detail = "One or more validation errors occurred.",
                            Instance = context.HttpContext.Request.Path
                        };

                        return new BadRequestObjectResult(ApiEnvelope.Fail(problem));
                    };
                });
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v2", new OpenApiInfo
                    {
                        Title = "AppIt API",
                        Version = "v2",
                        Description = "AppIt service endpoints."
                    });
                    options.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter your JWT token."
                    });
                    options.OperationFilter<BearerSecurityRequirementOperationFilter>();
                });

                #endregion

                var app = builder.Build();
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppItDbContext>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    InitialDataSeeder.SeedAsync(dbContext, builder.Configuration, logger).GetAwaiter().GetResult();
                }

                #region Global Exception Handling (HTTP Pipeline)

                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        var exceptionHandler =
                            context.Features.Get<IExceptionHandlerFeature>();

                        var exception = exceptionHandler?.Error;

                        var statusCode = exception switch
                        {
                            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                            InvalidOperationException invalidOp when invalidOp.Message.Contains("already", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status409Conflict,
                            InvalidOperationException => StatusCodes.Status400BadRequest,
                            KeyNotFoundException => StatusCodes.Status404NotFound,
                            _ => StatusCodes.Status500InternalServerError
                        };

                        context.Response.StatusCode = statusCode;
                        context.Response.ContentType = "application/json";
                        var detail = app.Environment.IsDevelopment() ? exception?.Message : null;
                        var problem = new ProblemDetails
                        {
                            Status = statusCode,
                            Title = statusCode == StatusCodes.Status500InternalServerError
                                ? "An unexpected error occurred."
                                : ReasonPhrases.GetReasonPhrase(statusCode),
                            Detail = detail,
                            Instance = context.Request.Path
                        };
                        problem.Extensions["exceptionType"] = exception?.GetType().Name;
                        problem.Extensions["traceId"] = context.TraceIdentifier;

                        await context.Response.WriteAsJsonAsync(ApiEnvelope.Fail(problem));
                    });
                });

                #endregion

                #region Middleware Pipeline

                app.MapGet("/", () => Results.Ok(new
                {
                    name = "AppIt API",
                    status = "ok"
                })).ExcludeFromDescription();
                app.MapHealthChecks("/health");
                app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("ready") || check.Name == "database"
                });

                if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Swagger:Enabled"))
                {
                    app.UseSwagger(options =>
                    {
                        options.RouteTemplate = "swagger/{documentName}/swagger.json";
                        options.OpenApiVersion = OpenApiSpecVersion.OpenApi2_0;
                    });
                    app.UseSwaggerUI(options =>
                    {
                        options.SwaggerEndpoint("/swagger/v2/swagger.json", "AppIt API v2");
                        options.RoutePrefix = "swagger";
                        options.DocumentTitle = "AppIt API Swagger";
                        options.DisplayRequestDuration();
                    });
                }

                app.UseStatusCodePages(async statusContext =>
                {
                    var response = statusContext.HttpContext.Response;
                    if (response.StatusCode < 400 || response.HasStarted)
                    {
                        return;
                    }

                    if (response.ContentLength.HasValue && response.ContentLength.Value > 0)
                    {
                        return;
                    }

                    response.ContentType = "application/json";
                    var problem = new ProblemDetails
                    {
                        Status = response.StatusCode,
                        Title = ReasonPhrases.GetReasonPhrase(response.StatusCode),
                        Instance = statusContext.HttpContext.Request.Path
                    };

                    await response.WriteAsJsonAsync(ApiEnvelope.Fail(problem));
                });

                app.UseAuthentication();
                app.UseAuthorization();
                app.MapControllers();

                #endregion

                app.Run();
            }
            catch (NotSupportedException ex) when (
                ex.Message.Contains("IsEnhancedModelMetadataSupported"))
            {
                // ?? CLEAR diagnostic for the exact issue you hit
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("? MVC Startup Failure:");
                Console.WriteLine("Enhanced Model Metadata is REQUIRED but not enabled.");
                Console.WriteLine("Fix: AppContext.SetSwitch(\"Microsoft.AspNetCore.Mvc.ApiExplorer.IsEnhancedModelMetadataSupported\", true)");
                Console.WriteLine();
                Console.WriteLine(ex);
                Console.ResetColor();

                throw;
            }
            catch (Exception ex)
            {
                // ?? Final safety net (startup-level failure)
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("?? Application failed to start");
                Console.WriteLine(ex);
                Console.ResetColor();

                throw;
            }
        }

    }
}
