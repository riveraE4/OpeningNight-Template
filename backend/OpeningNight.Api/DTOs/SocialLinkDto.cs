using System.ComponentModel.DataAnnotations;

namespace OpeningNight.Api.DTOs;

public class SocialLinkDto
{
    public int Id { get; set; }
    public string Platform { get; set; } = null!;
    public string Url { get; set; } = null!;
}

public class AddSocialLinkRequest
{
    [Required]
    [StringLength(50)]
    public string Platform { get; set; } = null!;

    [Required]
    [Url]
    public string Url { get; set; } = null!;
}
