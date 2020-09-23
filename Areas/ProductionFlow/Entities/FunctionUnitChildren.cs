using BinmakBackEnd.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.ProductionFlow.Entities
{
    public class FunctionUnitChildren
    {
        public int FunctionUnitChildrenId { get; set; }
        public string FunctionUnitChildrenName { get; set; }
        public int FunctionUnitId { get; set; }
        public int AssetId { get; set; }
        [ForeignKey("AssetNodeId")]
        public AssetNode AssetNode { get; set; }
        public int ClientAssetNameId { get; set; }
        [ForeignKey("ClientAssetNameId")]
        public ClientAsset ClientAsset { get; set; }
        public string FunctionChildrenBachgroundColor { get; set; }
        public string FunctionChildrenColor { get; set; }
        public string Frequency { get; set; }
        public string MeasurementUnit { get; set; }

        public string MonthlyTarget { get; set; }
        public string MonthlyTargetColor { get; set; }
        public bool MonthlyTargetIsBackground { get; set; }

        public string Target { get; set; }
        public string TargetColor { get; set; }
        public bool TargetIsBackground { get; set; }

        public string Budget { get; set; }
        public string BudgetColor { get; set; }
        public bool BudgetIsBackground { get; set; }

        public string Threshold { get; set; }
        public string ThresholdColor { get; set; }
        public bool ThresholdIsBackground { get; set; }
    }
}
