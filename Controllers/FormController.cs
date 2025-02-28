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
    public async Task<IActionResult> Submit(FormDTO form)
    {

        Console.WriteLine($"Received PostId: {form.PostId}");
        Console.WriteLine($"Received Answers Count: {form.Answers?.Count ?? 0}");

        if (form.Answers == null || form.Answers.Count == 0)
        {
            Console.WriteLine("❌ ERROR: Answers is NULL or EMPTY");
        }

        foreach (var answer in form.Answers ?? new List<AnswerDTO>())
        {
            Console.WriteLine($"Answer: QuestionId={answer.QuestionId}, Content={answer.Content}");
        }


        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _currentUser.GetCurrentUser();
        if (user == null) return RedirectToAction("Login", "Auth");

        var newForm = new Form
        {
            UserId = user.Id,
            PostId = form.PostId,
            Status = FormStatus.Pending, // Default to Pending
            Answers = form.Answers.Select(a => new Answer
            {
                Content = a.Content,
                QuestionId = a.QuestionId
            }).ToList()
        };

        

        await _db.Forms.AddAsync(newForm);
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

        form.Status = FormStatus.Approved;
        var join = new Join {
            UserId = form.UserId,
            PostId = form.PostId
        };

        await _db.Joins.AddAsync(join);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Form approved"});
    }

    [HttpPost("/Form/Reject/{FormId}")]
    async public Task<IActionResult> Reject(int FormId)
    {
        System.Console.WriteLine(FormId);
        var form = await _db.Forms.Where(f => f.FormId == FormId)
                                    .Include(f => f.Post)
                                    .FirstOrDefaultAsync();
        if (form == null)
        {
            return NotFound("Form not found");
        }
        if (form.Post.Owner != await _currentUser.GetCurrentUser())
        {
            return Unauthorized("Not authorized");
        }

        form.Status = FormStatus.Rejected;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Form rejected", formId = FormId });
    }
}