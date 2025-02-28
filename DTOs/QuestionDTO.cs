using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs;

public class QuestionDTO{
    [Required(ErrorMessage = "Content is required")]
    public required string Content { get; set; }
}