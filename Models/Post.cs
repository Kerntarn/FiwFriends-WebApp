using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FiwFriends.Models;
public class Post
    {
        [Key]
        public int PostId { get; set; }
        public string Activity { get; set; }
        public string Description { get; set; }
        public string ExpiredTime { get; set; }
        public string AppointmentTime { get; set; }
        public int OwnerId { get; set; }
        public User Owner { get; set; }

        public ICollection<User> FavoritedBy { get; set; } = new List<User>();

        public ICollection<Join> Participants { get; set; }
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<Form> Forms { get; set; }
        public ICollection<Question> Questions { get; set; }
    }