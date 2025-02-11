using Microsoft.AspNetCore.Mvc;
using FiwFriends.Data;
using FiwFriends.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;

namespace FiwFriends.Controllers;

public class PostController : Controller
{
    //define DB context
    private readonly ApplicationDBContext _db;
    public PostController(ApplicationDBContext db){
        _db = db;
    }

    //GET all
    public IActionResult Index(){   
        IEnumerable<Post> allPost = _db.Posts;
        return Ok(allPost);
    }

    public IActionResult Detail(int id){
        var post = _db.Posts
                    .Where(p => p.PostId == id)
                    .Include(p => p.Participants)
                    .ThenInclude(j => j.User)
                    .FirstOrDefault();
        if(post == null){
            return NotFound();
        }
        return Ok(new {
            post.PostId,
            post.Activity,
            post.Description,
            post.AppointmentTime,
            post.ExpiredTime,
            Participants = post.Participants.Select(j => new {
                j.UserId,
                j.User.Username
            })
        });
    }
    //GET Create page
    public string Create(){
        return "Show page to create new post.";
    }
    //POST Create
    [HttpPost]
    public IActionResult Create([FromBody] Post post){ //Delete [FromBody] if need to send request from View.
        if (!ModelState.IsValid){
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors)){
                Console.WriteLine(error.ErrorMessage);
            }
            return BadRequest(ModelState);
        }

        post.OwnerId = 1;  //From current user
        _db.Add(post);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }
    //GET Delete Page, maybe unneccessary
    public string Delete(){
        return "Show page to delete post."; 
    }
    //DELETE Post
    [HttpDelete("Post/{id}")]
    public IActionResult Delete(int id){
        //todo: Check if post belong to current user
        var post = _db.Posts.Find(id);
        if (post == null){
            return NotFound();
        }
        _db.Posts.Remove(post);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    //PUT Update Post
    [HttpPut("Post/{id}")]
    public IActionResult Edit(int id, [FromBody] Post post){ //Delete [FromBody] if need to send request from View.
        Console.WriteLine(post.ToJson());
        int row_affected = _db.Posts
                            .Where(p => p.PostId == id)
                            .ExecuteUpdate(setters => setters
                                .SetProperty(p => p.Activity, post.Activity)
                                .SetProperty(p => p.Description, post.Description)
                                .SetProperty(p => p.ExpiredTime, post.ExpiredTime)
                                .SetProperty(p => p.AppointmentTime, post.AppointmentTime)
                                .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));
        if (row_affected == 0){
            return NotFound("Post is not found to delete.");
        }

        return RedirectToAction("Index");
    }

    [HttpPost("Post/Join/{id}")]
    public IActionResult Join(int id){
        var user = _db.Users.Find(1);   //get current user
        var post = _db.Posts.Find(id);
        if (post == null){
            return NotFound("Post is not found.");
        }
        if (user == null){
            return NotFound("User is not found.");
        }
        var join = new Join{
            UserId = user.UserId,
            PostId = post.PostId
        };
        _db.Joins.Add(join);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }
    [HttpPost("Post/Favorite/{id}")]
    public IActionResult Favorite(int id){
        var user = _db.Users.Find(1);   //get current user
        var post = _db.Posts.Find(id);
        if (post == null){
            return NotFound("Post is not found.");
        }
        if (user == null){
            return NotFound("User is not found.");
        }
        post.FavoritedBy.Add(user);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }
}
