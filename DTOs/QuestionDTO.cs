using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs;

public class QuestionDTO{
    [Range(0, 1000, ErrorMessage = "Content must be less than 1000 characters.")]
    public string? Content { get; set; }
}