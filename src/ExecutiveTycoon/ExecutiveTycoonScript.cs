using System;
using ExecutiveTycoon.Models;
using ExecutiveTycoon.Services;
using ExecutiveTycoon.UI;
using GTA;
using GTA.Math;
using GTA.UI;

namespace ExecutiveTycoon;

public sealed class ExecutiveTycoonScript : Script
{
    private readonly ExecutiveConfig _config;
    private readonly Logger _logger;
    private readonly SaveService _saveService;
    private readonly CompanyState _state;
    private readonly OfficeManager _officeManager;
    private readonly SimulationEngine _simulation;
    private readonly UiController _ui;

    private DateTime _nextTickUtc;
    private DateTime _nextBusinessDayUtc;
    private DateTime _nextAutoSaveUtc;

    public ExecutiveTycoonScript()
    {
        var iniPath = "scripts/ExecutiveTycoon.ini";
        var bootstrap = ExecutiveConfig.Load(iniPath);
        bootstrap.WriteDefaultIniIfMissing(iniPath);

        _config = ExecutiveConfig.Load(iniPath);
        _logger = new Logger(_config.LogFolderPath);
        _saveService = new SaveService(_config.SaveFilePath, _logger);
        _state = _saveService.LoadOrCreateDefault();

        _officeManager = new OfficeManager(_state);
        _simulation = new SimulationEngine(_state, _officeManager, _logger);
        _ui = new UiController(_state, _officeManager, _simulation, _saveService, _logger);

        var phone = new PhoneContactService(_logger);
        phone.TryRegisterExecutiveBrokerContact();

        Tick += OnTick;
        KeyDown += OnKeyDown;
        Aborted += OnAborted;

        Interval = 100;
        _nextTickUtc = DateTime.UtcNow;
        _nextBusinessDayUtc = DateTime.UtcNow.AddMinutes(_config.MinutesPerBusinessDay);
        _nextAutoSaveUtc = DateTime.UtcNow.AddSeconds(_config.AutoSaveSeconds);

        _logger.Info("ExecutiveTycoon initialized.");
        Screen.ShowNotification("~b~ExecutiveTycoon loaded. Use office desktop (E) or F7 overlay.");
    }

    private void OnTick(object sender, EventArgs e)
    {
        _ui.Process();

        if (DateTime.UtcNow >= _nextTickUtc)
        {
            _nextTickUtc = DateTime.UtcNow.AddSeconds(_config.TickSeconds);
        }

        if (DateTime.UtcNow >= _nextBusinessDayUtc)
        {
            _nextBusinessDayUtc = DateTime.UtcNow.AddMinutes(_config.MinutesPerBusinessDay);
            var summary = _simulation.RunBusinessDay();
            _ui.SetLatestSummary(summary);
            Screen.ShowNotification($"~b~ExecutiveTycoon~s~ {summary}");

            if (_state.BusinessDaysElapsed > 0 && _state.BusinessDaysElapsed % 7 == 0)
            {
                Screen.ShowNotification($"~g~{_simulation.BuildWeeklyReport()}");
            }
        }

        if (DateTime.UtcNow >= _nextAutoSaveUtc)
        {
            _nextAutoSaveUtc = DateTime.UtcNow.AddSeconds(_config.AutoSaveSeconds);
            _saveService.Save(_state);
        }

        TryHandleDesktopInteraction();
    }

    private void TryHandleDesktopInteraction()
    {
        var activeHq = _officeManager.GetActiveHqDefinition();
        if (activeHq == null)
        {
            return;
        }

        var playerPos = Game.Player.Character.Position;
        var distance = playerPos.DistanceTo(activeHq.DesktopPosition);

        if (distance < 25f)
        {
            World.DrawMarker(MarkerType.VerticalCylinder, activeHq.DesktopPosition - new Vector3(0f, 0f, 1f), Vector3.Zero, Vector3.Zero, new Vector3(0.5f, 0.5f, 0.5f), System.Drawing.Color.FromArgb(180, 40, 120, 220));
        }

        if (distance < 1.5f)
        {
            Screen.ShowHelpTextThisFrame("Press ~INPUT_CONTEXT~ to access Executive Desktop");
            if (Game.IsControlJustPressed(0, (Control)_config.DashboardKey) && !_ui.IsAnyMenuOpen)
            {
                _ui.OpenDashboard();
            }
        }
    }

    private void OnKeyDown(object sender, GTA.KeyEventArgs e)
    {
        if (e.KeyCode == (System.Windows.Forms.Keys)_config.QuickOverlayKey)
        {
            _ui.ShowQuickOverlay();
        }
    }

    private void OnAborted(object sender, EventArgs e)
    {
        _saveService.Save(_state);
    }
}
