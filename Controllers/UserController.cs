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
        var user = await _db.Users.FindAsync(id);
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
        var user = await _db.Users.FirstOrDefaultAsync(i => i.UserId == id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    // POST: Edit User
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [FromBody] User user)
    {
        if (id != user.UserId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _db.Entry(user).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
