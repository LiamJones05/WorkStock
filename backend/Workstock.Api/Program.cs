using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Workstock.Api.Data;
using Workstock.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("web", policy =>
    policy.WithOrigins(allowedOrigins.Length > 0 ? allowedOrigins : ["http://localhost:5173"])
        .AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddDbContext<WorkstockDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("WorkstockDb")));

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("Database:AutoMigrate"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<WorkstockDbContext>().Database.MigrateAsync();
}

app.UseExceptionHandler(exceptionApp => exceptionApp.Run(async context =>
{
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Errors");
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    logger.LogError(exception, "Unhandled request error. TraceId: {TraceId}", context.TraceIdentifier);
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await Results.Problem(statusCode: 500, title: "An unexpected error occurred.", extensions: new Dictionary<string, object?> { ["traceId"] = context.TraceIdentifier }).ExecuteAsync(context);
}));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (builder.Configuration.GetValue<bool>("Security:ForceHttps")) app.UseHttpsRedirection();
app.UseCors("web");
app.UseRateLimiter();
app.UseMiddleware<TenantAuthenticationMiddleware>();

app.MapControllers();

app.MapGet("/api/health", async (WorkstockDbContext db) =>
{
    var databaseConnected = await db.Database.CanConnectAsync();
    return Results.Json(new
    {
        status = databaseConnected ? "healthy" : "unhealthy",
        service = "Workstock API",
        database = databaseConnected ? "connected" : "unavailable"
    }, statusCode: databaseConnected ? 200 : 503);
});

app.Run();
