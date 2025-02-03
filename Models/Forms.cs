namespace FiwFriends.Models;

public class Form{
    public int FormId { get; set; }
    public bool IsApproved { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } //Who answer
    public int PostId { get; set; }
    public Post Post { get; set; }
}