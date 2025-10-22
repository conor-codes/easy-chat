using Microsoft.AspNetCore.Identity;

namespace EasyChat.Api.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(IdentityUser user);
    }
}
