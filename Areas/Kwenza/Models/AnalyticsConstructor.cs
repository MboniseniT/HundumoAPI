using BinmakBackEnd.Areas.Kwenza.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.Kwenza.Models
{
    public class AnalyticsConstructor
    {
        public int KPAId { get; set; }
        public string KPAName { get; set; }
        public List<Production> Productions { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
