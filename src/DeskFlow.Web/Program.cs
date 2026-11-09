using DeskFlow.Web.Data;
using DeskFlow.Web.Models;
using DeskFlow.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(o =>
{
    // Pages/Admin altındaki bütün sayfalar sadece yöneticiye açık.
    o.Conventions.AuthorizeFolder("/Admin", "YoneticiPolitikasi");
});

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("YoneticiPolitikasi", p => p.RequireRole(Roller.Yonetici));
});

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<RezervasyonServisi>();

builder.Services
    .AddIdentity<Kullanici, IdentityRole>(o =>
    {
        o.Password.RequiredLength = 6;
        o.Password.RequireDigit = true;
        o.Password.RequireLowercase = true;
        o.Password.RequireUppercase = false;
        o.Password.RequireNonAlphanumeric = false;
        o.User.RequireUniqueEmail = true;
        o.Lockout.MaxFailedAccessAttempts = 5;
        o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddErrorDescriber<TurkceIdentityHatalari>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/GirisYap";
    o.LogoutPath = "/CikisYap";
    o.AccessDeniedPath = "/YetkisizErisim";
});

var app = builder.Build();

try
{
    await RolSeed.CalistirAsync(app.Services);
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Roller oluşturulamadı. Veritabanı hazır mı? (dotnet ef database update)");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
