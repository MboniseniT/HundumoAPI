using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.Kwenza.Models
{
    public class FormulaCreationVM
    {
        public int keyProcessAreaId { get; set; }
        public List<int> KPAIds { get; set; }
        public List<int> OpsIds { get; set; }
        public List<int> Indeces { get; set; }
    }
}
