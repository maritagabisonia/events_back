using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace usermangment.Service.Models.Authentication.SIgnUp
{
    public class RegisterUser 
    {

        [Required(ErrorMessage = "User Name is required")]
        public new string? UserName { get; set; }
        public string? LastName { get; set; }
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public new string? Email{ get; set;}
        public string? PersonId { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
        public List<string>? Roles { get; set; }

        public string? Category { get; set; }
        public string? PhotoPath { get; set; }
        public string? CvPath { get; set; }
        public string? ReviewCount { get; set; }
        public string? Rating { get; set; }
        public bool? Pinned { get; set; }




    }
}
