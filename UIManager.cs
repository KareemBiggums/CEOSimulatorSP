using System;
using System.Collections.Generic;
using System.Linq;
using GTA.UI;
using NativeUI;

namespace CEOSimulatorSP
{
    /// <summary>
    /// Handles NativeUI menus and interactions.
    /// </summary>
    public class UIManager
    {
        private readonly ConfigData _config;
        private readonly SaveData _saveData;
        private readonly EventManager _eventManager;
        private readonly DecisionManager _decisionManager;
        private readonly BoardMeetingManager _boardMeetingManager;
        private readonly WorldReactionManager _worldReactionManager;
        private readonly SaveManager _saveManager;

        private MenuPool _menuPool;
        private UIMenu _dashboardMenu;
        private UIMenu _statsMenu;
        private UIMenu _eventsMenu;
        private UIMenu _financialMenu;
        private UIMenu _eventDetailMenu;
        private UIMenu _assistantMenu;
        private UIMenu _boardMenu;

        public UIManager(
            ConfigData config,
            SaveData saveData,
            EventManager eventManager,
            DecisionManager decisionManager,
            BoardMeetingManager boardMeetingManager,
            WorldReactionManager worldReactionManager,
            SaveManager saveManager)
        {
            _config = config;
            _saveData = saveData;
            _eventManager = eventManager;
            _decisionManager = decisionManager;
            _boardMeetingManager = boardMeetingManager;
            _worldReactionManager = worldReactionManager;
            _saveManager = saveManager;

            BuildMenus();
        }

        public void Process()
        {
            _menuPool?.ProcessMenus();
        }

        public void ShowDashboard()
        {
            RefreshDashboard();
            _dashboardMenu.Visible = true;
        }

        public void ShowAssistantBriefing()
        {
            RefreshAssistantBriefing();
            _assistantMenu.Visible = true;
        }

        public void ShowBoardMeeting()
        {
            RefreshBoardMeeting();
            _boardMenu.Visible = true;
        }

        private void BuildMenus()
        {
            _menuPool = new MenuPool();

            _dashboardMenu = new UIMenu("CEO Dashboard", "Corporate Overview");
            _menuPool.Add(_dashboardMenu);

            _statsMenu = _menuPool.AddSubMenu(_dashboardMenu, "Stats", "Review current CEO metrics.");
            _eventsMenu = _menuPool.AddSubMenu(_dashboardMenu, "Active Events", "Review and resolve events.");
            _financialMenu = _menuPool.AddSubMenu(_dashboardMenu, "Financial Overview", "Cash, stock value, and overhead.");
            _eventDetailMenu = new UIMenu("Event Detail", "Choose a response");
            _menuPool.Add(_eventDetailMenu);

            var endDayItem = new UIMenuItem("End Day", "Advance the corporate day and generate new events.");
            endDayItem.Activated += (sender, item) => EndDay();
            _dashboardMenu.AddItem(endDayItem);

            var saveItem = new UIMenuItem("Save Now", "Write a manual save to disk.");
            saveItem.Activated += (sender, item) =>
            {
                _saveManager.Save(_saveData);
                Screen.ShowNotification("~g~Save complete.");
            };
            _dashboardMenu.AddItem(saveItem);

            BuildAssistantMenu();
            BuildBoardMenu();

            RefreshDashboard();
        }

        private void BuildAssistantMenu()
        {
            _assistantMenu = new UIMenu("Assistant Briefing", "Daily Briefing");
            _menuPool.Add(_assistantMenu);
        }

        private void BuildBoardMenu()
        {
            _boardMenu = new UIMenu("Board Meeting", "Executive Session");
            _menuPool.Add(_boardMenu);

            var performanceItem = new UIMenuItem("Performance Review", "Discuss recent company performance.");
            var voteItem = new UIMenuItem("Vote of Confidence", "Gauge board confidence.");
            var strategyItem = new UIMenuItem("Strategic Direction", "Align on next steps.");

            performanceItem.Activated += (sender, item) => ExecuteBoardMeeting();
            voteItem.Activated += (sender, item) => ExecuteBoardMeeting();
            strategyItem.Activated += (sender, item) => ExecuteBoardMeeting();

            _boardMenu.AddItem(performanceItem);
            _boardMenu.AddItem(voteItem);
            _boardMenu.AddItem(strategyItem);
        }

        private void RefreshDashboard()
        {
            RefreshStatsMenu();
            RefreshEventsMenu();
            RefreshFinancialMenu();
        }

        private void RefreshStatsMenu()
        {
            _statsMenu.Clear();
            var stats = _saveData.Stats;
            _statsMenu.AddItem(new UIMenuItem($"Power: {stats.Power}"));
            _statsMenu.AddItem(new UIMenuItem($"Public Reputation: {stats.PublicReputation}"));
            _statsMenu.AddItem(new UIMenuItem($"Board Approval: {stats.BoardApproval}"));
            _statsMenu.AddItem(new UIMenuItem($"Employee Morale: {stats.EmployeeMorale}"));
            _statsMenu.AddItem(new UIMenuItem($"Risk Level: {stats.RiskLevel}"));
            _statsMenu.AddItem(new UIMenuItem($"Stock Value: {stats.StockValue:0.0}"));
            _statsMenu.AddItem(new UIMenuItem($"Cash: ${stats.Cash:n0}"));
        }

        private void RefreshFinancialMenu()
        {
            _financialMenu.Clear();
            _financialMenu.AddItem(new UIMenuItem($"Cash Reserves: ${_saveData.Stats.Cash:n0}"));
            _financialMenu.AddItem(new UIMenuItem($"Stock Value: {_saveData.Stats.StockValue:0.0}"));
            _financialMenu.AddItem(new UIMenuItem($"Daily Overhead: ${_config.Tuning.DailyOverhead:n0}"));
        }

        private void RefreshEventsMenu()
        {
            _eventsMenu.Clear();
            if (_saveData.ActiveEvents.Count == 0)
            {
                _eventsMenu.AddItem(new UIMenuItem("No active events", "End the day to generate more events."));
                return;
            }

            foreach (var corporateEvent in _saveData.ActiveEvents.ToList())
            {
                var eventItem = new UIMenuItem(corporateEvent.Title, corporateEvent.Description);
                eventItem.Activated += (sender, item) => ShowEventDetail(corporateEvent);
                _eventsMenu.AddItem(eventItem);
            }
        }

        private void RefreshAssistantBriefing()
        {
            _assistantMenu.Clear();
            _assistantMenu.AddItem(new UIMenuItem($"Day {_saveData.Day}", "Current corporate day."));
            var daysUntilMeeting = Math.Max(0, _config.Tuning.BoardMeetingIntervalDays - (_saveData.Day - _saveData.LastBoardMeetingDay));
            _assistantMenu.AddItem(new UIMenuItem($"Next board meeting in {daysUntilMeeting} days", "Schedule overview."));

            if (_saveData.ActiveEvents.Count == 0)
            {
                _assistantMenu.AddItem(new UIMenuItem("No active priorities", "End the day to generate new events."));
            }
            else
            {
                foreach (var corporateEvent in _saveData.ActiveEvents)
                {
                    _assistantMenu.AddItem(new UIMenuItem($"Priority: {corporateEvent.Title}", corporateEvent.Description));
                }
            }

            var warnings = new List<string>();
            if (_saveData.Stats.RiskLevel > 70)
            {
                warnings.Add("Risk level is high.");
            }

            if (_saveData.Stats.BoardApproval < 35)
            {
                warnings.Add("Board approval is slipping.");
            }

            if (_saveData.Stats.EmployeeMorale < 35)
            {
                warnings.Add("Employee morale is low.");
            }

            if (warnings.Count > 0)
            {
                foreach (var warning in warnings)
                {
                    _assistantMenu.AddItem(new UIMenuItem($"Warning: {warning}"));
                }
            }

            if (_saveData.TerminationThreatActive)
            {
                _assistantMenu.AddItem(new UIMenuItem("Alert: Board has issued a termination threat."));
            }
        }

        private void RefreshBoardMeeting()
        {
            _boardMenu.Subtitle.Caption = _saveData.TerminationThreatActive
                ? "Board is on edge - recovery required"
                : "Executive Session";
        }

        private void EndDay()
        {
            _saveData.Day += 1;
            _saveData.Stats.Cash -= _config.Tuning.DailyOverhead;
            _saveData.Stats.ClampAll();

            _decisionManager.ApplyPendingEvents(_saveData);

            var newEvents = _eventManager.GenerateDailyEvents(_saveData.Stats, _saveData.Day);
            _saveData.ActiveEvents.AddRange(newEvents);

            _worldReactionManager.HandleDayStart(_saveData.Stats);

            _saveManager.Save(_saveData);
            Screen.ShowNotification("~g~Day advanced. New events have arrived.");
            RefreshDashboard();
        }

        private void ExecuteBoardMeeting()
        {
            if (!_boardMeetingManager.CanStartMeeting(_saveData))
            {
                Screen.ShowNotification("~y~Board meeting is not scheduled today.");
                return;
            }

            var outcome = _boardMeetingManager.RunMeeting(_saveData);
            var message = outcome switch
            {
                BoardMeetingOutcome.StrongSupport => "Board response: Strong Support.",
                BoardMeetingOutcome.Mixed => "Board response: Mixed but stable.",
                BoardMeetingOutcome.Warning => "Board response: Warning issued.",
                BoardMeetingOutcome.Crisis => "Board response: Crisis and termination threat.",
                _ => "Board meeting concluded."
            };

            Screen.ShowNotification(message);
            if (_saveData.BoardMeetingCooldownUntilDay > _saveData.Day)
            {
                Screen.ShowNotification("~r~Removed as CEO. Board access locked temporarily.");
            }

            _saveManager.Save(_saveData);
            RefreshDashboard();
        }

        private void ShowEventDetail(CorporateEvent corporateEvent)
        {
            _eventDetailMenu.Clear();
            _eventDetailMenu.Subtitle.Caption = corporateEvent.Title;
            _eventDetailMenu.AddItem(new UIMenuItem($"Category: {corporateEvent.Category}"));
            _eventDetailMenu.AddItem(new UIMenuItem($"Severity: {corporateEvent.Severity}"));
            _eventDetailMenu.AddItem(new UIMenuItem(corporateEvent.Description));

            foreach (var choice in corporateEvent.Choices)
            {
                var choiceItem = new UIMenuItem(choice.Label, choice.OutcomeText);
                choiceItem.Activated += (sender, item) =>
                {
                    _decisionManager.ApplyChoice(_saveData, corporateEvent, choice);
                    Screen.ShowNotification(choice.OutcomeText);
                    RefreshDashboard();
                    _saveManager.Save(_saveData);
                    _eventDetailMenu.Visible = false;
                };
                _eventDetailMenu.AddItem(choiceItem);
            }

            _eventDetailMenu.Visible = true;
        }
    }
}
