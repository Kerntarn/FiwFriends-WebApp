using Microsoft.AspNetCore.Mvc;
using FiwFriends.Models;
using FiwFriends.Data;
using Microsoft.EntityFrameworkCore;
using FiwFriends.DTOs;
using FiwFriends.Services;
namespace FiwFriends.Controllers;

public class FormController : Controller{
    private readonly ApplicationDBContext _db;
    private readonly CurrentUserService _currentUser;
    public FormController(ApplicationDBContext db, CurrentUserService currentUser){
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet("Form/{PostId}")]
    public IActionResult Index(int PostId){
        IEnumerable<Form> forms = _db.Forms
                        .Where(f => f.PostId == PostId)
                        .Include(f => f.Answers);
        return View(forms);
    }

    [HttpPost("Form/Submit")]
    async public Task<IActionResult> Submit(FormDTO form){
        if (!ModelState.IsValid){
            return BadRequest(ModelState);
        }
        var userId = await _currentUser.GetCurrentUserId();
        if (userId == null){
            return RedirectToAction("Login", "Auth");
        }
        await _db.Forms.AddAsync(new Form{
            UserId = userId,         //get current user
            PostId = form.PostId,
            Answers = form.Answers.Select(a => new Answer{
                Content = a.Content,
                QuestionId = a.QuestionId
            }).ToList()
        });
        await _db.SaveChangesAsync();
        return RedirectToAction("Post", "Detail", new { id = form.PostId });
    }

    [HttpPost("Form/Approve/{FormId}")]
    async public Task<IActionResult> Approve(int FormId){
        var form = await _db.Forms.Where(f => f.FormId == FormId)
                                    .Include(f => f.Post)
                                    .FirstOrDefaultAsync();
        if (form == null){
            return NotFound();
        }
        if (form.Post.OwnerId != _currentUser.GetCurrentUserId().Result){
            return Unauthorized();
        }
        form.IsApproved = true;
        var join = new Join{
            UserId = form.UserId,
            PostId = form.PostId
        };
        await _db.Joins.AddAsync(join);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index", new { id = form.PostId });
    }
}