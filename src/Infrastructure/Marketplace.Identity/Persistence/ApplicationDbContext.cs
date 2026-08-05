using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Identity.Persistence;

public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Coupon> Coupons => Set<Coupon>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(u => u.Id);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
            builder.Property(u => u.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Vendor>(builder =>
        {
            builder.HasKey(v => v.Id);
            builder.Property(v => v.ShopNameEn).HasMaxLength(200).IsRequired();
            builder.Property(v => v.CommissionRate).HasPrecision(18, 4);
            builder.HasOne(v => v.User).WithMany().HasForeignKey(v => v.UserId);
        });

        modelBuilder.Entity<Payment>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Amount).HasPrecision(18, 2);
            builder.Property(p => p.PlatformFee).HasPrecision(18, 2);
            builder.Property(p => p.VendorAmount).HasPrecision(18, 2);
            builder.HasOne(p => p.Order).WithMany().HasForeignKey(p => p.OrderId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Coupon>(builder =>
        {
            builder.HasKey(c => c.Id);
            builder.HasIndex(c => c.Code).IsUnique();
            builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
            builder.Property(c => c.DiscountPercent).HasPrecision(18, 2);
            builder.Property(c => c.DiscountAmount).HasPrecision(18, 2);
            builder.Property(c => c.MinOrderAmount).HasPrecision(18, 2);
            builder.Property(c => c.MaxDiscountAmount).HasPrecision(18, 2);

            builder.HasData(
                new { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Code = "NOORZAI20", DiscountPercent = 20.00m, DiscountAmount = 0m, IsPercentage = true, MinOrderAmount = 10.00m, MaxDiscountAmount = 200.00m, ExpiryDate = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc), UsageLimit = 5000, UsedCount = 0, IsActive = true, VendorId = (Guid?)null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), Code = "WELCOME10", DiscountPercent = 0m, DiscountAmount = 10.00m, IsPercentage = false, MinOrderAmount = 20.00m, MaxDiscountAmount = 10.00m, ExpiryDate = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc), UsageLimit = 5000, UsedCount = 0, IsActive = true, VendorId = (Guid?)null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        });
        modelBuilder.Entity<Role>(builder =>
        {
            builder.HasKey(r => r.Id);
            builder.HasIndex(r => r.Name).IsUnique();
            builder.Property(r => r.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Permission>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.HasIndex(p => p.Code).IsUnique();
            builder.Property(p => p.Code).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<UserRole>(builder =>
        {
            builder.HasKey(ur => new { ur.UserId, ur.RoleId });
            builder.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
            builder.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
        });

        modelBuilder.Entity<RolePermission>(builder =>
        {
            builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            builder.HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId);
            builder.HasOne(rp => rp.Permission).WithMany(p => p.RolePermissions).HasForeignKey(rp => rp.PermissionId);
        });

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.HasKey(t => t.Id);
            builder.HasIndex(t => t.Token).IsUnique();
            builder.HasOne(t => t.User).WithMany(u => u.RefreshTokens).HasForeignKey(t => t.UserId);
        });

        modelBuilder.Entity<AuditLog>(builder =>
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        });

        var catSpicesId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var catCarpetsId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var catClothingId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var catElectronicsId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        var adminUserId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var defaultVendorId = Guid.Parse("66666666-6666-6666-6666-666666666666");

        // Seed a default User and Vendor for existing products
        modelBuilder.Entity<User>().HasData(new
        {
            Id = adminUserId,
            FullName = "System Admin",
            Email = "admin@noorzai.com",
            PasswordHash = "$argon2id$v=19$m=65536,t=3,p=1$t3spA9wh4NUB1wk5kT9ejw$WIU+dzsDyvQ2XZcKoeWI3KMXvsMTCfQtZ1DrlWd8P4w",
            IsEmailConfirmed = true,
            IsTwoFactorEnabled = false,
            IsLockoutEnabled = true,
            AccessFailedCount = 0,
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<Vendor>().HasData(new
        {
            Id = defaultVendorId,
            UserId = adminUserId,
            ShopNameEn = "Noorzai Official",
            ShopNamePrs = "فروشگاه رسمی نورزی",
            ShopNamePs = "د نورزی رسمي پلورنځی",
            DescriptionEn = "Official products from Noorzai.",
            DescriptionPrs = "محصولات رسمی از طرف بازار نورزی.",
            DescriptionPs = "د نورزی بازار رسمي محصولات.",
            LogoUrl = "",
            BannerUrl = "",
            CommissionRate = 0.10m,
            IsVerified = true,
            Rating = 5.0,
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });


        modelBuilder.Entity<Category>(builder =>
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.NameEn).HasMaxLength(150).IsRequired();
            builder.Property(c => c.NamePrs).HasMaxLength(150).IsRequired();
            builder.Property(c => c.NamePs).HasMaxLength(150).IsRequired();

            builder.HasOne(c => c.Parent)
                   .WithMany(c => c.SubCategories)
                   .HasForeignKey(c => c.ParentId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Seed 3-Layer Category Tree
            var catAgriId = Guid.Parse("10000000-0000-0000-0000-000000000001");
            var catHandicraftsId = Guid.Parse("10000000-0000-0000-0000-000000000002");
            var catApparelId = Guid.Parse("10000000-0000-0000-0000-000000000003");

            var catSaffronGroup = Guid.Parse("20000000-0000-0000-0000-000000000001");
            var catNutsGroup = Guid.Parse("20000000-0000-0000-0000-000000000002");
            var catCarpetsGroup = Guid.Parse("20000000-0000-0000-0000-000000000003");

            var catSuperSargol = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var catPushalSaffron = Guid.Parse("11111111-1111-1111-1111-222222222222");
            var catSilkCarpet = Guid.Parse("22222222-2222-2222-2222-222222222222");

            builder.HasData(
                // Level 1: Main Categories
                new { Id = catAgriId, NameEn = "Agricultural & Dried Fruits", NamePrs = "محصولات کشاورزی و خشکبار", NamePs = "کرنیز او وچه میوه جات", IconName = "eco", ParentId = (Guid?)null, Level = 1, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = catHandicraftsId, NameEn = "Handicrafts & Rugs", NamePrs = "صنایع دستی و فرش", NamePs = "لاسي صنایع او غالۍ", IconName = "style", ParentId = (Guid?)null, Level = 1, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = catApparelId, NameEn = "Apparel & Textiles", NamePrs = "پوشاک و منسوجات", NamePs = "کالي او ټوکران", IconName = "checkroom", ParentId = (Guid?)null, Level = 1, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

                // Level 2: Sub Categories
                new { Id = catSaffronGroup, NameEn = "Saffron & Spices", NamePrs = "زعفران و ادویه‌جات اعلا", NamePs = "زعفران او اعلی مصالحې", IconName = "local_florist", ParentId = (Guid?)catAgriId, Level = 2, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = catNutsGroup, NameEn = "Nuts & Almonds", NamePrs = "خشکبار، پسته و بادام", NamePs = "وچه میوه، پسته او بادام", IconName = "grain", ParentId = (Guid?)catAgriId, Level = 2, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = catCarpetsGroup, NameEn = "Handwoven Carpets", NamePrs = "قالین‌های دستبافت افغانی", NamePs = "د افغانستان لاسي غالۍ", IconName = "square_foot", ParentId = (Guid?)catHandicraftsId, Level = 2, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

                // Level 3: Leaf Categories
                new { Id = catSuperSargol, NameEn = "Super Sargol Saffron", NamePrs = "زعفران ممتاز سرگل هرات", NamePs = "د هرات ممتاز سرګل زعفران", IconName = "star", ParentId = (Guid?)catSaffronGroup, Level = 3, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = catPushalSaffron, NameEn = "Export Pushal Saffron", NamePrs = "زعفران پوشال صادراتی", NamePs = "صادراتي پوښال زعفران", IconName = "verified", ParentId = (Guid?)catSaffronGroup, Level = 3, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = catSilkCarpet, NameEn = "Herat Silk Rugs", NamePrs = "قالیچه ابریشمی ممتاز هرات", NamePs = "د هرات ممتاز ورېښمینې غالۍ", IconName = "grade", ParentId = (Guid?)catCarpetsGroup, Level = 3, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        });

        modelBuilder.Entity<Product>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.TitleEn).HasMaxLength(250).IsRequired();
            builder.Property(p => p.Price).HasPrecision(18, 2);
            builder.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId);
            builder.HasOne(p => p.Vendor).WithMany(v => v.Products).HasForeignKey(p => p.VendorId).OnDelete(DeleteBehavior.Restrict);

            builder.HasData(
                new { Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"), TitleEn = "Herat Red Gold Premium Saffron 10g", TitlePrs = "زعفران ممتاز طلای سرخ هرات ۱۰ گرام", TitlePs = "د هرات ممتاز سور زر زعفران ۱۰ ګرامه", DescriptionEn = "100% pure organic super nagin saffron harvested from Herat fields.", DescriptionPrs = "زعفران ۱۰۰٪ طبیعی و خالص سوپر نگین برداشت شده از مزارع ولایت هرات.", DescriptionPs = "د هرات ولایت له کروندو څخه راټول شوي ۱۰۰٪ طبعي او خالص سوپر نګین زعفران.", Price = 45.00m, StockQuantity = 100, Rating = 4.9, ImageUrl = "https://images.unsplash.com/photo-1615485290382-441e4d049cb5?w=500", CategoryId = catSpicesId, VendorId = defaultVendorId, AvailableSizes = "5g,10g,25g", AvailableColors = "Red", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"), TitleEn = "Handcrafted Wool Silk Rug (1.5x2m)", TitlePrs = "قالین دستی ابریشمی پشمی هرات (۱.۵ در ۲ متر)", TitlePs = "د پشم او ورېښمو لاسي غالۍ (۱.۵ په ۲ متره)", DescriptionEn = "Authentic hand-knotted traditional Afghan carpet with intricate patterns.", DescriptionPrs = "قالین بافته‌شده با دست با نخ‌های ابریشمی و پشم طبیعی با نقش‌های اصیل عنعنوی.", DescriptionPs = "د وریښمنو او طبعي پشم تارونو څخه په لاس اوبدل شوې د اصیلو نقشو غالۍ.", Price = 280.00m, StockQuantity = 20, Rating = 4.8, ImageUrl = "https://images.unsplash.com/photo-1600121848594-d8644e57abab?w=500", CategoryId = catCarpetsId, VendorId = defaultVendorId, AvailableSizes = "1.5x2m,2x3m", AvailableColors = "Maroon,Navy,Gold", IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        });

        modelBuilder.Entity<Order>(builder =>
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
            builder.Property(o => o.TotalAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<OrderItem>(builder =>
        {
            builder.HasKey(i => i.Id);
            builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
            builder.HasOne(i => i.Order).WithMany(o => o.Items).HasForeignKey(i => i.OrderId);
            builder.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId);
            builder.HasOne<Vendor>().WithMany().HasForeignKey(i => i.VendorId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
