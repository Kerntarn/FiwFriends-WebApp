using System.ComponentModel.DataAnnotations;

namespace FiwFriends.Models
{
    public class Answer
    {
        [Key]
        public int AnswerId { get; set; }
        [Required]
        public required string Content { get; set; }
        [Required]
        public required int QuestionId { get; set; }
        public Question Question { get; set; } = null!;
        [Required]
        public int FormId { get; set; }
        public Form Form { get; set; } = null!;
    }
}