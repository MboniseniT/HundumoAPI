using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BinmakAPI.Data;
using BinmakBackEnd.Areas.Kwenza.Entities;
using BinmakBackEnd.Areas.Kwenza.Models;
using BinmakBackEnd.Areas.ProductionFlow.Entities;
using BinmakBackEnd.Entities;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace BinmakBackEnd.Areas.Kwenza.Controllers
{
    [EnableCors("CorsPolicy")]
    [Area("Kwenza")]
    [ApiController]
    [Route("Kwenza/[controller]")]
    public class ProductionFlowConfigurationController : ControllerBase
    {
        private readonly BinmakDbContext _context;

        public ProductionFlowConfigurationController(BinmakDbContext context)
        {
            _context = context;
        }

        [HttpGet("kpa")]
        public IActionResult GetProcessType()
        {

            try
            {
                List<KeyProcessAreaType> keyProcessAreaTypes = _context.KeyProcessAreaTypes.ToList();

                return Ok(keyProcessAreaTypes);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        [HttpGet("deleteProcess")]
        public IActionResult DeleteProcess(int processId)
        {
            if (processId == 0)
            {
                return BadRequest("Something bad happened. Make sure process is selected");
            }

            try
            {
                //Find Process

                Process process = _context.Processes.FirstOrDefault(id => id.ProcessId == processId);

                //Fill all KPAs linked to process
                List<KeyProcessArea> keyProcessAreas = _context.KeyProcessAreas.Where(id => id.ProcessId == process.ProcessId).ToList();

                //Find all Targets linked to KPA
                foreach (var item in keyProcessAreas)
                {
                    _context.Targets.RemoveRange(_context.Targets.Where(id => id.KeyProcessAreaId == item.KeyProcessAreaId).ToList());
                }

                //Find all Find all productions linked to KPA
                foreach (var item in keyProcessAreas)
                {
                    _context.Productions.RemoveRange(_context.Productions.Where(id => id.KeyProcessAreaId == item.KeyProcessAreaId).ToList());
                }

                //Remove KPAs
                _context.KeyProcessAreas.RemoveRange(keyProcessAreas);
                _context.Processes.Remove(process);

                _context.SaveChanges();

                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened!"+ Ex.Message);
            }
        }

        [HttpGet("deleteKPA")]
        public IActionResult DeleteKPA(int kpaId)
        {
            if (kpaId == 0)
            {
                return BadRequest("Something bad happened. Make sure KPA is selected");
            }

            try
            {

                KeyProcessArea keyProcessArea = _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == kpaId);

                if (keyProcessArea == null)
                {
                    return BadRequest("KPA does not exists!");
                }

                 _context.Targets.RemoveRange(_context.Targets.Where(id => id.KeyProcessAreaId == kpaId).ToList());

                 _context.Productions.RemoveRange(_context.Productions.Where(id => id.KeyProcessAreaId == kpaId).ToList());

                 _context.KeyProcessAreas.Remove(keyProcessArea);

                 _context.SaveChanges();

                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened!" + Ex.Message);
            }
        }

        [HttpGet("kpaByProcess")]
        public IActionResult GetKPAByProcess(int processId)
        {
            if (processId == 0)
            {
                return BadRequest("Something bad happened. Make sure valid process is selected");
            }
            try
            {
                List<KeyProcessArea> keyProcessAreas = _context.KeyProcessAreas.Where(id=>id.ProcessId == processId).ToList();

                var kpas = keyProcessAreas.Select(result => new
                {
                    AssetNodeId = result.AssetNodeId,
                    Name = result.KeyProcessAreaName,
                    KeyProcessAreaTypeId = result.KeyProcessAreaTypeId,
                    Type = _context.KeyProcessAreaTypes.FirstOrDefault(id=>id.KeyProcessAreaTypeId == result.KeyProcessAreaTypeId).KeyProcessAreaTypeName,
                    Color = result.Color,
                    BackgroundColor = result.BackgroundColor,
                    Process = _context.Processes.FirstOrDefault(id=>id.ProcessId == result.ProcessId).ProcessName,
                    ProcessId = result.ProcessId,
                    Id = result.KeyProcessAreaId,
                    DateMonth = result.KPADate.ToString("yyyy-MMM"),
                    Frequency = _context.Frequencies.FirstOrDefault(id=>id.KeyProcessAreaId == result.KeyProcessAreaId).FrequencyName,
                    IsTargetSet = result.IsTargetSet,
                    IsProcessSummary = _context.Processes.FirstOrDefault(id => id.ProcessId == result.ProcessId).IsSummary
                });

                return Ok(kpas);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        [HttpPost("pdAddValue")]
        public IActionResult GetProductionFlowEditValue([FromBody] AddPFStatVM model)
        {
            if (model == null)
            {
                return BadRequest("Error, Refresh the page or re log-in and try again or ensure the form is filled correctly.");
            }

            try
            {
                DateTime date = DateTime.Parse(model.ProductionDate);

                if (date > DateTime.Now)
                {
                    return BadRequest("Error, You do not know the future values!");
                }

                foreach (var item in model.ValueKPAs)
                {
                    Production production = _context.Productions.FirstOrDefault(id => (id.KeyProcessAreaId == item.value) 
                    && (id.Year == date.Year) && (id.Month == date.Month) && (id.Day == date.Day));

                    production.Reference = model.Reference;
                    production.DateStamp = DateTime.Now;
                    production.Value = item.amount;

                    _context.Productions.Update(production);
                    _context.SaveChanges();
                }

                return Ok();

            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened! " + Ex.Message);
            }
        }


        [HttpPost("pdEditValue")]
        public IActionResult AddProductionFlowEditValue([FromBody] ProductionFlowEditValue model)
        {
            if (model == null)
            {
                return BadRequest("Error, Refresh the page or re log-in and try again.");
            }

            try
            {
                DateTime date = DateTime.Parse(model.ProductionDate);

                List<KeyProcessArea> keyProcessAreas = _context.KeyProcessAreas.Where(id => (id.AssetNodeId == model.AssetNodeId)
                /*&& (id.KPADate.Year == date.Year) && (id.KPADate.Month == date.Month) && (id.KPADate.Day == date.Day)*/).ToList();

                List<Production> productions = new List<Production>();

                foreach (var item in keyProcessAreas)
                {
                    Production production = _context.Productions.FirstOrDefault(id => (id.KeyProcessAreaId == item.KeyProcessAreaId)
                    && (id.Year == date.Year) && (id.Month == date.Month) && (id.Day == date.Day));

                    productions.Add(production);
                }

                var pdEditValues = productions.Select(result => new
                {
                    AssetNodeId = result.AssetNodeId,
                    KeyProcessAreaId = result.KeyProcessAreaId,
                    Type = _context.KeyProcessAreaTypes.FirstOrDefault(id => id.KeyProcessAreaTypeId == 
                    (_context.KeyProcessAreas.FirstOrDefault(id=>id.KeyProcessAreaId == result.KeyProcessAreaId)).KeyProcessAreaTypeId).KeyProcessAreaTypeName,
                    Color = _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == result.KeyProcessAreaId).Color,
                    BackgroundColor = _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == result.KeyProcessAreaId).BackgroundColor,
                    Process = _context.Processes.FirstOrDefault(id => id.ProcessId == _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == result.KeyProcessAreaId).ProcessId).ProcessName,
                    ProcessId = _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == result.KeyProcessAreaId).ProcessId,
                    DateMonth = _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == result.KeyProcessAreaId).KPADate,
                    MasterKPAs = GetMasterPFKPAs(result.AssetNodeId, date),
                    Frequency = _context.Frequencies.FirstOrDefault(id => id.KeyProcessAreaId == result.KeyProcessAreaId).FrequencyName,
                    IsTargetSet = _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == result.KeyProcessAreaId).IsTargetSet
                });

                return Ok(pdEditValues);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        public List<MasterPFKPAs> GetMasterPFKPAs(int assetNodeId, DateTime date)
        {
            List<MasterPFKPAs> masterPFKPAs = new List<MasterPFKPAs>();

            List<KeyProcessArea> keyProcessAreas = _context.KeyProcessAreas.Where(id => (id.AssetNodeId == assetNodeId)
            /*&& (id.KPADate.Year == date.Year) && (id.KPADate.Month == date.Month) && (id.KPADate.Day == date.Day)*/).ToList();

            List<Production> productions = new List<Production>();

            foreach (var item in keyProcessAreas)
            {
                Production production = _context.Productions.FirstOrDefault(id => (id.KeyProcessAreaId == item.KeyProcessAreaId)
                && (id.Year == date.Year) && (id.Month == date.Month) && (id.Day == date.Day));

                productions.Add(production);
            }

            foreach (var item in productions)
            {
                MasterPFKPAs masterPFKPAs1 = new MasterPFKPAs();

                masterPFKPAs1.amount = _context.Productions.FirstOrDefault(id => (id.KeyProcessAreaId == item.KeyProcessAreaId)
                 && (id.Year == date.Year) && (id.Month == date.Month) && (id.Day == date.Day)).Value;
                masterPFKPAs1.label = _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == 
                item.KeyProcessAreaId).KeyProcessAreaName + " (" + _context.KeyProcessAreaTypes.FirstOrDefault(id => id.KeyProcessAreaTypeId 
                == _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == item.KeyProcessAreaId).KeyProcessAreaTypeId).KeyProcessAreaTypeName + ")";
                masterPFKPAs1.value = item.KeyProcessAreaId;

                masterPFKPAs.Add(masterPFKPAs1);
            }

            return masterPFKPAs;
        }

        [HttpPost("getProcesses")]
        public IActionResult GetProcesses([FromBody] MonthProcessProduction monthProcessProduction)
        {
            if (monthProcessProduction == null)
            {
                return BadRequest("Error, make sure productive is selected correctly.");
            }

            try
            {
                List<Process> processes = _context.Processes.Where(id => (id.AssetNodeId == monthProcessProduction.AssetNodeId) && 
                (id.ProcessDate.Year == monthProcessProduction.ProductionDate.Year) &&
                (id.ProcessDate.Month == monthProcessProduction.ProductionDate.Month)).ToList();

                return Ok(processes);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        [HttpPost("kpa")]
        public IActionResult SaveKpa([FromBody] KeyProcessAreaVM model)
        {

            if (model == null)
            {
                return BadRequest("Error, make sure KPA form is filled correctly.");
            }

            Process process = _context.Processes.FirstOrDefault(id => id.ProcessId == model.ProcessId);
            model.ProcessDate = process.ProcessDate;

            KeyProcessArea kpaChecker = _context.KeyProcessAreas.FirstOrDefault(id =>
            (id.AssetNodeId == model.AssetNodeId) && (id.KeyProcessAreaName == model.KeyProcessAreaName)
            && (id.ProcessId == model.ProcessId) && (id.KPADate.Year == model.ProcessDate.Year) 
            && (id.KPADate.Month == model.ProcessDate.Month));

            if (kpaChecker != null)
            {
                return BadRequest("Error, There is already an existing KPA for the month"+ model.ProcessDate.Year +"-"+ model.ProcessDate.Year + ". Choose a different name or edit the existing one.");
            }

            try
            {
                KeyProcessArea keyProcessArea = new KeyProcessArea();
                keyProcessArea.KeyProcessAreaName = model.KeyProcessAreaName;
                keyProcessArea.KeyProcessAreaTypeId = model.KeyProcessAreaTypeId;
                keyProcessArea.AssetNodeId = model.AssetNodeId;
                keyProcessArea.ProcessId = model.ProcessId;
                keyProcessArea.Color = model.Color;
                keyProcessArea.BackgroundColor = model.BackgroundColor;
                keyProcessArea.KPADate = model.ProcessDate;
                keyProcessArea.Reference = model.Reference;

                _context.KeyProcessAreas.Add(keyProcessArea);
                _context.SaveChanges();

                Frequency frequency = new Frequency();
                frequency.FrequencyName = model.Frequency;
                frequency.KeyProcessAreaId = keyProcessArea.KeyProcessAreaId;
                _context.Add(frequency);
                _context.SaveChanges();

                List<DateTime> dateTimes = getAllDates(model.ProcessDate.Year, model.ProcessDate.Month);

                List<Production> productions = new List<Production>();

                foreach (var date in dateTimes)
                {
                    Production production = new Production();
                    production.AssetNodeId = model.AssetNodeId;
                    production.KeyProcessAreaId = keyProcessArea.KeyProcessAreaId;
                    production.KeyProcessArea = _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == keyProcessArea.KeyProcessAreaId);
                    production.Month = date.Month;
                    production.Year = date.Year;
                    production.Day = date.Day;
                    production.Reference = model.Reference;
                    production.Value = 0;
                    production.DateStamp = DateTime.Now;
                    productions.Add(production);
                }

                _context.Productions.AddRange(productions);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }

        }

        public static List<DateTime> getAllDates(int year, int month)
        {
            var ret = new List<DateTime>();
            for (int i = 1; i <= DateTime.DaysInMonth(year, month); i++)
            {
                ret.Add(new DateTime(year, month, i));
            }
            return ret;
        }

        [HttpPost("pfDatastructureOverall")]
        public IActionResult PullOverallProductionFlowDatastructure([FromBody] ProductionFlowDatastructureVM model)
        {

            if (model == null)
            {
                return BadRequest("Error, make sure asset is selected.");
            }

            try
            {
                AssetNode assetNode =  _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == model.AssetNodeId);
                List<ClientAsset> clientAssets = _context.ClientAssetNames.Where(id => id.ClientAssetNameId == assetNode.ParentAssetNodeId).ToList();
                List<ProductionFlowInputDatastructureModMainWrapper> productionFlowInputDatastructureModMainWrappers = new List<ProductionFlowInputDatastructureModMainWrapper>();

                foreach (var ii in clientAssets)
                {

                    List<Process> processes = _context.Processes.Where(id => (id.AssetNodeId == ii.AssetNodeId)
                    && (id.ProcessDate.Year == model.ProductionMonthDate.Year) && (id.ProcessDate.Month == model.ProductionMonthDate.Month)).ToList();

                    List<ProductionFlowInputDatastructure> productionFlowInputDatastructures = new List<ProductionFlowInputDatastructure>();

                    foreach (var p in processes)
                    {
                        ProductionFlowInputDatastructure productionFlowInputDatastructure = new ProductionFlowInputDatastructure();
                        productionFlowInputDatastructure.Process = p;
                        productionFlowInputDatastructure.KeyProcessAreas = _context.KeyProcessAreas.Where(id => (id.ProcessId == p.ProcessId)
                        && (id.KPADate.Year == model.ProductionMonthDate.Year) && (id.KPADate.Month == model.ProductionMonthDate.Month)).ToList();

                        productionFlowInputDatastructures.Add(productionFlowInputDatastructure);
                    }

                    List<ProductionFlowInputDatastructureMod> productionFlowInputDatastructureMods = new List<ProductionFlowInputDatastructureMod>();

                    foreach (var item in productionFlowInputDatastructures)
                    {
                        ProductionFlowInputDatastructureMod productionFlowInputDatastructureMod = new ProductionFlowInputDatastructureMod();
                        productionFlowInputDatastructureMod.Process = item.Process;
                        productionFlowInputDatastructureMod.ProductionDates = GetProductionDate(model.ProductionMonthDate.Year, model.ProductionMonthDate.Month);
                        productionFlowInputDatastructureMod.AssetName = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == ii.AssetNodeId).Name;

                        List<KPApfVM> kPApfVMs = new List<KPApfVM>();


                        foreach (var i in item.KeyProcessAreas)
                        {
                            if (i.IsTargetSet == true)
                            {
                                KPApfVM kPApfVM = new KPApfVM();
                                kPApfVM.KeyProcessAreaId = i.KeyProcessAreaId;
                                kPApfVM.AssetNodeId = i.AssetNodeId;
                                kPApfVM.AssetName = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == i.AssetNodeId).Name;
                                kPApfVM.ProcessId = i.ProcessId;
                                kPApfVM.Process = _context.Processes.FirstOrDefault(id => id.ProcessId == i.ProcessId);
                                kPApfVM.KeyProcessAreaName = _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == i.KeyProcessAreaId).KeyProcessAreaName;
                                kPApfVM.Color = i.Color;
                                kPApfVM.IsTargetSet = i.IsTargetSet;
                                kPApfVM.KPADate = i.KPADate;
                                kPApfVM.BackgroundColor = i.BackgroundColor;
                                kPApfVM.KeyProcessAreaTypeId = i.KeyProcessAreaTypeId;
                                kPApfVM.KPAType = _context.KeyProcessAreaTypes.FirstOrDefault(id => id.KeyProcessAreaTypeId == i.KeyProcessAreaTypeId).KeyProcessAreaTypeName;
                                kPApfVM.Frequency = _context.Frequencies.FirstOrDefault(id => id.KeyProcessAreaId == i.KeyProcessAreaId).FrequencyName;
                                kPApfVM.Target = _context.Targets.FirstOrDefault(id => id.KeyProcessAreaId == i.KeyProcessAreaId);
                                kPApfVM.TargetId = _context.Targets.FirstOrDefault(id => id.KeyProcessAreaId == i.KeyProcessAreaId).TargetId;
                                kPApfVM.TargetBudget = _context.Targets.FirstOrDefault(id => id.KeyProcessAreaId == i.KeyProcessAreaId).Budget;
                                kPApfVM.TargetThreshold = _context.Targets.FirstOrDefault(id => id.KeyProcessAreaId == i.KeyProcessAreaId).Threshold;
                                kPApfVM.TargetValue = _context.Targets.FirstOrDefault(id => id.KeyProcessAreaId == i.KeyProcessAreaId).TargetValue;
                                kPApfVM.Productions = GetOrderedProd(i.KeyProcessAreaId, _context.Targets.FirstOrDefault(id => id.KeyProcessAreaId == i.KeyProcessAreaId).TargetId);
                                kPApfVM.IsBuffer = i.KeyProcessAreaTypeId == 1 ? true : false;
                                kPApfVM.IsProcess = i.KeyProcessAreaTypeId == 2 ? true : false;
                                kPApfVM.FlagColor = GetFlag(i.KeyProcessAreaId, i.KPADate);

                                kPApfVMs.Add(kPApfVM);
                            }

                        }

                        productionFlowInputDatastructureMod.KeyProcessAreas = kPApfVMs;

                        productionFlowInputDatastructureMods.Add(productionFlowInputDatastructureMod);
                    }

                    ProductionFlowInputDatastructureModMainWrapper productionFlowInputDatastructureModMainWrapper = new ProductionFlowInputDatastructureModMainWrapper();
                    productionFlowInputDatastructureModMainWrapper.ParentAssetName = ii.ClientName;
                    productionFlowInputDatastructureModMainWrapper.AssetName = _context.AssetNodes.FirstOrDefault(id=>id.AssetNodeId == ii.AssetNodeId).Name;
                    productionFlowInputDatastructureModMainWrapper.ProductionFlowInputDatastructureMods = productionFlowInputDatastructureMods;
                    productionFlowInputDatastructureModMainWrappers.Add(productionFlowInputDatastructureModMainWrapper);
                }

                return Ok(productionFlowInputDatastructureModMainWrappers);

            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        [HttpPost("pfDatastructure")]
        public IActionResult PullProductionFlowDatastructure([FromBody] ProductionFlowDatastructureVM model)
        {

            if (model == null)
            {
                return BadRequest("Error, make sure asset is selected.");
            }

            try
            {
                List<Process> processes = _context.Processes.Where(id => (id.AssetNodeId == model.AssetNodeId) 
                && (id.ProcessDate.Year == model.ProductionMonthDate.Year) && (id.ProcessDate.Month == model.ProductionMonthDate.Month)).ToList();

                List<ProductionFlowInputDatastructure> productionFlowInputDatastructures = new List<ProductionFlowInputDatastructure>();

                foreach (var p in processes)
                {
                    ProductionFlowInputDatastructure productionFlowInputDatastructure = new ProductionFlowInputDatastructure();
                    productionFlowInputDatastructure.Process = p;
                    productionFlowInputDatastructure.KeyProcessAreas = _context.KeyProcessAreas.Where(id => (id.ProcessId == p.ProcessId)
                    && (id.KPADate.Year == model.ProductionMonthDate.Year) && (id.KPADate.Month == model.ProductionMonthDate.Month)).ToList();

                    productionFlowInputDatastructures.Add(productionFlowInputDatastructure);
                }

                List<ProductionFlowInputDatastructureMod> productionFlowInputDatastructureMods = new List<ProductionFlowInputDatastructureMod>();

                foreach (var item in productionFlowInputDatastructures)
                {
                    ProductionFlowInputDatastructureMod productionFlowInputDatastructureMod = new ProductionFlowInputDatastructureMod();
                    productionFlowInputDatastructureMod.Process = item.Process;
                    productionFlowInputDatastructureMod.ProductionDates = GetProductionDate(model.ProductionMonthDate.Year, model.ProductionMonthDate.Month);
                    productionFlowInputDatastructureMod.AssetName = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == model.AssetNodeId).Name;

                    List<KPApfVM> kPApfVMs = new List<KPApfVM>();


                    foreach (var i in item.KeyProcessAreas)
                    {
                        if (i.IsTargetSet == true)
                        {
                            KPApfVM kPApfVM = new KPApfVM();
                            kPApfVM.KeyProcessAreaId = i.KeyProcessAreaId;
                            kPApfVM.AssetNodeId = i.AssetNodeId;
                            kPApfVM.AssetName = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == i.AssetNodeId).Name;
                            kPApfVM.ProcessId = i.ProcessId;
                            kPApfVM.Process = _context.Processes.FirstOrDefault(id => id.ProcessId == i.ProcessId);
                            kPApfVM.KeyProcessAreaName = _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == i.KeyProcessAreaId).KeyProcessAreaName;
                            kPApfVM.Color = i.Color;
                            kPApfVM.IsTargetSet = i.IsTargetSet;
                            kPApfVM.KPADate = i.KPADate;
                            kPApfVM.BackgroundColor = i.BackgroundColor;
                            kPApfVM.KeyProcessAreaTypeId = i.KeyProcessAreaTypeId;
                            kPApfVM.KPAType = _context.KeyProcessAreaTypes.FirstOrDefault(id => id.KeyProcessAreaTypeId == i.KeyProcessAreaTypeId).KeyProcessAreaTypeName;
                            kPApfVM.Frequency = _context.Frequencies.FirstOrDefault(id => id.KeyProcessAreaId == i.KeyProcessAreaId).FrequencyName;
                            kPApfVM.Target = _context.Targets.FirstOrDefault(id => id.KeyProcessAreaId == i.KeyProcessAreaId);
                            kPApfVM.TargetId = _context.Targets.FirstOrDefault(id => id.KeyProcessAreaId == i.KeyProcessAreaId).TargetId;
                            kPApfVM.TargetBudget = _context.Targets.FirstOrDefault(id => id.KeyProcessAreaId == i.KeyProcessAreaId).Budget;
                            kPApfVM.TargetThreshold = _context.Targets.FirstOrDefault(id => id.KeyProcessAreaId == i.KeyProcessAreaId).Threshold;
                            kPApfVM.TargetValue = _context.Targets.FirstOrDefault(id => id.KeyProcessAreaId == i.KeyProcessAreaId).TargetValue;
                            kPApfVM.Productions = GetOrderedProd(i.KeyProcessAreaId, _context.Targets.FirstOrDefault(id => id.KeyProcessAreaId == i.KeyProcessAreaId).TargetId);
                            kPApfVM.IsBuffer = i.KeyProcessAreaTypeId == 1 ? true : false;
                            kPApfVM.IsProcess = i.KeyProcessAreaTypeId == 2 ? true : false;
                            kPApfVM.FlagColor = GetFlag(i.KeyProcessAreaId, i.KPADate);

                            kPApfVMs.Add(kPApfVM);
                        }

                    }

                    productionFlowInputDatastructureMod.KeyProcessAreas = kPApfVMs;

                    productionFlowInputDatastructureMods.Add(productionFlowInputDatastructureMod);
                }

                return Ok(productionFlowInputDatastructureMods);

            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        public string GetFlag(int kpaId, DateTime productionDate)
        {

            if (productionDate > DateTime.Now)
            {
                return "none";
            }

            Production production = _context.Productions.FirstOrDefault(id => (id.KeyProcessAreaId == kpaId)
            && (id.Year == productionDate.Year) && (id.Month == productionDate.Month) && (id.Day == productionDate.Day));

            Target target = _context.Targets.FirstOrDefault(id => (id.KeyProcessAreaId == kpaId)
            && (id.Year == productionDate.Year) && (id.Month == productionDate.Month));

            if (production.Value >= target.TargetValue)
            {
                return "limegreen";
            }
            else if ((production.Value >= target.Threshold) && (production.Value < target.TargetValue))
            {
                return "orange";
            }
            else
            {
                return "red";
            }

        }

        public List<string> GetProductionDate(int year, int month)
        {
           List<DateTime> dateTimes =  getAllDates(year, month);
            List<string> dateTimeString = new List<string>();

            foreach (var item in dateTimes)
            {
                dateTimeString.Add(item.ToString("dd-MMM-yyyy"));
            }

            return dateTimeString;
        }

        public List<ProductionVM> GetOrderedProd(int KpaId, int targetId)
        {
            var productions = _context.Productions.Where(id => (id.KeyProcessAreaId == KpaId)
                            && (id.Year == _context.Targets.FirstOrDefault(id => id.KeyProcessAreaId == KpaId).Year)
                            && (id.Month == _context.Targets.FirstOrDefault(id => id.KeyProcessAreaId == KpaId).Month)).ToList();

            var prds = productions.Select(result => new ProductionVM
            {
                ProductionId = result.ProductionId,
                Year = result.Year,
                Month = result.Month,
                Day = result.Day,
                Reference = result.Reference,
                DateStamp = result.DateStamp,
                KeyProcessAreaId = result.KeyProcessAreaId,
                KeyProcessArea = result.KeyProcessArea,
                AssetNodeId = result.AssetNodeId,
                Value = result.Value,
                IsBuffer = _context.KeyProcessAreas.FirstOrDefault(id=>id.KeyProcessAreaId == result.KeyProcessAreaId).KeyProcessAreaTypeId == 1 ? true : false,
                IsProcess = _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == result.KeyProcessAreaId).KeyProcessAreaTypeId == 2 ? true : false,
                FlagColor = GetFlag(result.KeyProcessAreaId, new DateTime(result.Year, result.Month, result.Day))
        });

            var orderedProductions = prds.OrderBy(id => id.Day).ToList();

            return orderedProductions;
        }


        [HttpPost("kpaLimit")]
        public IActionResult SaveKpa([FromBody] Target model)
        {

            if (model == null)
            {
                return BadRequest("Error, make sure KPA target form is filled correctly.");
            }

            try
            {
                if (model.TargetId == 0)
                {
                    Process process = _context.Processes.FirstOrDefault(id => id.ProcessId == _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == model.KeyProcessAreaId).ProcessId);

                    model.DateStamp = DateTime.Now;
                    model.Month = process.ProcessDate.Month;
                    model.Year = process.ProcessDate.Year;

                    _context.Targets.Add(model);
                    _context.SaveChanges();

                    KeyProcessArea keyProcessArea = _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == model.KeyProcessAreaId);
                    keyProcessArea.IsTargetSet = true;
                    _context.KeyProcessAreas.Update(keyProcessArea);
                    _context.SaveChanges();
                }
                else
                {
                    Target target = _context.Targets.FirstOrDefault(id => id.TargetId == model.TargetId);
                    target.TargetValue = model.TargetValue;
                    target.Budget = model.Budget;
                    target.Threshold = model.Threshold;

                    _context.Targets.Update(target);
                    _context.SaveChanges();
                }



                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }

        }

        [HttpPost("updateKPA")]
        public IActionResult UpdateKPA([FromBody] KPAUpdate kpa)
        {

            if (kpa == null)
            {
                return BadRequest("Error, make sure KPA update form is filled correctly.");
            }

            KeyProcessArea k = _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == kpa.KeyProcessAreaId);

            try
            {
                k.KeyProcessAreaName = kpa.KeyProcessAreaName;
                k.Color = kpa.Color;
                k.BackgroundColor = kpa.BackgroundColor;
                k.KeyProcessAreaTypeId = kpa.KeyProcessAreaTypeId;

                _context.Frequencies.Remove(_context.Frequencies.FirstOrDefault(id => id.KeyProcessAreaId == kpa.KeyProcessAreaId));
                _context.SaveChanges();
                _context.KeyProcessAreas.Update(k);

                Frequency frequency = new Frequency();
                frequency.FrequencyName = kpa.Frequency;
                frequency.KeyProcessAreaId = kpa.KeyProcessAreaId;

                _context.Frequencies.Add(frequency);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }

        }

        [HttpPost("updateProcesses")]
        public IActionResult SaveProcesses([FromBody] ProcessUpdate process)
        {

            if (process == null)
            {
                return BadRequest("Error, make sure process form is filled correctly.");
            }

            Process p = _context.Processes.FirstOrDefault(id => id.ProcessId == process.ProcessId);

            try
            {
                p.ProcessName = process.ProcessName;
                p.Color = process.Color;
                p.BackgroundColor = process.BackgroundColor;


                _context.Processes.Update(p);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }

        }

        [HttpPost("processes")]
        public IActionResult SaveProcesses([FromBody] Process process)
        {

            if (process == null)
            {
                return BadRequest("Error, make sure process form is filled correctly.");
            }

            Process processChecker = _context.Processes.FirstOrDefault(id => 
            (id.AssetNodeId == process.AssetNodeId) && (id.ProcessName == process.ProcessName) 
            && (id.ProcessDate.Year == process.ProcessDate.Year) && (id.ProcessDate.Month == process.ProcessDate.Month));

            if (processChecker != null)
            {
                return BadRequest("Error, There is already an existing process. Choose a different process name or edit an existing one.");
            }

            try
            {
                AssetNode parentAssetNode = _context.AssetNodes.FirstOrDefault(id=>id.AssetNodeId == _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == process.AssetNodeId).ParentAssetNodeId);
                Process p = _context.Processes.FirstOrDefault(id => (id.parentAssetNodeId == parentAssetNode.AssetNodeId) && (id.IsSummary == true));

                var pr = process;

                Process tempProcess = new Process();
                tempProcess.AssetNodeId = process.AssetNodeId;
                tempProcess.BackgroundColor = process.BackgroundColor;
                tempProcess.Color = process.Color;
                tempProcess.ProcessDate = process.ProcessDate;
                tempProcess.ProcessName = process.ProcessName;
                tempProcess.Reference = process.Reference;
                tempProcess.DateStamp = DateTime.Now;

                if (p == null)
                {
                    Process proc = process;

                    proc.parentAssetNodeId = parentAssetNode.AssetNodeId;
                    proc.IsSummary = true;
                    proc.ProcessName = "Summary/Overall Production";
                    proc.DateStamp = DateTime.Now;
                    proc.Color = "white";
                    proc.BackgroundColor = "navy";
                    proc.Reference = process.Reference;

                    _context.Processes.Add(proc);
                    _context.SaveChanges();
                }


                _context.Processes.Add(tempProcess);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }

        }


        [HttpGet("mathsOperator")]
        public IActionResult GetOperators()
        {
            List<MathematicalOperator> mathematicalOperators = _context.MathematicalOperators.ToList();

            return Ok(mathematicalOperators);
        }


        [HttpPost("formularCreation")]
        public IActionResult CreateKPAFormula([FromBody] FormulaCreationVM formulaCreation)
        {
            if (formulaCreation == null)
            {
                return BadRequest("Something bad happened.");
            }

            try
            {
                //Check formular existance and delete before adding
                List<FormulaCreation> formulaCreations1 = _context.FormulaCreations.Where(id => id.FormularOwnerKPAId == formulaCreation.keyProcessAreaId).ToList();

                if (formulaCreations1.Count != 0)
                {
                    _context.FormulaCreations.RemoveRange(formulaCreations1);
                    _context.SaveChanges();
                }

                List<FormulaCreation> formulaCreations = new List<FormulaCreation>();

                for (int i = 0; i < formulaCreation.KPAIds.Count; i++)
                {
                    FormulaCreation formula = new FormulaCreation();
                    formula.KeyProcessAreaId = formulaCreation.KPAIds[i];
                    formula.FormularOwnerKPAId = formulaCreation.keyProcessAreaId;
                }

                for (int i = 0; i < formulaCreation.OpsIds.Count; i++)
                {
                    FormulaCreation formula = new FormulaCreation();
                    formula.MathematicalOperatorId = formulaCreation.OpsIds[i];
                    formula.FormularOwnerKPAId = formulaCreation.keyProcessAreaId;
                    formulaCreations.Add(formula);
                }

                //var t = formulaCreations;

                //_context.FormulaCreations.AddRange(formulaCreations);
                //_context.SaveChanges();
                

                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. Make sure formula is crafted correclty"+ Ex.Message);
            }

        }


        [HttpGet("GetClientAssetKPAs")]
        public IActionResult GetKPATarget(int keyProcessAreaId)
        {
            if (keyProcessAreaId == 0)
            {
                return BadRequest("Make sure KPA is selected.");
            }

            try
            {
                //Find asset node
                int assetNodeId = _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == keyProcessAreaId).AssetNodeId;

                int parentAssetNodeId = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == assetNodeId).ParentAssetNodeId;

                List<AssetNode> assetNodesWithParents = _context.AssetNodes.Where(id => id.ParentAssetNodeId == parentAssetNodeId).ToList();

                List<KeyProcessArea> keyProcessAreas1 = new List<KeyProcessArea>();

                foreach (var item in assetNodesWithParents)
                {
                    List<KeyProcessArea> keyProcessAreas = _context.KeyProcessAreas.Where(id => id.AssetNodeId == item.AssetNodeId).ToList();

                    foreach (var kpa in keyProcessAreas)
                    {
                        keyProcessAreas1.Add(kpa);
                    }
                }

                var allKPAs = keyProcessAreas1.Select(result => new
                {
                    AssetNodeId = result.AssetNodeId,
                    KPAName = "[" + _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == result.AssetNodeId).Code + "] " + _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == result.KeyProcessAreaId).KeyProcessAreaName 
                    +" | "+ _context.KeyProcessAreaTypes.FirstOrDefault(id => id.KeyProcessAreaTypeId == result.KeyProcessAreaTypeId).KeyProcessAreaTypeName,
                    KPAId = result.KeyProcessAreaId,
                });

                return Ok(allKPAs);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        [HttpPost("kpaTarget")]
        public IActionResult GetKPATarget([FromBody] KPATargetVM kPATargetVM)
        {
            if (kPATargetVM == null)
            {
                return BadRequest("Error, make sure KPA is selected correctly.");
            }

            try
            {
                KeyProcessArea keyProcessArea = _context.KeyProcessAreas.FirstOrDefault(id => id.KeyProcessAreaId == kPATargetVM.KeyProcessAreaId);
                DateTime processDate = _context.Processes.FirstOrDefault(id => id.ProcessId == keyProcessArea.ProcessId).ProcessDate;

                Target target = _context.Targets.FirstOrDefault(id => (id.KeyProcessAreaId == kPATargetVM.KeyProcessAreaId) &&
                (id.Year == processDate.Year) && (id.Month == processDate.Month));

                return Ok(target);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        [HttpGet("kpas")]
        public IActionResult GetKPAs(int processId)
        {
            if (processId == 0)
            {
                return BadRequest("Error, make sure process is selected correctly.");
            }

            try
            {
                List<KeyProcessArea> keyProcessAreas = _context.KeyProcessAreas.Where(id => id.ProcessId == processId).ToList();

                return Ok(keyProcessAreas);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }


        [HttpGet("colors")]
        public IActionResult GetColorPalletes()
        {

            try
            {
                List<ColorPallete> colorPalletes = _context.ColorPalletes.ToList();

                return Ok(colorPalletes);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }
    }
}
