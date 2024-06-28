
using Microsoft.AspNetCore.Identity;
using usermangment.Data.Models;

namespace User.Management.Service.Models.Authentication.User
{
    public class CreateUserResponse
    {
        public string? Token {  get; set; }
        public AplicationUser User { get; set; }

    }
}
