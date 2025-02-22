namespace FiwFriends.Models;
public class Join
{
    public required string UserId { get; set; }  
    public required int PostId { get; set; }
    
    public User User { get; set; } = null!;
    public Post Post { get; set; } = null!;
}