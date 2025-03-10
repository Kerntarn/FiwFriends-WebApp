using FiwFriends.Data;
using Microsoft.EntityFrameworkCore;
using FiwFriends.Models;

namespace FiwFriends.Services;

public class UpdateFormStatusService{
    private readonly ApplicationDBContext _db;
    private readonly CurrentUserService _currentUser;
    public UpdateFormStatusService(ApplicationDBContext db, CurrentUserService currentUser){
        _db = db;
        _currentUser = currentUser;
    }

    async public Task Update(){
        var user = await _currentUser.GetCurrentUser();
        var forms = await _db.Forms.Where(f => (f.UserId == user.Id || f.Post.OwnerId == user.Id) && f.Status == FormStatus.Pending && f.Post.ExpiredTime < DateTimeOffset.UtcNow).ExecuteDeleteAsync();
        await _db.SaveChangesAsync();
    }
}