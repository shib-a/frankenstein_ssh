namespace frankenstein.Server.DTOs.Matrix
{
    public class MatrixReplyDTO
    {
        public double[] Solution { get; set; }
        public ReplyEnum Status { get; set; }
        public string Message { get; set; }
        public int IterationCount { get; set; }
        public double Norm { get; set; }
        public double[] Errors { get; set; }
        public MatrixReplyDTO(double[] solution, ReplyEnum status, string message, int iterationCount, double norm, double[] errors)
        {
            Solution = solution;
            Status = status;
            Message = message;
            IterationCount = iterationCount;
            Norm = norm;
            Errors = errors;
        }
        public MatrixReplyDTO(ReplyEnum status, string message)
        {
            Status = status;
            Message = message;
        }
    }
}
