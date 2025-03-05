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
        Console.WriteLine($"Received Answers Count: {form.Answers?.Count ?? -1}");
        foreach (var answer in form.Answers ?? new List<AnswerDTO>())
        {
            Console.WriteLine($"Answer: QuestionId={answer.QuestionId}, Content={answer.Content}");
        }

        if (!ModelState.IsValid) return BadRequest(ModelState);

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
        var post = await _db.Posts.Where(p => p.PostId == form.PostId).Include(p => p.Questions).FirstOrDefaultAsync();
        if(post == null) return NotFound("Post not found");

        var IsExisted = await _db.Forms.AnyAsync(f => f.UserId == user.Id && f.PostId == form.PostId) || 
                        await _db.Joins.AnyAsync(j => j.UserId == user.Id && j.PostId == form.PostId);
        var IsOwned = user.Id == post.OwnerId;
        if (IsExisted || IsOwned) return BadRequest("You've already joined");

        if ( post.Questions.Count == 0 ) return RedirectToAction("Join", "Post", new { id = post.PostId});     //if there's no question on post, just join
        
        var newForm = new Form
        {
            UserId = user.Id,
            PostId = form.PostId,
            Status = FormStatus.Pending, // Default to Pending
            Answers = validAnswers
        };

        await _db.Forms.AddAsync(newForm);
        await _db.SaveChangesAsync();

        TempData["Message"] = "Form has been submit successfully!";
        return RedirectToAction("Index", "Post");
    }

    [HttpPost("Form/Approve/{FormId}")]
    async public Task<IActionResult> Approve(int FormId){                            //Approve post by PostId and UserId, only done by PostOwner
        var user = await _currentUser.GetCurrentUser();
        var query = await _db.Forms.Where(f => f.FormId == FormId && f.Status == FormStatus.Pending)
                                    .Select(f => new {
                                        form = f,
                                        IsGonnaFull = f.Post.Participants.Count + 1 >= f.Post.Limit,
                                        IsAllow = f.Post.OwnerId == user.Id,
                                        post = f.Post
                                    })
                                    .FirstOrDefaultAsync();
        if (query == null) return NotFound();
        var form = query.form;
        var IsGonnaFull = query.IsGonnaFull;
        var IsAllow = query.IsAllow;
        var post = query.post;

        if ( !IsAllow ) return Unauthorized();

        form.Status = FormStatus.Approved;
        var join = new Join {
            UserId = form.UserId,
            PostId = form.PostId
        };
        await _db.Joins.AddAsync(join);
        await _db.SaveChangesAsync();

        if ( IsGonnaFull ){
            post.ExpiredTime = DateTimeOffset.UtcNow;
            await _db.Forms
                    .Where(f => f.PostId == form.PostId && f.Status == FormStatus.Pending)
                    .ExecuteDeleteAsync();

        }
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
        if (form.Post.OwnerId != user?.Id) return Unauthorized();

        form.Status = FormStatus.Rejected;
        await _db.SaveChangesAsync();

        TempData["Message"] = "Form has been rejected successfully!";
        return RedirectToAction("UserPendingStatus", "User");
    }
}