using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs
{
    public class UpdateUserDto
    {
        [Required]
        public required string Username { get; set; }
        [Required]
        public required string FirstName { get; set; }
        [Required]
        public required string LastName { get; set; }
        public string? Password { get; set; }
    }
}