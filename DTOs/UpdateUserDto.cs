using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs
{
    public class UpdateUserDto
    {
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Firstname must be between 3 and 50 characters long")]
        public required string Username { get; set; }
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Firstname must be between 1 and 50 characters long")]
        public required string FirstName { get; set; }
        [StringLength(50,MinimumLength = 1,ErrorMessage = "Lastname must be between 1 and 50 characters long")]
        public required string LastName { get; set; }
        public string? Bio {get; set;}
        public string? Contact {get; set;}
        public IFormFile? ProfilePic { get; set; }
        public string? ConfirmPassword { get; set; }
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 50 characters long")]
        public string NewPassword { get; set; }
    }
}