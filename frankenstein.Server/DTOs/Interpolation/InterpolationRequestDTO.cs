namespace frankenstein.Server.DTOs.Interpolation
{
    public class InterpolationRequestDTO
    {
        public double[] YValues { get; set; }
        public double[] XValues { get; set; }
        //public string Function { get; set; }
        //public double X0 { get; set; }
        //public double Xn { get; set; }
        //public double N { get; set; }
        public double TargetX { get; set; }

    }
}
