using Microsoft.EntityFrameworkCore;
using Sandoohouse.Models;

namespace Sandoohouse.ApplicationProgram;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    
    public DbSet<Admin> Admins { get; set; }
}