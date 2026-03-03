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
public class AuthController : ControllerBase
{
    private readonly MovieClubContext _context;

    public AuthController(MovieClubContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Sync the authenticated Auth0 user with the local database.
    /// Call this from the frontend after a successful Auth0 login.
    /// Creates the user if they don't exist, or returns the existing record.
    /// </summary>
    [Authorize]
    [HttpPost("sync")]
    public async Task<IActionResult> Sync()
    {
        var auth0Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email)
                    ?? User.FindFirstValue("email");
        var name = User.FindFirstValue(ClaimTypes.Name)
                   ?? User.FindFirstValue("name")
                   ?? User.FindFirstValue("nickname");

        if (string.IsNullOrEmpty(auth0Id))
            return Unauthorized("Missing user identifier in token");

        // Try to find existing user by Auth0 ID (stored in OAuth connections)
        var oauthConnection = await _context.UserOAuths
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Provider == "auth0" && o.ProviderUserId == auth0Id);

        if (oauthConnection != null)
        {
            // User exists — update last login info
            var existingUser = oauthConnection.User;
            existingUser.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new UserResponseDto
            {
                Id = existingUser.Id,
                Username = existingUser.Username,
                Email = existingUser.Email,
                AvatarUrl = existingUser.AvatarUrl,
                Bio = existingUser.Bio
            });
        }

        // Check if a user with this email already exists (migrated from old JWT auth)
        User? user = null;
        if (!string.IsNullOrEmpty(email))
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        if (user == null)
        {
            // Brand new user — create them
            user = new User
            {
                Username = name ?? email?.Split('@')[0] ?? "user",
                Email = email ?? "",
                PasswordHash = null, // Auth0 handles passwords
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        // Link Auth0 identity to local user
        var newOAuth = new UserOAuth
        {
            UserId = user.Id,
            Provider = "auth0",
            ProviderUserId = auth0Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserOAuths.Add(newOAuth);
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

    /// <summary>Get the current authenticated user's profile.</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var auth0Id = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var oauthConnection = await _context.UserOAuths
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Provider == "auth0" && o.ProviderUserId == auth0Id);

        if (oauthConnection == null)
            return NotFound("User not synced. Call POST /api/auth/sync first.");

        var user = oauthConnection.User;

        return Ok(new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio
        });
    }
}
