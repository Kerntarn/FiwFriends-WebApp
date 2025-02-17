using Microsoft.AspNetCore.Mvc;
using FiwFriends.Data;
using FiwFriends.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using FiwFriends.DTOs;

namespace FiwFriends.Controllers;

public class PostController : Controller
{
    private readonly ApplicationDBContext _db;
    public PostController(ApplicationDBContext db){
        _db = db;
    }

    //GET all
    public IActionResult Index(){   
        IEnumerable<Post> allPost = _db.Posts;
        return Ok(allPost);
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
        return Ok(new {
            post.PostId,
            post.Activity,
            post.Description,
            post.AppointmentTime,
            post.ExpiredTime,
            Owner = new {
                post.Owner.Id,
                post.Owner.UserName
            },
            Participants = post.Participants.Select(j => new {
                j.UserId,
                j.User.UserName
            }),
            FavoritedBy = post.FavoritedBy.Select(u => new {
                u.Id,
                u.UserName
            }),
            Questions = post.Questions.Select(q => new {
                q.QuestionId,
                q.Content
            }),
            Forms = post.Forms.Select(f => new {
                f.FormId,
                f.IsApproved,
                Answers = f.Answers.Select(a => new {
                    a.Content,
                    a.QuestionId
                })
            }),
            Tags = post.Tags.Select(t => t.Name)
        });
    }

    //GET Create page
    public IActionResult Create(){
        return Ok("Show page to create new post.");
    }

    //POST Create
    [HttpPost("Post")]
    public IActionResult Create([FromBody] PostDTO post){ //Delete [FromBody] if need to send request from View.
        if (!ModelState.IsValid){
            return BadRequest(ModelState);
        }
        var ownerId = 8; //get current user
        _db.Posts.Add(new Post{
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
    public IActionResult Edit(int id, [FromBody] PostDTO post){ //Delete [FromBody] if need to send request from View.
        int row_affected = _db.Posts
                            .Where(p => p.PostId == id)
                            .ExecuteUpdate(setters => setters
                                .SetProperty(p => p.Activity, post.Activity)
                                .SetProperty(p => p.Description, post.Description)
                                .SetProperty(p => p.ExpiredTime, post.ExpiredTime)
                                .SetProperty(p => p.AppointmentTime, post.AppointmentTime)
                                .SetProperty(p => p.UpdatedAt, DateTime.UtcNow)
                                .SetProperty(p => p.Tags, _db.Tags.Where(t => post.Tags.Select( dto => dto.Name ).Contains(t.Name)).ToList()));
        if (row_affected == 0){
            return NotFound("Post is not found to delete.");
        }

        return RedirectToAction("Index");
    }

    [HttpPost("Post/Join/{id}")]
    public IActionResult Join(int id){
        var user = _db.Users.Find(1);   // Replace with actual current user logic
        var post = _db.Posts.Find(id);
        if (post == null){
            return NotFound("Post is not found.");
        }
        if (user == null){
            return NotFound("User is not found.");
        }
        var join = new Join{
            UserId = user.Id,  
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
        if (post.FavoritedBy.Any(u => u.Id == user.Id)){
            post.FavoritedBy.Remove(user);
        } else {
            post.FavoritedBy.Add(user);
        }
        _db.SaveChanges();
        return RedirectToAction("Index");
    }
    
}
