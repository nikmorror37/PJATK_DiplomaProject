using BookingWepApp.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

public class CustomUserClaimsPrincipalFactory : IUserClaimsPrincipalFactory<ApplicationUser>
{
    private readonly IUserStore<ApplicationUser> _userStore;

    public CustomUserClaimsPrincipalFactory(IUserStore<ApplicationUser> userStore)
    {
        _userStore = userStore;
    }

    public async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var identity = new ClaimsIdentity("Identity.Application");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
        identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

        if (user.Roles != null)
        {
            foreach (var role in user.Roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }

        var principal = new ClaimsPrincipal(identity);
        return await Task.FromResult(principal);
    }
}
