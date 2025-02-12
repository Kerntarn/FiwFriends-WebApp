using Microsoft.AspNetCore.Mvc;
using FiwFriends.Models;
using FiwFriends.Data;
using Microsoft.EntityFrameworkCore;
namespace FiwFriends.Controllers;

public class FormController : Controller{
    private readonly ApplicationDBContext _db;
    public FormController(ApplicationDBContext db){
        _db = db;
    }

    [HttpPost("Form/Submit")]
    public IActionResult Submit([FromBody] Form form){
        if (!ModelState.IsValid){
            return BadRequest(ModelState);
        }
        form.IsApproved = false;
        
        _db.Forms.Add(form);
        _db.SaveChanges();
        return Ok();
    }

    [HttpPost("Form/Approve/{PostId}/{UserId}")]
    public IActionResult Approve(int PostId, int UserId){
        var form = _db.Forms.Where(f => f.UserId == UserId && f.PostId == PostId)
                                    .Include(f => f.Post)
                                    .FirstOrDefault();
        if (form == null){
            return NotFound();
        }
        form.IsApproved = true;
        var join = new Join{
            UserId = form.UserId,
            PostId = form.PostId
        };
        _db.Joins.Add(join);
        _db.SaveChanges();
        return Ok();
    }
}