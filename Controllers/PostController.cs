using Microsoft.AspNetCore.Mvc;
using FiwFriends.Data;
using FiwFriends.Models;

namespace FiwFriends.Controllers;

public class PostController : Controller
{
    private readonly ApplicationDBContext _db;
    public PostController(ApplicationDBContext db){
        _db = db;
    }


    //GET
    public IEnumerable<Post> Index(){
        IEnumerable<Post> allPost = _db.Posts;
        return allPost;
    }

    public IActionResult Create(){
        return View();
    }
    //POST
    [HttpPost]
    public IActionResult Create(Post post){
        
        if (!ModelState.IsValid){
            return BadRequest("Invalid product data.");
        }

        Console.WriteLine(post);
        _db.Add(post);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    //DELETE


    //PUT
    public string Info(){
        return "Some Info";
    }
}
