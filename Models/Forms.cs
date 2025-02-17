using System.ComponentModel.DataAnnotations;
namespace FiwFriends.Models;

public class Form{
    [Key]
    public int FormId { get; set; }
    public bool IsApproved { get; set; } = false;
    [Required]
    public int PostId { get; set; } 
    public Post? Post { get; set; } = null!;
    [Required]
    public string UserId { get; set; }
    public User? User { get; set ; } = null!;
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}