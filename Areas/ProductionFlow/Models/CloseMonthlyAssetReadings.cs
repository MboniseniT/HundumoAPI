using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.ProductionFlow.Models
{
    public class CloseMonthlyAssetReadings
    {
        public int AssetId { get; set; }
        public DateTime DateStamp { get; set; }
    }
}
