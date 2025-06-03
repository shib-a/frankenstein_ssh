namespace frankenstein.Server.DTOs.Nonlinear
{
    public class NonlinearRequestDTO
    {
        public string FunctionInfo { get; set; }
        public double Precision { get; set; }
        public double LowerBoundary { get; set; }
        public double UpperBoundary { get; set; }
        public string Method { get; set; }
        public string Type { get; set; }
        public double X0 { get; set; }
        public double Y0 { get; set; }
    }
}
