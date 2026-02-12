using System.Collections.Generic;
using ExecutiveTycoon.Models;
using GTA.Math;

namespace ExecutiveTycoon.Data;

public static class OfficeCatalog
{
    public static IReadOnlyList<OfficeDefinition> All { get; } = new List<OfficeDefinition>
    {
        new()
        {
            Id = "arcadius",
            DisplayName = "Arcadius Business Center",
            BasePrice = 2250000,
            StaffCapacityBonus = 1,
            OperatingCostModifier = 0.97f,
            InteriorPosition = new Vector3(-141.6f, -620.9f, 168.82f),
            DesktopPosition = new Vector3(-141.0f, -620.1f, 168.82f)
        },
        new()
        {
            Id = "maze_west",
            DisplayName = "Maze Bank West",
            BasePrice = 1000000,
            StaffCapacityBonus = 0,
            OperatingCostModifier = 1.0f,
            InteriorPosition = new Vector3(-1367.2f, -471.3f, 72.04f),
            DesktopPosition = new Vector3(-1365.9f, -472.3f, 72.04f)
        },
        new()
        {
            Id = "lombank",
            DisplayName = "Lombank West",
            BasePrice = 3100000,
            StaffCapacityBonus = 2,
            OperatingCostModifier = 0.95f,
            InteriorPosition = new Vector3(-1581.4f, -565.4f, 108.52f),
            DesktopPosition = new Vector3(-1578.9f, -565.4f, 108.52f)
        },
        new()
        {
            Id = "maze_tower",
            DisplayName = "Maze Bank Tower",
            BasePrice = 4000000,
            StaffCapacityBonus = 3,
            OperatingCostModifier = 0.92f,
            InteriorPosition = new Vector3(-75.84f, -827.95f, 243.39f),
            DesktopPosition = new Vector3(-77.3f, -830.2f, 243.39f)
        }
    };
}
