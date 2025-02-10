using System.ComponentModel.DataAnnotations;
namespace FiwFriends.Models;

public class Form{
    [Key]
    public int FormId { get; set; }
    public bool IsApproved { get; set; }
    public int PostId { get; set; } 
    public Post? Post { get; set; }
    public int UserId { get; set; }
    public User? User { get; set ;}
    public ICollection<Answer>? Answers { get; set; }
}