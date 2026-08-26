using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workstock.Api.Data;
using Workstock.Api.Models;
using Workstock.Api.DTOs;
namespace Workstock.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganisationsController(WorkstockDbContext db) : ControllerBase
{

    [HttpPost]
    public async Task<ActionResult<OrganisationResponse>> CreateOrganisation(
        OrganisationCreateRequest request)
    {
        var organisation = new Organisation
        {
            Name = request.Name,
            Description = request.Description,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            WebsiteUrl = request.WebsiteUrl,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            County = request.County,
            PostCode = request.PostCode,
            Country = request.Country,
            LogoUrl = request.LogoUrl
        };
        db.Organisations.Add(organisation);
        await db.SaveChangesAsync();

        var response = new OrganisationResponse
        {
            Id = organisation.Id,
            Name = organisation.Name,
            Description = organisation.Description,
            Email = organisation.Email,
            PhoneNumber = organisation.PhoneNumber,
            WebsiteUrl = organisation.WebsiteUrl,
            AddressLine1 = organisation.AddressLine1,
            AddressLine2 = organisation.AddressLine2,
            City = organisation.City,
            County = organisation.County,
            PostCode = organisation.PostCode,
            Country = organisation.Country,
            LogoUrl = organisation.LogoUrl,
            CreatedAt = organisation.CreatedAt,
            UpdatedAt = organisation.UpdatedAt
        };

        return CreatedAtAction(
            nameof(GetOrganisation),
            new { id = organisation.Id },
            response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrganisationResponse>> GetOrganisation(Guid id)
    {
        var organisation = await db.Organisations.FindAsync(id);

        if (organisation is null)
        {
            return NotFound();
        }

        return Ok(organisation);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrganisationResponse>>> GetAllOrganisations()
    {
        var organisations = await db.Organisations
            .AsNoTracking()
            .ToListAsync();

        var responses = organisations.Select(organisation => new OrganisationResponse
        {
            Id = organisation.Id,
            Name = organisation.Name,
            Description = organisation.Description,
            Email = organisation.Email,
            PhoneNumber = organisation.PhoneNumber,
            WebsiteUrl = organisation.WebsiteUrl,
            AddressLine1 = organisation.AddressLine1,
            AddressLine2 = organisation.AddressLine2,
            City = organisation.City,
            County = organisation.County,
            PostCode = organisation.PostCode,
            Country = organisation.Country,
            LogoUrl = organisation.LogoUrl,
            CreatedAt = organisation.CreatedAt,
            UpdatedAt = organisation.UpdatedAt
        });
        return Ok(responses);
    }
}