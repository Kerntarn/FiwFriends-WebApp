using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs
{
    public class UpdateUserDto
    {
        public required string Username { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? Password { get; set; }
        public string? Bio {get; set;}
        public string? Contact {get; set;}
    }
}