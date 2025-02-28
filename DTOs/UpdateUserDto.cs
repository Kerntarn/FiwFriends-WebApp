using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs
{
    public class UpdateUserDto
    {
        [Required (ErrorMessage = "Username is required")]
        public required string Username { get; set; }
        [Required (ErrorMessage = "FirstName is required")]
        public required string FirstName { get; set; }
        [Required (ErrorMessage = "FirstName is required")]
        public required string LastName { get; set; }
        public string? Password { get; set; }
        public string? Bio {get; set;}
        public string? Contact {get; set;}
    }
}