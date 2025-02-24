using System.ComponentModel.DataAnnotations;
namespace FiwFriends.Models;

public enum FormStatus
{
    Pending,
    Approved,
    Rejected
}

public class Form
{
    [Key]
    public int FormId { get; set; }
    public FormStatus Status { get; set; } = FormStatus.Pending;  // Default status is pending
    [Required]
    public required int PostId { get; set; } 
    public Post Post { get; set; } = null!;
    [Required]
    public required string UserId { get; set; }
    public User User { get; set ; } = null!;
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}