using System;

namespace CEOSimulatorSP
{
    /// <summary>
    /// Handles board meeting logic and outcomes.
    /// </summary>
    public class BoardMeetingManager
    {
        private readonly Logger _logger;
        private readonly ConfigData _config;

        public BoardMeetingManager(Logger logger, ConfigData config, SaveData saveData)
        {
            _logger = logger;
            _config = config;
        }

        public bool CanStartMeeting(SaveData saveData)
        {
            if (saveData.Day < saveData.BoardMeetingCooldownUntilDay)
            {
                return false;
            }

            return saveData.Day - saveData.LastBoardMeetingDay >= _config.Tuning.BoardMeetingIntervalDays;
        }

        public BoardMeetingOutcome RunMeeting(SaveData saveData)
        {
            var wasThreat = saveData.TerminationThreatActive;
            var outcome = EvaluateOutcome(saveData);

            ApplyOutcome(saveData, outcome);
            saveData.LastBoardMeetingDay = saveData.Day;

            if (outcome == BoardMeetingOutcome.Crisis)
            {
                saveData.TerminationThreatActive = true;
            }

            if (outcome == BoardMeetingOutcome.StrongSupport || outcome == BoardMeetingOutcome.Mixed)
            {
                saveData.TerminationThreatActive = false;
            }

            if (wasThreat && (outcome == BoardMeetingOutcome.Warning || outcome == BoardMeetingOutcome.Crisis))
            {
                ApplyTermination(saveData);
                saveData.TerminationThreatActive = false;
            }

            _logger.Info($"Board meeting outcome: {outcome}.");
            return outcome;
        }

        private BoardMeetingOutcome EvaluateOutcome(SaveData saveData)
        {
            var stockScore = Math.Min(saveData.Stats.StockValue / 150.0f, 1.0f) * 100.0f;
            var stabilityScore = 100 - saveData.Stats.RiskLevel;
            var overall = (saveData.Stats.BoardApproval * 0.4f)
                          + (saveData.Stats.PublicReputation * 0.2f)
                          + (stockScore * 0.2f)
                          + (stabilityScore * 0.2f);

            if (saveData.Stats.BoardApproval < 15 || (saveData.Stats.RiskLevel > 85 && saveData.Stats.PublicReputation < 25))
            {
                return BoardMeetingOutcome.Crisis;
            }

            if (overall >= 75)
            {
                return BoardMeetingOutcome.StrongSupport;
            }

            if (overall >= 55)
            {
                return BoardMeetingOutcome.Mixed;
            }

            return BoardMeetingOutcome.Warning;
        }

        private void ApplyOutcome(SaveData saveData, BoardMeetingOutcome outcome)
        {
            switch (outcome)
            {
                case BoardMeetingOutcome.StrongSupport:
                    saveData.Stats.BoardApproval += 5;
                    saveData.Stats.Power += 3;
                    saveData.Stats.StockValue += 2.0f;
                    break;
                case BoardMeetingOutcome.Mixed:
                    saveData.Stats.BoardApproval += 1;
                    saveData.Stats.StockValue += 0.5f;
                    break;
                case BoardMeetingOutcome.Warning:
                    saveData.Stats.BoardApproval -= 5;
                    saveData.Stats.Power -= 3;
                    saveData.Stats.StockValue -= 1.0f;
                    break;
                case BoardMeetingOutcome.Crisis:
                    saveData.Stats.BoardApproval -= 8;
                    saveData.Stats.Power -= 5;
                    saveData.Stats.StockValue -= 2.5f;
                    break;
            }

            saveData.Stats.ClampAll();
        }

        private void ApplyTermination(SaveData saveData)
        {
            saveData.Stats.Power = 10;
            saveData.Stats.BoardApproval = 10;
            saveData.Stats.Cash = (int)(saveData.Stats.Cash * 0.7f);
            saveData.BoardMeetingCooldownUntilDay = saveData.Day + 5;
            _logger.Warn("Board removed the CEO. Cooling down board meetings.");
        }
    }

    public enum BoardMeetingOutcome
    {
        StrongSupport,
        Mixed,
        Warning,
        Crisis
    }
}
