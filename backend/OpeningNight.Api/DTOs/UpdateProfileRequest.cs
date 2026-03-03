using System.ComponentModel.DataAnnotations;

namespace OpeningNight.Api.DTOs;

public class UpdateProfileRequest
{
    [StringLength(30, MinimumLength = 3)]
    public string? Username { get; set; }

    [Url]
    public string? AvatarUrl { get; set; }

    [StringLength(300)]
    public string? Bio { get; set; }
}
