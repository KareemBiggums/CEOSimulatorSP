using System;
using System.Collections.Generic;

namespace ExecutiveTycoon.Models;

public sealed class CompanyState
{
    public decimal Treasury { get; set; } = 250000m;
    public decimal OutstandingDebt { get; set; }
    public float Reputation { get; set; } = 50f;
    public float RiskHeat { get; set; } = 25f;
    public int BusinessDaysElapsed { get; set; }
    public DateTime LastSaveUtc { get; set; } = DateTime.UtcNow;
    public string ActiveHqOfficeId { get; set; }
    public List<OfficeState> Offices { get; set; } = new();
    public List<ManagerProfile> Managers { get; set; } = new();
    public List<DirectiveState> ActiveDirectives { get; set; } = new();
}
