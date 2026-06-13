using KnowledgeHub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeHub.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ResourceTag> ResourceTags => Set<ResourceTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(x => x.Email)
                .IsUnique();

            entity.Property(x => x.PasswordHash)
                .IsRequired();
        });

        modelBuilder.Entity<Collection>(entity =>
        {
            entity.ToTable("collections");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasOne(x => x.User)
                .WithMany(x => x.Collections)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.ToTable("resources");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.Url)
                .IsRequired(false);

            entity.Property(x => x.ResourceType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.HasOne(x => x.Collection)
                .WithMany(x => x.Resources)
                .HasForeignKey(x => x.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(x => x.Name)
                .IsUnique();
        });

        modelBuilder.Entity<ResourceTag>(entity =>
        {
            entity.ToTable("resource_tags");

            entity.HasKey(x => new { x.ResourceId, x.TagId });

            entity.HasOne(x => x.Resource)
                .WithMany(x => x.ResourceTags)
                .HasForeignKey(x => x.ResourceId);

            entity.HasOne(x => x.Tag)
                .WithMany(x => x.ResourceTags)
                .HasForeignKey(x => x.TagId);
        });
    }
}