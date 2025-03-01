using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs;

public class TagDTO{
    [Required (ErrorMessage = "Name is required")]
    public required string Name { get; set; }
}