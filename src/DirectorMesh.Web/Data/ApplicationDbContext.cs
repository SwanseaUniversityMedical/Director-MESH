using DirectorMesh.Web.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace DirectorMesh.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<DataPlane> DataPlanes => Set<DataPlane>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<NetworkService> NetworkServices => Set<NetworkService>();
    public DbSet<AclRule> AclRules => Set<AclRule>();
    public DbSet<SiteConfiguration> SiteConfigurations => Set<SiteConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Site>()
            .HasMany(s => s.DataPlanes)
            .WithMany(dp => dp.Sites)
            .UsingEntity("SiteDataPlane");

        modelBuilder.Entity<AclRule>()
            .HasOne(a => a.SourceService)
            .WithMany(s => s.SourceAclRules)
            .HasForeignKey(a => a.SourceServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AclRule>()
            .HasOne(a => a.DestinationService)
            .WithMany(s => s.DestinationAclRules)
            .HasForeignKey(a => a.DestinationServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AclRule>()
            .HasOne(a => a.DataPlane)
            .WithMany(d => d.AclRules)
            .HasForeignKey(a => a.DataPlaneId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
