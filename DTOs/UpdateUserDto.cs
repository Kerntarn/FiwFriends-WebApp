using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs
{
    public class UpdateUserDto
    {
        public required string Username { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? Bio {get; set;}
        public string? Contact {get; set;}
         public string ConfirmPassword { get; set; }
        public string NewPassword { get; set; }
    }
}