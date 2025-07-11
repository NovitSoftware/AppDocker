using AcademiaNovit.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademiaNovit;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Product> Products { get; set; }
}