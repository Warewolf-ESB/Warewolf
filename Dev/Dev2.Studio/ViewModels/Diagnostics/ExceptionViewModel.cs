
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Feedback;
using Dev2.Studio.Model;

// ReSharper disable CheckNamespace
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
        private BindableCollection<ExceptionUiModel> _exception;
        private RelayCommand _cancelComand;
        private string _stackTrace;

        #endregion

        #region Constructor

        public ExceptionViewModel()
        {
            //MEF!!!
            WindowNavigation = CustomContainer.Get<IWindowManager>();
            FeedbackInvoker = CustomContainer.Get<IFeedbackInvoker>();
        }

        #endregion Constructor

        #region public properties
        public IWindowManager WindowNavigation { get; set; }

        public IFeedbackInvoker FeedbackInvoker { get; set; }

        public IFeedbackAction FeedbackAction { get; set; }

        public string OutputText { get; set; }

        public string OutputPath { get; set; }

        public string ServerLogTempPath { get; set; }

        public string StudioLogTempPath { get; set; }

        /// <summary>
        /// Gets or sets the exception ui wrapper, using a collection to bind a treeview to it.
        /// </summary>
        /// <value>
        /// The exception UI Model, wrapping an actual exception.
        /// </value>
        /// <author>jurie.smit</author>
        /// <date>2013/01/15</date>
        public BindableCollection<ExceptionUiModel> Exception
        {
            get
            {
                return _exception ?? (_exception = new BindableCollection<ExceptionUiModel>());
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
                if(_stackTrace == value) return;

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

        public bool Critical { get; set; }

        #endregion

        public ICommand CancelCommand
        {
            get
            {
                return _cancelComand ?? (_cancelComand = new RelayCommand(param => Cancel(), param => true));
            }
        }

        #region public methods

        public void Cancel()
        {
            RequestClose();
        }

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
            var path = new FileInfo(OutputPath);
            if(!Critical)
            {
                FileHelper.CreateTextFile(OutputText, OutputPath);
            }
            else
            {
                //2013.06.27: Ashley Lewis for bug 9817 - critical exceptions hang open during feedback (they close themselves) file may exist
                if(path.Directory != null && !path.Directory.Exists)
                {
                    FileHelper.CreateTextFile(OutputText, OutputPath);
                }
            }
            FeedbackInvoker.InvokeFeedback(FeedbackAction);
            //2013.06.27: Ashley Lewis for bug 9817 - don't close critical exception messages (they close themselves)
            if(!Critical)
            {
                RequestClose();
            }
        }
        #endregion
    }
}
