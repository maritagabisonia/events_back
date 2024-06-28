using System.ComponentModel.DataAnnotations;

namespace usermangment.API.Models.Authentication.Login
{
    public class LoginModel
    {
        [Required(ErrorMessage = "User Email is required")]
        public string? Email { get; set; }


        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }
}
