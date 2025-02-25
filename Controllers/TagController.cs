using FiwFriends.Data;
using FiwFriends.DTOs;
using FiwFriends.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace FiwFriends.Controllers;

public class TagController : Controller
{
    private readonly ApplicationDBContext _db;
    public TagController(ApplicationDBContext db)
    {
        _db = db;
    }
    async public Task<IActionResult> Index()                                //Show all Tags
    {
        IEnumerable<Tag> tags = await _db.Tags.ToListAsync();
        return View(tags);
    }
    
    [HttpPost("Tag")]
    [Authorize]
    async public Task<IActionResult> Create(TagDTO tag){         //Create Tag by DTO (may be this is done by AJAX?)
        var allTags = await _db.Tags.ToListAsync();
        if(allTags.Count == 0){
            await _db.Tags.AddAsync(new Tag{ Name = tag.Name});
        } else {
            var existing_tags = await _db.Tags.Where(t => t.Name.ToLower().Contains(tag.Name.ToLower())).ToListAsync();
            if(existing_tags.Count > 0){
                return BadRequest("This Tag Already Exists");
            }
            await _db.Tags.AddAsync(new Tag { Name = tag.Name });
        }
        await _db.SaveChangesAsync();
        return Ok();
    }

}