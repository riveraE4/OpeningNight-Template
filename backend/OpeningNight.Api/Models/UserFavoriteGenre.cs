namespace OpeningNight.Api.Models;

public class UserFavoriteGenre
{
    public int UserId { get; set; }
    public int TmdbGenreId { get; set; }
    public string? GenreName { get; set; }

    //Navigation
    public User User { get; set; } = null!;
}
