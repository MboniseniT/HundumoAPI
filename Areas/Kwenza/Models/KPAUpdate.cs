using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.Kwenza.Models
{
    public class KPAUpdate
    {
        public int KeyProcessAreaId { get; set; }
        public string KeyProcessAreaName { get; set; }
        public string Color { get; set; }
        public string BackgroundColor { get; set; }
        public int KeyProcessAreaTypeId { get; set; }
        public string Frequency { get; set; }
    }
}
