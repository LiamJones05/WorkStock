using System.ComponentModel.DataAnnotations;
using Workstock.Api.Models;

namespace Workstock.Api.DTOs;

public sealed class RegisterRequest
{
    [Required, StringLength(160, MinimumLength = 2)] public string OrganisationName { get; init; } = "";
    [Required, StringLength(120, MinimumLength = 2)] public string DisplayName { get; init; } = "";
    [Required, EmailAddress, StringLength(320)] public string Email { get; init; } = "";
    [Required, StringLength(128, MinimumLength = 12)] public string Password { get; init; } = "";
}
public sealed class LoginRequest { [Required, EmailAddress] public string Email { get; init; } = ""; [Required] public string Password { get; init; } = ""; }
public sealed class CreateUserRequest { [Required, StringLength(120)] public string DisplayName { get; init; } = ""; [Required, EmailAddress] public string Email { get; init; } = ""; [Required, StringLength(128, MinimumLength = 12)] public string Password { get; init; } = ""; public OrganisationRole Role { get; init; } = OrganisationRole.Employee; }
public sealed class UpdateUserRequest { [Required, StringLength(120)] public string DisplayName { get; init; } = ""; public OrganisationRole Role { get; init; } = OrganisationRole.Employee; public bool IsActive { get; init; } = true; }
public sealed class CustomerRequest
{
    [Required, StringLength(160)] public string Name { get; init; } = "";
    [StringLength(160)] public string? CompanyName { get; init; }
    [EmailAddress, StringLength(320)] public string? Email { get; init; }
    [StringLength(60)] public string? Phone { get; init; }
    [StringLength(160)] public string? AddressLine1 { get; init; }
    [StringLength(160)] public string? AddressLine2 { get; init; }
    [StringLength(100)] public string? City { get; init; }
    [StringLength(100)] public string? County { get; init; }
    [StringLength(20)] public string? PostCode { get; init; }
    [StringLength(100)] public string? Country { get; init; }
    [StringLength(5000)] public string? Notes { get; init; }
}
public sealed class SiteRequest
{
    [Required, StringLength(160)] public string Name { get; init; } = "";
    [StringLength(160)] public string? AddressLine1 { get; init; }
    [StringLength(160)] public string? AddressLine2 { get; init; }
    [StringLength(100)] public string? City { get; init; }
    [StringLength(100)] public string? County { get; init; }
    [StringLength(20)] public string? PostCode { get; init; }
    [StringLength(100)] public string? Country { get; init; }
    [StringLength(2000)] public string? AccessInstructions { get; init; }
    [StringLength(5000)] public string? Notes { get; init; }
}
public sealed class JobRequest
{
    [Required] public Guid CustomerId { get; init; }
    public Guid? SiteId { get; init; }
    [Required] public Guid JobStatusId { get; init; }
    [Required, StringLength(200)] public string Title { get; init; } = "";
    [StringLength(10000)] public string? Description { get; init; }
    public JobPriority Priority { get; init; } = JobPriority.Normal;
    public DateTime? ScheduledStart { get; init; }
    public DateTime? ScheduledEnd { get; init; }
    public DateTime? DueDate { get; init; }
    [Range(0, 999999999)] public decimal? EstimatedPrice { get; init; }
    [Range(0, 999999999)] public decimal? ActualPrice { get; init; }
    [StringLength(5000)] public string? InternalNotes { get; init; }
    [StringLength(5000)] public string? CustomerNotes { get; init; }
    [StringLength(200)] public string? ExternalReference { get; init; }
    [StringLength(200)] public string? AssetReference { get; init; }
}
public sealed class StatusChangeRequest { [Required] public Guid JobStatusId { get; init; } }
public sealed class AssignmentRequest { [Required] public Guid UserId { get; init; } }
public sealed class JobItemRequest { [Required, StringLength(200)] public string Name { get; init; } = ""; [Range(0.001, 999999)] public decimal Quantity { get; init; } = 1; [Required, StringLength(32)] public string Unit { get; init; } = "each"; [StringLength(2000)] public string? Notes { get; init; } }
public sealed class JobNoteRequest { [Required, StringLength(5000)] public string Body { get; init; } = ""; public bool IsCustomerVisible { get; init; } }
