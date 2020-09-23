using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakAPI.Models
{
    public class ApplicationUserVM
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateStamp { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int CountryId { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public int RoleId { get; set; }
        public bool IsLocked { get; set; }

    }
}
