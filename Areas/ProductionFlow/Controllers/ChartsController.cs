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
    public class ChartsController : Controller
    {
        private BinmakDbContext _context;

        public ChartsController(BinmakDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public IActionResult GetAssetNodeById(int assetId)
        {
            if (assetId == 0)
            {
                return BadRequest("Something bad happened, try again");
            }

            try
            {
                BinmakBackEnd.Entities.AssetNode assetNode = _context.AssetNodes.FirstOrDefault(id => id.AssetNodeId == assetId);

                return Ok(assetNode);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        [HttpPost("parent")]
        public IActionResult GenerateParentChart(ChartModel model)
        {
            if (model == null)
            {
                return BadRequest("Make sure fields are completed correctly.");
            }

            try
            {
                ClientAsset clientAsset = _context.ClientAssetNames.FirstOrDefault(id => id.ClientAssetNameId == model.AssetId);

                if (clientAsset == null)
                {
                    return BadRequest("This is not a parent to any productive unit, choose normal chart");
                }

                //List of Ids to iterate over
                List<BinmakBackEnd.Entities.AssetNode> assetNodes = _context.AssetNodes.Where(id => id.ParentAssetNodeId == model.AssetId).ToList();

                List<Dictionary<string, object>> masterDictionary = new List<Dictionary<string, object>>();

                foreach (var a in assetNodes)
                {
                    model.AssetId = a.AssetNodeId;

                    int AssetId = model.AssetId;
                    ProductionFlowAsset asset = _context.ProductionFlowAssets.FirstOrDefault(id => id.AssetId == AssetId);
                    if (asset == null)
                    {
                        return BadRequest("Make sure asset node is a productive unit! Or contact your administrator.");
                    }
                    int ClientAssetId = asset.ClientAssetNameId;

                    List<Reading> readings = _context.Readings.Where(id => (id.AssetId == AssetId)
                    && (id.DateProduction >= model.StartDate) && (id.DateProduction <= model.EndDate)).ToList();


                    List<int> dailyEndsDrilled = new List<int>();
                    List<int> firstFourHoursDrilledEnds = new List<int>();
                    List<int> dailyEndsSupported = new List<int>();
                    List<int> supportedEnds = new List<int>();
                    List<int> unlashedEnds = new List<int>();
                    List<int> hoistedTons = new List<int>();
                    List<int> cleanedEnds = new List<int>();
                    List<int> lashedPreparedEnds = new List<int>();

                    List<double> dailyEndsDrilledPercentile = new List<double>();
                    List<double> dailyFirst4HoursEndsDrilledPercentile = new List<double>();
                    List<double> endsSupportedPercentile = new List<double>();
                    List<double> supportedEndsPercentile = new List<double>();
                    List<double> supportedEndsPercentile2 = new List<double>();


                    List<double> dailyEndsDrilledHist = new List<double>();
                    List<double> dailyEndsDrilledHist2 = new List<double>();
                    List<double> dailyEndsDrilledHist3 = new List<double>();
                    List<double> dailyEndsDrilledHist4 = new List<double>();
                    List<double> dailyEndsDrilledHist5 = new List<double>();


                    foreach (var item in readings)
                    {
                        dailyEndsDrilledHist.Add(item.EndsDrilled);
                        dailyEndsDrilledHist2.Add(item.Day1st4HoursEnds);
                        dailyEndsDrilledHist3.Add(item.SupportedEnds);
                        dailyEndsDrilledHist4.Add(item.EndsSupported);
                        dailyEndsDrilledHist5.Add(item.HoistedTons);


                        dailyEndsDrilled.Add(item.EndsDrilled);
                        firstFourHoursDrilledEnds.Add(item.Day1st4HoursEnds);
                        dailyEndsSupported.Add(item.EndsSupported);
                        supportedEnds.Add(item.SupportedEnds);
                        unlashedEnds.Add(item.UnlashedEnds);
                        hoistedTons.Add(item.HoistedTons);
                        cleanedEnds.Add(item.TotalCleanedEnds);
                        lashedPreparedEnds.Add(item.LashedPreparedForSupport);


                        dailyEndsDrilledPercentile.Add(item.EndsDrilled);
                        dailyFirst4HoursEndsDrilledPercentile.Add(item.Day1st4HoursEnds);
                        endsSupportedPercentile.Add(item.EndsSupported);
                        supportedEndsPercentile.Add(item.SupportedEnds);
                        supportedEndsPercentile2.Add(item.HoistedTons);
                    }



                    double minimumH = dailyEndsDrilledHist.Min();
                    double maximumH = dailyEndsDrilledHist.Max();
                    double divider = dailyEndsDrilledHist.Max() / 11;

                    //Find x axis
                    List<double> histogramXAxis = new List<double>();

                    for (int i = 0; i < 13; i++)
                    {
                        histogramXAxis.Add(i * Math.Round(divider, 0));
                    }

                    //Find y axis
                    List<double> histogramYAxis = new List<double>();

                    for (int i = 0; i < 12; i++)
                    {
                        var start = histogramXAxis[i];
                        var end = histogramXAxis[i + 1];

                        var numQuery =
                        from num in dailyEndsDrilledHist
                        where num >= start && num < end
                        select num;

                        histogramYAxis.Add(numQuery.Count());
                    }

                    double lastItem = histogramXAxis.Last();

                    Histogram histogram1 = new Histogram();
                    histogram1.xAxis = histogramXAxis;
                    histogram1.yAxis = histogramYAxis;
                    double[] med = histogramYAxis.ToArray();
                    histogram1.Name = "Daily Ends Drilled Process Capability";
                    histogram1.NumberOfValues = dailyEndsDrilledHist.Count();
                    HistogramValues histogramValues = new HistogramValues();
                    histogramValues.Maximum = maximumH;
                    histogramValues.Minimum = minimumH;
                    histogram1.HistogramValues = histogramValues;
                    histogram1.Mean = dailyEndsDrilledHist.Sum() / dailyEndsDrilledHist.Count();



                    double minimumH2 = dailyEndsDrilledHist2.Min();
                    double maximumH2 = dailyEndsDrilledHist2.Max();
                    double divider2 = dailyEndsDrilledHist2.Max() / 11;

                    //Find x axis
                    List<double> histogramXAxis2 = new List<double>();

                    for (int i = 0; i < 13; i++)
                    {
                        histogramXAxis2.Add(i * Math.Round(divider2, 0));
                    }

                    //Find y axis
                    List<double> histogramYAxis2 = new List<double>();

                    for (int i = 0; i < 12; i++)
                    {
                        var start = histogramXAxis2[i];
                        var end = histogramXAxis2[i + 1];

                        var numQuery =
                        from num in dailyEndsDrilledHist2
                        where num >= start && num < end
                        select num;

                        histogramYAxis2.Add(numQuery.Count());
                    }

                    double lastItem2 = histogramXAxis2.Last();

                    Histogram histogram2 = new Histogram();
                    histogram2.xAxis = histogramXAxis2;
                    histogram2.yAxis = histogramYAxis2;
                    double[] med2 = histogramYAxis2.ToArray();
                    histogram2.Name = "First 4 Hours Drilled Ends Process Capability";
                    histogram2.NumberOfValues = dailyEndsDrilledHist2.Count();
                    HistogramValues histogramValues2 = new HistogramValues();
                    histogramValues2.Maximum = maximumH2;
                    histogramValues2.Minimum = minimumH2;
                    histogram2.HistogramValues = histogramValues2;
                    histogram2.Mean = dailyEndsDrilledHist2.Sum() / dailyEndsDrilledHist2.Count();




                    double minimumH3 = dailyEndsDrilledHist3.Min();
                    double maximumH3 = dailyEndsDrilledHist3.Max();
                    double divider3 = dailyEndsDrilledHist3.Max() / 11;

                    //Find x axis
                    List<double> histogramXAxis3 = new List<double>();

                    for (int i = 0; i < 13; i++)
                    {
                        histogramXAxis3.Add(i * Math.Round(divider3, 0));
                    }

                    //Find y axis
                    List<double> histogramYAxis3 = new List<double>();

                    for (int i = 0; i < 12; i++)
                    {
                        var start = histogramXAxis3[i];
                        var end = histogramXAxis3[i + 1];

                        var numQuery =
                        from num in dailyEndsDrilledHist3
                        where num >= start && num < end
                        select num;

                        histogramYAxis3.Add(numQuery.Count());
                    }

                    double lastItem3 = histogramXAxis3.Last();

                    Histogram histogram3 = new Histogram();
                    histogram3.xAxis = histogramXAxis3;
                    histogram3.yAxis = histogramYAxis3;
                    double[] med3 = histogramYAxis3.ToArray();
                    histogram3.Name = "Daily Ends Supported Process Capability";
                    histogram3.NumberOfValues = dailyEndsDrilledHist3.Count();
                    HistogramValues histogramValues3 = new HistogramValues();
                    histogramValues3.Maximum = maximumH3;
                    histogramValues3.Minimum = minimumH3;
                    histogram3.HistogramValues = histogramValues3;
                    histogram3.Mean = dailyEndsDrilledHist3.Sum() / dailyEndsDrilledHist3.Count();


                    double minimumH4 = dailyEndsDrilledHist4.Min();
                    double maximumH4 = dailyEndsDrilledHist4.Max();
                    double divider4 = dailyEndsDrilledHist4.Max() / 11;

                    //Find x axis
                    List<double> histogramXAxis4 = new List<double>();

                    for (int i = 0; i < 13; i++)
                    {
                        histogramXAxis4.Add(i * Math.Round(divider4, 0));
                    }

                    //Find y axis
                    List<double> histogramYAxis4 = new List<double>();

                    for (int i = 0; i < 12; i++)
                    {
                        var start = histogramXAxis4[i];
                        var end = histogramXAxis4[i + 1];

                        var numQuery =
                        from num in dailyEndsDrilledHist4
                        where num >= start && num < end
                        select num;

                        histogramYAxis4.Add(numQuery.Count());
                    }

                    double lastItem4 = histogramXAxis4.Last();

                    Histogram histogram4 = new Histogram();
                    histogram4.xAxis = histogramXAxis4;
                    histogram4.yAxis = histogramYAxis4;
                    double[] med4 = histogramYAxis4.ToArray();
                    histogram4.Name = "First 4 Hours Supported Ends Process Capability";
                    histogram4.NumberOfValues = dailyEndsDrilledHist4.Count();
                    HistogramValues histogramValues4 = new HistogramValues();
                    histogramValues4.Maximum = maximumH4;
                    histogramValues4.Minimum = minimumH4;
                    histogram4.HistogramValues = histogramValues4;
                    histogram4.Mean = dailyEndsDrilledHist4.Sum() / dailyEndsDrilledHist4.Count();



                    double minimumH5 = dailyEndsDrilledHist5.Min();
                    double maximumH5 = dailyEndsDrilledHist5.Max();
                    double divider5 = dailyEndsDrilledHist5.Max() / 11;

                    //Find x axis
                    List<double> histogramXAxis5 = new List<double>();

                    for (int i = 0; i < 13; i++)
                    {
                        histogramXAxis5.Add(i * Math.Round(divider5, 0));
                    }

                    //Find y axis
                    List<double> histogramYAxis5 = new List<double>();

                    for (int i = 0; i < 12; i++)
                    {
                        var start = histogramXAxis5[i];
                        var end = histogramXAxis5[i + 1];

                        var numQuery =
                        from num in dailyEndsDrilledHist5
                        where num >= start && num < end
                        select num;

                        histogramYAxis5.Add(numQuery.Count());
                    }

                    double lastItem5 = histogramXAxis5.Last();

                    Histogram histogram5 = new Histogram();
                    histogram5.xAxis = histogramXAxis5;
                    histogram5.yAxis = histogramYAxis5;
                    double[] med5 = histogramYAxis5.ToArray();
                    histogram5.Name = "Daily Tons Hoisted Process Capability";
                    histogram5.NumberOfValues = dailyEndsDrilledHist5.Count();
                    HistogramValues histogramValues5 = new HistogramValues();
                    histogramValues5.Maximum = maximumH5;
                    histogramValues5.Minimum = minimumH5;
                    histogram5.HistogramValues = histogramValues5;
                    histogram5.Mean = dailyEndsDrilledHist5.Sum() / dailyEndsDrilledHist5.Count();



                    PlotBox plotBox = new PlotBox();
                    var EndsDrilledPercentil = dailyEndsDrilledPercentile.ToArray();
                    Array.Sort(EndsDrilledPercentil);

                    PlotBoxValues plotBoxValues = new PlotBoxValues();

                    plotBoxValues.Minimum = EndsDrilledPercentil.First();
                    plotBoxValues.LowerQuartile = Percentile(EndsDrilledPercentil, 25);
                    plotBoxValues.Median = Median(EndsDrilledPercentil);
                    plotBoxValues.UpperQuartile = Percentile(EndsDrilledPercentil, 75);
                    plotBoxValues.Maximum = EndsDrilledPercentil.Last();
                    plotBox.plotBoxValues = plotBoxValues;
                    plotBox.NumberOfValues = dailyEndsDrilledPercentile.Count;
                    plotBox.Mean = EndsDrilledPercentil.Sum() / EndsDrilledPercentil.Length;
                    plotBox.Axis = model.StartDate.ToString("dd-MM-yyyy") + " To " + model.EndDate.ToString("dd-MM-yyyy");
                    plotBox.Name = "EndsDrilledPlotBox";


                    PlotBox plotBox2 = new PlotBox();
                    var EndsDrilledPercentil2 = dailyFirst4HoursEndsDrilledPercentile.ToArray();
                    Array.Sort(EndsDrilledPercentil2);

                    PlotBoxValues plotBoxValues2 = new PlotBoxValues();

                    plotBoxValues2.Minimum = EndsDrilledPercentil2.First();
                    plotBoxValues2.LowerQuartile = Percentile(EndsDrilledPercentil2, 25);
                    plotBoxValues2.Median = Median(EndsDrilledPercentil2);
                    plotBoxValues2.UpperQuartile = Percentile(EndsDrilledPercentil2, 75);
                    plotBoxValues2.Maximum = EndsDrilledPercentil2.Last();
                    plotBox2.plotBoxValues = plotBoxValues2;
                    plotBox2.NumberOfValues = dailyFirst4HoursEndsDrilledPercentile.Count;
                    plotBox2.Mean = EndsDrilledPercentil2.Sum() / EndsDrilledPercentil2.Length;
                    plotBox2.Axis = model.StartDate.ToString("dd-MM-yyyy") + " To " + model.EndDate.ToString("dd-MM-yyyy");
                    plotBox2.Name = "First4EndsDrilledPlotBox";


                    PlotBox plotBox3 = new PlotBox();
                    var EndsDrilledPercentil3 = endsSupportedPercentile.ToArray();
                    Array.Sort(EndsDrilledPercentil3);

                    PlotBoxValues plotBoxValues3 = new PlotBoxValues();

                    plotBoxValues3.Minimum = EndsDrilledPercentil3.First();
                    plotBoxValues3.LowerQuartile = Percentile(EndsDrilledPercentil3, 25);
                    plotBoxValues3.Median = Median(EndsDrilledPercentil3);
                    plotBoxValues3.UpperQuartile = Percentile(EndsDrilledPercentil3, 75);
                    plotBoxValues3.Maximum = EndsDrilledPercentil3.Last();
                    plotBox3.plotBoxValues = plotBoxValues3;
                    plotBox3.NumberOfValues = endsSupportedPercentile.Count;
                    plotBox3.Mean = EndsDrilledPercentil3.Sum() / EndsDrilledPercentil3.Length;
                    plotBox3.Axis = model.StartDate.ToString("dd-MM-yyyy") + " To " + model.EndDate.ToString("dd-MM-yyyy");
                    plotBox3.Name = "EndsSupportedPlotBox";

                    PlotBox plotBox4 = new PlotBox();
                    var EndsDrilledPercentil4 = supportedEndsPercentile.ToArray();
                    Array.Sort(EndsDrilledPercentil4);

                    PlotBoxValues plotBoxValues4 = new PlotBoxValues();

                    plotBoxValues4.Minimum = EndsDrilledPercentil4.First();
                    plotBoxValues4.LowerQuartile = Percentile(EndsDrilledPercentil4, 25);
                    plotBoxValues4.Median = Median(EndsDrilledPercentil4);
                    plotBoxValues4.UpperQuartile = Percentile(EndsDrilledPercentil4, 75);
                    plotBoxValues4.Maximum = EndsDrilledPercentil4.Last();
                    plotBox4.plotBoxValues = plotBoxValues4;
                    plotBox4.NumberOfValues = supportedEndsPercentile.Count;
                    plotBox4.Mean = EndsDrilledPercentil4.Sum() / EndsDrilledPercentil4.Length;
                    plotBox4.Axis = model.StartDate.ToString("dd-MM-yyyy") + " To " + model.EndDate.ToString("dd-MM-yyyy");
                    plotBox4.Name = "SupportedEndsPlotBox";

                    PlotBox plotBox5 = new PlotBox();
                    var EndsDrilledPercentil5 = supportedEndsPercentile2.ToArray();
                    Array.Sort(EndsDrilledPercentil5);

                    PlotBoxValues plotBoxValues5 = new PlotBoxValues();

                    plotBoxValues5.Minimum = EndsDrilledPercentil5.First();
                    plotBoxValues5.LowerQuartile = Percentile(EndsDrilledPercentil5, 25);
                    plotBoxValues5.Median = Median(EndsDrilledPercentil5);
                    plotBoxValues5.UpperQuartile = Percentile(EndsDrilledPercentil5, 75);
                    plotBoxValues5.Maximum = EndsDrilledPercentil5.Last();
                    plotBox5.plotBoxValues = plotBoxValues5;
                    plotBox5.NumberOfValues = supportedEndsPercentile2.Count;
                    plotBox5.Mean = EndsDrilledPercentil5.Sum() / EndsDrilledPercentil5.Length;
                    plotBox5.Axis = model.StartDate.ToString("dd-MM-yyyy") + " To " + model.EndDate.ToString("dd-MM-yyyy");
                    plotBox5.Name = "HoistedTonsPlotBox";


                    double mean = 0;
                    double meanFirstFourHours = 0;
                    double meanDailyEndsSupported = 0;
                    double meanSupportedEnds = 0;
                    double meanUnleashedEnds = 0;
                    double meanHoistedTons = 0;
                    double meanCleanedEnds = 0;
                    double meanLashedPreparedEnds = 0;

                    int sum = dailyEndsDrilled.Sum();
                    int sumFirstFourHours = firstFourHoursDrilledEnds.Sum();
                    int sumDailyEndsSupported = dailyEndsSupported.Sum();
                    int sumSupportedEnds = supportedEnds.Sum();
                    int sumUnleashedEnds = unlashedEnds.Sum();
                    int sumHoistedTons = hoistedTons.Sum();
                    int sumCleanedEnds = cleanedEnds.Sum();
                    int sumLashedPreparedEnds = lashedPreparedEnds.Sum();

                    int length = dailyEndsDrilled.Count();
                    int lengthFirstFourHours = firstFourHoursDrilledEnds.Count();
                    int lengthdailyEndsSupported = dailyEndsSupported.Count();
                    int lengthSupportedEnds = supportedEnds.Count();
                    int lengthUnleashedEnds = unlashedEnds.Count();
                    int lengthHoistedTons = hoistedTons.Count();
                    int lengthCleanedEnds = cleanedEnds.Count();
                    int lengthLashedPreparedEnds = lashedPreparedEnds.Count();

                    if (dailyEndsDrilled.Count > 0)
                    {
                        mean = sum / length;
                    }

                    if (firstFourHoursDrilledEnds.Count > 0)
                    {
                        meanFirstFourHours = sumFirstFourHours / lengthFirstFourHours;
                    }

                    if (dailyEndsSupported.Count > 0)
                    {
                        meanDailyEndsSupported = sumDailyEndsSupported / lengthdailyEndsSupported;
                    }

                    if (supportedEnds.Count > 0)
                    {
                        meanSupportedEnds = sumSupportedEnds / lengthSupportedEnds;
                    }

                    if (unlashedEnds.Count > 0)
                    {
                        meanUnleashedEnds = sumUnleashedEnds / lengthUnleashedEnds;
                    }

                    if (hoistedTons.Count > 0)
                    {
                        meanHoistedTons = sumHoistedTons / lengthHoistedTons;
                    }

                    if (cleanedEnds.Count > 0)
                    {
                        meanCleanedEnds = sumCleanedEnds / lengthCleanedEnds;
                    }

                    if (lashedPreparedEnds.Count > 0)
                    {
                        meanLashedPreparedEnds = sumLashedPreparedEnds / lengthLashedPreparedEnds;
                    }

                    double sumOfSquaresOfDifferences = dailyEndsDrilled.Select(val => (val - mean) * (val - mean)).Sum();
                    double sumOfSquaresOfDifferencesFirst4 = firstFourHoursDrilledEnds.Select(val => (val - meanFirstFourHours) * (val - meanFirstFourHours)).Sum();
                    double sumOfDailyEndsSupported = dailyEndsSupported.Select(val => (val - meanDailyEndsSupported) * (val - meanDailyEndsSupported)).Sum();
                    double sumOfSupportedEnds = supportedEnds.Select(val => (val - meanSupportedEnds) * (val - meanSupportedEnds)).Sum();
                    double sumOfUnleasheEnds = unlashedEnds.Select(val => (val - meanUnleashedEnds) * (val - meanUnleashedEnds)).Sum();
                    double sumOfHoistedTons = hoistedTons.Select(val => (val - meanHoistedTons) * (val - meanHoistedTons)).Sum();
                    double sumOfCleanedEnds = cleanedEnds.Select(val => (val - meanCleanedEnds) * (val - meanCleanedEnds)).Sum();
                    double sumOfLashedPreparedEnds = cleanedEnds.Select(val => (val - meanCleanedEnds) * (val - meanCleanedEnds)).Sum();

                    double standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / dailyEndsDrilled.Count);
                    double standardDeviationFirst4Hours = Math.Sqrt(sumOfSquaresOfDifferencesFirst4 / firstFourHoursDrilledEnds.Count);
                    double standardDeviationDailySupportedEnds = Math.Sqrt(sumOfDailyEndsSupported / dailyEndsSupported.Count);
                    double standardDeviationSupportedEnds = Math.Sqrt(sumOfSupportedEnds / supportedEnds.Count);
                    double standardDeviationUnleashedEnds = Math.Sqrt(sumUnleashedEnds / unlashedEnds.Count);
                    double standardDeviationHoistedTons = Math.Sqrt(sumHoistedTons / hoistedTons.Count);
                    double standardDeviationCleanedEnds = Math.Sqrt(sumCleanedEnds / cleanedEnds.Count);
                    double standardDeviationLashedPreparedEnds = Math.Sqrt(sumLashedPreparedEnds / lashedPreparedEnds.Count);

                    List<ControlLimit> controlLimits = new List<ControlLimit>();
                    List<ControlLimit> controlLimitsFirst4Hours = new List<ControlLimit>();
                    List<ControlLimit> controlLimitsDailySupportedEnds = new List<ControlLimit>();
                    List<ControlLimit> controlLimitsSupportedEnds = new List<ControlLimit>();
                    List<ControlLimit> controlLimitsUnleashedEnds = new List<ControlLimit>();
                    List<ControlLimit> controlLimitsHoistedTons = new List<ControlLimit>();
                    List<ControlLimit> controlLimitsCleanedEnds = new List<ControlLimit>();
                    List<ControlLimit> controlLimitsLashedPreparedEnds = new List<ControlLimit>();

                    var orderedReadings = readings.OrderBy(id => id.DateProduction);
                    var orderedReadingsFirst4Hours = readings.OrderBy(id => id.DateProduction);
                    var orderedReadingsSupportedEnds = readings.OrderBy(id => id.DateProduction);
                    var orderedReadingsSupportedEndsl = readings.OrderBy(id => id.DateProduction);
                    var orderedReadingsUnleashedEndsl = readings.OrderBy(id => id.DateProduction);
                    var orderedReadingsHoistedTonsl = readings.OrderBy(id => id.DateProduction);
                    var orderedReadingsCleanedEndsl = readings.OrderBy(id => id.DateProduction);
                    var orderedReadingsPreparedLashedEndsl = readings.OrderBy(id => id.DateProduction);

                    foreach (var item in orderedReadings)
                    {
                        ControlLimit controlLimit = new ControlLimit();
                        controlLimit.date = item.DateProduction.ToString("yyyy-MM-dd");
                        controlLimit.mean = mean;
                        controlLimit.mesuarement = item.EndsDrilled;
                        controlLimit.lcl = mean - (3 * standardDeviation);
                        controlLimit.ucl = mean + (3 * standardDeviation);
                        controlLimits.Add(controlLimit);
                    }

                    foreach (var item in orderedReadingsFirst4Hours)
                    {
                        ControlLimit controlLimit = new ControlLimit();
                        controlLimit.date = item.DateProduction.ToString("yyyy-MM-dd");
                        controlLimit.mean = mean;
                        controlLimit.mesuarement = item.Day1st4HoursEnds;
                        controlLimit.lcl = mean - (3 * standardDeviationFirst4Hours);
                        controlLimit.ucl = mean + (3 * standardDeviationFirst4Hours);
                        controlLimitsFirst4Hours.Add(controlLimit);
                    }

                    foreach (var item in orderedReadingsSupportedEnds)
                    {
                        ControlLimit controlLimit = new ControlLimit();
                        controlLimit.date = item.DateProduction.ToString("yyyy-MM-dd");
                        controlLimit.mean = mean;
                        controlLimit.mesuarement = item.Day1st4HoursEnds;
                        controlLimit.lcl = mean - (3 * standardDeviationDailySupportedEnds);
                        controlLimit.ucl = mean + (3 * standardDeviationDailySupportedEnds);
                        controlLimitsDailySupportedEnds.Add(controlLimit);
                    }

                    foreach (var item in orderedReadingsSupportedEndsl)
                    {
                        ControlLimit controlLimit = new ControlLimit();
                        controlLimit.date = item.DateProduction.ToString("yyyy-MM-dd");
                        controlLimit.mean = mean;
                        controlLimit.mesuarement = item.SupportedEnds;
                        controlLimit.lcl = mean - (3 * standardDeviationSupportedEnds);
                        controlLimit.ucl = mean + (3 * standardDeviationSupportedEnds);
                        controlLimitsSupportedEnds.Add(controlLimit);
                    }

                    foreach (var item in orderedReadingsUnleashedEndsl)
                    {
                        ControlLimit controlLimit = new ControlLimit();
                        controlLimit.date = item.DateProduction.ToString("yyyy-MM-dd");
                        controlLimit.mean = mean;
                        controlLimit.mesuarement = item.UnlashedEnds;
                        controlLimit.lcl = mean - (3 * standardDeviationUnleashedEnds);
                        controlLimit.ucl = mean + (3 * standardDeviationUnleashedEnds);
                        controlLimitsUnleashedEnds.Add(controlLimit);
                    }

                    foreach (var item in orderedReadingsHoistedTonsl)
                    {
                        ControlLimit controlLimit = new ControlLimit();
                        controlLimit.date = item.DateProduction.ToString("yyyy-MM-dd");
                        controlLimit.mean = mean;
                        controlLimit.mesuarement = item.HoistedTons;
                        controlLimit.lcl = mean - (3 * standardDeviationHoistedTons);
                        controlLimit.ucl = mean + (3 * standardDeviationHoistedTons);
                        controlLimitsHoistedTons.Add(controlLimit);
                    }

                    foreach (var item in orderedReadingsCleanedEndsl)
                    {
                        ControlLimit controlLimit = new ControlLimit();
                        controlLimit.date = item.DateProduction.ToString("yyyy-MM-dd");
                        controlLimit.mean = mean;
                        controlLimit.mesuarement = item.TotalCleanedEnds;
                        controlLimit.lcl = mean - (3 * standardDeviationCleanedEnds);
                        controlLimit.ucl = mean + (3 * standardDeviationCleanedEnds);
                        controlLimitsCleanedEnds.Add(controlLimit);
                    }

                    foreach (var item in orderedReadingsPreparedLashedEndsl)
                    {
                        ControlLimit controlLimit = new ControlLimit();
                        controlLimit.date = item.DateProduction.ToString("yyyy-MM-dd");
                        controlLimit.mean = mean;
                        controlLimit.mesuarement = item.LashedPreparedForSupport;
                        controlLimit.lcl = mean - (3 * standardDeviationLashedPreparedEnds);
                        controlLimit.ucl = mean + (3 * standardDeviationLashedPreparedEnds);
                        controlLimitsLashedPreparedEnds.Add(controlLimit);
                    }

                    Dictionary<string, object> graphValues = new Dictionary<string, object>();
                    graphValues.Add("LineEndsDrilled", controlLimits);
                    graphValues.Add("First4Hours", controlLimitsFirst4Hours);
                    graphValues.Add("DailySupportedEnds", controlLimitsDailySupportedEnds);
                    graphValues.Add("SupportedEnds", controlLimitsSupportedEnds);
                    graphValues.Add("UnlashedEnds", controlLimitsUnleashedEnds);
                    graphValues.Add("HoistedTons", controlLimitsHoistedTons);
                    graphValues.Add("CleanedEnds", controlLimitsCleanedEnds);
                    graphValues.Add("LashedPreparedEnds", controlLimitsLashedPreparedEnds);
                    graphValues.Add("EndsDrilledPlotBox", plotBox);
                    graphValues.Add("First4EndsDrilledPlotBox", plotBox2);
                    graphValues.Add("EndsSupportedPlotBox", plotBox3);
                    graphValues.Add("SupportedEndsPlotBox", plotBox4);
                    graphValues.Add("HoistedTonsPlotBox", plotBox5);

                    graphValues.Add("Histogram1", histogram1);
                    graphValues.Add("Histogram2", histogram2);
                    graphValues.Add("Histogram3", histogram3);
                    graphValues.Add("Histogram4", histogram4);
                    graphValues.Add("Histogram5", histogram5);

                    masterDictionary.Add(graphValues);

                }

                if (masterDictionary.Count == 1)
                {
                    return Ok(masterDictionary[0]);
                }

                else if(masterDictionary.Count > 1)
                {
                    return Ok(masterDictionary[0]);
                }

                return Ok(masterDictionary);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }


        [HttpPost("")]
        public IActionResult GenerateChart(ChartModel model)
        {
            if (model == null)
            {
                return BadRequest("Make sure fields are completed correctly.");
            }
            try
            {

                int AssetId = model.AssetId;
                ProductionFlowAsset asset = _context.ProductionFlowAssets.FirstOrDefault(id => id.AssetId == AssetId);
                if (asset == null)
                {
                    return BadRequest("Make sure asset node is a productive unit! Or contact your administrator.");
                }
                int ClientAssetId = asset.ClientAssetNameId;

                List<Reading> readings = _context.Readings.Where(id => (id.AssetId == AssetId)
                && (id.DateProduction >= model.StartDate) && (id.DateProduction <= model.EndDate)).ToList();

                List<int> dailyEndsDrilled = new List<int>();
                List<int> firstFourHoursDrilledEnds = new List<int>();
                List<int> dailyEndsSupported = new List<int>();
                List<int> supportedEnds = new List<int>();
                List<int> unlashedEnds = new List<int>();
                List<int> hoistedTons = new List<int>();
                List<int> cleanedEnds = new List<int>();
                List<int> lashedPreparedEnds = new List<int>();

                List<double> dailyEndsDrilledPercentile = new List<double>();
                List<double> dailyFirst4HoursEndsDrilledPercentile = new List<double>();
                List<double> endsSupportedPercentile = new List<double>();
                List<double> supportedEndsPercentile = new List<double>();
                List<double> supportedEndsPercentile2 = new List<double>();


                List<double> dailyEndsDrilledHist = new List<double>();
                List<double> dailyEndsDrilledHist2 = new List<double>();
                List<double> dailyEndsDrilledHist3 = new List<double>();
                List<double> dailyEndsDrilledHist4 = new List<double>();
                List<double> dailyEndsDrilledHist5 = new List<double>();


                foreach (var item in readings)
                {
                    dailyEndsDrilledHist.Add(item.EndsDrilled);
                    dailyEndsDrilledHist2.Add(item.Day1st4HoursEnds);
                    dailyEndsDrilledHist3.Add(item.SupportedEnds);
                    dailyEndsDrilledHist4.Add(item.EndsSupported);
                    dailyEndsDrilledHist5.Add(item.HoistedTons);


                    dailyEndsDrilled.Add(item.EndsDrilled);
                    firstFourHoursDrilledEnds.Add(item.Day1st4HoursEnds);
                    dailyEndsSupported.Add(item.EndsSupported);
                    supportedEnds.Add(item.SupportedEnds);
                    unlashedEnds.Add(item.UnlashedEnds);
                    hoistedTons.Add(item.HoistedTons);
                    cleanedEnds.Add(item.TotalCleanedEnds);
                    lashedPreparedEnds.Add(item.LashedPreparedForSupport);


                    dailyEndsDrilledPercentile.Add(item.EndsDrilled);
                    dailyFirst4HoursEndsDrilledPercentile.Add(item.Day1st4HoursEnds);
                    endsSupportedPercentile.Add(item.EndsSupported);
                    supportedEndsPercentile.Add(item.SupportedEnds);
                    supportedEndsPercentile2.Add(item.HoistedTons);
                }



                double minimumH = dailyEndsDrilledHist.Min();
                double maximumH = dailyEndsDrilledHist.Max();
                double divider = dailyEndsDrilledHist.Max() / 11;

                //Find x axis
                List<double> histogramXAxis = new List<double>();

                for (int i = 0; i < 13; i++)
                {
                    histogramXAxis.Add(i * Math.Round(divider, 0));
                }

                //Find y axis
                List<double> histogramYAxis = new List<double>();

                for (int i = 0; i < 12; i++)
                {
                    var start = histogramXAxis[i];
                    var end = histogramXAxis[i + 1];

                    var numQuery =
                    from num in dailyEndsDrilledHist
                    where num >= start && num < end
                    select num;

                    histogramYAxis.Add(numQuery.Count());
                }

                double lastItem = histogramXAxis.Last();

                Histogram histogram1 = new Histogram();
                histogram1.xAxis = histogramXAxis;
                histogram1.yAxis = histogramYAxis;
                double[] med = histogramYAxis.ToArray();
                histogram1.Name = "Daily Ends Drilled Process Capability";
                histogram1.NumberOfValues = dailyEndsDrilledHist.Count();
                HistogramValues histogramValues = new HistogramValues();
                histogramValues.Maximum = maximumH;
                histogramValues.Minimum = minimumH;
                histogram1.HistogramValues = histogramValues;
                histogram1.Mean = dailyEndsDrilledHist.Sum() / dailyEndsDrilledHist.Count();



                double minimumH2 = dailyEndsDrilledHist2.Min();
                double maximumH2 = dailyEndsDrilledHist2.Max();
                double divider2 = dailyEndsDrilledHist2.Max() / 11;

                //Find x axis
                List<double> histogramXAxis2 = new List<double>();

                for (int i = 0; i < 13; i++)
                {
                    histogramXAxis2.Add(i * Math.Round(divider2,0));
                }

                //Find y axis
                List<double> histogramYAxis2 = new List<double>();

                for (int i = 0; i < 12; i++)
                {
                    var start = histogramXAxis2[i];
                    var end = histogramXAxis2[i + 1];

                    var numQuery =
                    from num in dailyEndsDrilledHist2
                    where num >= start && num < end
                    select num;

                    histogramYAxis2.Add(numQuery.Count());
                }

                double lastItem2 = histogramXAxis2.Last();

                Histogram histogram2 = new Histogram();
                histogram2.xAxis = histogramXAxis2;
                histogram2.yAxis = histogramYAxis2;
                double[] med2 = histogramYAxis2.ToArray();
                histogram2.Name = "First 4 Hours Drilled Ends Process Capability";
                histogram2.NumberOfValues = dailyEndsDrilledHist2.Count();
                HistogramValues histogramValues2 = new HistogramValues();
                histogramValues2.Maximum = maximumH2;
                histogramValues2.Minimum = minimumH2;
                histogram2.HistogramValues = histogramValues2;
                histogram2.Mean = dailyEndsDrilledHist2.Sum() / dailyEndsDrilledHist2.Count();




                double minimumH3 = dailyEndsDrilledHist3.Min();
                double maximumH3 = dailyEndsDrilledHist3.Max();
                double divider3 = dailyEndsDrilledHist3.Max() / 11;

                //Find x axis
                List<double> histogramXAxis3 = new List<double>();

                for (int i = 0; i < 13; i++)
                {
                    histogramXAxis3.Add(i * Math.Round(divider3, 0));
                }

                //Find y axis
                List<double> histogramYAxis3 = new List<double>();

                for (int i = 0; i < 12; i++)
                {
                    var start = histogramXAxis3[i];
                    var end = histogramXAxis3[i + 1];

                    var numQuery =
                    from num in dailyEndsDrilledHist3
                    where num >= start && num < end
                    select num;

                    histogramYAxis3.Add(numQuery.Count());
                }

                double lastItem3 = histogramXAxis3.Last();

                Histogram histogram3 = new Histogram();
                histogram3.xAxis = histogramXAxis3;
                histogram3.yAxis = histogramYAxis3;
                double[] med3 = histogramYAxis3.ToArray();
                histogram3.Name = "Daily Ends Supported Process Capability";
                histogram3.NumberOfValues = dailyEndsDrilledHist3.Count();
                HistogramValues histogramValues3 = new HistogramValues();
                histogramValues3.Maximum = maximumH3;
                histogramValues3.Minimum = minimumH3;
                histogram3.HistogramValues = histogramValues3;
                histogram3.Mean = dailyEndsDrilledHist3.Sum() / dailyEndsDrilledHist3.Count();


                double minimumH4 = dailyEndsDrilledHist4.Min();
                double maximumH4 = dailyEndsDrilledHist4.Max();
                double divider4 = dailyEndsDrilledHist4.Max() / 11;

                //Find x axis
                List<double> histogramXAxis4 = new List<double>();

                for (int i = 0; i < 13; i++)
                {
                    histogramXAxis4.Add(i * Math.Round(divider4, 0));
                }

                //Find y axis
                List<double> histogramYAxis4 = new List<double>();

                for (int i = 0; i < 12; i++)
                {
                    var start = histogramXAxis4[i];
                    var end = histogramXAxis4[i + 1];

                    var numQuery =
                    from num in dailyEndsDrilledHist4
                    where num >= start && num < end
                    select num;

                    histogramYAxis4.Add(numQuery.Count());
                }

                double lastItem4 = histogramXAxis4.Last();

                Histogram histogram4 = new Histogram();
                histogram4.xAxis = histogramXAxis4;
                histogram4.yAxis = histogramYAxis4;
                double[] med4 = histogramYAxis4.ToArray();
                histogram4.Name = "First 4 Hours Supported Ends Process Capability";
                histogram4.NumberOfValues = dailyEndsDrilledHist4.Count();
                HistogramValues histogramValues4 = new HistogramValues();
                histogramValues4.Maximum = maximumH4;
                histogramValues4.Minimum = minimumH4;
                histogram4.HistogramValues = histogramValues4;
                histogram4.Mean = dailyEndsDrilledHist4.Sum() / dailyEndsDrilledHist4.Count();



                double minimumH5 = dailyEndsDrilledHist5.Min();
                double maximumH5 = dailyEndsDrilledHist5.Max();
                double divider5 = dailyEndsDrilledHist5.Max() / 11;

                //Find x axis
                List<double> histogramXAxis5 = new List<double>();

                for (int i = 0; i < 13; i++)
                {
                    histogramXAxis5.Add(i * Math.Round(divider5, 0));
                }

                //Find y axis
                List<double> histogramYAxis5 = new List<double>();

                for (int i = 0; i < 12; i++)
                {
                    var start = histogramXAxis5[i];
                    var end = histogramXAxis5[i + 1];

                    var numQuery =
                    from num in dailyEndsDrilledHist5
                    where num >= start && num < end
                    select num;

                    histogramYAxis5.Add(numQuery.Count());
                }

                double lastItem5 = histogramXAxis5.Last();

                Histogram histogram5 = new Histogram();
                histogram5.xAxis = histogramXAxis5;
                histogram5.yAxis = histogramYAxis5;
                double[] med5 = histogramYAxis5.ToArray();
                histogram5.Name = "Daily Tons Hoisted Process Capability";
                histogram5.NumberOfValues = dailyEndsDrilledHist5.Count();
                HistogramValues histogramValues5 = new HistogramValues();
                histogramValues5.Maximum = maximumH5;
                histogramValues5.Minimum = minimumH5;
                histogram5.HistogramValues = histogramValues5;
                histogram5.Mean = dailyEndsDrilledHist5.Sum() / dailyEndsDrilledHist5.Count();



                PlotBox plotBox = new PlotBox();
                var EndsDrilledPercentil = dailyEndsDrilledPercentile.ToArray();
                Array.Sort(EndsDrilledPercentil);

                PlotBoxValues plotBoxValues = new PlotBoxValues();

                plotBoxValues.Minimum = EndsDrilledPercentil.First();
                plotBoxValues.LowerQuartile = Percentile(EndsDrilledPercentil, 25);
                plotBoxValues.Median = Median(EndsDrilledPercentil);
                plotBoxValues.UpperQuartile = Percentile(EndsDrilledPercentil, 75);
                plotBoxValues.Maximum = EndsDrilledPercentil.Last();
                plotBox.plotBoxValues = plotBoxValues;
                plotBox.NumberOfValues = dailyEndsDrilledPercentile.Count;
                plotBox.Mean = EndsDrilledPercentil.Sum() / EndsDrilledPercentil.Length;
                plotBox.Axis = model.StartDate.ToString("dd-MM-yyyy") + " To " + model.EndDate.ToString("dd-MM-yyyy");
                plotBox.Name = "EndsDrilledPlotBox";


                PlotBox plotBox2 = new PlotBox();
                var EndsDrilledPercentil2 = dailyFirst4HoursEndsDrilledPercentile.ToArray();
                Array.Sort(EndsDrilledPercentil2);

                PlotBoxValues plotBoxValues2 = new PlotBoxValues();

                plotBoxValues2.Minimum = EndsDrilledPercentil2.First();
                plotBoxValues2.LowerQuartile = Percentile(EndsDrilledPercentil2, 25);
                plotBoxValues2.Median = Median(EndsDrilledPercentil2);
                plotBoxValues2.UpperQuartile = Percentile(EndsDrilledPercentil2, 75);
                plotBoxValues2.Maximum = EndsDrilledPercentil2.Last();
                plotBox2.plotBoxValues = plotBoxValues2;
                plotBox2.NumberOfValues = dailyFirst4HoursEndsDrilledPercentile.Count;
                plotBox2.Mean = EndsDrilledPercentil2.Sum() / EndsDrilledPercentil2.Length;
                plotBox2.Axis = model.StartDate.ToString("dd-MM-yyyy") + " To " + model.EndDate.ToString("dd-MM-yyyy");
                plotBox2.Name = "First4EndsDrilledPlotBox";


                PlotBox plotBox3 = new PlotBox();
                var EndsDrilledPercentil3 = endsSupportedPercentile.ToArray();
                Array.Sort(EndsDrilledPercentil3);

                PlotBoxValues plotBoxValues3 = new PlotBoxValues();

                plotBoxValues3.Minimum = EndsDrilledPercentil3.First();
                plotBoxValues3.LowerQuartile = Percentile(EndsDrilledPercentil3, 25);
                plotBoxValues3.Median = Median(EndsDrilledPercentil3);
                plotBoxValues3.UpperQuartile = Percentile(EndsDrilledPercentil3, 75);
                plotBoxValues3.Maximum = EndsDrilledPercentil3.Last();
                plotBox3.plotBoxValues = plotBoxValues3;
                plotBox3.NumberOfValues = endsSupportedPercentile.Count;
                plotBox3.Mean = EndsDrilledPercentil3.Sum() / EndsDrilledPercentil3.Length;
                plotBox3.Axis = model.StartDate.ToString("dd-MM-yyyy") + " To " + model.EndDate.ToString("dd-MM-yyyy");
                plotBox3.Name = "EndsSupportedPlotBox";

                PlotBox plotBox4 = new PlotBox();
                var EndsDrilledPercentil4 = supportedEndsPercentile.ToArray();
                Array.Sort(EndsDrilledPercentil4);

                PlotBoxValues plotBoxValues4 = new PlotBoxValues();

                plotBoxValues4.Minimum = EndsDrilledPercentil4.First();
                plotBoxValues4.LowerQuartile = Percentile(EndsDrilledPercentil4, 25);
                plotBoxValues4.Median = Median(EndsDrilledPercentil4);
                plotBoxValues4.UpperQuartile = Percentile(EndsDrilledPercentil4, 75);
                plotBoxValues4.Maximum = EndsDrilledPercentil4.Last();
                plotBox4.plotBoxValues = plotBoxValues4;
                plotBox4.NumberOfValues = supportedEndsPercentile.Count;
                plotBox4.Mean = EndsDrilledPercentil4.Sum() / EndsDrilledPercentil4.Length;
                plotBox4.Axis = model.StartDate.ToString("dd-MM-yyyy") + " To " + model.EndDate.ToString("dd-MM-yyyy");
                plotBox4.Name = "SupportedEndsPlotBox";

                PlotBox plotBox5 = new PlotBox();
                var EndsDrilledPercentil5 = supportedEndsPercentile2.ToArray();
                Array.Sort(EndsDrilledPercentil5);

                PlotBoxValues plotBoxValues5 = new PlotBoxValues();

                plotBoxValues5.Minimum = EndsDrilledPercentil5.First();
                plotBoxValues5.LowerQuartile = Percentile(EndsDrilledPercentil5, 25);
                plotBoxValues5.Median = Median(EndsDrilledPercentil5);
                plotBoxValues5.UpperQuartile = Percentile(EndsDrilledPercentil5, 75);
                plotBoxValues5.Maximum = EndsDrilledPercentil5.Last();
                plotBox5.plotBoxValues = plotBoxValues5;
                plotBox5.NumberOfValues = supportedEndsPercentile2.Count;
                plotBox5.Mean = EndsDrilledPercentil5.Sum() / EndsDrilledPercentil5.Length;
                plotBox5.Axis = model.StartDate.ToString("dd-MM-yyyy") + " To " + model.EndDate.ToString("dd-MM-yyyy");
                plotBox5.Name = "HoistedTonsPlotBox";


                double mean = 0;
                double meanFirstFourHours = 0;
                double meanDailyEndsSupported = 0;
                double meanSupportedEnds = 0;
                double meanUnleashedEnds = 0;
                double meanHoistedTons = 0;
                double meanCleanedEnds = 0;
                double meanLashedPreparedEnds = 0;

                int sum = dailyEndsDrilled.Sum();
                int sumFirstFourHours = firstFourHoursDrilledEnds.Sum();
                int sumDailyEndsSupported = dailyEndsSupported.Sum();
                int sumSupportedEnds = supportedEnds.Sum();
                int sumUnleashedEnds = unlashedEnds.Sum();
                int sumHoistedTons = hoistedTons.Sum();
                int sumCleanedEnds = cleanedEnds.Sum();
                int sumLashedPreparedEnds = lashedPreparedEnds.Sum();

                int length = dailyEndsDrilled.Count();
                int lengthFirstFourHours = firstFourHoursDrilledEnds.Count();
                int lengthdailyEndsSupported = dailyEndsSupported.Count();
                int lengthSupportedEnds = supportedEnds.Count();
                int lengthUnleashedEnds = unlashedEnds.Count();
                int lengthHoistedTons = hoistedTons.Count();
                int lengthCleanedEnds = cleanedEnds.Count();
                int lengthLashedPreparedEnds = lashedPreparedEnds.Count();

                if (dailyEndsDrilled.Count > 0)
                {
                    mean = sum / length;
                }

                if (firstFourHoursDrilledEnds.Count > 0)
                {
                    meanFirstFourHours = sumFirstFourHours / lengthFirstFourHours;
                }

                if (dailyEndsSupported.Count > 0)
                {
                    meanDailyEndsSupported = sumDailyEndsSupported / lengthdailyEndsSupported;
                }

                if (supportedEnds.Count > 0)
                {
                    meanSupportedEnds = sumSupportedEnds / lengthSupportedEnds;
                }

                if (unlashedEnds.Count > 0)
                {
                    meanUnleashedEnds = sumUnleashedEnds / lengthUnleashedEnds;
                }

                if (hoistedTons.Count > 0)
                {
                    meanHoistedTons = sumHoistedTons / lengthHoistedTons;
                }

                if (cleanedEnds.Count > 0)
                {
                    meanCleanedEnds = sumCleanedEnds / lengthCleanedEnds;
                }

                if (lashedPreparedEnds.Count > 0)
                {
                    meanLashedPreparedEnds = sumLashedPreparedEnds / lengthLashedPreparedEnds;
                }

                double sumOfSquaresOfDifferences = dailyEndsDrilled.Select(val => (val - mean) * (val - mean)).Sum();
                double sumOfSquaresOfDifferencesFirst4 = firstFourHoursDrilledEnds.Select(val => (val - meanFirstFourHours) * (val - meanFirstFourHours)).Sum();
                double sumOfDailyEndsSupported = dailyEndsSupported.Select(val => (val - meanDailyEndsSupported) * (val - meanDailyEndsSupported)).Sum();
                double sumOfSupportedEnds = supportedEnds.Select(val => (val - meanSupportedEnds) * (val - meanSupportedEnds)).Sum();
                double sumOfUnleasheEnds = unlashedEnds.Select(val => (val - meanUnleashedEnds) * (val - meanUnleashedEnds)).Sum();
                double sumOfHoistedTons = hoistedTons.Select(val => (val - meanHoistedTons) * (val - meanHoistedTons)).Sum();
                double sumOfCleanedEnds = cleanedEnds.Select(val => (val - meanCleanedEnds) * (val - meanCleanedEnds)).Sum();
                double sumOfLashedPreparedEnds = cleanedEnds.Select(val => (val - meanCleanedEnds) * (val - meanCleanedEnds)).Sum();

                double standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / dailyEndsDrilled.Count);
                double standardDeviationFirst4Hours = Math.Sqrt(sumOfSquaresOfDifferencesFirst4 / firstFourHoursDrilledEnds.Count);
                double standardDeviationDailySupportedEnds = Math.Sqrt(sumOfDailyEndsSupported / dailyEndsSupported.Count);
                double standardDeviationSupportedEnds = Math.Sqrt(sumOfSupportedEnds / supportedEnds.Count);
                double standardDeviationUnleashedEnds = Math.Sqrt(sumUnleashedEnds / unlashedEnds.Count);
                double standardDeviationHoistedTons = Math.Sqrt(sumHoistedTons / hoistedTons.Count);
                double standardDeviationCleanedEnds = Math.Sqrt(sumCleanedEnds / cleanedEnds.Count);
                double standardDeviationLashedPreparedEnds = Math.Sqrt(sumLashedPreparedEnds / lashedPreparedEnds.Count);

                List<ControlLimit> controlLimits = new List<ControlLimit>();
                List<ControlLimit> controlLimitsFirst4Hours = new List<ControlLimit>();
                List<ControlLimit> controlLimitsDailySupportedEnds = new List<ControlLimit>();
                List<ControlLimit> controlLimitsSupportedEnds = new List<ControlLimit>();
                List<ControlLimit> controlLimitsUnleashedEnds = new List<ControlLimit>();
                List<ControlLimit> controlLimitsHoistedTons = new List<ControlLimit>();
                List<ControlLimit> controlLimitsCleanedEnds = new List<ControlLimit>();
                List<ControlLimit> controlLimitsLashedPreparedEnds = new List<ControlLimit>();

                var orderedReadings = readings.OrderBy(id => id.DateProduction);
                var orderedReadingsFirst4Hours = readings.OrderBy(id => id.DateProduction);
                var orderedReadingsSupportedEnds = readings.OrderBy(id => id.DateProduction);
                var orderedReadingsSupportedEndsl = readings.OrderBy(id => id.DateProduction);
                var orderedReadingsUnleashedEndsl = readings.OrderBy(id => id.DateProduction);
                var orderedReadingsHoistedTonsl = readings.OrderBy(id => id.DateProduction);
                var orderedReadingsCleanedEndsl = readings.OrderBy(id => id.DateProduction);
                var orderedReadingsPreparedLashedEndsl = readings.OrderBy(id => id.DateProduction);

                foreach (var item in orderedReadings)
                {
                    ControlLimit controlLimit = new ControlLimit();
                    controlLimit.date = item.DateProduction.ToString("yyyy-MM-dd");
                    controlLimit.mean = mean;
                    controlLimit.mesuarement = item.EndsDrilled;
                    controlLimit.lcl = mean - (3 * standardDeviation);
                    controlLimit.ucl = mean + (3 * standardDeviation);
                    controlLimits.Add(controlLimit);
                }

                foreach (var item in orderedReadingsFirst4Hours)
                {
                    ControlLimit controlLimit = new ControlLimit();
                    controlLimit.date = item.DateProduction.ToString("yyyy-MM-dd");
                    controlLimit.mean = mean;
                    controlLimit.mesuarement = item.Day1st4HoursEnds;
                    controlLimit.lcl = mean - (3 * standardDeviationFirst4Hours);
                    controlLimit.ucl = mean + (3 * standardDeviationFirst4Hours);
                    controlLimitsFirst4Hours.Add(controlLimit);
                }

                foreach (var item in orderedReadingsSupportedEnds)
                {
                    ControlLimit controlLimit = new ControlLimit();
                    controlLimit.date = item.DateProduction.ToString("yyyy-MM-dd");
                    controlLimit.mean = mean;
                    controlLimit.mesuarement = item.Day1st4HoursEnds;
                    controlLimit.lcl = mean - (3 * standardDeviationDailySupportedEnds);
                    controlLimit.ucl = mean + (3 * standardDeviationDailySupportedEnds);
                    controlLimitsDailySupportedEnds.Add(controlLimit);
                }

                foreach (var item in orderedReadingsSupportedEndsl)
                {
                    ControlLimit controlLimit = new ControlLimit();
                    controlLimit.date = item.DateProduction.ToString("yyyy-MM-dd");
                    controlLimit.mean = mean;
                    controlLimit.mesuarement = item.SupportedEnds;
                    controlLimit.lcl = mean - (3 * standardDeviationSupportedEnds);
                    controlLimit.ucl = mean + (3 * standardDeviationSupportedEnds);
                    controlLimitsSupportedEnds.Add(controlLimit);
                }

                foreach (var item in orderedReadingsUnleashedEndsl)
                {
                    ControlLimit controlLimit = new ControlLimit();
                    controlLimit.date = item.DateProduction.ToString("yyyy-MM-dd");
                    controlLimit.mean = mean;
                    controlLimit.mesuarement = item.UnlashedEnds;
                    controlLimit.lcl = mean - (3 * standardDeviationUnleashedEnds);
                    controlLimit.ucl = mean + (3 * standardDeviationUnleashedEnds);
                    controlLimitsUnleashedEnds.Add(controlLimit);
                }

                foreach (var item in orderedReadingsHoistedTonsl)
                {
                    ControlLimit controlLimit = new ControlLimit();
                    controlLimit.date = item.DateProduction.ToString("yyyy-MM-dd");
                    controlLimit.mean = mean;
                    controlLimit.mesuarement = item.HoistedTons;
                    controlLimit.lcl = mean - (3 * standardDeviationHoistedTons);
                    controlLimit.ucl = mean + (3 * standardDeviationHoistedTons);
                    controlLimitsHoistedTons.Add(controlLimit);
                }

                foreach (var item in orderedReadingsCleanedEndsl)
                {
                    ControlLimit controlLimit = new ControlLimit();
                    controlLimit.date = item.DateProduction.ToString("yyyy-MM-dd");
                    controlLimit.mean = mean;
                    controlLimit.mesuarement = item.TotalCleanedEnds;
                    controlLimit.lcl = mean - (3 * standardDeviationCleanedEnds);
                    controlLimit.ucl = mean + (3 * standardDeviationCleanedEnds);
                    controlLimitsCleanedEnds.Add(controlLimit);
                }

                foreach (var item in orderedReadingsPreparedLashedEndsl)
                {
                    ControlLimit controlLimit = new ControlLimit();
                    controlLimit.date = item.DateProduction.ToString("yyyy-MM-dd");
                    controlLimit.mean = mean;
                    controlLimit.mesuarement = item.LashedPreparedForSupport;
                    controlLimit.lcl = mean - (3 * standardDeviationLashedPreparedEnds);
                    controlLimit.ucl = mean + (3 * standardDeviationLashedPreparedEnds);
                    controlLimitsLashedPreparedEnds.Add(controlLimit);
                }

                Dictionary<string, object> graphValues = new Dictionary<string, object>();
                graphValues.Add("LineEndsDrilled", controlLimits);
                graphValues.Add("First4Hours", controlLimitsFirst4Hours);
                graphValues.Add("DailySupportedEnds", controlLimitsDailySupportedEnds);
                graphValues.Add("SupportedEnds", controlLimitsSupportedEnds);
                graphValues.Add("UnlashedEnds", controlLimitsUnleashedEnds);
                graphValues.Add("HoistedTons", controlLimitsHoistedTons);
                graphValues.Add("CleanedEnds", controlLimitsCleanedEnds);
                graphValues.Add("LashedPreparedEnds", controlLimitsLashedPreparedEnds);
                graphValues.Add("EndsDrilledPlotBox", plotBox);
                graphValues.Add("First4EndsDrilledPlotBox", plotBox2);
                graphValues.Add("EndsSupportedPlotBox", plotBox3);
                graphValues.Add("SupportedEndsPlotBox", plotBox4);
                graphValues.Add("HoistedTonsPlotBox", plotBox5);

                graphValues.Add("Histogram1", histogram1);
                graphValues.Add("Histogram2", histogram2);
                graphValues.Add("Histogram3", histogram3);
                graphValues.Add("Histogram4", histogram4);
                graphValues.Add("Histogram5", histogram5);

                return Ok(graphValues);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        public double Percentile(double[] sortedData, double p)
        {
            // algo derived from Aczel pg 15 bottom
            if (p >= 100.0d) return sortedData[sortedData.Length - 1];

            double position = (sortedData.Length + 1) * p / 100.0;
            double leftNumber = 0.0d, rightNumber = 0.0d;

            double n = p / 100.0d * (sortedData.Length - 1) + 1.0d;

            if (position >= 1)
            {
                leftNumber = sortedData[(int)Math.Floor(n) - 1];
                rightNumber = sortedData[(int)Math.Floor(n)];
            }
            else
            {
                leftNumber = sortedData[0]; // first data
                rightNumber = sortedData[1]; // first data
            }

            //if (leftNumber == rightNumber)
            if (Equals(leftNumber, rightNumber))
                return leftNumber;
            double part = n - Math.Floor(n);
            return leftNumber + part * (rightNumber - leftNumber);
        } // end of internal function percentile

        public double Median(double[] array)
        {
            Array.Sort(array);

            var n = array.Length;

            double median;

            var isOdd = n % 2 != 0;
            if (isOdd)
            {
                median = array[(n + 1) / 2 - 1];
            }
            else
            {
                median = (array[n / 2 - 1] + array[n / 2]) / 2.0d;
            }

            return median;
        }
    }
}
