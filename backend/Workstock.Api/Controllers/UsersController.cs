using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workstock.Api.Data;
using Workstock.Api.DTOs;
using Workstock.Api.Infrastructure;
using Workstock.Api.Models;

namespace Workstock.Api.Controllers;

[ApiController, Route("api/users"), RequireWorkstockAuth]
public sealed class UsersController(WorkstockDbContext db, PasswordService passwords) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> List()
    {
        var current = this.CurrentUser();
        var users = await db.Users.AsNoTracking().Where(x => x.OrganisationId == current.OrganisationId)
            .OrderBy(x => x.DisplayName).Select(x => new { x.Id, x.DisplayName, x.Email, x.Role, x.IsActive, x.CreatedAt }).ToListAsync();
        var activeEmployeeCount = await db.Users.CountAsync(x => x.OrganisationId == current.OrganisationId && x.IsActive);
        return Ok(new { activeEmployeeCount, users });
    }

    [HttpPost, RequireWorkstockAuth(OrganisationRole.Owner, OrganisationRole.Manager)]
    public async Task<ActionResult> Create(CreateUserRequest request)
    {
        var current = this.CurrentUser();
        if (current.Role == OrganisationRole.Manager && request.Role != OrganisationRole.Employee) return Forbid();
        if (await db.Users.AnyAsync(x => x.Email == request.Email.Trim().ToLower())) return Conflict(new { error = "An account already exists for this email address." });
        var user = new User { OrganisationId = current.OrganisationId, DisplayName = request.DisplayName.Trim(), Email = request.Email.Trim().ToLowerInvariant(), PasswordHash = passwords.Hash(request.Password), Role = request.Role };
        db.Users.Add(user); ActivityWriter.Add(db, current, "user", user.Id, "created", $"Created user {user.DisplayName}");
        await db.SaveChangesAsync();
        await RefreshActiveEmployeeCount(current.OrganisationId);
        return Created($"/api/users/{user.Id}", new { user.Id, user.DisplayName, user.Email, user.Role, user.IsActive });
    }

    [HttpPatch("{id:guid}"), RequireWorkstockAuth(OrganisationRole.Owner, OrganisationRole.Manager)]
    public async Task<ActionResult> Update(Guid id, UpdateUserRequest request)
    {
        var current = this.CurrentUser(); var user = await db.Users.SingleOrDefaultAsync(x => x.Id == id && x.OrganisationId == current.OrganisationId);
        if (user is null) return NotFound(); if (current.Role == OrganisationRole.Manager && (user.Role != OrganisationRole.Employee || request.Role != OrganisationRole.Employee)) return Forbid();
        if (user.Id == current.Id && !request.IsActive) return BadRequest(new { error = "You cannot deactivate yourself." });
        if (user.Id == current.Id && request.Role != current.Role) return BadRequest(new { error = "You cannot change your own role." });
        user.DisplayName = request.DisplayName.Trim(); user.Role = request.Role; user.IsActive = request.IsActive; user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await RefreshActiveEmployeeCount(current.OrganisationId);
        return Ok(new { user.Id, user.DisplayName, user.Email, user.Role, user.IsActive });
    }

    private async Task RefreshActiveEmployeeCount(Guid organisationId)
    {
        var organisation = await db.Organisations.SingleAsync(x => x.Id == organisationId);
        organisation.ActiveEmployeeCount = await db.Users.CountAsync(x => x.OrganisationId == organisationId && x.IsActive);
        organisation.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
}
