using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.Kwenza.Models
{
    public class KeyProcessAreaVM
    {
        public int AssetNodeId { get; set; }
        public int ProcessId { get; set; }
        public string KeyProcessAreaName { get; set; }
        public string Color { get; set; }
        public string BackgroundColor { get; set; }
        public int KeyProcessAreaTypeId { get; set; }
        public string Frequency { get; set; }
        public DateTime ProcessDate { get; set; }
        public string Reference { get; set; }
    }
}
