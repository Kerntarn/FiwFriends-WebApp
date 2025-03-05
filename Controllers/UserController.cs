using Microsoft.AspNetCore.Mvc;
using FiwFriends.Data;
using FiwFriends.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using FiwFriends.Services;
using FiwFriends.DTOs;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace FiwFriends.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly UserManager<User> _userManager;
        private readonly CurrentUserService _currentUserService;
        private readonly MapperService _mapper;

        public UserController(ApplicationDBContext db, UserManager<User> userManager, CurrentUserService currentUserService, MapperService mapperService)
        {
            _db = db;
            _userManager = userManager;
            _currentUserService = currentUserService;
            _mapper = mapperService;
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var user = await _currentUserService.GetCurrentUser();
            if (user is null) return null;
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id.ToString());
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Auth");

            return View(user);
        }
        
        public async Task<IActionResult> people(string id){
            if (id is null) return RedirectToAction("Index", "Home");
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [Authorize]
        [HttpGet("user/edit")]
        public async Task<IActionResult> Edit()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Auth");

            var userDto = new UpdateUserDto
            {
                Username = user.UserName ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                Bio = user.Bio,
                Contact = user.Contact
            };

            return View();
        }

        [Authorize]
    [HttpPost("user/edit")]
    public async Task<IActionResult> Edit([FromBody] UpdateUserDto userEditor)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { error = "Invalid input data" });

        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized(new { error = "User not found" });

        var existingUser = await _userManager.FindByNameAsync(userEditor.Username);
        if (existingUser != null && existingUser.Id != user.Id)
        {
            return BadRequest(new { error = "Username already exists" });
        }

        user.UserName = userEditor.Username;
        user.FirstName = userEditor.FirstName;
        user.LastName = userEditor.LastName;
        user.Bio = userEditor.Bio;
        user.Contact = userEditor.Contact;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new { error = "Failed to update user information" });
        }

        if (!string.IsNullOrEmpty(userEditor.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, userEditor.Password);

            if (!resetResult.Succeeded)
            {
                return BadRequest(new { error = "Password reset failed" });
            }
        }

        return Ok(new { message = "Profile updated successfully" });
    }


        [HttpGet("Inbox")]
        public async Task<IActionResult> UserInboxStatus()
        {
            var user = await _currentUserService.GetCurrentUser();
            if (user == null)
            {
                return NoContent();
            }

            var joinedPosts = await _db.Joins
                .Where(f => f.User == user)
                .Select(f => new UserPostStatusViewModel
                {
                    Activity = f.Post.Activity,
                    PostID = f.Post.PostId.ToString(),
                    Owner = _db.Users
                        .Where(j => j.Id == f.Post.OwnerId)
                        .Select(k => k.UserName)
                        .FirstOrDefault() ?? "Unknown",
                        AppointmentTime = f.Post.AppointmentTime,
                        Status = "Joined"
                    })
                    .ToListAsync();

                var userForms = await _db.Forms
                    .Where(f => f.User == user)
                    .Include(f => f.Post)
                    .ThenInclude(p => p.Tags)
                    .Include(f => f.Post.Questions)
                    .ToListAsync();

                var pendingPosts = userForms
                    .Where(f => f.Status == FormStatus.Pending && f.Post != null)
                    .Select(f => new UserPostStatusViewModel
                    {
                        Activity = f.Post.Activity,
                        PostID = f.Post.PostId.ToString(),
                        Owner = _db.Users
                            .Where(j => j.Id == f.Post.OwnerId)
                            .Select(k => k.UserName)
                            .FirstOrDefault() ?? "Unknown",
                        AppointmentTime = f.Post.AppointmentTime,
                        Status = "Pending"
                    })
                    .ToList();

                var rejectedPosts = userForms
                    .Where(f => f.Status == FormStatus.Rejected && f.Post != null)
                    .Select(f => new UserPostStatusViewModel
                    {
                        Activity = f.Post.Activity,
                        PostID = f.Post.PostId.ToString(),
                        Owner = _db.Users
                            .Where(j => j.Id == f.Post.OwnerId)
                            .Select(k => k.UserName)
                            .FirstOrDefault() ?? "Unknown",
                        AppointmentTime = f.Post.AppointmentTime,
                        Status = "Rejected"
                    })
                    .ToList();

                var allPosts = joinedPosts
                    .Concat(pendingPosts)
                    .Concat(rejectedPosts)
                    .OrderBy(s => s.AppointmentTime)
                    .ToList();

                return View(allPosts);
            }

        [HttpGet("Pending")]
        public async Task<IActionResult> UserPendingStatus()
        {
            var user = await _currentUserService.GetCurrentUser();
            if (user == null)
            {
                return BadRequest(new { message = "There is no user" });
            }

            var userPostIds = await _db.Posts
                .Where(f => f.Owner == user)
                .Select(f => f.PostId)
                .ToListAsync();


            var userPostForms = await _db.Forms
                .Where(f => userPostIds.Contains(f.PostId) && f.Status == FormStatus.Pending)
                .Select(f => new UserPendingStatusViewModel
                {
                    Activity = f.Post.Activity,
                    FormId = f.FormId.ToString(),
                    UserId = _db.Users
                        .Where(u => u.Id == f.UserId)
                        .Select(u => u.Id)
                        .FirstOrDefault() ?? "Unknown",
                    Username = _db.Users
                        .Where(u => u.Id == f.UserId)
                        .Select(u => u.UserName)
                        .FirstOrDefault() ?? "Unknown",
                    PostId = f.PostId.ToString(),
                    Status = f.Status.ToString(),
                    QnAs = f.Answers
                        .Select(a => new QnA
                        {
                            Question = a.Question.Content, // Get the question content
                            Answer = a.Content // Get the answer content
                        })
                        .ToList()
                })
                .OrderBy(f => f.FormId)
                .ToListAsync();

            if (userPostIds.Any() != true)
            {
                return View(userPostForms);
            }

            return View(userPostForms);
        }

        [HttpGet("/MyPosts")]
        public async Task<IActionResult> MyPost(){
            User? user = await _currentUserService.GetCurrentUser();
            if( user == null ) return RedirectToAction("Login", "Auth");
            var posts = await _db.Users.Where(u => u.Id == user.Id).Include(u => u.OwnPosts)
                                        .SelectMany(u => u.OwnPosts)
                                        .Include(p => p.Owner)
                                        .Include(p => p.Participants).ThenInclude(j => j.User)
                                        .Include(p => p.Tags)
                                        .Include(p => p.FavoritedBy).ToListAsync();
            List<IndexPost> own_post = new List<IndexPost>();

            foreach (var post in posts){
                own_post.Add(await _mapper.MapAsync<Post, IndexPost>(post));
            }
            return View(own_post);
        }
    }
}
