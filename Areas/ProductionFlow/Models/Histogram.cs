using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.ProductionFlow.Models
{
    public class Histogram
    {
        public List<double> xAxis { get; set; }
        public List<double> yAxis { get; set; }
        public string Name { get; set; }
        public HistogramValues HistogramValues { get; set; }
        public int NumberOfValues { get; set; }
        public double Mean { get; set; }
    }

    public class HistogramValues
    {
        //public double Mean { get; set; }
        public double Median { get; set; }
        public double Maximum { get; set; }
        public double Minimum { get; set; }
        public double UpperQuartile { get; set; }
        public double LowerQuartile { get; set; }
    }
}
