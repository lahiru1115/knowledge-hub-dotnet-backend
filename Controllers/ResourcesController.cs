using System.Security.Claims;
using KnowledgeHub.Api.Data;
using KnowledgeHub.Api.DTOs.Resources;
using KnowledgeHub.Api.DTOs.Tags;
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
        [FromQuery(Name = "collection_id")] Guid? collectionId,
        [FromQuery(Name = "resource_type")] ResourceType? resourceType,
        [FromQuery(Name = "tag_id")] Guid? tagId,
        [FromQuery(Name = "created_from")] DateTime? createdFrom,
        [FromQuery(Name = "created_to")] DateTime? createdTo,
        [FromQuery] string sort = "newest",
        [FromQuery] int page = 1,
        [FromQuery(Name = "page_size")] int pageSize = 20
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
            var keyword = search.Trim();

            query = query.Where(x =>
                EF.Functions.ILike(x.Title, $"%{keyword}%") ||
                (x.Notes != null && EF.Functions.ILike(x.Notes, $"%{keyword}%")) ||
                (x.Url != null && EF.Functions.ILike(x.Url, $"%{keyword}%"))
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

        if (tagId.HasValue)
        {
            query = query.Where(x => x.ResourceTags.Any(rt => rt.TagId == tagId.Value));
        }

        // Dates arrive without a time zone; Npgsql needs UTC.
        if (createdFrom.HasValue)
        {
            var from = DateTime.SpecifyKind(createdFrom.Value.Date, DateTimeKind.Utc);
            query = query.Where(x => x.CreatedAt >= from);
        }

        if (createdTo.HasValue)
        {
            // created_to includes the whole day.
            var to = DateTime.SpecifyKind(createdTo.Value.Date, DateTimeKind.Utc).AddDays(1);
            query = query.Where(x => x.CreatedAt < to);
        }

        var ordered = sort.ToLowerInvariant() switch
        {
            "newest" => query.OrderByDescending(x => x.CreatedAt),
            "oldest" => query.OrderBy(x => x.CreatedAt),
            "title_asc" => query.OrderBy(x => x.Title),
            _ => null
        };

        if (ordered == null)
        {
            return Problem(
                detail: "sort must be one of: newest, oldest, title_asc",
                statusCode: 400
            );
        }

        var total = await query.CountAsync();

        var resources = await ordered
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ResourceProjection.ToResponse)
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
            .Select(ResourceProjection.ToResponse)
            .FirstOrDefaultAsync();

        if (resource == null)
        {
            return Problem(detail: "Resource not found", statusCode: 404);
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
            return Problem(detail: "Collection not found", statusCode: 404);
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
            return Problem(detail: "Resource not found", statusCode: 404);
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
            return Problem(detail: "Resource not found", statusCode: 404);
        }

        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Resource deleted successfully"
        });
    }
}