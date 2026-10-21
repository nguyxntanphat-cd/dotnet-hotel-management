using Microsoft.EntityFrameworkCore;

namespace MyRazorApp.Models;

public class AppDBContext : DbContext
{
    public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();

    //Cách 3
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---------- Category ----------
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(c => c.CategoryId);

            entity.Property(c => c.CategoryName)
                  .IsRequired()
                  .HasMaxLength(100);
        });

        // ---------- Product ----------
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.ProductId);

            entity.Property(p => p.ProductName)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(p => p.Price)
                  .HasPrecision(18, 2);

            entity.Ignore(p => p.PriceText);   // không tạo cột trong DB

            // Quan hệ: 1 Category - nhiều Product
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);   // xoá loại => CategoryId của sản phẩm về NULL
        });

        // ---------- Order ----------
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.OrderId);

            entity.Property(o => o.CustomerName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(o => o.OrderDate)
                  .HasDefaultValueSql("GETDATE()");   // giá trị mặc định phía SQL Server

            entity.Property(o => o.Status)
                  .HasMaxLength(20)
                  .HasDefaultValue("Pending");
        });

        // ---------- OrderDetail ----------
        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.ToTable("OrderDetails");
            entity.HasKey(d => d.OrderDetailId);

            entity.Property(d => d.UnitPrice)
                  .HasPrecision(18, 2);

            // Quan hệ: 1 Order - nhiều OrderDetail
            entity.HasOne(d => d.Order)
                  .WithMany(o => o.OrderDetails)
                  .HasForeignKey(d => d.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);    // xoá đơn => xoá luôn chi tiết

            // Quan hệ: 1 Product - nhiều OrderDetail
            entity.HasOne(d => d.Product)
                  .WithMany(p => p.OrderDetails)
                  .HasForeignKey(d => d.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);   // không cho xoá sản phẩm đã từng được bán

            // Một sản phẩm chỉ xuất hiện 1 lần trong cùng 1 đơn
            entity.HasIndex(d => new { d.OrderId, d.ProductId })
                  .IsUnique();
        });
    }

    //Cách 1 + 2
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSeeding((context, _) =>
        {
            if (context.Set<Category>().Any()) return;

            var dienThoai = new Category { CategoryName = "Điện thoại" };
            var laptop = new Category { CategoryName = "Laptop" };
            context.Set<Category>().AddRange(dienThoai, laptop);

            context.Set<Product>().AddRange(
                new Product { ProductName = "iPhone 15", Price = 22000000m, Description = "Điện thoại Apple", Category = dienThoai },
                new Product { ProductName = "Galaxy S24", Price = 20000000m, Description = "Điện thoại Samsung", Category = dienThoai },
                new Product { ProductName = "MacBook Air M2", Price = 28000000m, Category = laptop },
                new Product { ProductName = "Dell XPS 13", Price = 30000000m, Category = laptop }
            );

            context.SaveChanges();
        });
    }


}