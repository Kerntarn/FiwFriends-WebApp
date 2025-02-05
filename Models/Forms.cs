using System.ComponentModel.DataAnnotations;
namespace FiwFriends.Models;

public class Form{
    [Key]
    public int Id { get; set; }
    public int PostId { get; set; }
    public Post Post { get; set; }

    // public ICollection<User> PendingUsers { get; set; }

    // public ICollection<Question> Questions { get; set; }
}