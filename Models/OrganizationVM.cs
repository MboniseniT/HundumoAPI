using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BinmakBackEnd.Models
{
    public class OrganizationVM
    {
        public int OrganizationId { get; set; }
        public int RootOrganizationId { get; set; }
        public int ParentOrganizationId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTime DateStamp { get; set; }
        public string Reference { get; set; }
        public int Height { get; set; }
        public int NodeId { get; set; }
        public int NodeType { get; set; }
    }

}
