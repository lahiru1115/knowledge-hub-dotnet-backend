namespace KnowledgeHub.Api.DTOs.Collections;

public class CollectionUpdateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}