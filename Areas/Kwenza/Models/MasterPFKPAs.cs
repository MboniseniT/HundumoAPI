using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.Kwenza.Models
{
    public class MasterPFKPAs
    {
        //KPA Id
        public int value { get; set; }
        //KPA Name
        public string label { get; set; }
        //Value
        public int amount { get; set; }
    }
}
