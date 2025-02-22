using Microsoft.AspNetCore.Mvc;
using FiwFriends.Data;
using FiwFriends.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using FiwFriends.DTOs;
using FiwFriends.Services;
using AspNetCoreGeneratedDocument;
using System.Threading.Tasks;

namespace FiwFriends.Controllers;

public class PostController : Controller
{
    private readonly ApplicationDBContext _db;
    private readonly CurrentUserService _currentUser;
    private readonly MapperService _mapper;
    public PostController(ApplicationDBContext db, CurrentUserService currentUser, MapperService mapperService){
        _db = db;
        _currentUser = currentUser;
        _mapper = mapperService;
    }

    //GET all
    public async Task<IActionResult> Index(){   
        IEnumerable<Post> allPost = await _db.Posts.Include(p => p.Owner)
                                            .Include(p => p.Participants).ThenInclude(j => j.User)
                                            .Include(p => p.Tags)
                                            .Include(p => p.FavoritedBy).ToListAsync();
        List<IndexPost> indexPosts = new List<IndexPost>();
        foreach (var post in allPost){
            indexPosts.Add(await _mapper.MapAsync<Post, IndexPost>(post));
        }
        return Ok(indexPosts.Select(p => p.Activity));
    }

    [HttpGet("Search/{search}")]
    async public Task<IActionResult> Search(string search){
        var posts = await _db.Posts.Where(p => p.Activity.ToLower().Contains(search.ToLower()) || p.Description.ToLower().Contains(search.ToLower()))
                                    .Include(p => p.Owner)
                                    .Include(p => p.Participants).ThenInclude(j => j.User)
                                    .Include(p => p.Tags)
                                    .Include(p => p.FavoritedBy)
                                    .ToListAsync();
        List<IndexPost> indexPosts = new List<IndexPost>();

        foreach (var post in posts){
            indexPosts.Add(await _mapper.MapAsync<Post, IndexPost>(post));
        }
        return Ok(indexPosts.Select(p => p.Tags));
    }

    [HttpPost("Filter")]
    public IActionResult Filter([FromBody] IEnumerable<TagDTO> tags){
        var posts = _db.Posts
                        .Include(p => p.Tags)
                        .Where(p => tags.Select(t => t.Name.ToLower()).All(t => p.Tags.Select(t => t.Name.ToLower()).Contains(t)));
        return Ok(posts);
    }

    [HttpGet("Post/{id}")]
    async public Task<IActionResult> Detail(int id){
        var post = _db.Posts.Where(p => p.PostId == id)
                    .Include(p => p.Participants).ThenInclude(j => j.User)
                    .Include(p => p.Owner)
                    .Include(p => p.FavoritedBy)
                    .Include(p => p.Questions)
                    .Include(p => p.Forms).ThenInclude(f => f.Answers)
                    .Include(p => p.Tags).FirstOrDefault();
        if(post == null){
            return NotFound();
        }
        return Ok(await _mapper.MapAsync<Post, DetailPost>(post));
    }
    //GET Create page
    public IActionResult Create(){
        return View();
    }

    //POST Create
    [HttpPost("Post")]
    async public Task<IActionResult> Create([FromBody] PostDTO post){ //Delete [FromBody] if need to send request from View.
        if (!ModelState.IsValid){
            return BadRequest(ModelState);
        }

        var ownerId = await _currentUser.GetCurrentUserId(); //Replace with actual current user logic
        if (ownerId == null){
            return RedirectToAction("Login", "Auth");
        }

        await _db.Posts.AddAsync(await _mapper.MapAsync<PostDTO, Post>(post));
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    //DELETE Post
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
                                .SetProperty(p => p.ExpiredTime, DateTimeOffset.Parse(post.ExpiredTime))
                                .SetProperty(p => p.AppointmentTime, DateTimeOffset.Parse(post.AppointmentTime))
                                .SetProperty(p => p.UpdatedAt, DateTime.UtcNow)
                                .SetProperty(p => p.Tags, tags));
        if (row_affected == 0){
            return NotFound("Post is not found to delete.");
        }

        return RedirectToAction("Detail", new { id });
    }

    [HttpPost("Post/Close/{postId}")]
    async public Task<IActionResult> Close(int postId){
        var post = await _db.Posts.FindAsync(postId);
        if(post == null){
            return NotFound("Post not found");  
        }

        var userId = await _currentUser.GetCurrentUserId();
        if(userId == null){
            return RedirectToAction("Login", "Auth");
        }
        var user = await _db.Users.FindAsync(userId);
        if(user == null){
            return RedirectToAction("Login", "Auth");
        }
        if(userId != post.OwnerId){
            return Unauthorized("You don't have permission");
        }

        post.ExpiredTime = DateTimeOffset.UtcNow;
        return Ok(post);
    }

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
    
    // [HttpPost("Post/Favorite")]
    // async public Task<IActionResult> GetFavoritedPost(){
    //     var userId = await _currentUser.GetCurrentUserId();
    //     if(userId == null){
    //         return RedirectToAction("Login", "Auth");
    //     }
    //     foreach await _db.Users.Select(p => p.FavoritePosts).ToListAsync();
    //     favPosts = 


    //     return Ok(favPosts);

    // }
}
