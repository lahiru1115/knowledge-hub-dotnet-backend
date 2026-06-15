using KnowledgeHub.Api.Data;
using KnowledgeHub.Api.DTOs.Tags;
using KnowledgeHub.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeHub.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/tags")]
public class TagsController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    [HttpGet]
    public async Task<ActionResult<List<TagResponse>>> GetAll()
    {
        var tags = await _context.Tags
            .OrderBy(x => x.Name)
            .Select(x => new TagResponse
            {
                Id = x.Id,
                Name = x.Name,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(tags);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TagResponse>> GetById(Guid id)
    {
        var tag = await _context.Tags
            .Where(x => x.Id == id)
            .Select(x => new TagResponse
            {
                Id = x.Id,
                Name = x.Name,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (tag == null)
        {
            return NotFound(new
            {
                message = "Tag not found"
            });
        }

        return Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<TagResponse>> Create(TagCreateRequest request)
    {
        var name = request.Name.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new
            {
                message = "Tag name is required"
            });
        }

        var exists = await _context.Tags.AnyAsync(x => x.Name == name);

        if (exists)
        {
            return BadRequest(new
            {
                message = "Tag already exists"
            });
        }

        var tag = new Tag
        {
            Name = name
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        return Ok(new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            CreatedAt = tag.CreatedAt
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TagResponse>> Update(
        Guid id,
        TagUpdateRequest request
    )
    {
        var tag = await _context.Tags.FindAsync(id);

        if (tag == null)
        {
            return NotFound(new
            {
                message = "Tag not found"
            });
        }

        var name = request.Name.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new
            {
                message = "Tag name is required"
            });
        }

        var exists = await _context.Tags
            .AnyAsync(x => x.Name == name && x.Id != id);

        if (exists)
        {
            return BadRequest(new
            {
                message = "Tag already exists"
            });
        }

        tag.Name = name;

        await _context.SaveChangesAsync();

        return Ok(new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            CreatedAt = tag.CreatedAt
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tag = await _context.Tags.FindAsync(id);

        if (tag == null)
        {
            return NotFound(new
            {
                message = "Tag not found"
            });
        }

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Tag deleted successfully"
        });
    }
}