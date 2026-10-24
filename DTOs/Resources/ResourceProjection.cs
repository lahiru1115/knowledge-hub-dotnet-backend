using System.Linq.Expressions;
using KnowledgeHub.Api.DTOs.Tags;
using KnowledgeHub.Api.Models;

namespace KnowledgeHub.Api.DTOs.Resources;

public static class ResourceProjection
{
    // One place that turns a Resource into its API shape.
    // It is an Expression, so EF Core can translate it into SQL.
    public static readonly Expression<Func<Resource, ResourceResponse>> ToResponse = x =>
        new ResourceResponse
        {
            Id = x.Id,
            CollectionId = x.CollectionId,
            Title = x.Title,
            Url = x.Url,
            Notes = x.Notes,
            ResourceType = x.ResourceType,
            CreatedAt = x.CreatedAt,
            Tags = x.ResourceTags
                .Select(rt => new TagResponse
                {
                    Id = rt.Tag.Id,
                    Name = rt.Tag.Name,
                    CreatedAt = rt.Tag.CreatedAt
                })
                .ToList()
        };
}
