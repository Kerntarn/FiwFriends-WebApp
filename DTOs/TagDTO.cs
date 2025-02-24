using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs;

public class TagDTO{
    [Required]
    public required string Name { get; set; }
}