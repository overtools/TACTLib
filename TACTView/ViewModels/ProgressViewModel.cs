using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using JetBrains.Annotations;
using TACTView.Api;

namespace TACTView.ViewModels {
    public sealed class ProgressViewModel : INotifyPropertyChanged, IProgressReporter {
        public ProgressViewModel() {
            Timer.AutoReset = false;
            Timer.Elapsed += Update;
            Timer.Interval = 100;
            Timer.Start();
        }

        public int Value { get; set; }
        public double Percent => 100.0 * Value / (Maximum - Minimum);

        public bool InvalidValue => Value < Minimum;

        private Timer Timer { get; } = new();
        public event PropertyChangedEventHandler? PropertyChanged;
        public int Maximum { get; set; }
        public int Minimum { get; set; }
        public string Status { get; set; } = "Idle";

        public void Report(int value, int? max = null, int? min = null, string? status = null) {
            if (!Application.Current.CheckAccess()) {
                Application.Current.Dispatcher.Invoke((ReportDelegate) Report, DispatcherPriority.Normal, value, max, min, status);
                return;
            }

            if (max.HasValue && max != Maximum) Maximum = max.Value;

            if (min.HasValue && min != Minimum) Minimum = min.Value;

            Value = value;

            if (status != Status) Status = status ?? "Working...";

            if (Value == Minimum || InvalidValue) Update();
        }

        private void Update(object sender, ElapsedEventArgs e) {
            if (Value == Minimum || InvalidValue) return;

            Update();
        }

        private void Update() {
            Timer.Stop();
            OnPropertyChanged(nameof(Maximum));
            OnPropertyChanged(nameof(Minimum));
            OnPropertyChanged(nameof(Value));
            if (Percent > 1) OnPropertyChanged(nameof(Percent));
            OnPropertyChanged(nameof(InvalidValue));
            OnPropertyChanged(nameof(Status));
            Timer.Start();
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private delegate void ReportDelegate(int value, int? max = null, int? min = null, string? status = null);
    }
}
