using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using OpeningNight.Api.Data;
using OpeningNight.Api.Models;
using System.Net;

namespace OpeningNight.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MovieClubContext _context;
    private readonly IConfiguration _configuration;
    private readonly Authorization _authorization;

    public AuthController(MovieClubContext context, IConfiguration configuration, Authorization authorization)
    {
        _context = context;
        _configuration = configuration;
        _authorization = authorization;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // does the email already exist?
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Email is already in use");
        // check if the username already exists
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest("Username already taken");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { user.Id, user.Username, user.Email });

    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // this finds the user by email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        // when the user is not found this will return a 401 error
        if (user == null)
            return Unauthorized("Invalid email or password");
        // this will verify the password and return the right response
        var passwordMatch = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!passwordMatch)
            return Unauthorized("Invalid email or password");
        var token = GenerateJwtToken(user);
        return Ok(new { user.Id, user.Username, user.Email, token });
    }
    [HttpPost("group")]
    [HttpPut("group")]
    [HttpGet("group")]
    [HttpDelete("group")]
    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username)
        };
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:ExpiryInDays"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class RegisterRequest
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
