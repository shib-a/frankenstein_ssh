using frankenstein.Server.DTOs.Integral;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace frankenstein.Server.Controllers
{
    [Route("integral")]
    [ApiController]
    public class IntegralController : ControllerBase
    {
        delegate double Function(double x);
        [HttpPost("data")]
        public ActionResult<IntegralReplyDTO> SolveIntegral([FromBody] IntegralRequestDTO request)
        {
            var result = RunIntegration(request);
            if (result == null)
            {
                return BadRequest("Invalid input data.");
            }
            return Ok(result);
        }

        private Function GetFunction(string functionType)
        {
            switch(functionType)
            {
                case "LINEAR": return x => x;
                case "EXPONENTIAL": return x => Math.Exp(x);
                case "LOGARITHMIC": return x => Math.Log(x);
                case "SINE": return x => Math.Sin(x);
                case "POLYNOM": return x => 2 * Math.Pow(x, 3) - 3 * Math.Pow(x, 2) + 5 * x - 9;
                default: return x => 1;
            }
        }

        private IntegralReplyDTO RunIntegration(IntegralRequestDTO requestDTO)
        {
            Function function = GetFunction(requestDTO.FunctionInfo[0]);
            try
            {
                double lowerBoundary = requestDTO.LowerBoundary;
                double upperBoundary = requestDTO.UpperBoundary;
                double precision = requestDTO.Precision;
                int n = 4;
                IntegralReplyDTO result = new IntegralReplyDTO();
                switch (requestDTO.Method)
                {
                    case "LEFT_RECT":  CalculateLeftRectangle(function, lowerBoundary, upperBoundary, precision, ref n, ref result); break;
                    case "RIGHT_RECT": CalculateRightRectangle(function, lowerBoundary, upperBoundary, precision, ref n, ref result); break;
                    case "MID_RECT": CalculateMidRectangle(function, lowerBoundary, upperBoundary, precision, ref n, ref result); break;
                    case "TRAPEZOID": CalculateTrapezoid(function, lowerBoundary, upperBoundary, precision, ref n, ref result); break;
                    case "SIMPSON": CalculateSimpson(function, lowerBoundary, upperBoundary, precision, ref n, ref result); break;
                    default: CalculateSimpson(function, lowerBoundary, upperBoundary, precision, ref n, ref result); break;
                }
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private void CalculateLeftRectangle(Function function, double lowerBoundary, double upperBoundary, 
            double precision, ref int div_number, ref IntegralReplyDTO result) 
        {
            double prev, current = 0;
            do
            {
                prev = current;
                current = 0;
                double h = (upperBoundary - lowerBoundary) / div_number;
                for (int i = 0; i < div_number; i++)
                    current += function(lowerBoundary + i * h) * h;
                div_number *= 2;
            } while (Math.Abs(current - prev) / 3 > precision && div_number < 1000000);
            result.DivisionNumber = div_number/2;
            result.Result = current;
        }
        private void CalculateRightRectangle(Function function, double lowerBoundary, double upperBoundary,
            double precision, ref int div_number, ref IntegralReplyDTO result)
        {
            double prev, current = 0;
            do
            {
                prev = current;
                current = 0;
                double h = (upperBoundary - lowerBoundary) / div_number;
                for (int i = 1; i <= div_number; i++)
                    current += function(lowerBoundary + i * h) * h;
                div_number *= 2;
            } while (Math.Abs(current - prev) / 3 > precision && div_number < 1000000);
            result.DivisionNumber = div_number / 2;
            result.Result = current;
        }
        private void CalculateMidRectangle(Function function, double lowerBoundary, double upperBoundary,
            double precision, ref int div_number, ref IntegralReplyDTO result)
        {
            double prev, current = 0;
            do
            {
                prev = current;
                current = 0;
                double h = (upperBoundary - lowerBoundary) / div_number;
                for (int i = 0; i < div_number; i++)
                    current += function(lowerBoundary + (i + 0.5) * h) * h;
                div_number *= 2;
            } while (Math.Abs(current - prev) / 3 > precision && div_number < 1000000);
            result.DivisionNumber = div_number / 2;
            result.Result = current;
        }
        private void CalculateTrapezoid(Function function, double lowerBoundary, double upperBoundary,
            double precision, ref int div_number, ref IntegralReplyDTO result)
        {
            double prev, current = 0;
            do
            {
                prev = current;
                current = 0;
                double h = (upperBoundary - lowerBoundary) / div_number;
                for (int i = 1; i < div_number; i++)
                    current += function(lowerBoundary + i * h);
                current *= h;
                div_number *= 2;
            } while (Math.Abs(current - prev) / 3 > precision && div_number < 1000000);
            result.DivisionNumber = div_number / 2;
            result.Result = current;
        }
        private void CalculateSimpson(Function function, double lowerBoundary, double upperBoundary,
            double precision, ref int div_number, ref IntegralReplyDTO result)
        {
            double prev, current = 0;
            do
            {
                prev = current;
                current = 0;
                double h = (upperBoundary - lowerBoundary) / div_number;
                current = function(lowerBoundary) + function(upperBoundary);
                for (int i = 1; i < div_number; i++)
                {
                    double x = lowerBoundary + i * h;
                    current += i % 2 == 0 ? 2 * function(x) : 4 * function(x);
                }
                current *= h / 3;
                div_number *= 2;
            } while (Math.Abs(current - prev) / 15 > precision && div_number < 1000000);
            result.DivisionNumber = div_number / 2;
            result.Result = current;
        }
    }
}
