using BinmakBackEnd.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.Kwenza.Entities
{
    public class KeyProcessArea
    {
        [Key]
        public int KeyProcessAreaId { get; set; }
        public int AssetNodeId { get; set; }
        public int ProcessId { get; set; }
        [ForeignKey("ProcessId")]
        public virtual Process Process { get; set; }
        public string KeyProcessAreaName { get; set; }
        public string Color { get; set; }
        public string Reference { get; set; }
        public bool IsTargetSet { get; set; }
        public DateTime KPADate { get; set; }
        public string BackgroundColor { get; set; }
        public int KeyProcessAreaTypeId { get; set; }
        [ForeignKey("KeyProcessAreaTypeId")]
        public virtual KeyProcessAreaType KeyProcessAreaType { get; set; }
    }
}
