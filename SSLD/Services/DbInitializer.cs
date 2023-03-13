using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SSLD.Data;
using SSLD.Tools;

namespace SSLD.Services;

public class DbInitializer : IDbInitializer
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DbInitializer(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public void Initialize()
    {
        try
        {
            if (_db.Database.GetPendingMigrations().Any())
            {
                _db.Database.Migrate();
            }
        }
        catch (Exception)
        {

        }

        if (_db.Roles.Any(x => x.Name == SD.Role_Admin)) return;

        _db.SaveChanges();


        _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
        _roleManager.CreateAsync(new IdentityRole(SD.Role_User)).GetAwaiter().GetResult();
        _roleManager.CreateAsync(new IdentityRole(SD.Role_Power_User)).GetAwaiter().GetResult();
        _roleManager.CreateAsync(new IdentityRole(SD.Role_Front_Office)).GetAwaiter().GetResult();
        _roleManager.CreateAsync(new IdentityRole(SD.Role_Security)).GetAwaiter().GetResult();

        _userManager.CreateAsync(new ApplicationUser
        {
            UserName = "lostuser@lostuser.ru",
            Name = "lostuser@lostuser.ru",
            Email = "lostuser@lostuser.ru",
            EmailConfirmed = true
        }, "*******").GetAwaiter().GetResult();

        _db.SaveChanges();
        var user = _userManager.FindByEmailAsync("lostuser@lostuser.ru").GetAwaiter().GetResult();
        _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
    }
}