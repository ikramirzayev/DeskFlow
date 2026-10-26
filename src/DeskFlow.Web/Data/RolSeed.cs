using DeskFlow.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace DeskFlow.Web.Data;

public static class RolSeed
{
    public static async Task CalistirAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var rolYoneticisi = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var kullaniciYoneticisi = scope.ServiceProvider.GetRequiredService<UserManager<Kullanici>>();
        var ayarlar = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        foreach (var rol in Roller.Tumu)
        {
            if (!await rolYoneticisi.RoleExistsAsync(rol))
            {
                await rolYoneticisi.CreateAsync(new IdentityRole(rol));
            }
        }

        var email = ayarlar["VarsayilanYonetici:Email"];
        var sifre = ayarlar["VarsayilanYonetici:Sifre"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(sifre))
        {
            return;
        }

        if (await kullaniciYoneticisi.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var yonetici = new Kullanici
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            Ad = "Sistem",
            Soyad = "Yöneticisi"
        };

        var sonuc = await kullaniciYoneticisi.CreateAsync(yonetici, sifre);
        if (sonuc.Succeeded)
        {
            await kullaniciYoneticisi.AddToRoleAsync(yonetici, Roller.Yonetici);
        }
    }
}
