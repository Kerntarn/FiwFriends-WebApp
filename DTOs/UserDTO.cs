
namespace FiwFriends.DTOs;
public class UserDTO
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public byte[]? ProfilePic { get; set; }
    public string? Bio { get; set; }
    public string? Contact {get; set;}
}