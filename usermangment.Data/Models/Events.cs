using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace usermangment.Data.Models
{
    public class Events
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? EmailOfArtist { get; set; }
        public DateTime? DateAndTime { get; set; }

    }
}
