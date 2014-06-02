
namespace Dev2.CustomControls.Progress
{
    public interface IProgressDialog : IProgressNotifier
    {
        string Label { get; set; }
        string SubLabel { get; set; }
        double ProgressValue { get; set; }
        bool IsCancelButtonEnabled { get; set; }
    }

    public interface IProgressNotifier
    {
        void StartCancel();
        void Show();
        void Close();
        void StatusChanged(string fileName, int progressPercent, long totalBytes);
    }

}