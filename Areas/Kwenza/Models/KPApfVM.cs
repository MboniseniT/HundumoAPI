using BinmakBackEnd.Areas.Kwenza.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.Kwenza.Models
{
    public class KPApfVM
    {
        public int KeyProcessAreaId { get; set; }
        public int AssetNodeId { get; set; }
        public int ProcessId { get; set; }
        public string Frequency { get; set; }
        public string ProcessName { get; set; }
        public virtual Process Process { get; set; }
        public string KeyProcessAreaName { get; set; }
        public string Color { get; set; }
        public bool IsTargetSet { get; set; }
        public DateTime KPADate { get; set; }
        public string BackgroundColor { get; set; }
        public int KeyProcessAreaTypeId { get; set; }
        public string KPAType { get; set; }
        public int TargetId { get; set; }
        public Target Target { get; set; }
        public int TargetThreshold { get; set; }
        public int TargetBudget { get; set; }
        public int TargetValue { get; set; }
        public List<ProductionVM> Productions { get; set; }
        public string AssetName { get; set; }
        public bool IsProcess { get; set; }
        public bool IsBuffer { get; set; }
        public string FlagColor { get; set; }
    }
}
