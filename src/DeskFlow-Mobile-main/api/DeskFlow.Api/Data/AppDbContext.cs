using DeskFlow.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Salon> Salonlar { get; set; }
    public DbSet<Masa> Masalar { get; set; }
}
