using Caliburn.Micro;
using Dev2.Studio.Feedback;
using Dev2.Studio.Model;

namespace Dev2.Studio.ViewModels.Diagnostics
{
    public interface IExceptionViewModel
    {
        string OutputText { get; set; }
        string OutputPath { get; set; }
        string ServerLogTempPath { get; set; }
        string StudioLogTempPath { get; set; }
        IWindowManager WindowNavigation { get; set; }
        IFeedbackInvoker FeedbackInvoker { get; set; }
        BindableCollection<ExceptionUiModel> Exception { get; set; }
        string StackTrace { get; set; }
        void Show();
        void SendReport();
        bool Critical { get; set; }
    }
}
