using Microsoft.AspNetCore.Mvc;
using frankenstein.Server.DTOs.Approximation;
using ScottPlot;
using frankenstein.Server.DTOs.Matrix;
using Microsoft.AspNetCore.Cors;

namespace frankenstein.Server.Controllers
{
    [ApiController]
    [Route("approximation")]
    public class ApproximationController : ControllerBase
    {
        public class ApproximationResult
        {
            public Function Function { get; set; }
            public double[] Coefficients { get; set; }
        }

        public class ApproximationData
        {
            public ApproximationResult Result { get; set; }
            public double Deviation { get; set; }
            public double MSE { get; set; }
            public double R2 { get; set; }
        }
        public delegate double Function(double x);
        private readonly MatrixService _matrixService;

        public ApproximationController(MatrixService service)
        {
            approximations.Add(LinearApproximation);
            approximations.Add(QuadraticApproximation);
            approximations.Add(CubicApproximation);
            approximations.Add(ExponentialApproximation);
            approximations.Add(LogarithmicApproximation);
            approximations.Add(PowerApproximation);
            _matrixService = service;
        }
        public List<Func<double[], double[], ApproximationResult>> approximations =
            new List<Func<double[], double[], ApproximationResult>>();
        
        [HttpPost("data")]
        public ActionResult<ApproximationReplyDTO> SolveApproximation([FromBody] ApproximationRequestDTO requestDTO)
        {
            ApproximationReplyDTO result = RunApproximation(requestDTO);
            return Ok(result);
        }
        public ApproximationReplyDTO RunApproximation(ApproximationRequestDTO request)
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
            var xDelta = Math.Abs((xValues.Max() - xValues.Min()) / xValues.Length);
            var yDelta = Math.Abs((yValues.Max() - yValues.Min()) / yValues.Length);
            plt.Axes.SetLimits(xValues.Min() - xDelta, xValues.Max() + xDelta, yValues.Min() - yDelta, yValues.Max() + yDelta);
            Dictionary<string, ApproximationData> results = new Dictionary<string, ApproximationData>();
            foreach (var approximation in approximations)
            {
                var result = approximation(yValues, xValues);
                var deviation = CalculateDeviation(yValues, xValues, result.Function);
                var mse = CalculateMSE(yValues, xValues, result.Function);
                var r2 = CalculateDeterminationCoefficient(yValues, xValues, result.Function);
                ApproximationData data = new ApproximationData();
                data.Result = result;
                data.Deviation = deviation;
                data.MSE = mse;
                data.R2 = r2;
                results.Add(approximation.Method.Name, data);
                var plot_y = xValues.Select(x => result.Function(x)).ToArray();
                var t = plt.Add.Scatter(xValues, plot_y);
                t.LegendText = approximation.Method.Name;
            }
            var bestApproximation = results.OrderByDescending(x => x.Value.R2).First();
            var reply = new ApproximationReplyDTO();
            reply.Function = bestApproximation.Key;
            reply.Coefficients = bestApproximation.Value.Result.Coefficients;
            reply.Deviation = bestApproximation.Value.Deviation;
            reply.MSE = bestApproximation.Value.MSE;
            reply.R2 = bestApproximation.Value.R2;
            plt.SavePng("results.png", 500, 500);
            var fileBytes = System.IO.File.ReadAllBytes("results.png");
            var fileBase64 = Convert.ToBase64String(fileBytes);
            reply.Plot = fileBase64;
            if (Double.IsNaN(reply.Deviation) || Double.IsNaN(reply.R2) || Double.IsNaN(reply.MSE))
            {
                reply.ErrorMessage = "Cannot count statistics because input data is invalid";
            }
            return reply;
        }
        [HttpPost("file")]
        public async Task<ActionResult<ApproximationReplyDTO>> HandleFileUpload(IFormFile file)
        {
            ApproximationReplyDTO result = new ApproximationReplyDTO();
            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                if(!reader.EndOfStream)
                {
                    try
                    {
                        var xLine = await reader.ReadLineAsync();
                        var splitXLine = xLine.Split(",");
                        var yLine = await reader.ReadLineAsync();
                        var splitYLine = yLine.Split(",");
                        if(splitXLine.Length != splitYLine.Length)
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

                        var request = new ApproximationRequestDTO();
                        request.XValues = xValues;
                        request.YValues = yValues;
                        var res = RunApproximation(request);
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

        private double CalculateDeviation(double[] yValues, double[] xValues, Function function)
        {
            double deviation = 0;
            for (int i = 0; i < yValues.Length; i++)
            {
                deviation += Math.Pow(function(xValues[i]) - yValues[i], 2);
            }
            return deviation;
        }

        private ApproximationResult LinearApproximation(double[] yValues, double[] xValues)
        {
            double sx = xValues.Sum();
            double sxx = xValues.Sum(x => x * x);
            double sy = yValues.Sum();
            double sxy = 0;
            for (int i = 0; i < xValues.Length; i++)
            {
                sxy += xValues[i] * yValues[i];
            }

            var request = new MatrixRequestDTO();
            var coefs = new double[2][];
            coefs[0] = new[] {sxx, sx};
            coefs[1] = new[] { sx, xValues.Length };
            var vals = new double[2];
            vals[0] = sxy;
            vals[1] = sy;
            request.Coefficients = coefs;
            request.Values = vals;
            request.Precision = 0.0001;
            var res = _matrixService.SolveEquations(request);
            var ret = new ApproximationResult();
            try
            {
                ret.Coefficients = res.Solution;
                ret.Function = x => res.Solution[0] * x + res.Solution[1];
                return ret;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private ApproximationResult QuadraticApproximation(double[] yValues, double[] xValues)
        {
            double sx = xValues.Sum();
            double sxx = xValues.Sum(x => x * x);
            double sxxx = xValues.Sum(x => x * x * x);
            double sxxxx = xValues.Sum(x => x * x * x * x);
            double sy = yValues.Sum();
            double sxy = 0;
            for (int i = 0; i < xValues.Length; i++)
            {
                sxy += xValues[i] * yValues[i];
            }
            double sxxy = 0;
            for (int i = 0; i < xValues.Length; i++)
            {
                sxxy += xValues[i] * xValues[i] * yValues[i];
            }

            var request = new MatrixRequestDTO();
            var coefs = new double[3][];
            coefs[0] = new[] { xValues.Length, sx, sxx};
            coefs[1] = new[] { sx, sxx, sxxx };
            coefs[2] = new[] { sxx, sxxx, sxxxx };
            var vals = new double[3];
            vals[0] = sy;
            vals[1] = sxy;
            vals[2] = sxxy;
            request.Coefficients = coefs;
            request.Values = vals;
            request.Precision = 0.0001;
            var res = _matrixService.SolveEquations(request);
            var ret = new ApproximationResult();
            ret.Coefficients = res.Solution;
            ret.Function = x => res.Solution[0] + res.Solution[1] * x + res.Solution[2] * Math.Pow(x, 2);
            return ret;
        }

        private ApproximationResult CubicApproximation(double[] yValues, double[] xValues)
        {
            double sx = xValues.Sum();
            double sxx = xValues.Sum(x => x * x);
            double sxxx = xValues.Sum(x => x * x * x);
            double sxxxx = xValues.Sum(x => x * x * x * x);
            double sxxxxx = xValues.Sum(x => x * x * x * x * x);
            double sxxxxxx = xValues.Sum(x => x * x * x * x * x * x);
            double sy = yValues.Sum();
            double sxy = 0;
            for (int i = 0; i < xValues.Length; i++)
            {
                sxy += xValues[i] * yValues[i];
            }
            double sxxy = 0;
            for (int i = 0; i < xValues.Length; i++)
            {
                sxxy += xValues[i] * xValues[i] * yValues[i];
            }
            double sxxxy = 0;
            for (int i = 0; i < xValues.Length; i++)
            {
                sxxxy += Math.Pow(xValues[i], 3) * yValues[i];
            }
            var request = new MatrixRequestDTO();
            var coefs = new double[4][];
            coefs[0] = new[] { xValues.Length, sx, sxx, sxxx };
            coefs[1] = new[] { sx, sxx, sxxx, sxxxx };
            coefs[2] = new[] { sxx, sxxx, sxxxx, sxxxxx };
            coefs[3] = new[] { sxxx, sxxxx, sxxxxx, sxxxxxx };
            var vals = new double[4];
            vals[0] = sy;
            vals[1] = sxy;
            vals[2] = sxxy;
            vals[3] = sxxxy;
            request.Coefficients = coefs;
            request.Values = vals;
            request.Precision = 0.0001;
            var res = _matrixService.SolveEquations(request);
            var ret = new ApproximationResult();
            ret.Coefficients = res.Solution;
            ret.Function = x => res.Solution[0] + res.Solution[1] * x + res.Solution[2] * Math.Pow(x, 2) + res.Solution[3] * Math.Pow(x, 3);
            return ret;
        }
        private ApproximationResult ExponentialApproximation(double[] yValues, double[] xValues)
        {
            double[] logYValues = yValues.Select(y => Math.Log(y)).ToArray();
            var res = LinearApproximation(logYValues, xValues);
            var a = Math.Exp(res.Coefficients.Max());
            var b = res.Coefficients.Min();
            var ret = new ApproximationResult();
            ret.Coefficients = new double[2];
            ret.Coefficients[0] = a;
            ret.Coefficients[1] = b;
            ret.Function = x => a * Math.Exp(b * x);
            return ret;
        }
        private ApproximationResult LogarithmicApproximation(double[] yValues, double[] xValues)
        {
            double[] logXValues = xValues.Select(x => Math.Log(x)).ToArray();
            var res = LinearApproximation(yValues, logXValues);
            var a = res.Coefficients[0];
            var b = res.Coefficients[1];
            var ret = new ApproximationResult();
            ret.Coefficients = new double[2];
            ret.Coefficients[0] = a;
            ret.Coefficients[1] = b;
            ret.Function = x => a + b * Math.Log(x);
            return ret;
        }

        private ApproximationResult PowerApproximation(double[] yValues, double[] xValues)
        {
            double[] logXValues = xValues.Select(x => Math.Log(x)).ToArray();
            double[] logYValues = yValues.Select(y => Math.Log(y)).ToArray();
            var res = LinearApproximation(logYValues, logXValues);
            var a = Math.Exp(res.Coefficients[1]);
            var b = res.Coefficients[0];
            var ret = new ApproximationResult();
            ret.Coefficients = new double[2];
            ret.Coefficients[0] = a;
            ret.Coefficients[1] = b;
            ret.Function = x => a * Math.Pow(x, b);
            return ret;
        }

        private double CalculateMSE(double[] yValues, double[] xValues, Function f)
        {
            var ans_arr = new double[xValues.Length];
            for (int i=0; i < yValues.Length; i++)
            {
               ans_arr[i] = Math.Pow(f(xValues[i]) - yValues[i], 2);
            }

            return Math.Sqrt(ans_arr.Sum() / xValues.Length);
        }

        private double CalculateDeterminationCoefficient(double[] yValues, double[] xValues, Function f)
        {
            var phi = xValues.Select(x => f(x)).ToArray();
            var phiAvg = phi.Sum()/xValues.Length;
            double yAvg = 0;
            for (int i = 0; i < xValues.Length; i++)
            {
                yAvg += Math.Pow((yValues[i] - phi[i]),2);
            }

            return 1 - (yAvg / yValues.Select(y => Math.Pow(y - phiAvg, 2)).Sum());
        }
    }
}
