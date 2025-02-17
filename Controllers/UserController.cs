// using Microsoft.AspNetCore.Mvc;
// using FiwFriends.Data;
// using FiwFriends.Models;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Authorization;

// namespace FiwFriends.Controllers;

// public class UserController : Controller
// {
//     private readonly ApplicationDBContext _db;
//     private readonly UserManager<IdentityUser> _userManager;
    
//     public UserController(ApplicationDBContext db, UserManager<IdentityUser> userManager)
//     {
//         _db = db;
//         _userManager = userManager;
//     }

//     // GET: Users
//     [HttpGet]
//     public async Task<IActionResult> Index()
//     {
//         var users = await _db.Users.ToListAsync();
//         return View(users);
//     }

//     // GET: User by ID
//     public async Task<IActionResult> GetUserById(string id)
//     {
//         var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
//         if (user == null)
//         {
//             return NotFound();
//         }
//         return View(user);
//     }

//     [Authorize]
//     [HttpGet("user/profile/edit")]
//     public async Task<IActionResult> Edit(string id)
//     {
//         var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
//         if (user == null)
//         {
//             return NotFound();
//         }
//         return View(user);
//     }

//     [Authorize]
//     [HttpPost]
//     [ValidateAntiForgeryToken]
//     public async Task<IActionResult> Edit(string id, User user)
//     {
//         if (id != user.Id)
//         {
//             return NotFound();
//         }

//         if (ModelState.IsValid)
//         {
//             try
//             {
//                 var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
//                 if (existingUser == null)
//                 {
//                     return NotFound();
//                 }

//                 // Update only the properties that are provided in the form
//                 existingUser.FirstName = user.FirstName;
//                 existingUser.LastName = user.LastName;
//                 existingUser.UserName = user.UserName;
//                 existingUser.Bio = user.Bio;

//                 // Only update the password if it's provided
//                 if (!string.IsNullOrEmpty(user.Password))
//                 {
//                     existingUser.Password = user.Password;
//                 }

//                 _db.Entry(existingUser).State = EntityState.Modified;
//                 await _db.SaveChangesAsync();
//                 return RedirectToAction("Index");
//             }
//             catch (DbUpdateConcurrencyException)
//             {
//                 if (!_db.Users.Any(u => u.Id == user.Id))
//                 {
//                     return NotFound();
//                 }
//                 else
//                 {
//                     throw;
//                 }
//             }
//         }

//         return View(user);
//     }

//     public async Task<IActionResult> Delete(int id)
//     {
//         var user = await _db.Users.FindAsync(id);
//         if (user == null)
//         {
//             return NotFound();
//         }
//         return View(user);
//     }

//     [HttpPost, ActionName("Delete")]
//     [ValidateAntiForgeryToken]
//     public async Task<IActionResult> DeleteConfirmed(int id)
//     {
//         var user = await _db.Users.FindAsync(id);
//         if (user == null)
//         {
//             return NotFound();
//         }

//         _db.Users.Remove(user);
//         await _db.SaveChangesAsync();
//         return RedirectToAction("Index");
//     }
// }
