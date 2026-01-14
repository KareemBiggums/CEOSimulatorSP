using System;

namespace CEOSimulatorSP
{
    /// <summary>
    /// Represents CEO statistics and provides clamping utilities.
    /// </summary>
    public class CEOStats
    {
        public int Power { get; set; } = 55;
        public int PublicReputation { get; set; } = 55;
        public int BoardApproval { get; set; } = 55;
        public int EmployeeMorale { get; set; } = 55;
        public int RiskLevel { get; set; } = 30;
        public float StockValue { get; set; } = 100.0f;
        public int Cash { get; set; } = 250000;

        public void ApplyDeltas(StatDeltas deltas)
        {
            if (deltas == null)
            {
                return;
            }

            Power += deltas.Power;
            PublicReputation += deltas.PublicReputation;
            BoardApproval += deltas.BoardApproval;
            EmployeeMorale += deltas.EmployeeMorale;
            RiskLevel += deltas.RiskLevel;
            ClampAll();
        }

        public void ClampAll()
        {
            Power = Clamp(Power, 0, 100);
            PublicReputation = Clamp(PublicReputation, 0, 100);
            BoardApproval = Clamp(BoardApproval, 0, 100);
            EmployeeMorale = Clamp(EmployeeMorale, 0, 100);
            RiskLevel = Clamp(RiskLevel, 0, 100);
            StockValue = Math.Max(0.0f, StockValue);
            Cash = Math.Max(0, Cash);
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}
