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

    public IActionResult Index()    //IActionResult is for using Razor view
    {
        return View();
    }
}
