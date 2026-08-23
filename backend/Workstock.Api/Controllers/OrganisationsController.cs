using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workstock.Api.Data;
using Workstock.Api.Models;

namespace Workstock.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganisationsController(WorkstockDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Organisation>>> GetOrganisations()
    {
        var organisations = await db.Organisations
            .AsNoTracking()
            .ToListAsync();

        return Ok(organisations);
    }

    [HttpPost]
    public async Task<ActionResult<Organisation>> CreateOrganisation(
        Organisation organisation)
    {
        db.Organisations.Add(organisation);
        await db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetOrganisation),
            new { id = organisation.Id },
            organisation);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Organisation>> GetOrganisation(Guid id)
    {
        var organisation = await db.Organisations.FindAsync(id);

        if (organisation is null)
        {
            return NotFound();
        }

        return Ok(organisation);
    }
}