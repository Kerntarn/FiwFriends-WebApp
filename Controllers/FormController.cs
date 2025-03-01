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

    [HttpPost("Form/Submit")]
    public async Task<IActionResult> Submit(FormDTO form)
    {
        Console.WriteLine($"Received PostId: {form.PostId}");
        Console.WriteLine($"Received Answers Count: {form.Answers?.Count ?? -1}");

        foreach (var answer in form.Answers ?? new List<AnswerDTO>())
        {
            Console.WriteLine($"Answer: QuestionId={answer.QuestionId}, Content={answer.Content}");
        }

        if (!ModelState.IsValid) return BadRequest(ModelState);
        var user = await _currentUser.GetCurrentUser();
        var post = await _db.Posts.Where(p => p.PostId == form.PostId).Include(p => p.Questions).FirstOrDefaultAsync();
        if(post == null) return NotFound("Post not found");

        var IsExisted = await _db.Forms.AnyAsync(f => f.UserId == user.Id && f.PostId == form.PostId);
        var IsOwned = user.Id == post.OwnerId;
        if (IsExisted || IsOwned) return BadRequest("You can not submit this post");

        if ( post.Questions.Count == 0 ) return RedirectToAction("Join", "Post");   
              //if there's no question on post, just join
        var newForm = new Form
        {
            UserId = user.Id,
            PostId = form.PostId,
            Status = FormStatus.Pending, // Default to Pending
            Answers = (form.Answers ?? new List<AnswerDTO>()).Select(a => new Answer        //warning preventing
            {
                Content = a.Content,
                QuestionId = a.QuestionId                   //if question doesn't exist?
            }).ToList()
        };

        await _db.Forms.AddAsync(newForm);
        await _db.SaveChangesAsync();

        TempData["Message"] = "Form has been submit successfully!";
        return RedirectToAction("Index", "Post");
    }

    [HttpPost("Form/Approve/{FormId}")]
    async public Task<IActionResult> Approve(int FormId){                            //Approve post by PostId and UserId, only done by PostOwner
        var form = await _db.Forms.Where(f => f.FormId == FormId && f.Status == FormStatus.Pending)
                                    .Include(f => f.Post)
                                    .FirstOrDefaultAsync();
        if (form == null) return NotFound();

        var user = await _currentUser.GetCurrentUser();
        if (form.Post.OwnerId != user.Id) return Unauthorized();

        form.Status = FormStatus.Approved;
        var join = new Join {
            UserId = form.UserId,
            PostId = form.PostId
        };

        await _db.Joins.AddAsync(join);
        await _db.SaveChangesAsync();

        TempData["Message"] = "Form has been approved successfully!";
        return RedirectToAction("UserPendingStatus", "User");
    }

    [HttpPost("/Form/Reject/{FormId}")]
    async public Task<IActionResult> Reject(int FormId)
    {
        var form = await _db.Forms.Where(f => f.FormId == FormId && f.Status == FormStatus.Pending)
                                    .Include(f => f.Post)
                                    .FirstOrDefaultAsync();
        if (form == null) return NotFound("Form not found");

        var user = await _currentUser.GetCurrentUser();
        if (form.Post.OwnerId != user.Id) return Unauthorized();

        form.Status = FormStatus.Rejected;
        await _db.SaveChangesAsync();

        TempData["Message"] = "Form has been rejected successfully!";
        return RedirectToAction("UserPendingStatus", "User");
    }
}