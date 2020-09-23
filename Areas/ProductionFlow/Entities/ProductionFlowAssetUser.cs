using BinmakAPI.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.ProductionFlow.Entities
{
    public class ProductionFlowAssetUser
    {
        public int ProductionFlowAssetUserId { get; set; }
        public int AssetId { get; set; }
        public string Reference { get; set; }
        public string UserId { get; set; }
        [ForeignKey("Id")]
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime DateStamp { get; set; }
        public bool IsOverallProductionProcess { get; set; }
        public bool IsOverallProductionBuffer { get; set; }
        public bool IsDrillAndBlast { get; set; }
        public bool IsSupport { get; set; }
        public bool IsShe { get; set; }
        public bool IsLoadAndHaul { get; set; }
        public bool IsFacePreparation { get; set; }
        public bool IsEquipmentStatus { get; set; }
    }
}
