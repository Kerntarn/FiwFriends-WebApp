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
        [HttpGet("user/profile/edit")]
        public async Task<IActionResult> Edit()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Auth");

            var userDto = new UpdateUserDto
            {
                Username = user.UserName ?? "",
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