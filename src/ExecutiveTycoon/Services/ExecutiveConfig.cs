using System.IO;
using GTA;
using Newtonsoft.Json;

namespace ExecutiveTycoon.Services;

public sealed class ExecutiveConfig
{
    public string DataFolderPath { get; set; }
    public string SaveFilePath { get; set; }
    public string LogFolderPath { get; set; }
    public int TickSeconds { get; set; }
    public int MinutesPerBusinessDay { get; set; }
    public int AutoSaveSeconds { get; set; }
    public bool EnableStaffSpawn { get; set; }
    public int MaxStaffSpawn { get; set; }
    public int DashboardKey { get; set; }
    public int QuickOverlayKey { get; set; }

    public static ExecutiveConfig Load(string path)
    {
        var settings = ScriptSettings.Load(path);
        var dataFolder = settings.GetValue("Paths", "DataFolder", "scripts/ExecutiveTycoon");
        var saveFile = Path.Combine(dataFolder, "save.json");
        var logFolder = Path.Combine(dataFolder, "logs");

        Directory.CreateDirectory(dataFolder);
        Directory.CreateDirectory(logFolder);

        return new ExecutiveConfig
        {
            DataFolderPath = dataFolder,
            SaveFilePath = saveFile,
            LogFolderPath = logFolder,
            TickSeconds = settings.GetValue("Simulation", "TickSeconds", 5),
            MinutesPerBusinessDay = settings.GetValue("Simulation", "MinutesPerBusinessDay", 10),
            AutoSaveSeconds = settings.GetValue("Persistence", "AutoSaveSeconds", 120),
            EnableStaffSpawn = settings.GetValue("Performance", "EnableStaffSpawn", false),
            MaxStaffSpawn = settings.GetValue("Performance", "MaxStaffSpawn", 6),
            DashboardKey = settings.GetValue("Keybinds", "DashboardContextControl", 51),
            QuickOverlayKey = settings.GetValue("Keybinds", "QuickOverlayVirtualKey", 118)
        };
    }

    public void WriteDefaultIniIfMissing(string iniPath)
    {
        if (File.Exists(iniPath))
        {
            return;
        }

        var template = "[Paths]\nDataFolder=scripts/ExecutiveTycoon\n\n[Simulation]\nTickSeconds=5\nMinutesPerBusinessDay=10\n\n[Persistence]\nAutoSaveSeconds=120\n\n[Performance]\nEnableStaffSpawn=false\nMaxStaffSpawn=6\n\n[Keybinds]\nDashboardContextControl=51\nQuickOverlayVirtualKey=118\n";
        File.WriteAllText(iniPath, template);
    }

    public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);
}
