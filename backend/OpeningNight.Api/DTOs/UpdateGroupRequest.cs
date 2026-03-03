using System.ComponentModel.DataAnnotations;

namespace OpeningNight.Api.DTOs;

public class UpdateGroupRequest
{
    [StringLength(100, MinimumLength = 2)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool? IsPrivate { get; set; }
}
