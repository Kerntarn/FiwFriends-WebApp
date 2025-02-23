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

    [HttpPost("Form/Submit/")]
    public async Task<IActionResult> Submit([FromBody] FormDTO form)
    {
        Console.WriteLine($"Received PostId: {form.PostId}"); // Debugging Log
        if (form.PostId == 0)
        {
            return BadRequest("Invalid PostId.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = await _currentUser.GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized("User not logged in");
        }

        // Check if the provided PostId exists
        var postExists = await _db.Posts.AnyAsync(p => p.PostId == form.PostId);
        if (!postExists)
        {
            return BadRequest("The specified PostId does not exist.");
        }
        
        var currentPost = await _db.Posts.FindAsync(form.PostId);

        var newForm = new Form
        {
            UserId = userId,
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

        return Ok(new { message = "Form submitted successfully", formId = newForm.FormId });
    }


    [HttpPost("/Form/Approve/{FormId}")]
    async public Task<IActionResult> Approve(int FormId)
    {
        var form = await _db.Forms.Where(f => f.FormId == FormId)
                                    .Include(f => f.Post)
                                    .FirstOrDefaultAsync();
        System.Console.WriteLine("Form");
        if (form == null)
        {
            return NotFound("Form not found");
        }
        System.Console.WriteLine("Yooooo");
        if (form.Post.OwnerId != await _currentUser.GetCurrentUserId())
        {
            return Unauthorized("Not authorized");
        }

        form.Status = FormStatus.Approved;

        var join = new Join
        {
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
        if (form.Post.OwnerId != await _currentUser.GetCurrentUserId())
        {
            return Unauthorized("Not authorized");
        }

        form.Status = FormStatus.Rejected;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Form rejected", formId = FormId });
    }
}