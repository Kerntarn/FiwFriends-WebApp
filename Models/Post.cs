using System.ComponentModel.DataAnnotations;

namespace FiwFriends.Models;

public class Post : BaseModel
    {
        [Key]
        public int PostId { get; set; }
        [Required]
        public required string Activity { get; set; }
        [Required]
        public required string Description { get; set; }
        [Required]
        public required string ExpiredTime { get; set; }
        [Required]
        public required string AppointmentTime { get; set; }
        [Required]
        public required string OwnerId { get; set; }  
        public User? Owner { get; set; } = null!;

        public ICollection<User> FavoritedBy { get; set; } = new List<User>();
        public ICollection<Join> Participants { get; set; }  = new List<Join>();
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<Form> Forms { get; set; } = new List<Form>();
        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }

public class PublicViewPost : BaseModel {
        public int PostId { get; set; }
        public required string Activity { get; set; }
        public required string Description { get; set; }
        public required string ExpiredTime { get; set; }
        public required string AppointmentTime { get; set; }
        public required User Owner { get; set; }
        public required IEnumerable<User> Participants { get; set; }
        public required IEnumerable<Tag> Tags { get; set; }
        public required IEnumerable<Question> Questions { get; set; }
        public required IEnumerable<User> FavoritedBy { get; set; }
        public required int ParticipantsCount { get; set; }
}

public class OwnerViewPost : PublicViewPost {
    public required IEnumerable<Form> Forms { get; set; }
}