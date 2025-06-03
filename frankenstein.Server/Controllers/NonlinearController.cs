using frankenstein.Server.DTOs.Nonlinear;
using Microsoft.AspNetCore.Mvc;
using ScottPlot;

namespace frankenstein.Server.Controllers
{
    [ApiController]
    [Route("nonlinear")]
    public class NonlinearController : ControllerBase
    {
        [HttpPost("data")]
        public ActionResult<NonlinearReplyDTO> SolveApproximation([FromBody] NonlinearRequestDTO requestDTO)
        {
            NonlinearReplyDTO result = RunSolution(requestDTO);
            return Ok(result);
        }
        private NonlinearReplyDTO RunSolution(NonlinearRequestDTO requestDTO)
        {
            if (requestDTO.Type == "EQUATION")
            {
                Func<double, double> f = GetFunction(requestDTO.FunctionInfo);
                double a = requestDTO.LowerBoundary;
                double b = requestDTO.UpperBoundary;
                double eps = requestDTO.Precision;
                double root = 0;
                int iterations = 0;
                double funcValue = 0;
                bool convergence;
                string plot;
                switch (requestDTO.Method)
                {
                    case "CHORD":
                    {
                        root = ChordMethod(f, a, b, eps, out iterations);
                        break;
                    }
                    case "NEWTON":
                    {
                        root = NewtonMethod(f, SelectNewtonInitialGuess(f, a, b), eps, out iterations);
                        break;
                    }
                    case "SIMPLE":
                    {
                        var phi = GetIterationFunction(requestDTO.FunctionInfo);
                        root = SimpleIterationMethod(f, phi, a, b, eps, out iterations, out convergence);
                        break;
                    }
                }

                if (root != null)
                {
                    funcValue = f(root);

                }

                plot = GenerateFunctionPlot(f, a, b);
                var result = new NonlinearReplyDTO();
                result.Root = root;
                result.IterationCount = iterations;
                result.Plot = plot;
                return result;
            }
            else
            {
                var systems = new Dictionary<int, (Func<double, double, double> f1,
                    Func<double, double, double> f2,
                    Func<double, double, double> phi1,
                    Func<double, double, double> phi2)>
                {
                    [1] = ( // Новая система
                            (x, y) => Math.Sin(x + y) - 1.5 * x + 0.1, // sin(x+y) = 1.5x - 0.1
                            (x, y) => x * x + 2 * y * y - 1,           // x^2 + 2y^2 = 1
                            (x, y) => (Math.Sin(x + y) + 0.1) / 1.5,   // x = (sin(x+y) + 0.1)/1.5
                            (x, y) => Math.Sqrt((1 - x * x) / 2)        // y = sqrt((1 - x^2)/2)
                        )
                };
                var (f1, f2, phi1, phi2) = systems[1];
                double[] solution;
                double[] errors;
                double eps = requestDTO.Precision;
                double x = requestDTO.X0;
                double y = requestDTO.Y0;
                int iterations = 0;
                double errX; double errY;
                do
                {
                    double xNew = phi1(x, y);
                    double yNew = phi2(x, y);

                    errX = Math.Abs(xNew - x);
                    errY = Math.Abs(yNew - y);

                    x = xNew;
                    y = yNew;
                    iterations++;
                }
                while (Math.Max(errX, errY) > eps);
                solution = new double[] { x, y };
                var result = new NonlinearReplyDTO();
                result.Solutions = solution;
                result.IterationCount = iterations;
                result.Plot = GenerateSystemPlot(f1, f2, solution[0] - 2, solution[0] + 2,
                    solution[1] - 2, solution[1] + 2);
                return result;
            }
        }
        private Func<double, double> GetFunction(string functionType)
        {
            return functionType switch
            {
                "ONE" => x => -1.8 * x * x * x - 2.94 * x * 2 + 10.37 * x + 5.38,
                "TWO" => x => x * x * x - x + 4,
                "THREE" => x => Math.Atan(x),
                _ => x => x * x * x - x + 4,
            };
        }
        private Func<double, double> GetIterationFunction(string functionType)
        {
            return functionType switch
            {
                "ONE" => x =>
                {
                    double numerator = -2.94 * x * x + 10.37 * x + 5.38;
                    double value = numerator / 1.8;
                    return Math.Sign(value) * Math.Pow(Math.Abs(value), 1.0 / 3.0);
                },
                "TWO" => x => Math.Pow(x - 4, 1.0 / 3.0),
                "THREE" => x => x - Math.Atan(x),
                _ => x => Math.Pow(x - 4, 1.0 / 3.0)
            };
        }

        private double ChordMethod(Func<double, double> f, double a, double b, double eps, out int iterations)
        {
            iterations = 0;
            double c = 0;
            while (Math.Abs(b - a) > eps)
            {
                c = (a * f(b) - b * f(a)) / (f(b) - f(a));
                if (f(a) * f(c) < 0)
                    b = c;
                else
                    a = c;
                iterations++;
            }
            return c;
        }
        private double NewtonMethod(Func<double, double> f, double x0, double eps, out int iterations)
        {
            iterations = 0;
            double x = x0;
            double h = 1e-5;
            while (true)
            {
                double df = (f(x + h) - f(x - h)) / (2 * h);
                double xNew = x - f(x) / df;
                if (Math.Abs(xNew - x) < eps) break;
                x = xNew;
                iterations++;
            }
            return x;
        }
        private double SimpleIterationMethod(Func<double, double> f, Func<double, double> phi,
            double a, double b, double eps, out int iterations, out bool convergence)
        {
            convergence = CheckConvergence(phi, a, b);
            iterations = 0;
            double x = (a + b) / 2;
            while (true)
            {
                double xNew = phi(x);
                if (Math.Abs(xNew - x) < eps) break;
                x = xNew;
                iterations++;
            }
            return x;
        }
        private bool CheckConvergence(Func<double, double> phi, double a, double b)
        {
            // Проверка производной на интервале
            double h = (b - a) / 100;
            double maxDerivative = 0;
            for (double x = a; x <= b; x += h)
            {
                double derivative = Math.Abs((phi(x + h) - phi(x - h)) / (2 * h));
                if (derivative > maxDerivative)
                    maxDerivative = derivative;
            }
            return maxDerivative < 1;
        }
        private double SelectNewtonInitialGuess(Func<double, double> f, double a, double b)
        {
            // Простая эвристика: выбираем конец, где функция ближе к нулю
            return Math.Abs(f(a)) < Math.Abs(f(b)) ? a : b;
        }

        private string GenerateFunctionPlot(Func<double, double> f, double a, double b)
        {
            // Создаем график
            Plot plt = new();
            // Генерируем данные
            double[] xs = Enumerable.Range(0, 1001)
                .Select(i => a + (b - a) * i / 1000.0)
                .ToArray();
            double[] ys = xs.Select(x => f(x)).ToArray();

            // Добавляем график функции
            var t = plt.Add.Scatter(xs, ys);
            // Настраиваем оформление
            plt.Title($"График функции на интервале [{a}, {b}]");
            plt.XLabel("x");
            plt.YLabel("f(x)");
            plt.Add.Legend();

            // Сохраняем в файл
            plt.SavePng("results.png", 600, 600);
            var fileBytes = System.IO.File.ReadAllBytes("results.png");
            var fileBase64 = Convert.ToBase64String(fileBytes);
            return fileBase64;
        }

        private bool CheckSystemConvergence(
            Func<double, double, double> phi1,
            Func<double, double, double> phi2,
            double x0, double y0)
        {
            // Проверка частных производных
            double h = 1e-5;
            double dphi1dx = (phi1(x0 + h, y0) - phi1(x0 - h, y0)) / (2 * h);
            double dphi1dy = (phi1(x0, y0 + h) - phi1(x0, y0 - h)) / (2 * h);
            double dphi2dx = (phi2(x0 + h, y0) - phi2(x0 - h, y0)) / (2 * h);
            double dphi2dy = (phi2(x0, y0 + h) - phi2(x0, y0 - h)) / (2 * h);

            // Норма матрицы Якоби (максимальная сумма модулей)
            double norm = Math.Max(
                Math.Abs(dphi1dx) + Math.Abs(dphi1dy),
                Math.Abs(dphi2dx) + Math.Abs(dphi2dy));

            return norm < 1;
        }
        private string GenerateSystemPlot(
            Func<double, double, double> f1,
            Func<double, double, double> f2,
            double xMin, double xMax,
            double yMin, double yMax)
        {
            // Создаем график
            Plot plt = new();

            // Генерируем сетку точек
            int resolution = 100;
            double[] xs = Enumerable.Range(0, resolution)
                .Select(i => xMin + (xMax - xMin) * i / (double)(resolution - 1))
                .ToArray();
            double[] ys = Enumerable.Range(0, resolution)
                .Select(i => yMin + (yMax - yMin) * i / (double)(resolution - 1))
                .ToArray();

            // Вычисляем значения функций
            double[,] z1 = new double[resolution, resolution];
            double[,] z2 = new double[resolution, resolution];

            for (int i = 0; i < resolution; i++)
            {
                for (int j = 0; j < resolution; j++)
                {
                    z1[j, i] = f1(xs[i], ys[j]);
                    z2[j, i] = f2(xs[i], ys[j]);
                }
            }

            // Добавляем контурные графики для уравнений
            var contour1 = plt.Add.ContourLines(xs, ys, z1, levels: new double[] { 0 });
            contour1.LineStyle = LineStyle.Solid;
            contour1.LineWidth = 2;
            contour1.Label = "Уравнение 1";

            var contour2 = plt.AddContour(xs, ys, z2, levels: new double[] { 0 });
            contour2.LineStyle = LineStyle.Dash;
            contour2.LineWidth = 2;
            contour2.Label = "Уравнение 2";

            // Настраиваем оформление
            plt.Title("График системы уравнений");
            plt.XLabel("x");
            plt.YLabel("y");
            plt.Add.Legend();
            plt.Axes.SetLimits(xMin, xMax, yMin, yMax);

            // Сохраняем в файл
            plt.SavePng("results.png", 600, 600);
        }
    }
}
