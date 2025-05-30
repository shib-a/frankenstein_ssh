using System.Collections;
using frankenstein.Server.DTOs.Diff;
using Microsoft.AspNetCore.Mvc;
using ScottPlot.Statistics;

namespace frankenstein.Server.Controllers
{
    [ApiController]
    [Route("diff")]
    public class DiffController : ControllerBase
    {
        public delegate double Function(double x, double y);

        public List<Func<Function, double, double, double, double, double, (double[], double)>> methods =
            new List<Func<Function, double, double, double, double, double, (double[], double)>>();

        public DiffController()
        {
            methods.Add(SolveEuler);
            methods.Add(SolveImprovedEuler);
            //methods.Add(SolveMilne);
        }

        [HttpPost("data")]
        public ActionResult<DiffReplyDTO> SolveDiff([FromBody] DiffRequestDTO request)
        {
            DiffReplyDTO reply = RunSolution(request);
            return Ok(reply);
        }

        public DiffReplyDTO RunSolution(DiffRequestDTO request)
        {
            ScottPlot.Plot plt = new();
            Dictionary<string, double[]> results = new Dictionary<string, double[]>();
            Function f;
            Func<double, double, double, double> exact;
            switch (request.Function)
            {
                case "LINEAR":
                    f = (x, y) => x + y;
                    exact = (x, x0, y0) => (Math.Exp(x - x0) * (y0 + x0 + 1) - x - 1);
                    break;
                case "QUADRATIC":
                    f = (x, y) => y + y * y + x * y * y;
                    exact = (x, x0, y0) =>
                        (-Math.Exp(x) / (x * Math.Exp(x) - ((x0 * Math.Exp(x0) * y0 + Math.Exp(x0)) / y0)));
                    break;
                case "EXPONENTIAL":
                    f = (x, y) => Math.Exp(x);
                    exact = (x, x0, y0) => (y0 - Math.Exp(x0) + Math.Exp(x));
                    break;
                default:
                    f = (x, y) => x + y;
                    exact = (x, x0, y0) => (Math.Exp(x - x0) * (y0 + x0 + 1) - x - 1);
                    break;
            }

            DiffReplyDTO result = new DiffReplyDTO();
            List<double[]> list = new List<double[]>();
            var resultErrors = new List<double>();
            foreach (var method in methods)
            {
                var xValues = new List<double>();
                var (res, error) = method(f, request.X0, request.Y0, request.Xn, request.H, request.Eps);
                for (int i = 0; i < res.Length; i++)
                {
                    xValues.Add(request.X0 + i * request.H);
                }

                var t = plt.Add.Scatter(xValues.ToArray(), res);
                t.LegendText = method.Method.Name;
                resultErrors.Add(error);
            }

            var mres = SolveMilne(f, request.X0, request.Y0, request.Xn, request.H, request.Eps);
            var xs = new List<double>();
            for (int i = 0; i < mres.Length; i++)
            {
                xs.Add(request.X0 + i * request.H);
            }

            var tmp = plt.Add.Scatter(xs.ToArray(), mres);
            tmp.LegendText = "SolveMilne";
            resultErrors.Add(xs.Select((x, i) => Math.Abs(mres[i] - exact(x, request.X0, request.Y0))).Max());
            //plt.Axes.SetLimits(request.X0 - request.H, request.Xn + request.H, -3, 3);
            var s = plt.Add.Scatter(xs, xs.Select((x) => exact(x, request.X0, request.Y0)).ToList());
            s.Smooth = true;
            s.LegendText = "Actual";
            plt.SavePng("results.png", 500, 500);
            var fileBytes = System.IO.File.ReadAllBytes("results.png");
            var fileBase64 = Convert.ToBase64String(fileBytes);
            result.Plot = fileBase64;
            result.Solutions = resultErrors.ToArray();
            // add error msg
            return result;
        }

        public (double[] Solution, double MaxError) SolveEuler(Function f, double x0, double y0, double xn, double h,
            double eps)
        {
            List<double> res = new List<double> { y0 };
            List<double> stepErrors = new List<double>();
            double x = x0;
            double y = y0;
            double p = 1;
            while (x < xn)
            {
                double currentH = h;
                bool stepAccepted = false;
                double error;
                do
                {
                    if (x + currentH > xn) currentH = xn - x;
                    double y1 = y + currentH * f(x, y);
                    double yHalf = y + (currentH / 2) * f(x, y);
                    double y2 = yHalf + (currentH / 2) * f(x + currentH / 2, yHalf);
                    error = Math.Abs(y1 - y2) / (Math.Pow(2, p) - 1);
                    if (error < eps)
                    {
                        y = y2;
                        x += currentH;
                        res.Add(y);
                        stepAccepted = true;
                    }
                    else
                    {
                        currentH /= 2;
                    }

                    stepErrors.Add(error);
                } while (!stepAccepted);
            }

            double maxError = stepErrors.Count > 0 ? stepErrors.Max() : 0;
            return (res.ToArray(), maxError);
        }

        public (double[] Solution, double MaxError) SolveImprovedEuler(Function f, double x0, double y0, double xn,
            double h, double eps)
        {
            List<double> res = new List<double> { y0 };
            List<double> stepErrors = new List<double>();
            double x = x0;
            double y = y0;
            double p = 2;
            while (x < xn)
            {
                double currentH = h;
                bool stepAccepted = false;
                double error = 0;
                do
                {
                    if (x + currentH > xn) currentH = xn - x;
                    double k1 = currentH * f(x, y);
                    double k2 = currentH * f(x + currentH, y + k1);
                    double y1 = y + (k1 + k2) / 2;
                    double hHalf = currentH / 2;
                    double k1Half1 = hHalf * f(x, y);
                    double k2Half1 = hHalf * f(x + hHalf, y + k1Half1);
                    double yHalf = y + (k1Half1 + k2Half1) / 2;
                    double k1Half2 = hHalf * f(x + hHalf, yHalf);
                    double k2Half2 = hHalf * f(x + currentH, yHalf + k1Half2);
                    double y2 = yHalf + (k1Half2 + k2Half2) / 2;
                    error = Math.Abs(y1 - y2) / (Math.Pow(2, p) - 1);
                    if (error < eps)
                    {
                        y = y2;
                        x += currentH;
                        res.Add(y);
                        stepAccepted = true;
                    }
                    else
                    {
                        currentH /= 2;
                    }

                    stepErrors.Add(error);

                } while (!stepAccepted);
            }

            double maxError = stepErrors.Count > 0 ? stepErrors.Max() : 0;
            return (res.ToArray(), maxError);
        }

        public double[] SolveMilne(Function f, double x0, double y0, double xn, double h, double eps)
        {
            (double[] initialPoints, var t) = SolveImprovedEuler(f, x0, y0, x0 + 3 * h, h, eps);
            if (initialPoints.Length < 4)
            {
                throw new ArgumentException("Not enough starting points");
            }

            List<double> solution = new List<double>(initialPoints);
            double x = x0 + 3 * h;
            int index = 3;

            while (x < xn)
            {
                double prediction = solution[index - 3] +
                                    4 * h / 3 * (2 * f(x - 3 * h, solution[index - 3]) -
                                                 f(x - 2 * h, solution[index - 2]) +
                                                 2 * f(x - h, solution[index - 1]));
                double correction;
                double error;
                int maxIterations = 100;
                do
                {
                    correction = solution[index - 1] +
                                 h / 3 * (f(x - h, solution[index - 1]) +
                                          4 * f(x, prediction) +
                                          f(x + h, prediction));
                    error = Math.Abs(correction - prediction);
                    prediction = correction;
                    if (--maxIterations <= 0) break;
                } while (error > eps);

                solution.Add(correction);
                x += h;
                index++;
            }

            return solution.ToArray();
        }
    }
}
