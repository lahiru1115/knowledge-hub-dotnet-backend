namespace KnowledgeHub.Api.DTOs.Collections;

public class CollectionCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}