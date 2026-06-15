using KnowledgeHub.Api.Models;

namespace KnowledgeHub.Api.DTOs.Resources;

public class ResourceCreateRequest
{
    public Guid CollectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Notes { get; set; }
    public ResourceType ResourceType { get; set; } = ResourceType.Other;
    public List<Guid> TagIds { get; set; } = [];
}