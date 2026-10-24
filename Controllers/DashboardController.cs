using System.Security.Claims;
using KnowledgeHub.Api.Data;
using KnowledgeHub.Api.DTOs.Dashboard;
using KnowledgeHub.Api.DTOs.Resources;
using KnowledgeHub.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeHub.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        return Guid.Parse(userId);
    }

    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Get()
    {
        var userId = GetUserId();

        var collectionsCount = await _context.Collections
            .CountAsync(x => x.UserId == userId);

        var resourcesCount = await _context.Resources
            .Include(x => x.Collection)
            .CountAsync(x => x.Collection.UserId == userId);

        var tagsCount = await _context.Tags.CountAsync();

        var recentResources = await _context.Resources
            .Where(x => x.Collection.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .Select(ResourceProjection.ToResponse)
            .ToListAsync();

        var countsByType = await _context.Resources
            .Where(x => x.Collection.UserId == userId)
            .GroupBy(x => x.ResourceType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        // Always return every type, even when the count is 0.
        var typeBreakdown = Enum.GetValues<ResourceType>().ToDictionary(
            type => type.ToString().ToLowerInvariant(),
            type => countsByType.FirstOrDefault(c => c.Type == type)?.Count ?? 0
        );

        return Ok(new DashboardResponse
        {
            CollectionsCount = collectionsCount,
            ResourcesCount = resourcesCount,
            TagsCount = tagsCount,
            RecentResources = recentResources,
            TypeBreakdown = typeBreakdown
        });
    }
}