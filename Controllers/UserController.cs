using Microsoft.AspNetCore.Mvc;
using FiwFriends.Data;
using FiwFriends.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using FiwFriends.Services;
using FiwFriends.DTOs;

namespace FiwFriends.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly UserManager<User> _userManager;
        private readonly CurrentUserService _currentUserService;
        private readonly MapperService _mapper;
        private readonly UpdateFormStatusService _uFormStatus;

        public UserController(ApplicationDBContext db, UserManager<User> userManager, CurrentUserService currentUserService, MapperService mapperService, UpdateFormStatusService updateFormStatusService)
        {
            _db = db;
            _userManager = userManager;
            _currentUserService = currentUserService;
            _mapper = mapperService;
            _uFormStatus = updateFormStatusService;
        }
        
        [HttpGet("User/Edit")]
        public async Task<IActionResult> Edit()
        {
            var user = await _currentUserService.GetCurrentUser();

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

        [HttpPost("User/Edit")]
        public async Task<IActionResult> Edit([FromForm] UpdateUserDto userEditor)
        {
            if (userEditor == null)
            {
                return BadRequest("No data provided.");
            }

            var user = await _currentUserService.GetCurrentUser();
    
            user.FirstName = string.IsNullOrEmpty(userEditor.FirstName) ? user.FirstName : userEditor.FirstName;
            user.LastName = string.IsNullOrEmpty(userEditor.LastName) ? user.LastName : userEditor.LastName;
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
                if (string.IsNullOrEmpty(userEditor.OldPassword))
                {
                    return BadRequest(new { error = "Old Password is required to edit your profile" });
                }
                // Ensure both new password and confirm new password are provided
                if (string.IsNullOrEmpty(userEditor.ConfirmNewPassword))
                {
                    return BadRequest(new { error = "Confirm New password is required to edit your profile" });
                }

                if (userEditor.NewPassword != userEditor.ConfirmNewPassword)
                {
                    return BadRequest(new { error = "New password and Confirm new password do not match" });
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
            else if (!string.IsNullOrEmpty(userEditor.ConfirmNewPassword))
            {
                // If NewPassword is empty but ConfirmNewPassword is not, return an error
                return BadRequest(new { error = "New Password is required to confirm your new password" });
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
            await _uFormStatus.Update();
            var user = await _currentUserService.GetCurrentUser();           
            var allPosts = await _db.Forms
                                    .Where(f => f.UserId == user.Id && f.Post != null)
                                    .Select(f => new UserPostStatusViewModel{
                                        Activity = f.Post.Activity,
                                        PostID = f.Post.PostId.ToString(),
                                        Owner = f.Post.Owner.UserName ?? "",
                                        AppointmentTime = f.Post.AppointmentTime,
                                        Status = f.Status == FormStatus.Approved ? "Joined" : f.Status.ToString()
                                    })
                                    .OrderBy(f => f.AppointmentTime)
                                    .ToListAsync();

            return View(allPosts);
        }

        [HttpGet("Pending")]
        public async Task<IActionResult> UserPendingStatus()
        {
            await _uFormStatus.Update();
            var user = await _currentUserService.GetCurrentUser();

            var userPostForms = await _db.Forms
                                .Where(f => f.Post.OwnerId == user.Id && f.Status == FormStatus.Pending)
                                .Select(f => new UserPendingStatusViewModel{
                                    Activity = f.Post.Activity,
                                    FormId = f.FormId.ToString(),
                                    UserId = f.UserId,
                                    Username = f.User.UserName ?? "",
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

            return View(userPostForms);
        }

        [HttpGet("/MyPosts")]
        public async Task<IActionResult> MyPost(){
            User user = await _currentUserService.GetCurrentUser();
            var postCondition = _db.Posts.Where(p => p.Owner == user);   
            var own_post = await _mapper.MapAsync<IQueryable<Post>, IEnumerable<IndexPost>>(postCondition);
            return View(own_post);
        }
    }
}
