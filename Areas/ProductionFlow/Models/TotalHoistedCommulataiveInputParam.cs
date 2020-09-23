using BinmakBackEnd.Areas.ProductionFlow.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.ProductionFlow.Models
{
    public class TotalHoistedCommulataiveInputParam
    {
        public Reading Reading { get; set; }
        public List<ProductionFlowAsset> Asset { get; set; }

    }

    public class TotalHoistedTonsInputParam
    {
        public Reading Reading { get; set; }
        public List<ProductionFlowAsset> Asset { get; set; }
    }
}
