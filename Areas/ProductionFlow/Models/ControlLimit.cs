using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.ProductionFlow.Models
{
    public class ControlLimit
    {
        public int mesuarement { get; set; }
        public double mean { get; set; }
        public double ucl { get; set; }
        public double lcl { get; set; }
        public string date { get; set; }
    }
}
