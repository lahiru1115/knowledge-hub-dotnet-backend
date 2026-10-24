using KnowledgeHub.Api.DTOs.Resources;

namespace KnowledgeHub.Api.DTOs.Dashboard;

public class DashboardResponse
{
    public int CollectionsCount { get; set; }
    public int ResourcesCount { get; set; }
    public int TagsCount { get; set; }
    public List<ResourceResponse> RecentResources { get; set; } = [];

    // Keys are lowercase resource types: article, video, pdf, website, other.
    public Dictionary<string, int> TypeBreakdown { get; set; } = [];
}
