using System;
using System.Linq;
using ExecutiveTycoon.Models;
using ExecutiveTycoon.Services;
using GTA;
using GTA.UI;
using LemonUI;
using LemonUI.Menus;

namespace ExecutiveTycoon.UI;

public sealed class UiController
{
    private readonly CompanyState _state;
    private readonly OfficeManager _officeManager;
    private readonly SimulationEngine _sim;
    private readonly SaveService _save;
    private readonly Logger _logger;

    private readonly ObjectPool _pool = new();
    private readonly NativeMenu _dashboard;
    private readonly NativeMenu _brokerMenu;
    private readonly NativeMenu _officesMenu;
    private readonly NativeMenu _staffMenu;
    private readonly NativeMenu _directiveMenu;
    private readonly NativeMenu _financeMenu;

    private string _latestSummary = "ExecutiveTycoon initialized.";

    public UiController(CompanyState state, OfficeManager officeManager, SimulationEngine sim, SaveService save, Logger logger)
    {
        _state = state;
        _officeManager = officeManager;
        _sim = sim;
        _save = save;
        _logger = logger;

        _dashboard = new NativeMenu("ExecutiveTycoon", "CEO Dashboard");
        _brokerMenu = new NativeMenu("Executive Broker", "Acquire Offices");
        _officesMenu = new NativeMenu("Office Network", "Owned Offices");
        _staffMenu = new NativeMenu("Leadership", "Hire/FIre Managers");
        _directiveMenu = new NativeMenu("Directives", "Strategic Actions");
        _financeMenu = new NativeMenu("Finance", "Treasury Controls");

        _pool.Add(_dashboard);
        _pool.Add(_brokerMenu);
        _pool.Add(_officesMenu);
        _pool.Add(_staffMenu);
        _pool.Add(_directiveMenu);
        _pool.Add(_financeMenu);

        BuildMenus();
    }

    public void Process() => _pool.Process();

    public bool IsAnyMenuOpen => _pool.AreAnyVisible;

    public void OpenDashboard()
    {
        RefreshMenus();
        _dashboard.Visible = true;
    }

    public void OpenBrokerMenu()
    {
        RefreshBrokerItems();
        _brokerMenu.Visible = true;
    }

    public void ShowQuickOverlay()
    {
        var text = $"Treasury: ${Math.Round(_state.Treasury)} ~n~Rep: {Math.Round(_state.Reputation, 1)} ~n~Risk: {Math.Round(_state.RiskHeat, 1)} ~n~Active HQ: {_state.ActiveHqOfficeId ?? "None"} ~n~{_latestSummary}";
        Screen.ShowSubtitle(text, 5000);
    }

    public void SetLatestSummary(string summary)
    {
        _latestSummary = summary;
    }

    private void BuildMenus()
    {
        _dashboard.AddSubMenu(_officesMenu);
        _dashboard.AddSubMenu(_staffMenu);
        _dashboard.AddSubMenu(_directiveMenu);
        _dashboard.AddSubMenu(_financeMenu);

        var brokerItem = new NativeItem("Open Executive Office Broker", "Purchase and upgrade executive offices.");
        brokerItem.Activated += (_, _) => OpenBrokerMenu();
        _dashboard.Add(brokerItem);

        var saveItem = new NativeItem("Save Company", "Manual save.");
        saveItem.Activated += (_, _) =>
        {
            _save.Save(_state);
            Notification.Show("~g~ExecutiveTycoon: Saved.");
        };
        _dashboard.Add(saveItem);

        var statusItem = new NativeItem("Status Snapshot", "Display latest company simulation summary.");
        statusItem.Activated += (_, _) => Notification.Show($"~b~{_latestSummary}");
        _dashboard.Add(statusItem);

        RefreshMenus();
    }

    private void RefreshMenus()
    {
        RefreshBrokerItems();
        RefreshOwnedOfficesMenu();
        RefreshStaffMenu();
        RefreshDirectivesMenu();
        RefreshFinanceMenu();
    }

    private void RefreshBrokerItems()
    {
        _brokerMenu.Clear();
        foreach (var office in _officeManager.Definitions)
        {
            var owned = _officeManager.IsOwned(office.Id);
            var item = new NativeItem(
                owned ? $"{office.DisplayName} (Owned)" : $"Buy {office.DisplayName} - ${office.BasePrice:N0}",
                "Payment Mode: Personal/Company/Split cycling per click.");

            item.Activated += (_, _) =>
            {
                if (owned)
                {
                    Notification.Show("~y~Already owned.");
                    return;
                }

                var mode = PickPaymentMode();
                var success = _officeManager.TryPurchase(office, mode, _state.Treasury, Game.Player.Money, out var companyCost, out var personalCost);
                if (!success)
                {
                    Notification.Show("~r~Insufficient funds for purchase.");
                    return;
                }

                _state.Treasury -= companyCost;
                Game.Player.Money -= personalCost;
                Notification.Show($"~g~Purchased {office.DisplayName}. Active HQ: {_state.ActiveHqOfficeId}");
                RefreshMenus();
            };

            _brokerMenu.Add(item);

            if (owned)
            {
                var upgradeItem = new NativeItem($"Upgrade {office.DisplayName}", "Upgrade tier 0-3");
                upgradeItem.Activated += (_, _) =>
                {
                    var mode = PickPaymentMode();
                    var ok = _officeManager.TryUpgradeOffice(office.Id, _state.Treasury, Game.Player.Money, mode, out var companyCost, out var personalCost);
                    if (!ok)
                    {
                        Notification.Show("~r~Upgrade unavailable or insufficient funds.");
                        return;
                    }

                    _state.Treasury -= companyCost;
                    Game.Player.Money -= personalCost;
                    Notification.Show("~g~Office upgraded.");
                    RefreshMenus();
                };
                _brokerMenu.Add(upgradeItem);
            }
        }
    }

    private void RefreshOwnedOfficesMenu()
    {
        _officesMenu.Clear();

        foreach (var stateOffice in _state.Offices.Where(o => o.Owned))
        {
            var def = _officeManager.Definitions.FirstOrDefault(o => o.Id == stateOffice.OfficeId);
            if (def == null)
            {
                continue;
            }

            var isActive = _state.ActiveHqOfficeId == stateOffice.OfficeId;
            var item = new NativeItem($"{def.DisplayName} {(isActive ? "[ACTIVE HQ]" : string.Empty)}", $"Upgrade Tier: {stateOffice.UpgradeTier}");
            item.Activated += (_, _) =>
            {
                if (_officeManager.TryActivateHq(stateOffice.OfficeId))
                {
                    Notification.Show($"~g~Active HQ set to {def.DisplayName}.");
                    RefreshOwnedOfficesMenu();
                }
            };
            _officesMenu.Add(item);
        }

        if (_state.Offices.All(o => !o.Owned))
        {
            _officesMenu.Add(new NativeItem("No offices owned.", "Buy your first office from broker menu."));
        }
    }

    private void RefreshStaffMenu()
    {
        _staffMenu.Clear();
        AddStaffHireItem(ManagerRole.COO);
        AddStaffHireItem(ManagerRole.CFO);
        AddStaffHireItem(ManagerRole.HeadOfSecurity);

        foreach (var manager in _state.Managers)
        {
            var summary = new NativeItem($"{manager.Role} - {manager.Name}", $"Comp {manager.Competence} / Loy {manager.Loyalty} / Risk {manager.RiskAppetite}");
            _staffMenu.Add(summary);
        }
    }

    private void AddStaffHireItem(ManagerRole role)
    {
        var exists = _state.Managers.Any(m => m.Role == role);
        var item = new NativeItem(exists ? $"{role} (Hired)" : $"Hire {role}", "V1 hire set: COO, CFO, HeadOfSecurity.");
        item.Activated += (_, _) =>
        {
            if (!_sim.HireManager(role))
            {
                Notification.Show("~y~Role already filled.");
                return;
            }

            Notification.Show($"~g~Hired {role}.");
            RefreshStaffMenu();
        };
        _staffMenu.Add(item);
    }

    private void RefreshDirectivesMenu()
    {
        _directiveMenu.Clear();
        AddDirectiveItem(DirectiveType.ExpandRegion, "Raise growth and risk.");
        AddDirectiveItem(DirectiveType.IncreaseCompliance, "Reduce risk, increases cost.");
        AddDirectiveItem(DirectiveType.DecreaseCompliance, "Short-term margin, raises risk.");
        AddDirectiveItem(DirectiveType.IncreaseSecurityBudget, "Lowers future incidents.");
        AddDirectiveItem(DirectiveType.LaunchProductLine, "Revenue burst with execution risk.");

        foreach (var active in _state.ActiveDirectives)
        {
            _directiveMenu.Add(new NativeItem($"Active: {active.Type}", $"Remaining Days: {active.RemainingBusinessDays}"));
        }
    }

    private void AddDirectiveItem(DirectiveType type, string description)
    {
        var item = new NativeItem($"Issue {type}", description);
        item.Activated += (_, _) =>
        {
            if (!_sim.AddDirective(type))
            {
                Notification.Show("~y~Directive already active.");
                return;
            }

            Notification.Show($"~g~Directive issued: {type}");
            RefreshDirectivesMenu();
        };
        _directiveMenu.Add(item);
    }

    private void RefreshFinanceMenu()
    {
        _financeMenu.Clear();

        var inject = new NativeItem("Inject Capital ($50,000)", "Move from personal to company.");
        inject.Activated += (_, _) =>
        {
            const int amount = 50000;
            if (!_sim.TryTransferToTreasury(Game.Player.Money, amount))
            {
                Notification.Show("~r~Unable to inject capital.");
                return;
            }

            Game.Player.Money -= amount;
            Notification.Show("~g~Capital invested.");
        };

        var withdraw = new NativeItem("Withdraw Dividend ($25,000)", "Blocked if low reputation or high risk.");
        withdraw.Activated += (_, _) =>
        {
            const int amount = 25000;
            if (!_sim.TryWithdrawDividend(amount))
            {
                Notification.Show("~r~Dividend blocked by treasury/risk/reputation constraints.");
                return;
            }

            Game.Player.Money += amount;
            Notification.Show("~g~Dividend paid.");
        };

        _financeMenu.Add(new NativeItem($"Treasury: ${Math.Round(_state.Treasury)}", $"Debt: ${Math.Round(_state.OutstandingDebt)}"));
        _financeMenu.Add(new NativeItem($"Reputation: {Math.Round(_state.Reputation, 1)}", $"Risk/Heat: {Math.Round(_state.RiskHeat, 1)}"));
        _financeMenu.Add(inject);
        _financeMenu.Add(withdraw);
    }

    private PaymentMode PickPaymentMode()
    {
        var day = DateTime.UtcNow.Day % 3;
        return day switch
        {
            0 => PaymentMode.PersonalOnly,
            1 => PaymentMode.CompanyOnly,
            _ => PaymentMode.Split
        };
    }
}
