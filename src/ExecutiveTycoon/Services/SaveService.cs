using System;
using System.IO;
using ExecutiveTycoon.Models;
using Newtonsoft.Json;

namespace ExecutiveTycoon.Services;

public sealed class SaveService
{
    private readonly string _saveFilePath;
    private readonly Logger _logger;

    public SaveService(string saveFilePath, Logger logger)
    {
        _saveFilePath = saveFilePath;
        _logger = logger;
    }

    public CompanyState LoadOrCreateDefault()
    {
        try
        {
            if (!File.Exists(_saveFilePath))
            {
                var state = CreateDefaultState();
                Save(state);
                return state;
            }

            var json = File.ReadAllText(_saveFilePath);
            var loaded = JsonConvert.DeserializeObject<CompanyState>(json);
            if (loaded == null)
            {
                _logger.Warn("Save file was empty/invalid. Creating default state.");
                loaded = CreateDefaultState();
            }

            loaded.Managers ??= new();
            loaded.Offices ??= new();
            loaded.ActiveDirectives ??= new();
            return loaded;
        }
        catch (Exception ex)
        {
            _logger.Error($"Load failed: {ex}");
            return CreateDefaultState();
        }
    }

    public void Save(CompanyState state)
    {
        try
        {
            state.LastSaveUtc = DateTime.UtcNow;
            var json = JsonConvert.SerializeObject(state, Formatting.Indented);
            File.WriteAllText(_saveFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.Error($"Save failed: {ex}");
        }
    }

    private static CompanyState CreateDefaultState()
    {
        return new CompanyState();
    }
}
