using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Workstock.Api.Data;
using Workstock.Api.DTOs;
using Workstock.Api.Infrastructure;
using Workstock.Api.Models;

namespace Workstock.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public sealed class AuthController(WorkstockDbContext db, PasswordService passwords, TokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (!HasStrongPassword(request.Password)) return BadRequest(new { error = "Use at least 12 characters including upper-case, lower-case, and a number." });
        if (await db.Users.AnyAsync(x => x.Email == email)) return Conflict(new { error = "An account already exists for this email address." });

        await using var transaction = await db.Database.BeginTransactionAsync();
        var organisation = new Organisation { Name = request.OrganisationName.Trim(), ActiveEmployeeCount = 1 };
        var owner = new User { Organisation = organisation, Email = email, DisplayName = request.DisplayName.Trim(), PasswordHash = passwords.Hash(request.Password), Role = OrganisationRole.Owner };
        db.AddRange(organisation, owner, new Subscription { OrganisationId = organisation.Id, Plan = "free" });
        for (var i = 0; i < WorkstockDefaults.Statuses.Length; i++)
        {
            var status = WorkstockDefaults.Statuses[i];
            db.JobStatuses.Add(new JobStatus { OrganisationId = organisation.Id, Name = status.Name, SortOrder = i, IsTerminal = status.Terminal, IsDefault = i == 0, Colour = status.Colour });
        }
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Ok(await CreateSession(owner));
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email && x.IsActive);
        if (user is null || !passwords.Verify(request.Password, user.PasswordHash)) return Unauthorized(new { error = "Invalid email or password." });
        return Ok(await CreateSession(user));
    }

    [HttpPost("logout"), RequireWorkstockAuth]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Headers.Authorization.ToString()[7..].Trim();
        var session = await db.UserSessions.FirstOrDefaultAsync(x => x.TokenHash == TokenService.Hash(token));
        if (session is not null) { session.RevokedAt = DateTime.UtcNow; await db.SaveChangesAsync(); }
        return NoContent();
    }

    [HttpGet("me"), RequireWorkstockAuth]
    public async Task<ActionResult> Me()
    {
        var current = this.CurrentUser();
        var organisation = await db.Organisations.AsNoTracking().SingleAsync(x => x.Id == current.OrganisationId);
        return Ok(new { user = current, organisation = new { organisation.Id, organisation.Name, organisation.Email, organisation.PhoneNumber, organisation.ActiveEmployeeCount } });
    }

    private async Task<object> CreateSession(User user)
    {
        var pair = tokens.NewToken();
        db.UserSessions.Add(new UserSession { UserId = user.Id, TokenHash = pair.Hash, ExpiresAt = DateTime.UtcNow.AddDays(7) });
        await db.SaveChangesAsync();
        return new { token = pair.Token, expiresAt = DateTime.UtcNow.AddDays(7), user = new { user.Id, user.OrganisationId, user.Email, user.DisplayName, user.Role } };
    }
    private static bool HasStrongPassword(string password) => password.Length >= 12 && password.Any(char.IsUpper) && password.Any(char.IsLower) && password.Any(char.IsDigit);
}

[ApiController]
[Route("api/organisation")]
[RequireWorkstockAuth]
public sealed class OrganisationController(WorkstockDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Get()
    {
        var orgId = this.CurrentUser().OrganisationId;
        return Ok(await db.Organisations.AsNoTracking().SingleAsync(x => x.Id == orgId));
    }

    [HttpPatch]
    [RequireWorkstockAuth(OrganisationRole.Owner)]
    public async Task<ActionResult> Update(OrganisationUpdateRequest request)
    {
        var org = await db.Organisations.SingleAsync(x => x.Id == this.CurrentUser().OrganisationId);
        if (!string.IsNullOrWhiteSpace(request.Name)) org.Name = request.Name.Trim();
        org.Description = request.Description; org.Email = request.Email; org.PhoneNumber = request.PhoneNumber; org.WebsiteUrl = request.WebsiteUrl;
        org.AddressLine1 = request.AddressLine1; org.AddressLine2 = request.AddressLine2; org.City = request.City; org.County = request.County; org.PostCode = request.PostCode; org.Country = request.Country; org.LogoUrl = request.LogoUrl; org.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(org);
    }
}
