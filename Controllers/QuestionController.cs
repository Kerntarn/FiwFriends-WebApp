using Microsoft.AspNetCore.Mvc;
using FiwFriends.Models;
namespace FiwFriends.Controllers;

public class QuestionController : Controller{

    [HttpPost("Question")]
    public IActionResult Create([FromBody] Question question){
        if (!ModelState.IsValid){
            return BadRequest(ModelState);
        }
        return Ok("Show all questions.");
    }

    public IActionResult CreateMany([FromBody] List<Question> questions){
        if (!ModelState.IsValid){
            return BadRequest(ModelState);
        }
        return Ok("Show all questions.");
    }
}