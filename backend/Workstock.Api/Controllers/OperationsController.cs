using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workstock.Api.Data;
using Workstock.Api.Infrastructure;
using Workstock.Api.Models;

namespace Workstock.Api.Controllers;

[ApiController, Route("api/dashboard"), RequireWorkstockAuth]
public sealed class DashboardController(WorkstockDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] DateOnly? date)
    {
        var current = this.CurrentUser(); var day = date ?? DateOnly.FromDateTime(DateTime.UtcNow); var start = day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc); var end = start.AddDays(1);
        IQueryable<Job> jobs = db.Jobs.AsNoTracking().Where(x => x.OrganisationId == current.OrganisationId).Include(x => x.Customer).Include(x => x.Site).Include(x => x.Status).Include(x => x.Assignments).ThenInclude(x => x.User);
        if (!current.IsManagerOrOwner()) jobs = jobs.Where(x => x.Assignments.Any(a => a.UserId == current.Id));
        var today = await jobs.Where(x => x.ScheduledStart >= start && x.ScheduledStart < end).OrderBy(x => x.ScheduledStart)
            .Select(x => new { x.Id, x.JobNumber, x.Title, x.ScheduledStart, x.ScheduledEnd, status = x.Status.Name, x.Priority, customer = x.Customer.Name, site = x.Site == null ? null : new { x.Site.Name, x.Site.AddressLine1, x.Site.City, x.Site.PostCode }, assignedUsers = x.Assignments.Select(a => a.User.DisplayName) }).ToListAsync();
        var overdue = await jobs.Where(x => !x.Status.IsTerminal && x.DueDate < DateTime.UtcNow).OrderBy(x => x.DueDate).Take(10)
            .Select(x => new { x.Id, x.JobNumber, x.Title, x.DueDate, status = x.Status.Name, customer = x.Customer.Name }).ToListAsync();
        var active = await jobs.CountAsync(x => !x.Status.IsTerminal); var awaiting = await jobs.CountAsync(x => x.Status.Name == "Awaiting Approval" || x.Status.Name == "Awaiting Parts");
        return Ok(new { date = day, summary = new { active, awaiting, overdue = overdue.Count }, today, overdue });
    }
}

[ApiController, Route("api/search"), RequireWorkstockAuth]
public sealed class SearchController(WorkstockDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Search([FromQuery] string q)
    {
        var current = this.CurrentUser(); var term = q.Trim().ToLower(); if (term.Length < 2) return Ok(new { customers = Array.Empty<object>(), sites = Array.Empty<object>(), jobs = Array.Empty<object>() });
        var customers = current.IsManagerOrOwner() ? await db.Customers.AsNoTracking().Where(x => x.OrganisationId == current.OrganisationId && (x.Name.ToLower().Contains(term) || (x.CompanyName ?? "").ToLower().Contains(term))).OrderBy(x => x.Name).Take(8).Select(x => new { x.Id, x.Name, x.CompanyName, x.City }).ToListAsync() : [];
        var sites = current.IsManagerOrOwner() ? await db.Sites.AsNoTracking().Include(x => x.Customer).Where(x => x.OrganisationId == current.OrganisationId && (x.Name.ToLower().Contains(term) || (x.AddressLine1 ?? "").ToLower().Contains(term))).Take(8).Select(x => new { x.Id, x.Name, customer = x.Customer.Name, x.AddressLine1, x.City }).ToListAsync() : [];
        var jobs = db.Jobs.AsNoTracking().Include(x => x.Customer).Include(x => x.Status).Where(x => x.OrganisationId == current.OrganisationId && (x.JobNumber.ToLower().Contains(term) || x.Title.ToLower().Contains(term) || x.Customer.Name.ToLower().Contains(term)));
        if (!current.IsManagerOrOwner()) jobs = jobs.Where(x => x.Assignments.Any(a => a.UserId == current.Id));
        return Ok(new { customers, sites, jobs = await jobs.OrderByDescending(x => x.CreatedAt).Take(10).Select(x => new { x.Id, x.JobNumber, x.Title, customer = x.Customer.Name, status = x.Status.Name }).ToListAsync() });
    }
}

[ApiController, Route("api/activity"), RequireWorkstockAuth]
public sealed class ActivityController(WorkstockDbContext db) : ControllerBase
{
    [HttpGet]
    [RequireWorkstockAuth(OrganisationRole.Owner, OrganisationRole.Manager)]
    public async Task<ActionResult> List([FromQuery] string? entityType, [FromQuery] Guid? entityId, [FromQuery] int take = 50)
    {
        var current = this.CurrentUser(); var query = db.Activities.AsNoTracking().Where(x => x.OrganisationId == current.OrganisationId);
        if (!string.IsNullOrWhiteSpace(entityType)) query = query.Where(x => x.EntityType == entityType); if (entityId is not null) query = query.Where(x => x.EntityId == entityId);
        return Ok(await query.OrderByDescending(x => x.CreatedAt).Take(Math.Clamp(take, 1, 200)).ToListAsync());
    }
}

[ApiController, Route("api/jobs/{jobId:guid}/documents"), RequireWorkstockAuth]
public sealed class JobDocumentsController(WorkstockDbContext db, IWebHostEnvironment environment, IConfiguration config) : ControllerBase
{
    private const long MaxSize = 10 * 1024 * 1024;
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp", "application/pdf"];

    [HttpPost]
    [RequestSizeLimit(MaxSize)]
    public async Task<ActionResult> Upload(Guid jobId, IFormFile file)
    {
        var current = this.CurrentUser(); if (!await CanAccess(jobId, current)) return NotFound();
        if (file.Length is <= 0 or > MaxSize || !AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant())) return BadRequest(new { error = "Upload a JPEG, PNG, WebP, or PDF no larger than 10 MB." });
        var subscription = await db.Subscriptions.SingleAsync(x => x.OrganisationId == current.OrganisationId);
        var used = await db.Documents.Where(x => x.OrganisationId == current.OrganisationId).SumAsync(x => (long?)x.SizeBytes) ?? 0;
        if (used + file.Length > subscription.StorageLimitBytes) return StatusCode(402, new { error = "Your plan's storage limit has been reached." });
        var extension = file.ContentType switch { "image/jpeg" => ".jpg", "image/png" => ".png", "image/webp" => ".webp", "application/pdf" => ".pdf", _ => "" };
        var storageKey = $"{current.OrganisationId:N}/{Guid.NewGuid():N}{extension}"; var root = GetStorageRoot(); var path = Path.Combine(root, storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!); await using (var stream = System.IO.File.Create(path)) await file.CopyToAsync(stream);
        var document = new Document { OrganisationId = current.OrganisationId, OwnerType = DocumentOwnerType.Job, OwnerId = jobId, FileName = Path.GetFileName(file.FileName), StorageKey = storageKey, ContentType = file.ContentType, SizeBytes = file.Length, UploadedByUserId = current.Id };
        db.Documents.Add(document); ActivityWriter.Add(db, current, "job", jobId, "document_uploaded", $"Uploaded {document.FileName}"); await db.SaveChangesAsync(); return Created($"/api/documents/{document.Id}", new { document.Id, document.FileName, document.ContentType, document.SizeBytes, document.CreatedAt });
    }

    [HttpGet("/api/documents/{id:guid}")]
    public async Task<IActionResult> Download(Guid id)
    {
        var current = this.CurrentUser(); var document = await db.Documents.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.OrganisationId == current.OrganisationId && x.OwnerType == DocumentOwnerType.Job);
        if (document is null || !await CanAccess(document.OwnerId, current)) return NotFound(); var path = Path.Combine(GetStorageRoot(), document.StorageKey);
        if (!System.IO.File.Exists(path)) return NotFound(); return File(System.IO.File.OpenRead(path), document.ContentType, document.FileName, enableRangeProcessing: true);
    }

    [HttpDelete("/api/documents/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var current = this.CurrentUser(); var document = await db.Documents.SingleOrDefaultAsync(x => x.Id == id && x.OrganisationId == current.OrganisationId && x.OwnerType == DocumentOwnerType.Job);
        if (document is null || !await CanAccess(document.OwnerId, current)) return NotFound(); var path = Path.Combine(GetStorageRoot(), document.StorageKey); if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        db.Remove(document); ActivityWriter.Add(db, current, "job", document.OwnerId, "document_deleted", $"Deleted {document.FileName}"); await db.SaveChangesAsync(); return NoContent();
    }

    private async Task<bool> CanAccess(Guid jobId, CurrentUser user) => user.IsManagerOrOwner() || await db.JobAssignments.AnyAsync(x => x.JobId == jobId && x.OrganisationId == user.OrganisationId && x.UserId == user.Id);
    private string GetStorageRoot() => Path.GetFullPath(config["Storage:LocalPath"] ?? Path.Combine(environment.ContentRootPath, "App_Data", "uploads"));
}
