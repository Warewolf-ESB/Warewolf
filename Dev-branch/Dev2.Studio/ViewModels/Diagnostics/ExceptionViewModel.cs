using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Feedback;
using Dev2.Studio.Model;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using IWindowManager = Dev2.Studio.Core.Interfaces.IDev2WindowManager;

namespace Dev2.Studio.ViewModels.Diagnostics
{
    /// <summary>
    /// Used to display a user-friendly exceptionmessage, and allow the user to send a report via email
    /// </summary>
    /// <author>jurie.smit</author>
    /// <date>2013/01/15</date>
    public sealed class ExceptionViewModel : SimpleBaseViewModel, IExceptionViewModel
    {
        #region private fields
        private BindableCollection<ExceptionUIModel> _exception;
        private string _stackTrace;
        #endregion

        #region Constructor

        public ExceptionViewModel()
        {
            WindowNavigation = ImportService.GetExportValue<IWindowManager>();
            FeedbackInvoker = ImportService.GetExportValue<IFeedbackInvoker>();
        }

        #endregion Constructor

        #region public properties
        public IWindowManager WindowNavigation { get; set; }

        public IFeedbackInvoker FeedbackInvoker { get; set; }

        public IFeedbackAction FeedbackAction { get; set; }

        public string OutputText { get; set; }

        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets the exception ui wrapper, using a collection to bind a treeview to it.
        /// </summary>
        /// <value>
        /// The exception UI Model, wrapping an actual exception.
        /// </value>
        /// <author>jurie.smit</author>
        /// <date>2013/01/15</date>
        public BindableCollection<ExceptionUIModel> Exception
        {
            get
            {
                return _exception ?? (_exception = new BindableCollection<ExceptionUIModel>());
            }
            set
            {
                _exception = value;
                NotifyOfPropertyChange(() => Exception);
            }
        }

        public string StackTrace
        {
            get
            {
                return _stackTrace;
            }
            set
            {
                if (_stackTrace == value) return;

                _stackTrace = value;
                NotifyOfPropertyChange(() => StackTrace);
            }
        }

        public BitmapSource ErrorIcon
        {
            get
            {
                //Just wraps the default errorIcon in a BitMapSource
                return Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Error.Handle, Int32Rect.Empty,
                                                                BitmapSizeOptions.FromEmptyOptions());
            }
        }
        #endregion

        #region public methods
        /// <summary>
        /// Shows this instance.
        /// </summary>
        /// <author>jurie.smit</author>
        /// <date>2013/01/15</date>
        public void Show()
        {
            WindowNavigation.ShowDialog(this);
        }

        /// <summary>
        /// Sends the report using the imported feedbackinvoker (.
        /// </summary>
        /// <author>jurie.smit</author>
        /// <date>2013/01/15</date>
        public void SendReport()
        {
            FileHelper.CreateTextFile(OutputText, OutputPath);         
            FeedbackInvoker.InvokeFeedback(FeedbackAction);
            RequestClose();
        }
        #endregion
    }
}
