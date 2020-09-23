using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.Kwenza.Entities
{
    public class Frequency
    {
        public int FrequencyId { get; set; }
        //Perday OR Available OR Per Month
        public string FrequencyName { get; set; }
        public int KeyProcessAreaId { get; set; }
        [ForeignKey("KeyProcessAreaId")]
        public virtual KeyProcessArea KeyProcessArea { get; set; }
    }
}
