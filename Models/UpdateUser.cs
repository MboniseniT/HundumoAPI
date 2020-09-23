using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Models
{
    public class UpdateUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int RoleId { get; set; }
        //Top Asset Node
        public int AssignedAssetsNode { get; set; }
        public List<int> BinmakModuleId { get; set; }
        public string Reference { get; set; }
        public string Id { get; set; }
        public int RootId { get; set; }
    }
}
