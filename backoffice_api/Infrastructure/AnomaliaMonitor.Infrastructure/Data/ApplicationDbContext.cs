using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AnomaliaMonitor.Domain.Entities;

namespace AnomaliaMonitor.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<SubjectToResearch> SubjectsToResearch { get; set; }
    public DbSet<SubjectExample> SubjectExamples { get; set; }
    public DbSet<SiteCategory> SiteCategories { get; set; }
    public DbSet<Site> Sites { get; set; }
    public DbSet<SiteLink> SiteLinks { get; set; }
    public DbSet<Anomaly> Anomalies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SubjectToResearch configuration
        modelBuilder.Entity<SubjectToResearch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // SubjectExample configuration
        modelBuilder.Entity<SubjectExample>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Example).HasMaxLength(2000);
            
            entity.HasOne(e => e.SubjectToResearch)
                  .WithMany(s => s.Examples)
                  .HasForeignKey(e => e.SubjectToResearchId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // SiteCategory configuration
        modelBuilder.Entity<SiteCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Color).IsRequired().HasMaxLength(7);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Site configuration
        modelBuilder.Entity<Site>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Url).IsUnique();
        });

        // SiteLink configuration
        modelBuilder.Entity<SiteLink>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(1000);
            
            entity.HasOne(e => e.Site)
                  .WithMany(s => s.Links)
                  .HasForeignKey(e => e.SiteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Anomaly configuration
        modelBuilder.Entity<Anomaly>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IdentifiedSubject).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ExampleOrReason).HasMaxLength(2000);
            
            entity.HasOne(e => e.SiteLink)
                  .WithMany(sl => sl.Anomalies)
                  .HasForeignKey(e => e.SiteLinkId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.SubjectToResearch)
                  .WithMany(s => s.Anomalies)
                  .HasForeignKey(e => e.SubjectToResearchId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Many-to-many relationships
        modelBuilder.Entity<SubjectToResearch>()
            .HasMany(s => s.Categories)
            .WithMany(c => c.Subjects)
            .UsingEntity("SubjectCategories");

        modelBuilder.Entity<Site>()
            .HasMany(s => s.Categories)
            .WithMany(c => c.Sites)
            .UsingEntity("SiteCategoryMappings");

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
        });
    }

 
}