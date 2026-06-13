namespace KnowledgeHub.Api.Models;

public class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ResourceTag> ResourceTags { get; set; } = new List<ResourceTag>();
}