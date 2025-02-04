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
    public IEnumerable<Post> Index(){
        IEnumerable<Post> allPost = _db.Posts;
        return allPost;
    }

    //GET Create page
    public IActionResult Create(){
        return View();
    }
    //POST Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([FromBody] Post post){ //Delete [FromBody] if need to send request from View.
        
        if (!ModelState.IsValid){
            return BadRequest("Invalid product data.");
        }
        _db.Add(post);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }
    //GET Delete Page, maybe neccessary
    public IActionResult Delete(){
        return View();
    }
    //DELETE Post
    [HttpDelete]
    public IActionResult Delete(int id){
        var post = _db.Posts.Find(id);
        if (post == null){
            return NotFound();
        }
        _db.Posts.Remove(post);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    //PUT Update Post
    [HttpPut]
    public IActionResult Edit(int id,[FromBody] Post post){ //Delete [FromBody] if need to send request from View.
        Console.WriteLine(post.ToJson());
        int row_affected = _db.Posts
                            .Where(p => p.Id == id)
                            .ExecuteUpdate(setters => setters
                                .SetProperty(p => p.Activity, post.Activity)
                                .SetProperty(p => p.Description, post.Description)
                                .SetProperty(p => p.ExpiredTime, post.ExpiredTime)
                                .SetProperty(p => p.AppointmentTime, post.AppointmentTime));
        if (row_affected == 0){
            return NotFound("Post is not found to delete.");
        }

        return RedirectToAction("Index");
    }
}
