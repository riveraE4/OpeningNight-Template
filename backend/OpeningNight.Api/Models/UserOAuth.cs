namespace OpeningNight.Api.Models;

public class UserOAuth
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Provider { get; set; } = null!;
    public string ProviderUserId { get; set; } = null!;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //Navigation
    public User User { get; set; } = null!;
}
