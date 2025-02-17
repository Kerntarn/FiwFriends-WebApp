using Microsoft.AspNetCore.Mvc;
using FiwFriends.Models;
using FiwFriends.Data;
namespace FiwFriends.Controllers;

public class QuestionController : Controller{
    private readonly ApplicationDBContext _db;
    public QuestionController(ApplicationDBContext db){
        _db = db;
    }

    [HttpPost("Questions")]
    public IActionResult CreateMany(List<Question> questions, int postId){
        if (questions == null || !questions.Any()){
            return BadRequest("Question list cannot be empty.");
        }
        if (!ModelState.IsValid){
            return BadRequest(ModelState);
        }
        Console.WriteLine("Kuay");
        questions.ForEach(q => q.PostId = postId);
        _db.Questions.AddRange(questions);
        _db.SaveChanges();
        return RedirectToAction("Detail", "Post", postId);
    }
}