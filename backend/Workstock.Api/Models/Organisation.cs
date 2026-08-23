namespace Workstock.Api.Models;

public class Organisation
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? WebsiteUrl { get; set; }

    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? County { get; set; }
    public string? PostCode { get; set; }
    public string? Country { get; set; }

    public string? LogoUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}