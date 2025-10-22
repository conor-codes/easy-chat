using Microsoft.AspNetCore.Identity;

namespace EasyChat.Api.Services.Interfaces
{
    public interface ITokenService
    {
        public string GenerateToken(IdentityUser user);
    }
}
