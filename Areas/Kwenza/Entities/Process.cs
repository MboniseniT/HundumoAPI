using BinmakBackEnd.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.Kwenza.Entities
{
    public class Process
    {
        public int ProcessId { get; set; }
        public int AssetNodeId { get; set; }
        [ForeignKey("AssetNodeId")]
        public virtual AssetNode AssetNode { get; set; }
        public string ProcessName { get; set; }
        public DateTime DateStamp { get; set; }
        public string Reference { get; set; }
        public string Color { get; set; }
        public string BackgroundColor { get; set; }
        public DateTime ProcessDate { get; set; }
        public bool IsSummary { get; set; }
        public int parentAssetNodeId { get; set; }
    }
}
