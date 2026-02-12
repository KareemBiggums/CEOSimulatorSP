using System;
using System.Collections.Generic;
using System.Linq;
using ExecutiveTycoon.Models;

namespace ExecutiveTycoon.Services;

public sealed class SimulationEngine
{
    private readonly CompanyState _state;
    private readonly OfficeManager _officeManager;
    private readonly Logger _logger;
    private readonly Random _random = new();

    public SimulationEngine(CompanyState state, OfficeManager officeManager, Logger logger)
    {
        _state = state;
        _officeManager = officeManager;
        _logger = logger;
    }

    public string RunBusinessDay()
    {
        var managerCompetence = _state.Managers.Count == 0 ? 40f : _state.Managers.Average(m => m.Competence);
        var managerLoyalty = _state.Managers.Count == 0 ? 40f : _state.Managers.Average(m => m.Loyalty);
        var securityLead = _state.Managers.FirstOrDefault(m => m.Role == ManagerRole.HeadOfSecurity);

        var officeCount = _state.Offices.Count(o => o.Owned);
        var revenueBase = 22000m + (officeCount * 6500m);
        var directiveRevenueBonus = _state.ActiveDirectives.Sum(GetRevenueImpact);
        var randomSwing = (decimal)(_random.NextDouble() * 0.16 - 0.08);

        var revenue = revenueBase * (decimal)(1 + (managerCompetence / 250f) + directiveRevenueBonus) * (1 + randomSwing);

        var payroll = _state.Managers.Sum(m => m.SalaryPerBusinessDay);
        var officeOverhead = _state.Offices.Where(o => o.Owned).Sum(o => (decimal)(1800f * o.OperatingCostModifier * (1f + (o.UpgradeTier * 0.08f))));
        var complianceCost = 1500m + (decimal)(_state.ActiveDirectives.Count(d => d.Type == DirectiveType.IncreaseCompliance) * 1100f);
        var securityCost = 1000m + (decimal)(_state.ActiveDirectives.Count(d => d.Type == DirectiveType.IncreaseSecurityBudget) * 900f);

        var totalCosts = payroll + officeOverhead + complianceCost + securityCost;
        var profit = revenue - totalCosts;

        _state.Treasury += profit;
        _state.BusinessDaysElapsed++;

        var riskDelta = 0.8f;
        riskDelta += _state.ActiveDirectives.Count(d => d.Type == DirectiveType.ExpandRegion) * 1.2f;
        riskDelta += _state.ActiveDirectives.Count(d => d.Type == DirectiveType.DecreaseCompliance) * 2.2f;
        riskDelta -= _state.ActiveDirectives.Count(d => d.Type == DirectiveType.IncreaseCompliance) * 2.5f;
        riskDelta -= securityLead == null ? 0f : securityLead.Competence / 120f;
        riskDelta -= managerLoyalty / 260f;
        riskDelta += (float)(_random.NextDouble() - 0.5);

        _state.RiskHeat = Clamp(_state.RiskHeat + riskDelta, 0f, 100f);

        var repDelta = (profit > 0 ? 0.7f : -1.1f) + (managerLoyalty / 200f) - (_state.RiskHeat / 200f);
        _state.Reputation = Clamp(_state.Reputation + repDelta, 0f, 100f);

        ResolveDirectiveTimers();
        var randomEvent = MaybeTriggerRandomEvent();

        var summary = $"Day {_state.BusinessDaysElapsed}: Revenue ${Math.Round(revenue)}, Costs ${Math.Round(totalCosts)}, Profit ${Math.Round(profit)}, Treasury ${Math.Round(_state.Treasury)}";
        if (!string.IsNullOrEmpty(randomEvent))
        {
            summary += $" | Event: {randomEvent}";
        }

        _logger.Info(summary);
        return summary;
    }

    public string BuildWeeklyReport()
    {
        return $"Weekly Report - Day {_state.BusinessDaysElapsed}. Treasury: ${Math.Round(_state.Treasury)}, Debt: ${Math.Round(_state.OutstandingDebt)}, Reputation: {Math.Round(_state.Reputation, 1)}, Risk: {Math.Round(_state.RiskHeat, 1)}, Active HQ: {_state.ActiveHqOfficeId ?? "None"}";
    }

    public bool AddDirective(DirectiveType type)
    {
        if (_state.ActiveDirectives.Count(d => d.Type == type) >= 1)
        {
            return false;
        }

        _state.ActiveDirectives.Add(new DirectiveState
        {
            Type = type,
            RemainingBusinessDays = 5,
            Intensity = 1f
        });

        return true;
    }

    public bool HireManager(ManagerRole role)
    {
        if (_state.Managers.Any(m => m.Role == role))
        {
            return false;
        }

        _state.Managers.Add(new ManagerProfile
        {
            Name = role.ToString(),
            Role = role,
            Competence = _random.Next(45, 85),
            Loyalty = _random.Next(40, 90),
            RiskAppetite = _random.Next(25, 80),
            SalaryPerBusinessDay = role switch
            {
                ManagerRole.COO => 2400,
                ManagerRole.CFO => 2600,
                ManagerRole.HeadOfSecurity => 2300,
                _ => 2100
            }
        });

        return true;
    }

    public bool TryTransferToTreasury(int personalFundsAvailable, int amount)
    {
        if (amount <= 0 || personalFundsAvailable < amount)
        {
            return false;
        }

        _state.Treasury += amount;
        _state.Reputation = Clamp(_state.Reputation + 0.3f, 0f, 100f);
        return true;
    }

    public bool TryWithdrawDividend(int amount)
    {
        if (amount <= 0 || _state.Treasury < amount)
        {
            return false;
        }

        if (_state.Reputation < 30f || _state.RiskHeat > 75f)
        {
            return false;
        }

        _state.Treasury -= amount;
        _state.Reputation = Clamp(_state.Reputation - 0.2f, 0f, 100f);
        return true;
    }

    private decimal GetRevenueImpact(DirectiveState directive)
    {
        return directive.Type switch
        {
            DirectiveType.ExpandRegion => 0.15m,
            DirectiveType.LaunchProductLine => 0.18m,
            DirectiveType.DecreaseCompliance => 0.06m,
            _ => 0m
        };
    }

    private string MaybeTriggerRandomEvent()
    {
        var eventChance = 0.07 + (_state.RiskHeat / 500f);
        if (_random.NextDouble() > eventChance)
        {
            return string.Empty;
        }

        var roll = _random.Next(0, 4);
        switch (roll)
        {
            case 0:
                _state.Treasury -= 15000;
                _state.Reputation = Clamp(_state.Reputation - 2.4f, 0f, 100f);
                return "Audit hit: legal penalties paid.";
            case 1:
                _state.Reputation = Clamp(_state.Reputation - 3.2f, 0f, 100f);
                return "PR issue: reputational decline.";
            case 2:
                if (_state.Managers.Count > 0)
                {
                    var idx = _random.Next(0, _state.Managers.Count);
                    _state.Managers[idx].Loyalty = Math.Max(0, _state.Managers[idx].Loyalty - 10);
                }

                return "Employee turnover: loyalty dip in leadership.";
            default:
                _state.Treasury -= 9000;
                return "Supply cost spike increased expenses.";
        }
    }

    private void ResolveDirectiveTimers()
    {
        var remaining = new List<DirectiveState>();
        foreach (var directive in _state.ActiveDirectives)
        {
            directive.RemainingBusinessDays--;
            if (directive.RemainingBusinessDays > 0)
            {
                remaining.Add(directive);
            }
        }

        _state.ActiveDirectives = remaining;
    }

    private static float Clamp(float value, float min, float max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }
}
