using System;
using System.Windows.Forms;
using GTA;
using GTA.UI;
using NativeUI;

namespace CEOSimulatorSP
{
    /// <summary>
    /// Script entry point for CEOSimulatorSP.
    /// </summary>
    public class Main : Script
    {
        private const string ScriptName = "CEOSimulatorSP";
        private readonly Logger _logger;
        private readonly ConfigManager _configManager;
        private readonly SaveManager _saveManager;
        private readonly EventManager _eventManager;
        private readonly DecisionManager _decisionManager;
        private readonly OfficeManager _officeManager;
        private readonly BoardMeetingManager _boardMeetingManager;
        private readonly WorldReactionManager _worldReactionManager;
        private readonly UIManager _uiManager;
        private SaveData _saveData;
        private DateTime _lastTickLog;

        public Main()
        {
            _logger = new Logger(ScriptName);
            _configManager = new ConfigManager(_logger);
            _saveManager = new SaveManager(_logger);
            _eventManager = new EventManager(_logger);
            _decisionManager = new DecisionManager(_logger, _eventManager);

            _saveData = _saveManager.Load();
            var config = _configManager.Load();

            if (_saveData.ActiveEvents.Count == 0)
            {
                _saveData.ActiveEvents.AddRange(_eventManager.GenerateDailyEvents(_saveData.Stats, _saveData.Day));
            }

            _officeManager = new OfficeManager(config);
            _boardMeetingManager = new BoardMeetingManager(_logger, config, _saveData);
            _worldReactionManager = new WorldReactionManager(_logger, config, _saveData);
            _uiManager = new UIManager(
                config,
                _saveData,
                _eventManager,
                _decisionManager,
                _boardMeetingManager,
                _worldReactionManager,
                _saveManager);

            Tick += OnTick;
            KeyDown += OnKeyDown;
            Aborted += OnAborted;

            Interval = 250;
        }

        private void OnTick(object sender, EventArgs e)
        {
            _officeManager.DrawMarkers();
            _uiManager.Process();
            _worldReactionManager.Update(_saveData.Stats);

            if ((DateTime.UtcNow - _lastTickLog).TotalSeconds > 30)
            {
                _lastTickLog = DateTime.UtcNow;
                _logger.Info("Tick heartbeat.");
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            var config = _configManager.CurrentConfig;
            if (e.KeyCode == config.Keybinds.OpenMenu)
            {
                _uiManager.ShowDashboard();
                return;
            }

            if (e.KeyCode != config.Keybinds.Interact)
            {
                return;
            }

            var interaction = _officeManager.GetInteractionTarget(Game.Player.Character.Position);
            switch (interaction)
            {
                case OfficeInteraction.Desk:
                    _uiManager.ShowDashboard();
                    break;
                case OfficeInteraction.Assistant:
                    _uiManager.ShowAssistantBriefing();
                    break;
                case OfficeInteraction.Boardroom:
                    if (_boardMeetingManager.CanStartMeeting(_saveData))
                    {
                        _uiManager.ShowBoardMeeting();
                    }
                    else
                    {
                        Screen.ShowNotification("~y~Board meeting is not available yet.");
                    }
                    break;
            }
        }

        private void OnAborted(object sender, EventArgs e)
        {
            _logger.Info("Script aborted. Saving state.");
            _saveManager.Save(_saveData);
        }
    }
}
