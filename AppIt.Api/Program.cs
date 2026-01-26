using AppIt.Api.Controllers;    
using AppIt.Core.AppServices;
using AppIt.Core.Interfaces;
using AppIt.Core.Interfaces.Services;
using AppIt.Core.Services;
using AppIt.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using AppIt.Core.Interfaces.Repositories;
//using AppIt.Data.Repositories;
// Swagger/OpenAPI
// Note: OpenApi types are not required in this file; Swashbuckle will auto-generate documents
using System.Net;
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

                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("DefaultConnection is not configured.");
                }

                #endregion

                #region Database

                builder.Services.AddDbContext<AppItDbContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                });

                #endregion
                //Account Services

                #region Application Services

                builder.Services.AddScoped<IFeatureService, FeatureService>();
                builder.Services.AddScoped<IFeaturePermissionService, FeaturePermissionService>();
                builder.Services.AddScoped<IPermissionService, PermissionService>();
                builder.Services.AddScoped<IProductService, ProductService>();
                builder.Services.AddScoped<IRoleService, RoleService>();
                builder.Services.AddScoped<IRoleFeatureService, RoleFeatureService>();
                builder.Services.AddScoped<ICompanyService, CompanyService>();
                builder.Services.AddScoped<IDepartmentService, DepartmentService>();
                builder.Services.AddScoped<IAccountService, AccountService>();
                builder.Services.AddScoped<ISupplierService, SupplierService>();
                builder.Services.AddScoped<ISupplierService, SupplierService>();
                builder.Services.AddScoped<ICustomerTypeService, CustomerTypeService>();
                builder.Services.AddScoped<IReservationService, ReservationService>();
                builder.Services.AddScoped<ICustomerService, CustomerService>();
                builder.Services.ConfigureHttpJsonOptions(options =>
                {
                    // Ensure a runtime TypeInfoResolver is available so source-generation is not required
                    // This prevents JsonTypeInfo metadata errors when serializing DTOs at runtime.
                    options.SerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
                });

                #endregion

                #region API & Documentation

                builder.Services.AddControllers()
                    .AddJsonOptions(opts =>
                    {
                        // Ensure runtime TypeInfoResolver for controller JSON serialization
                        opts.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
                    });
                builder.Services.AddEndpointsApiExplorer();
                // Use Swashbuckle to generate OpenAPI/Swagger documents for controllers
                builder.Services.AddSwaggerGen(options =>
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "AppIt API",
                    Version = "v2"
                }));
            

            // Core services registration (ensure PermissionService is registered)
            builder.Services.AddScoped<IPermissionService, PermissionService>();

                #endregion

                var app = builder.Build();

                #region Global Exception Handling (HTTP Pipeline)

                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        var exceptionHandler =
                            context.Features.Get<IExceptionHandlerFeature>();

                        var exception = exceptionHandler?.Error;

                        context.Response.StatusCode =
                            (int)HttpStatusCode.InternalServerError;
                        context.Response.ContentType = "application/json";

                        // Build a minimal JSON string manually to avoid System.Text.Json source-generation issues
                        string Escape(string? s)
                        {
                            if (s == null) return string.Empty;
                            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
                        }

                        var detail = app.Environment.IsDevelopment() ? Escape(exception?.Message) : null;
                        var exceptionType = exception?.GetType().Name;

                        var json = $"{{\"error\":\"An unexpected error occurred.\",\"detail\":{(detail == null ? "null" : "\"" + detail + "\"")},\"exceptionType\":{(exceptionType == null ? "null" : "\"" + Escape(exceptionType) + "\"")}}}";

                        await context.Response.WriteAsync(json);
                    });
                });

                #endregion

                #region Middleware Pipeline

                if (app.Environment.IsDevelopment())
                {
                    // Enable middleware to serve generated Swagger as JSON endpoint.
                    app.UseSwagger();

                    // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
                    // specifying the Swagger JSON endpoint and route prefix.
                    app.UseSwaggerUI(options =>
                    {
                        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AppIt API v1");
                        options.RoutePrefix = "swagger";
                    });

                    // Provide a fallback minimal OpenAPI document at the expected Swashbuckle path
                    // so Swagger UI can successfully load even if the OpenAPI generator fails.
                   
                }

                app.UseHttpsRedirection();
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
