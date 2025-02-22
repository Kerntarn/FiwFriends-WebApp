using System.ComponentModel.DataAnnotations;
namespace FiwFriends.Models;

public class Form{
    [Key]
    public int FormId { get; set; }
    [Required]
    public bool IsApproved { get; set; } = false;
    [Required]
    public required int PostId { get; set; } 
    public Post Post { get; set; } = null!;
    [Required]
    public required string UserId { get; set; }
    public User User { get; set ; } = null!;
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}