using Microsoft.EntityFrameworkCore;
using Quarzt.Entities;

namespace Quarzt.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}