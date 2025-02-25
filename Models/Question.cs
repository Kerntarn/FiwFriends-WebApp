using System.ComponentModel.DataAnnotations;

namespace FiwFriends.Models;

public class Question{
    [Key]
    public int QuestionId { get; set; }
    [Required]
    public required string Content { get; set; }
    [Required]
    public int PostId { get; set; }
    public Post? Post { get; set; } = null!;
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}

public class ViewQuestion{
    public required int QuestionId { get; set; }
    public required string Content { get; set; }
}