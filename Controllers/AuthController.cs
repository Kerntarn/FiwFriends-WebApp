using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FiwFriends.DTOs.Auth;
using FiwFriends.Data;
using FiwFriends.Models;
using System.Threading.Tasks;
using Azure.Identity;

namespace FiwFriends.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDBContext _db;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDBContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        [HttpGet]
        public IActionResult Register()
        {
            var response = new RegisterDto();
            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid) return View(registerDto);
            var user = new User { UserName = registerDto.Username, FirstName = registerDto.FirstName, LastName = registerDto.Lastname };
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(result.Errors);
        }
        
        [HttpGet]
        public IActionResult Login(){
            var response = new LoginDto();
            return View(response);
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await _signInManager.PasswordSignInAsync(loginDto.Username, loginDto.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            return View("LoginFailed");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}