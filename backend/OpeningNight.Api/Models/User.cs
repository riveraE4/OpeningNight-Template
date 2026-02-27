namespace OpeningNight.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PasswordHash { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // for navigation properties
    public ICollection<UserOAuth> OAuthConnections { get; set; } = new List<UserOAuth>();
    public ICollection<UserSocialLink> SocialLinks { get; set; } = new List<UserSocialLink>();
    public ICollection<UserFavoriteGenre> FavoriteGenres { get; set; } = new List<UserFavoriteGenre>();
    public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
}
