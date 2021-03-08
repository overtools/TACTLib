using JetBrains.Annotations;

namespace TACTView.Api {
    [PublicAPI]
    public interface IProgressReporter {
        int Maximum { get; set; }
        int Minimum { get; set; }
        string Status { get; set; }
        void Report(int value, int? max = null, int? min = null, string? status = null);
    }
}
