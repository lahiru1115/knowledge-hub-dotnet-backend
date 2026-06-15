using System.Security.Claims;
using KnowledgeHub.Api.Data;
using KnowledgeHub.Api.DTOs.Resources;
using KnowledgeHub.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeHub.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/resources")]
public class ResourcesController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        return Guid.Parse(userId);
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResourcesResponse>> GetAll(
        [FromQuery] string? search,
        [FromQuery] Guid? collectionId,
        [FromQuery] ResourceType? resourceType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        var userId = GetUserId();

        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var query = _context.Resources
            .Include(x => x.ResourceTags)
                .ThenInclude(x => x.Tag)
            .Include(x => x.Collection)
            .Where(x => x.Collection.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLower();

            query = query.Where(x =>
                x.Title.ToLower().Contains(keyword) ||
                (x.Notes != null && x.Notes.ToLower().Contains(keyword)) ||
                (x.Url != null && x.Url.ToLower().Contains(keyword))
            );
        }

        if (collectionId.HasValue)
        {
            query = query.Where(x => x.CollectionId == collectionId.Value);
        }

        if (resourceType.HasValue)
        {
            query = query.Where(x => x.ResourceType == resourceType.Value);
        }

        var total = await query.CountAsync();

        var resources = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ResourceResponse
            {
                Id = x.Id,
                CollectionId = x.CollectionId,
                Title = x.Title,
                Url = x.Url,
                Notes = x.Notes,
                ResourceType = x.ResourceType.ToString().ToLower(),
                CreatedAt = x.CreatedAt,
                Tags = x.ResourceTags
                    .Select(rt => rt.Tag.Name)
                    .ToList()
            })
            .ToListAsync();

        return Ok(new PaginatedResourcesResponse
        {
            Items = resources,
            Page = page,
            PageSize = pageSize,
            Total = total
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResourceResponse>> GetById(Guid id)
    {
        var userId = GetUserId();

        var resource = await _context.Resources
            .Include(x => x.ResourceTags)
                .ThenInclude(x => x.Tag)
            .Include(x => x.Collection)
            .Where(x => x.Id == id && x.Collection.UserId == userId)
            .Select(x => new ResourceResponse
            {
                Id = x.Id,
                CollectionId = x.CollectionId,
                Title = x.Title,
                Url = x.Url,
                Notes = x.Notes,
                ResourceType = x.ResourceType.ToString().ToLower(),
                CreatedAt = x.CreatedAt,
                Tags = x.ResourceTags
                    .Select(rt => rt.Tag.Name)
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (resource == null)
        {
            return NotFound(new
            {
                message = "Resource not found"
            });
        }

        return Ok(resource);
    }

    [HttpPost]
    public async Task<ActionResult<ResourceResponse>> Create(ResourceCreateRequest request)
    {
        var userId = GetUserId();

        var collectionExists = await _context.Collections
            .AnyAsync(x => x.Id == request.CollectionId && x.UserId == userId);

        if (!collectionExists)
        {
            return NotFound(new
            {
                message = "Collection not found"
            });
        }

        var resource = new Resource
        {
            CollectionId = request.CollectionId,
            Title = request.Title.Trim(),
            Url = string.IsNullOrWhiteSpace(request.Url) ? null : request.Url.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            ResourceType = request.ResourceType
        };

        if (request.TagIds.Count != 0)
        {
            var tags = await _context.Tags
                .Where(x => request.TagIds.Contains(x.Id))
                .ToListAsync();

            foreach (var tag in tags)
            {
                resource.ResourceTags.Add(new ResourceTag
                {
                    ResourceId = resource.Id,
                    TagId = tag.Id
                });
            }
        }

        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        return await GetById(resource.Id);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ResourceResponse>> Update(
        Guid id,
        ResourceUpdateRequest request
    )
    {
        var userId = GetUserId();

        var resource = await _context.Resources
            .Include(x => x.ResourceTags)
            .Include(x => x.Collection)
            .FirstOrDefaultAsync(x => x.Id == id && x.Collection.UserId == userId);

        if (resource == null)
        {
            return NotFound(new
            {
                message = "Resource not found"
            });
        }

        resource.Title = request.Title.Trim();
        resource.Url = string.IsNullOrWhiteSpace(request.Url) ? null : request.Url.Trim();
        resource.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        resource.ResourceType = request.ResourceType;

        resource.ResourceTags.Clear();

        if (request.TagIds.Count != 0)
        {
            var tags = await _context.Tags
                .Where(x => request.TagIds.Contains(x.Id))
                .ToListAsync();

            foreach (var tag in tags)
            {
                resource.ResourceTags.Add(new ResourceTag
                {
                    ResourceId = resource.Id,
                    TagId = tag.Id
                });
            }
        }

        await _context.SaveChangesAsync();

        return await GetById(resource.Id);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();

        var resource = await _context.Resources
            .Include(x => x.Collection)
            .FirstOrDefaultAsync(x => x.Id == id && x.Collection.UserId == userId);

        if (resource == null)
        {
            return NotFound(new
            {
                message = "Resource not found"
            });
        }

        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Resource deleted successfully"
        });
    }
}