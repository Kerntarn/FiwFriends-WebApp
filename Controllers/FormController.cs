using Microsoft.AspNetCore.Mvc;
using FiwFriends.Models;
using FiwFriends.Data;
using Microsoft.EntityFrameworkCore;
using FiwFriends.DTOs;
using FiwFriends.Services;
using Microsoft.AspNetCore.Authorization;
namespace FiwFriends.Controllers;

[Authorize]
public class FormController : Controller{
    private readonly ApplicationDBContext _db;
    private readonly CurrentUserService _currentUser;
    public FormController(ApplicationDBContext db, CurrentUserService currentUser){
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet("Form/{PostId}")]
    async public Task<IActionResult> Index(int PostId){                             //Get all form of post and only for PostOwner
        var user = await _currentUser.GetCurrentUser();  
        var post = await _db.Posts.FindAsync(PostId);
        if(post == null) return NotFound("Post not found");
        if(post.OwnerId != user?.Id) return Unauthorized("You don't have permission");                
        IEnumerable<Form> forms = _db.Forms
                        .Where(f => f.PostId == PostId)
                        .Include(f => f.Answers).ThenInclude(a => a.Question);
        return View(forms);
    }

    [HttpPost("Form/Submit")]
    async public Task<IActionResult> Submit(FormDTO form){                          //Submit FormDTO
        if (!ModelState.IsValid) return BadRequest(ModelState); 
        
        var user = await _currentUser.GetCurrentUser();
        if (user == null) return RedirectToAction("Login", "Auth");
        
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
    async public Task<IActionResult> Approve(int PostId, string UserId){                            //Approve post by PostId and UserId, only done by PostOwner
        var form = await _db.Forms.Where(f => f.PostId == PostId && f.UserId == UserId)
                                    .Include(f => f.Post)
                                    .FirstOrDefaultAsync();
        if (form == null) return NotFound();

        var user = await _currentUser.GetCurrentUser();
        if (user == null) return RedirectToAction("Login", "Auth");
        
        if (form.Post.OwnerId != user.Id) return Unauthorized();

        form.IsApproved = true;
        var join = new Join {
            UserId = form.UserId,
            PostId = form.PostId
        };

        await _db.Joins.AddAsync(join);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index", new { id = form.PostId });
    }
}