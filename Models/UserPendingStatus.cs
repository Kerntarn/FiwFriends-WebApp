namespace FiwFriends.Models
{
    public class UserPendingStatusViewModel
    {
        public required string Activity { get; set; }

        public required string UserId { get; set; }

        public required string Username { get; set; }

        public required string PostId { get; set; }
        public required string FormId { get; set; }
        public required string Status {get; set;}
        public required List<QnA> QnAs { get; set; }
    }
}