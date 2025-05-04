using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace EmailAccountManager
{
    public static class Logger
    {
        private static readonly string logFilePath = Path.Combine(AsmUtility.GetAssemblyDirectoryName(), "error.log");


        public static void LogError(string message, Exception exception = null)
        {
            Log("ERROR", message, exception);
        }

        public static void LogInfo(string message)
        {
            Log("INFO", message);
        }

        private static void Log(string logLevel, string message, Exception exception = null)
        {
            try
            {
                var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {message}";

                if (exception != null)
                {
                    logMessage += Environment.NewLine + exception.ToString();
                }

                File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show("failed to write log" + ex.Message);
            }
        }
    }
}
