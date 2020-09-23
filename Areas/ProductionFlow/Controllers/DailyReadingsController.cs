using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinmakAPI.Data;
using BinmakBackEnd.Areas.ProductionFlow.Entities;
using BinmakBackEnd.Areas.ProductionFlow.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace BinmakBackEnd.Areas.ProductionFlow.Controllers
{
    [EnableCors("CorsPolicy")]
    [Area("ProductionFlow")]
    [ApiController]
    [Route("ProductionFlow/[controller]")]
    public class DailyReadingsController : ControllerBase
    {
        private readonly BinmakDbContext _context;

        public DailyReadingsController(BinmakDbContext context)
        {
            _context = context;
        }

        [HttpGet("clientAssset")]
        public IActionResult SaveClientAsset(string referrence)
        {
            List<ClientAsset> clientAssetNames = _context.ClientAssetNames.Where(id => id.Reference == referrence).ToList();

            return Ok(clientAssetNames);
        }

        [HttpPost("clientAssset")]
        public IActionResult SaveClientAsset([FromBody] ClientAsset model)
        {
            if (model == null)
            {
                return BadRequest("Something bad happened.");
            }
            try
            {
                ClientAsset clientAssetName = new ClientAsset();
                clientAssetName.ClientName = model.ClientName;
                clientAssetName.AssetName = model.AssetName;
                clientAssetName.Reference = model.Reference;
                clientAssetName.DateStamp = DateTime.Now;

                _context.ClientAssetNames.Add(clientAssetName);
                _context.SaveChanges();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }

            return Ok();
        }


        [HttpPost("LimitObject")]
        public IActionResult ReadingObject(LimitObject limit)
        {
            if (limit == null)
            {
                return BadRequest("Something bad happened. Make sure the form is filled.");
            }

            try
            {
                var limits = _context.Readings.FirstOrDefault(x => (x.AssetId == limit.AssetId) && (x.DateProduction.Month == limit.ProductionDate.Month) && (x.DateProduction.Year == limit.ProductionDate.Year));

                return Ok(limits);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }


        [HttpPost("ReadingObject")]
        public IActionResult ReadingObject(ReadingObject reading)
        {
            if (reading == null)
            {
                return BadRequest("Something bad happened. Make sure the form is filled.");
            }

            try
            {
                var readings = _context.Readings.FirstOrDefault(x => (x.ReadingId == reading.ReadingId) && (x.DateProduction.Month == reading.ProductionDate.Month) && (x.DateProduction.Year == reading.ProductionDate.Year));

                return Ok(readings);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }


        [HttpPost("UpdateMonthlyLimit")]
        public IActionResult UpdateMonthlyAsset(ReadingMonthlyLimits reading)
        {
            if (reading == null)
            {
                return BadRequest("Something bad happened. Make sure the form is filled.");
            }

            try
            {
                var readings = _context.Readings.Where(x => (x.AssetId == reading.AssetId) && (x.DateProduction.Month == reading.DateProduction.Month) && (x.DateProduction.Year == x.DateProduction.Year)).ToList();

                foreach (var r in readings)
                {
                    r.SheMonthTarget = reading.SheMonthTarget;
                    r.BoltersMonthTarget = reading.BoltersMonthTarget;
                    r.Day1st4HoursEndsMonthTarget = reading.Day1st4HoursEndsMonthTarget;
                    r.DriftersMonthTarget = reading.DriftersMonthTarget;
                    r.DrillRigsMonthTarget = reading.DrillRigsMonthTarget;
                    r.DumpTrucksMonthTarget = reading.DumpTrucksMonthTarget;
                    r.EndsBlastedMonthTarget = reading.EndsBlastedMonthTarget;
                    r.EndsDrilledMonthTarget = reading.EndsDrilledMonthTarget;
                    r.EndsLashedMonthTarget = reading.EndsLashedMonthTarget;
                    r.EndsPreparedMonthTarget = reading.EndsPreparedMonthTarget;
                    r.EndsSupportedMonthTarget = reading.EndsSupportedMonthTarget;
                    r.HoistedTonsMonthTarget = reading.HoistedTonsMonthTarget;
                    r.LHDsMonthTarget = reading.LHDsMonthTarget;
                    r.LashedPreparedForSupportMonthTarget = reading.LashedPreparedForSupportMonthTarget;
                    r.MuckbayTonsMonthTarget = reading.MuckbayTonsMonthTarget;
                    r.Night1st4HoursEndsMonthTarget = reading.Night1st4HoursEndsMonthTarget;
                    r.PreparedMarkedEndsMonthTarget = reading.PreparedMarkedEndsMonthTarget;
                    r.SupportedEndsMonthTarget = reading.SupportedEndsMonthTarget;
                    r.TotalCleanedEndsMonthTarget = reading.TotalCleanedEndsMonthTarget;
                    r.UGCrusherBinMonthTarget = reading.UGCrusherBinMonthTarget;
                    r.UnlashedEndsMonthTarget = reading.UnlashedEndsMonthTarget;


                    r.SheTarget = reading.SheTarget;
                    r.BoltersTarget = reading.BoltersTarget;
                    r.Day1st4HoursEndsTarget = reading.Day1st4HoursEndsTarget;
                    r.DriftersTarget = reading.DriftersTarget;
                    r.DrillRigsTarget = reading.DrillRigsTarget;
                    r.DumpTrucksTarget = reading.DumpTrucksTarget;
                    r.EndsBlastedTarget = reading.EndsBlastedTarget;
                    r.EndsDrilledTarget = reading.EndsDrilledTarget;
                    r.EndsLashedTarget = reading.EndsLashedTarget;
                    r.EndsPreparedTarget = reading.EndsPreparedTarget;
                    r.EndsSupportedTarget = reading.EndsSupportedTarget;
                    r.HoistedTonsTarget = reading.HoistedTonsTarget;
                    r.LHDsTarget = reading.LHDsTarget;
                    r.LashedPreparedForSupportTarget = reading.LashedPreparedForSupportTarget;
                    r.MuckbayTonsTarget = reading.MuckbayTonsTarget;
                    r.Night1st4HoursEndsTarget = reading.Night1st4HoursEndsTarget;
                    r.PreparedMarkedEndsTarget = reading.PreparedMarkedEndsTarget;
                    r.SupportedEndsTarget = reading.SupportedEndsTarget;
                    r.TotalCleanedEndsTarget = reading.TotalCleanedEndsTarget;
                    r.UGCrusherBinTarget = reading.UGCrusherBinTarget;
                    r.UnlashedEndsTarget = reading.UnlashedEndsTarget;


                    r.SheBudget = reading.SheBudget;
                    r.BoltersBudget = reading.BoltersBudget;
                    r.Day1st4HoursEndsBudget = reading.Day1st4HoursEndsBudget;
                    r.DriftersBudget = reading.DriftersBudget;
                    r.DrillRigsBudget = reading.DrillRigsBudget;
                    r.DumpTrucksBudget = reading.DumpTrucksBudget;
                    r.EndsBlastedBudget = reading.EndsBlastedBudget;
                    r.EndsDrilledBudget = reading.EndsDrilledBudget;
                    r.EndsLashedBudget = reading.EndsLashedBudget;
                    r.EndsPreparedBudget = reading.EndsPreparedBudget;
                    r.EndsSupportedBudget = reading.EndsSupportedBudget;
                    r.HoistedTonsBudget = reading.HoistedTonsBudget;
                    r.LHDsBudget = reading.LHDsBudget;
                    r.LashedPreparedForSupportBudget = reading.LashedPreparedForSupportBudget;
                    r.MuckbayTonsBudget = reading.MuckbayTonsBudget;
                    r.Night1st4HoursEndsBudget = reading.Night1st4HoursEndsBudget;
                    r.PreparedMarkedEndsBudget = reading.PreparedMarkedEndsBudget;
                    r.SupportedEndsBudget = reading.SupportedEndsBudget;
                    r.TotalCleanedEndsBudget = reading.TotalCleanedEndsBudget;
                    r.UGCrusherBinBudget = reading.UGCrusherBinBudget;
                    r.UnlashedEndsBudget = reading.UnlashedEndsBudget;


                    r.SheThreshold = reading.SheThreshold;
                    r.BoltersThreshold = reading.BoltersThreshold;
                    r.Day1st4HoursEndsThreshold = reading.Day1st4HoursEndsThreshold;
                    r.DriftersThreshold = reading.DriftersThreshold;
                    r.DrillRigsThreshold = reading.DrillRigsThreshold;
                    r.DumpTrucksThreshold = reading.DumpTrucksThreshold;
                    r.EndsBlastedThreshold = reading.EndsBlastedThreshold;
                    r.EndsDrilledThreshold = reading.EndsDrilledThreshold;
                    r.EndsLashedThreshold = reading.EndsLashedThreshold;
                    r.EndsPreparedThreshold = reading.EndsPreparedThreshold;
                    r.EndsSupportedThreshold = reading.EndsSupportedThreshold;
                    r.HoistedTonsThreshold = reading.HoistedTonsThreshold;
                    r.LHDsThreshold = reading.LHDsThreshold;
                    r.LashedPreparedForSupportThreshold = reading.LashedPreparedForSupportThreshold;
                    r.MuckbayTonsThreshold = reading.MuckbayTonsThreshold;
                    r.Night1st4HoursEndsThreshold = reading.Night1st4HoursEndsThreshold;
                    r.PreparedMarkedEndsThreshold = reading.PreparedMarkedEndsThreshold;
                    r.SupportedEndsThreshold = reading.SupportedEndsThreshold;
                    r.TotalCleanedEndsThreshold = reading.TotalCleanedEndsThreshold;
                    r.UGCrusherBinThreshold = reading.UGCrusherBinThreshold;
                    r.UnlashedEndsThreshold = reading.UnlashedEndsThreshold;

                    _context.Readings.Update(r);
                }
                _context.SaveChanges();

                return Ok();

            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }

        }

        [HttpPost("")]
        public IActionResult SaveReading(Reading reading)
        {
            if (reading == null)
            {
                return BadRequest("Something bad happened. Make sure the form is filled.");
            }

            try
            {

                var r = _context.Readings.FirstOrDefault(x => x.ReadingId == reading.ReadingId);

                r.She = reading.She;
                r.Bolters = reading.Bolters;
                r.Day1st4HoursEnds = reading.Day1st4HoursEnds;
                r.Drifters = reading.Drifters;
                r.DrillRigs = reading.DrillRigs;
                r.DumpTrucks = reading.DumpTrucks;
                r.EndsBlasted = reading.EndsBlasted;
                r.EndsDrilled = reading.EndsDrilled;
                r.EndsLashed = reading.EndsLashed;
                r.EndsPrepared = reading.EndsPrepared;
                r.EndsSupported = reading.EndsSupported;
                r.HoistedTons = reading.HoistedTons;
                r.LHDs = reading.LHDs;
                r.LashedPreparedForSupport = reading.LashedPreparedForSupport;
                r.MuckbayTons = reading.MuckbayTons;
                r.Night1st4HoursEnds = reading.Night1st4HoursEnds;
                r.PreparedMarkedEnds = reading.PreparedMarkedEnds;
                r.SupportedEnds = reading.SupportedEnds;
                r.TotalCleanedEnds = reading.TotalCleanedEnds;
                r.UGCrusherBin = reading.UGCrusherBin;
                r.UnlashedEnds = reading.UnlashedEnds;

                _context.Readings.Update(r);
                _context.SaveChanges();

                return Ok();

            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }
    }
}
