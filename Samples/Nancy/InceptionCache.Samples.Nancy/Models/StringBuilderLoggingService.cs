using System;
using System.Text;
using SimpleLogging.Core;

namespace InceptionCache.Samples.Nancy.Models
{
    public class StringBuilderLoggingService : ILoggingService
    {
        private static readonly StringBuilder Log = new StringBuilder();

        public string GetLog
        {
            get
            {
                return Log.ToString();
            }
        }

        private static void AddToLog(string level, string message)
        {
            Log.AppendLine(string.Format("{0}|{1}|{2}{3}", DateTime.Now.ToShortTimeString(), level, message, "<br>"));
        }

        public void Trace(string message)
        {
            AddToLog("TRACE", message);
        }

        public void Trace(string message, params object[] args)
        {
            AddToLog("TRACE", string.Format(message, args));
        }

        public void Debug(string message)
        {
            AddToLog("DEBUG", message);
        }

        public void Debug(string message, params object[] args)
        {
            AddToLog("DEBUG", string.Format(message, args));
        }

        public void Info(string message)
        {
            AddToLog("INFO", message);
        }

        public void Info(string message, params object[] args)
        {
            AddToLog("INFO", string.Format(message, args));
        }

        public void Warning(string message)
        {
            AddToLog("WARNING", message);
        }

        public void Warning(string message, params object[] args)
        {
            AddToLog("WARNING", string.Format(message, args));
        }

        public void Error(string message)
        {
            AddToLog("ERROR", message);
        }

        public void Error(string message, params object[] args)
        {
            AddToLog("ERROR", string.Format(message, args));
        }

        public void Error(Exception exception, string message = null, bool isStackTraceIncluded = true)
        {
            AddToLog("ERROR", exception.Message);
        }

        public void Fatal(string message)
        {
            AddToLog("FATAL", message);
        }

        public void Fatal(string message, params object[] args)
        {
            AddToLog("TRACE", string.Format(message, args));
        }

        public void Fatal(Exception exception, string message = null, bool isStackTraceIncluded = true)
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            get
            {
                return "StringBuilderLoggingService";
            }
        }
    }
}