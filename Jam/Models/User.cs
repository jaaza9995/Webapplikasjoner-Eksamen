namespace Jam.Models;

public class User 
{
    public int UserId { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public string? Lastname { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // public int AvatarId { get; set; }
    // public Avatar Avatar { get; set; }
}