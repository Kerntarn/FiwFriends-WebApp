using FiwFriends.Data;
using FiwFriends.Models;
using Microsoft.AspNetCore.Mvc;


namespace FiwFriends.Controllers;

public class TagController : Controller
{
    private readonly ApplicationDBContext _db;
    public TagController(ApplicationDBContext db)
    {
        _db = db;
    }
    public IActionResult Index()
    {
        IEnumerable<Tag> tags = _db.Tags;
        return View(tags);
    }
}