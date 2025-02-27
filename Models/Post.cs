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
        public required string Location { get; set; }
        [Required]
        public required DateTimeOffset ExpiredTime { get; set; }
        [Required]
        public required DateTimeOffset AppointmentTime { get; set; }
        [Required]
        public required int Limit { get; set; }
        [Required]
        public required string OwnerId { get; set; }  
        public User Owner { get; set; } = null!;
        public ICollection<User> FavoritedBy { get; set; } = new List<User>();
        public ICollection<Join> Participants { get; set; }  = new List<Join>();
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<Form> Forms { get; set; } = new List<Form>();
        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }

public class IndexPost{
        public required int PostId { get; set; }
        public required string Activity { get; set; }
        public required string Description { get; set; }
        public required string Location { get; set; }
        public required DateTimeOffset AppointmentTime { get; set; }
        public required User Owner { get; set; }
        public required int ParticipantsCount { get; set; }
        public required bool IsFav { get; set; }
        public required IEnumerable<Tag> Tags { get; set; }

}

public class DetailPost : IndexPost {
        public required DateTimeOffset ExpiredTime { get; set; }
        public IEnumerable<User> Participants { get; set; }  = new List<User>();
        public IEnumerable<Question> Questions { get; set; } = new List<Question>();
}