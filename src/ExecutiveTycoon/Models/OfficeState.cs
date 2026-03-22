namespace ExecutiveTycoon.Models;

public sealed class OfficeState
{
    public string OfficeId { get; set; }
    public bool Owned { get; set; }
    public int UpgradeTier { get; set; }
    public int DecorTier { get; set; }
    public int StaffCapacityBonus { get; set; }
    public float OperatingCostModifier { get; set; }
}
