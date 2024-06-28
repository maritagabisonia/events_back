using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using User.Management.Service.Models;
using usermangment.API.Models;
using User.Management.Service.Services;
using usermangment.API.Models.Authentication.Login;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using usermangment.Service.Models.Authentication.SIgnUp;
using usermangment.Data.Models;
using User.Management.Service.Models.Authentication.User;
using User.Management.Service.Models.Authentication.SIgnUp;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;




namespace usermangment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {

        private readonly UserManager<AplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IUserManagment _user;
        private readonly ApplicationDbContext _context;


        public AuthenticationController(UserManager<AplicationUser> userManager,
                                         IEmailService emailService,
                                         IUserManagment user,
                                         ApplicationDbContext context)
        {
            _userManager = userManager;
            _emailService = emailService; 
            _user = user;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser ([FromBody] RegisterUser registerUser)
        {

            var tokenResponse = await _user.CreateUserWithTokenAsync(registerUser);
            if (tokenResponse.IsSuccess)
            {
                //Add token to verify the email
                await _user.AssignRoleToUserAsync(registerUser.Roles, tokenResponse.Response.User);
                var confirmationLink = Url.Action(nameof(ConfirmEmail), "Authentication", new { tokenResponse.Response.Token, email = registerUser.Email }, Request.Scheme);
                var message = new Message(new string[] { registerUser.Email! }, "Confirmation email link", confirmationLink);
                _emailService.SendEmail(message);

                return StatusCode(StatusCodes.Status200OK,
                     new Response { Status = "Success", Message = "Email Verified Successfully" });


            }
            return StatusCode(StatusCodes.Status500InternalServerError,
                       new Response { Status = "Error", Message = tokenResponse.Message, IsSuccess = false });
        }

     
        /*  
          [HttpGet("TestEmail")]
          public IActionResult TestEmial()
          {
              var message = new Message(new string[] 
                            { "" }, "test", "hiiiiiiiiiii");
              _emailService.SendEmail(message);
              return StatusCode(StatusCodes.Status200OK,
                  new Response { Status = "Success", Message = "Email sent Successfully" });
          }
         */
        [HttpGet("ConfrimEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if(result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK,
                        new Response { Status = "Success", Message = "Email Verified Successfully" });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError,
                        new Response { Status = "Error", Message = "This user doesnt exist!" });
        }


        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login ([FromBody] LoginModel loginModel)
        {

            var loginOtpResponse = await _user.GetOtpByLoginAsync(loginModel);
            if(loginOtpResponse.Response != null)
            {
                var user = loginOtpResponse.Response.User;
                if (user.TwoFactorEnabled)
                {
                    var token = loginOtpResponse.Response.Token;
                    var message = new Message(new string[] { user.Email! }, "OTP Confirmation", token);
                    _emailService.SendEmail(message);


                    return StatusCode(StatusCodes.Status200OK,
                        new Response { IsSuccess = loginOtpResponse.IsSuccess, Status = "Success", Message = $"We have sent OTP to you Email{user.Email}!" });

                }

                if (user != null && await _userManager.CheckPasswordAsync(user, loginModel.Password))
                {
                    var serviceResponse = await _user.GetJwtTokenAsync(user);
                    return Ok(serviceResponse);
                }
            }
            return Unauthorized();
        }
        
        [HttpPost]
        [Route("emailVerificationCode")]
        public async Task<IActionResult> emailVerificationCode(string Email)
        {

            var message = new Message(new string[] { Email }, "OTP Confirmation");
            _emailService.SendEmail(message);
            return StatusCode(StatusCodes.Status200OK,
                new Response { Status = "Succes", Message = " Email sent"});
        }

       /* [HttpPost]
        [Route("CheckemailVerificationCode")]
        public async Task<IActionResult> CheckemailVerificationCode(string Email, string otp)
        {


            bool isValid = await _emailService.CheckEmailVerificationCode(Email, otp);
            if (isValid)
            {
                return Ok(new ApiResponse<LoginResponse>
                {
                    IsSuccess = true,
                    Message = "OTP is valid.",
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status404NotFound,
                   new Response { Status = "Error", Message = "Invalid OTP or OTP has expired." });
               
            }
        }
        */


        [HttpPost]
        [Route("Login-2Fa")]
        public async Task<IActionResult> LoginWithOTP( string otp, string email)
        {
            var jwt = await _user.LoginUserWIthJwtTokenAsync(otp, email);
            if (jwt.IsSuccess)
            {
                    return Ok(jwt);
               
            }
            return StatusCode(StatusCodes.Status404NotFound,
                      new Response { Status = "Error", Message = "Invalid code!" });
        }

        [HttpPost]
        [Route("Refresh-Token")]
        public async Task<IActionResult> RefreshToken(LoginResponse tokens)
        {
            var jwt = await _user.RenewAccessTokenAsync(tokens);
            if (jwt.IsSuccess)
            {
                return Ok(jwt);

            }
            return StatusCode(StatusCodes.Status404NotFound,
                      new Response { Status = "Error", Message = "Invalid code!" });
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword([Required] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user!=null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);   
                var forgotPasswordLink = Url.Action(nameof(ResetPassword), "Authentication", new { token, email = user.Email }, Request.Scheme);
                var message = new Message(new string[] { user.Email! }, "Forgot Password link", forgotPasswordLink);
                _emailService.SendEmail(message);
                return StatusCode(StatusCodes.Status200OK,
                    new Response { Status = "Success", Message = $"Password Chang request is sent on email{user.Email}. Please Open Your email and cklick on the link" });
            }
            return StatusCode(StatusCodes.Status400BadRequest,
                   new Response { Status = "Error", Message = $"Could not send link to email, please try again." });

        }


        [HttpGet("reset-password")]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            var model = new ResetPassword { Token = token, Email = email };

            return Ok(
                new
                {
                    model
                });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPassword resetPassword)
        {
            var user = await _userManager.FindByEmailAsync(resetPassword.Email);
            if (user!=null)
            {
                var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);
                if(!resetPassResult.Succeeded)
                {
                    foreach(var error in resetPassResult.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return Ok(ModelState);
                }
     
                return StatusCode(StatusCodes.Status200OK,
                    new Response { Status = "Success", Message = $"Password has been Changed " });
            }
            return StatusCode(StatusCodes.Status400BadRequest,
                   new Response { Status = "Error", Message = $"Could not send link to email, please try again." });

        }


      




    }
}
