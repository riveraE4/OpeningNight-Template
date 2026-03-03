namespace OpeningNight.Api.DTOs;

public class GroupInviteDto
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string InviteToken { get; set; } = null!;
    public int InvitedBy { get; set; }
    public string InvitedByUsername { get; set; } = null!;
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateInviteRequest
{
    /// <summary>Optional expiry in hours. Null = never expires.</summary>
    public int? ExpiryInHours { get; set; }
}
