using System.ComponentModel.DataAnnotations;

namespace FiwFriends.Models;
public class User
{
    [Key]
    public int UserId { get; set; }
    [Required]
    public required string FirstName { get; set; }
    [Required]
    public required string LastName { get; set; }
    [Required]
    public required string Username { get; set; }
    [Required]
    public required string Password { get; set; }
    public string? Bio { get; set; }

    public ICollection<Post> OwnPosts { get; set; } = new List<Post>();
    public ICollection<Post> FavoritePosts { get; set; } = new List<Post>();
    public ICollection<Join> JoinedPosts { get; set; } = new List<Join>();  
    public ICollection<Form> SubmittedForms { get; set; } = new List<Form>();  
}