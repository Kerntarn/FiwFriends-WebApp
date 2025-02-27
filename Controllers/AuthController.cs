using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FiwFriends.DTOs.Auth;
using FiwFriends.Models;
using System.Threading.Tasks;

namespace FiwFriends.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet("/Register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("/Register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid) return View(registerDto);

            var existingUser = await _userManager.FindByNameAsync(registerDto.Username);
            if (existingUser != null)
            {
                 ModelState.AddModelError("Username", "Username already exists.");
                return View(registerDto);
            }

            var user = new User 
            {
                UserName = registerDto.Username, 
                FirstName = registerDto.FirstName, 
                LastName = registerDto.LastName
            };
            
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if(result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Post");    
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(registerDto);
        }
        
        [HttpGet("Login")]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl ?? Url.Content("~/");
            return View();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto loginDto, string? returnUrl = null)
        {
            if (!ModelState.IsValid) 
                return View(loginDto);

            var existingUser = await _userManager.FindByNameAsync(loginDto.Username);
            if (existingUser == null)
            {
                ModelState.AddModelError("Username", "Username not found.");
                return View(loginDto);
            }

            var result = await _signInManager.PasswordSignInAsync(loginDto.Username, loginDto.Password, false, false);
            
            if (result.Succeeded)
            {
                if(Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                return RedirectToAction("Index", "Post");
            }

            ModelState.AddModelError("Password", "Incorrect password.");
            return View(loginDto);
        }

        [HttpPost("/Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }
    }
}