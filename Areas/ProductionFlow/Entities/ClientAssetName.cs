using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.ProductionFlow.Entities
{
    public class ClientAsset
    {
        public int ClientAssetId { get; set; }
        public int ClientAssetNameId { get; set; }
        public string AssetName { get; set; }
        public string ClientName { get; set; }
        public string Reference { get; set; }
        public DateTime DateStamp { get; set; }
        public int AssetNodeId { get; set; }
    }
}
