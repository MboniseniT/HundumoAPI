using BinmakBackEnd.Areas.Kwenza.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.Kwenza.Models
{
    public class ProductionVM
    {
        public int ProductionId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public string Reference { get; set; }
        public DateTime DateStamp { get; set; }
        public int KeyProcessAreaId { get; set; }
        public virtual KeyProcessArea KeyProcessArea { get; set; }
        public int AssetNodeId { get; set; }
        public int Value { get; set; }
        public bool IsBuffer { get; set; }
        public bool IsProcess { get; set; }
        public string FlagColor { get; set; }
    }
}
