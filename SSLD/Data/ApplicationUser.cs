using Microsoft.AspNetCore.Identity;

namespace SSLD.Data;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; }
    //public string? CustomClaim { get; set; }
}