using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using User.Management.Service.Models;
using User.Management.Service.Models.Authentication.User;
using usermangment.API.Models.Authentication.Login;
using usermangment.Data.Models;
using usermangment.Service.Models.Authentication.SIgnUp;

namespace User.Management.Service.Services
{
    public interface IUserManagment
    {
        Task<ApiResponse<CreateUserResponse>> CreateUserWithTokenAsync(RegisterUser registerUser);
        Task<ApiResponse<List<string>>> AssignRoleToUserAsync(List<string> roles, AplicationUser user);
        Task<ApiResponse<LoginOtpResponse>> GetOtpByLoginAsync(LoginModel loginModel);
        Task<ApiResponse<LoginResponse>> GetJwtTokenAsync(AplicationUser user);
        Task<ApiResponse<LoginResponse>> LoginUserWIthJwtTokenAsync(string otp, string email);
        Task<ApiResponse<LoginResponse>> RenewAccessTokenAsync(LoginResponse tokens);







    }
}
