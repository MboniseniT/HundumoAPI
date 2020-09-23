using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.ProductionFlow.Models
{
    public class LimitObject
    {
        public DateTime ProductionDate { get; set; }
        public int AssetId { get; set; }
    }
}
