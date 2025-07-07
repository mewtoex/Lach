using Microsoft.EntityFrameworkCore;
using WhatsAppService.Entities;

namespace WhatsAppService.Data;

public class WhatsAppDbContext : DbContext
{
    public WhatsAppDbContext(DbContextOptions<WhatsAppDbContext> options) : base(options)
    {
    }

    public DbSet<WhatsAppSessionEntity> Sessions { get; set; }
    public DbSet<WhatsAppMessageEntity> Messages { get; set; }
    public DbSet<WhatsAppContactEntity> Contacts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // WhatsApp Session
        modelBuilder.Entity<WhatsAppSessionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.QrCode).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            entity.HasIndex(e => e.SessionId).IsUnique();
            entity.HasIndex(e => e.PhoneNumber);
        });

        // WhatsApp Message
        modelBuilder.Entity<WhatsAppMessageEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.MessageId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FromNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ToNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.MessageType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Direction).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.ProcessedAt);
            
            entity.HasIndex(e => e.MessageId).IsUnique();
            entity.HasIndex(e => e.FromNumber);
            entity.HasIndex(e => e.CreatedAt);
        });

        // WhatsApp Contact
        modelBuilder.Entity<WhatsAppContactEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.CustomerId);
            entity.Property(e => e.LastInteraction).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
            entity.HasIndex(e => e.CustomerId);
        });
    }
} 