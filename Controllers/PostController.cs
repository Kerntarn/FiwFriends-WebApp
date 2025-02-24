using Microsoft.AspNetCore.Mvc;
using FiwFriends.Data;
using FiwFriends.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using FiwFriends.DTOs;
using FiwFriends.Services;
using Microsoft.AspNetCore.Authorization;

namespace FiwFriends.Controllers;

public class PostController : Controller
{
    private readonly ApplicationDBContext _db;
    private readonly CurrentUserService _currentUser;
    public PostController(ApplicationDBContext db, CurrentUserService currentUser){
        _db = db;
        _currentUser = currentUser;
    }

    //GET all
    public IActionResult Index(){   
        IEnumerable<Post> allPost = _db.Posts;
        return View(allPost);
    }

    [HttpGet("Post/{id}")]
    public IActionResult Detail(int id){
        var post = _db.Posts
                    .Where(p => p.PostId == id)
                    .Include(p => p.Participants)
                    .ThenInclude(j => j.User)
                    .Include(p => p.Owner)
                    .Include(p => p.FavoritedBy)
                    .Include(p => p.Questions)
                    .Include(p => p.Forms)
                    .ThenInclude(f => f.Answers)
                    .Include(p => p.Tags)
                    .FirstOrDefault();
        if(post == null){
            return NotFound();
        }
        return View(post);
    }
    //GET Create page
    public IActionResult Create(){
        return View();
    }

    //POST Create
    [Authorize]
    [HttpPost("Post")]
    async public Task<IActionResult> Create(PostDTO post){ //Delete [FromBody] if need to send request from View.
        if (!ModelState.IsValid){
            return BadRequest(ModelState);
        }
        var ownerId = await _currentUser.GetCurrentUserId(); //Replace with actual current user logic
        if (ownerId == null){
            return RedirectToAction("Login", "Auth");
        }
        await _db.Posts.AddAsync(new Post{
            Activity = post.Activity,
            Description = post.Description,
            ExpiredTime = post.ExpiredTime,
            AppointmentTime = post.AppointmentTime,
            OwnerId = ownerId,
            Tags = _db.Tags.Where(t => post.Tags.Select( dto => dto.Name ).Contains(t.Name)).ToList(),   //Attach Tags
            Questions = post.Questions.Select(q => new Question{
                Content = q.Content
            }).ToList()
        });
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    //DELETE Post
    [Authorize]
    [HttpDelete("Post/{id}")]
    async public Task<IActionResult> Delete(int id){
        //todo: Check if post belong to current user
        var post = _db.Posts.Find(id);
        if (post == null){
            return NotFound();
        }
        if (post.OwnerId != await _currentUser.GetCurrentUserId()){
            return Unauthorized();
        }
        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    //PUT Update Post
    [Authorize]
    [HttpPut("Post/{id}")]
    async public Task<IActionResult> Edit(int id, PostDTO post){ //Delete [FromBody] if need to send request from View.
        var tags = await _db.Tags
                        .Where(t => post.Tags.Select(dto => dto.Name).Contains(t.Name))
                        .ToListAsync();
        int row_affected = await _db.Posts
                            .Where(p => p.PostId == id)
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(p => p.Activity, post.Activity)
                                .SetProperty(p => p.Description, post.Description)
                                .SetProperty(p => p.ExpiredTime, post.ExpiredTime)
                                .SetProperty(p => p.AppointmentTime, post.AppointmentTime)
                                .SetProperty(p => p.UpdatedAt, DateTime.UtcNow)
                                .SetProperty(p => p.Tags, tags));
        if (row_affected == 0){
            return NotFound("Post is not found to delete.");
        }

        return RedirectToAction("Detail", new { id });
    }

    [Authorize]
    [HttpPost("Post/Join/{id}")]
    async public Task<IActionResult> Join(int id){
        var userId = await _currentUser.GetCurrentUserId();   
        var post = await _db.Posts.FindAsync(id);
        if (post == null){
            return NotFound("Post is not found.");
        }
        if (userId == null){
            return RedirectToAction("Login", "Auth");
        }
        var join = new Join{
            UserId = userId,  
            PostId = id
        };
        await _db.Joins.AddAsync(join);
        await _db.SaveChangesAsync();
        return RedirectToAction("Detail", new { id });
    }

    [Authorize]
    [HttpPost("Post/Favorite/{id}")]
    async public Task<IActionResult> Favorite(int id){
        var userId = await _currentUser.GetCurrentUserId();   
        var post = await _db.Posts.FindAsync(id);
        if (post == null){
            return NotFound("Post is not found.");
        }
        if (userId == null){
            return RedirectToAction("Login", "Auth");
        }
        if (post.FavoritedBy.Any(u => u.Id == userId)){
            post.FavoritedBy.Remove(post.FavoritedBy.First(u => u.Id == userId));
        } else {
            var user = await _db.Users.FindAsync(userId);
            if (user == null){
                return NotFound("User is not found.");
            }
            post.FavoritedBy.Add(user);
        }
        await _db.SaveChangesAsync();
        return Ok("Index");
    }
    
}
