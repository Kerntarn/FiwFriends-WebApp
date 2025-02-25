using FiwFriends.Models;
using System.Collections.Generic;

namespace FiwFriends.Models
{
    public class UserPostStatusViewModel
    {
        public required string Activity { get; set; }
        public required string Owner { get; set; }
        public required DateTimeOffset AppointmentTime { get; set; }
        public string Status {get; set;}
    }
}
