public class OrganisationCreateRequest
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt {get; set; }
}