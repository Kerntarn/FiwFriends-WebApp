using System.ComponentModel.DataAnnotations;

namespace FiwFriends.DTOs;

public class UserPendingStatusDTO
    {
        public required string Activity { get; set; }
        public required string User { get; set; }
        public required string FormId { get; set; }
        public string Status {get; set;}
    }