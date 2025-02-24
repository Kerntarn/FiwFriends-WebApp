using FiwFriends.Models;
using System.Collections.Generic;

namespace FiwFriends.DTOs
{
    public class UserPostStatusViewModel
    {
        public IOrderedEnumerable<UserPostStatusDTO> Posts { get; set; }
    }
}
