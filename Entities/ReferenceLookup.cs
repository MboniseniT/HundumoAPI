using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Entities
{
    public class ReferenceLookup
    {
        public int ReferenceLookupId { get; set; }
        public string UserId { get; set; }
        public string Reference { get; set; }
    }
}
