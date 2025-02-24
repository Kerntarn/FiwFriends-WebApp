using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs;

public class QuestionDTO{
    [Required]
    public required string Content { get; set; }
}