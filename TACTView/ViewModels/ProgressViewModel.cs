using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TACTView.Api;

namespace TACTView.ViewModels {
    public sealed class ProgressViewModel : INotifyPropertyChanged, IProgressReporter {
        public int Value { get; set; }
        public double Percent => 100.0 * Value / (Maximum - Minimum);
        public event PropertyChangedEventHandler? PropertyChanged;
        public int Maximum { get; set; }
        public int Minimum { get; set; }
        public string Status { get; set; } = "Idle";

        public void Report(int value, int? max = null, int? min = null, string? status = null) {
            if (max.HasValue) {
                Maximum = max.Value;
                OnPropertyChanged(nameof(Maximum));
            }

            if (min.HasValue) {
                Minimum = min.Value;
                OnPropertyChanged(nameof(Minimum));
            }

            Value = value;
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(Percent));

            if (status == Status) return;

            Status = status ?? "Working...";
            OnPropertyChanged(nameof(Status));
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
