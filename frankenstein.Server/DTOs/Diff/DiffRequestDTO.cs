namespace frankenstein.Server.DTOs.Diff
{
    public class DiffRequestDTO
    {
        public string Function { get; set; } //LINEAR, QUADRATIC, TRIGONOMETRIC
        public double X0 { get; set; }
        public double Y0 { get; set; }
        public double Xn { get; set; }
        public double H { get; set; }
        public double Eps { get; set; }
    }
}
