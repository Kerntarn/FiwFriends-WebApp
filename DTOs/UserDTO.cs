
namespace FiwFriends.DTOs;
public class UserDTO
{

    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    private byte[]? _profilePic;
    public byte[]? ProfilePic
    {
        get => _profilePic;
        set
        {
            _profilePic = value;
            ProfilePicBase64 = value != null ? $"data:image/png;base64,{Convert.ToBase64String(value)}" : null;
        }
    }
    public string? ProfilePicBase64 { get; private set; }
    public string? Bio { get; set; }
    public string? Contact {get; set; } 
}