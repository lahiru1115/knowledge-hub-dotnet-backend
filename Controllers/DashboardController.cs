using System.Security.Claims;
using KnowledgeHub.Api.Data;
using KnowledgeHub.Api.DTOs.Dashboard;
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

        return Ok(new DashboardResponse
        {
            CollectionsCount = collectionsCount,
            ResourcesCount = resourcesCount,
            TagsCount = tagsCount
        });
    }
}