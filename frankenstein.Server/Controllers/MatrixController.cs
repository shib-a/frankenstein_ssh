using frankenstein.Server.DTOs.Matrix;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;

namespace frankenstein.Server.Controllers;
[ApiController]
[Route("matrix")]
public class MatrixController
{
    [HttpPost("table")]
    public MatrixReplyDTO SolveMatrixOne([FromBody] MatrixRequestDTO request)
    {
        var solution = SolveEquations(request);
        return solution;
    }

    [HttpPost("file")]
    public async Task<MatrixReplyDTO> HandleFileUpload(IFormFile file)
    {
        if (file == null || file.Length == 0) return new MatrixReplyDTO(ReplyEnum.FILE_NOT_FOUND, "No file uploaded.");
        
        using (var stream = file.OpenReadStream())
        using (var reader = new StreamReader(stream))
        {
            if (!reader.EndOfStream)
            {
                try
                {
                    //if (file.FileName.IndexOf(".") <= 0 || file.FileName.Substring(file.FileName.IndexOf("."), file.FileName.Length)!=".txt"){ return new ReplyDTO(ReplyEnum.EMPTY_FILE, "The file has no data"); }
                    ;
                    var firstLine = await reader.ReadLineAsync();
                    var splitLine = firstLine.Split(' ');
                    var size = int.Parse(splitLine[0]);
                    var precision = double.Parse(string.Format(string.Concat("0",splitLine[1])), System.Globalization.CultureInfo.InvariantCulture);
                    double[][] coefficients = new double[size][];
                    double[] values = new double[size];
                    for(int i=0; i<size; i++)
                    {
                        coefficients[i] = new double[size];
                    }
                    int line_count = 0;
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        var spl = line.Split(' ');
                        for (int i = 0; i < spl.Length-1; i++)
                        {
                            coefficients[line_count][i] = double.Parse(spl[i], System.Globalization.CultureInfo.InvariantCulture);
                        }
                        values[line_count] = double.Parse(spl[spl.Length - 1], System.Globalization.CultureInfo.InvariantCulture);
                        line_count++;
                    }
                    var result = new MatrixRequestDTO();
                    result.Coefficients = coefficients;
                    result.Values = values;
                    result.Precision = precision;
                    var solution = SolveEquations(result);
                    return solution;
                } catch (Exception e)
                {
                    return new MatrixReplyDTO(ReplyEnum.ERROR, $"The file has invalid data");
                }

            }
            return new MatrixReplyDTO(ReplyEnum.EMPTY_FILE, "The file has no data");
        }
    }

    public MatrixReplyDTO SolveEquations(MatrixRequestDTO request)
    {
        try
        {
            if (request.Coefficients == null || !request.Coefficients.Any() ||
                request.Values == null || request.Values.Length != request.Coefficients.Length ||
                request.Coefficients.Any(row => row.Length != request.Coefficients.Length))
            {
                return new MatrixReplyDTO(ReplyEnum.INVALID_MATRIX_ERROR, "Invalid matrix or vector dimensions");
            }

            int n = request.Coefficients.Length;
            double[,] matrix = new double[n, n + 1];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    matrix[i, j] = request.Coefficients[i][j];
                matrix[i, n] = request.Values[i];
            }
            if (!CheckDiagonalDominance(matrix))
            {
                matrix = EnsureDiagonalDominance(matrix);
                if (matrix == null)
                {
                    return new MatrixReplyDTO(ReplyEnum.NO_DIAGONAL_DOMINANCE, "Cannot achieve diagonal dominance");
                }
            }
            double[,] a = new double[n, n];
            double[] b = new double[n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    a[i, j] = -matrix[i, j] / matrix[i, i];
                b[i] = matrix[i, n] / matrix[i, i];
                a[i, i] = 0;
            }
            double matrixNorm = CalculateMatrixNorm(a);
            var (solution, iterations, errors) = SolveBySimpleIteration(a, b, request.Precision);
            return new ReplyDTO(solution, ReplyEnum.OK, "", iterations, matrixNorm, errors);
        }
        catch (Exception ex)
        {
            return new MatrixReplyDTO(ReplyEnum.ERROR, $"Error solving equations: {ex.Message}");
        }
    }
    
    private bool CheckDiagonalDominance(double[,] matrix)
    {
        int n = matrix.GetLength(0);
        for (int i = 0; i < n; i++)
        {
            double diagonal = Math.Abs(matrix[i, i]);
            double rowSum = 0;
            for (int j = 0; j < n; j++)
            {
                if (i != j)
                    rowSum += Math.Abs(matrix[i, j]);
            }
            if (diagonal <= rowSum)
                return false;
        }
        return true;
    }

    private double[,] EnsureDiagonalDominance(double[,] matrix)
    {
        int n = matrix.GetLength(0);
        int[] rowOrder = Enumerable.Range(0, n).ToArray();
        double[,] tempMatrix = (double[,])matrix.Clone();
        if (GeneratePermutations(rowOrder, 0, tempMatrix))
        {
            double[,] result = new double[n, n + 1];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n + 1; j++)
                {
                    result[i, j] = matrix[rowOrder[i], j];
                }
            }
            return result;
        }
        return null;
    }

    private bool GeneratePermutations(int[] array, int start, double[,] matrix)
    {
        if (start == array.Length)
        {
            double[,] permutedMatrix = new double[array.Length, matrix.GetLength(1)];
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    permutedMatrix[i, j] = matrix[array[i], j];
                }
            }
            return CheckDiagonalDominance(permutedMatrix);
        }
        for (int i = start; i < array.Length; i++)
        {
            Swap(ref array[start], ref array[i]);
            if (GeneratePermutations(array, start + 1, matrix))
                return true;
            Swap(ref array[start], ref array[i]);
        }
        return false;
    }

    private void Swap(ref int a, ref int b)
    {
        int temp = a;
        a = b;
        b = temp;
    }

    private (double[] solution, int iterations, double[] errors) SolveBySimpleIteration(
        double[,] a, double[] b, double precision)
    {
        int n = b.Length;
        double[] x = new double[n];
        double[] xPrev = new double[n];
        int maxIterations = 1000;
        int iterations = 0;
        do
        {
            Array.Copy(x, xPrev, n);
            for (int i = 0; i < n; i++)
            {
                x[i] = b[i];
                for (int j = 0; j < n; j++)
                    x[i] += a[i, j] * xPrev[j];
                xPrev[i] = x[i];
            }
            iterations++;
        } while (CalculateError(x, xPrev) > precision && iterations < maxIterations);
        double[] errors = new double[n];
        for (int i = 0; i < n; i++)
            errors[i] = Math.Abs(x[i] - xPrev[i]);
        return (x, iterations, errors);
    }

    private double CalculateMatrixNorm(double[,] matrix)
    {
        int n = matrix.GetLength(0);
        double maxSum = 0;
        for (int i = 0; i < n; i++)
        {
            double sum = 0;
            for (int j = 0; j < n; j++)
                sum += Math.Abs(matrix[i, j]);
            maxSum = Math.Max(maxSum, sum);
        }
        return maxSum;
    }

    private double CalculateError(double[] x, double[] xPrev)
    {
        double maxError = 0;
        for (int i = 0; i < x.Length; i++)
            maxError = Math.Max(maxError, Math.Abs(x[i] - xPrev[i]));
        return maxError;
    }
}
