using System;
using System.IO;
using System.Text;

namespace Extension4WithVSSDK
{
    public sealed class SimpleFileLogger
    {
        private const string LogDirectory = @"C:\AppHostedServices";
        private const string LogFileName = "extension-log.txt";
        private static readonly object Sync = new object();
        private readonly string _logFilePath;

        public SimpleFileLogger()
        {
            Directory.CreateDirectory(LogDirectory);
            _logFilePath = Path.Combine(LogDirectory, LogFileName);
        }

        public void Log(string message)
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}";

            lock (Sync)
            {
                File.AppendAllText(_logFilePath, line, Encoding.UTF8);
            }
        }
    }
}