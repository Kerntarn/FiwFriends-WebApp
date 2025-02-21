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

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterDto());
        }

        [HttpPost]
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
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(registerDto);
        }
        
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginDto());
        }

        [HttpPost("api/auth/login")]
        public async Task<IActionResult> LoginApi([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var user = await _userManager.FindByNameAsync(loginDto.Username);
            if (user == null)
                return Unauthorized(new { message = "Invalid username or password" });

            var result = await _signInManager.PasswordSignInAsync(loginDto.Username, loginDto.Password, false, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid username or password" });

            return Ok(new { message = "Login successful" });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}