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
using Microsoft.AspNetCore.Authorization;

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

    public async Task<IActionResult> Index(){                                   //Get all post
        IEnumerable<Post> allPost = await _db.Posts.Where(p => p.ExpiredTime < DateTimeOffset.UtcNow)
                                            .Include(p => p.Owner)
                                            .Include(p => p.Participants).ThenInclude(j => j.User)
                                            .Include(p => p.Tags)
                                            .Include(p => p.FavoritedBy).ToListAsync();
        List<IndexPost> indexPosts = new List<IndexPost>();
        foreach (var post in allPost){
            indexPosts.Add(await _mapper.MapAsync<Post, IndexPost>(post));
        }
        return View(indexPosts);                                                //return view with List<IndexPost>
    }

    [HttpGet("Search/{search}")]
    async public Task<IActionResult> Search(string search){                     //Search by check activity and description string
        var posts = await _db.Posts.Where(p => (p.Activity.ToLower().Contains(search.ToLower()) || p.Description.ToLower().Contains(search.ToLower())) && p.ExpiredTime < DateTimeOffset.UtcNow )
                                    .Include(p => p.Owner)
                                    .Include(p => p.Participants).ThenInclude(j => j.User)
                                    .Include(p => p.Tags)
                                    .Include(p => p.FavoritedBy).ToListAsync();
        List<IndexPost> indexPosts = new List<IndexPost>();

        foreach (var post in posts){
            indexPosts.Add(await _mapper.MapAsync<Post, IndexPost>(post));
        }
        return View(indexPosts);                                                //return view with List<IndexPost>
    }

    [HttpPost("Filter")]
    async public Task<IActionResult> Filter(IEnumerable<TagDTO> tags){          //Filter by Tags
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var posts = await _db.Posts.Where(p => tags.Select(t => t.Name.ToLower()).All(t => p.Tags.Select(t => t.Name.ToLower()).Contains(t)))
                        .Include(p => p.Owner)
                        .Include(p => p.Participants).ThenInclude(j => j.User)
                        .Include(p => p.Tags)
                        .Include(p => p.FavoritedBy).ToListAsync();

        List<IndexPost> indexPosts = new List<IndexPost>();
        foreach (var post in posts){
            indexPosts.Add(await _mapper.MapAsync<Post, IndexPost>(post));
        }                
        return View(posts);                                                     //return view with List<IndexPost>
    }

    [HttpGet("Post/{id}")]
    async public Task<IActionResult> Detail(int id){                            //Detail of Single Post exluded Forms
        var post = await _db.Posts.Where(p => p.PostId == id)
                    .Include(p => p.Participants).ThenInclude(j => j.User)
                    .Include(p => p.Owner)
                    .Include(p => p.FavoritedBy)
                    .Include(p => p.Questions)
                    .Include(p => p.Tags).FirstOrDefaultAsync();

        if(post == null) return NotFound();

        return View(await _mapper.MapAsync<Post, DetailPost>(post));            //Return view with DetailPost
    }
    //GET Create page
    [Authorize]
    [HttpGet("Post/Create")]
    public IActionResult Create(){                                              //Get Create page
        return View();
    }

    //POST Create
    [HttpPost("Post")]
    [Authorize]
    async public Task<IActionResult> Create(PostDTO post){                      //Create Post by PostDTO
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var postModel = await _mapper.MapAsync<PostDTO, Post>(post);
        await _db.Posts.AddAsync(postModel);
        await _db.SaveChangesAsync();
        return RedirectToAction("Detail", postModel.PostId);                //Redirect to Detail of this post
    }

    //DELETE Post
    [HttpDelete("Post/{id}")]
    [Authorize]
    async public Task<IActionResult> Delete(int id){                        //Delete Post by just PostId
        var post = _db.Posts.Find(id);
        if (post == null) return NotFound(); 

        var user = await _currentUser.GetCurrentUser();
        if ( user == null ) return RedirectToAction("Login", "Auth"); 
        if (post.OwnerId != user.Id) return Unauthorized("You are not owner of this post"); 

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");                                   //Rediret to Index
    }

    //PUT Update Post
    [HttpPut("Post/{id}")]
    [Authorize]
    async public Task<IActionResult> Edit(int id, PostDTO post){            //Edit Post by define PostId to edit and update info based on PostDTO
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
        return RedirectToAction("Detail", id);                              //return view with Detail of this post
    }

    [HttpPost("Post/Close/{postId}")]
    [Authorize]
    async public Task<IActionResult> Close(int postId){                     //Just close post by PostId
        var post = await _db.Posts.FindAsync(postId);
        if(post == null) return NotFound("Post not found");

        var user = await _currentUser.GetCurrentUser();
        if(user == null) return RedirectToAction("Login", "Auth");
        if(user.Id != post.OwnerId) return Unauthorized("You don't have permission!");

        post.ExpiredTime = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return View("Index");                                               //Return to another View (or may be jsut Ok()?)
    }

    [HttpPost("Post/Join/{id}")]
    [Authorize]
    async public Task<IActionResult> Join(int id){                          //Join post by PostId with current User logged in
        var user = await _currentUser.GetCurrentUser();   
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound("Post is not found.");
        if (user == null) return RedirectToAction("Login", "Auth");
        
        var join = new Join{
            UserId = user.Id,  
            PostId = id
        };
        await _db.Joins.AddAsync(join);
        await _db.SaveChangesAsync();
        return RedirectToAction("Detail", id);                              //Return detail of this post
    }

    [HttpPost("Post/Favorite/{id}")]
    [Authorize]
    async public Task<IActionResult> Favorite(int id){                      //Just Favorite Post by PostId with current User logged in
        var user = await _currentUser.GetCurrentUser();   
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound("Post is not found.");
        if (user == null) return RedirectToAction("Login", "Auth");

        if (post.FavoritedBy.Any(u => u.Id == user.Id)){
            post.FavoritedBy.Remove(post.FavoritedBy.First(u => u.Id == user.Id));
        } else {
            post.FavoritedBy.Add(user);
        }
        await _db.SaveChangesAsync();

        return Ok();                                                        //Done
    }
    
    [HttpPost("Post/Favorite")]
    [Authorize]
    async public Task<IActionResult> GetFavoritedPost(){                    //Get current User's Favorited Post (or maybe this should be in UserController?)
        var user = await _currentUser.GetCurrentUser();
        if(user == null) return RedirectToAction("Login", "Auth");
        List<Post> posts = await _db.Users.Where(u => u.Id == user.Id).SelectMany(p => p.FavoritePosts)
                                            .Include(p => p.Owner)
                                            .Include(p => p.Participants).ThenInclude(j => j.User)
                                            .Include(p => p.Tags)
                                            .Include(p => p.FavoritedBy).ToListAsync();
        List<IndexPost> fav_post = new List<IndexPost>();
        foreach (var post in posts){
            fav_post.Add(await _mapper.MapAsync<Post, IndexPost>(post));
        }
            
        return View(fav_post);                                              //Return view with List<IndexPost>
    }
}   
