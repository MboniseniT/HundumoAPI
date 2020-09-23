using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BinmakAPI.Data;
using BinmakBackEnd.Areas.Kwenza.Entities;
using BinmakBackEnd.Areas.Kwenza.Models;
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
    public class AssetsController : ControllerBase
    {
        private readonly BinmakDbContext _context;

        public AssetsController(BinmakDbContext context)
        {
            _context = context;
        }

        [HttpGet("assetTasks")]
        public IActionResult GetAssetTasks(int assetId)
        {
            try
            {
                var lAction = _context.Actions.Where(id => id.AssetId == assetId).ToList();

                var actions = lAction.Select(result => new
                {
                    AssetId = result.AssetId,
                    ActionName = result.ActionName,
                    Tasks = _context.DailyTasks.Where(id => (id.AssetId == assetId) && (id.ActionIndex == result.ActionIndex) 
                    && (id.DateProduction == result.DateProduction)).ToList(),
                    DateStamp = result.DateProduction,
                    Reference = _context.Users.FirstOrDefault(id => id.Id == result.Reference).FirstName + " " 
                    + _context.Users.FirstOrDefault(id => id.Id == result.Reference).LastName,
                });

                return Ok(actions);
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

        [HttpGet("getAssetById")]
        public IActionResult GetAsseById(int assetId)
        {
            try
            {
                var readings = _context.ProductionFlowAssets.FirstOrDefault(id => id.AssetId == assetId);


                return Ok(readings);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        [HttpPost("dailyReadings")]
        public IActionResult GetAssets([FromBody] AssetProdDate assetProdDate)
        {
            try
            {
                List<Reading> readings = new List<Reading>();
                ProductionFlowAsset asset = _context.ProductionFlowAssets.FirstOrDefault(id => id.AssetId == assetProdDate.AssetId);
                readings = _context.Readings.Where(id => (id.AssetId == assetProdDate.AssetId) && (id.DateProduction.Month == assetProdDate.ProductionDate.Month) 
                && (id.DateProduction.Year == assetProdDate.ProductionDate.Year)).ToList();

                if (asset == null)
                {
                    return BadRequest("Error, Make sure this is a productive unit! OR You are assigned to this asset node.");
                }

                if (readings.Count == 0)
                {
                    List<DateTime> dates = getAllDates(assetProdDate.ProductionDate.Year, assetProdDate.ProductionDate.Month);
                    List<Reading> rds = new List<Reading>();

                    foreach (var date in dates)
                    {
                        Reading reading = new Reading();
                        reading.DateProduction = date;
                        reading.AssetId = asset.AssetId;
                        reading.Reference = asset.Reference;

                        rds.Add(reading);

                    }

                    _context.Readings.AddRange(rds);
                    _context.SaveChanges();

                }


                var orderedReadings = readings.OrderBy(d => d.DateProduction);

                return Ok(orderedReadings);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        [HttpPost("overallProduction")]
        public IActionResult GetAssetOverallProduction([FromBody] OverallProductionFEInput overallProductionFEInput)
        {
            int assetId = overallProductionFEInput.AssetId;
            DateTime lDate = overallProductionFEInput.ProductionDate;

            if (assetId == 0)
            {
                return BadRequest("Something bad happened. Asset ID can not be null.");
            }

            try
            {

                ProductionFlowAsset asset = _context.ProductionFlowAssets.FirstOrDefault(id => id.AssetId == assetId);
                List<OverallProductionFunctionUnitObject> overallProductionFunctionUnitObjects = new List<OverallProductionFunctionUnitObject>();

                List<ProductionFlowAsset> assets = _context.ProductionFlowAssets.Where(id => id.ClientAssetNameId == 
                _context.ClientAssetNames.FirstOrDefault(id => id.ClientAssetNameId == asset.ClientAssetNameId).ClientAssetNameId).ToList();

                List<FunctionUnit> functionUnits = _context.FunctionUnits.Where(id => id.AssetId == assetId).ToList();
                List<FunctionUnitChildren> functionUnitsChildren = _context.FunctionUnitChildrens.Where(id => id.AssetId == assetId).ToList();

                List<FunctionUnitObject> fub = new List<FunctionUnitObject>();
                List<FunctionUnitObject> totalFub = new List<FunctionUnitObject>();


                //Adding Overall Production
                OverallProductionFunctionUnitObject firstOverallProductionFunctionUnitObjects = new OverallProductionFunctionUnitObject();
                firstOverallProductionFunctionUnitObjects.SiteName = _context.ClientAssetNames.FirstOrDefault(id => id.ClientAssetNameId == asset.ClientAssetNameId).AssetName + " TOTAL";

                FunctionUnitObject totalOfunctionUnitObject;
                List<string> tempfus = new List<string>();

                List<int> tempfvls;
                List<FunctionUnitObject> fubTotal = new List<FunctionUnitObject>();
                List<ListOfValues> listOfValues = new List<ListOfValues>();

                //Date for report
                DateTime dateTime = lDate;

                foreach (Reading reading in _context.Readings.Where(id => (id.DateProduction.Month == dateTime.Month) && (id.AssetId == assetId)).ToList().OrderBy(d => d.DateProduction))
                {
                    ListOfValues listOfValue = new ListOfValues();
                    tempfvls = new List<int>();
                    List<ValueObject> valueObjects = new List<ValueObject>();


                    tempfvls.Add(reading.DateProduction.Day);
                    valueObjects.Add(ValueObjectDate(reading));

                    tempfvls.Add(this.SheAsset(reading));
                    valueObjects.Add(SheAssetObject(reading));


                    //Hoisted Tons Commulative, We need one value here
                    var reading1 = new Reading();
                    var assets1 = new List<ProductionFlowAsset>();
                    reading1 = reading;
                    assets1 = assets;

                    TotalHoistedCommulataiveInputParam totalHoistedCommulataiveInputParam = new TotalHoistedCommulataiveInputParam();
                    totalHoistedCommulataiveInputParam.Reading = reading1;
                    totalHoistedCommulataiveInputParam.Asset = assets1;

                    valueObjects.Add(this.TotalTonsHoisetedCommulativeObject(totalHoistedCommulataiveInputParam));

                    foreach (var item in assets)
                    {
                        //tempfvls.Add(this.HoistedTonsPerSite(_context.Readings.FirstOrDefault(id => (id.DateProduction == reading.DateProduction) && (id.AssetId == item.AssetId))));
                        valueObjects.Add(this.HoistedTonsPerSiteObject(_context.Readings.FirstOrDefault(id => (id.DateProduction == reading.DateProduction) && (id.AssetId == item.AssetId))));
                    }
                    foreach (var item in assets)
                    {
                        //tempfvls.Add(this.EndsAvailablePerSite(_context.Readings.FirstOrDefault(id => (id.DateProduction == reading.DateProduction) && (id.AssetId == item.AssetId))));
                        valueObjects.Add(this.EndsAvailablePerSiteObjects(_context.Readings.FirstOrDefault(id => (id.DateProduction == reading.DateProduction) && (id.AssetId == item.AssetId))));
                    }

                    TotalHoistedTonsInputParam totalHoistedInputParam = new TotalHoistedTonsInputParam();
                    totalHoistedInputParam.Reading = reading1;
                    totalHoistedInputParam.Asset = assets1;

                    valueObjects.Add(this.TotalTonsHoiseted(totalHoistedInputParam));

                    int intCounter = tempfvls.Count;
                    int objectCounter = valueObjects.Count;

                    listOfValue.Readings = tempfvls.ToArray();
                    listOfValue.ReadingsObject = valueObjects;

                    listOfValues.Add(listOfValue);
                }

                tempfvls = new List<int>();


                tempfus.Add("Day of " + dateTime.ToString("MMM yyyy"));
                tempfus.Add("SHE Incidents");
                tempfus.Add("Hoisted Commulative Tons");

                foreach (var item in assets)
                {
                    tempfus.Add(item.SiteName + " Hoisted Tons");
                }
                foreach (var item in assets)
                {
                    tempfus.Add(item.SiteName + " Total Ends Available");
                }
                tempfus.Add("Total Tons Hoisted");

                totalOfunctionUnitObject = new FunctionUnitObject();
                totalOfunctionUnitObject.FunctionUnit = "Overall Production";
                List<FunctionUnitChildren> functionUnitChildrens = new List<FunctionUnitChildren>();

                foreach (var item in tempfus)
                {
                    FunctionUnitChildren functionUnitChildren = new FunctionUnitChildren();
                    functionUnitChildren.AssetId = 0;
                    functionUnitChildren.FunctionUnitChildrenName = item;
                    functionUnitChildren.FunctionUnitId = 0;
                    functionUnitChildren.FunctionUnitChildrenId = 0;

                    if (item.Contains("20"))
                    {
                        functionUnitChildren.MeasurementUnit = "";
                        functionUnitChildren.FunctionChildrenBachgroundColor = "rgb(255, 255, 255)";
                        functionUnitChildren.FunctionChildrenColor = "rgb(0, 0, 0)";
                        functionUnitChildren.Frequency = "Frequency";
                        functionUnitChildren.MonthlyTarget = "Month Target";
                        functionUnitChildren.Target = "Target";
                        functionUnitChildren.Budget = "Budget";
                        functionUnitChildren.Threshold = "Threshold";
                    }
                    else if (item.Contains("SHE Incidents"))
                    {
                        functionUnitChildren.FunctionChildrenBachgroundColor = "rgb(34, 184, 243)";
                        functionUnitChildren.FunctionChildrenColor = "rgb(5, 100, 155)";
                        functionUnitChildren.MeasurementUnit = "Process (Rate)";
                        functionUnitChildren.Frequency = "Per Day";
                        functionUnitChildren.MonthlyTarget = "0";
                        functionUnitChildren.Target = "0";
                        functionUnitChildren.Budget = "0";
                        functionUnitChildren.Threshold = "0";

                        var SheMonthlyTarget = GetSheMonthlyTarget(assets, lDate);
                        var SheTarget = GetSheTarget(assets, lDate);
                        var SheBudget = GetSheBudget(assets, lDate);
                        var SheThreshold = GetSheThreshold(assets, lDate);

                        functionUnitChildren.MonthlyTarget = SheMonthlyTarget.SheMonthlyTarget.ToString(); //Get Monthly Readings by asset and date, it happens once a month
                        functionUnitChildren.MonthlyTargetIsBackground = SheMonthlyTarget.IsBacfground;
                        functionUnitChildren.MonthlyTargetColor = SheMonthlyTarget.color;

                        functionUnitChildren.Target = SheTarget.SheTarget.ToString(); //Get Target Readings by asset and date, it happens once a month
                        functionUnitChildren.TargetIsBackground = SheTarget.IsBacfground;
                        functionUnitChildren.TargetColor = SheTarget.color;

                        functionUnitChildren.Budget = SheBudget.SheBudget.ToString(); //Get Target Readings by asset and date, it happens once a month
                        functionUnitChildren.BudgetIsBackground = SheBudget.IsBacfground;
                        functionUnitChildren.BudgetColor = SheBudget.color;

                        functionUnitChildren.Threshold = SheThreshold.SheThreshold.ToString(); //Get Target Readings by asset and date, it happens once a month
                        functionUnitChildren.ThresholdIsBackground = SheThreshold.IsBacfground;
                        functionUnitChildren.ThresholdColor = SheThreshold.color;

                    }
                    else if (item.Contains("Ends Available"))
                    {

                        var MonthlyTarget = GetMonthlyTarget(assets, lDate, item);
                        var Target = GetTarget(assets, lDate, item);
                        var Budget = GetBudget(assets, lDate, item);
                        var Threshold = GetThreshold(assets, lDate, item);

                        functionUnitChildren.FunctionChildrenBachgroundColor = "rgb(5, 100, 155)";
                        functionUnitChildren.FunctionChildrenColor = "rgb(255, 255, 255)";
                        functionUnitChildren.MeasurementUnit = "Buffer";
                        functionUnitChildren.Frequency = "Available";

                        functionUnitChildren.MonthlyTarget = MonthlyTarget.MonthlyTarget.ToString(); //Get Monthly Readings by asset and date, it happens once a month
                        functionUnitChildren.MonthlyTargetIsBackground = MonthlyTarget.IsBacfground;
                        functionUnitChildren.MonthlyTargetColor = MonthlyTarget.color;

                        functionUnitChildren.Target = Target.Target.ToString(); //Get Target Readings by asset and date, it happens once a month
                        functionUnitChildren.TargetIsBackground = Target.IsBacfground;
                        functionUnitChildren.TargetColor = Target.color;

                        functionUnitChildren.Budget = Budget.Budget.ToString(); //Get Target Readings by asset and date, it happens once a month
                        functionUnitChildren.BudgetIsBackground = Budget.IsBacfground;
                        functionUnitChildren.BudgetColor = Budget.color;

                        functionUnitChildren.Threshold = Threshold.Threshold.ToString(); //Get Target Readings by asset and date, it happens once a month
                        functionUnitChildren.ThresholdIsBackground = Threshold.IsBacfground;
                        functionUnitChildren.ThresholdColor = Threshold.color;
                    }
                    else if (item.EndsWith("Total Tons Hoisted"))
                    {
                        var MonthlyTarget = GetMonthlyTargetTT(assets, lDate, item);
                        var Target = GetTargetTT(assets, lDate, item);
                        var Budget = GetBudgetTT(assets, lDate, item);
                        var Threshold = GetThresholdTT(assets, lDate, item);

                        functionUnitChildren.FunctionChildrenBachgroundColor = "rgb(34, 184, 243)";
                        functionUnitChildren.FunctionChildrenColor = "rgb(5, 100, 155)";
                        functionUnitChildren.MeasurementUnit = "Process Rate";
                        functionUnitChildren.Frequency = "Per Day";

                        functionUnitChildren.MonthlyTarget = MonthlyTarget.MonthlyTarget.ToString();
                        functionUnitChildren.MonthlyTargetColor = MonthlyTarget.color;
                        functionUnitChildren.MonthlyTargetIsBackground = MonthlyTarget.IsBacfground;

                        functionUnitChildren.Target = Target.Target.ToString();
                        functionUnitChildren.TargetColor = Target.color;
                        functionUnitChildren.TargetIsBackground = Target.IsBacfground;

                        functionUnitChildren.Budget = Budget.Budget.ToString();
                        functionUnitChildren.BudgetColor = Budget.color;
                        functionUnitChildren.BudgetIsBackground = Budget.IsBacfground;

                        functionUnitChildren.Threshold = Threshold.Threshold.ToString();
                        functionUnitChildren.ThresholdColor = Threshold.color;
                        functionUnitChildren.ThresholdIsBackground = Threshold.IsBacfground;

                    }
                    else if (item.EndsWith("Commulative Tons"))
                    {
                        var MonthlyTarget = GetMonthlyTargetCT(assets, lDate, item);
                        var Target = GetTargetCT(assets, lDate, item);
                        var Budget = GetBudgetCT(assets, lDate, item);
                        var Threshold = GetThresholdCT(assets, lDate, item);

                        functionUnitChildren.FunctionChildrenBachgroundColor = "rgb(34, 184, 243)";
                        functionUnitChildren.FunctionChildrenColor = "rgb(5, 100, 155)";

                        functionUnitChildren.MeasurementUnit = "Process (Rate)";
                        functionUnitChildren.Frequency = "For Month";

                        functionUnitChildren.MonthlyTarget = MonthlyTarget.MonthlyTarget.ToString();
                        functionUnitChildren.MonthlyTargetColor = MonthlyTarget.color;
                        functionUnitChildren.MonthlyTargetIsBackground = MonthlyTarget.IsBacfground;

                        functionUnitChildren.Target = Target.Target.ToString();
                        functionUnitChildren.TargetColor = Target.color;
                        functionUnitChildren.TargetIsBackground = Target.IsBacfground;

                        functionUnitChildren.Budget = Budget.Budget.ToString();
                        functionUnitChildren.BudgetColor = Budget.color;
                        functionUnitChildren.BudgetIsBackground = Budget.IsBacfground;

                        functionUnitChildren.Threshold = Threshold.Threshold.ToString();
                        functionUnitChildren.ThresholdColor = Threshold.color;
                        functionUnitChildren.ThresholdIsBackground = Threshold.IsBacfground;

                    }

                    else if (item.Contains("Hoisted Tons"))
                    {
                        var MonthlyTarget = GetMonthlyTargetTH(assets, lDate, item);
                        var Target = GetTargetTH(assets, lDate, item);
                        var Budget = GetBudgetTH(assets, lDate, item);
                        var Threshold = GetThresholdTH(assets, lDate, item);

                        functionUnitChildren.FunctionChildrenBachgroundColor = "rgb(34, 184, 243)";
                        functionUnitChildren.FunctionChildrenColor = "rgb(5, 100, 155)";

                        functionUnitChildren.MeasurementUnit = "Process (Rate)";
                        functionUnitChildren.Frequency = "For Month";

                        functionUnitChildren.MonthlyTarget = MonthlyTarget.MonthlyTarget.ToString();
                        functionUnitChildren.MonthlyTargetColor = MonthlyTarget.color;
                        functionUnitChildren.MonthlyTargetIsBackground = MonthlyTarget.IsBacfground;

                        functionUnitChildren.Target = Target.Target.ToString();
                        functionUnitChildren.TargetColor = Target.color;
                        functionUnitChildren.TargetIsBackground = Target.IsBacfground;

                        functionUnitChildren.Budget = Budget.Budget.ToString();
                        functionUnitChildren.BudgetColor = Budget.color;
                        functionUnitChildren.BudgetIsBackground = Budget.IsBacfground;

                        functionUnitChildren.Threshold = Threshold.Threshold.ToString();
                        functionUnitChildren.ThresholdColor = Threshold.color;
                        functionUnitChildren.ThresholdIsBackground = Threshold.IsBacfground;
                    }
                    else
                    {
                        functionUnitChildren.FunctionChildrenBachgroundColor = "rgb(34, 184, 243)";
                        functionUnitChildren.FunctionChildrenColor = "rgb(5, 100, 155)";
                        functionUnitChildren.MeasurementUnit = "Process (Rate)";
                        functionUnitChildren.Frequency = "For Month";
                        functionUnitChildren.MonthlyTarget = "0";
                        functionUnitChildren.Target = "0";
                        functionUnitChildren.Budget = "0";
                        functionUnitChildren.Threshold = "0";
                    }
                    functionUnitChildrens.Add(functionUnitChildren);
                }

                totalOfunctionUnitObject.FunctionChildren = functionUnitChildrens;
                totalOfunctionUnitObject.FunctionUnitValues = listOfValues.ToArray();
                fubTotal.Add(totalOfunctionUnitObject);

                firstOverallProductionFunctionUnitObjects.FunctionUnitObjects = fubTotal;
                overallProductionFunctionUnitObjects.Add(firstOverallProductionFunctionUnitObjects);

                List<FunctionUnitChildren> fuChildrenNames;

                foreach (FunctionUnit fu in functionUnits)
                {
                    List<ListOfValues> listOfValues1 = new List<ListOfValues>();

                    fuChildrenNames = new List<FunctionUnitChildren>();
                    FunctionUnitObject functionUnitObject = new FunctionUnitObject();
                    int fuIdTemp = fu.FunctionUnitId;
                    List<string> fuChildrenNamesTemp = new List<string>();
                    List<int> tempfvls1 = new List<int>();
                    ListOfValues listOfValue1 = new ListOfValues();
                    foreach (FunctionUnitChildren fuc in functionUnitsChildren.Where(id => id.FunctionUnitId == fuIdTemp).ToList())
                    {
                        fuChildrenNames.Add(fuc);

                    }

                    //List of function unit children 
                    List<FunctionUnitChildren> functionUnitChildrensHeader = new List<FunctionUnitChildren>();
                    functionUnitChildrensHeader = fuChildrenNames;

                    List<FunctionUnitChildren> updatedFunctionUnitChildren = GetUpdatedFUC(functionUnitChildrensHeader, dateTime);


                    var readings = _context.Readings.Where(id => (id.DateProduction.Month == dateTime.Month) && (id.AssetId == assetId)).ToList();
                    var orderedReadings = readings.OrderBy(id => id.DateProduction);

                    foreach (var item in orderedReadings)
                    {
                        var vls = ValueCalculatorMonthlyTarget(item.AssetId, item.DateProduction, fu);
                        int counter = vls.Count();

                        List<ValueObject> temp = new List<ValueObject>();

                        if (assets.Count > 0)
                        {
                            var output = counter - 1;
                            temp = vls.Last().ToList();
                        }

                        listOfValue1.ReadingsObject = temp;
                        listOfValues.Add(listOfValue1);

                    }

                    functionUnitObject.FunctionUnitValues = listOfValues1.ToArray();

                    functionUnitObject.FunctionUnit = fu.FunctionUnitName;
                    functionUnitObject.FunctionChildren = updatedFunctionUnitChildren;

                    fub.Add(functionUnitObject);
                }


                foreach (var item in assets)
                {
                    OverallProductionFunctionUnitObject overallProductionFunctionUnitObject = new OverallProductionFunctionUnitObject();
                    overallProductionFunctionUnitObject.SiteName = item.SiteName;
                    overallProductionFunctionUnitObject.AssetId = item.AssetId;
                    overallProductionFunctionUnitObject.FunctionUnitObjects = fub;
                    overallProductionFunctionUnitObjects.Add(overallProductionFunctionUnitObject);
                }

                List<OverallProductionFunctionUnitObject> functionUnitObjectsTempLastList = functionUnitObjects(overallProductionFunctionUnitObjects, lDate);

                return Ok(functionUnitObjectsTempLastList);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }

        }

        public SheTargetObject GetSheTarget(List<ProductionFlowAsset> assets, DateTime dateTime)
        {
            int sheMonthlyValue = 0;
            List<int> assetsIds = new List<int>();

            SheTargetObject monthlyTargetObject = new SheTargetObject();

            foreach (var asset in assets)
            {
                assetsIds.Add(asset.AssetId);
            }

            foreach (var i in assetsIds)
            {
                sheMonthlyValue = sheMonthlyValue + _context.Readings.FirstOrDefault(id => id.AssetId == i).SheTarget;
            }

            monthlyTargetObject.SheTarget = sheMonthlyValue;
            if (sheMonthlyValue > 0)
            {
                monthlyTargetObject.color = "red";
            }
            else
            {
                monthlyTargetObject.color = "limegreen";
            }

            monthlyTargetObject.IsBacfground = false;

            return monthlyTargetObject;
        }

        public SheBudgetObject GetSheBudget(List<ProductionFlowAsset> assets, DateTime dateTime)
        {
            int sheMonthlyValue = 0;
            List<int> assetsIds = new List<int>();

            SheBudgetObject monthlyTargetObject = new SheBudgetObject();

            foreach (var asset in assets)
            {
                assetsIds.Add(asset.AssetId);
            }

            foreach (var i in assetsIds)
            {
                sheMonthlyValue = sheMonthlyValue + _context.Readings.FirstOrDefault(id => id.AssetId == i).SheBudget;
            }

            monthlyTargetObject.SheBudget = sheMonthlyValue;
            if (sheMonthlyValue > 0)
            {
                monthlyTargetObject.color = "red";
            }
            else
            {
                monthlyTargetObject.color = "limegreen";
            }

            monthlyTargetObject.IsBacfground = false;

            return monthlyTargetObject;
        }

        public SheThresholdObject GetSheThreshold(List<ProductionFlowAsset> assets, DateTime dateTime)
        {
            int sheMonthlyValue = 0;
            List<int> assetsIds = new List<int>();

            SheThresholdObject monthlyTargetObject = new SheThresholdObject();

            foreach (var asset in assets)
            {
                assetsIds.Add(asset.AssetId);
            }

            foreach (var i in assetsIds)
            {
                sheMonthlyValue = sheMonthlyValue + _context.Readings.FirstOrDefault(id => id.AssetId == i).SheThreshold;
            }

            monthlyTargetObject.SheThreshold = sheMonthlyValue;
            if (sheMonthlyValue > 0)
            {
                monthlyTargetObject.color = "red";
            }
            else
            {
                monthlyTargetObject.color = "limegreen";
            }

            monthlyTargetObject.IsBacfground = false;

            return monthlyTargetObject;
        }


        public SheMonthlyTargetObject GetSheMonthlyTarget(List<ProductionFlowAsset> assets, DateTime dateTime)
        {
            int sheMonthlyValue = 0;
            List<int> assetsIds = new List<int>();

            SheMonthlyTargetObject monthlyTargetObject = new SheMonthlyTargetObject();

            foreach (var asset in assets)
            {
                assetsIds.Add(asset.AssetId);
            }

            foreach (var i in assetsIds)
            {
                sheMonthlyValue = sheMonthlyValue + _context.Readings.FirstOrDefault(id => id.AssetId == i).SheMonthTarget;
            }

            monthlyTargetObject.SheMonthlyTarget = sheMonthlyValue;
            if (sheMonthlyValue > 0)
            {
                monthlyTargetObject.color = "red";
            }
            else
            {
                monthlyTargetObject.color = "limegreen";
            }

            monthlyTargetObject.IsBacfground = false;

            return monthlyTargetObject;
        }

        public MonthlyTargetTotalAvailableEndsObject GetMonthlyTargetTH(List<ProductionFlowAsset> assets, DateTime dateTime, string label)
        {

            var text = Regex.Replace(label, " Hoisted Tons", "");
            ProductionFlowAsset asset = _context.ProductionFlowAssets.FirstOrDefault(id => id.SiteName == text);

            MonthlyTargetTotalAvailableEndsObject monthlyTargetObject = new MonthlyTargetTotalAvailableEndsObject();
            Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == asset.AssetId) && (id.DateProduction.Year == dateTime.Year) && (id.DateProduction.Month == dateTime.Month));
            monthlyTargetObject.MonthlyTarget = reading.HoistedTonsMonthTarget;
            monthlyTargetObject.color = "limegreen";
            monthlyTargetObject.IsBacfground = false;

            return monthlyTargetObject;
        }

        //Sum of Hoisted tons per site
        public MonthlyTargetTotalAvailableEndsObject GetMonthlyTargetCT(List<ProductionFlowAsset> assets, DateTime dateTime, string label)
        {
            int monthlyTarget = 0;

            MonthlyTargetTotalAvailableEndsObject monthlyTargetObject = new MonthlyTargetTotalAvailableEndsObject();

            foreach (var item in assets)
            {
                Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && (id.DateProduction.Year == dateTime.Year) 
                && (id.DateProduction.Month == dateTime.Month));

                if (reading == null)
                {
                    AssetProdDate assetProdDate = new AssetProdDate();
                    assetProdDate.ProductionDate = dateTime;
                    assetProdDate.AssetId = item.AssetId;
                    var newReadings = PopulateReadings(assetProdDate);
                    reading = newReadings.FirstOrDefault(id => (id.AssetId == item.AssetId) && (id.DateProduction.Year == dateTime.Year) && (id.DateProduction.Month == dateTime.Month));
                }

                monthlyTarget = monthlyTarget + reading.HoistedTonsMonthTarget;
            }
            monthlyTargetObject.MonthlyTarget = monthlyTarget;
            monthlyTargetObject.color = "limegreen";
            monthlyTargetObject.IsBacfground = false;

            return monthlyTargetObject;
        }

        public MonthlyTargetTotalAvailableEndsObject GetMonthlyTargetTT(List<ProductionFlowAsset> assets, DateTime dateTime, string label)
        {
            int monthlyTarget = 0;

            MonthlyTargetTotalAvailableEndsObject monthlyTargetObject = new MonthlyTargetTotalAvailableEndsObject();

            foreach (var item in assets)
            {
                Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && (id.DateProduction.Year == dateTime.Year) && (id.DateProduction.Month == dateTime.Month));
                monthlyTarget = monthlyTarget + reading.HoistedTonsMonthTarget;
            }

            monthlyTargetObject.MonthlyTarget = monthlyTarget /*/ System.DateTime.DaysInMonth(dateTime.Year, dateTime.Month)*/;
            monthlyTargetObject.color = "limegreen";
            monthlyTargetObject.IsBacfground = false;

            return monthlyTargetObject;
        }

        public TargetTotalAvailableEndsObject GetTargetTT(List<ProductionFlowAsset> assets, DateTime dateTime, string label)
        {
            int monthlyTarget = 0;

            TargetTotalAvailableEndsObject targetObject = new TargetTotalAvailableEndsObject();

            foreach (var item in assets)
            {
                Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && (id.DateProduction.Year == dateTime.Year) && (id.DateProduction.Month == dateTime.Month));
                monthlyTarget = monthlyTarget + reading.HoistedTonsTarget;
            }
            targetObject.Target = monthlyTarget /*/ System.DateTime.DaysInMonth(dateTime.Year, dateTime.Month)*/;
            targetObject.color = "limegreen";
            targetObject.IsBacfground = false;

            return targetObject;
        }

        public TargetTotalAvailableEndsObject GetTargetCT(List<ProductionFlowAsset> assets, DateTime dateTime, string label)
        {
            int Target = 0;

            TargetTotalAvailableEndsObject TargetObject = new TargetTotalAvailableEndsObject();

            foreach (var item in assets)
            {
                Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && (id.DateProduction.Year == dateTime.Year) && (id.DateProduction.Month == dateTime.Month));
                Target = Target + reading.HoistedTonsTarget;
            }
            TargetObject.Target = Target;
            TargetObject.color = "limegreen";
            TargetObject.IsBacfground = false;

            return TargetObject;
        }

        public ThresholdTotalAvailableEndsObject GetThresholdCT(List<ProductionFlowAsset> assets, DateTime dateTime, string label)
        {
            int threshold = 0;

            ThresholdTotalAvailableEndsObject thresholdObject = new ThresholdTotalAvailableEndsObject();

            foreach (var item in assets)
            {
                Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && (id.DateProduction.Year == dateTime.Year) && (id.DateProduction.Month == dateTime.Month));
                threshold = threshold + reading.HoistedTonsThreshold;
            }
            thresholdObject.Threshold = threshold;
            thresholdObject.color = "red";
            thresholdObject.IsBacfground = false;

            return thresholdObject;
        }

        public ThresholdTotalAvailableEndsObject GetThresholdTT(List<ProductionFlowAsset> assets, DateTime dateTime, string label)
        {
            int threshold = 0;

            ThresholdTotalAvailableEndsObject thresholdObject = new ThresholdTotalAvailableEndsObject();

            foreach (var item in assets)
            {
                Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && 
                (id.DateProduction.Year == dateTime.Year) && (id.DateProduction.Month == dateTime.Month));
                threshold = threshold + reading.HoistedTonsThreshold;
            }
            thresholdObject.Threshold = threshold /*/ System.DateTime.DaysInMonth(dateTime.Year, dateTime.Month)*/;
            thresholdObject.color = "red";
            thresholdObject.IsBacfground = false;

            return thresholdObject;
        }

        public BudgetTotalAvailableEndsObject GetBudgetTT(List<ProductionFlowAsset> assets, DateTime dateTime, string label)
        {
            int budget = 0;

            BudgetTotalAvailableEndsObject targetObject = new BudgetTotalAvailableEndsObject();

            foreach (var item in assets)
            {
                Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && (id.DateProduction.Year == dateTime.Year) 
                && (id.DateProduction.Month == dateTime.Month));
                budget = budget + reading.HoistedTonsBudget;
            }
            targetObject.Budget = budget /*/ System.DateTime.DaysInMonth(dateTime.Year, dateTime.Month)*/;
            targetObject.color = "orange";
            targetObject.IsBacfground = false;

            return targetObject;
        }

        public BudgetTotalAvailableEndsObject GetBudgetCT(List<ProductionFlowAsset> assets, DateTime dateTime, string label)
        {
            int budget = 0;

            BudgetTotalAvailableEndsObject targetObject = new BudgetTotalAvailableEndsObject();

            foreach (var item in assets)
            {
                Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && (id.DateProduction.Year == dateTime.Year) && (id.DateProduction.Month == dateTime.Month));
                budget = budget + reading.HoistedTonsBudget;
            }
            targetObject.Budget = budget;
            targetObject.color = "orange";
            targetObject.IsBacfground = false;

            return targetObject;
        }

        public TargetTotalAvailableEndsObject GetTargetTH(List<ProductionFlowAsset> assets, DateTime dateTime, string label)
        {

            var text = Regex.Replace(label, " Hoisted Tons", "");
            ProductionFlowAsset asset = _context.ProductionFlowAssets.FirstOrDefault(id => id.SiteName == text);

            TargetTotalAvailableEndsObject monthlyTargetObject = new TargetTotalAvailableEndsObject();
            Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == asset.AssetId) && (id.DateProduction.Year == dateTime.Year) && (id.DateProduction.Month == dateTime.Month));
            monthlyTargetObject.Target = reading.HoistedTonsTarget;
            monthlyTargetObject.color = "limegreen";
            monthlyTargetObject.IsBacfground = false;

            return monthlyTargetObject;
        }

        public BudgetTotalAvailableEndsObject GetBudgetTH(List<ProductionFlowAsset> assets, DateTime dateTime, string label)
        {

            var text = Regex.Replace(label, " Hoisted Tons", "");
            ProductionFlowAsset asset = _context.ProductionFlowAssets.FirstOrDefault(id => id.SiteName == text);

            BudgetTotalAvailableEndsObject monthlyTargetObject = new BudgetTotalAvailableEndsObject();
            Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == asset.AssetId) && (id.DateProduction.Year == dateTime.Year) && (id.DateProduction.Month == dateTime.Month));
            monthlyTargetObject.Budget = reading.HoistedTonsBudget;
            monthlyTargetObject.color = "orange";
            monthlyTargetObject.IsBacfground = false;

            return monthlyTargetObject;
        }

        public ThresholdTotalAvailableEndsObject GetThresholdTH(List<ProductionFlowAsset> assets, DateTime dateTime, string label)
        {

            var text = Regex.Replace(label, " Hoisted Tons", "");
            ProductionFlowAsset asset = _context.ProductionFlowAssets.FirstOrDefault(id => id.SiteName == text);

            ThresholdTotalAvailableEndsObject monthlyTargetObject = new ThresholdTotalAvailableEndsObject();
            Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == asset.AssetId) && (id.DateProduction.Year == dateTime.Year) && (id.DateProduction.Month == dateTime.Month));
            monthlyTargetObject.Threshold = reading.HoistedTonsThreshold;
            monthlyTargetObject.color = "red";
            monthlyTargetObject.IsBacfground = false;

            return monthlyTargetObject;
        }

        public MonthlyTargetTotalAvailableEndsObject GetMonthlyTarget(List<ProductionFlowAsset> assets, DateTime dateTime, string label)
        {
            int MonthlyTarget = 0;
            //Extract asset name from label
            var text = Regex.Replace(label, " Total Ends Available", "");
            ProductionFlowAsset asset = _context.ProductionFlowAssets.FirstOrDefault(id => id.SiteName == text);

            MonthlyTargetTotalAvailableEndsObject monthlyTargetObject = new MonthlyTargetTotalAvailableEndsObject();
            Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == asset.AssetId) && (id.DateProduction.Year == dateTime.Year) && (id.DateProduction.Month == dateTime.Month));
            monthlyTargetObject.MonthlyTarget = reading.UnlashedEndsMonthTarget + reading.TotalCleanedEndsMonthTarget + reading.SupportedEndsMonthTarget + reading.PreparedMarkedEndsMonthTarget;
            monthlyTargetObject.color = "limegreen";
            monthlyTargetObject.IsBacfground = true;
            return monthlyTargetObject;
        }

        public TargetTotalAvailableEndsObject GetTarget(List<ProductionFlowAsset> assets, DateTime dateTime, string label)
        {
            int Target = 0;
            //Extract asset name from label
            var text = Regex.Replace(label, " Total Ends Available", "");
            ProductionFlowAsset asset = _context.ProductionFlowAssets.FirstOrDefault(id => id.SiteName == text);

            TargetTotalAvailableEndsObject TargetObject = new TargetTotalAvailableEndsObject();
            Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == asset.AssetId) && (id.DateProduction.Year == dateTime.Year) && (id.DateProduction.Month == dateTime.Month));
            TargetObject.Target = reading.UnlashedEndsTarget + reading.TotalCleanedEndsTarget + reading.SupportedEndsTarget + reading.PreparedMarkedEndsTarget;
            TargetObject.color = "limegreen";
            TargetObject.IsBacfground = true;
            return TargetObject;
        }

        public BudgetTotalAvailableEndsObject GetBudget(List<ProductionFlowAsset> assets, DateTime dateTime, string label)
        {
            int Budget = 0;
            //Extract asset name from label
            var text = Regex.Replace(label, " Total Ends Available", "");
            ProductionFlowAsset asset = _context.ProductionFlowAssets.FirstOrDefault(id => id.SiteName == text);

            BudgetTotalAvailableEndsObject BudgetObject = new BudgetTotalAvailableEndsObject();
            Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == asset.AssetId) && (id.DateProduction.Year == dateTime.Year) && (id.DateProduction.Month == dateTime.Month));
            BudgetObject.Budget = reading.UnlashedEndsBudget + reading.TotalCleanedEndsBudget + reading.SupportedEndsBudget + reading.PreparedMarkedEndsBudget;
            BudgetObject.color = "orange";
            BudgetObject.IsBacfground = true;
            return BudgetObject;
        }

        public ThresholdTotalAvailableEndsObject GetThreshold(List<ProductionFlowAsset> assets, DateTime dateTime, string label)
        {
            var text = Regex.Replace(label, " Total Ends Available", "");
            ProductionFlowAsset asset = _context.ProductionFlowAssets.FirstOrDefault(id => id.SiteName == text);

            ThresholdTotalAvailableEndsObject ThresholdObject = new ThresholdTotalAvailableEndsObject();
            Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == asset.AssetId) && (id.DateProduction.Year == dateTime.Year) && (id.DateProduction.Month == dateTime.Month));
            ThresholdObject.Threshold = reading.UnlashedEndsThreshold + reading.TotalCleanedEndsThreshold + reading.SupportedEndsThreshold + reading.PreparedMarkedEndsThreshold;
            ThresholdObject.color = "red";
            ThresholdObject.IsBacfground = true;
            return ThresholdObject;
        }

        public List<OverallProductionFunctionUnitObject> functionUnitObjects(List<OverallProductionFunctionUnitObject> overallProductionFunctionUnitObjects, DateTime monthDate)
        {
            List<OverallProductionFunctionUnitObject> functionUnitObjectsTempList = new List<OverallProductionFunctionUnitObject>();
            OverallProductionFunctionUnitObject overallProductionFunctionUnitObject;

            //AddTotal
            functionUnitObjectsTempList.Add(overallProductionFunctionUnitObjects.FirstOrDefault(id => id.AssetId == 0));

            foreach (var fub in overallProductionFunctionUnitObjects.Where(id => id.AssetId != 0))
            {
                FunctionUnitObject functionUnitObject = new FunctionUnitObject();

                overallProductionFunctionUnitObject = new OverallProductionFunctionUnitObject();

                overallProductionFunctionUnitObject.AssetId = fub.AssetId;
                overallProductionFunctionUnitObject.SiteName = fub.SiteName;
                overallProductionFunctionUnitObject.FunctionUnitObjects = getFunctionUnitObject(fub.AssetId, monthDate);

                functionUnitObjectsTempList.Add(overallProductionFunctionUnitObject);
            }

            return functionUnitObjectsTempList;
        }

        public List<FunctionUnitObject> getFunctionUnitObject(int assetId, DateTime monthDate)
        {
            List<FunctionUnitChildren> fuChildrenNames;
            List<FunctionUnitObject> fub = new List<FunctionUnitObject>();

            foreach (FunctionUnit fu in _context.FunctionUnits.Where(id => id.AssetId == assetId).ToList())
            {
                List<ListOfValues> listOfValues1 = new List<ListOfValues>();

                fuChildrenNames = new List<FunctionUnitChildren>();
                FunctionUnitObject functionUnitObject = new FunctionUnitObject();
                int fuIdTemp = fu.FunctionUnitId;
                List<string> fuChildrenNamesTemp = new List<string>();
                List<int> tempfvls1 = new List<int>();
                ListOfValues listOfValue1 = new ListOfValues();
                foreach (FunctionUnitChildren fuc in _context.FunctionUnitChildrens.Where(id => id.FunctionUnitId == fuIdTemp).ToList())
                {
                    fuChildrenNames.Add(fuc);
                }


                //End

                foreach (var item in _context.Readings.Where(id => (id.DateProduction.Month == monthDate.Month) && (id.AssetId == assetId)).ToList().OrderBy(d => d.DateProduction))
                {
                    listOfValue1 = new ListOfValues();

                    //Test
                    ListOfValues listOfValue2 = new ListOfValues();
                    var vls = ValueCalculatorMonthlyTarget(assetId, item.DateProduction, fu);
                    vls.ToList();
                    int counter = vls.Count();

                    List<ValueObject> temp = new List<ValueObject>();

                    if (counter > 0)
                    {
                        var output = counter - 1;
                        temp = vls.Last().ToList();
                    }
                    listOfValue1.ReadingsObject = temp;
                    listOfValues1.Add(listOfValue1);
                }

                List<FunctionUnitChildren> functionUnitChildrensHeader = new List<FunctionUnitChildren>();
                //Test Mbo Headings
                functionUnitChildrensHeader = fuChildrenNames;
                //Test Mbo Headings
                List<FunctionUnitChildren> updatedFunctionUnitChildren = GetUpdatedFUC(functionUnitChildrensHeader, monthDate);

                functionUnitObject.FunctionUnitValues = listOfValues1.ToArray();

                functionUnitObject.FunctionUnit = fu.FunctionUnitName;
                //Substitution Mbo
                //functionUnitObject.FunctionChildren = fuChildrenNames;
                functionUnitObject.FunctionChildren = updatedFunctionUnitChildren;

                fub.Add(functionUnitObject);
            }

            return fub;
        }

        public List<OverallProductionFunctionUnitObject> ModifyOverallProd(List<ProductionFlowAsset> assets, DateTime dateProduction)
        {
            List<OverallProductionFunctionUnitObject> temp = new List<OverallProductionFunctionUnitObject>();
            List<OverallProductionFunctionUnitObject> overallProductionFunctionUnitObjects = new List<OverallProductionFunctionUnitObject>();
            List<FunctionUnitObject> fub = new List<FunctionUnitObject>();
            int counter = 0;

            foreach (ProductionFlowAsset asset in assets)
            {
                counter = counter + 1;

                List<FunctionUnitChildren> fuChildrenNames;

                foreach (FunctionUnit fu in _context.FunctionUnits.Where(id => id.AssetId == asset.AssetId))
                {
                    List<ListOfValues> listOfValues1 = new List<ListOfValues>();

                    fuChildrenNames = new List<FunctionUnitChildren>();
                    FunctionUnitObject functionUnitObject = new FunctionUnitObject();
                    int fuIdTemp = fu.FunctionUnitId;
                    ListOfValues listOfValue1 = new ListOfValues();

                    foreach (FunctionUnitChildren fuc in _context.FunctionUnitChildrens.Where(id => (id.FunctionUnitId == fuIdTemp) && (id.AssetId == asset.AssetId)).ToList())
                    {
                        fuChildrenNames.Add(fuc);
                    }

                    foreach (var item in _context.Readings.Where(id => (id.DateProduction.Month == dateProduction.Month) && (id.AssetId == asset.AssetId)).ToList().OrderBy(d => d.DateProduction))
                    {
                        listOfValue1 = new ListOfValues();
                        var values = ValueCalculator(item.AssetId, item.DateProduction, fu);
                        var lastValueRow = values.Last();
                        listOfValue1.LatestReadings = values;
                        listOfValue1.Readings = values.Last();
                        listOfValues1.Add(listOfValue1);
                    }

                    //List of function unit children 
                    List<FunctionUnitChildren> functionUnitChildrensHeader = new List<FunctionUnitChildren>();
                    //Test Mbo Headings
                    functionUnitChildrensHeader = fuChildrenNames;
                    //Test Mbo Headings
                    List<FunctionUnitChildren> updatedFunctionUnitChildren = GetUpdatedFUC(functionUnitChildrensHeader, dateProduction);

                    functionUnitObject.FunctionUnitValues = listOfValues1.ToArray();

                    if (assets.Count <= 1)
                    {
                        functionUnitObject.FunctionUnit = fu.FunctionUnitName;
                        functionUnitObject.FunctionChildren = fuChildrenNames;
                    }

                    //Test Mbo Headings
                    functionUnitObject.FunctionChildren = updatedFunctionUnitChildren;

                    fub.Add(functionUnitObject);
                }

                OverallProductionFunctionUnitObject overallProductionFunctionUnitObject = new OverallProductionFunctionUnitObject();
                overallProductionFunctionUnitObject.SiteName = asset.SiteName;
                overallProductionFunctionUnitObject.AssetId = asset.AssetId;
                overallProductionFunctionUnitObject.FunctionUnitObjects = fub;

                overallProductionFunctionUnitObjects.Add(overallProductionFunctionUnitObject);
            }

            return overallProductionFunctionUnitObjects;
        }


        public List<ValueObject[]> ValueCalculatorMonthlyTarget(int assetId, DateTime dateTime, FunctionUnit functionUnit)
        {
            List<ValueObject> values = new List<ValueObject>();
            List<ValueObject[]> totalList = new List<ValueObject[]>();

            Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == assetId) && (id.DateProduction == dateTime));

            List<FunctionUnit> fus = _context.FunctionUnits.Where(id =>/* (id.FunctionUnitId == functionUnit.FunctionUnitId) && (*/id.AssetId == reading.AssetId)/*)*/.ToList();

            foreach (FunctionUnit fu in fus)
            {
                List<FunctionUnitChildren> functionUnitChildrens = _context.FunctionUnitChildrens.Where(fu => fu.FunctionUnitId == functionUnit.FunctionUnitId).ToList();

                values = new List<ValueObject>();

                foreach (FunctionUnitChildren fuc in functionUnitChildrens)
                {
                    ValueObject valueObject = new ValueObject();

                    if (fuc.FunctionUnitChildrenName == "Day 1st 4 Hours Ends")
                    {
                        valueObject.Value = reading.Day1st4HoursEnds;
                        if (reading.Day1st4HoursEnds >= reading.Day1st4HoursEndsTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.Day1st4HoursEnds < reading.Day1st4HoursEndsTarget) && (reading.Day1st4HoursEnds >= reading.Day1st4HoursEndsThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        valueObject.IsBackground = false;
                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Night 1st 4 Hours Ends")
                    {
                        valueObject.Value = reading.Night1st4HoursEnds;

                        if (reading.Night1st4HoursEnds >= reading.Night1st4HoursEndsTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.Night1st4HoursEnds < reading.Night1st4HoursEndsTarget) && (reading.Night1st4HoursEnds >= reading.Night1st4HoursEndsThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }
                        valueObject.IsBackground = false;

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Ends Drilled")
                    {
                        valueObject.Value = reading.EndsDrilled;
                        if (reading.EndsDrilled >= reading.EndsDrilledTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.EndsDrilled < reading.EndsDrilledTarget) && (reading.EndsDrilled >= reading.EndsDrilledThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        valueObject.IsBackground = false;
                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Ends Blasted")
                    {
                        valueObject.Value = reading.EndsBlasted;

                        if (reading.EndsBlasted >= reading.EndsBlastedTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.EndsBlasted < reading.EndsBlastedTarget) && (reading.EndsBlasted >= reading.EndsBlastedThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        valueObject.IsBackground = false;
                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Unlashed Ends")
                    {
                        valueObject.Value = reading.UnlashedEnds;

                        if (reading.UnlashedEnds >= reading.UnlashedEndsTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.UnlashedEnds < reading.UnlashedEndsTarget) && (reading.UnlashedEnds >= reading.UnlashedEndsThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }

                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        valueObject.IsBackground = true;
                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Ends Lashed")
                    {
                        valueObject.Value = reading.EndsLashed;

                        if (reading.EndsLashed >= reading.EndsLashedTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.EndsLashed < reading.EndsLashedTarget) && (reading.EndsLashed >= reading.EndsLashedThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        valueObject.IsBackground = false;
                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Total Cleaned Ends")
                    {
                        valueObject.Value = reading.TotalCleanedEnds;

                        if (reading.TotalCleanedEnds >= reading.TotalCleanedEndsTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.TotalCleanedEnds < reading.TotalCleanedEndsTarget) && (reading.TotalCleanedEnds >= reading.TotalCleanedEndsThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        valueObject.IsBackground = true;
                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Lashed Prepared For Support")
                    {
                        valueObject.Value = reading.LashedPreparedForSupport;

                        if (reading.LashedPreparedForSupport >= reading.LashedPreparedForSupportTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.LashedPreparedForSupport < reading.LashedPreparedForSupportTarget) && (reading.LashedPreparedForSupport >= reading.LashedPreparedForSupportThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        valueObject.IsBackground = true;
                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Muckbay Tons")
                    {
                        valueObject.Value = reading.MuckbayTons;

                        if (reading.MuckbayTons >= reading.MuckbayTonsTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.MuckbayTons < reading.MuckbayTonsTarget) && (reading.MuckbayTons >= reading.MuckbayTonsThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        valueObject.IsBackground = true;
                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Hoisted Tons")
                    {
                        valueObject.Value = reading.HoistedTons;

                        if (reading.HoistedTons >= reading.HoistedTonsTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.HoistedTons < reading.HoistedTonsTarget) && (reading.HoistedTons >= reading.HoistedTonsThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        valueObject.IsBackground = false;
                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "UG Crusher Bin")
                    {
                        valueObject.Value = reading.UGCrusherBin;

                        if (reading.UGCrusherBin >= reading.UGCrusherBinTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.UGCrusherBin < reading.UGCrusherBinTarget) && (reading.UGCrusherBin >= reading.UGCrusherBinThreshold) && (reading.UGCrusherBin > 0))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        valueObject.IsBackground = true;
                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Ends Supported")
                    {
                        valueObject.Value = reading.EndsSupported;

                        if (reading.EndsSupported >= reading.EndsSupportedTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.EndsSupported < reading.EndsSupportedTarget) && (reading.EndsSupported >= reading.EndsSupportedThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        valueObject.IsBackground = false;
                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Supported Ends")
                    {
                        valueObject.Value = reading.SupportedEnds;

                        if (reading.SupportedEnds >= reading.SupportedEndsTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.SupportedEnds < reading.SupportedEndsTarget) && (reading.SupportedEnds >= reading.SupportedEndsThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        valueObject.IsBackground = true;
                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Ends Prepared")
                    {
                        valueObject.Value = reading.EndsPrepared;

                        if (reading.EndsPrepared >= reading.EndsPreparedTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.EndsPrepared < reading.EndsPreparedTarget) && (reading.EndsPrepared >= reading.EndsPreparedThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        valueObject.IsBackground = false;
                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Prepared Marked Ends")
                    {
                        valueObject.Value = reading.PreparedMarkedEnds;

                        if (reading.PreparedMarkedEnds >= reading.PreparedMarkedEndsTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.PreparedMarkedEnds < reading.PreparedMarkedEndsTarget) && (reading.PreparedMarkedEnds >= reading.PreparedMarkedEndsThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        valueObject.IsBackground = true;
                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Drill Rigs")
                    {
                        valueObject.Value = reading.DrillRigs;

                        if (reading.DrillRigs >= reading.DrillRigsTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.DrillRigs < reading.DrillRigsTarget) && (reading.DrillRigs >= reading.DrillRigsThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        valueObject.IsBackground = true;
                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "LHDs")
                    {
                        valueObject.Value = reading.LHDs;

                        if (reading.LHDs >= reading.LHDsTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.LHDs < reading.LHDsTarget) && (reading.LHDs >= reading.LHDsThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }


                        valueObject.IsBackground = true;
                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Dump Trucks")
                    {
                        valueObject.Value = reading.DumpTrucks;

                        if (reading.DumpTrucks >= reading.DumpTrucksTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.DumpTrucks < reading.DumpTrucksTarget) && (reading.DumpTrucks >= reading.DumpTrucksThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        valueObject.IsBackground = true;
                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Bolters")
                    {
                        valueObject.Value = reading.Bolters;

                        if (reading.Bolters >= reading.BoltersTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.Bolters < reading.BoltersTarget) && (reading.Bolters >= reading.BoltersThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        valueObject.IsBackground = true;
                        values.Add(valueObject);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Drifters")
                    {
                        valueObject.Value = reading.Drifters;

                        if (reading.Drifters >= reading.DriftersTarget)
                        {
                            valueObject.Color = "limegreen";
                        }
                        else if ((reading.Drifters < reading.DriftersTarget) && (reading.Drifters >= reading.DriftersThreshold))
                        {
                            valueObject.Color = "orange";
                        }
                        else
                        {
                            if (reading.DateProduction < DateTime.Now)
                            {
                                valueObject.Color = "red";
                            }
                            else
                            {
                                valueObject.Color = "none";
                            }
                        }

                        if (reading.DateProduction > DateTime.Now)
                        {
                            valueObject.Color = "none";
                        }

                        valueObject.IsBackground = true;
                        values.Add(valueObject);
                    }

                    totalList.Add(values.ToArray());
                }
            }
            return totalList;
        }


        public List<int[]> ValueCalculator(int assetId, DateTime dateTime, FunctionUnit functionUnit)
        {
            List<int> values = new List<int>();
            List<int[]> totalList = new List<int[]>();

            Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == assetId) && (id.DateProduction == dateTime));

            List<FunctionUnit> fus = _context.FunctionUnits.Where(id =>/* (id.FunctionUnitId == functionUnit.FunctionUnitId) && (*/id.AssetId == reading.AssetId)/*)*/.ToList();

            foreach (FunctionUnit fu in fus)
            {
                List<FunctionUnitChildren> functionUnitChildrens = _context.FunctionUnitChildrens.Where(fu => fu.FunctionUnitId == functionUnit.FunctionUnitId).ToList();

                values = new List<int>();

                foreach (FunctionUnitChildren fuc in functionUnitChildrens)
                {


                    if (fuc.FunctionUnitChildrenName == "Day 1st 4 Hours Ends")
                    {
                        values.Add(reading.Day1st4HoursEnds);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Night 1st 4 Hours Ends")
                    {
                        values.Add(reading.Night1st4HoursEnds);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Ends Drilled")
                    {
                        values.Add(reading.EndsDrilled);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Ends Blasted")
                    {
                        values.Add(reading.EndsBlasted);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Unlashed Ends")
                    {
                        values.Add(reading.UnlashedEnds);
                    }

                    if (fuc.FunctionUnitChildrenName == "Ends Lashed")
                    {
                        values.Add(reading.EndsLashed);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Total Cleaned Ends")
                    {
                        values.Add(reading.TotalCleanedEnds);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Lashed Prepared For Support")
                    {
                        values.Add(reading.LashedPreparedForSupport);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Muckbay Tons")
                    {
                        values.Add(reading.MuckbayTons);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Hoisted Tons")
                    {
                        values.Add(reading.HoistedTons);
                    }
                    else if (fuc.FunctionUnitChildrenName == "UG Crusher Bin")
                    {
                        values.Add(reading.UGCrusherBin);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Ends Supported")
                    {
                        values.Add(reading.EndsSupported);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Supported Ends")
                    {
                        values.Add(reading.SupportedEnds);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Ends Prepared")
                    {
                        values.Add(reading.EndsPrepared);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Prepared Marked Ends")
                    {
                        values.Add(reading.PreparedMarkedEnds);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Drill Rigs")
                    {
                        values.Add(reading.DrillRigs);
                    }
                    else if (fuc.FunctionUnitChildrenName == "LHDs")
                    {
                        values.Add(reading.LHDs);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Dump Trucks")
                    {
                        values.Add(reading.DumpTrucks);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Bolters")
                    {
                        values.Add(reading.Bolters);
                    }
                    else if (fuc.FunctionUnitChildrenName == "Drifters")
                    {
                        values.Add(reading.Drifters);
                    }

                    totalList.Add(values.ToArray());
                }
            }
            return totalList;
        }

        public List<FunctionUnitChildren> GetUpdatedFUC(List<FunctionUnitChildren> functionUnitChildrens, DateTime dateTime)
        {
            List<FunctionUnitChildren> tempFUC = new List<FunctionUnitChildren>();

            foreach (var fuc in functionUnitChildrens)
            {
                Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == functionUnitChildrens.FirstOrDefault().AssetId) && (id.DateProduction.Year == dateTime.Year) && (id.DateProduction.Month == dateTime.Month));

                FunctionUnitChildren functionUnitChildren = new FunctionUnitChildren();
                functionUnitChildren.AssetId = reading.AssetId;
                functionUnitChildren.ClientAssetNameId = fuc.ClientAssetNameId;
                functionUnitChildren.FunctionChildrenBachgroundColor = fuc.FunctionChildrenBachgroundColor;
                functionUnitChildren.FunctionChildrenColor = fuc.FunctionChildrenColor;
                functionUnitChildren.FunctionUnitId = fuc.FunctionUnitId;
                functionUnitChildren.FunctionUnitChildrenName = fuc.FunctionUnitChildrenName;
                functionUnitChildren.Frequency = fuc.Frequency;
                functionUnitChildren.MeasurementUnit = fuc.MeasurementUnit;

                if (fuc.FunctionUnitChildrenName == "Day 1st 4 Hours Ends")
                {
                    functionUnitChildren.MonthlyTarget = reading.Day1st4HoursEndsMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = false;

                    functionUnitChildren.Target = reading.Day1st4HoursEndsTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = false;

                    functionUnitChildren.Budget = reading.Day1st4HoursEndsBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = false;

                    functionUnitChildren.Threshold = reading.Day1st4HoursEndsThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = false;

                }
                else if (fuc.FunctionUnitChildrenName == "Night 1st 4 Hours Ends")
                {
                    functionUnitChildren.MonthlyTarget = reading.Night1st4HoursEndsMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = false;

                    functionUnitChildren.Target = reading.Night1st4HoursEndsTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = false;

                    functionUnitChildren.Budget = reading.Night1st4HoursEndsBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = false;

                    functionUnitChildren.Threshold = reading.Night1st4HoursEndsThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = false;
                }
                else if (fuc.FunctionUnitChildrenName == "Ends Drilled")
                {
                    functionUnitChildren.MonthlyTarget = reading.EndsDrilledMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = false;

                    functionUnitChildren.Target = reading.EndsDrilledTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = false;

                    functionUnitChildren.Budget = reading.EndsDrilledBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = false;

                    functionUnitChildren.Threshold = reading.EndsDrilledThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = false;
                }
                else if (fuc.FunctionUnitChildrenName == "Ends Blasted")
                {
                    functionUnitChildren.MonthlyTarget = reading.EndsBlastedMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = false;

                    functionUnitChildren.Target = reading.EndsBlastedTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = false;

                    functionUnitChildren.Budget = reading.EndsBlastedBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = false;

                    functionUnitChildren.Threshold = reading.EndsBlastedThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = false;
                }
                else if (fuc.FunctionUnitChildrenName == "Unlashed Ends")
                {
                    functionUnitChildren.MonthlyTarget = reading.UnlashedEndsMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = true;

                    functionUnitChildren.Target = reading.UnlashedEndsTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = true;

                    functionUnitChildren.Budget = reading.UnlashedEndsBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = true;

                    functionUnitChildren.Threshold = reading.UnlashedEndsThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = true;
                }
                else if (fuc.FunctionUnitChildrenName == "Ends Lashed")
                {
                    functionUnitChildren.MonthlyTarget = reading.EndsLashedMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = false;

                    functionUnitChildren.Target = reading.EndsLashedTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = false;

                    functionUnitChildren.Budget = reading.EndsLashedBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = false;

                    functionUnitChildren.Threshold = reading.EndsLashedThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = false;
                }
                else if (fuc.FunctionUnitChildrenName == "Total Cleaned Ends")
                {
                    functionUnitChildren.MonthlyTarget = reading.TotalCleanedEndsMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = true;

                    functionUnitChildren.Target = reading.TotalCleanedEndsTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = true;

                    functionUnitChildren.Budget = reading.TotalCleanedEndsBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = true;

                    functionUnitChildren.Threshold = reading.TotalCleanedEndsThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = true;
                }
                else if (fuc.FunctionUnitChildrenName == "Lashed Prepared For Support")
                {
                    functionUnitChildren.MonthlyTarget = reading.LashedPreparedForSupportMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = true;

                    functionUnitChildren.Target = reading.LashedPreparedForSupportTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = true;

                    functionUnitChildren.Budget = reading.LashedPreparedForSupportBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = true;

                    functionUnitChildren.Threshold = reading.LashedPreparedForSupportThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = true;
                }
                else if (fuc.FunctionUnitChildrenName == "Muckbay Tons")
                {
                    functionUnitChildren.MonthlyTarget = reading.MuckbayTonsMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = true;

                    functionUnitChildren.Target = reading.MuckbayTonsTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = true;

                    functionUnitChildren.Budget = reading.MuckbayTonsBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = true;

                    functionUnitChildren.Threshold = reading.MuckbayTonsThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = true;
                }
                else if (fuc.FunctionUnitChildrenName == "Hoisted Tons")
                {
                    functionUnitChildren.MonthlyTarget = reading.HoistedTonsMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = false;

                    functionUnitChildren.Target = reading.HoistedTonsTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = false;

                    functionUnitChildren.Budget = reading.HoistedTonsBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = false;

                    functionUnitChildren.Threshold = reading.HoistedTonsThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = false;
                }
                else if (fuc.FunctionUnitChildrenName == "UG Crusher Bin")
                {
                    functionUnitChildren.MonthlyTarget = reading.UGCrusherBinMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = true;

                    functionUnitChildren.Target = reading.UGCrusherBinTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = true;

                    functionUnitChildren.Budget = reading.UGCrusherBinBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = true;

                    functionUnitChildren.Threshold = reading.UGCrusherBinThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = true;
                }
                else if (fuc.FunctionUnitChildrenName == "Ends Supported")
                {
                    functionUnitChildren.MonthlyTarget = reading.EndsSupportedMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = false;

                    functionUnitChildren.Target = reading.EndsSupportedTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = false;

                    functionUnitChildren.Budget = reading.EndsSupportedBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = false;

                    functionUnitChildren.Threshold = reading.EndsSupportedThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = false;
                }
                else if (fuc.FunctionUnitChildrenName == "Supported Ends")
                {
                    functionUnitChildren.MonthlyTarget = reading.SupportedEndsMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = true;

                    functionUnitChildren.Target = reading.SupportedEndsTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = true;

                    functionUnitChildren.Budget = reading.SupportedEndsBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = true;

                    functionUnitChildren.Threshold = reading.SupportedEndsThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = true;
                }
                else if (fuc.FunctionUnitChildrenName == "Ends Prepared")
                {
                    functionUnitChildren.MonthlyTarget = reading.EndsPreparedMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = false;

                    functionUnitChildren.Target = reading.EndsPreparedTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = false;

                    functionUnitChildren.Budget = reading.EndsPreparedBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = false;

                    functionUnitChildren.Threshold = reading.EndsPreparedThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = false;
                }
                else if (fuc.FunctionUnitChildrenName == "Prepared Marked Ends")
                {
                    functionUnitChildren.MonthlyTarget = reading.PreparedMarkedEndsMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = true;

                    functionUnitChildren.Target = reading.PreparedMarkedEndsTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = true;

                    functionUnitChildren.Budget = reading.PreparedMarkedEndsBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = true;

                    functionUnitChildren.Threshold = reading.PreparedMarkedEndsThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = true;
                }
                else if (fuc.FunctionUnitChildrenName == "Drill Rigs")
                {
                    functionUnitChildren.MonthlyTarget = reading.DrillRigsMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = true;

                    functionUnitChildren.Target = reading.DrillRigsTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = true;

                    functionUnitChildren.Budget = reading.DrillRigsBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = true;

                    functionUnitChildren.Threshold = reading.DrillRigsThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = true;
                }
                else if (fuc.FunctionUnitChildrenName == "LHDs")
                {
                    functionUnitChildren.MonthlyTarget = reading.LHDsMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = true;

                    functionUnitChildren.Target = reading.LHDsTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = true;

                    functionUnitChildren.Budget = reading.LHDsBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = true;

                    functionUnitChildren.Threshold = reading.LHDsThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = true;
                }
                else if (fuc.FunctionUnitChildrenName == "Dump Trucks")
                {
                    functionUnitChildren.MonthlyTarget = reading.DumpTrucksMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = true;

                    functionUnitChildren.Target = reading.DumpTrucksTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = true;

                    functionUnitChildren.Budget = reading.DumpTrucksBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = true;

                    functionUnitChildren.Threshold = reading.DumpTrucksThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = true;
                }
                else if (fuc.FunctionUnitChildrenName == "Bolters")
                {
                    functionUnitChildren.MonthlyTarget = reading.BoltersMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = true;

                    functionUnitChildren.Target = reading.BoltersTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = true;

                    functionUnitChildren.Budget = reading.BoltersBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = true;

                    functionUnitChildren.Threshold = reading.BoltersThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = true;
                }
                else if (fuc.FunctionUnitChildrenName == "Drifters")
                {
                    functionUnitChildren.MonthlyTarget = reading.DriftersMonthTarget.ToString();
                    functionUnitChildren.MonthlyTargetColor = "limegreen";
                    functionUnitChildren.MonthlyTargetIsBackground = true;

                    functionUnitChildren.Target = reading.DriftersTarget.ToString();
                    functionUnitChildren.TargetColor = "limegreen";
                    functionUnitChildren.TargetIsBackground = true;

                    functionUnitChildren.Budget = reading.DriftersBudget.ToString();
                    functionUnitChildren.BudgetColor = "orange";
                    functionUnitChildren.BudgetIsBackground = true;

                    functionUnitChildren.Threshold = reading.DriftersThreshold.ToString();
                    functionUnitChildren.ThresholdColor = "red";
                    functionUnitChildren.ThresholdIsBackground = true;
                }
                tempFUC.Add(functionUnitChildren);
            }

            return tempFUC;
        }


        [HttpGet("")]
        public IActionResult GetAssets(string reference)
        {
            try
            {
                var lAssets = _context.ProductionFlowAssets.Where(id=>id.Reference == reference).ToList();
                //var lAssets = _context.AssetUsers.Where(id => id.UserId == reference).ToList();

                if (lAssets != null)
                {
                    var assets = lAssets.Select(result => new
                    {
                        AssetId = result.AssetId,
                        //Template = _context.Templates.FirstOrDefault(id => id.TemplateId == _context.ProductionFlowAssets.FirstOrDefault(id => id.AssetId == result.AssetId).TemplateId).TemplateName,
                        AssetName = _context.ClientAssetNames.FirstOrDefault(id => id.ClientAssetNameId == _context.ProductionFlowAssets.FirstOrDefault(id => id.AssetId == result.AssetId).ClientAssetNameId).AssetName,
                        ClientName = _context.ClientAssetNames.FirstOrDefault(id => id.ClientAssetNameId == _context.ProductionFlowAssets.FirstOrDefault(id => id.AssetId == result.AssetId).ClientAssetNameId).ClientName,
                        SiteName = _context.ProductionFlowAssets.FirstOrDefault(id => id.AssetId == result.AssetId).SiteName,
                        DateStamp = result.DateStamp,
                        Reference = _context.Users.FirstOrDefault(id => id.Id == result.Reference).FirstName + " " + _context.Users.FirstOrDefault(id => id.Id == result.Reference).LastName,
                    });
                    return Ok(lAssets);
                }
                else
                {
                    return Ok();
                }



            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        [HttpPost("")]
        public IActionResult SaveAsset([FromBody] ProductionFlowAsset model)
        {
            if (model == null)
            {
                return BadRequest("Make sure all required fields are completed!");
            }

            try
            {
                ProductionFlowAsset asset = CreateAsset(model);

                List<FunctionUnit> functionUnits = saveAssetFunctionUnits(asset);
                List<FunctionUnitChildren> functionUnitsChildren = saveAssetFunctionUnitsChildren(asset);
                if (asset != null)
                {
                    //Add Default asset user

                    ProductionFlowAssetUser assetUser = new ProductionFlowAssetUser();
                    assetUser.UserId = asset.Reference;
                    assetUser.Reference = asset.Reference;
                    assetUser.AssetId = asset.AssetId;
                    assetUser.DateStamp = DateTime.Now;
                    assetUser.IsDrillAndBlast = true;
                    assetUser.IsEquipmentStatus = true;
                    assetUser.IsFacePreparation = true;
                    assetUser.IsLoadAndHaul = true;
                    assetUser.IsOverallProductionBuffer = true;
                    assetUser.IsSupport = true;
                    assetUser.IsOverallProductionProcess = true;
                    assetUser.IsShe = true;

                    _context.ProductionFlowAssetUsers.Add(assetUser);
                    _context.SaveChanges();
                }


                //if (_context.Templates.FirstOrDefault(id => id.TemplateId == model.TemplateId).TemplateName == "Production Flow")
                //{
                    List<DateTime> dates = getAllDates(model.SinceDateProduction.Year, model.SinceDateProduction.Month);
                    List<Reading> readings = new List<Reading>();

                    foreach (var date in dates)
                    {
                        Reading reading = new Reading();
                        reading.DateProduction = date;
                        reading.AssetId = asset.AssetId;
                        reading.Reference = asset.Reference;

                        readings.Add(reading);

                    }

                    _context.Readings.AddRange(readings);
                    _context.SaveChanges();

                //}


                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        public List<FunctionUnitChildren> saveAssetFunctionUnitsChildren(ProductionFlowAsset asset)
        {
            List<FunctionUnitChildren> fucTemp = new List<FunctionUnitChildren>();
            List<FunctionUnitChildren> orderdFucs = new List<FunctionUnitChildren>();

            List<FunctionUnit> functionUnits = _context.FunctionUnits.Where(id => (id.AssetId == asset.AssetId) && (id.ClientAssetNameId == asset.ClientAssetNameId)).ToList();
            functionUnits.OrderBy(id => id.FunctionUnitId);


            List<FunctionUnitChildren> functionUnitChildrens = _context.FunctionUnitChildrens.Where(id => (id.AssetId == 0) && (id.ClientAssetNameId == 0)).ToList();

            orderdFucs = functionUnitChildrens.OrderBy(id => id.FunctionUnitChildrenId).ToList();

            foreach (var fuc in orderdFucs)
            {
                FunctionUnitChildren functionUnitChildren = new FunctionUnitChildren();
                functionUnitChildren.AssetId = asset.AssetId;
                functionUnitChildren.ClientAssetNameId = asset.ClientAssetNameId;
                functionUnitChildren.Frequency = fuc.Frequency;
                functionUnitChildren.FunctionUnitChildrenName = fuc.FunctionUnitChildrenName;
                functionUnitChildren.FunctionChildrenBachgroundColor = fuc.FunctionChildrenBachgroundColor;
                functionUnitChildren.FunctionUnitId = fuc.FunctionUnitId;
                functionUnitChildren.FunctionChildrenColor = fuc.FunctionChildrenColor;
                functionUnitChildren.MeasurementUnit = fuc.MeasurementUnit;
                functionUnitChildren.FunctionUnitChildrenId = fuc.FunctionUnitChildrenId;
                fucTemp.Add(functionUnitChildren);
            }

            List<int> tempFUIds = new List<int>();

            foreach (var item in _context.FunctionUnits.Take(5).OrderByDescending(id => id.FunctionUnitId))
            {
                tempFUIds.Add(item.FunctionUnitId);
            }

            tempFUIds.Sort();

            List<int> holdings = new List<int>();
            List<int> holdings1 = new List<int>();

            holdings = tempFUIds;
            holdings1 = tempFUIds;

            int counter = 0;

            List<FunctionUnitChildren> fucTemp2 = new List<FunctionUnitChildren>();

            foreach (var item in holdings)
            {
                counter = counter + 1;

                foreach (var it in fucTemp.Where(id => id.FunctionUnitId == counter).ToList())
                {
                    FunctionUnitChildren functionUnitChildren1 = new FunctionUnitChildren();
                    functionUnitChildren1.AssetId = asset.AssetId;
                    functionUnitChildren1.ClientAssetNameId = asset.ClientAssetNameId;
                    functionUnitChildren1.Frequency = it.Frequency;
                    functionUnitChildren1.FunctionUnitChildrenName = it.FunctionUnitChildrenName;
                    functionUnitChildren1.FunctionChildrenBachgroundColor = it.FunctionChildrenBachgroundColor;
                    functionUnitChildren1.FunctionUnitId = holdings1[counter - 1];
                    functionUnitChildren1.FunctionChildrenColor = it.FunctionChildrenColor;
                    functionUnitChildren1.MeasurementUnit = it.MeasurementUnit;
                    fucTemp2.Add(functionUnitChildren1);
                }

            }

            _context.FunctionUnitChildrens.AddRange(fucTemp2);
            _context.SaveChanges();

            return fucTemp2;
        }

        public List<FunctionUnit> saveAssetFunctionUnits(ProductionFlowAsset asset)
        {

            List<FunctionUnit> functionUnits = _context.FunctionUnits.Where(id => id.AssetId == 0).ToList();
            List<FunctionUnit> fucTemp = new List<FunctionUnit>();

            foreach (FunctionUnit item in functionUnits)
            {
                FunctionUnit functionUnit = new FunctionUnit();
                functionUnit.AssetId = asset.AssetId;
                functionUnit.ClientAssetNameId = asset.ClientAssetNameId;
                functionUnit.FunctionUnitName = item.FunctionUnitName;
                fucTemp.Add(functionUnit);
            }

            _context.FunctionUnits.AddRange(fucTemp);
            _context.SaveChanges();

            return fucTemp;
        }

        public ProductionFlowAsset CreateAsset(ProductionFlowAsset model)
        {
            ProductionFlowAsset asset = new ProductionFlowAsset();
            asset.ClientAssetNameId = model.ClientAssetNameId;
            asset.DateStamp = DateTime.Now;
            asset.Reference = model.Reference;
            asset.TemplateId = model.TemplateId;
            asset.SiteName = model.SiteName;

            _context.ProductionFlowAssets.Add(asset);
            _context.SaveChanges();

            return asset;
        }


        public ValueObject ValueObjectDate(Reading reading)
        {
            ValueObject valueObject = new ValueObject();


            valueObject.Value = reading.DateProduction.Day;

            valueObject.Color = "none";

            valueObject.IsBackground = false;

            return valueObject;
        }
        public ValueObject SheAssetObject(Reading reading)
        {
            ValueObject valueObject = new ValueObject();

            int sheCounter = 0;
            var clientNameAssetId = _context.ProductionFlowAssets.FirstOrDefault(id => id.AssetId == reading.AssetId).ClientAssetNameId;

            foreach (var item in _context.ProductionFlowAssets.Where(id => id.ClientAssetNameId == clientNameAssetId).ToList())
            {
                sheCounter = sheCounter + _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && (id.DateProduction == reading.DateProduction)).She;
            }
            valueObject.Value = sheCounter;
            if (sheCounter > 0)
            {

                valueObject.Color = "red";
            }
            else
            {
                valueObject.Color = "limegreen";
            }

            valueObject.IsBackground = false;
            if (reading.DateProduction > DateTime.Now)
            {
                valueObject.Color = "none";
            }

            return valueObject;
        }

        //Calculate SHE
        public int SheAsset(Reading reading)
        {
            int sheCounter = 0;
            var clientNameAssetId = _context.ProductionFlowAssets.FirstOrDefault(id => id.AssetId == reading.AssetId).ClientAssetNameId;
            var tempList = _context.ProductionFlowAssets.Where(id => id.ClientAssetNameId == clientNameAssetId).ToList();
            foreach (var item in tempList)
            {
                var she = _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && (id.DateProduction == reading.DateProduction));
                if (she == null)
                {
                    //Add them, then come back with the one we looking for
                    AssetProdDate assetProdDate = new AssetProdDate();
                    assetProdDate.AssetId = item.AssetId;
                    assetProdDate.ProductionDate = reading.DateProduction;

                    var lsiOfReadings = PopulateReadings(assetProdDate);
                    var lShe = lsiOfReadings.FirstOrDefault(id => (id.AssetId == item.AssetId) && (id.DateProduction == reading.DateProduction));
                    sheCounter = sheCounter + lShe.She;
                }
                else
                {
                    sheCounter = sheCounter + she.She;
                }


            }

            return sheCounter;
        }

        // Calculate Hoisted Tons for each site
        public int HoistedTonsPerSite(Reading reading)
        {
            int tonsHoistedCounter = 0;
            // Check if current day is the first in the month 
            if (reading.DateProduction.Day == 1)
            {
                tonsHoistedCounter = tonsHoistedCounter + reading.HoistedTons;
            }
            else
            {
                if (reading.HoistedTons > 0)
                {

                    var readings = _context.Readings.Where(id => (id.AssetId == reading.AssetId) && (id.DateProduction.Month == reading.DateProduction.Month)).ToList();

                    foreach (var item in readings)
                    {
                        tonsHoistedCounter = tonsHoistedCounter + item.HoistedTons;
                    }
                }

            }
            return tonsHoistedCounter;
        }

        public ValueObject HoistedTonsPerSiteObject(Reading reading)
        {
            ValueObject valueObject = new ValueObject();
            ProductionFlowAsset asset = _context.ProductionFlowAssets.FirstOrDefault(id => id.AssetId == reading.AssetId);
            //Get List of readings 

            int totalHoistedTons = 0;
            int target = 0;
            int budget = 0;

            if (reading.DateProduction.Day == 1)
            {


                Reading lReading = _context.Readings.FirstOrDefault(id => (id.AssetId == reading.AssetId) && (id.DateProduction.Year == reading.DateProduction.Year) 
                && (id.DateProduction.Month == reading.DateProduction.Month) && (id.DateProduction.Day == reading.DateProduction.Day));

                target = target + lReading.HoistedTonsTarget;
                budget = budget + lReading.HoistedTonsThreshold;
                totalHoistedTons = totalHoistedTons + lReading.HoistedTons;

                valueObject.Value = totalHoistedTons;

                if (totalHoistedTons >= target)
                {
                    valueObject.Color = "limegreen";
                }
                else if ((totalHoistedTons < target) && (totalHoistedTons >= budget))
                {
                    valueObject.Color = "orange";
                }
                else
                {
                    valueObject.Color = "red";
                }
            }
            else
            {
                var firstDayOfMonth = new DateTime(reading.DateProduction.Year, reading.DateProduction.Month, 1);
                var currentDate = reading.DateProduction;

                List<DateTime> dateTimes = new List<DateTime>();

                dateTimes = Enumerable.Range(0, (currentDate - firstDayOfMonth).Days + 1)
                        .Select(day => firstDayOfMonth.AddDays(day)).ToList();


                foreach (var item in dateTimes)
                {
                    Reading lReading = _context.Readings.FirstOrDefault(id => (id.AssetId == asset.AssetId) && (id.DateProduction == item));
                    totalHoistedTons = totalHoistedTons + lReading.HoistedTons;
                    //var t = GetTargetCT(param.Asset, item, "");
                    target = lReading.HoistedTonsTarget;
                    //var b = GetBudgetCT(param.Asset, item, "");
                    budget = lReading.HoistedTonsThreshold;
                }

                valueObject.Value = totalHoistedTons;

            }


            if (totalHoistedTons >= target)
            {
                valueObject.Color = "limegreen";
            }
            else if ((totalHoistedTons < target) && (totalHoistedTons >= budget))
            {
                valueObject.Color = "orange";
            }
            else
            {
                valueObject.Color = "red";
            }

            valueObject.IsBackground = false;

            if (reading.DateProduction > DateTime.Now)
            {
                valueObject.Color = "none";
            }

            return valueObject;
        }

        // Calculate Ends Available for each site
        public int EndsAvailablePerSite(Reading reading)
        {
            return reading.UnlashedEnds + reading.TotalCleanedEnds + reading.SupportedEnds + reading.PreparedMarkedEnds;
        }

        public ValueObject EndsAvailablePerSiteObjects(Reading reading)
        {
            int endsAvailable = 0;
            int target = 0;
            int budget = 0;


            ValueObject valueObject = new ValueObject();

            endsAvailable = reading.UnlashedEnds + reading.TotalCleanedEnds + reading.SupportedEnds + reading.PreparedMarkedEnds;
            target = reading.UnlashedEndsTarget + reading.TotalCleanedEndsTarget + reading.SupportedEndsTarget + reading.PreparedMarkedEndsTarget;
            budget = reading.UnlashedEndsBudget + reading.TotalCleanedEndsBudget + reading.SupportedEndsThreshold + reading.PreparedMarkedEndsThreshold;

            valueObject.Value = endsAvailable;

            if (endsAvailable >= target)
            {
                valueObject.Color = "limegreen";
            }
            else if ((endsAvailable < target) && (endsAvailable >= budget))
            {
                valueObject.Color = "orange";
            }
            else
            {
                valueObject.Color = "red";
            }

            if (reading.DateProduction > DateTime.Now)
            {
                valueObject.Color = "none";
            }

            valueObject.IsBackground = true;

            return valueObject;
        }


        //Calculate Total Hoisted Tons Accumalative
        public int TotalTonsHoisetedCommulative(Reading reading)
        {
            int totalTons = 0;

            if (reading.DateProduction.Day == 1)
            {
                int ClientId = _context.ProductionFlowAssets.FirstOrDefault(id => id.AssetId == reading.AssetId).ClientAssetNameId;

                List<ProductionFlowAsset> assets = _context.ProductionFlowAssets.Where(id => id.ClientAssetNameId == ClientId).ToList();

                foreach (ProductionFlowAsset item in assets)
                {
                    totalTons = totalTons + _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && (id.DateProduction == reading.DateProduction)).HoistedTons;
                }
            }
            else
            {
                int ClientId = _context.ProductionFlowAssets.FirstOrDefault(id => id.AssetId == reading.AssetId).ClientAssetNameId;

                List<ProductionFlowAsset> assets = _context.ProductionFlowAssets.Where(id => id.ClientAssetNameId == ClientId).ToList();

                foreach (ProductionFlowAsset item in assets)
                {
                    totalTons = totalTons + _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && (id.DateProduction == reading.DateProduction)).HoistedTons;
                }

                foreach (ProductionFlowAsset item in assets)
                {
                    totalTons = totalTons + _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && (id.DateProduction == reading.DateProduction.AddDays(-1))).HoistedTons;
                }

            }

            return totalTons;
        }

        public ValueObject TotalTonsHoiseted(TotalHoistedTonsInputParam param)
        {
            ValueObject valueObject = new ValueObject();
            int monthlyTarget = 0;
            int monthlyBudget = 0;
            int monthlyThreshold = 0;
            int target = 0;
            int budget = 0;
            int totalHoistedTons = 0;

            foreach (var item in param.Asset)
            {
                Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && 
                (id.DateProduction.Year == param.Reading.DateProduction.Year) && (id.DateProduction.Month == param.Reading.DateProduction.Month));
                totalHoistedTons = totalHoistedTons + _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && 
                (id.DateProduction == param.Reading.DateProduction)).HoistedTons;
                monthlyTarget = monthlyTarget + reading.HoistedTonsTarget;
                monthlyBudget = monthlyThreshold + reading.HoistedTonsThreshold;
            }
            target = monthlyTarget / System.DateTime.DaysInMonth(param.Reading.DateProduction.Year, param.Reading.DateProduction.Month);
            budget = monthlyBudget / System.DateTime.DaysInMonth(param.Reading.DateProduction.Year, param.Reading.DateProduction.Month);

            valueObject.Value = totalHoistedTons;

            if (totalHoistedTons >= target)
            {
                valueObject.Color = "limegreen";
            }
            else if ((totalHoistedTons < target) && (totalHoistedTons >= budget))
            {
                valueObject.Color = "orange";
            }
            else
            {
                valueObject.Color = "red";
            }

            if (param.Reading.DateProduction > DateTime.Now)
            {
                valueObject.Color = "none";
            }

            return valueObject;
        }


        public ValueObject TotalTonsHoisetedCommulativeObject(TotalHoistedCommulataiveInputParam param)
        {
            ValueObject valueObject = new ValueObject();
            //Get List of readings 

            int totalHoistedTons = 0;
            int target = 0;
            int budget = 0;

            int monthlyTarget = 0;
            int monthlyBudget = 0;

            if (param.Reading.DateProduction.Day == 1)
            {

                foreach (var item in param.Asset)
                {
                    Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && 
                    (id.DateProduction.Year == param.Reading.DateProduction.Year) && (id.DateProduction.Month == param.Reading.DateProduction.Month));
                    monthlyTarget = monthlyTarget + reading.HoistedTonsTarget;
                }
                target = monthlyTarget / System.DateTime.DaysInMonth(param.Reading.DateProduction.Year, param.Reading.DateProduction.Month);

                foreach (var item in param.Asset)
                {
                    Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && 
                    (id.DateProduction.Year == param.Reading.DateProduction.Year) && (id.DateProduction.Month == param.Reading.DateProduction.Month));
                    monthlyBudget = monthlyBudget + reading.HoistedTonsThreshold;
                }
                var t = System.DateTime.DaysInMonth(param.Reading.DateProduction.Year, param.Reading.DateProduction.Month);
                budget = budget + monthlyBudget / System.DateTime.DaysInMonth(param.Reading.DateProduction.Year, param.Reading.DateProduction.Month);

                foreach (ProductionFlowAsset item in param.Asset)
                {
                    totalHoistedTons = totalHoistedTons + _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && 
                    (id.DateProduction == param.Reading.DateProduction)).HoistedTons;
                }

                valueObject.Value = totalHoistedTons;

                if (totalHoistedTons >= target)
                {
                    valueObject.Color = "limegreen";
                }
                else if ((totalHoistedTons < target) && (totalHoistedTons >= budget))
                {
                    valueObject.Color = "orange";
                }
                else
                {
                    valueObject.Color = "red";
                }
            }
            else
            {
                var firstDayOfMonth = new DateTime(param.Reading.DateProduction.Year, param.Reading.DateProduction.Month, 1);
                var currentDate = param.Reading.DateProduction;

                List<DateTime> dateTimes = new List<DateTime>();

                dateTimes = Enumerable.Range(0, (currentDate - firstDayOfMonth).Days + 1)
                        .Select(day => firstDayOfMonth.AddDays(day)).ToList();

                foreach (var asset in param.Asset)
                {
                    foreach (var item in dateTimes)
                    {
                        Reading reading = _context.Readings.FirstOrDefault(id => (id.AssetId == asset.AssetId) && (id.DateProduction == item));
                        totalHoistedTons = totalHoistedTons + reading.HoistedTons;
                        var t = GetTargetCT(param.Asset, item, "");
                        target = t.Target;
                        var b = GetBudgetCT(param.Asset, item, "");
                        budget = b.Budget;
                    }

                }

            }

            valueObject.Value = totalHoistedTons;

            if (totalHoistedTons >= target)
            {
                valueObject.Color = "limegreen";
            }
            else if ((totalHoistedTons < target) && (totalHoistedTons >= budget))
            {
                valueObject.Color = "orange";
            }
            else
            {
                valueObject.Color = "red";
            }

            if (param.Reading.DateProduction > DateTime.Now)
            {
                valueObject.Color = "none";
            }

            return valueObject;
        }

        public int TotalTonsHoiseted(Reading reading)
        {
            int ClientId = _context.ProductionFlowAssets.FirstOrDefault(id => id.AssetId == reading.AssetId).ClientAssetNameId;

            List<ProductionFlowAsset> assets = _context.ProductionFlowAssets.Where(id => id.ClientAssetNameId == ClientId).ToList();

            int totalTons = 0;

            foreach (ProductionFlowAsset item in assets)
            {
                totalTons = totalTons + _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && (id.DateProduction == reading.DateProduction)).HoistedTons;
            }

            return totalTons;
        }

        public ValueObject TotalTonsHoisetedObject(Reading reading)
        {
            ValueObject valueObject = new ValueObject();
            int totalHoistedTons = 0;
            int target = 0;
            int budget = 0;

            int ClientId = _context.ProductionFlowAssets.FirstOrDefault(id => id.AssetId == reading.AssetId).ClientAssetNameId;

            List<ProductionFlowAsset> assets = _context.ProductionFlowAssets.Where(id => id.ClientAssetNameId == ClientId).ToList();

            foreach (ProductionFlowAsset item in assets)
            {
                totalHoistedTons = totalHoistedTons + _context.Readings.FirstOrDefault(id => (id.AssetId == item.AssetId) && (id.DateProduction == reading.DateProduction)).HoistedTons;
            }
            valueObject.Value = totalHoistedTons;

            if (totalHoistedTons >= target)
            {
                valueObject.Color = "limegreen";
            }
            else if ((totalHoistedTons < target) && (totalHoistedTons >= budget))
            {
                valueObject.Color = "orange";
            }
            else
            {
                valueObject.Color = "red";
            }

            if (reading.DateProduction > DateTime.Now)
            {
                valueObject.Color = "none";
            }

            return valueObject;
        }

        [HttpPost("CheckAssetUser")]
        public IActionResult CheckAssetUser([FromBody] AssetUserRightsIncomingObject model)
        {
            if (model == null)
            {
                return BadRequest("Make sure all required fields are completed!");
            }

            try
            {
                ProductionFlowAssetUser assetUser = _context.ProductionFlowAssetUsers.FirstOrDefault(id => 
                (id.AssetId == model.AssetId && id.UserId == model.UserId));

                return Ok(assetUser);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened! " + Ex.Message);
            }
        }

        [HttpPost("CloseMonthlyReadings")]
        public IActionResult CloseMonthlyReadings([FromBody] CloseMonthlyAssetReadings model)
        {
            if (model == null)
            {
                return BadRequest("Make sure date field is selected!");
            }

            try
            {
                List<Reading> readings = _context.Readings.Where(id => (id.AssetId == model.AssetId) && (id.DateProduction.Year == model.DateStamp.Year) 
                && (id.DateProduction.Month == model.DateStamp.Month)).ToList();

                foreach (var r in readings)
                {
                    r.IsClosed = true;
                }

                _context.Readings.UpdateRange(readings);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened! " + Ex.Message);
            }
        }

        public List<Reading> PopulateReadings([FromBody] AssetProdDate assetProdDate)
        {
            List<Reading> readings = new List<Reading>();
            ProductionFlowAsset asset = _context.ProductionFlowAssets.FirstOrDefault(id => id.AssetId == assetProdDate.AssetId);
            readings = _context.Readings.Where(id => (id.AssetId == assetProdDate.AssetId) && (id.DateProduction.Month == assetProdDate.ProductionDate.Month) 
            && (id.DateProduction.Year == assetProdDate.ProductionDate.Year)).ToList();
            List<Reading> rds = new List<Reading>();
            if (readings.Count == 0)
            {
                List<DateTime> dates = getAllDates(assetProdDate.ProductionDate.Year, assetProdDate.ProductionDate.Month);


                foreach (var date in dates)
                {
                    Reading reading = new Reading();
                    reading.DateProduction = date;
                    reading.AssetId = asset.AssetId;
                    reading.Reference = asset.Reference;

                    rds.Add(reading);

                }

                _context.Readings.AddRange(rds);
                _context.SaveChanges();

            }

            //var orderedReadings = readings.Where(id=>(id.AssetId == assetProdDate.AssetId) && (id.DateProduction.Year == assetProdDate.ProductionDate.Year) && (id.DateProduction.Month == assetProdDate.ProductionDate.Month)).ToList();
            var tempOrdering = rds.OrderBy(d => d.DateProduction);

            return tempOrdering.ToList();
        }

    }
}
