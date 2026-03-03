using System.Text.Json;

namespace OpeningNight.Api.Services;

public class TmdbService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public TmdbService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Tmdb:ApiKey"]!;
    }

    // ─── Movies ──────────────────────────────────────────────────────

    public async Task<JsonElement> SearchMoviesAsync(string query, int page = 1)
    {
        return await GetAsync($"search/movie?query={Uri.EscapeDataString(query)}&page={page}");
    }

    public async Task<JsonElement> GetPopularMoviesAsync(int page = 1)
    {
        return await GetAsync($"movie/popular?page={page}");
    }

    public async Task<JsonElement> GetNowPlayingAsync(int page = 1)
    {
        return await GetAsync($"movie/now_playing?page={page}");
    }

    public async Task<JsonElement> GetMovieDetailsAsync(int id)
    {
        return await GetAsync($"movie/{id}?append_to_response=credits,videos,recommendations");
    }

    public async Task<JsonElement> GetMovieGenresAsync()
    {
        return await GetAsync("genre/movie/list");
    }

    // ─── TV Shows ────────────────────────────────────────────────────

    public async Task<JsonElement> SearchTvAsync(string query, int page = 1)
    {
        return await GetAsync($"search/tv?query={Uri.EscapeDataString(query)}&page={page}");
    }

    public async Task<JsonElement> GetTvDetailsAsync(int id)
    {
        return await GetAsync($"tv/{id}?append_to_response=credits,videos,recommendations");
    }

    public async Task<JsonElement> GetTvGenresAsync()
    {
        return await GetAsync("genre/tv/list");
    }

    // ─── Trending ────────────────────────────────────────────────────

    public async Task<JsonElement> GetTrendingAsync(string mediaType = "movie", string timeWindow = "day")
    {
        return await GetAsync($"trending/{mediaType}/{timeWindow}");
    }

    // ─── Discover ────────────────────────────────────────────────────

    public async Task<JsonElement> DiscoverMoviesAsync(string? genreIds = null, string? sortBy = null, int page = 1)
    {
        var url = $"discover/movie?page={page}";
        if (!string.IsNullOrEmpty(genreIds)) url += $"&with_genres={genreIds}";
        if (!string.IsNullOrEmpty(sortBy)) url += $"&sort_by={sortBy}";
        return await GetAsync(url);
    }

    // ─── Core HTTP Helper ────────────────────────────────────────────

    private async Task<JsonElement> GetAsync(string endpoint)
    {
        // Add API key separator
        var separator = endpoint.Contains('?') ? '&' : '?';
        var url = $"{endpoint}{separator}api_key={_apiKey}&language=en-US";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(json);
    }
}
