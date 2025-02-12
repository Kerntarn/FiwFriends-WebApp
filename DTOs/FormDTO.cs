using System.ComponentModel.DataAnnotations;
namespace FiwFriends.DTOs;

public class FormDTO{
    public int PostId { get; set; } 
    public List<AnswerDTO> Answers { get; set; } = new List<AnswerDTO>();
}