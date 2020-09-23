using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.Kwenza.Models
{
    public class PlotBox
    {
        public string Axis { get; set; }
        public string Name { get; set; }
        public PlotBoxValues plotBoxValues { get; set; }
        public int NumberOfValues { get; set; }
        public double Mean { get; set; }
    }

    public class PlotBoxValues
    {
        public double Median { get; set; }
        public double Maximum { get; set; }
        public double Minimum { get; set; }
        public double UpperQuartile { get; set; }
        public double LowerQuartile { get; set; }
    }
}
