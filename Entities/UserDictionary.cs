using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Entities
{
    public class UserDictionary
    {
        public int UserDictionaryId { get; set; }
        public string Reference { get; set; }
        public string UserId { get; set; }
        public DateTime DateStamp { get; set; }
    }
}
