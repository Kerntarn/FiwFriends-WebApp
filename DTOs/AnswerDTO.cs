using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs
{
    public class AnswerDTO
    {
        public required string Content { get; set; }
        public int QuestionId { get; set; }
    }
}