namespace OpeningNight.Api.DTOs;

public class GroupResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? BannerUrl { get; set; }
    public bool IsPrivate { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
}
