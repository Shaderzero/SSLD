using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Radzen;
using Radzen.Blazor;
using SSLD.Data;
using SSLD.Tools;

namespace SSLD.Pages.Admin;

[Authorize(Roles = SD.Role_Admin)]
public partial class PageAccount
{
    [Inject] public NavigationManager NavigationManager { get; set; }
    [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
    [Inject] public RoleManager<IdentityRole> RoleManager { get; set; }
    [Inject] public NotificationService NotificationService { get; set; }
    [Inject] public DialogService DialogService { get; set; }
    [Inject] public IConfiguration Configuration { get; set; }
    // [Inject] public IMailService MailService { get; set; }
    private readonly IEnumerable<int> _pageSizeOptions = new[] { 10, 35, 100 };
    private IList<User> _users;
    private IList<string> _roles;
    private RadzenDataGrid<User> _usersGrid;
    private const string GeneratedPassword = "gpedefaultpassword";

    protected override async Task OnInitializedAsync()
    {
        await ReloadUsers();
    }

    private async Task ReloadUsers()
    {
        var identityRoles = await RoleManager.Roles.ToListAsync();
        _roles = new List<string>();
        foreach (var role in identityRoles.Where(role => role.Name is SD.Role_Admin or SD.Role_User))
        {
            _roles.Add(role.Name);
        }
        _users = new List<User>();
        var users = await UserManager.Users
            .OrderBy(x => x.Name)
            .ToListAsync();

        foreach (var user in users)
        {
            var u = await FillUserWithRoles(user);
            _users.Add(u);
        }
    }

    private async Task<User> FillUserWithRoles(ApplicationUser appUser)
    {
        var u = new User()
        {
            Id = appUser.Id,
            Name = appUser.Name,
            Email = appUser.Email
        };
        foreach (var role in _roles)
        {
            u.Roles.Add(new Role()
            {
                Name = role,
                IsSelected = await UserManager.IsInRoleAsync(appUser, role)
            });
        }

        return u;
    }
    
    private void InsertRow()
    {
        var user = new User();
        foreach (var role in _roles)
        {
            user.Roles.Add(new Role()
            {
                Name = role,
                IsSelected = false
            });
        }
        _usersGrid.InsertRow(user);
    }

    private async Task OnCreateRow(User user)
    {
        var isUnique = UserManager.Users
            .Select(x => new User()
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email
            })
            .FirstOrDefault(x => x.Name == user.Name || x.Email == user.Email);
        if (isUnique != null)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Пользователь уже существует",
                Detail = "Пользователь с именем" + user.Name + " и почтой " + user.Email + " уже существует",
                Duration = 3000
            });
            return;
        }
        //var generatedPassword = GenerateRandomPassword();
        var newUser = new ApplicationUser()
        {
            UserName = user.Email,
            Email = user.Email,
            Name = user.Name,
            EmailConfirmed = true
        };
        var result = await UserManager.CreateAsync(newUser, GeneratedPassword);
        if (result != null)
        {
            var roles = (from role in user.Roles where role.IsSelected select role.Name).ToList();
            await UserManager.AddToRolesAsync(newUser, roles);
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Создан новый пользователь",
                Detail = "Пользователь " + user.Name + " успешно создан",
                Duration = 3000
            });
            await ReloadUsers();
            // var site = Configuration.GetValue<string>("SiteAddress");
            // var link = site + "/loginwa?user=" + user.Email + "&p=" + GeneratedPassword;
            // var htmlMessage = "Have been created account for <a href='" + HtmlEncoder.Default.Encode(site) + "'>Risk Suite</a>.<br/>" +
            // "Username: " + user.Email + "<br/>" +
            // "Password: " + GeneratedPassword + "<br/>" +
            // "You can login by <a href='" + HtmlEncoder.Default.Encode(link) + "'>clicking here</a>.";
            // await MailService.SendEmailAsync(user.Email, "Link for login", htmlMessage);
        }
        else
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка создания пользователя",
                Detail = "Не удалось создать пользователя " + user.Name,
                Duration = 3000
            });
        }
    }

    private async Task SetPassword(User user)
    {
        var confirmation = await DialogService.OpenAsync<ChangePasswordModal>($"Сменить пароль",
            new Dictionary<string, object>() { { "User", user } },
            new DialogOptions() { Resizable = true, Draggable = true });
        if (confirmation != null)
        {
            var appUser = await UserManager.FindByIdAsync(user.Id);
            appUser.PasswordHash = UserManager.PasswordHasher.HashPassword(appUser, confirmation);
            var result = await UserManager.UpdateAsync(appUser);
            if (result.Succeeded)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Пароль сброшен",
                    Detail = "Пароль пользователя " + user.Name + " успешно сброшен",
                    Duration = 3000
                });
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Ошибка создания пользователя",
                    Detail = result.Errors?.ToString(),
                    Duration = 3000
                });
            }
        }
    }

    private async Task DeleteRow(User user)
    {
        var confirmation = await DialogService.Confirm($"Вы уверены, что хотите удалить пользователя {user.Name}?", "Удаление",
            new ConfirmOptions() { OkButtonText = "Да", CancelButtonText = "Отмена" });
        if (confirmation != true) return;
        var appUser = UserManager.Users.FirstOrDefault(x => x.Email == user.Email);
        if (appUser != null)
        {
            var result = await UserManager.DeleteAsync(appUser);
            if (result.Succeeded)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Пользователь удален",
                    Detail = "Пользователь " + user.Name + " удален",
                    Duration = 3000
                });
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Ошибка удаления пользователя",
                    Detail = result.Errors.FirstOrDefault()?.Description,
                    Duration = 3000
                });
            }
            _users.Remove(user);
            await _usersGrid.Reload();
        }
        else
        {
            _usersGrid.CancelEditRow(user);
        }
    }

    private void EditRow(User user)
    {
        _usersGrid.EditRow(user);
    }

    private async Task OnUpdateRow(User user)
    {
        var userFromDb = await UserManager.FindByIdAsync(user.Id);
        if (userFromDb == null)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Пользователь не найден",
                Detail = "В базе данных не найден редактируемый пользователь",
                Duration = 3000
            });
            return;
        }
        var existingRoles = await UserManager.GetRolesAsync(userFromDb);
        var rolesToDelete = user.Roles
            .Where(r => r.IsSelected == false && existingRoles.Any(x => x == r.Name))
            .Select(x => x.Name)
            .ToList();
        var rolesToAdd = user.Roles
            .Where(r => r.IsSelected && existingRoles.All(x => x != r.Name))
            .Select(x => x.Name)
            .ToList();
        try
        {
            await UserManager.RemoveFromRolesAsync(userFromDb, rolesToDelete);
            await UserManager.AddToRolesAsync(userFromDb, rolesToAdd);
            userFromDb.Email = user.Email;
            userFromDb.Name = user.Name;
            var result = await UserManager.UpdateAsync(userFromDb);
            if (result.Succeeded)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Пользователь обновлён",
                    Detail = "Пользователь " + user.Name + " обновлён",
                    Duration = 3000
                });
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Ошибка обновления пользователя",
                    Detail = result.Errors.FirstOrDefault()?.Description,
                    Duration = 3000
                });
            }
            await _usersGrid.Reload();
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Ошибка обновления пользователя",
                Detail = ex.Message,
                Duration = 3000
            });
        }
    }

    private async Task SaveRow(User user)
    {
        await _usersGrid.UpdateRow(user);
    }

    private void CancelEdit(User user)
    {
        _usersGrid.CancelEditRow(user);
    }

    private static string GenerateRandomPassword(PasswordOptions opts = null)
    {
        opts ??= new PasswordOptions()
        {
            RequiredLength = 10,
            RequiredUniqueChars = 4,
            RequireDigit = true,
            RequireLowercase = true,
            RequireNonAlphanumeric = true,
            RequireUppercase = true
        };

        var randomChars = new[] {
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",    // uppercase 
            "abcdefghijklmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "!@$?_-"                        // non-alphanumeric
        };
        var rand = new Random(Environment.TickCount);
        var chars = new List<char>();

        if (opts.RequireUppercase)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[0][rand.Next(0, randomChars[0].Length)]);

        if (opts.RequireLowercase)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[1][rand.Next(0, randomChars[1].Length)]);

        if (opts.RequireDigit)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[2][rand.Next(0, randomChars[2].Length)]);

        if (opts.RequireNonAlphanumeric)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[3][rand.Next(0, randomChars[3].Length)]);

        for (var i = chars.Count; i < opts.RequiredLength
                                  || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
        {
            var rcs = randomChars[rand.Next(0, randomChars.Length)];
            chars.Insert(rand.Next(0, chars.Count),
                rcs[rand.Next(0, rcs.Length)]);
        }

        return new string(chars.ToArray());
    }
}