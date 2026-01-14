using System;
using System.IO;

namespace CEOSimulatorSP
{
    /// <summary>
    /// Simple file logger for the mod.
    /// </summary>
    public class Logger
    {
        private readonly string _logPath;

        public Logger(string scriptName)
        {
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", scriptName);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            _logPath = Path.Combine(folder, "log.txt");
        }

        public void Info(string message)
        {
            Write("INFO", message);
        }

        public void Warn(string message)
        {
            Write("WARN", message);
        }

        public void Error(string message, Exception ex = null)
        {
            var detail = ex == null ? message : $"{message} | {ex}";
            Write("ERROR", detail);
        }

        private void Write(string level, string message)
        {
            try
            {
                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {level}: {message}";
                File.AppendAllLines(_logPath, new[] { line });
            }
            catch
            {
                // Ignore logging failures to avoid crashing the script.
            }
        }
    }
}
