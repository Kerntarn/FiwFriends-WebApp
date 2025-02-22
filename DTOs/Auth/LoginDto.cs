using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs.Auth;

public class LoginDto
    {
    [Required(ErrorMessage = "Username or username is required")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}