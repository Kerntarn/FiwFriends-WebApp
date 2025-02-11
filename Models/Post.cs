using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

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
        public required int OwnerId { get; set; }  
        public User? Owner { get; set; } = null!;

        public ICollection<User> FavoritedBy { get; set; } = new List<User>();
        public ICollection<Join> Participants { get; set; }  = new List<Join>();
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<Form> Forms { get; set; } = new List<Form>();
        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }