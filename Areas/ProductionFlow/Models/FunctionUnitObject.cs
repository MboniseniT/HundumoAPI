using BinmakBackEnd.Areas.ProductionFlow.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.ProductionFlow.Models
{
    public class FunctionUnitObject
    {
        public string FunctionUnit { get; set; }
        public List<FunctionUnitChildren> FunctionChildren { get; set; }
        public ListOfValues[] FunctionUnitValues { get; set; }

    }

    public class FunctionChildren
    {
        public string FunctionChildrenName { get; set; }
        //public string FunctionChildrenBachgroundColor { get; set; }
    }

    public class OverallProductionFunctionUnitObject
    {
        public string SiteName { get; set; }
        public int AssetId { get; set; }
        public List<FunctionUnitObject> FunctionUnitObjects { get; set; }
    }

    public class ListOfValues
    {
        public int[] Readings { get; set; }
        public List<ValueObject> ReadingsObject { get; set; }
        public List<int[]> LatestReadings { get; set; }
    }
}
