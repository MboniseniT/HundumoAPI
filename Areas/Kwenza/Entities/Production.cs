using BinmakBackEnd.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.Kwenza.Entities
{
    public class Production
    {
        public int ProductionId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public string Reference { get; set; }
        public DateTime DateStamp { get; set; }
        public int KeyProcessAreaId { get; set; }
        [ForeignKey("KeyProcessAreaId")]
        public virtual KeyProcessArea KeyProcessArea { get; set; }
        public int AssetNodeId { get; set; }
        public int Value { get; set; }
    }
}
