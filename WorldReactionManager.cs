using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;

namespace CEOSimulatorSP
{
    /// <summary>
    /// Handles world reactions such as protesters, security, and investigators.
    /// </summary>
    public class WorldReactionManager
    {
        private readonly Logger _logger;
        private readonly ConfigData _config;
        private readonly SaveData _saveData;
        private readonly Random _random = new Random();

        private readonly List<Ped> _protesters = new List<Ped>();
        private readonly List<Ped> _security = new List<Ped>();
        private Ped _investigator;
        private DateTime _lastNotificationTime = DateTime.MinValue;

        public WorldReactionManager(Logger logger, ConfigData config, SaveData saveData)
        {
            _logger = logger;
            _config = config;
            _saveData = saveData;
        }

        public void Update(CEOStats stats)
        {
            HandleProtesters(stats);
            HandleSecurity(stats);
            RestoreInvestigatorIfNeeded(stats);
            CleanupInvestigator(stats);
        }

        public void HandleDayStart(CEOStats stats)
        {
            if (stats.RiskLevel <= _config.Tuning.InvestigatorThreshold)
            {
                return;
            }

            if (_investigator != null && _investigator.Exists())
            {
                return;
            }

            if (_random.NextDouble() <= 0.2)
            {
                SpawnInvestigator();
            }
        }

        private void HandleProtesters(CEOStats stats)
        {
            if (stats.PublicReputation < _config.Tuning.ProtestThreshold && _protesters.Count == 0)
            {
                SpawnProtesters();
                return;
            }

            if (stats.PublicReputation > _config.Tuning.ProtestThreshold + 10 && _protesters.Count > 0)
            {
                DespawnGroup(_protesters);
                _saveData.ProtestersActive = false;
            }

            if (_saveData.ProtestersActive && _protesters.Count == 0 && stats.PublicReputation < _config.Tuning.ProtestThreshold)
            {
                SpawnProtesters();
            }
        }

        private void HandleSecurity(CEOStats stats)
        {
            var shouldSpawn = stats.Power > 80 || stats.RiskLevel > _config.Tuning.SecurityThreshold;
            if (shouldSpawn && _security.Count == 0)
            {
                SpawnSecurity();
                return;
            }

            if (!shouldSpawn && _security.Count > 0)
            {
                DespawnGroup(_security);
                _saveData.SecurityActive = false;
            }

            if (_saveData.SecurityActive && _security.Count == 0 && shouldSpawn)
            {
                SpawnSecurity();
            }
        }

        private void SpawnProtesters()
        {
            var spawn = _config.HQExteriorProtestSpawn.ToVector3();
            var count = _random.Next(6, 11);
            for (var i = 0; i < count; i++)
            {
                var ped = World.CreatePed(PedHash.A_M_Y_Hipster_01, spawn.Around(2.0f));
                if (ped != null)
                {
                    ped.Task.StartScenario("WORLD_HUMAN_CHEERING");
                    _protesters.Add(ped);
                }
            }

            NotifyOnce("~r~Protesters have gathered outside HQ.");
            _logger.Info("Spawned protesters.");
            _saveData.ProtestersActive = true;
        }

        private void SpawnSecurity()
        {
            foreach (var point in _config.HQInteriorSecuritySpawnPoints)
            {
                var ped = World.CreatePed(PedHash.S_M_M_Security_01, point.ToVector3());
                if (ped != null)
                {
                    ped.Task.StandStill(-1);
                    ped.BlockPermanentEvents = true;
                    _security.Add(ped);
                }
            }

            NotifyOnce("~b~Security has been increased inside HQ.");
            _logger.Info("Spawned security guards.");
            _saveData.SecurityActive = true;
        }

        private void SpawnInvestigator()
        {
            var spawn = _config.InvestigatorSpawn.ToVector3();
            _investigator = World.CreatePed(PedHash.S_M_M_FIBOffice_01, spawn);
            if (_investigator != null)
            {
                _investigator.Task.StartScenario("WORLD_HUMAN_STAND_IMPATIENT");
                _investigator.BlockPermanentEvents = true;
                NotifyOnce("~o~A regulator is on-site.");
                _logger.Warn("Spawned investigator.");
                _saveData.InvestigatorActive = true;
            }
        }

        private void CleanupInvestigator(CEOStats stats)
        {
            if (_investigator == null || !_investigator.Exists())
            {
                return;
            }

            if (stats.RiskLevel <= _config.Tuning.InvestigatorThreshold - 10)
            {
                _investigator.Delete();
                _investigator = null;
                _saveData.InvestigatorActive = false;
            }
        }

        private void RestoreInvestigatorIfNeeded(CEOStats stats)
        {
            if (!_saveData.InvestigatorActive)
            {
                return;
            }

            if (_investigator == null || !_investigator.Exists())
            {
                if (stats.RiskLevel > _config.Tuning.InvestigatorThreshold)
                {
                    SpawnInvestigator();
                }
                else
                {
                    _saveData.InvestigatorActive = false;
                }
            }
        }

        private void DespawnGroup(List<Ped> peds)
        {
            foreach (var ped in peds)
            {
                if (ped != null && ped.Exists())
                {
                    ped.Delete();
                }
            }

            peds.Clear();
        }

        private void NotifyOnce(string message)
        {
            if ((DateTime.UtcNow - _lastNotificationTime).TotalSeconds < 10)
            {
                return;
            }

            _lastNotificationTime = DateTime.UtcNow;
            GTA.UI.Screen.ShowNotification(message);
        }
    }
}
