using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.Kwenza.Entities
{
    public class KeyProcessAreaType
    {
        public int KeyProcessAreaTypeId { get; set; }
        //Process OR Buffer
        public string KeyProcessAreaTypeName { get; set; }
        public int AssetNodeId { get; set; }
    }
}
