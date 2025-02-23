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
    public class UserController : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly UserManager<User> _userManager;
        private readonly CurrentUserService _currentUserService;

        public UserController(ApplicationDBContext db, UserManager<User> userManager, CurrentUserService currentUserService)
        {
            _db = db;
            _userManager = userManager;
            _currentUserService = currentUserService;
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var userId = await _currentUserService.GetCurrentUserId();
            if (userId is null) return null;
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == userId.ToString());
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Auth");

            return View(user);
        }
        

        [Authorize]
        [HttpGet("user/profile/edit")]
        public async Task<IActionResult> Edit()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Auth");

            var userDto = new UpdateUserDto
            {
                Username = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            return View(userDto);
        }

        [Authorize]
        [HttpPost("user/profile/edit")]
        public async Task<IActionResult> Edit(UpdateUserDto userEditor)
        {
            if (!ModelState.IsValid)
                return BadRequest("wtf");

            var user = await GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Check for username duplication
            var existingUser = await _userManager.FindByNameAsync(userEditor.Username);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                TempData["Error"] = "Username already exists";
                return View(userEditor);
            }

            // Update user fields
            user.UserName = userEditor.Username;
            user.FirstName = userEditor.FirstName;
            user.LastName = userEditor.LastName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Failed to update user information";
                return View(userEditor);
            }

            // Update password if provided
            if (!string.IsNullOrEmpty(userEditor.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, token, userEditor.Password);

                if (!resetResult.Succeeded)
                {
                    TempData["Error"] = "Password reset failed";
                    return View(userEditor);
                }
            }

            TempData["Success"] = "Profile updated successfully";
            return RedirectToAction("Profile");
        }

        [HttpGet("user/posts/status")]
        public async Task<IActionResult> UserPostStatus()
        {
            var userId = await _currentUserService.GetCurrentUserId();
            System.Console.WriteLine(userId);
            if (userId == null)
            {
                return NoContent();
            }

            var joinedPosts = await _db.Joins
                .Where(f => f.UserId == userId)
                .Select(f => new UserPostStatusDTO
                {
                    Activity = f.Post.Activity,
                    Owner = _db.Users.Where(j => j.Id == f.Post.OwnerId).Select(k => k.UserName).FirstOrDefault(),
                    AppointmentTime = f.Post.AppointmentTime,
                    Status = "Joined" // Explicitly setting status
                })
                .ToListAsync();

            var userForms = await _db.Forms
                .Where(f => f.UserId == userId)
                .Include(f => f.Post)
                .ThenInclude(p => p.Tags)
                .Include(f => f.Post.Questions)
                .ToListAsync();

            var pendingPosts = userForms
                .Where(f => f.Status == FormStatus.Pending && f.Post != null)
                .Select(f => new UserPostStatusDTO
                {
                    Activity = f.Post.Activity,
                    Owner = _db.Users.Where(j => j.Id == f.Post.OwnerId).Select(k => k.UserName).FirstOrDefault(),
                    AppointmentTime = f.Post.AppointmentTime,
                    Status = "Pending" // Setting status dynamically
                })
                .ToList();

            var rejectedPosts = userForms
                .Where(f => f.Status == FormStatus.Rejected && f.Post != null)
                .Select(f => new UserPostStatusDTO
                {
                    Activity = f.Post.Activity,
                    Owner = _db.Users.Where(j => j.Id == f.Post.OwnerId).Select(k => k.UserName).FirstOrDefault(),
                    AppointmentTime = f.Post.AppointmentTime,
                    Status = "Rejected" // Setting status dynamically
                })
                .ToList();

            // Ensure all lists contain the same type (List<UserPostStatusDTO>)
            var uniqueJoinedPosts = joinedPosts
                .Except(pendingPosts)
                .Except(rejectedPosts)
                .ToList();

            var allPosts = joinedPosts
                .Concat(pendingPosts)
                .Concat(rejectedPosts)
                .ToList();
            System.Console.WriteLine("Hi");

            var model = new UserPostStatusViewModel
            {
                Posts = allPosts
            };
            
            return Ok(model);

        }

    }
}
