using FiwFriends.Models;
using System.Collections.Generic;

namespace FiwFriends.DTOs
{
    public class UserPendingStatusViewModel
    {
        public required string Activity { get; set; }
        public required string User { get; set; }
        public required string FormId { get; set; }
        public string Status {get; set;}
        public required List<QnA> QnAs { get; set; }
    }
}