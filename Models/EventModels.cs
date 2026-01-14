using System.Collections.Generic;

namespace CEOSimulatorSP
{
    /// <summary>
    /// Represents a stat delta bundle.
    /// </summary>
    public class StatDeltas
    {
        public int Power { get; set; }
        public int PublicReputation { get; set; }
        public int BoardApproval { get; set; }
        public int EmployeeMorale { get; set; }
        public int RiskLevel { get; set; }
    }

    /// <summary>
    /// Represents a choice within an event.
    /// </summary>
    public class EventChoice
    {
        public string Label { get; set; }
        public string OutcomeText { get; set; }
        public StatDeltas StatDeltas { get; set; } = new StatDeltas();
        public int CashDelta { get; set; }
        public float StockDelta { get; set; }
        public List<string> Flags { get; set; } = new List<string>();
        public string DelayedEventId { get; set; }
    }

    /// <summary>
    /// Represents a corporate event.
    /// </summary>
    public class CorporateEvent
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public int Severity { get; set; }
        public string Description { get; set; }
        public List<EventChoice> Choices { get; set; } = new List<EventChoice>();
    }

    /// <summary>
    /// Represents a pending event scheduled for a future day.
    /// </summary>
    public class PendingEvent
    {
        public string EventId { get; set; }
        public int TriggerDay { get; set; }
    }
}
