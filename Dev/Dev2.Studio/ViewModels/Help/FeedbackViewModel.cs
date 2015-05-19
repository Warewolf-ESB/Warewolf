
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
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Browsers;
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
        private ICommand _cancelCommand;
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

        public FeedbackViewModel(string attachments)
            : this(new Dictionary<string, string>(), attachments)
        {
        }

        public FeedbackViewModel(Dictionary<string, string> attachedFiles, string attachments)
        {
            Attachments = attachments;
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
        /// Gets or sets the sys info service to retreieve system info from.
        /// </summary>
        /// <value>
        /// The sys info service.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <datetime>2013/01/14-09:17 AM</datetime>
        public ISystemInfoService SysInfoService { get; set; }

        /// <summary>
        /// Browser Popup
        /// </summary>
        public IBrowserPopupController BrowserPopupController { get; set; }

        public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new DelegateCommand(o => Cancel())); }
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

        #endregion

        #region public methods



        /// <summary>
        /// Cancels the feedback
        /// </summary>
        public void Cancel()
        {
            RequestClose(ViewModelDialogResults.Cancel);
        }

        #endregion
    }
}
