using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Models
{
    public class Tree1
    {
        public string Name { get; set; }
        public int NodeId { get; set; }
        public int ParentNodeId { get; set; }
        public int NodeType { get; set; }
        public List<Tree1> Children { get; set; }
        public bool Checked { get; set; }
    }
}
