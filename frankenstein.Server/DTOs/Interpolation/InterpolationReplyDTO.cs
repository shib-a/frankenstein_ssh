namespace frankenstein.Server.DTOs.Interpolation
{
    public class InterpolationReplyDTO
    {
        public string Function { get; set; }
        public double[] Solutions { get; set; }
        public string ErrorMessage { get; set; }
        public string Plot { get; set; }
    }
}
