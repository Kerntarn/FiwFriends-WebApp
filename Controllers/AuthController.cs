using Microsoft.AspNetCore.Mvc;

namespace FiwFriends.Controllers;
public class AuthController : Controller{
    public IActionResult Login(){
        return View();
    }
    public IActionResult Register(){
        return View();
    }
    public IActionResult Logout(){
        return View();
    }
}