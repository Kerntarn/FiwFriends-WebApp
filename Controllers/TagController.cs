using FiwFriends.Data;
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
        var tags = _db.Tags;
        return Ok(tags);
    }
}