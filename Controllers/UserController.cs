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

            // Check if ProfilePic exists and convert to Base64 if not null
            if (user.ProfilePic != null)
            {
                var base64Pic = Convert.ToBase64String(user.ProfilePic);
                ViewBag.ImageData = string.Format("data:image/gif;base64,{0}", base64Pic);
            }
            else
            {
                ViewBag.ImageData = null;
            }

            return View(user);
        }

        public async Task<IActionResult> People(string id)
        {
            if (id == null) return RedirectToAction("Index", "Home");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if ProfilePic exists and convert to Base64 if not null
            if (user.ProfilePic != null)
            {
                var base64Pic = Convert.ToBase64String(user.ProfilePic);
                ViewBag.ImageData = string.Format("data:image/gif;base64,{0}", base64Pic);
            }
            else
            {
                ViewBag.ImageData = null;
            }

            return View(user);
        }

        [Authorize]
        [HttpGet("User/Edit")]
        public async Task<IActionResult> Edit()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Check if ProfilePic exists and convert to Base64 if not null
            if (user.ProfilePic != null)
            {
                var base64Pic = Convert.ToBase64String(user.ProfilePic);
                ViewBag.ImageData = string.Format("data:image/gif;base64,{0}", base64Pic);
            }
            else
            {
                ViewBag.ImageData = null;
            }

            return View(user);
        }

        [Authorize]
        [HttpPost("User/Edit")]
        public async Task<IActionResult> Edit([FromForm] UpdateUserDto userEditor)
        {
            if (userEditor == null)
            {
                return BadRequest("No data provided.");
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(new { error = "User not found" });


            if(!string.IsNullOrEmpty(userEditor.Username))
            {
                var existingUser = await _userManager.FindByNameAsync(userEditor.Username);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    return BadRequest(new { error = "Username already exists" });
                }
                user.UserName = userEditor.Username;
            }
    
            user.FirstName = string.IsNullOrEmpty(userEditor.FirstName) ? user.FirstName : userEditor.FirstName;
            user.LastName = string.IsNullOrEmpty(userEditor.LastName) ? user.LastName : userEditor.LastName;
            user.Bio = string.IsNullOrEmpty(userEditor.Bio) ? user.Bio : userEditor.Bio; 
            user.Contact = string.IsNullOrEmpty(userEditor.Contact) ? user.Contact : userEditor.Contact; 
            
            // Profile picture upload
            if (userEditor.ProfilePic != null && userEditor.ProfilePic.Length > 0)
            {
                // Check if the file size is greater than 2MB
                if (userEditor.ProfilePic.Length > 2097152)
                {
                    return BadRequest(new { error = "Profile picture size should not exceed 2MB" });
                }
                // Check if the file is an image
                var allowExtensions = new []{ ".jpg", ".jpeg", ".png", ".gif"};
                var extension = Path.GetExtension(userEditor.ProfilePic.FileName).ToLower();
                // Check if the file extension is allowed
                if (!allowExtensions.Contains(extension))
                {
                    return BadRequest(new { error = "Profile picture must be an image" });
                }
                
                using (var stream = new MemoryStream())
                {
                    // Copy the file to the memory stream
                    await userEditor.ProfilePic.CopyToAsync(stream);
                    // Set the profile picture to the byte array of the memory stream
                    user.ProfilePic = stream.ToArray();
                }
            }

            if (!string.IsNullOrEmpty(userEditor.NewPassword))
            {
                if (string.IsNullOrEmpty(userEditor.OldPassword)){
                    return BadRequest(new {error = "Old Password is required to edit your profile" });
                }
                if (string.IsNullOrEmpty(userEditor.ConfirmNewPassword)){
                    return BadRequest(new {error = "Confirm New password is required to edit your profile" });
                }
                var passwordcheck = await _userManager.CheckPasswordAsync(user, userEditor.OldPassword);
                if (!passwordcheck)
                {
                    return BadRequest(new { error = "Password is incorrect" });
                }
                var ChangepasswordResult = await _userManager.ChangePasswordAsync(user, userEditor.OldPassword, userEditor.NewPassword);
                if (!ChangepasswordResult.Succeeded)
                {
                    return BadRequest(new { error = "Password reset failed" });
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { error = "Failed to update user information" });
            }
            await _db.SaveChangesAsync();
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
                        AppointmentTime = f.Post.AppointmentTime.ToOffset(new TimeSpan(7,0,0)),
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
                        AppointmentTime = f.Post.AppointmentTime.ToOffset(new TimeSpan(7,0,0)),
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
                        AppointmentTime = f.Post.AppointmentTime.ToOffset(new TimeSpan(7,0,0)),
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
            var postCondition = _db.Posts.Where(p => p.Owner == user);   
            var own_post = await _mapper.MapAsync<IQueryable<Post>, IEnumerable<IndexPost>>(postCondition);
            return View(own_post);
        }
    }
}
