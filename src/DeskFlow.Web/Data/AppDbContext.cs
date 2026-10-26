using DeskFlow.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeskFlow.Web.Data;

public class AppDbContext : IdentityDbContext<Kullanici>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Salon> Salonlar { get; set; }
    public DbSet<Masa> Masalar { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Identity tablolarının (AspNetUsers, AspNetRoles...) ayarları için ilk satır şart.
        base.OnModelCreating(builder);
    }
}
