namespace OpeningNight.Api.Models;

public class GroupMember
{
    public int GroupId { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } = "Member";
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    //Navigation
    public Group Group { get; set; } = null!;
    public User User { get; set; } = null!;
}
