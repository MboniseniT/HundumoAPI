using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.Kwenza.Models
{
    public class ProcessUpdate
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string Color { get; set; }
        public string BackgroundColor { get; set; }
    }
}
