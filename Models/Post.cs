using System.ComponentModel.DataAnnotations;

namespace FiwFriends.Models;
public class Post
    {
        [Key]
        public int Id { get; set; }
        public required string Activity { get; set; }
        [Required]
        public required string Description { get; set; }
        [Required]
        public required string ExpiredTime { get; set; }
        [Required]
        public required string AppointmentTime { get; set; }
        [Required]
        public required string Tag { get; set; }
    }