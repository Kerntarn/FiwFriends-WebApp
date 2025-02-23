using Microsoft.AspNetCore.Mvc;
using FiwFriends.Data;
using FiwFriends.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using FiwFriends.DTOs;
using FiwFriends.Services;

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
        return Ok(allPost);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Post>> GetPostById(int id)
    {
        var post = await _db.Posts
            .Where(p => p.PostId == id)
            .Include(p => p.Participants)
                .ThenInclude(j => j.User)
            .Include(p => p.Owner)
            .Include(p => p.FavoritedBy)
            .Include(p => p.Questions)
            .Include(p => p.Forms)
                .ThenInclude(f => f.Answers)
            .Include(p => p.Tags)
            .FirstOrDefaultAsync();

        if (post == null)
        {
            return NotFound();
        }

        return Ok(post);
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
        return Ok(new{
            post.PostId,
            user = post.Participants.Select(p => p.UserId)}
            );
    }
    //GET Create page
    public IActionResult Create(){
        return Ok();
    }

    //POST Create
    [HttpPost("/Post/Create")]
    public async Task<ActionResult<Post>> CreatePost([FromBody] PostDTO postDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ownerId = await _currentUser.GetCurrentUserId();
            if (ownerId == null)
            {
                return Unauthorized("User not logged in");
            }

            var tags = await _db.Tags
                .Where(t => postDto.Tags.Select(dto => dto.Name).Contains(t.Name))
                .ToListAsync();

            var post = new Post
            {
                Activity = postDto.Activity,
                Description = postDto.Description,
                ExpiredTime = postDto.ExpiredTime,
                AppointmentTime = postDto.AppointmentTime,
                OwnerId = ownerId,
                Tags = tags,
                Questions = postDto.Questions.Select(q => new Question
                {
                    Content = q.Content
                }).ToList()
            };

            await _db.Posts.AddAsync(post);
            await _db.SaveChangesAsync();

            return Ok(new {post.PostId});
        }

    //DELETE Post
    [HttpDelete("Post/{id}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        var userId = await _currentUser.GetCurrentUserId();
        if (post.OwnerId != userId)
        {
            return Unauthorized("Not authorized to delete this post");
        }

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    //PUT Update Post
    [HttpPut("Post/{id}")]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] PostDTO postDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var post = await _db.Posts
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.PostId == id);

        if (post == null)
        {
            return NotFound();
        }

        var userId = await _currentUser.GetCurrentUserId();
        if (post.OwnerId != userId)
        {
            return Unauthorized("Not authorized to update this post");
        }

        var tags = await _db.Tags
            .Where(t => postDto.Tags.Select(dto => dto.Name).Contains(t.Name))
            .ToListAsync();

        post.Activity = postDto.Activity;
        post.Description = postDto.Description;
        post.ExpiredTime = postDto.ExpiredTime;
        post.AppointmentTime = postDto.AppointmentTime;
        post.UpdatedAt = DateTime.UtcNow;
        post.Tags = tags;

        await _db.SaveChangesAsync();

        return NoContent();
    }


    [HttpPost("Post/Join/{postId}")]
    public async Task<IActionResult> JoinPost(int postId)
    {
        var userId = await _currentUser.GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized("User not logged in");
        }

        var post = await _db.Posts.FindAsync(postId);
        if (post == null)
        {
            return NotFound("Post not found");
        }

        var existingJoin = await _db.Joins
            .FirstOrDefaultAsync(j => j.UserId == userId && j.PostId == postId);

        if (existingJoin != null)
        {
            return BadRequest("Already joined this event");
        }

        var join = new Join
        {
            UserId = userId,
            PostId = postId
        };

        await _db.Joins.AddAsync(join);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Joined successfully", postId });
    }


    [HttpPost("Post/Favorite/{id}")]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
        var userId = await _currentUser.GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized("User not logged in");
        }

        var post = await _db.Posts
            .Include(p => p.FavoritedBy)
            .FirstOrDefaultAsync(p => p.PostId == id);

        if (post == null)
        {
            return NotFound("Post not found");
        }

        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        if (post.FavoritedBy.Any(u => u.Id == userId))
        {
            post.FavoritedBy.Remove(user);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Post unfavorited", postId = id });
        }
        else
        {
            post.FavoritedBy.Add(user);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
