using KnowledgeHub.Api.DTOs.Tags;
using KnowledgeHub.Api.Models;

namespace KnowledgeHub.Api.DTOs.Resources;

public class ResourceResponse
{
    public Guid Id { get; set; }
    public Guid CollectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Notes { get; set; }
    public ResourceType ResourceType { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TagResponse> Tags { get; set; } = [];
}