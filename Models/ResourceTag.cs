namespace KnowledgeHub.Api.Models;

public class ResourceTag
{
    public Guid ResourceId { get; set; }

    public Guid TagId { get; set; }

    public Resource Resource { get; set; } = null!;

    public Tag Tag { get; set; } = null!;
}