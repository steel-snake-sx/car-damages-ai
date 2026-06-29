using CarDamageClaims.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarDamageClaims.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<DamageRequest> DamageRequests => Set<DamageRequest>();

    public DbSet<DamageRequestPhoto> DamageRequestPhotos => Set<DamageRequestPhoto>();

    public DbSet<DamageEstimateItem> DamageEstimateItems => Set<DamageEstimateItem>();

    public DbSet<NotificationOutbox> NotificationOutbox => Set<NotificationOutbox>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.FirstName).IsRequired();
            entity.Property(x => x.LastName).IsRequired();
            entity.Property(x => x.Role).HasConversion<string>().IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();

            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<DamageRequest>(entity =>
        {
            entity.ToTable("damage_requests");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().IsRequired();
            entity.Property(x => x.LastName).IsRequired();
            entity.Property(x => x.FirstName).IsRequired();
            entity.Property(x => x.Email).IsRequired();
            entity.Property(x => x.Phone).IsRequired();
            entity.Property(x => x.CarBrand).IsRequired();
            entity.Property(x => x.CarModel).IsRequired();
            entity.Property(x => x.CarYear).IsRequired();
            entity.Property(x => x.AiIsCar).IsRequired();
            entity.Property(x => x.AiSummary).IsRequired();
            entity.Property(x => x.AiEstimatedTotalCost).HasPrecision(18, 2).IsRequired();

            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.Email);

            entity
                .HasOne(x => x.ApprovedByUser)
                .WithMany(x => x.ApprovedRequests)
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DamageRequestPhoto>(entity =>
        {
            entity.ToTable("damage_request_photos");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.FileName).IsRequired();
            entity.Property(x => x.FilePath).IsRequired();
            entity.Property(x => x.SortOrder).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();

            entity.HasIndex(x => x.DamageRequestId);

            entity
                .HasOne(x => x.DamageRequest)
                .WithMany(x => x.Photos)
                .HasForeignKey(x => x.DamageRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DamageEstimateItem>(entity =>
        {
            entity.ToTable("damage_estimate_items");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.PartName).IsRequired();
            entity.Property(x => x.DamageDescription).IsRequired();
            entity.Property(x => x.Severity).IsRequired();
            entity.Property(x => x.EstimatedCost).HasPrecision(18, 2).IsRequired();
            entity.Property(x => x.Confidence).IsRequired();

            entity.HasIndex(x => x.DamageRequestId);

            entity
                .HasOne(x => x.DamageRequest)
                .WithMany(x => x.EstimateItems)
                .HasForeignKey(x => x.DamageRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationOutbox>(entity =>
        {
            entity.ToTable("notification_outbox");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.RecipientEmail).IsRequired();
            entity.Property(x => x.Subject);
            entity.Property(x => x.NotificationType).HasConversion<string>().IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();

            entity.HasIndex(x => x.DamageRequestId);
            entity.HasIndex(x => x.RecipientEmail);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.CreatedAt);

            entity
                .HasOne(x => x.DamageRequest)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.DamageRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}
