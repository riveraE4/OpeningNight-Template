using Microsoft.EntityFrameworkCore;
using OpeningNight.Api.Models;

namespace OpeningNight.Api.Data;

public class MovieClubContext : DbContext
{
    public MovieClubContext(DbContextOptions<MovieClubContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<UserOAuth> UserOAuths { get; set; }
    public DbSet<UserSocialLink> UserSocialLinks { get; set; }
    public DbSet<UserFavoriteGenre> UserFavoriteGenres { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }
    public DbSet<GroupInvite> GroupInvites { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserFavoriteGenre>()
            .HasKey(x => new { x.UserId, x.TmdbGenreId });

        modelBuilder.Entity<GroupMember>()
            .HasKey(x => new { x.GroupId, x.UserId });
    }

}
