namespace frankenstein.Server.DTOs.Integral
{
    public class IntegralRequestDTO
    {
        public string[] FunctionInfo { get; set; }
        public double Precision { get; set; }
        public double LowerBoundary { get; set; }
        public double UpperBoundary { get; set; }
        public string Method { get; set; }
    }
}
