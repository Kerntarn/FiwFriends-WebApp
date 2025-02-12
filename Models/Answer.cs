using System.ComponentModel.DataAnnotations;

namespace FiwFriends.Models
{
    public class Answer
    {
        [Key]
        public int AnswerId { get; set; }
        [Required]
        public required string Content { get; set; }
        public int QuestionId { get; set; }
        public Question? Question { get; set; } = null!;
        public int FormId { get; set; }
        public Form? Form { get; set; } = null!;
    }
}