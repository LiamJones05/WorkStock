using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workstock.Api.Data;
using Workstock.Api.DTOs;
using Workstock.Api.Infrastructure;
using Workstock.Api.Models;

namespace Workstock.Api.Controllers;

[ApiController, Route("api/customers"), RequireWorkstockAuth]
public sealed class CustomersController(WorkstockDbContext db) : ControllerBase
{
    [HttpGet]
    [RequireWorkstockAuth(OrganisationRole.Owner, OrganisationRole.Manager)]
    public async Task<ActionResult> List([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 30)
    {
        var orgId = this.CurrentUser().OrganisationId; page = Math.Max(page, 1); pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Customers.AsNoTracking().Where(x => x.OrganisationId == orgId);
        if (!string.IsNullOrWhiteSpace(q)) { var term = q.Trim().ToLower(); query = query.Where(x => x.Name.ToLower().Contains(term) || (x.CompanyName ?? "").ToLower().Contains(term) || (x.Email ?? "").ToLower().Contains(term)); }
        var total = await query.CountAsync();
        var customers = await query.OrderByDescending(x => x.LastActivityAt).ThenBy(x => x.Name).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new { x.Id, x.Name, x.CompanyName, x.Email, x.Phone, x.City, x.PostCode, x.LastActivityAt, x.CreatedAt }).ToListAsync();
        return Ok(new { items = customers, total, page, pageSize });
    }

    [HttpPost]
    [RequireWorkstockAuth(OrganisationRole.Owner, OrganisationRole.Manager)]
    public async Task<ActionResult> Create(CustomerRequest request)
    {
        var current = this.CurrentUser();
        var subscription = await db.Subscriptions.SingleAsync(x => x.OrganisationId == current.OrganisationId);
        if (await db.Customers.CountAsync(x => x.OrganisationId == current.OrganisationId) >= subscription.CustomerLimit) return StatusCode(402, new { error = "Your plan's customer limit has been reached." });
        var customer = Map(new Customer { OrganisationId = current.OrganisationId, Name = "" }, request); db.Customers.Add(customer);
        ActivityWriter.Add(db, current, "customer", customer.Id, "created", $"Created customer {customer.Name}"); await db.SaveChangesAsync();
        return Created($"/api/customers/{customer.Id}", customer);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> Get(Guid id)
    {
        var current = this.CurrentUser();
        var customer = await db.Customers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.OrganisationId == current.OrganisationId);
        if (customer is null) return NotFound();
        if (!current.IsManagerOrOwner() && !await db.JobAssignments.AnyAsync(a => a.UserId == current.Id && a.Job.CustomerId == id)) return Forbid();
        var sites = await db.Sites.AsNoTracking().Where(x => x.OrganisationId == current.OrganisationId && x.CustomerId == id).OrderBy(x => x.Name).ToListAsync();
        var jobs = await db.Jobs.AsNoTracking().Where(x => x.OrganisationId == current.OrganisationId && x.CustomerId == id).Include(x => x.Status).OrderByDescending(x => x.ScheduledStart).Take(25)
            .Select(x => new { x.Id, x.JobNumber, x.Title, Status = x.Status.Name, x.Priority, x.ScheduledStart, x.CompletedAt }).ToListAsync();
        return Ok(new { customer, sites, jobs });
    }

    [HttpPatch("{id:guid}")]
    [RequireWorkstockAuth(OrganisationRole.Owner, OrganisationRole.Manager)]
    public async Task<ActionResult> Update(Guid id, CustomerRequest request)
    {
        var current = this.CurrentUser(); var customer = await db.Customers.SingleOrDefaultAsync(x => x.Id == id && x.OrganisationId == current.OrganisationId);
        if (customer is null) return NotFound(); Map(customer, request); customer.UpdatedAt = DateTime.UtcNow;
        ActivityWriter.Add(db, current, "customer", id, "updated", $"Updated customer {customer.Name}"); await db.SaveChangesAsync(); return Ok(customer);
    }

    [HttpPost("{customerId:guid}/sites")]
    [RequireWorkstockAuth(OrganisationRole.Owner, OrganisationRole.Manager)]
    public async Task<ActionResult> CreateSite(Guid customerId, SiteRequest request)
    {
        var current = this.CurrentUser(); if (!await db.Customers.AnyAsync(x => x.Id == customerId && x.OrganisationId == current.OrganisationId)) return NotFound();
        var site = Map(new Site { OrganisationId = current.OrganisationId, CustomerId = customerId, Name = "" }, request); db.Sites.Add(site);
        ActivityWriter.Add(db, current, "site", site.Id, "created", $"Created site {site.Name}"); await db.SaveChangesAsync(); return Created($"/api/sites/{site.Id}", site);
    }

    [HttpPatch("{customerId:guid}/sites/{siteId:guid}")]
    [RequireWorkstockAuth(OrganisationRole.Owner, OrganisationRole.Manager)]
    public async Task<ActionResult> UpdateSite(Guid customerId, Guid siteId, SiteRequest request)
    {
        var current = this.CurrentUser(); var site = await db.Sites.SingleOrDefaultAsync(x => x.Id == siteId && x.CustomerId == customerId && x.OrganisationId == current.OrganisationId);
        if (site is null) return NotFound(); Map(site, request); site.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(); return Ok(site);
    }

    private static Customer Map(Customer x, CustomerRequest r)
    {
        x.Name = r.Name.Trim(); x.CompanyName = Clean(r.CompanyName); x.Email = Clean(r.Email); x.Phone = Clean(r.Phone); x.AddressLine1 = Clean(r.AddressLine1); x.AddressLine2 = Clean(r.AddressLine2); x.City = Clean(r.City); x.County = Clean(r.County); x.PostCode = Clean(r.PostCode); x.Country = Clean(r.Country); x.Notes = Clean(r.Notes); return x;
    }
    private static Site Map(Site x, SiteRequest r)
    {
        x.Name = r.Name.Trim(); x.AddressLine1 = Clean(r.AddressLine1); x.AddressLine2 = Clean(r.AddressLine2); x.City = Clean(r.City); x.County = Clean(r.County); x.PostCode = Clean(r.PostCode); x.Country = Clean(r.Country); x.AccessInstructions = Clean(r.AccessInstructions); x.Notes = Clean(r.Notes); return x;
    }
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
