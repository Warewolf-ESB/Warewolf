
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using Dev2.Common;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Services.Communication;
using Dev2.Studio.Core.Services.System;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Utils;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Help
{
    /// <summary>
    /// Viewmodel for the feedbackView - Basics for sending an email
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <datetime>2013/01/14-09:12 AM</datetime>
    public sealed class FeedbackViewModel : SimpleBaseViewModel
    {
        #region private fields
        private ICommand _sendCommand;
        private ICommand _cancelCommand;
        private ICommand _openRecordingAttachmentFolderCommand;
        private ICommand _openServerLogAttachmentFolderCommand;
        private ICommand _openStudioLogAttachmentFolderCommand;
        private string _comment;
        private OptomizedObservableCollection<string> _categories;
        private string _selectedCategory;
        private bool _updateCaretPosition;
        private string _recordingAttachmentPath;
        private string _serverlogAttachmentPath;
        private string _studiologAttachmentPath;
        #endregion

        #region ctors and init

        public Func<string, bool> DoesFileExists = fileName => File.Exists(fileName);

        public FeedbackViewModel()
            : this(new Dictionary<string, string>())
        {
        }

        public FeedbackViewModel(Dictionary<string, string> attachedFiles)
        {
            SysInfoService = CustomContainer.Get<ISystemInfoService>();

            var sysInfo = SysInfoService.GetSystemInfo();
            Init(sysInfo, attachedFiles);
            SelectedCategory = "Feedback";
            DisplayName = "Feedback";
            BrowserPopupController = new ExternalBrowserPopupController();
        }

        /// <summary>
        /// Inits the viewmodel with the specified sys info, and setups the default categories.
        /// </summary>
        /// <param name="sysInfo">The sys info.</param>
        /// <param name="attachedFiles">path to the attachment</param>
        /// <author>Jurie.smit</author>
        /// <datetime>2013/01/14-09:19 AM</datetime>
        private void Init(SystemInfoTO sysInfo, Dictionary<string, string> attachedFiles)
        {
            Comment = null;

            ServerLogAttachmentPath = attachedFiles.Where(f => f.Key.Equals("ServerLog", StringComparison.CurrentCulture)).Select(v => v.Value).SingleOrDefault();
            StudioLogAttachmentPath = attachedFiles.Where(f => f.Key.Equals("StudioLog", StringComparison.CurrentCulture)).Select(v => v.Value).SingleOrDefault();
            RecordingAttachmentPath = attachedFiles.Where(f => f.Key.Equals("RecordingLog", StringComparison.CurrentCulture)).Select(v => v.Value).SingleOrDefault();

            Comment = GenerateDefaultComment(sysInfo);
            SetCaretPosition();
            Categories.AddRange(new List<string> { "General", "Compliment", "Feature request", "Bug", "Feedback" });
            if(attachedFiles.Count > 0) SelectedCategory = "Feedback";
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets a value, used to trigger a property changed trigger updating the caret position.
        /// Updates caret position when toggled, independant of value
        /// Works in combination with SetCaretPosition
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <datetime>2013/01/14-09:15 AM</datetime>
        public bool UpdateCaretPosition
        {
            get
            {
                return _updateCaretPosition;
            }
            set
            {
                if(_updateCaretPosition == value) return;

                _updateCaretPosition = value;
                OnPropertyChanged("CaretPosition");
            }
        }

        /// <summary>
        /// Gets or sets the selected category.
        /// </summary>
        /// <value>
        /// The selected category.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <datetime>2013/01/14-09:17 AM</datetime>
        public string SelectedCategory
        {
            get
            {
                return _selectedCategory;
            }
            set
            {
                if(_selectedCategory == value) return;

                _selectedCategory = value;
                OnPropertyChanged("SelectedCategory");
            }
        }

        /// <summary>
        /// Gets or sets the categories available to the user.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <datetime>2013/01/14-09:17 AM</datetime>
        public OptomizedObservableCollection<string> Categories
        {
            get
            {
                return _categories ?? (_categories = new OptomizedObservableCollection<string>());
            }
            set
            {
                if(_categories == value) return;

                _categories = value;
                OnPropertyChanged("Categories");
            }
        }

        /// <summary>
        /// Gets or sets the comment displayed in the textbox.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <datetime>2013/01/14-09:17 AM</datetime>
        public string Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                if(_comment == value) return;

                _comment = value;
                OnPropertyChanged("Comment");
            }
        }

        /// <summary>
        /// Gets or sets the recordings file attachement path.
        /// </summary>
        /// <value>
        /// The attachement path.
        /// </value>
        public string RecordingAttachmentPath
        {
            get
            {
                return _recordingAttachmentPath;
            }
            private set
            {
                if(_recordingAttachmentPath == value) return;

                _recordingAttachmentPath = value;
                NotifyOfPropertyChange(() => RecordingAttachmentPath);
                NotifyOfPropertyChange(() => HasRecordingAttachment);
            }
        }

        /// <summary>
        /// Gets or sets the log file attachement path.
        /// </summary>
        /// <value>
        /// The attachement path.
        /// </value>
        public string ServerLogAttachmentPath
        {
            get
            {
                return _serverlogAttachmentPath;
            }
            set
            {
                if(_serverlogAttachmentPath == value) return;

                _serverlogAttachmentPath = value;
                NotifyOfPropertyChange(() => ServerLogAttachmentPath);
                NotifyOfPropertyChange(() => HasServerLogAttachment);
            }
        }

        /// <summary>
        /// Gets or sets the log file attachement path.
        /// </summary>
        /// <value>
        /// The attachement path.
        /// </value>
        public string StudioLogAttachmentPath
        {
            get
            {
                return _studiologAttachmentPath;
            }
            set
            {
                if(_studiologAttachmentPath == value) return;

                _studiologAttachmentPath = value;
                NotifyOfPropertyChange(() => StudioLogAttachmentPath);
                NotifyOfPropertyChange(() => HasStudioLogAttachment);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has a recording file attachment.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has an attachment; otherwise, <c>false</c>.
        /// </value>
        public bool HasRecordingAttachment
        {
            get
            {
                return DoesFileExists(RecordingAttachmentPath);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has a log file attachment.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has an attachment; otherwise, <c>false</c>.
        /// </value>
        public bool HasServerLogAttachment
        {
            get
            {
                return DoesFileExists(ServerLogAttachmentPath);
            }
        }


        /// <summary>
        /// Gets a value indicating whether this instance has a log file attachment.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has an attachment; otherwise, <c>false</c>.
        /// </value>
        public bool HasStudioLogAttachment { get { return DoesFileExists(StudioLogAttachmentPath); } }

        /// <summary>
        /// Get a value displayed on the button be it allowing user to send mail or to go to the community
        /// </summary>
        public string SendMessageButtonCaption
        {
            get { return IsOutlookInstalled() ? "Open Outlook Mail" : "Go to Community"; }
        }

        /// <summary>
        /// Gets or sets the sys info service to retreieve system info from.
        /// </summary>
        /// <value>
        /// The sys info service.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <datetime>2013/01/14-09:17 AM</datetime>
        public ISystemInfoService SysInfoService { get; set; }

        /// <summary>
        /// Gets or sets the communication service used for email
        /// </summary>
        /// <value>
        /// The email comm service.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <datetime>2013/01/14-09:17 AM</datetime>
        public MapiEmailCommService<EmailCommMessage> EmailCommService { get; set; }

        /// <summary>
        /// Browser Popup
        /// </summary>
        public IBrowserPopupController BrowserPopupController { get; set; }

        public ICommand SendCommand
        {
            get { return _sendCommand ?? (_sendCommand = new DelegateCommand(o => Send())); }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new DelegateCommand(o => Cancel())); }
        }

        public ICommand OpenRecordingAttachmentFolderCommand
        {
            get { return _openRecordingAttachmentFolderCommand ?? (_openRecordingAttachmentFolderCommand = new DelegateCommand(o => OpenAttachmentFolder(RecordingAttachmentPath))); }
        }

        public ICommand OpenServerLogAttachmentFolderCommand
        {
            get { return _openServerLogAttachmentFolderCommand ?? (_openServerLogAttachmentFolderCommand = new DelegateCommand(o => OpenAttachmentFolder(ServerLogAttachmentPath))); }
        }

        public ICommand OpenStudioLogAttachmentFolderCommand
        {
            get { return _openStudioLogAttachmentFolderCommand ?? (_openStudioLogAttachmentFolderCommand = new DelegateCommand(o => OpenAttachmentFolder(StudioLogAttachmentPath))); }
        }

        public string Attachments { get; private set; }

        #endregion public properties

        #region methods
        /// <summary>
        /// Used in conjunction with UpdateCaretPosition to set the caretposition in the generated comment
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <datetime>2013/01/14-09:18 AM</datetime>
        private void SetCaretPosition()
        {
            UpdateCaretPosition = !UpdateCaretPosition;
        }

        /// <summary>
        /// Generates the default comment.
        /// </summary>
        /// <param name="sysInfo">The sys info.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <datetime>2013/01/14-09:20 AM</datetime>
        private static string GenerateDefaultComment(SystemInfoTO sysInfo)
        {
            var sb = new StringBuilder();

            sb.Append("Comments : ");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            sb.Append("I Like the product : YES/NO");
            sb.Append(Environment.NewLine);

            sb.Append("I Use the product everyday : YES/NO");
            sb.Append(Environment.NewLine);

            sb.Append("My name is Earl : YES/NO");
            sb.Append(Environment.NewLine);

            sb.Append("Really, my name is Earl : YES/NO");
            sb.Append(Environment.NewLine);

            sb.Append("OS version : ");
            sb.Append(sysInfo.Name + " ");
            sb.Append(sysInfo.Edition + " ");
            sb.Append(sysInfo.ServicePack + " ");
            sb.Append(sysInfo.Version + " ");
            sb.Append(sysInfo.OsBits);
            sb.Append(Environment.NewLine);

            sb.Append("Product Version : ");
            sb.Append(VersionInfo.FetchVersionInfo() + " ");
            sb.Append(sysInfo.ApplicationExecutionBits + "-bit");
            return sb.ToString();

        }

        private void OpenAttachmentFolder(string path)
        {
            try
            {
                Process.Start("explorer.exe", "/select, \"" + path + "\"");
            }
            // ReSharper disable once EmptyGeneralCatchClause
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                //fail silently if the folder to the attachment couldn't be opened
            }
        }

        #endregion

        #region public methods
        /// <summary>
        /// Sends an actual comment through email, mainly invoked by the UI through the SendCommand
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <datetime>2013/01/14-09:19 AM</datetime>
        public void Send()
        {
            if(IsOutlookInstalled())
            {
                var emailCommService = new MapiEmailCommService<EmailCommMessage>();
                Send(emailCommService);
            }
            else
            {
                BrowserPopupController.ShowPopup(Warewolf.Studio.Resources.Languages.Core.Uri_Community_HomePage);
                RequestClose(ViewModelDialogResults.Okay);
            }
        }

        /// <summary>
        /// Function to determine if outlook is installed machine running the studio
        /// </summary>
        public Func<bool> IsOutlookInstalled = () =>
        {
            try
            {
                Dev2Logger.Log.Info("");
                Type type = Type.GetTypeFromCLSID(new Guid("0006F03A-0000-0000-C000-000000000046"));
                if(type == null)
                {
                    return false;
                }
                object obj = Activator.CreateInstance(type);
                Marshal.ReleaseComObject(obj);
                return true;
            }
            catch(COMException)
            {
                Dev2Logger.Log.Info("Outlook not installed on machine");
                return false;
            }
        };

        /// <summary>
        /// Cancels the feedback
        /// </summary>
        public void Cancel()
        {
            RequestClose(ViewModelDialogResults.Cancel);
        }

        /// <summary>
        /// Sends email info using the specified communication service.
        /// </summary>
        /// <param name="commService">The comm service.</param>
        /// <author>Jurie.smit</author>
        /// <datetime>2013/01/14-09:20 AM</datetime>
        /// <exception cref="System.NullReferenceException">ICommService of type EmailCommMessage</exception>
        public void Send(ICommService<EmailCommMessage> commService)
        {
            Dev2Logger.Log.Debug("");
            if(commService == null) throw new NullReferenceException("ICommService<EmailCommMessage>");

            var message = new EmailCommMessage
            {
                To = Warewolf.Studio.Resources.Languages.Core.FeedbackEmail,
                Subject = String.Format("Some Real Live Feedback{0}{1}"
                                        , String.IsNullOrWhiteSpace(SelectedCategory) ? "" : " : ", SelectedCategory),
                Content = Comment
            };

            if(HasRecordingAttachment)
            {
                Attachments += !string.IsNullOrEmpty(RecordingAttachmentPath) ? RecordingAttachmentPath : "";
            }

            if(HasServerLogAttachment)
            {
                Attachments += !string.IsNullOrEmpty(Attachments) ? ";" : "";
                Attachments += !string.IsNullOrEmpty(ServerLogAttachmentPath) ? ServerLogAttachmentPath : "";
            }

            if(HasStudioLogAttachment)
            {
                Attachments += !string.IsNullOrEmpty(Attachments) ? ";" : "";
                Attachments += !string.IsNullOrEmpty(StudioLogAttachmentPath) ? StudioLogAttachmentPath : "";
            }

            message.AttachmentLocation = Attachments;
            commService.SendCommunication(message);
            RequestClose(ViewModelDialogResults.Okay);
        }
        #endregion
    }
}
