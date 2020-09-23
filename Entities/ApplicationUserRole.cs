using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Entities
{
    public class ApplicationUserRole : IdentityRole
    {
        public int AssetId { get; set; }
        public DateTime DateStamp { get; set; }
        public string AddedBy { get; set; }
        public DateTime LastChangedDateStamp { get; set; }
        public string LastChangedAddedBy { get; set; }
    }
}
