using Microsoft.EntityFrameworkCore;
using Teletrome.API.Entities;

namespace Teletrome.API.Data;

public class TeletromeDbContext : DbContext
{
    public TeletromeDbContext(DbContextOptions<TeletromeDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Build> Builds => Set<Build>();
    public DbSet<FunctionRegistryEntry> FunctionRegistry => Set<FunctionRegistryEntry>();
    public DbSet<Install> Installs => Set<Install>();
    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("projects");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.ApiKey).HasColumnName("api_key").HasColumnType("char(64)").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("SYSDATETIMEOFFSET()");
            entity.HasIndex(e => e.ApiKey).IsUnique();
        });

        modelBuilder.Entity<Build>(entity =>
        {
            entity.ToTable("builds");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Version).HasColumnName("version").HasMaxLength(50).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("SYSDATETIMEOFFSET()");
            entity.HasIndex(e => new { e.ProjectId, e.Version }).IsUnique().HasDatabaseName("uq_build");
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("ix_builds_project_id");
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.Builds)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FunctionRegistryEntry>(entity =>
        {
            entity.ToTable("function_registry");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BuildId).HasColumnName("build_id");
            entity.Property(e => e.FunctionName).HasColumnName("function_name").HasMaxLength(256).IsRequired();
            entity.Property(e => e.FileName).HasColumnName("file_name").HasMaxLength(512).IsRequired();
            entity.Property(e => e.FirstSeenAt).HasColumnName("first_seen_at").HasDefaultValueSql("SYSDATETIMEOFFSET()");
            entity.HasIndex(e => new { e.BuildId, e.FunctionName, e.FileName }).IsUnique().HasDatabaseName("uq_function");
            entity.HasIndex(e => e.BuildId).HasDatabaseName("ix_function_registry_build_id");
            entity.HasOne(e => e.Build)
                  .WithMany(b => b.Functions)
                  .HasForeignKey(e => e.BuildId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Install>(entity =>
        {
            entity.ToTable("installs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.InstallId).HasColumnName("install_id").HasColumnType("char(36)").IsRequired();
            entity.Property(e => e.FirstSeenAt).HasColumnName("first_seen_at").HasDefaultValueSql("SYSDATETIMEOFFSET()");
            entity.Property(e => e.LastSeenAt).HasColumnName("last_seen_at").HasDefaultValueSql("SYSDATETIMEOFFSET()");
            entity.HasIndex(e => new { e.ProjectId, e.InstallId }).IsUnique().HasDatabaseName("uq_install");
            entity.HasIndex(e => e.ProjectId).HasDatabaseName("ix_installs_project_id");
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.Installs)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FunctionRegistryId).HasColumnName("function_registry_id");
            entity.Property(e => e.InstallId).HasColumnName("install_id");
            entity.Property(e => e.RecordedAt).HasColumnName("recorded_at");
            entity.HasIndex(e => e.FunctionRegistryId).HasDatabaseName("ix_events_function_registry_id");
            entity.HasIndex(e => e.InstallId).HasDatabaseName("ix_events_install_id");
            entity.HasOne(e => e.FunctionRegistryEntry)
                  .WithMany(f => f.Events)
                  .HasForeignKey(e => e.FunctionRegistryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Install)
                  .WithMany(i => i.Events)
                  .HasForeignKey(e => e.InstallId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
