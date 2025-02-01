using System.ComponentModel.DataAnnotations;

namespace FiwFriends.Models;
public class Post
    {
        [Key]
        public int Id { get; set; }
        public string Activity { get; set; }
        public string Description { get; set; }

        public string ExpiredTime { get; set; }
        public string AppointmentTime { get; set; }
        public string Tag { get; set; }
    }