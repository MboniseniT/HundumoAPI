using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinmakAPI.Data;
using BinmakBackEnd.Areas.Kwenza.Entities;
using BinmakBackEnd.Areas.Kwenza.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace BinmakBackEnd.Areas.Kwenza.Controllers
{
    [EnableCors("CorsPolicy")]
    [Area("Kwenza")]
    [ApiController]
    [Route("Kwenza/[controller]")]
    public class ProductionFlowAnalyticsController : Controller
    {
        private readonly BinmakDbContext _context;

        public ProductionFlowAnalyticsController(BinmakDbContext context)
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

      
        [HttpPost("")]
        public IActionResult GenerateChart(ChartModel model)
        {
            if (model == null)
            {
                return BadRequest("Make sure fields are completed correctly.");
            }
            try
            {
                List<KeyProcessArea> keyProcessAreas = _context.KeyProcessAreas.Where(id => id.AssetNodeId == model.AssetId).ToList();
                List<AnalyticsConstructor> analyticsConstructors = new List<AnalyticsConstructor>();

                foreach (var kpa in keyProcessAreas)
                {
                    AnalyticsConstructor analyticsConstructor = new AnalyticsConstructor();

                    List<Production> productions = _context.Productions.Where(id =>
                    (id.KeyProcessAreaId == kpa.KeyProcessAreaId) && ((id.Year >= model.StartDate.Year) && (id.Month >= model.StartDate.Month) && (id.Day >= model.StartDate.Day))
                    && ((id.Year <= model.EndDate.Year) && (id.Month <= model.EndDate.Month) && (id.Day <= model.EndDate.Day))).ToList();

                    analyticsConstructor.KPAName = kpa.KeyProcessAreaName;
                    analyticsConstructor.Productions = productions;
                    analyticsConstructor.StartDate = model.StartDate;
                    analyticsConstructor.EndDate = model.EndDate;

                    analyticsConstructors.Add(analyticsConstructor);
                }

                List<MasterChart> masterCharts = GenerateChart(analyticsConstructors);

                return Ok(masterCharts);
            }
            catch (Exception Ex)
            {
                return BadRequest("Something bad happened. " + Ex.Message);
            }
        }

        public List<MasterChart> GenerateChart(List<AnalyticsConstructor> analyticsConstructors)
        {
            List<MasterChart> masterCharts = new List<MasterChart>();
            List<Production> productions = new List<Production>();
            

            foreach (var ac in analyticsConstructors)
            {
                List<ControlLimit> controlLimits = new List<ControlLimit>();

                MasterChart masterChart = new MasterChart();
                masterChart.KPAId = ac.KPAId;
                masterChart.KPAName = ac.KPAName;
                masterChart.StartDate = ac.StartDate;
                masterChart.EndDate = ac.EndDate;

                List<int> values = new List<int>();

                foreach (var p in ac.Productions)
                {
                    values.Add(p.Value);
                }

                int mean = 0;

                if (values.Count > 0)
                {
                    mean = values.Sum() / values.Count;
                }

                double sumOfSquaresOfDifferences = values.Select(val => (val - mean) * (val - mean)).Sum();
                double standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / values.Count);

                List<string> dates = new List<string>();
                List<int> measurements = new List<int>();

                ControlLimit controlLimit = new ControlLimit();
                controlLimit.lcl = mean - (3 * standardDeviation);
                controlLimit.ucl = mean + (3 * standardDeviation);
                controlLimit.mean = mean;

                var orderedProds = ac.Productions.OrderBy(id => id.Day);

                foreach (var p in orderedProds)
                {
                    dates.Add(new DateTime(p.Year, p.Month, p.Day).ToString("yyyy-MM-dd"));
                    measurements.Add(p.Value);   
                }
                controlLimit.dates = dates;
                controlLimit.measurements = measurements;
                masterChart.ControlLimit = controlLimit;
                masterCharts.Add(masterChart);
            }

            return masterCharts;
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
