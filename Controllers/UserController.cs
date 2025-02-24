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
                return View(userEditor);

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
    }
}
