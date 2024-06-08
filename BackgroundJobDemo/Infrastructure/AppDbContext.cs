using BackgroundJobDemo.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackgroundJobDemo.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}
