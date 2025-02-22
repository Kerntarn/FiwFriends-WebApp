using Microsoft.AspNetCore.Mvc;
using FiwFriends.Data;
using FiwFriends.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using FiwFriends.DTOs;
using FiwFriends.Services;
using AspNetCoreGeneratedDocument;
using System.Threading.Tasks;
using NuGet.Packaging;

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
        IEnumerable<Post> allPost = await _db.Posts.Where(p => p.ExpiredTime < DateTimeOffset.UtcNow)
                                            .Include(p => p.Owner)
                                            .Include(p => p.Participants).ThenInclude(j => j.User)
                                            .Include(p => p.Tags)
                                            .Include(p => p.FavoritedBy).ToListAsync();
        List<IndexPost> indexPosts = new List<IndexPost>();
        foreach (var post in allPost){
            indexPosts.Add(await _mapper.MapAsync<Post, IndexPost>(post));
        }
        return Ok(indexPosts.Select(p => new { p.PostId, p.Activity }));
    }

    [HttpGet("Search/{search}")]
    async public Task<IActionResult> Search(string search){
        var posts = await _db.Posts.Where(p => (p.Activity.ToLower().Contains(search.ToLower()) || p.Description.ToLower().Contains(search.ToLower())) && p.ExpiredTime < DateTimeOffset.UtcNow )
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
        var posts = _db.Posts.Where(p => tags.Select(t => t.Name.ToLower()).All(t => p.Tags.Select(t => t.Name.ToLower()).Contains(t)))
                        .Include(p => p.Tags);
                        
        return Ok(posts);
    }

    [HttpGet("Post/{id}")]
    async public Task<IActionResult> Detail(int id){
        var post = await _db.Posts.Where(p => p.PostId == id)
                    .Include(p => p.Participants).ThenInclude(j => j.User)
                    .Include(p => p.Owner)
                    .Include(p => p.FavoritedBy)
                    .Include(p => p.Questions)
                    .Include(p => p.Forms).ThenInclude(f => f.Answers)
                    .Include(p => p.Tags).FirstOrDefaultAsync();

        if(post == null) return NotFound();

        return Ok(_mapper.MapAsync<Post, DetailPost>(post).GetAwaiter().GetResult().Activity);
    }
    //GET Create page
    public IActionResult Create(){
        return View();
    }

    //POST Create
    [HttpPost("Post")]
    async public Task<IActionResult> Create([FromBody] PostDTO post){ //Delete [FromBody] if need to send request from View.
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var owner = await _currentUser.GetCurrentUser(); //Replace with actual current user logic
        if (owner == null) return RedirectToAction("Login", "Auth");

        await _db.Posts.AddAsync(await _mapper.MapAsync<PostDTO, Post>(post));
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    //DELETE Post
    [HttpDelete("Post/{id}")]
    async public Task<IActionResult> Delete(int id){
        var post = _db.Posts.Find(id);
        if (post == null) return NotFound(); 

        var user = await _currentUser.GetCurrentUser();
        if ( user == null ) return RedirectToAction("Login", "Auth"); 
        if (post.OwnerId != user.Id) return Unauthorized(); 

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    //PUT Update Post
    [HttpPut("Post/{id}")]
    async public Task<IActionResult> Edit(int id,[FromBody] PostDTO post){ //Delete [FromBody] if need to send request from View.
        if(!ModelState.IsValid) return BadRequest("Invalid DTO");

        var user = await _currentUser.GetCurrentUser();
        if(user == null) return RedirectToAction("Login", "Auth");

        int row_affected = await _db.Posts
                            .Where(p => p.PostId == id && p.OwnerId == user.Id)
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(p => p.Activity, post.Activity)
                                .SetProperty(p => p.Description, post.Description)
                                .SetProperty(p => p.Location, post.Location)
                                .SetProperty(p => p.ExpiredTime, DateTimeOffset.Parse(post.ExpiredTime))
                                .SetProperty(p => p.AppointmentTime, DateTimeOffset.Parse(post.AppointmentTime))
                                .SetProperty(p => p.Limit, post.Limit)
                                .SetProperty(p => p.UpdatedAt, DateTimeOffset.UtcNow));
                                //Question??
        if (row_affected == 0) return NotFound("Post is not found to delete.");
        
        var existingPost = await _db.Posts.Where(p => p.PostId == id && p.OwnerId == user.Id).Include(p => p.Tags).FirstOrDefaultAsync();
        if( existingPost == null ) return NotFound("Post not found");

        var tags = await _db.Tags.Where(t => post.Tags.Select(dto => dto.Name).Contains(t.Name))
                        .ToListAsync();
        existingPost.Tags.Clear();
        existingPost.Tags.AddRange(tags);

        await _db.SaveChangesAsync();
        return RedirectToAction("Detail", id);
    }

    [HttpPost("Post/Close/{postId}")]
    async public Task<IActionResult> Close(int postId){
        var post = await _db.Posts.FindAsync(postId);
        if(post == null){ return NotFound("Post not found"); }

        var user = await _currentUser.GetCurrentUser();
        if(user == null){ return RedirectToAction("Login", "Auth"); }
        if(user.Id != post.OwnerId){ return Unauthorized("You don't have permission!"); }

        post.ExpiredTime = DateTimeOffset.UtcNow;
        return Ok();
    }

    [HttpPost("Post/Join/{id}")]
    async public Task<IActionResult> Join(int id){
        var user = await _currentUser.GetCurrentUser();   
        var post = await _db.Posts.FindAsync(id);
        if (post == null){ return NotFound("Post is not found."); }
        if (user == null){ return RedirectToAction("Login", "Auth"); }
        
        var join = new Join{
            UserId = user.Id,  
            PostId = id
        };
        await _db.Joins.AddAsync(join);
        await _db.SaveChangesAsync();
        return RedirectToAction("Detail", new { id });
    }

    [HttpPost("Post/Favorite/{id}")]
    async public Task<IActionResult> Favorite(int id){
        var user = await _currentUser.GetCurrentUser();   
        var post = await _db.Posts.FindAsync(id);
        if (post == null){ return NotFound("Post is not found."); }
        if (user == null){ return RedirectToAction("Login", "Auth"); }

        if (post.FavoritedBy.Any(u => u.Id == user.Id)){
            post.FavoritedBy.Remove(post.FavoritedBy.First(u => u.Id == user.Id));
        } else {
            post.FavoritedBy.Add(user);
        }
        await _db.SaveChangesAsync();

        return Ok("Index");
    }
    
    [HttpPost("Post/Favorite")]
    async public Task<IActionResult> GetFavoritedPost(){
        var user = await _currentUser.GetCurrentUser();
        if(user == null){
            return RedirectToAction("Login", "Auth");
        }
        return Ok();
    }
}   
