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
public class GroupController : ControllerBase
{
    private readonly MovieClubContext _context;

    public GroupController(MovieClubContext context)
    {
        _context = context;
    }

    /// <summary>List all groups (paginated).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 50) pageSize = 50;

        var query = _context.Groups.Include(g => g.Members).AsQueryable();

        var totalCount = await query.CountAsync();

        var groups = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new GroupResponseDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                BannerUrl = g.BannerUrl,
                IsPrivate = g.IsPrivate,
                CreatedBy = g.CreatedBy,
                CreatedAt = g.CreatedAt,
                MemberCount = g.Members.Count
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

    /// <summary>Get group details with member list.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var group = await _context.Groups
            .Include(g => g.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
            return NotFound("Group not found");

        var dto = new GroupDetailResponseDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            BannerUrl = group.BannerUrl,
            IsPrivate = group.IsPrivate,
            CreatedBy = group.CreatedBy,
            CreatedAt = group.CreatedAt,
            Members = group.Members.Select(m => new GroupMemberDto
            {
                UserId = m.UserId,
                Username = m.User.Username,
                AvatarUrl = m.User.AvatarUrl,
                Role = m.Role,
                JoinedAt = m.JoinedAt
            }).ToList()
        };

        return Ok(dto);
    }

    /// <summary>Create a new group. The creator is automatically added as an Admin member.</summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGroupRequest request)
    {
        var userId = await GetCurrentUserIdAsync();

        var group = new Group
        {
            Name = request.Name,
            Description = request.Description,
            IsPrivate = request.IsPrivate,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        // Add creator as Admin member
        var membership = new GroupMember
        {
            GroupId = group.Id,
            UserId = userId,
            Role = "Admin",
            JoinedAt = DateTime.UtcNow
        };

        _context.GroupMembers.Add(membership);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = group.Id }, new GroupResponseDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            BannerUrl = group.BannerUrl,
            IsPrivate = group.IsPrivate,
            CreatedBy = group.CreatedBy,
            CreatedAt = group.CreatedAt,
            MemberCount = 1
        });
    }

    /// <summary>Update a group. Only the creator can update.</summary>
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGroupRequest request)
    {
        var userId = await GetCurrentUserIdAsync();
        var group = await _context.Groups.FindAsync(id);

        if (group == null)
            return NotFound("Group not found");

        if (group.CreatedBy != userId)
            return Forbid();

        if (request.Name != null) group.Name = request.Name;
        if (request.Description != null) group.Description = request.Description;
        if (request.IsPrivate.HasValue) group.IsPrivate = request.IsPrivate.Value;
        group.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new GroupResponseDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            BannerUrl = group.BannerUrl,
            IsPrivate = group.IsPrivate,
            CreatedBy = group.CreatedBy,
            CreatedAt = group.CreatedAt,
            MemberCount = await _context.GroupMembers.CountAsync(m => m.GroupId == id)
        });
    }

    /// <summary>Delete a group. Only the creator can delete.</summary>
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = await GetCurrentUserIdAsync();
        var group = await _context.Groups.FindAsync(id);

        if (group == null)
            return NotFound("Group not found");

        if (group.CreatedBy != userId)
            return Forbid();

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Join a group.</summary>
    [Authorize]
    [HttpPost("{id}/join")]
    public async Task<IActionResult> Join(int id)
    {
        var userId = await GetCurrentUserIdAsync();

        var group = await _context.Groups.FindAsync(id);
        if (group == null)
            return NotFound("Group not found");

        var alreadyMember = await _context.GroupMembers
            .AnyAsync(m => m.GroupId == id && m.UserId == userId);
        if (alreadyMember)
            return BadRequest("You are already a member of this group");

        var membership = new GroupMember
        {
            GroupId = id,
            UserId = userId,
            Role = "Member",
            JoinedAt = DateTime.UtcNow
        };

        _context.GroupMembers.Add(membership);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Successfully joined the group" });
    }

    /// <summary>Leave a group.</summary>
    [Authorize]
    [HttpPost("{id}/leave")]
    public async Task<IActionResult> Leave(int id)
    {
        var userId = await GetCurrentUserIdAsync();

        var membership = await _context.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == id && m.UserId == userId);

        if (membership == null)
            return BadRequest("You are not a member of this group");

        _context.GroupMembers.Remove(membership);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Successfully left the group" });
    }

    // ─── Group Invitations ───────────────────────────────────────────

    /// <summary>Create an invite link for a group. Only admins can create invites.</summary>
    [Authorize]
    [HttpPost("{id}/invites")]
    public async Task<IActionResult> CreateInvite(int id, [FromBody] CreateInviteRequest request)
    {
        var userId = await GetCurrentUserIdAsync();

        var membership = await _context.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == id && m.UserId == userId);
        if (membership == null || membership.Role != "Admin")
            return Forbid();

        var invite = new GroupInvite
        {
            GroupId = id,
            InvitedBy = userId,
            InviteToken = Guid.NewGuid().ToString("N"),
            ExpiresAt = request.ExpiryInHours.HasValue
                ? DateTime.UtcNow.AddHours(request.ExpiryInHours.Value)
                : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.GroupInvites.Add(invite);
        await _context.SaveChangesAsync();

        return Ok(new GroupInviteDto
        {
            Id = invite.Id,
            GroupId = invite.GroupId,
            InviteToken = invite.InviteToken,
            InvitedBy = invite.InvitedBy,
            InvitedByUsername = (await _context.Users.FindAsync(userId))!.Username,
            ExpiresAt = invite.ExpiresAt,
            CreatedAt = invite.CreatedAt
        });
    }

    /// <summary>List invites for a group. Only admins can view.</summary>
    [Authorize]
    [HttpGet("{id}/invites")]
    public async Task<IActionResult> GetInvites(int id)
    {
        var userId = await GetCurrentUserIdAsync();

        var membership = await _context.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == id && m.UserId == userId);
        if (membership == null || membership.Role != "Admin")
            return Forbid();

        var invites = await _context.GroupInvites
            .Where(i => i.GroupId == id)
            .Include(i => i.InvitedByUser)
            .Select(i => new GroupInviteDto
            {
                Id = i.Id,
                GroupId = i.GroupId,
                InviteToken = i.InviteToken,
                InvitedBy = i.InvitedBy,
                InvitedByUsername = i.InvitedByUser.Username,
                ExpiresAt = i.ExpiresAt,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();

        return Ok(invites);
    }

    /// <summary>Join a group using an invite token.</summary>
    [Authorize]
    [HttpPost("join/{token}")]
    public async Task<IActionResult> JoinByToken(string token)
    {
        var userId = await GetCurrentUserIdAsync();

        var invite = await _context.GroupInvites
            .FirstOrDefaultAsync(i => i.InviteToken == token);

        if (invite == null)
            return NotFound("Invalid invite token");

        if (invite.ExpiresAt.HasValue && invite.ExpiresAt.Value < DateTime.UtcNow)
            return BadRequest("This invite has expired");

        var alreadyMember = await _context.GroupMembers
            .AnyAsync(m => m.GroupId == invite.GroupId && m.UserId == userId);
        if (alreadyMember)
            return BadRequest("You are already a member of this group");

        var membership = new GroupMember
        {
            GroupId = invite.GroupId,
            UserId = userId,
            Role = "Member",
            JoinedAt = DateTime.UtcNow
        };

        _context.GroupMembers.Add(membership);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Successfully joined the group via invite" });
    }

    /// <summary>Revoke an invite. Only admins can revoke.</summary>
    [Authorize]
    [HttpDelete("{groupId}/invites/{inviteId}")]
    public async Task<IActionResult> RevokeInvite(int groupId, int inviteId)
    {
        var userId = await GetCurrentUserIdAsync();

        var membership = await _context.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);
        if (membership == null || membership.Role != "Admin")
            return Forbid();

        var invite = await _context.GroupInvites
            .FirstOrDefaultAsync(i => i.Id == inviteId && i.GroupId == groupId);

        if (invite == null)
            return NotFound("Invite not found");

        _context.GroupInvites.Remove(invite);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<int> GetCurrentUserIdAsync()
    {
        var auth0Id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var oauth = await _context.UserOAuths
            .FirstOrDefaultAsync(o => o.Provider == "auth0" && o.ProviderUserId == auth0Id);
        return oauth?.UserId ?? throw new UnauthorizedAccessException("User not synced");
    }
}
