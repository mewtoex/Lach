using DeliveryService.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Data;

public class DeliveryDbContext : DbContext
{
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options)
    {
    }

    public DbSet<RestaurantLocationEntity> RestaurantLocations { get; set; }
    public DbSet<DeliveryConfigEntity> DeliveryConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RestaurantLocationEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Latitude).IsRequired();
            entity.Property(e => e.Longitude).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<DeliveryConfigEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BaseFee).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.FeePerKm).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.MaxDistanceKm).IsRequired();
            entity.Property(e => e.FreeDeliveryThreshold).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
} 