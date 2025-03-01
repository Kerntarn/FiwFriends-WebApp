using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs;
public class FormDTO
{
    [Required(ErrorMessage = "Post ID is required.")]
    public int PostId { get; set; } 

    [Required(ErrorMessage = "At least one answer is required.")]
    public List<AnswerDTO> Answers { get; set; } = new List<AnswerDTO>();
}
