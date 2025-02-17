using Microsoft.AspNetCore.Mvc;
using FiwFriends.Data;
using FiwFriends.Models;
using Microsoft.EntityFrameworkCore;

namespace FiwFriends.Controllers;

public class UserController : Controller
{
    private readonly ApplicationDBContext _db;
    
    public UserController(ApplicationDBContext db)
    {
        _db = db;
    }

    // GET: Users
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await _db.Users.ToListAsync();
        return View(users);
    }

    // GET: User by ID
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    // GET: Create User Form
    public IActionResult Create()
    {
        return View();
    }

    // POST: Create User
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User user)
    {
        if (ModelState.IsValid)
        {
            _db.Add(user);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        return View(user);
    }

    // GET: Edit User Form
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, User user)
    {
        if (id != user.UserId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.UserId == id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                // Update only the properties that are provided in the form
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.UserName = user.UserName;
                existingUser.Bio = user.Bio;

                // Only update the password if it's provided
                if (!string.IsNullOrEmpty(user.Password))
                {
                    existingUser.Password = user.Password;
                }

                _db.Entry(existingUser).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_db.Users.Any(u => u.UserId == user.UserId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // If we got this far, something failed; redisplay the form
        return View(user);
    }
    // GET: Delete User Confirmation
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    // POST: Delete User
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
