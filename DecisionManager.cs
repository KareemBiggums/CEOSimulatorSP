using System;

namespace CEOSimulatorSP
{
    /// <summary>
    /// Applies decisions and handles delayed consequences.
    /// </summary>
    public class DecisionManager
    {
        private readonly Logger _logger;
        private readonly EventManager _eventManager;
        private readonly Random _random = new Random();

        public DecisionManager(Logger logger, EventManager eventManager)
        {
            _logger = logger;
            _eventManager = eventManager;
        }

        public void ApplyChoice(SaveData saveData, CorporateEvent corporateEvent, EventChoice choice)
        {
            if (saveData == null || corporateEvent == null || choice == null)
            {
                return;
            }

            saveData.Stats.ApplyDeltas(choice.StatDeltas);
            saveData.Stats.Cash += choice.CashDelta;
            saveData.Stats.StockValue += choice.StockDelta;

            ApplyFlags(saveData, choice);

            saveData.Stats.ClampAll();

            if (!string.IsNullOrWhiteSpace(choice.DelayedEventId))
            {
                var delayDays = _random.Next(1, 3);
                saveData.PendingEvents.Add(new PendingEvent
                {
                    EventId = choice.DelayedEventId,
                    TriggerDay = saveData.Day + delayDays
                });
            }

            saveData.ActiveEvents.Remove(corporateEvent);
            _logger.Info($"Resolved event {corporateEvent.Id} with choice {choice.Label}.");
        }

        public void ApplyPendingEvents(SaveData saveData)
        {
            if (saveData == null)
            {
                return;
            }

            for (var i = saveData.PendingEvents.Count - 1; i >= 0; i--)
            {
                var pending = saveData.PendingEvents[i];
                if (pending.TriggerDay <= saveData.Day)
                {
                    var evt = _eventManager.GetEventById(pending.EventId);
                    if (evt != null)
                    {
                        saveData.ActiveEvents.Add(evt);
                    }

                    saveData.PendingEvents.RemoveAt(i);
                }
            }
        }

        private void ApplyFlags(SaveData saveData, EventChoice choice)
        {
            if (choice.Flags == null)
            {
                return;
            }

            if (choice.Flags.Contains("coverup"))
            {
                saveData.Stats.RiskLevel += 3;
            }

            if (choice.Flags.Contains("layoff"))
            {
                saveData.Stats.EmployeeMorale -= 2;
                saveData.Stats.PublicReputation -= 1;
            }

            if (choice.Flags.Contains("union_bust"))
            {
                saveData.Stats.PublicReputation -= 1;
                saveData.Stats.RiskLevel += 1;
            }

            if (choice.Flags.Contains("cost_cut"))
            {
                saveData.Stats.EmployeeMorale -= 1;
            }
        }
    }
}
