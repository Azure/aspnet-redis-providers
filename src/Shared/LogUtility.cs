using System.Diagnostics;

namespace Microsoft.Web.Redis
{
    internal static class LogUtility
    {
        public static void LogWarning(string msg, params object[] args)
        {
            string msgToPrint = (args.Length > 0) ? string.Format(msg, args) : msg;
            Trace.TraceWarning(msgToPrint);
        }

        public static void LogError(string msg, params object[] args)
        {
            string msgToPrint = (args.Length > 0) ? string.Format(msg, args) : msg;
            Trace.TraceError(msgToPrint);
        }

        public static void LogInfo(string msg, params object[] args)
        {
            string msgToPrint = (args.Length > 0) ? string.Format(msg, args) : msg;
            Trace.WriteLine(msgToPrint);
        }
    } 
}
