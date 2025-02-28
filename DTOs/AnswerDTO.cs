using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs
{
    public class AnswerDTO
    {
        [Required(ErrorMessage = "Content is required")]
        public required string Content { get; set; }
        [Required]
        public int QuestionId { get; set; }
    }
}