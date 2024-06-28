using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using User.Management.Service.Models;
using User.Management.Service.Models.Authentication.User;
using usermangment.API.Models.Authentication.Login;
using usermangment.Data.Models;
using usermangment.Service.Models.Authentication.SIgnUp;

namespace User.Management.Service.Services
{
    public class UserManagment : IUserManagment
    {
        private readonly UserManager<AplicationUser> _userManager;
        private readonly SignInManager<AplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public UserManagment(UserManager<AplicationUser> userManager,
                                        SignInManager<AplicationUser> signInManager,
                                        RoleManager<IdentityRole> roleManager,
                                        IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;

        }
        public async Task<ApiResponse<List<string>>> AssignRoleToUserAsync(List<string> roles, AplicationUser user)
        {
            var assignedRole = new List<string>();
            foreach (var role in roles)
            {
                if (await _roleManager.RoleExistsAsync(role))
                {
                    if (!await _userManager.IsInRoleAsync(user, role))
                    {
                        await _userManager.AddToRoleAsync(user, role);
                        assignedRole.Add(role);

                    }

                }

            }
            return new ApiResponse<List<string>> { IsSuccess = true, StatusCode = 200, Message = "Roles has been assigned", Response = assignedRole};
        }
        public async Task<ApiResponse<CreateUserResponse>> CreateUserWithTokenAsync(RegisterUser registerUser)
        {
            //check user exist
            var userExist = await _userManager.FindByEmailAsync(registerUser.Email);
            if (userExist != null)
            {
                return new ApiResponse<CreateUserResponse> { IsSuccess = false,  StatusCode = 403, Message = "User already exists!" };
            }

            //Add user in the database

            AplicationUser user = new()
            {
                Email = registerUser.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerUser.UserName,
                TwoFactorEnabled = true,
                LastName = registerUser.LastName,
                PersonalId = registerUser.PersonId,

                Category = registerUser.Category,
                PhotoPath = registerUser.PhotoPath,
                ReviewCount = registerUser.ReviewCount,
                Rating = registerUser.Rating,

            };
           
            var result = await _userManager.CreateAsync(user, registerUser.Password);

            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                return new ApiResponse<CreateUserResponse> { Response =new CreateUserResponse() { User = user, Token = token},IsSuccess = true, StatusCode = 201, Message =  "User created" };
            }
            else
            {
                return new ApiResponse<CreateUserResponse> { IsSuccess = false, StatusCode = 500, Message =  "User failed to create" };

            }

        }
        public async Task<ApiResponse<LoginOtpResponse>> GetOtpByLoginAsync(LoginModel loginModel)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.Email);

            if (user != null)
            {
               // await _signInManager.SignOutAsync();
               // await _signInManager.PasswordSignInAsync(user, loginModel.Password, false, true);

                if (user.TwoFactorEnabled)
                {
                    var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

                    return new ApiResponse<LoginOtpResponse>
                    {
                        Response = new LoginOtpResponse()
                        {
                            User =user,
                            Token  = token,
                            IsTwoFactorEnable = user.TwoFactorEnabled
                        },
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = $"OTP send to the email {user.Email}"
                    };

                }
                else
                {
                    return new ApiResponse<LoginOtpResponse>
                    {
                        Response = new LoginOtpResponse()
                        {
                            User =user,
                            Token  = string.Empty,
                            IsTwoFactorEnable = user.TwoFactorEnabled
                        },
                        IsSuccess = false,
                        StatusCode = 404,
                        Message = $"2FA is not enabled"
                    };

                }


            }
            else
            {
                return new ApiResponse<LoginOtpResponse>
                {
                    IsSuccess = true,
                    StatusCode = 404,
                    Message = $"User does not exist."
                };
            }


        }
        //
        public async Task<ApiResponse<LoginResponse>> GetJwtTokenAsync(AplicationUser user)
        {
            //ტოკენში დებს user-ის სახელს 
           //claimlist creation 
           var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

            //we add roles to the list
            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }
            //generate the token with the claims...
            var jwtToken = GetToken(authClaims);//Access token
            var refreshToken = GenerateRefreshToken();
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidity"], out int RefreshTokenValidity);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(RefreshTokenValidity);

            await _userManager.UpdateAsync(user);

            //returning the token...
            return new ApiResponse<LoginResponse>()
            {
                Response = new LoginResponse() 
                {
                    AccessToken = new TokenType()
                    {
                      Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                      ExpiryTokenDate = jwtToken.ValidTo
                    },
                    RefreshToken = new TokenType()
                    {
                        Token = user.RefreshToken,
                        ExpiryTokenDate = (DateTime) user.RefreshTokenExpiry
                    }
                },
                IsSuccess = true,
                StatusCode = 200,
                Message = "Token created"
            };     
    }
        public async Task<ApiResponse<LoginResponse>> LoginUserWIthJwtTokenAsync(string otp, string email)
        {

            var user = await _userManager.FindByEmailAsync(email);
            var signIn = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", otp);
            if (signIn)
            {
                if (user != null)
                {
                    return await GetJwtTokenAsync(user);
                }
            }
            return new ApiResponse<LoginResponse>()
            {
                Response = new LoginResponse()
                {

                },
                IsSuccess = false,
                StatusCode = 400,
                Message = "Invalid Otp"
            };

        }
        public async Task<ApiResponse<LoginResponse>> RenewAccessTokenAsync(LoginResponse tokens)
        {
            var accessToken = tokens.AccessToken;
            var refreshToken = tokens.RefreshToken; 
            var principal = GetClaimsPrincipal(accessToken.Token);
            var user = await  _userManager.FindByNameAsync(principal.Identity.Name);
            if(refreshToken.Token != user.RefreshToken && refreshToken.ExpiryTokenDate >= DateTime.Now)
            {
                return new ApiResponse<LoginResponse>()
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Bad request"
                };
            }
            var response = await GetJwtTokenAsync(user);
            return response;

        }


        #region PrivateMethods
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            _ = int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);
            var expirationTimeUtc = DateTime.UtcNow.AddMinutes(tokenValidityInMinutes);
            var localTimeZone = TimeZoneInfo.Local;
            var expirationTimeInLocalTimeZone = TimeZoneInfo.ConvertTimeFromUtc(expirationTimeUtc, localTimeZone);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: expirationTimeInLocalTimeZone,
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new Byte[64];
            var range = RandomNumberGenerator.Create();
            range.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal GetClaimsPrincipal(string accessToken)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);
            
            return principal;
        }
        #endregion

    }

}
