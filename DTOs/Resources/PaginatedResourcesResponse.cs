namespace KnowledgeHub.Api.DTOs.Resources;

public class PaginatedResourcesResponse
{
    public List<ResourceResponse> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
}