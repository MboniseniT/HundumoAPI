using BinmakBackEnd.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.ProductionFlow.Entities
{
    public class ProductionFlowAsset
    {
        public int ProductionFlowAssetId { get; set; }
        public int AssetId { get; set; }
        [ForeignKey("AssetId")]
        public AssetNode AssetNode { get; set; }
        public int ClientAssetNameId { get; set; }

        [ForeignKey("ClientAssetNameId")]
        public ClientAsset ClientAsset { get; set; }
        public string SiteName { get; set; }
        public int TemplateId { get; set; }
        [ForeignKey("TemplateId")]
        public Template Template { get; set; }
        public string Reference { get; set; }
        public DateTime DateStamp { get; set; }
        public DateTime SinceDateProduction { get; set; }
    }
}
