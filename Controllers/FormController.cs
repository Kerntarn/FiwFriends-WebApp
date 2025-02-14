using Microsoft.AspNetCore.Mvc;
using FiwFriends.Models;
using FiwFriends.Data;
using Microsoft.EntityFrameworkCore;
using FiwFriends.DTOs;
namespace FiwFriends.Controllers;

public class FormController : Controller{
    private readonly ApplicationDBContext _db;
    public FormController(ApplicationDBContext db){
        _db = db;
    }

    [HttpPost("Form/Submit")]
    public IActionResult Submit(FormDTO form){
        if (!ModelState.IsValid){
            return BadRequest(ModelState);
        }
        
        _db.Forms.Add(new Form{
            UserId = 1,         //get current user
            PostId = form.PostId,
            Answers = form.Answers.Select(a => new Answer{
                Content = a.Content,
                QuestionId = a.QuestionId
            }).ToList()
        });
        _db.SaveChanges();
        return RedirectToAction("Index", "Post");
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
        return RedirectToAction("Index");
    }
}