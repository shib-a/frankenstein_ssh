namespace frankenstein.Server.DTOs.Diff
{
    public class DiffReplyDTO
    {
        public string Plot { get; set; }
        public double[] Solutions { get; set; }
        public string ErrorMessage { get; set; }
    }
}
