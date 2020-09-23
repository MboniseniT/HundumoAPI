using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.Kwenza.Models
{
    public class AddPFStatVM
    {
        public int AssetNodeId { get; set; }
        public string ProductionDate { get; set; }
        public string Reference { get; set; }
        public List<MasterPFKPAs> ValueKPAs { get; set; }
    }
}
