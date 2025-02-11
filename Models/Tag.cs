using System.ComponentModel.DataAnnotations;

namespace FiwFriends.Models;

public class Tag{
    public int TagId { get; set; }
    [Required]
    public required string Name { get; set; }

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}