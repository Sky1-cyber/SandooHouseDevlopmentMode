using Microsoft.EntityFrameworkCore;
using Sandoohouse.Models;

namespace Sandoohouse.ApplicationProgram;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Shift> Shifts { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>()
            .HasOne(c => c.Brand)
            .WithMany(b => b.Categories)
            .HasForeignKey(c => c.BrandId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}