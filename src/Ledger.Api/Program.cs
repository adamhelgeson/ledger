using FluentValidation;
using Ledger.Application.Behaviors;
using Ledger.Infrastructure;
using Ledger.Infrastructure.Data;
using Ledger.Infrastructure.Seeders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Ledger.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ──────────────────────────────────────────────────────────────
var dataDir = builder.Configuration["DataDir"] ?? "./data";
Directory.CreateDirectory(dataDir); // ensure data directory exists
var dbPath = Path.Combine(dataDir, "ledger.db");
var connectionString = $"Data Source={dbPath}";

// ── Services ───────────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Ledger API — The All-Father's Treasury", Version = "v1" });
});

// Infrastructure (EF Core, repositories, CSV parser, seeder)
builder.Services.AddInfrastructure(connectionString);

// Health checks
builder.Services.AddHealthChecks();

// MediatR — scan Application + Infrastructure assemblies for handlers
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Ledger.Application.Accounts.Queries.GetAllAccountsQuery).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Ledger.Infrastructure.Handlers.ParseCsvHandler).Assembly);
});

// FluentValidation pipeline behavior
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Register all validators from Application assembly
builder.Services.AddValidatorsFromAssembly(
    typeof(Ledger.Application.Accounts.Commands.CreateAccountValidator).Assembly);

// CORS — local dev only
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── App Pipeline ───────────────────────────────────────────────────────────────
var app = builder.Build();

// Auto-migrate and seed on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LedgerDbContext>();
    await db.Database.MigrateAsync();

    if (app.Environment.IsDevelopment())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ledger API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
