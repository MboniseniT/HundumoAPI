using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Entities
{
    public class AssetUser
    {
        public int AssetUserId { get; set; }
        public int AssetNodeId { get; set; }
        public string UserId { get; set; }
        public int AssetNodeTypeId { get; set; }
        public string Reference { get; set; }
        public DateTime DateStamp { get; set; }
        public bool IsAssetAdmin { get; set; }
    }
}
