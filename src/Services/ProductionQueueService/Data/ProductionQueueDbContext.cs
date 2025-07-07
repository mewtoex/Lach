using ProductionQueueService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProductionQueueService.Data;

public class ProductionQueueDbContext : DbContext
{
    public ProductionQueueDbContext(DbContextOptions<ProductionQueueDbContext> options) : base(options)
    {
    }

    public DbSet<QueueItemEntity> QueueItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<QueueItemEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderId).IsRequired();
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Position).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Items).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);
            
            entity.HasIndex(e => e.OrderId).IsUnique();
            entity.HasIndex(e => e.Position);
            
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.StartedAt);
            entity.Property(e => e.CompletedAt);
        });
    }
} 