using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("solve")]
        public ActionResult<DiffReplyDTO> SolveDiff([FromBody] DiffRequestDTO request)
        {
            DiffReplyDTO reply = RunSolution(request);
            return Ok(reply);
        }

        public DiffReplyDTO RunSolution(DiffRequestDTO request)
        {
            DiffReplyDTO reply = new DiffReplyDTO();
            ScottPlot.Plot plt = new();
            Dictionary<string, double[]> results = new Dictionary<string, double[]>();
            foreach (var method in methods)
            {
                
            }

        }

        public double[] SolveEuler(Function f, double x0, double y0, double xn, double h, double eps)
        {
            List<double> res = new List<double> { y0 };
            double x = x0;
            double y = y0;
            while (x < xn)
            {
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
                double k1 = h * f(x, y);
                double k2 = h * f(x + h, y + k1);
                y += (k1 + k2) / 2;
                x += h;
                res.Add(y);
            }
            return res.ToArray();
        }

        public double[] SolveMilne(Function f, double x0, double y0, double xn, double h, double eps)
        {
            double[] initialPoints = SolveImprovedEuler(f, x0, y0, xn, h, eps);
            List<double> res = new List<double>(initialPoints);
            double x = x0 + 3 * h;
            for (int i = 3; i < res.Count; i++)
            {
                double prediction = res[i - 3] + 4 * h / 3 *
                    (2 * f(x - 3 * h, res[i - 3]) - f(x - 2 * h, res[i - 2]) + 2 * f(x - h, res[i - 1]));
                double correction = res[i - 1] + h / 3 * (f(x-h, res[i-1]) + 4*f(x, prediction) + f(x + h, prediction));
                res.Add(correction);
                x += h;
            }
            return res.ToArray();
        }
    }
}
