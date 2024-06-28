using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using usermangment.Service.Models.Authentication.SIgnUp;

namespace User.Management.Service.Models.Authentication.SIgnUp
{
    public  class RegisterDoctor:RegisterUser
    {
        public string? Category { get; set; }
        public string? PhotoPath { get; set; }
        public string? CvPath { get; set; }
        public string? ReviewCount { get; set; }
        public string? Rating { get; set; }
        public bool? Pinned { get; set; }
    }
}
