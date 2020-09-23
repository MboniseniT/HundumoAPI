using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.ProductionFlow.Entities
{
    public class DailyTask
    {
        public int DailyTaskId { get; set; }
        public string DailyTaskName { get; set; }
        public int ActionId { get; set; }
        public DateTime TaskDate { get; set; }
        public int AssetId { get; set; }
        public int ActionIndex { get; set; }
        public int TaskTarget { get; set; }
        public string Unit { get; set; }
        public string Frequency { get; set; }
        public string Reference { get; set; }
        public DateTime DateStamp { get; set; }
        public int DailyTaskValue { get; set; }
        public int Target { get; set; }
        public int Budget { get; set; }
        public int Threshold { get; set; }
        public DateTime DateProduction { get; set; }
    }
}
