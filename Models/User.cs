using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FiwFriends.Models;
public class User : IdentityUser
{
    [Required]
    public required string FirstName { get; set; }
    [Required]
    public required string LastName { get; set; }
    public byte[]? ProfilePicture { get; set; } // Allows NULL
    public string? Bio { get; set; } // Allows NULL
    public ICollection<Post> OwnPosts { get; set; } = new List<Post>();
    public ICollection<Post> FavoritePosts { get; set; } = new List<Post>();
    public ICollection<Join> JoinedPosts { get; set; } = new List<Join>();
    public ICollection<Form> SubmittedForms { get; set; } = new List<Form>();
}
