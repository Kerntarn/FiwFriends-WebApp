using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using FiwFriends.Models;

namespace FiwFriends.Services;

public class CurrentUserService 
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<User> _userManager;
    public CurrentUserService(IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public async Task<User?> GetCurrentUser()
    {
        var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User!);
        return currentUser;
    }
}