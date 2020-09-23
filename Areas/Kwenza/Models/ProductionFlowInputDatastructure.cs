using BinmakBackEnd.Areas.Kwenza.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.Kwenza.Models
{
    public class ProductionFlowInputDatastructure
    {
        public Process Process { get; set; }
        public List<KeyProcessArea> KeyProcessAreas { get; set; }
        //public List<KPApfVM> KPApfVM { get; set; }
    }

    public class ProductionFlowInputDatastructureMod
    {
        public Process Process { get; set; }
        public List<KPApfVM> KeyProcessAreas { get; set; }
        public List<string> ProductionDates { get; set; }
        public string AssetName { get; set; }
    }

    public class ProductionFlowInputDatastructureModMainWrapper
    {
        public string ParentAssetName { get; set; }
        public string AssetName { get; set; }
        public List<ProductionFlowInputDatastructureMod> ProductionFlowInputDatastructureMods { get; set; }
    }

}
