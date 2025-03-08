using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // ต้องใช้ namespace นี้สำหรับ IFormFile

namespace FiwFriends.DTOs
{
    public class UpdateUserDto
    {
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Firstname must be between 3 and 50 characters long")]
        public string? Username { get; set; } = string.Empty;

        [StringLength(50, MinimumLength = 1, ErrorMessage = "Firstname must be between 1 and 50 characters long")]
        public string? FirstName { get; set; } = string.Empty;

        [StringLength(50, MinimumLength = 1, ErrorMessage = "Lastname must be between 1 and 50 characters long")]
        public string? LastName { get; set; } = string.Empty;

        public string? Bio { get; set; }
        public string? Contact { get; set; }

        public IFormFile? ProfilePic { get; set; }

        public string? ConfirmPassword { get; set; }

        [StringLength(50, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 50 characters long")]
        public string? NewPassword { get; set; } = string.Empty;
    }
}
