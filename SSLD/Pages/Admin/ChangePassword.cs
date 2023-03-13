using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Radzen;
using SSLD.Data;
using SSLD.Tools;

namespace SSLD.Pages.Admin;

[Authorize]
public partial class ChangePassword
{
    private ChangePasswordUser Pass { get; set; } = new ChangePasswordUser();
    [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
    [Inject] public AuthenticationStateProvider AuthProvider { get; set; }
    [Inject] public NavigationManager NavigationManager { get; set; }


    private async Task OnSubmit(ChangePasswordUser password)
    {
        var state = await AuthProvider.GetAuthenticationStateAsync();
        {
            var user = await UserManager.GetUserAsync(state.User);
            var result = await UserManager.ChangePasswordAsync(user, password.OldPassword, password.Password);
            if (result.Succeeded)
            {
                //await signInManager.PasswordSignInAsync(user.Email, password.NewPassword, true, lockoutOnFailure: false);
                NavigationManager.NavigateTo("/");
            }
        }
    }

    void OnInvalidSubmit(FormInvalidSubmitEventArgs args)
    {

    }
}