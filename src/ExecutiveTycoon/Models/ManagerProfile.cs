namespace ExecutiveTycoon.Models;

public sealed class ManagerProfile
{
    public string Name { get; set; }
    public ManagerRole Role { get; set; }
    public int Competence { get; set; }
    public int Loyalty { get; set; }
    public int RiskAppetite { get; set; }
    public int SalaryPerBusinessDay { get; set; }
}
