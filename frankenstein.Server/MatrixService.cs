using frankenstein.Server.DTOs.Matrix;

namespace frankenstein.Server
{
    public class MatrixService
    {
        public bool CheckDiagonalDominance(double[,] A)
        {
            int n = A.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                for (int j = 0; j < n; j++)
                    if (j != i) sum += Math.Abs(A[i, j]);

                if (Math.Abs(A[i, i]) <= sum) return false;
            }
            return true;
        }

        public bool RearrangeForDominance(ref double[,] A, ref double[] B)
        {
            int n = A.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                double max = 0;
                int maxRow = i;

                for (int j = i; j < n; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < n; k++)
                        if (k != i) sum += Math.Abs(A[j, k]);

                    if (Math.Abs(A[j, i]) - sum > max)
                    {
                        max = Math.Abs(A[j, i]) - sum;
                        maxRow = j;
                    }
                }

                if (max <= 0) return false;
                if (maxRow != i)
                {
                    for (int j = 0; j < n; j++)
                        (A[i, j], A[maxRow, j]) = (A[maxRow, j], A[i, j]);
                    (B[i], B[maxRow]) = (B[maxRow], B[i]);
                }
            }
            return true;
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
                double[,] a = new double[n, n];
                double[] b = new double[n];
                double epsilon = request.Precision;
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                        a[i, j] = request.Coefficients[i][j];
                }
                b = request.Values;

                //if (!CheckDiagonalDominance(a))
                //{
                //    if (!RearrangeForDominance(ref a, ref b))
                //    {
                //        return new MatrixReplyDTO(ReplyEnum.NO_DIAGONAL_DOMINANCE, "No diagonal dominance in the matrix");
                //    }
                //}

                double[] solution = new double[n];
                int iterations = 0;
                double[] errors = new double[n];
                double[] prev = new double[n];
                double maxError;

                do
                {
                    Array.Copy(solution, prev, n);
                    maxError = 0;
                    for (int i = 0; i < n; i++)
                    {
                        double sum = b[i];
                        for (int j = 0; j < n; j++)
                        {
                            if (j < i) sum -= a[i, j] * solution[j];
                            if (j > i) sum -= a[i, j] * prev[j];
                        }

                        solution[i] = sum / a[i, i];
                        errors[i] = Math.Abs(solution[i] - prev[i]);
                        if (errors[i] > maxError) maxError = errors[i];
                    }
                    iterations++;
                } while (maxError > epsilon && iterations < 1000);
               
                return new MatrixReplyDTO(solution, ReplyEnum.OK, "", iterations, errors);
            }

            catch (Exception ex)
            {
                return new MatrixReplyDTO(ReplyEnum.ERROR, $"Error solving equations: {ex.Message}");
            }
        }
    }
}
