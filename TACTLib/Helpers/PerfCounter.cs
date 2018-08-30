using System;
using System.Diagnostics;

namespace TACTLib.Helpers {
    /// <summary>Performance tracker</summary>
    public sealed class PerfCounter : IDisposable {
        private readonly Stopwatch _sw;
        private readonly string _name;

        public PerfCounter(string name) {
            _name = name;
            _sw = Stopwatch.StartNew();
        }

        public void Dispose() {
            _sw.Stop();
            //Console.Out.WriteLine($"[{_name}] {_sw.ElapsedMilliseconds}ms {_sw.ElapsedTicks}t");
            if (Debugger.IsAttached) {
                Debugger.Log(1, "Perf", $"[{_name}] {_sw.ElapsedMilliseconds}ms {_sw.ElapsedTicks}t\n");
            }
        }
    }
}