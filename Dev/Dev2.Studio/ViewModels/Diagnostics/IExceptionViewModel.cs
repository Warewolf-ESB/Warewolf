using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Dev2.Studio.Feedback;
using Dev2.Studio.Model;
using IWindowManager = Dev2.Studio.Core.Interfaces.IDev2WindowManager;

namespace Dev2.Studio.ViewModels.Diagnostics
{
    public interface IExceptionViewModel
    {
        string OutputText { get; set; }
        IWindowManager WindowNavigation { get; set; }
        IFeedbackInvoker FeedbackInvoker { get; set; }
        BindableCollection<ExceptionUIModel> Exception { get; set; }
        string StackTrace { get; set; }
        void Show();
        void SendReport();
    }
}
