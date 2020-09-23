using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.Kwenza.Models
{
    public class ControlLimit
    {
        public List<int> measurements { get; set; }
        public double mean { get; set; }
        public double ucl { get; set; }
        public double lcl { get; set; }
        public List<string> dates { get; set; }
    }
}
