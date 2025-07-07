using Microsoft.EntityFrameworkCore;
using ProductService.Entities;

namespace ProductService.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<AddOnCategoryEntity> AddOnCategories { get; set; }
    public DbSet<ProductAddOnEntity> ProductAddOns { get; set; }
    public DbSet<ProductAddOnCategoryEntity> ProductAddOnCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Category
        modelBuilder.Entity<CategoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ImageUrl).HasMaxLength(200);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.DisplayOrder);
        });

        // Configuração da entidade Product
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ImageUrl).HasMaxLength(200);
            entity.Property(e => e.PreparationTimeMinutes).HasDefaultValue(15);
            entity.Property(e => e.HasAddOns).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsAvailable);
            entity.HasIndex(e => e.HasAddOns);
            
            // Relacionamento com Category
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração da entidade AddOnCategory
        modelBuilder.Entity<AddOnCategoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ImageUrl).HasMaxLength(200);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.MinPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MaxPrice).HasColumnType("decimal(18,2)");
        });

        // Configuração da entidade ProductAddOn
        modelBuilder.Entity<ProductAddOnEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ImageUrl).HasMaxLength(200);
            entity.Property(e => e.MaxQuantity).HasDefaultValue(5);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.AddOnCategoryId);
            entity.HasIndex(e => e.IsAvailable);
            
            // Relacionamento com AddOnCategory
            entity.HasOne(e => e.AddOnCategory)
                  .WithMany(c => c.AddOns)
                  .HasForeignKey(e => e.AddOnCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Relacionamento opcional com Product
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.SpecificAddOns)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired(false);
        });

        // Configuração da entidade ProductAddOnCategory (tabela de relacionamento)
        modelBuilder.Entity<ProductAddOnCategoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Relacionamento com Product
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.ProductAddOnCategories)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Relacionamento com AddOnCategory
            entity.HasOne(e => e.AddOnCategory)
                  .WithMany(c => c.ProductAddOnCategories)
                  .HasForeignKey(e => e.AddOnCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Índice composto para otimização
            entity.HasIndex(e => new { e.ProductId, e.AddOnCategoryId }).IsUnique();
        });

        // Seed Data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Categories
        var categories = new List<CategoryEntity>
        {
            new()
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Açaí",
                Description = "Produtos de açaí e bowls",
                ImageUrl = "https://example.com/categories/acai.jpg",
                Color = "#8B4513",
                Icon = "🍇",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Name = "Lanches",
                Description = "Sanduíches e hambúrgueres",
                ImageUrl = "https://example.com/categories/lanches.jpg",
                Color = "#FF6B35",
                Icon = "🍔",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Name = "Bebidas",
                Description = "Refrigerantes, sucos e shakes",
                ImageUrl = "https://example.com/categories/bebidas.jpg",
                Color = "#4ECDC4",
                Icon = "🥤",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                Name = "Sobremesas",
                Description = "Doces e sobremesas",
                ImageUrl = "https://example.com/categories/sobremesas.jpg",
                Color = "#FFE66D",
                Icon = "🍰",
                DisplayOrder = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        modelBuilder.Entity<CategoryEntity>().HasData(categories);

        // Products
        var products = new List<ProductEntity>
        {
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Açaí Tradicional",
                Description = "Açaí puro com granola e banana",
                Price = 12.90m,
                CategoryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                IsAvailable = true,
                HasAddOns = true,
                PreparationTimeMinutes = 5,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Açaí Especial",
                Description = "Açaí com frutas da estação",
                Price = 15.90m,
                CategoryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                IsAvailable = true,
                HasAddOns = true,
                PreparationTimeMinutes = 7,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "X-Burger",
                Description = "Hambúrguer com queijo, alface e tomate",
                Price = 18.90m,
                CategoryId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                IsAvailable = true,
                HasAddOns = true,
                PreparationTimeMinutes = 15,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "X-Bacon",
                Description = "Hambúrguer com bacon, queijo e molho especial",
                Price = 22.90m,
                CategoryId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                IsAvailable = true,
                HasAddOns = true,
                PreparationTimeMinutes = 18,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "Coca-Cola",
                Description = "Refrigerante Coca-Cola 350ml",
                Price = 6.50m,
                CategoryId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                IsAvailable = true,
                HasAddOns = false,
                PreparationTimeMinutes = 1,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Name = "Suco de Laranja",
                Description = "Suco natural de laranja 300ml",
                Price = 8.00m,
                CategoryId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                IsAvailable = true,
                HasAddOns = false,
                PreparationTimeMinutes = 3,
                CreatedAt = DateTime.UtcNow
            }
        };

        modelBuilder.Entity<ProductEntity>().HasData(products);

        // AddOn Categories
        var addOnCategories = new List<AddOnCategoryEntity>
        {
            new()
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Frutas",
                Description = "Frutas frescas para adicionar",
                ImageUrl = "https://example.com/addon-categories/frutas.jpg",
                Color = "#FF6B6B",
                Icon = "🍓",
                IsActive = true,
                DisplayOrder = 1,
                MaxSelections = 3,
                IsRequired = false,
                MinPrice = 2.00m,
                MaxPrice = 5.00m,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Name = "Granola",
                Description = "Granolas e cereais",
                ImageUrl = "https://example.com/addon-categories/granola.jpg",
                Color = "#FFD93D",
                Icon = "🌾",
                IsActive = true,
                DisplayOrder = 2,
                MaxSelections = 2,
                IsRequired = false,
                MinPrice = 1.50m,
                MaxPrice = 3.00m,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Name = "Adoçantes",
                Description = "Adoçantes e caldas",
                ImageUrl = "https://example.com/addon-categories/adoçantes.jpg",
                Color = "#FF8E53",
                Icon = "🍯",
                IsActive = true,
                DisplayOrder = 3,
                MaxSelections = 2,
                IsRequired = false,
                MinPrice = 1.00m,
                MaxPrice = 2.50m,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                Name = "Carnes",
                Description = "Carnes e proteínas",
                ImageUrl = "https://example.com/addon-categories/carnes.jpg",
                Color = "#8B4513",
                Icon = "🥩",
                IsActive = true,
                DisplayOrder = 4,
                MaxSelections = 2,
                IsRequired = false,
                MinPrice = 3.00m,
                MaxPrice = 6.00m,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                Name = "Queijos",
                Description = "Queijos especiais",
                ImageUrl = "https://example.com/addon-categories/queijos.jpg",
                Color = "#FFD700",
                Icon = "🧀",
                IsActive = true,
                DisplayOrder = 5,
                MaxSelections = 3,
                IsRequired = false,
                MinPrice = 2.00m,
                MaxPrice = 4.00m,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                Name = "Molhos",
                Description = "Molhos e temperos",
                ImageUrl = "https://example.com/addon-categories/molhos.jpg",
                Color = "#FF6347",
                Icon = "🌶️",
                IsActive = true,
                DisplayOrder = 6,
                MaxSelections = 2,
                IsRequired = false,
                MinPrice = 1.00m,
                MaxPrice = 3.00m,
                CreatedAt = DateTime.UtcNow
            }
        };

        modelBuilder.Entity<AddOnCategoryEntity>().HasData(addOnCategories);

        // AddOns
        var addOns = new List<ProductAddOnEntity>
        {
            // Frutas
            new()
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Banana",
                Description = "Banana fatiada",
                Price = 2.00m,
                IsAvailable = true,
                MaxQuantity = 3,
                AddOnCategoryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Name = "Morango",
                Description = "Morango fresco",
                Price = 3.00m,
                IsAvailable = true,
                MaxQuantity = 2,
                AddOnCategoryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Name = "Manga",
                Description = "Manga madura",
                Price = 2.50m,
                IsAvailable = true,
                MaxQuantity = 2,
                AddOnCategoryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                Name = "Kiwi",
                Description = "Kiwi fresco",
                Price = 3.50m,
                IsAvailable = true,
                MaxQuantity = 2,
                AddOnCategoryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                CreatedAt = DateTime.UtcNow
            },
            
            // Granola
            new()
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                Name = "Granola Tradicional",
                Description = "Granola crocante",
                Price = 1.50m,
                IsAvailable = true,
                MaxQuantity = 2,
                AddOnCategoryId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                Name = "Granola de Chocolate",
                Description = "Granola com chocolate",
                Price = 2.00m,
                IsAvailable = true,
                MaxQuantity = 2,
                AddOnCategoryId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                CreatedAt = DateTime.UtcNow
            },
            
            // Adoçantes
            new()
            {
                Id = Guid.Parse("11111111-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Leite Condensado",
                Description = "Leite condensado cremoso",
                Price = 1.00m,
                IsAvailable = true,
                MaxQuantity = 1,
                AddOnCategoryId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("22222222-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Name = "Mel",
                Description = "Mel natural",
                Price = 1.50m,
                IsAvailable = true,
                MaxQuantity = 1,
                AddOnCategoryId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                CreatedAt = DateTime.UtcNow
            },
            
            // Carnes
            new()
            {
                Id = Guid.Parse("33333333-cccc-cccc-cccc-cccccccccccc"),
                Name = "Bacon",
                Description = "Bacon crocante",
                Price = 4.00m,
                IsAvailable = true,
                MaxQuantity = 2,
                AddOnCategoryId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("44444444-dddd-dddd-dddd-dddddddddddd"),
                Name = "Frango",
                Description = "Frango grelhado",
                Price = 3.50m,
                IsAvailable = true,
                MaxQuantity = 2,
                AddOnCategoryId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                CreatedAt = DateTime.UtcNow
            },
            
            // Queijos
            new()
            {
                Id = Guid.Parse("55555555-eeee-eeee-eeee-eeeeeeeeeeee"),
                Name = "Queijo Cheddar",
                Description = "Queijo cheddar",
                Price = 2.00m,
                IsAvailable = true,
                MaxQuantity = 3,
                AddOnCategoryId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("66666666-ffff-ffff-ffff-ffffffffffff"),
                Name = "Queijo Gorgonzola",
                Description = "Queijo gorgonzola",
                Price = 3.00m,
                IsAvailable = true,
                MaxQuantity = 2,
                AddOnCategoryId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                CreatedAt = DateTime.UtcNow
            },
            
            // Molhos
            new()
            {
                Id = Guid.Parse("77777777-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Molho Especial",
                Description = "Molho da casa",
                Price = 1.50m,
                IsAvailable = true,
                MaxQuantity = 2,
                AddOnCategoryId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("88888888-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Name = "Molho Picante",
                Description = "Molho picante",
                Price = 2.00m,
                IsAvailable = true,
                MaxQuantity = 1,
                AddOnCategoryId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                CreatedAt = DateTime.UtcNow
            }
        };

        modelBuilder.Entity<ProductAddOnEntity>().HasData(addOns);

        // Product AddOn Categories (relacionamentos)
        var productAddOnCategories = new List<ProductAddOnCategoryEntity>
        {
            // Açaí Tradicional
            new()
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                AddOnCategoryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), // Frutas
                MaxSelections = 3,
                IsRequired = false,
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                AddOnCategoryId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), // Granola
                MaxSelections = 2,
                IsRequired = false,
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                AddOnCategoryId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), // Adoçantes
                MaxSelections = 2,
                IsRequired = false,
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            
            // Açaí Especial
            new()
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                ProductId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                AddOnCategoryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), // Frutas
                MaxSelections = 4,
                IsRequired = false,
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                ProductId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                AddOnCategoryId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), // Granola
                MaxSelections = 2,
                IsRequired = false,
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            
            // X-Burger
            new()
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                ProductId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                AddOnCategoryId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), // Carnes
                MaxSelections = 2,
                IsRequired = false,
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("11111111-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                ProductId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                AddOnCategoryId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), // Queijos
                MaxSelections = 3,
                IsRequired = false,
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("22222222-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                ProductId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                AddOnCategoryId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), // Molhos
                MaxSelections = 2,
                IsRequired = false,
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            
            // X-Bacon
            new()
            {
                Id = Guid.Parse("33333333-cccc-cccc-cccc-cccccccccccc"),
                ProductId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                AddOnCategoryId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), // Carnes
                MaxSelections = 2,
                IsRequired = false,
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("44444444-dddd-dddd-dddd-dddddddddddd"),
                ProductId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                AddOnCategoryId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), // Queijos
                MaxSelections = 3,
                IsRequired = false,
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("55555555-eeee-eeee-eeee-eeeeeeeeeeee"),
                ProductId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                AddOnCategoryId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), // Molhos
                MaxSelections = 2,
                IsRequired = false,
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        modelBuilder.Entity<ProductAddOnCategoryEntity>().HasData(productAddOnCategories);
    }
} 