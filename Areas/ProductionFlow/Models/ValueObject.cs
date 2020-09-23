using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.ProductionFlow.Models
{
    public class ValueObject
    {
        public int Value { get; set; }
        public bool IsBackground { get; set; }
        public string Color { get; set; }
    }
}
