using FiwFriends.Models;
using System.Collections.Generic;

namespace FiwFriends.DTOs
{
    public class UserPendingStatusViewModel
    {
        public IOrderedEnumerable<UserPendingStatusDTO> Forms { get; set; }
    }
}