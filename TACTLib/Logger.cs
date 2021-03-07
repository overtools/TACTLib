using System;
using System.Diagnostics;

namespace TACTLib {
    public static class Logger {
        public delegate void LogEvent(string category, string message);

        public static event LogEvent OnInfo;
        public static event LogEvent OnDebug;
        public static event LogEvent OnWarn;
        public static event LogEvent OnError;
        public static bool Registered { get; set; } = false;
        
        public static void Info(string category, string message) {
            OnInfo?.Invoke(category, message);
        }
        
        public static void Debug(string category, string message) {
            OnDebug?.Invoke(category, message);
        }
        
        public static void Warn(string category, string message) {
            OnWarn?.Invoke(category, message);
        }
        
        public static void Error(string category, string message) {
            OnError?.Invoke(category, message);
        }
        
        public static void RegisterBasic() {
            if (Registered) return;
            Registered = true;
            OnInfo += LogBasic;
            OnDebug += LogBasic;
            OnWarn += LogBasic;
            OnError += LogBasic;
        }

        private static void LogBasic(string category, string message) {
            Console.Out.WriteLine($"[{category}] {message}");
        #if DEBUG
            if (Debugger.IsAttached && Debugger.IsLogging()) {
                Debugger.Log(4, category, message);
            }
        #endif
        }
    }
}