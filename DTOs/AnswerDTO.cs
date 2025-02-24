using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs
{
    public class AnswerDTO
    {
        [Required]
        public required string Content { get; set; }
        [Required]
        public int QuestionId { get; set; }
    }
}