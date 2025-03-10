using Microsoft.AspNetCore.Mvc;
using FiwFriends.Data;
using FiwFriends.Models;
using FiwFriends.Services;
using Microsoft.EntityFrameworkCore;
using FiwFriends.DTOs;
using NuGet.Packaging;
using Microsoft.AspNetCore.Authorization;

namespace FiwFriends.Controllers;

[Authorize]
public class PostController : Controller
{
    private readonly ApplicationDBContext _db;
    private readonly CurrentUserService _currentUser;
    private readonly MapperService _mapper;
    private readonly string _apiKey;
    public PostController(ApplicationDBContext db, CurrentUserService currentUser, MapperService mapperService, string apiKey){
        _db = db;
        _currentUser = currentUser;
        _mapper = mapperService;
        _apiKey = apiKey;
    }

    public async Task<IActionResult> Index(){              
        var user = await _currentUser.GetCurrentUser();                     //Get all post
        IQueryable<Post> postCondition = _db.Posts.Where(p => p.ExpiredTime > DateTimeOffset.UtcNow && 
                                                    !p.Participants.Any(j => j.UserId == user.Id) &&        //exclude for joined post
                                                    p.OwnerId != user.Id &&                                 //exclude for owner
                                                    !p.Forms.Select(f => f.UserId).Contains(user.Id));      //exclude for submitted user
        
        var indexPosts = await _mapper.MapAsync<IQueryable<Post>, IEnumerable<IndexPost>>(postCondition);
        return View(indexPosts);                                                //return view with List<IndexPost>
    }

    [HttpGet("Search")]
    public async Task<IActionResult> Search(string search)
    {
        if (string.IsNullOrWhiteSpace(search)) return RedirectToAction("Index");

        var user = await _currentUser.GetCurrentUser(); 
        var postCondition = _db.Posts
                        .Where(p => (p.Activity.ToLower().Contains(search.ToLower()) || 
                                     p.Description.ToLower().Contains(search.ToLower())) && 
                                     p.ExpiredTime > DateTimeOffset.UtcNow && 
                                    !p.Participants.Any(j => j.UserId == user.Id) &&        //exclude for joined post
                                    p.OwnerId != user.Id &&                                 //exclude for owner
                                    !p.Forms.Select(f => f.UserId).Contains(user.Id));

        var indexPosts = await _mapper.MapAsync<IQueryable<Post>, IEnumerable<IndexPost>>(postCondition);

        return View("Index", indexPosts);
    }

    [HttpPost("Filter")]
    public async Task<IActionResult> Filter(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return RedirectToAction("Index");
        
        var user = await _currentUser.GetCurrentUser(); 
        var postCondition = _db.Posts
                        .Where(p => p.ExpiredTime > DateTimeOffset.UtcNow && 
                                    p.Tags.Any(t => t.Name.ToLower() == tag.ToLower()) && 
                                    !p.Participants.Any(j => j.UserId == user.Id) &&        //exclude for joined post
                                    p.OwnerId != user.Id &&                                 //exclude for owner
                                    !p.Forms.Select(f => f.UserId).Contains(user.Id));

        var indexPosts = await _mapper.MapAsync<IQueryable<Post>, IEnumerable<IndexPost>>(postCondition);
        ViewBag.SelectedTag = tag;
        return View("Index", indexPosts.ToList());
    }

    [HttpGet("Post/Detail/{id}")]
    async public Task<IActionResult> Detail(int id){                            //Detail of Single Post exluded Forms
        var query = _db.Posts
                        .Where(p => p.PostId == id);
        try {
            var post = await _mapper.MapAsync<IQueryable<Post>, DetailPost>(query);
            var uri = new Uri(Request.Headers["Referer"].ToString());
            ViewData["prev_page"] = uri.AbsolutePath.TrimStart('/');

            return View(post);                                                  //Return view with DetailPost
        }
        catch (Exception e){
            return NotFound(e.Message);
        }
    }

    //GET Create page
    [HttpGet("Post/Create")]
    public IActionResult Create(){                                              //Get Create page
        ViewData["API_KEY"] = _apiKey;
        return View();
    }

    //POST Create
    [HttpPost("Post/Create")]
    async public Task<IActionResult> Create(PostDTO post){                      //Create Post by PostDTO
        if (!ModelState.IsValid) {
            ViewData["API_KEY"] = _apiKey;
            return View(post);
        } 

        var postModel = await _mapper.MapAsync<PostDTO, Post>(post);
        
        await _db.Posts.AddAsync(postModel);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Post created successfully!";
        return RedirectToAction("Detail", new { id = postModel.PostId });              //Redirect to Detail of this post
    }

    //DELETE Post
    [HttpPost("Post/Delete/{id}")]
    async public Task<IActionResult> Delete(int id){                        //Delete Post by just PostId
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound(); 

        var user = await _currentUser.GetCurrentUser();
        if (post.OwnerId != user.Id) return Unauthorized("You are not owner of this post"); 

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Post successfully deleted!";
        return RedirectToAction("MyPost", "User");                               //Rediret to Mypost
    }

    [HttpPost("Post/Close/{postId}")]
    async public Task<IActionResult> Close(int postId){                     //Just close post by PostId
        var post = await _db.Posts.FindAsync(postId);
        if(post == null) return NotFound("Post not found");

        var user = await _currentUser.GetCurrentUser();
        if(user.Id != post.OwnerId) return Unauthorized("You don't have permission!");

        post.ExpiredTime = DateTimeOffset.UtcNow;
        await _db.Forms
                    .Where(f => f.PostId == postId && f.Status == FormStatus.Pending)
                    .ExecuteDeleteAsync();
        await _db.SaveChangesAsync();
        TempData["Message"] = "Post closed successfully!";
        return RedirectToAction("MyPost", "User");                                               //Return to another View (or may be jsut Ok()?)
    }

    [HttpGet("Post/Join/{id}")]
    async public Task<IActionResult> Join(int id){                          //Join post by PostId with current User logged in
        var user = await _currentUser.GetCurrentUser();   
        var post = await _db.Posts.Where(p => p.PostId == id).Include(p => p.Participants).FirstOrDefaultAsync();
        if (post == null) return NotFound("Post is not found.");
        if (post.ExpiredTime < DateTimeOffset.UtcNow) return BadRequest("It's already expired bro");
        
        var joined = post.Participants.Any(j => (j.UserId == user.Id  && j.PostId == post.PostId) || post.OwnerId == user.Id );        //Already Join
        if ( joined ) return BadRequest("You've already joined this post");

        if (post.Participants.Count + 1 >= post.Limit){
            post.ExpiredTime = DateTimeOffset.UtcNow;
            await _db.Forms
                    .Where(f => f.PostId == post.PostId && f.Status == FormStatus.Pending)
                    .ExecuteDeleteAsync();
        }

        await _db.Joins.AddAsync(new Join {
            UserId = user.Id,
            PostId = id
        });
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");                              //Return detail of this post
    }

    [HttpPost("Post/Favorite/{id}")]
    public async Task<IActionResult> Favorite(int id)
    {
        var user = await _currentUser.GetCurrentUser();
        var post = await _db.Posts.Include(p => p.FavoritedBy).FirstOrDefaultAsync(p => p.PostId == id);

        if (post == null)
        {
            Console.WriteLine($"[ERROR] Post ID {id} not found.");
            return NotFound("Post not found.");
        }
        if (user == null)
        {
            Console.WriteLine("[ERROR] User not logged in.");
            return RedirectToAction("Login", "Auth");
        }

        // Debug: เช็กว่าผู้ใช้กด Favorite หรือไม่
        bool isAlreadyFavorited = post.FavoritedBy.Any(u => u.Id == user.Id);
        Console.WriteLine($"[DEBUG] Post ID: {id}, User ID: {user.Id}, IsFav: {isAlreadyFavorited}");

        if (isAlreadyFavorited)
        {
            post.FavoritedBy.Remove(post.FavoritedBy.First(u => u.Id == user.Id));
        }
        else
        {
            post.FavoritedBy.Add(user);
        }

        int result = await _db.SaveChangesAsync();
        Console.WriteLine($"[DEBUG] SaveChangesAsync Result: {result}");

        if (result > 0)
        {
            return Ok(new { success = true, isFav = !isAlreadyFavorited });
        }
        else
        {
            return BadRequest("Failed to update favorite.");
        }
    }

    
    [HttpGet("Post/Favorite")]
    async public Task<IActionResult> GetFavoritedPost(){                    //Get current User's Favorited Post (or maybe this should be in UserController?)
        var user = await _currentUser.GetCurrentUser();

        var postCondition = _db.Posts
                                .Where(p => p.FavoritedBy.Any(u => u.Id == user.Id));
                                                        
        var indexPosts = await _mapper.MapAsync<IQueryable<Post>, IEnumerable<IndexPost>>(postCondition);
        return View(indexPosts);                                              //Return view with List<IndexPost>
    }
}   