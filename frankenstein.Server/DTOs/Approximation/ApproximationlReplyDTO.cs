namespace frankenstein.Server.DTOs.Approximation
{
    public class ApproximationReplyDTO
    {
        public string Function { get; set; }
        public double[] Coefficients { get; set; }
        public double Deviation { get; set; }
        public double MSE { get; set; }
        public double R2 { get; set; }
        public string ErrorMessage { get; set; }
        public string Plot { get; set; }
    }
}
