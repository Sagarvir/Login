using Backend_API_s.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend_API_s.Controllers;

[ApiController]
[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TagsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Tag tag)
    {
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return Ok(tag);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignTag(int keyId, int tagId)
    {
        var keyTag = new KeyTag
        {
            TranslationKeyId = keyId,
            TagId = tagId
        };

        _context.KeyTags.Add(keyTag);
        await _context.SaveChangesAsync();

        return Ok();
    }
}
