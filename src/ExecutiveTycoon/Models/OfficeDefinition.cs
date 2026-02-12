using GTA.Math;

namespace ExecutiveTycoon.Models;

public sealed class OfficeDefinition
{
    public string Id { get; init; }
    public string DisplayName { get; init; }
    public int BasePrice { get; init; }
    public float OperatingCostModifier { get; init; }
    public int StaffCapacityBonus { get; init; }
    public Vector3 InteriorPosition { get; init; }
    public Vector3 DesktopPosition { get; init; }
}
