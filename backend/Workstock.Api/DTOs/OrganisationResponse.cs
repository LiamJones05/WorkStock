namespace Workstock.Api.DTOs;

public class OrganisationResponse
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public string? Email { get; init; }

    public string? PhoneNumber { get; init; }

    public string? WebsiteUrl { get; init; }

    public string? AddressLine1 { get; init; }

    public string? AddressLine2 { get; init; }

    public string? City { get; init; }

    public string? County { get; init; }

    public string? PostCode { get; init; }

    public string? Country { get; init; }

    public string? LogoUrl { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}