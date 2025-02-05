using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FiwFriends.Models;
public class Post
    {
        [Key]
        public int Id { get; set; }
        public string Activity { get; set; }
        public string Description { get; set; }
        public string ExpiredTime { get; set; }
        public string AppointmentTime { get; set; }

        [AllowNull]
        public Form Form { get; set; }
        // public string Tag { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; }

        public ICollection<Favorite> FavoriteBy { get; set; }

        public ICollection<Join> Participants { get; set; }

    }