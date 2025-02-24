using System.ComponentModel.DataAnnotations;
namespace FiwFriends.DTOs;

public class FormDTO{
    [Required]
    public int PostId { get; set; } 
    [Required]
    public List<AnswerDTO> Answers { get; set; } = new List<AnswerDTO>();
}