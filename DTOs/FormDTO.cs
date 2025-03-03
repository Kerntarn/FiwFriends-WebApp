using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs;
public class FormDTO
{
    [Required(ErrorMessage = "Post ID is required.")]
    public int PostId { get; set; } 

    [Required(ErrorMessage = "Answers is always required, even it's empty list.")]
    public List<AnswerDTO> Answers { get; set; } = new List<AnswerDTO>();               //actually this should be required but for now, it works.
    
}
