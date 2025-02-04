using System.ComponentModel.DataAnnotations;

namespace FiwFriends.Models;
public class User
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Bio { get; set; }

        public ICollection<Post> OwnPosts { get; set; }
        public ICollection<Favorite> FavoritePosts { get; set; }
        public ICollection<Join> JoinedPosts { get; set; }


    }