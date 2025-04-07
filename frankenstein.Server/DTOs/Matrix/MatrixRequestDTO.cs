namespace frankenstein.Server.DTOs.Matrix
{
    public class MatrixRequestDTO
    {
        public double[][] Coefficients { get; set; }
        public double[] Values { get; set; }
        public double Precision { get; set; }
    }
}
