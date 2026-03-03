using Microsoft.AspNetCore.Mvc;
using OpeningNight.Api.Services;

namespace OpeningNight.Api.Controllers;

[ApiController]
[Route("api")]
public class MovieController : ControllerBase
{
    private readonly TmdbService _tmdb;

    public MovieController(TmdbService tmdb)
    {
        _tmdb = tmdb;
    }

    // ─── Movie Endpoints ─────────────────────────────────────────────

    /// <summary>Search movies by title.</summary>
    [HttpGet("movie/search")]
    public async Task<IActionResult> SearchMovies([FromQuery] string query, [FromQuery] int page = 1)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query is required");

        var result = await _tmdb.SearchMoviesAsync(query, page);
        return Ok(result);
    }

    /// <summary>Get trending movies.</summary>
    [HttpGet("movie/trending")]
    public async Task<IActionResult> TrendingMovies([FromQuery] string window = "day")
    {
        var result = await _tmdb.GetTrendingAsync("movie", window);
        return Ok(result);
    }

    /// <summary>Get popular movies.</summary>
    [HttpGet("movie/popular")]
    public async Task<IActionResult> PopularMovies([FromQuery] int page = 1)
    {
        var result = await _tmdb.GetPopularMoviesAsync(page);
        return Ok(result);
    }

    /// <summary>Get now-playing movies.</summary>
    [HttpGet("movie/now-playing")]
    public async Task<IActionResult> NowPlaying([FromQuery] int page = 1)
    {
        var result = await _tmdb.GetNowPlayingAsync(page);
        return Ok(result);
    }

    /// <summary>Get movie details (includes credits, videos, recommendations).</summary>
    [HttpGet("movie/{id}")]
    public async Task<IActionResult> MovieDetails(int id)
    {
        var result = await _tmdb.GetMovieDetailsAsync(id);
        return Ok(result);
    }

    /// <summary>Get movie genres list.</summary>
    [HttpGet("movie/genres")]
    public async Task<IActionResult> MovieGenres()
    {
        var result = await _tmdb.GetMovieGenresAsync();
        return Ok(result);
    }

    /// <summary>Discover movies by genre and sort.</summary>
    [HttpGet("movie/discover")]
    public async Task<IActionResult> DiscoverMovies(
        [FromQuery] string? genres = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] int page = 1)
    {
        var result = await _tmdb.DiscoverMoviesAsync(genres, sortBy, page);
        return Ok(result);
    }

    // ─── TV Endpoints ────────────────────────────────────────────────

    /// <summary>Search TV shows by title.</summary>
    [HttpGet("tv/search")]
    public async Task<IActionResult> SearchTv([FromQuery] string query, [FromQuery] int page = 1)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query is required");

        var result = await _tmdb.SearchTvAsync(query, page);
        return Ok(result);
    }

    /// <summary>Get trending TV shows.</summary>
    [HttpGet("tv/trending")]
    public async Task<IActionResult> TrendingTv([FromQuery] string window = "day")
    {
        var result = await _tmdb.GetTrendingAsync("tv", window);
        return Ok(result);
    }

    /// <summary>Get TV show details (includes credits, videos, recommendations).</summary>
    [HttpGet("tv/{id}")]
    public async Task<IActionResult> TvDetails(int id)
    {
        var result = await _tmdb.GetTvDetailsAsync(id);
        return Ok(result);
    }

    /// <summary>Get TV genres list.</summary>
    [HttpGet("tv/genres")]
    public async Task<IActionResult> TvGenres()
    {
        var result = await _tmdb.GetTvGenresAsync();
        return Ok(result);
    }
}
