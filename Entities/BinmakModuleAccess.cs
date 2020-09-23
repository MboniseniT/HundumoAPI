using BinmakAPI.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Entities
{
    public class BinmakModuleAccess
    {
        public int BinmakModuleAccessId { get; set; }
        public int BinmakModuleId { get; set; }
        [ForeignKey("BinmakModuleId")]
        public BinmakModule BinmakModule { get; set; }
        public DateTime DateStamp { get; set; }
        public string Reference { get; set; }
        [ForeignKey("Id")]
        public ApplicationUser ApplicationUser { get; set; }
    }
}
