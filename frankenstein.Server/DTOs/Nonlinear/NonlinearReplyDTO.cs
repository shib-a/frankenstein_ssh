namespace frankenstein.Server.DTOs.Nonlinear
{
    public class NonlinearReplyDTO
    {
        public double Root { get; set; }
        public int DivisionNumber { get; set; }
        public string Message { get; set; }
        public int IterationCount { get; set; }
        public double[] Errors { get; set; }
        public string Plot { get; set; }
    }
}
