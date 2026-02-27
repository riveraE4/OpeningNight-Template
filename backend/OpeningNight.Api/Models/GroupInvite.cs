namespace OpeningNight.Api.Models;

public class GroupInvite
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int InvitedBy { get; set; }
    public string InviteToken { get; set; } = null!;
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //Navigation
    public Group Group { get; set; } = null!;
    public User InvitedByUser { get; set; } = null!;
}
