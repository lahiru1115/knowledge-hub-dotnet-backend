using System.Security.Claims;
using KnowledgeHub.Api.Data;
using KnowledgeHub.Api.DTOs.Collections;
using KnowledgeHub.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeHub.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/collections")]
public class CollectionsController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
        return Guid.Parse(userId);
    }

    [HttpGet]
    public async Task<ActionResult<List<CollectionResponse>>> GetAll()
    {
        var userId = GetUserId();

        var collections = await _context.Collections
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new CollectionResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(collections);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CollectionResponse>> GetById(Guid id)
    {
        var userId = GetUserId();

        var collection = await _context.Collections
            .Where(x => x.Id == id && x.UserId == userId)
            .Select(x => new CollectionResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (collection == null)
        {
            return NotFound(new
            {
                message = "Collection not found"
            });
        }

        return Ok(collection);
    }

    [HttpPost]
    public async Task<ActionResult<CollectionResponse>> Create(
        CollectionCreateRequest request
    )
    {
        var userId = GetUserId();

        var collection = new Collection
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim()
        };

        _context.Collections.Add(collection);
        await _context.SaveChangesAsync();

        return Ok(new CollectionResponse
        {
            Id = collection.Id,
            Name = collection.Name,
            Description = collection.Description,
            CreatedAt = collection.CreatedAt
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CollectionResponse>> Update(
        Guid id,
        CollectionUpdateRequest request
    )
    {
        var userId = GetUserId();

        var collection = await _context.Collections
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (collection == null)
        {
            return NotFound(new
            {
                message = "Collection not found"
            });
        }

        collection.Name = request.Name.Trim();
        collection.Description = request.Description?.Trim();

        await _context.SaveChangesAsync();

        return Ok(new CollectionResponse
        {
            Id = collection.Id,
            Name = collection.Name,
            Description = collection.Description,
            CreatedAt = collection.CreatedAt
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();

        var collection = await _context.Collections
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (collection == null)
        {
            return NotFound(new
            {
                message = "Collection not found"
            });
        }

        _context.Collections.Remove(collection);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Collection deleted successfully"
        });
    }
}