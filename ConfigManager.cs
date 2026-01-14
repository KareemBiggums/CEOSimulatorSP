using System;
using System.Collections.Generic;
using System.IO;
using GTA.Math;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CEOSimulatorSP
{
    /// <summary>
    /// Handles loading and saving configuration data.
    /// </summary>
    public class ConfigManager
    {
        private readonly Logger _logger;
        private readonly string _configPath;

        public ConfigData CurrentConfig { get; private set; }

        public ConfigManager(Logger logger)
        {
            _logger = logger;
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "CEOSimulatorSP");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            _configPath = Path.Combine(folder, "Config.json");
        }

        public ConfigData Load()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    CurrentConfig = ConfigData.CreateDefault();
                    Save(CurrentConfig);
                    _logger.Info("Config.json created with defaults.");
                    return CurrentConfig;
                }

                var json = File.ReadAllText(_configPath);
                var config = JsonConvert.DeserializeObject<ConfigData>(json);
                CurrentConfig = config ?? ConfigData.CreateDefault();
                _logger.Info("Config.json loaded.");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load Config.json, using defaults.", ex);
                CurrentConfig = ConfigData.CreateDefault();
            }

            return CurrentConfig;
        }

        public void Save(ConfigData config)
        {
            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to save Config.json.", ex);
            }
        }
    }

    /// <summary>
    /// Serializable configuration data.
    /// </summary>
    public class ConfigData
    {
        public Vector3Data OfficeMarker { get; set; }
        public Vector3Data DeskMarker { get; set; }
        public Vector3Data AssistantMarker { get; set; }
        public Vector3Data BoardroomChairMarker { get; set; }
        public Vector3Data HQExteriorProtestSpawn { get; set; }
        public List<Vector3Data> HQInteriorSecuritySpawnPoints { get; set; } = new List<Vector3Data>();
        public Vector3Data InvestigatorSpawn { get; set; }
        public KeybindConfig Keybinds { get; set; } = new KeybindConfig();
        public TuningConfig Tuning { get; set; } = new TuningConfig();

        public static ConfigData CreateDefault()
        {
            return new ConfigData
            {
                OfficeMarker = new Vector3Data(-75.0f, -818.0f, 243.0f),
                DeskMarker = new Vector3Data(-75.6f, -822.2f, 243.0f),
                AssistantMarker = new Vector3Data(-72.5f, -821.5f, 243.0f),
                BoardroomChairMarker = new Vector3Data(-70.4f, -814.3f, 243.0f),
                HQExteriorProtestSpawn = new Vector3Data(-75.0f, -818.0f, 26.0f),
                HQInteriorSecuritySpawnPoints = new List<Vector3Data>
                {
                    new Vector3Data(-77.1f, -818.8f, 243.0f),
                    new Vector3Data(-73.4f, -816.9f, 243.0f)
                },
                InvestigatorSpawn = new Vector3Data(-74.2f, -819.6f, 243.0f),
                Keybinds = new KeybindConfig
                {
                    OpenMenu = Keys.F7,
                    Interact = Keys.E
                },
                Tuning = new TuningConfig
                {
                    ProtestThreshold = 30,
                    SecurityThreshold = 70,
                    InvestigatorThreshold = 75,
                    DailyOverhead = 10000,
                    BoardMeetingIntervalDays = 3
                }
            };
        }
    }

    /// <summary>
    /// Simple serializable vector wrapper for JSON.
    /// </summary>
    public class Vector3Data
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3Data() { }

        public Vector3Data(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }
    }

    /// <summary>
    /// Keybind configuration data.
    /// </summary>
    public class KeybindConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Keys OpenMenu { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Keys Interact { get; set; }
    }

    /// <summary>
    /// Gameplay tuning configuration data.
    /// </summary>
    public class TuningConfig
    {
        public int ProtestThreshold { get; set; }
        public int SecurityThreshold { get; set; }
        public int InvestigatorThreshold { get; set; }
        public int DailyOverhead { get; set; }
        public int BoardMeetingIntervalDays { get; set; }
    }
}
