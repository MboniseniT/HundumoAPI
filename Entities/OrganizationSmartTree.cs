using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Entities
{
    public class OrganizationSmartTree
    {
        public int OrganizationSmartTreeId { get; set; }
        public int OrganizationId { get; set; }
        public int ParentOrganizationId { get; set; }
        public int RootParentOrganizationId { get; set; }
        public int HierachyLevel { get; set; }
    }
}
