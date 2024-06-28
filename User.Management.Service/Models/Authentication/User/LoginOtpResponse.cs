

using Microsoft.AspNetCore.Identity;
using usermangment.Data.Models;

namespace User.Management.Service.Models.Authentication.User
{
    public class LoginOtpResponse
    {
        public string Token { get; set; } = null!;
        public bool IsTwoFactorEnable { get; set; }

        public AplicationUser User { get; set; } = null!;
    }
}
