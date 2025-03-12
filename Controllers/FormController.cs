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
        if (!ModelState.IsValid) return RedirectToAction("Detail", "Post", new { id = form.PostId });

        var validQuestionIds = await _db.Questions.Select(q => q.QuestionId).ToHashSetAsync();
        var validAnswers = (form.Answers ?? new List<AnswerDTO>())
            .Where(a => validQuestionIds.Contains(a.QuestionId))  // Ensure QuestionId exists
            .Select(a => new Answer
            {
                Content = a.Content,
                QuestionId = a.QuestionId
            })
            .ToList();

        if (validAnswers.Count != form.Answers?.Count)
        {
            ModelState.AddModelError("Answers", "One or more Question IDs are invalid.");
            return BadRequest(ModelState);
        }

        var user = await _currentUser.GetCurrentUser();
        var q = await _db.Posts.Where(p => p.PostId == form.PostId)
                                    .Select(p => new {
                                        postId = p.PostId,
                                        answerRequired = p.Questions.Count > 0,
                                        isAllow = user.Id == p.OwnerId || 
                                                    _db.Forms.Any(f => f.UserId == user.Id && f.PostId == form.PostId) || 
                                                    _db.Joins.Any(j => j.UserId == user.Id && j.PostId == form.PostId),
                                    })
                                    .FirstOrDefaultAsync();
        if (q == null) return NotFound("Post not found");
        if (q.isAllow) return RedirectToAction("Detail", "Post", new { id = form.PostId });     //unable to joinwith this user
        if ( !q.answerRequired ) return RedirectToAction("Join", "Post", new { id = q.postId});     //if there's no question on post, just join

        await _db.Forms.AddAsync(new Form
        {
            UserId = user.Id,
            PostId = form.PostId,
            Status = FormStatus.Pending, // Default to Pending
            Answers = validAnswers
        });

        await _db.SaveChangesAsync();

        TempData["Message"] = "Form has been submit successfully!";
        return RedirectToAction("Index", "Post");
    }

    [HttpPost("Form/Approve/{FormId}")]
    async public Task<IActionResult> Approve(int FormId){                            //Approve post by PostId and UserId, only done by PostOwner
        var user = await _currentUser.GetCurrentUser();
        var q = await _db.Forms.Where(f => f.FormId == FormId && f.Status == FormStatus.Pending)
                                    .Select(f => new {
                                        form = f,
                                        post = f.Post,
                                        IsGonnaFull = f.Post.Participants.Count + 1 >= f.Post.Limit,
                                        IsAllow = f.Post.OwnerId == user.Id
                                    })
                                    .FirstOrDefaultAsync();
        if (q == null) return NotFound();
        if ( !q.IsAllow ) return Unauthorized();

        q.form.Status = FormStatus.Approved;
        await _db.Joins.AddAsync(new Join {
            UserId = q.form.UserId,
            PostId = q.form.PostId
        });

        await _db.SaveChangesAsync();

        if ( q.IsGonnaFull ){
            q.post.ExpiredTime = DateTimeOffset.UtcNow;
            await _db.Forms
                    .Where(f => f.PostId == q.form.PostId && f.Status == FormStatus.Pending)
                    .ExecuteDeleteAsync();

        }
        await _db.SaveChangesAsync();

        TempData["Message"] = "Form has been approved successfully!";
        return RedirectToAction("UserPendingStatus", "User");
    }

    [HttpPost("/Form/Reject/{FormId}")]
    async public Task<IActionResult> Reject(int FormId)
    {
        var user = await _currentUser.GetCurrentUser();
        var q = await _db.Forms.Where(f => f.FormId == FormId && f.Status == FormStatus.Pending)
                                    .Select(f => new {
                                        form = f,
                                        isOwner = f.Post.OwnerId == user.Id
                                    })
                                    .FirstOrDefaultAsync();
        if (q == null) return NotFound("Form not found");
        if (q.isOwner) return Unauthorized();

        q.form.Status = FormStatus.Rejected;
        await _db.SaveChangesAsync();

        TempData["Message"] = "Form has been rejected successfully!";
        return RedirectToAction("UserPendingStatus", "User");
    }
}