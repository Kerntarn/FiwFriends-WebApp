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
    async public Task<IActionResult> Submit([FromBody] FormDTO form){
        if (!ModelState.IsValid){
            return BadRequest(ModelState);
        }
        var user = await _currentUser.GetCurrentUser();
        if (user == null){
            return RedirectToAction("Login", "Auth");
        }
        await _db.Forms.AddAsync(new Form{
            UserId = user.Id,         //get current user
            PostId = form.PostId,
            Answers = form.Answers.Select(a => new Answer{
                Content = a.Content,
                QuestionId = a.QuestionId
            }).ToList()
        });
        await _db.SaveChangesAsync();
        return RedirectToAction("Detail", "Post", new { id = form.PostId });
    }

    [HttpPost("Form/Approve/{PostId}/{UserId}")]
    async public Task<IActionResult> Approve(int PostId, string UserId){
        var form = await _db.Forms.Where(f => f.PostId == PostId && f.UserId == UserId)
                                    .Include(f => f.Post)
                                    .FirstOrDefaultAsync();
        if (form == null){
            return NotFound();
        }
        var user = await _currentUser.GetCurrentUser();
        if (user == null){
            return RedirectToAction("Login", "Auth");
        }
        if (form.Post.OwnerId != user.Id){
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