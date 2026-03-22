using GTA.Math;

namespace ExecutiveTycoon.Models;

public sealed class OfficeDefinition
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public int BasePrice { get; set; }
    public float OperatingCostModifier { get; set; }
    public int StaffCapacityBonus { get; set; }
    public Vector3 InteriorPosition { get; set; }
    public Vector3 DesktopPosition { get; set; }
}
