using Microsoft.AspNetCore.Mvc;
using frankenstein.Server.DTOs.Approximation;
using frankenstein.Server.DTOs.Interpolation;
using ScottPlot;

namespace frankenstein.Server.Controllers
{
    [ApiController]
    [Route("interpolation")]
    public class InterpolationController : ControllerBase
    {

        public delegate double Function(double x);

        public InterpolationController(MatrixService service)
        {
            interpolations.Add(NewtonInterpolation);
            interpolations.Add(LagrangeInterpolation);
            //interpolations.Add(StirlingInterpolation);
            //interpolations.Add(BesselInterpolation);
        }
        public List<Func<double[], double[], List<List<double>>, double, double>> interpolations =
            new List<Func<double[], double[], List<List<double>>, double, double>>();
        
        [HttpPost("data")]
        public ActionResult<InterpolationReplyDTO> SolveApproximation([FromBody] InterpolationRequestDTO requestDTO)
        {
            InterpolationReplyDTO result = RunInterpolation(requestDTO);
            return Ok(result);
        }

        [HttpPost("file")]
        public async Task<ActionResult<ApproximationReplyDTO>> HandleFileUpload(IFormFile file)
        {
            ApproximationReplyDTO result = new ApproximationReplyDTO();
            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                if (!reader.EndOfStream)
                {
                    try
                    {
                        var xLine = await reader.ReadLineAsync();
                        var splitXLine = xLine.Split(",");
                        var yLine = await reader.ReadLineAsync();
                        var splitYLine = yLine.Split(",");
                        if (splitXLine.Length != splitYLine.Length)
                        {
                            result.ErrorMessage = "INVALID_FILE_DATA";
                            return result;
                        }

                        double[] xValues = new double[splitXLine.Length];
                        double[] yValues = new double[splitYLine.Length];
                        for (int i = 0; i < splitXLine.Length; i++)
                        {
                            xValues[i] = double.Parse(splitXLine[i], System.Globalization.CultureInfo.InvariantCulture);
                            yValues[i] = double.Parse(splitYLine[i], System.Globalization.CultureInfo.InvariantCulture);
                        }

                        var request = new InterpolationRequestDTO();
                        request.XValues = xValues;
                        request.YValues = yValues;
                        var res = RunInterpolation(request);
                        return Ok(res);
                    }
                    catch (Exception e)
                    {
                        result.ErrorMessage = "INVALID_FILE";
                    }
                }
            }

            return result;
        }
                
        public InterpolationReplyDTO RunInterpolation(InterpolationRequestDTO request)
        {
            ScottPlot.Plot plt = new();
            var yValues = request.YValues;
            var xValues = request.XValues;
            var vals = new[] { xValues, yValues };
            var sortedXValues = new double[xValues.Length];
            var sortedYValues = new double[yValues.Length];
            double[][] pairs = new double[xValues.Length][];
            for (int i = 0; i < xValues.Length; i++)
            {
                pairs[i] = new[] { xValues[i], yValues[i] };
            }

            Array.Sort(pairs, (x, y) =>
            {
                return x[0].CompareTo(y[0]);
            });
            for (int i = 0; i < yValues.Length; i++)
            {
                sortedXValues[i] = pairs[i][0];
                sortedYValues[i] = pairs[i][1];
            }
            
            xValues = sortedXValues;
            yValues = sortedYValues;

            plt.Add.ScatterPoints(xValues, yValues, color: Color.FromColor(System.Drawing.Color.Purple));
            var s = plt.Add.Scatter(xValues, yValues);
            s.Smooth = true;
            
            var reply = new InterpolationReplyDTO();
            Dictionary<string,double> results = new Dictionary<string, double>();
            var sols = new List<double>();
            foreach (var interpolation in interpolations)
            {
                var result = interpolation(xValues, yValues, BuildDifferenceTable(yValues), request.TargetX);
                results.Add(interpolation.Method.Name, result);
                //var plot_y = xValues.Select(x => result.Function(x)).ToArray();
                var t = plt.Add.Scatter(request.TargetX, result);
                t.LegendText = interpolation.Method.Name;
                sols.Add(result);
            }
            plt.SavePng("results.png", 500, 500);
            var fileBytes = System.IO.File.ReadAllBytes("results.png");
            var fileBase64 = Convert.ToBase64String(fileBytes);
            reply.Plot = fileBase64;
            reply.Solutions = sols.ToArray();
            return reply;
        }
        
        
        private List<List<double>> BuildDifferenceTable(double[] yValues)
        {
            int n = yValues.Length;
            var table = new List<List<double>>() { new List<double>(yValues) };
            for (int i = 1; i < n; i++)
            {
                var newRow = new List<double>();
                for (int j = 0; j < n - i; j++)
                    newRow.Add(table[i - 1][j + 1] - table[i - 1][j]);
                table.Add(newRow);
            }
            return table;
        }
        private double NewtonInterpolation(double[] x, double[] y, List<List<double>> diffTable, double targetX)
        {
            int n = x.Length;
            double h = x[1] - x[0];
            double t = (targetX - x[0]) / h;
            double result = diffTable[0][0];
            double term = 1;

            for (int i = 1; i < diffTable.Count; i++)
            {
                term *= (t - i + 1) / i;
                result += term * diffTable[i][0];
            }
            return result;
        }
        
        private double LagrangeInterpolation(double[] x, double[] y, List<List<double>> diffTable, double targetX)
        {
            int n = x.Length;
            double result = 0;
            for (int i = 0; i < n; i++)
            {
                double term = y[i];
                for (int j = 0; j < n; j++)
                {
                    if (j != i)
                        term *= (targetX - x[j]) / (x[i] - x[j]);
                }
                result += term;
            }
            return result;
        }

        private double StirlingInterpolation(double[] x, double[] y, List<List<double>> diffTable, double targetX)
        {
            int n = x.Length;
            int centerIndex = n / 2 - 1;
            double h = x[1] - x[0];
            double t = (targetX - x[centerIndex]) / h;
            double result = diffTable[0][centerIndex];
            double term = 1;
            double factorial = 1;

            for (int i = 1; i < diffTable.Count; i++)
            {
                factorial *= i;
                int shift = (i + 1) / 2;
                double delta = diffTable[i][centerIndex - shift];

                if (i % 2 == 1)
                {
                    term *= (t * t - (shift - 1) * (shift - 1)) / (2 * i - 1);
                    result += term * delta / factorial;
                }
                else
                {
                    term *= t / i;
                    result += term * delta / factorial;
                }
            }
            return result;
        }

        private double BesselInterpolation(double[] x, double[] y, List<List<double>> diffTable, double targetX)
        {
            int n = x.Length;
            int centerIndex = n / 2 - 1;
            double h = x[1] - x[0];
            double t = (targetX - x[centerIndex]) / h - 0.5;
            double result = (diffTable[0][centerIndex] + diffTable[0][centerIndex + 1]) / 2;
            double term = t;
            double factorial = 1;

            for (int i = 1; i < diffTable.Count; i++)
            {
                factorial *= i;
                int shift = i / 2;
                double delta = diffTable[i][centerIndex - shift];

                if (i % 2 == 0)
                {
                    term *= (t * t - (shift) * (shift)) / (2 * i);
                    result += term * delta / factorial;
                }
                else
                {
                    term *= t / i;
                    result += term * delta / factorial;
                }
            }
            return result;
        }

    }
}
