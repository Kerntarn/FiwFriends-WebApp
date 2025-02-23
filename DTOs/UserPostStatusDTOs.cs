using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs;

public class UserPostStatusDTO
    {
        public required string Activity { get; set; }
        public required string Owner { get; set; }
        public required string AppointmentTime { get; set; }
        public string Status {get; set;}
    }