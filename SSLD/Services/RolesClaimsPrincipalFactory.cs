using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SSLD.Data;
using System.Security.Claims;

namespace SSLD.Services;

public class RolesClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public RolesClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager
        , RoleManager<IdentityRole> roleManager
        , IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        //if (!string.IsNullOrWhiteSpace(user.CustomClaim))
        //{
        //    identity.AddClaim(new Claim("custom_claim", user.CustomClaim));
        //}
        return identity;
    }
}