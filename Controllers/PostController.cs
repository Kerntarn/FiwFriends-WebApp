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
    public PostController(ApplicationDBContext db, CurrentUserService currentUser, MapperService mapperService){
        _db = db;
        _currentUser = currentUser;
        _mapper = mapperService;
    }

    private IQueryable<Post> BasePostQuery()
    {
        return _db.Posts.Include(p => p.Owner)
                        .Include(p => p.Participants).ThenInclude(j => j.User)
                        .Include(p => p.Tags)
                        .Include(p => p.FavoritedBy);
    }

    public async Task<IActionResult> Index(){              
        var user = await _currentUser.GetCurrentUser();                     //Get all post
        IEnumerable<Post> allPost = await BasePostQuery()
                                            .Where(p => p.ExpiredTime > DateTimeOffset.UtcNow && 
                                                        !p.Participants.Any(j => j.UserId == user.Id) &&        //exclude for joined post
                                                        p.OwnerId != user.Id)                                   //exclude for owner
                                            .ToListAsync();
        List<IndexPost> indexPosts = new List<IndexPost>();
        foreach (var post in allPost){
            indexPosts.Add(await _mapper.MapAsync<Post, IndexPost>(post));
        }
        return View(indexPosts);                                                //return view with List<IndexPost>
    }

    [HttpGet("Search/{search}")]
    async public Task<IActionResult> Search(string search){                     //Search by check activity and description string
        var posts = await BasePostQuery()
                            .Where(p => (p.Activity.ToLower().Contains(search.ToLower()) || 
                                         p.Description.ToLower().Contains(search.ToLower())) && 
                                         p.ExpiredTime > DateTimeOffset.UtcNow )
                            .ToListAsync();
        List<IndexPost> indexPosts = new List<IndexPost>();

        foreach (var post in posts){
            indexPosts.Add(await _mapper.MapAsync<Post, IndexPost>(post));
        }
        return View(indexPosts);                                                //return view with List<IndexPost>
    }

    [HttpPost("Filter")]
    async public Task<IActionResult> Filter(IEnumerable<TagDTO> tags){          //Filter by Tags
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var posts = await BasePostQuery()
                            .Where(p => p.ExpiredTime > DateTimeOffset.UtcNow && 
                                        tags.Select(t => t.Name.ToLower())
                                            .All(t => p.Tags.Select(t => t.Name.ToLower()).Contains(t)))
                            .ToListAsync();

        List<IndexPost> indexPosts = new List<IndexPost>();
        foreach (var post in posts){
            indexPosts.Add(await _mapper.MapAsync<Post, IndexPost>(post));
        }                
        return View(posts);                                                     //return view with List<IndexPost>
    }

    [HttpGet("Post/Detail/{id}")]
    async public Task<IActionResult> Detail(int id){                            //Detail of Single Post exluded Forms
        var post = await BasePostQuery()
                        .Where(p => p.PostId == id)
                        .Include(p => p.Questions)
                        .FirstOrDefaultAsync();

        if(post == null) return NotFound();
        return View(await _mapper.MapAsync<Post, DetailPost>(post));            //Return view with DetailPost
    }

    //GET Create page
    [HttpGet("Post/Create")]
    public IActionResult Create(){                                              //Get Create page
        return View();
    }

    //POST Create
    [HttpPost("Post/Create")]
    async public Task<IActionResult> Create(PostDTO post){                      //Create Post by PostDTO
        if (!ModelState.IsValid) return BadRequest(ModelState);

        foreach (var tag in post.Tags)
        {
            Console.WriteLine($"Tag: {tag.Name}");
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
        await _db.SaveChangesAsync();
        TempData["Message"] = "Post closed successfully!";
        return RedirectToAction("MyPost", "User");                                               //Return to another View (or may be jsut Ok()?)
    }

    [HttpGet("Post/Join/{id}")]
    async public Task<IActionResult> Join(int id){                          //Join post by PostId with current User logged in
        var user = await _currentUser.GetCurrentUser();   
        var post = await _db.Posts.Where(p => p.PostId == id).Include(p => p.Participants).FirstOrDefaultAsync();
        if (post == null) return NotFound("Post is not found.");
        
        var joined = post.Participants.Any(j => (j.UserId == user.Id  && j.PostId == post.PostId) || post.OwnerId == user.Id );        //Already Join
        if ( joined ) return RedirectToAction("Detail", new { id = post.PostId});

        if (post.Participants.Count >= post.Limit) return BadRequest("The post has reached its participant limit.");

        await _db.Joins.AddAsync(new Join {
            UserId = user.Id,
            PostId = id
        });
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");                              //Return detail of this post
    }

    [HttpPost("Post/Favorite/{id}")]
    async public Task<IActionResult> Favorite(int id){                      //Just Favorite Post by PostId with current User logged in
        var user = await _currentUser.GetCurrentUser();   
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound("Post is not found.");

        if (post.FavoritedBy.Any(u => u.Id == user.Id)){
             post.FavoritedBy.Remove(post.FavoritedBy.First(u => u.Id == user.Id));
         } else {
             post.FavoritedBy.Add(user);
         }
         await _db.SaveChangesAsync();

         return Ok();                                                        //Done
     }
    
    [HttpGet("Post/Favorite")]
    async public Task<IActionResult> GetFavoritedPost(){                    //Get current User's Favorited Post (or maybe this should be in UserController?)
        var user = await _currentUser.GetCurrentUser();

        List<Post> posts = await BasePostQuery()
                                    .Where(p => p.FavoritedBy.Any(u => u.Id == user.Id))
                                    .ToListAsync();
                                                        
        List<IndexPost> fav_post = new List<IndexPost>();
        foreach (var post in posts){
            fav_post.Add(await _mapper.MapAsync<Post, IndexPost>(post));
        }
            
        return View(fav_post);                                              //Return view with List<IndexPost>
    }
}   