using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using OpeningNight.Api.Data;
using OpeningNight.Api.DTOs;
using OpeningNight.Api.Models;

namespace OpeningNight.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly MovieClubContext _context;

    public UserController(MovieClubContext context)
    {
        _context = context;
    }

    /// <summary>Get the current user's profile.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = await GetCurrentUserIdAsync();
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound("User not found");

        return Ok(new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio
        });
    }

    /// <summary>Update the current user's profile.</summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = await GetCurrentUserIdAsync();
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound("User not found");

        if (request.Username != null)
        {
            var taken = await _context.Users
                .AnyAsync(u => u.Username == request.Username && u.Id != userId);
            if (taken)
                return BadRequest("Username already taken");

            user.Username = request.Username;
        }

        if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;
        if (request.Bio != null) user.Bio = request.Bio;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio
        });
    }

    /// <summary>List groups the current user belongs to (paginated).</summary>
    [HttpGet("me/groups")]
    public async Task<IActionResult> GetMyGroups([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 50) pageSize = 50;

        var userId = await GetCurrentUserIdAsync();

        var query = _context.GroupMembers
            .Where(m => m.UserId == userId);

        var totalCount = await query.CountAsync();

        var groups = await query
            .OrderByDescending(m => m.JoinedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new GroupResponseDto
            {
                Id = m.Group.Id,
                Name = m.Group.Name,
                Description = m.Group.Description,
                BannerUrl = m.Group.BannerUrl,
                IsPrivate = m.Group.IsPrivate,
                CreatedBy = m.Group.CreatedBy,
                CreatedAt = m.Group.CreatedAt,
                MemberCount = m.Group.Members.Count
            })
            .ToListAsync();

        return Ok(new PaginatedResponse<GroupResponseDto>
        {
            Items = groups,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    // ─── Social Links ────────────────────────────────────────────────

    /// <summary>Get the current user's social links.</summary>
    [HttpGet("me/social-links")]
    public async Task<IActionResult> GetSocialLinks()
    {
        var userId = await GetCurrentUserIdAsync();

        var links = await _context.UserSocialLinks
            .Where(l => l.UserId == userId)
            .Select(l => new SocialLinkDto
            {
                Id = l.Id,
                Platform = l.Platform,
                Url = l.Url
            })
            .ToListAsync();

        return Ok(links);
    }

    /// <summary>Add a social link.</summary>
    [HttpPost("me/social-links")]
    public async Task<IActionResult> AddSocialLink([FromBody] AddSocialLinkRequest request)
    {
        var userId = await GetCurrentUserIdAsync();

        var link = new UserSocialLink
        {
            UserId = userId,
            Platform = request.Platform,
            Url = request.Url
        };

        _context.UserSocialLinks.Add(link);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSocialLinks), null, new SocialLinkDto
        {
            Id = link.Id,
            Platform = link.Platform,
            Url = link.Url
        });
    }

    /// <summary>Delete a social link.</summary>
    [HttpDelete("me/social-links/{id}")]
    public async Task<IActionResult> DeleteSocialLink(int id)
    {
        var userId = await GetCurrentUserIdAsync();

        var link = await _context.UserSocialLinks
            .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);

        if (link == null)
            return NotFound("Social link not found");

        _context.UserSocialLinks.Remove(link);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ─── Favorite Genres ─────────────────────────────────────────────

    /// <summary>Get the current user's favorite genres.</summary>
    [HttpGet("me/favorite-genres")]
    public async Task<IActionResult> GetFavoriteGenres()
    {
        var userId = await GetCurrentUserIdAsync();

        var genres = await _context.UserFavoriteGenres
            .Where(g => g.UserId == userId)
            .Select(g => new FavoriteGenreDto
            {
                TmdbGenreId = g.TmdbGenreId,
                GenreName = g.GenreName
            })
            .ToListAsync();

        return Ok(genres);
    }

    /// <summary>Replace the current user's favorite genres (bulk set).</summary>
    [HttpPut("me/favorite-genres")]
    public async Task<IActionResult> SetFavoriteGenres([FromBody] SetFavoriteGenresRequest request)
    {
        var userId = await GetCurrentUserIdAsync();

        // Remove existing genres
        var existing = await _context.UserFavoriteGenres
            .Where(g => g.UserId == userId)
            .ToListAsync();
        _context.UserFavoriteGenres.RemoveRange(existing);

        // Add new genres
        var newGenres = request.Genres.Select(g => new UserFavoriteGenre
        {
            UserId = userId,
            TmdbGenreId = g.TmdbGenreId,
            GenreName = g.GenreName
        }).ToList();

        _context.UserFavoriteGenres.AddRange(newGenres);
        await _context.SaveChangesAsync();

        return Ok(newGenres.Select(g => new FavoriteGenreDto
        {
            TmdbGenreId = g.TmdbGenreId,
            GenreName = g.GenreName
        }));
    }

    private async Task<int> GetCurrentUserIdAsync()
    {
        var auth0Id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var oauth = await _context.UserOAuths
            .FirstOrDefaultAsync(o => o.Provider == "auth0" && o.ProviderUserId == auth0Id);
        return oauth?.UserId ?? throw new UnauthorizedAccessException("User not synced");
    }
}
