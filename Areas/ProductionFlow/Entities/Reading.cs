using BinmakBackEnd.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BinmakBackEnd.Areas.ProductionFlow.Entities
{
    public class Reading
    {
        public int ReadingId { get; set; }
        public int AssetId { get; set; }
        [ForeignKey("AssetId")]
        public AssetNode AssetNode { get; set; }
        public string Reference { get; set; }
        public DateTime DateProduction { get; set; }
        public bool IsClosed { get; set; }

        public int She { get; set; }
        public string SheUnit { get; set; }
        public int SheFrequency { get; set; }
        public int SheMonthTarget { get; set; }
        public int SheTarget { get; set; }
        public int SheBudget { get; set; }
        public int SheThreshold { get; set; }


        public int Day1st4HoursEnds { get; set; }
        public string Day1st4HoursEndsUnit { get; set; }
        public int Day1st4HoursEndsFrequency { get; set; }
        public int Day1st4HoursEndsMonthTarget { get; set; }
        public int Day1st4HoursEndsTarget { get; set; }
        public int Day1st4HoursEndsBudget { get; set; }
        public int Day1st4HoursEndsThreshold { get; set; }

        public int Night1st4HoursEnds { get; set; }
        public string Night1st4HoursEndsUnit { get; set; }
        public int Night1st4HoursEndsFrequency { get; set; }
        public int Night1st4HoursEndsMonthTarget { get; set; }
        public int Night1st4HoursEndsTarget { get; set; }
        public int Night1st4HoursEndsBudget { get; set; }
        public int Night1st4HoursEndsThreshold { get; set; }


        public int EndsDrilled { get; set; }
        public string EndsDrilledUnit { get; set; }
        public int EndsDrilledFrequency { get; set; }
        public int EndsDrilledMonthTarget { get; set; }
        public int EndsDrilledTarget { get; set; }
        public int EndsDrilledBudget { get; set; }
        public int EndsDrilledThreshold { get; set; }

        public int EndsBlasted { get; set; }
        public string EndsBlastedUnit { get; set; }
        public int EndsBlastedFrequency { get; set; }
        public int EndsBlastedMonthTarget { get; set; }
        public int EndsBlastedTarget { get; set; }
        public int EndsBlastedBudget { get; set; }
        public int EndsBlastedThreshold { get; set; }

        public int UnlashedEnds { get; set; }
        public string UnlashedEndsUnit { get; set; }
        public int UnlashedEndsFrequency { get; set; }
        public int UnlashedEndsMonthTarget { get; set; }
        public int UnlashedEndsTarget { get; set; }
        public int UnlashedEndsBudget { get; set; }
        public int UnlashedEndsThreshold { get; set; }

        public int EndsLashed { get; set; }
        public string EndsLashedUnit { get; set; }
        public int EndsLashedFrequency { get; set; }
        public int EndsLashedMonthTarget { get; set; }
        public int EndsLashedTarget { get; set; }
        public int EndsLashedBudget { get; set; }
        public int EndsLashedThreshold { get; set; }

        public int TotalCleanedEnds { get; set; }
        public string TotalCleanedEndsUnit { get; set; }
        public int TotalCleanedEndsFrequency { get; set; }
        public int TotalCleanedEndsMonthTarget { get; set; }
        public int TotalCleanedEndsTarget { get; set; }
        public int TotalCleanedEndsBudget { get; set; }
        public int TotalCleanedEndsThreshold { get; set; }

        public int LashedPreparedForSupport { get; set; }
        public string LashedPreparedForSupportUnit { get; set; }
        public int LashedPreparedForSupportFrequency { get; set; }
        public int LashedPreparedForSupportMonthTarget { get; set; }
        public int LashedPreparedForSupportTarget { get; set; }
        public int LashedPreparedForSupportBudget { get; set; }
        public int LashedPreparedForSupportThreshold { get; set; }

        public int MuckbayTons { get; set; }
        public string MuckbayTonsUnit { get; set; }
        public int MuckbayTonsFrequency { get; set; }
        public int MuckbayTonsMonthTarget { get; set; }
        public int MuckbayTonsTarget { get; set; }
        public int MuckbayTonsBudget { get; set; }
        public int MuckbayTonsThreshold { get; set; }

        public int HoistedTons { get; set; }
        public string HoistedTonsUnit { get; set; }
        public int HoistedTonsFrequency { get; set; }
        public int HoistedTonsMonthTarget { get; set; }
        public int HoistedTonsTarget { get; set; }
        public int HoistedTonsBudget { get; set; }
        public int HoistedTonsThreshold { get; set; }

        public int UGCrusherBin { get; set; }
        public string UGCrusherBinUnit { get; set; }
        public int UGCrusherBinFrequency { get; set; }
        public int UGCrusherBinMonthTarget { get; set; }
        public int UGCrusherBinTarget { get; set; }
        public int UGCrusherBinBudget { get; set; }
        public int UGCrusherBinThreshold { get; set; }

        public int EndsSupported { get; set; }
        public string EndsSupportedUnit { get; set; }
        public int EndsSupportedFrequency { get; set; }
        public int EndsSupportedMonthTarget { get; set; }
        public int EndsSupportedTarget { get; set; }
        public int EndsSupportedBudget { get; set; }
        public int EndsSupportedThreshold { get; set; }

        public int SupportedEnds { get; set; }
        public string SupportedEndsUnit { get; set; }
        public int SupportedEndsFrequency { get; set; }
        public int SupportedEndsMonthTarget { get; set; }
        public int SupportedEndsTarget { get; set; }
        public int SupportedEndsBudget { get; set; }
        public int SupportedEndsThreshold { get; set; }

        public int EndsPrepared { get; set; }
        public string EndsPreparedUnit { get; set; }
        public int EndsPreparedFrequency { get; set; }
        public int EndsPreparedMonthTarget { get; set; }
        public int EndsPreparedTarget { get; set; }
        public int EndsPreparedBudget { get; set; }
        public int EndsPreparedThreshold { get; set; }

        public int PreparedMarkedEnds { get; set; }
        public string PreparedMarkedEndsUnit { get; set; }
        public int PreparedMarkedEndsFrequency { get; set; }
        public int PreparedMarkedEndsMonthTarget { get; set; }
        public int PreparedMarkedEndsTarget { get; set; }
        public int PreparedMarkedEndsBudget { get; set; }
        public int PreparedMarkedEndsThreshold { get; set; }

        public int DrillRigs { get; set; }
        public string DrillRigsUnit { get; set; }
        public int DrillRigsFrequency { get; set; }
        public int DrillRigsMonthTarget { get; set; }
        public int DrillRigsTarget { get; set; }
        public int DrillRigsBudget { get; set; }
        public int DrillRigsThreshold { get; set; }

        public int LHDs { get; set; }
        public string LHDsUnit { get; set; }
        public int LHDsFrequency { get; set; }
        public int LHDsMonthTarget { get; set; }
        public int LHDsTarget { get; set; }
        public int LHDsBudget { get; set; }
        public int LHDsThreshold { get; set; }

        public int DumpTrucks { get; set; }
        public string DumpTrucksUnit { get; set; }
        public int DumpTrucksFrequency { get; set; }
        public int DumpTrucksMonthTarget { get; set; }
        public int DumpTrucksTarget { get; set; }
        public int DumpTrucksBudget { get; set; }
        public int DumpTrucksThreshold { get; set; }

        public int Bolters { get; set; }
        public string BoltersUnit { get; set; }
        public int BoltersFrequency { get; set; }
        public int BoltersMonthTarget { get; set; }
        public int BoltersTarget { get; set; }
        public int BoltersBudget { get; set; }
        public int BoltersThreshold { get; set; }

        public int Drifters { get; set; }
        public string DriftersUnit { get; set; }
        public int DriftersFrequency { get; set; }
        public int DriftersMonthTarget { get; set; }
        public int DriftersTarget { get; set; }
        public int DriftersBudget { get; set; }
        public int DriftersThreshold { get; set; }
    }
}
