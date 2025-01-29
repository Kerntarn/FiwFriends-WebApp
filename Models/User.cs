using System.ComponentModel.DataAnnotations;

namespace FiwFriends.Models;
public class User
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Bio { get; set; }
    }