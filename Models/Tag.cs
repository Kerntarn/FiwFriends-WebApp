namespace FiwFriends.Models;

public class Tag{
    public int TagId { get; set; }
    public string Name { get; set; }

    public ICollection<Post>? Posts { get; set; }
}