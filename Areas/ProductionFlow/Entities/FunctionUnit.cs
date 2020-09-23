using BinmakBackEnd.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.ProductionFlow.Entities
{
    public class FunctionUnit
    {
        public int FunctionUnitId { get; set; }
        public string FunctionUnitName { get; set; }
        public int AssetId { get; set; }
        [ForeignKey("AssetNodeId")]
        public AssetNode AssetNode { get; set; }
        public int ClientAssetNameId { get; set; }
        [ForeignKey("ClientAssetNameId")]
        public ClientAsset ClientAsset { get; set; }
    }
}
