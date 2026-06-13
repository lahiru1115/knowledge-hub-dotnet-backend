namespace KnowledgeHub.Api.Models;

public class Resource
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CollectionId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Url { get; set; }

    public string? Notes { get; set; }

    public ResourceType ResourceType { get; set; } = ResourceType.Other;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Collection Collection { get; set; } = null!;

    public ICollection<ResourceTag> ResourceTags { get; set; } = new List<ResourceTag>();
}