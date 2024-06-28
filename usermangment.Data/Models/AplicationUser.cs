using Microsoft.AspNetCore.Identity;
namespace usermangment.Data.Models
{
    public class AplicationUser : IdentityUser
    {
        public string? LastName { get; set; }
        public string? PersonalId { get; set; }
        public string? ActivationCode { get; set; }
        public string? Category {  get; set; }
        public string? PhotoPath { get; set; }
        public string? ReviewCount { get; set; }
        public string? Rating { get; set; }
        public string? RefreshToken {  get; set; }
        public DateTime RefreshTokenExpiry { get; set; }



    }
}
