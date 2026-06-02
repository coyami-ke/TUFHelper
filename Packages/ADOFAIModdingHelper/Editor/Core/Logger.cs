using System;
using System.IO;
using UnityEngine;
using ADOFAIModdingHelper.Common;

namespace ADOFAIModdingHelper.Core
{
    public static class Logger
    {
        private static readonly string logFilePath;

        static Logger()
        {
            if (!Directory.Exists(Constants.LogFolder))
                Directory.CreateDirectory(Constants.LogFolder);

            logFilePath = Path.Combine(Constants.LogFolder, "Log.txt");

            Application.logMessageReceived += HandleLog;
        }
        public static void Init() { }
        private static void HandleLog(string logString, string stackTrace, LogType type)
        {
            try
            {
                string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{type}] {logString}";
                File.AppendAllText(logFilePath, line + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Debug.LogError("Logger failed to write log: " + ex.Message);
            }
        }

        public static void Clear()
        {
            if (File.Exists(logFilePath))
                File.WriteAllText(logFilePath, string.Empty);

            Debug.Log("Log file cleared: " + logFilePath);
        }
    }
}