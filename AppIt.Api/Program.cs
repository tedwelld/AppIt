using System.Globalization;
using AppIt.Core.AppServices;
using AppIt.Core.Interfaces;
using AppIt.Data;
using Microsoft.EntityFrameworkCore;

// MUST be before CreateSlimBuilder
AppContext.SetSwitch("System.Globalization.Invariant", false);
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
// Ensure enhanced model metadata is enabled so MVC ApiExplorer initializes metadata correctly
AppContext.SetSwitch("Microsoft.AspNetCore.Mvc.ApiExplorer.IsEnhancedModelMetadataSupported", true);

var builder = WebApplication.CreateSlimBuilder(args);

// JSON options
builder.Services.ConfigureHttpJsonOptions(options => { });

// DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("DefaultConnection is not configured.");
}

builder.Services.AddDbContext<AppItDbContext>(
    options => options.UseSqlServer(connectionString));

// Core services
builder.Services.AddScoped<IFeatureService, FeatureService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRoleFeatureService, RoleFeatureService>();
builder.Services.AddScoped<IAccountCategoryService, AccountCategoryService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.Run();
