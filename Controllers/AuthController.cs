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

        [HttpPost("/auth/register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var existingUser = await _userManager.FindByNameAsync(registerDto.Username);
            if (existingUser != null)
            {
                return BadRequest(new { error = "Username already exists." });
            }

            var user = new User 
            {
                UserName = registerDto.Username, 
                FirstName = registerDto.FirstName, 
                LastName = registerDto.LastName
            };
            
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return Ok(new { message = "User registered successfully!" });
        }
        
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginDto());
        }

        [HttpPost("/auth/login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var existingUser = await _userManager.FindByNameAsync(loginDto.Username);
            if (existingUser == null)
            {
                return BadRequest(new { error = "Username not found." });
            }

            var result = await _signInManager.PasswordSignInAsync(loginDto.Username, loginDto.Password, false, false);
            if (!result.Succeeded)
            {
                return BadRequest(new { error = "Incorrect username or password." });
            }

            return Ok(new { message = "Login successful!" });
        }

        [HttpPost("/auth/logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully!" });
        }
    }
}
