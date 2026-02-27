namespace OpeningNight.Api.Models;

public class UserSocialLink
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Platform { get; set; } = null!;
    public string Url { get; set; } = null!;

    //Navigation
    public User User { get; set; } = null!;
}
