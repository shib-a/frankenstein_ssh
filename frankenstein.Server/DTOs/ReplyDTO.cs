namespace frankenstein.Server.DTOs
{
    public class ReplyDTO
    {
        public double[] Solution { get; set; }
        public ReplyEnum Status { get; set; }
        public string Message { get; set; }
        public int IterationCount { get; set; }
        public double Norm { get; set; }
        public double[] Errors { get; set; }
        public ReplyDTO(double[] solution, ReplyEnum status, string message, int iterationCount, double norm, double[] errors)
        {
            Solution = solution;
            Status = status;
            Message = message;
            IterationCount = iterationCount;
            Norm = norm;
            Errors = errors;
        }
        public ReplyDTO(ReplyEnum status, string message)
        {
            Status = status;
            Message = message;
        }
    }
}
