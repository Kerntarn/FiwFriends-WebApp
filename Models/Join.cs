namespace FiwFriends.Models;
public class Join
{
    public string UserId { get; set; }  
    public int PostId { get; set; }
    
    public User User { get; set; }
    public Post Post { get; set; }
}