using System.ComponentModel.DataAnnotations;

namespace OpeningNight.Api.DTOs;

public class FavoriteGenreDto
{
    public int TmdbGenreId { get; set; }
    public string? GenreName { get; set; }
}

public class SetFavoriteGenresRequest
{
    [Required]
    public List<FavoriteGenreItem> Genres { get; set; } = new();
}

public class FavoriteGenreItem
{
    [Required]
    public int TmdbGenreId { get; set; }

    [StringLength(50)]
    public string? GenreName { get; set; }
}
