using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "Firstname is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Firstname must be between 1 and 50 characters long")]
    public string FirstName { get; set; }

    [Required(ErrorMessage ="Lastname is required")]
    [StringLength(50,MinimumLength = 1,ErrorMessage = "Lastname must be between 1 and 50 characters long")]
    public string LastName {get; set;}

    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters long")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 50 characters long")]
    public string Password { get; set; }
}