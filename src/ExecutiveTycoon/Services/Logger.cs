using System;
using System.IO;

namespace ExecutiveTycoon.Services;

public sealed class Logger
{
    private readonly string _logFile;

    public Logger(string logFolder)
    {
        Directory.CreateDirectory(logFolder);
        _logFile = Path.Combine(logFolder, $"ExecutiveTycoon_{DateTime.UtcNow:yyyyMMdd}.log");
    }

    public void Info(string message) => Write("INFO", message);

    public void Warn(string message) => Write("WARN", message);

    public void Error(string message) => Write("ERROR", message);

    private void Write(string level, string message)
    {
        var line = $"[{DateTime.UtcNow:O}] [{level}] {message}{Environment.NewLine}";
        File.AppendAllText(_logFile, line);
    }
}
