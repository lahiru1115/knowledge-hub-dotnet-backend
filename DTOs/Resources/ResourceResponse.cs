namespace KnowledgeHub.Api.DTOs.Resources;

public class ResourceResponse
{
    public Guid Id { get; set; }
    public Guid CollectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Notes { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<string> Tags { get; set; } = [];
}