using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workstock.Api.Data;
using Workstock.Api.DTOs;
using Workstock.Api.Infrastructure;
using Workstock.Api.Models;

namespace Workstock.Api.Controllers;

[ApiController, Route("api/jobs"), RequireWorkstockAuth]
public sealed class JobsController(WorkstockDbContext db) : ControllerBase
{
    [HttpGet("statuses")]
    public async Task<ActionResult> Statuses() => Ok(await db.JobStatuses.AsNoTracking().Where(x => x.OrganisationId == this.CurrentUser().OrganisationId).OrderBy(x => x.SortOrder).ToListAsync());

    [HttpGet]
    public async Task<ActionResult> List([FromQuery] string? q, [FromQuery] Guid? statusId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? view, [FromQuery] int page = 1, [FromQuery] int pageSize = 30)
    {
        var current = this.CurrentUser(); page = Math.Max(page, 1); pageSize = Math.Clamp(pageSize, 1, 100);
        IQueryable<Job> query = db.Jobs.AsNoTracking().Where(x => x.OrganisationId == current.OrganisationId).Include(x => x.Customer).Include(x => x.Site).Include(x => x.Status).Include(x => x.Assignments).ThenInclude(x => x.User);
        if (!current.IsManagerOrOwner()) query = query.Where(x => x.Assignments.Any(a => a.UserId == current.Id));
        if (!string.IsNullOrWhiteSpace(q)) { var term = q.Trim().ToLower(); query = query.Where(x => x.JobNumber.ToLower().Contains(term) || x.Title.ToLower().Contains(term) || x.Customer.Name.ToLower().Contains(term)); }
        if (statusId is not null) query = query.Where(x => x.JobStatusId == statusId);
        if (from is not null) query = query.Where(x => x.ScheduledStart >= from);
        if (to is not null) query = query.Where(x => x.ScheduledStart <= to);
        if (view == "active") query = query.Where(x => !x.Status.IsTerminal);
        var total = await query.CountAsync();
        var jobs = await query.OrderBy(x => x.ScheduledStart == null).ThenBy(x => x.ScheduledStart).ThenByDescending(x => x.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new { x.Id, x.JobNumber, x.Title, x.Priority, x.ScheduledStart, x.ScheduledEnd, x.DueDate, x.CompletedAt, status = new { x.Status.Id, x.Status.Name, x.Status.Colour, x.Status.IsTerminal }, customer = new { x.Customer.Id, x.Customer.Name }, site = x.Site == null ? null : new { x.Site.Id, x.Site.Name, x.Site.AddressLine1, x.Site.City, x.Site.PostCode }, assignedUsers = x.Assignments.Select(a => new { a.UserId, a.User.DisplayName }) }).ToListAsync();
        return Ok(new { items = jobs, total, page, pageSize });
    }

    [HttpPost]
    [RequireWorkstockAuth(OrganisationRole.Owner, OrganisationRole.Manager)]
    public async Task<ActionResult> Create(JobRequest request)
    {
        var current = this.CurrentUser();
        var customer = await db.Customers.SingleOrDefaultAsync(x => x.Id == request.CustomerId && x.OrganisationId == current.OrganisationId);
        var status = await db.JobStatuses.SingleOrDefaultAsync(x => x.Id == request.JobStatusId && x.OrganisationId == current.OrganisationId);
        if (customer is null || status is null) return BadRequest(new { error = "The customer or job status is not valid for this organisation." });
        if (request.SiteId is not null && !await db.Sites.AnyAsync(x => x.Id == request.SiteId && x.CustomerId == customer.Id && x.OrganisationId == current.OrganisationId)) return BadRequest(new { error = "The selected site does not belong to this customer." });
        var subscription = await db.Subscriptions.SingleAsync(x => x.OrganisationId == current.OrganisationId);
        var activeCount = await db.Jobs.CountAsync(x => x.OrganisationId == current.OrganisationId && !x.Status.IsTerminal);
        if (!status.IsTerminal && activeCount >= subscription.ActiveJobLimit) return StatusCode(402, new { error = "Your plan's active job limit has been reached." });
        var organisation = await db.Organisations.SingleAsync(x => x.Id == current.OrganisationId);
        var job = Map(new Job { OrganisationId = current.OrganisationId, JobNumber = $"JOB-{organisation.NextJobNumber:D5}", Title = "" }, request); organisation.NextJobNumber++; customer.LastActivityAt = DateTime.UtcNow;
        db.Jobs.Add(job); ActivityWriter.Add(db, current, "job", job.Id, "created", $"Created {job.JobNumber}: {job.Title}"); await db.SaveChangesAsync();
        return Created($"/api/jobs/{job.Id}", new { job.Id, job.JobNumber });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> Get(Guid id)
    {
        var current = this.CurrentUser();
        var job = await db.Jobs.AsNoTracking().Where(x => x.Id == id && x.OrganisationId == current.OrganisationId)
            .Include(x => x.Customer).Include(x => x.Site).Include(x => x.Status).Include(x => x.Assignments).ThenInclude(x => x.User).SingleOrDefaultAsync();
        if (job is null) return NotFound(); if (!await CanAccess(job.Id, current)) return Forbid();
        var items = await db.JobItems.AsNoTracking().Where(x => x.JobId == id && x.OrganisationId == current.OrganisationId).OrderBy(x => x.CreatedAt).ToListAsync();
        var notes = await db.JobNotes.AsNoTracking().Where(x => x.JobId == id && x.OrganisationId == current.OrganisationId).Include(x => x.User).OrderByDescending(x => x.CreatedAt).Select(x => new { x.Id, x.Body, x.IsCustomerVisible, x.CreatedAt, user = x.User.DisplayName }).ToListAsync();
        var documents = await db.Documents.AsNoTracking().Where(x => x.OrganisationId == current.OrganisationId && x.OwnerType == DocumentOwnerType.Job && x.OwnerId == id).OrderByDescending(x => x.CreatedAt).Select(x => new { x.Id, x.FileName, x.ContentType, x.SizeBytes, x.CreatedAt }).ToListAsync();
        var activity = await db.Activities.AsNoTracking().Where(x => x.OrganisationId == current.OrganisationId && x.EntityType == "job" && x.EntityId == id).OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync();
        return Ok(new { job, items, notes, documents, activity });
    }

    [HttpPatch("{id:guid}")]
    [RequireWorkstockAuth(OrganisationRole.Owner, OrganisationRole.Manager)]
    public async Task<ActionResult> Update(Guid id, JobRequest request)
    {
        var current = this.CurrentUser(); var job = await db.Jobs.SingleOrDefaultAsync(x => x.Id == id && x.OrganisationId == current.OrganisationId);
        if (job is null) return NotFound();
        if (!await db.Customers.AnyAsync(x => x.Id == request.CustomerId && x.OrganisationId == current.OrganisationId) || !await db.JobStatuses.AnyAsync(x => x.Id == request.JobStatusId && x.OrganisationId == current.OrganisationId)) return BadRequest(new { error = "The customer or job status is not valid for this organisation." });
        if (request.SiteId is not null && !await db.Sites.AnyAsync(x => x.Id == request.SiteId && x.CustomerId == request.CustomerId && x.OrganisationId == current.OrganisationId)) return BadRequest(new { error = "The selected site does not belong to this customer." });
        Map(job, request); job.UpdatedAt = DateTime.UtcNow; ActivityWriter.Add(db, current, "job", id, "updated", $"Updated {job.JobNumber}"); await db.SaveChangesAsync(); return Ok(job);
    }

    [HttpPost("{id:guid}/status")]
    public async Task<ActionResult> ChangeStatus(Guid id, StatusChangeRequest request)
    {
        var current = this.CurrentUser(); var job = await db.Jobs.Include(x => x.Status).SingleOrDefaultAsync(x => x.Id == id && x.OrganisationId == current.OrganisationId);
        if (job is null) return NotFound(); if (!await CanAccess(job.Id, current)) return Forbid();
        var next = await db.JobStatuses.SingleOrDefaultAsync(x => x.Id == request.JobStatusId && x.OrganisationId == current.OrganisationId);
        if (next is null) return BadRequest(new { error = "The requested status is invalid." }); if (next.Id == job.JobStatusId) return Ok(new { job.Id, status = next.Name });
        var previous = job.Status.Name; job.JobStatusId = next.Id; job.UpdatedAt = DateTime.UtcNow; if (next.IsTerminal && next.Name == "Completed") job.CompletedAt ??= DateTime.UtcNow; else if (!next.IsTerminal) job.CompletedAt = null;
        ActivityWriter.Add(db, current, "job", id, "status_changed", $"Changed status: {previous} → {next.Name}"); await db.SaveChangesAsync(); return Ok(new { job.Id, status = next.Name, job.CompletedAt });
    }

    [HttpPost("{id:guid}/assignments")]
    [RequireWorkstockAuth(OrganisationRole.Owner, OrganisationRole.Manager)]
    public async Task<ActionResult> Assign(Guid id, AssignmentRequest request)
    {
        var current = this.CurrentUser(); if (!await db.Jobs.AnyAsync(x => x.Id == id && x.OrganisationId == current.OrganisationId)) return NotFound();
        var assignee = await db.Users.SingleOrDefaultAsync(x => x.Id == request.UserId && x.OrganisationId == current.OrganisationId && x.IsActive); if (assignee is null) return BadRequest(new { error = "The selected employee is not active in this organisation." });
        if (await db.JobAssignments.AnyAsync(x => x.JobId == id && x.UserId == request.UserId)) return NoContent();
        db.JobAssignments.Add(new JobAssignment { OrganisationId = current.OrganisationId, JobId = id, UserId = request.UserId }); ActivityWriter.Add(db, current, "job", id, "assigned", $"Assigned {assignee.DisplayName}"); await db.SaveChangesAsync(); return NoContent();
    }

    [HttpDelete("{id:guid}/assignments/{userId:guid}")]
    [RequireWorkstockAuth(OrganisationRole.Owner, OrganisationRole.Manager)]
    public async Task<IActionResult> Unassign(Guid id, Guid userId)
    {
        var current = this.CurrentUser(); var assignment = await db.JobAssignments.SingleOrDefaultAsync(x => x.JobId == id && x.UserId == userId && x.OrganisationId == current.OrganisationId); if (assignment is null) return NotFound(); db.Remove(assignment); ActivityWriter.Add(db, current, "job", id, "unassigned", "Removed job assignment"); await db.SaveChangesAsync(); return NoContent();
    }

    [HttpPost("{id:guid}/items")]
    public async Task<ActionResult> AddItem(Guid id, JobItemRequest request)
    {
        var current = this.CurrentUser(); if (!await CanAccess(id, current)) return NotFound();
        var subscription = await db.Subscriptions.SingleAsync(x => x.OrganisationId == current.OrganisationId);
        var count = await db.JobItems.CountAsync(x => x.OrganisationId == current.OrganisationId && db.Jobs.Any(j => j.Id == x.JobId && !j.Status.IsTerminal));
        if (count >= subscription.ActiveJobItemLimit) return StatusCode(402, new { error = "Your plan's active job-item limit has been reached." });
        var item = new JobItem { OrganisationId = current.OrganisationId, JobId = id, Name = request.Name.Trim(), Quantity = request.Quantity, Unit = request.Unit.Trim(), Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim() }; db.JobItems.Add(item);
        ActivityWriter.Add(db, current, "job", id, "item_added", $"Added {item.Quantity} {item.Unit} {item.Name}"); await db.SaveChangesAsync(); return Created($"/api/jobs/{id}/items/{item.Id}", item);
    }

    [HttpPatch("{id:guid}/items/{itemId:guid}")]
    public async Task<ActionResult> UpdateItem(Guid id, Guid itemId, JobItemRequest request, [FromQuery] bool? completed)
    {
        var current = this.CurrentUser(); if (!await CanAccess(id, current)) return NotFound(); var item = await db.JobItems.SingleOrDefaultAsync(x => x.Id == itemId && x.JobId == id && x.OrganisationId == current.OrganisationId); if (item is null) return NotFound();
        item.Name = request.Name.Trim(); item.Quantity = request.Quantity; item.Unit = request.Unit.Trim(); item.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(); if (completed is not null) item.IsCompleted = completed.Value; await db.SaveChangesAsync(); return Ok(item);
    }

    [HttpPost("{id:guid}/notes")]
    public async Task<ActionResult> AddNote(Guid id, JobNoteRequest request)
    {
        var current = this.CurrentUser(); if (!await CanAccess(id, current)) return NotFound(); var note = new JobNote { OrganisationId = current.OrganisationId, JobId = id, UserId = current.Id, Body = request.Body.Trim(), IsCustomerVisible = request.IsCustomerVisible }; db.JobNotes.Add(note); ActivityWriter.Add(db, current, "job", id, "note_added", "Added a job note"); await db.SaveChangesAsync(); return Created($"/api/jobs/{id}/notes/{note.Id}", note);
    }

    private async Task<bool> CanAccess(Guid jobId, CurrentUser current) => current.IsManagerOrOwner() || await db.JobAssignments.AnyAsync(x => x.JobId == jobId && x.OrganisationId == current.OrganisationId && x.UserId == current.Id);
    private static Job Map(Job x, JobRequest r)
    {
        x.CustomerId = r.CustomerId; x.SiteId = r.SiteId; x.JobStatusId = r.JobStatusId; x.Title = r.Title.Trim(); x.Description = Clean(r.Description); x.Priority = r.Priority; x.ScheduledStart = r.ScheduledStart; x.ScheduledEnd = r.ScheduledEnd; x.DueDate = r.DueDate; x.EstimatedPrice = r.EstimatedPrice; x.ActualPrice = r.ActualPrice; x.InternalNotes = Clean(r.InternalNotes); x.CustomerNotes = Clean(r.CustomerNotes); x.ExternalReference = Clean(r.ExternalReference); x.AssetReference = Clean(r.AssetReference); return x;
    }
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
