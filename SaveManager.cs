using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace CEOSimulatorSP
{
    /// <summary>
    /// Handles save/load of game state.
    /// </summary>
    public class SaveManager
    {
        private readonly Logger _logger;
        private readonly string _savePath;

        public SaveManager(Logger logger)
        {
            _logger = logger;
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "CEOSimulatorSP");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            _savePath = Path.Combine(folder, "save.json");
        }

        public SaveData Load()
        {
            try
            {
                if (!File.Exists(_savePath))
                {
                    _logger.Info("No save file found. Creating new save.");
                    return SaveData.CreateNew();
                }

                var json = File.ReadAllText(_savePath);
                var data = JsonConvert.DeserializeObject<SaveData>(json);
                return data ?? SaveData.CreateNew();
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load save.json. Starting new save.", ex);
                return SaveData.CreateNew();
            }
        }

        public void Save(SaveData data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(_savePath, json);
                _logger.Info("Save.json written.");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to save save.json.", ex);
            }
        }
    }

    /// <summary>
    /// Serializable save data for the mod.
    /// </summary>
    public class SaveData
    {
        public int Day { get; set; }
        public CEOStats Stats { get; set; } = new CEOStats();
        public List<CorporateEvent> ActiveEvents { get; set; } = new List<CorporateEvent>();
        public List<PendingEvent> PendingEvents { get; set; } = new List<PendingEvent>();
        public int LastBoardMeetingDay { get; set; }
        public bool TerminationThreatActive { get; set; }
        public int BoardMeetingCooldownUntilDay { get; set; }
        public bool ProtestersActive { get; set; }
        public bool SecurityActive { get; set; }
        public bool InvestigatorActive { get; set; }

        public static SaveData CreateNew()
        {
            return new SaveData
            {
                Day = 1,
                LastBoardMeetingDay = 0,
                TerminationThreatActive = false,
                BoardMeetingCooldownUntilDay = 0,
                ProtestersActive = false,
                SecurityActive = false,
                InvestigatorActive = false,
                ActiveEvents = new List<CorporateEvent>(),
                PendingEvents = new List<PendingEvent>(),
                Stats = new CEOStats()
            };
        }
    }
}
