using System.Collections;
using Microsoft.AspNetCore.Mvc;
using ScottPlot.Statistics;

namespace frankenstein.Server.DTOs.Diff
{
    [ApiController]
    [Route("diff")]
    public class DiffController : ControllerBase
    {
        public delegate double Function(double x, double y);

        public List<Func<Function, double, double, double, double, double, double[]>> methods =
            new List<Func<Function, double, double, double, double, double, double[]>>();
        public DiffController()
        {
            methods.Add(SolveEuler);
            methods.Add(SolveImprovedEuler);
            methods.Add(SolveMilne);
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
            switch (request.Function)
            {
                case "LINEAR":
                    f = (x, y) => x + y;
                    break;
                case "QUADRATIC":
                    f = (x, y) => y + y * y + x * y * y;
                    break;
                case "TRIGONOMETRIC":
                    f = (x, y) => Math.Sin(x) + Math.Cos(y);
                    break;
                default:
                    f = (x, y) => x + y;
                    break;
            }
            DiffReplyDTO result = new DiffReplyDTO();
            double[][] sols = new double[3][];
            List<double[]> list = new List<double[]>();
            foreach (var method in methods)
            {
                Console.WriteLine("executing method" + method.ToString());
                var xValues = new List<double>();
                var res = method(f, request.X0, request.Y0, request.Xn, request.H, request.Eps);
                for (int i = 0; i < res.Length; i++)
                {
                    xValues.Add(request.X0 + i * request.H);
                }

                list.Add(res);
                var t = plt.Add.Scatter(xValues.ToArray(), res);
                t.LegendText = method.Method.Name;
            }
            plt.Axes.SetLimits(request.X0 - request.H, request.Xn + request.H, -3, 3);
            result.Solution = list.ToArray();
            plt.SavePng("results.png", 500, 500);
            var fileBytes = System.IO.File.ReadAllBytes("results.png");
            var fileBase64 = Convert.ToBase64String(fileBytes);
            result.Plot = fileBase64;
            // add error msg
            return result;
        }

        public double[] SolveEuler(Function f, double x0, double y0, double xn, double h, double eps)
        {
            List<double> res = new List<double> { y0 };
            double x = x0;
            double y = y0;
            
            while (x < xn)
            {
                double addedH = h;
                double error;
                do
                {
                    double y1 = y + addedH * f(x, y);
                    double y2 = (y + (addedH / 2) * f(x, y)) + (addedH / 2) *
                        f(x + addedH / 2, y + (addedH / 2) * f(x, y));
                    error = Math.Abs(y2 - y1);
                    addedH /= 2;
                } while (error > eps);
                y += h * f(x, y);
                x += h;
                res.Add(y);
            }
            return res.ToArray();
        }

        public double[] SolveImprovedEuler(Function f, double x0, double y0, double xn, double h, double eps)
        {
            List<double> res = new List<double> { y0 };
            double x = x0;
            double y = y0;
            while (x < xn)
            {
                double addedH = h;
                double error;
                do
                {
                    double k1 = addedH * f(x, y);
                    double k2 = addedH * f(x + addedH, y + k1);
                    double y1 = y + (k1 + k2) / 2;

                    double k1Half = addedH / 2 * f(x, y);
                    double k2Half = addedH / 2 * f(x + addedH / 2, y + k1Half);
                    double y2 = y + (k1Half + k2Half) / 2;
                    error = Math.Abs(y2 - y1);
                    addedH /= 2;
                } while(error > eps);

                y += (h * f(x,y) + h * f(x + h, y + h * f(x, y))) / 2;
                x += h;
                res.Add(y);
            }
            return res.ToArray();
        }

        public double[] SolveMilne(Function f, double x0, double y0, double xn, double h, double eps)
        {
            //double[] initialPoints = SolveImprovedEuler(f, x0, y0, x0 + 3 * h, h, eps);
            double[] initialPoints = SolveRungeKutta4(f, x0, y0, x0 + 3 * h, h);
            if (initialPoints.Length < 4)
            {
                throw new ArgumentException("Недостаточно начальных точек.");
            }

            List<double> res = new List<double>(initialPoints);
            double x = x0 + 3 * h;
            int i = 3;

            while (x + h <= xn)
            {
                double addedH = h;
                double prediction;
                double correction;
                double error;

                do
                {
                    prediction = res[i - 3] + (4 * addedH / 3) *
                        (2 * f(x - 3 * addedH, res[i - 3])
                         - f(x - 2 * addedH, res[i - 2])
                         + 2 * f(x - addedH, res[i - 1]));

                    correction = res[i - 1] + addedH / 3 *
                        (f(x - addedH, res[i - 1])
                         + 4 * f(x, prediction)
                         + f(x + addedH, prediction));

                    error = Math.Abs(correction - prediction);
                    if (error > eps)
                    {
                        addedH /= 2;
                    }
                } while (error > eps);

                res.Add(correction);
                x += addedH;
                i++;
            }
            return res.ToArray();
        }
        public double[] SolveRungeKutta4(Function f, double x0, double y0, double xn, double h)
        {
            List<double> res = new List<double> { y0 };
            double x = x0;
            double y = y0;
            while (x < xn)
            {
                double k1 = h * f(x, y);
                double k2 = h * f(x + h / 2, y + k1 / 2);
                double k3 = h * f(x + h / 2, y + k2 / 2);
                double k4 = h * f(x + h, y + k3);
                y += (k1 + 2 * k2 + 2 * k3 + k4) / 6;
                x += h;
                res.Add(y);
            }
            return res.ToArray();
        }
    }
}
