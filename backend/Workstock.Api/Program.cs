using Microsoft.EntityFrameworkCore;
using Workstock.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddDbContext<WorkstockDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("WorkstockDb")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        status = "healthy",
        service = "Workstock API"
    });
});

app.Run();