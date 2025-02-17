using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FiwFriends.Models;
public class User : IdentityUser
    {
        [Key]
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte[] ProfilePicture { get; set; }
        public string Password { get; set; }
        public string Bio { get; set; }

        public ICollection<Post> OwnPosts { get; set; }
        public ICollection<Post> FavoritePosts { get; set; } = new List<Post>();
        public ICollection<Join> JoinedPosts { get; set; }
        public ICollection<Form> SubmittedForms { get; set; }
    }