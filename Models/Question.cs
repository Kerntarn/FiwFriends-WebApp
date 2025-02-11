using System.ComponentModel.DataAnnotations;

namespace FiwFriends.Models;

public class Question{
    public int QuestionId { get; set; }
    [Required]
    public required string Content { get; set; }
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}