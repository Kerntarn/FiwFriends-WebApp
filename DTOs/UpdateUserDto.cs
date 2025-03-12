using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FiwFriends.DTOs
{
    public class UpdateUserDto
    {
        [StringLength(50, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 50 characters long")]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; } = string.Empty;

        [StringLength(50, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 50 characters long")]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; } = string.Empty;

        [Display(Name = "Contact")]
        public string? Contact { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePic { get; set; }

        [Display(Name = "Old Password")]
        public string? OldPassword { get; set; }

        [StringLength(50, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 50 characters long")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public string? ConfirmNewPassword { get; set; }
    }
}