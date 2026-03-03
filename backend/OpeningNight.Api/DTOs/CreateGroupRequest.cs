using System.ComponentModel.DataAnnotations;

namespace OpeningNight.Api.DTOs;

public class CreateGroupRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsPrivate { get; set; } = false;
}
