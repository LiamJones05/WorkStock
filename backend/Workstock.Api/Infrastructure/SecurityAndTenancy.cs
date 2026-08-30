using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Workstock.Api.Data;
using Workstock.Api.Models;

namespace Workstock.Api.Infrastructure;

public sealed record CurrentUser(Guid Id, Guid OrganisationId, string Email, string DisplayName, OrganisationRole Role);

public sealed class TenantAuthenticationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, WorkstockDbContext db)
    {
        var header = context.Request.Headers.Authorization.ToString();
        if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = header[7..].Trim();
            if (token.Length is > 0 and <= 512)
            {
                var tokenHash = TokenService.Hash(token);
                var session = await db.UserSessions.AsNoTracking()
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.TokenHash == tokenHash && x.RevokedAt == null && x.ExpiresAt > DateTime.UtcNow);
                if (session?.User.IsActive == true)
                    context.Items["workstock.user"] = new CurrentUser(session.User.Id, session.User.OrganisationId,
                        session.User.Email, session.User.DisplayName, session.User.Role);
            }
        }
        await next(context);
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireWorkstockAuthAttribute(params OrganisationRole[] roles) : Attribute, IAsyncActionFilter
{
    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.GetCurrentUser();
        if (user is null)
            context.Result = new UnauthorizedObjectResult(new { error = "Authentication is required." });
        else if (roles.Length > 0 && !roles.Contains(user.Role))
            context.Result = new ObjectResult(new { error = "You do not have permission to perform this action." }) { StatusCode = StatusCodes.Status403Forbidden };
        return context.Result is null ? next() : Task.CompletedTask;
    }
}

public static class CurrentUserExtensions
{
    public static CurrentUser? GetCurrentUser(this HttpContext context) => context.Items["workstock.user"] as CurrentUser;
    public static CurrentUser CurrentUser(this ControllerBase controller) =>
        controller.HttpContext.GetCurrentUser() ?? throw new UnauthorizedAccessException();
    public static bool IsManagerOrOwner(this CurrentUser user) => user.Role is OrganisationRole.Owner or OrganisationRole.Manager;
}

public sealed class PasswordService
{
    private const int Iterations = 210_000;
    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA512, 32);
        return $"v1.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string stored)
    {
        var parts = stored.Split('.');
        if (parts.Length != 4 || parts[0] != "v1" || !int.TryParse(parts[1], out var iterations)) return false;
        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA512, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException) { return false; }
    }
}

public sealed class TokenService
{
    public (string Token, string Hash) NewToken()
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        return (token, Hash(token));
    }
    public static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}

public static class ActivityWriter
{
    public static void Add(WorkstockDbContext db, CurrentUser user, string entityType, Guid entityId, string action, string description, string? metadataJson = null) =>
        db.Activities.Add(new Activity { OrganisationId = user.OrganisationId, UserId = user.Id, UserDisplayName = user.DisplayName, EntityType = entityType, EntityId = entityId, Action = action, Description = description, MetadataJson = metadataJson });
}

public static class WorkstockDefaults
{
    public static readonly (string Name, bool Terminal, string Colour)[] Statuses =
    [
        ("New", false, "slate"), ("Received", false, "blue"), ("Diagnosing", false, "purple"),
        ("Awaiting Approval", false, "amber"), ("Awaiting Parts", false, "orange"), ("In Progress", false, "indigo"),
        ("Testing", false, "cyan"), ("Ready for Collection", false, "emerald"), ("Completed", true, "green"), ("Cancelled", true, "red")
    ];
}
