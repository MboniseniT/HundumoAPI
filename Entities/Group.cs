using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Entities
{
    public class Group
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int AssetNodeId { get; set; }
        [ForeignKey("AssetNodeId")]
        public AssetNode AssetNode { get; set; }
        public string Reference { get; set; }
        public DateTime DateStamp { get; set; }
        public int RootId { get; set; }
    }
}
