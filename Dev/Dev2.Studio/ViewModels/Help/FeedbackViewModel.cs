using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Services.Communication;
using Dev2.Studio.Core.Services.System;
using Dev2.Studio.Core.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Input;

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
        private ICommand _openAttachmentFolderCommand;
        private string _comment;
        private OptomizedObservableCollection<string> _categories;
        private string _selectedCategory;
        private bool _updateCaretPosition;
        private string _attachmentPath;
        #endregion

        #region ctors and init

        public FeedbackViewModel()
            : this("")
        {
        }

        public FeedbackViewModel(string attachmentPath)
        {
            SysInfoService = ImportService.GetExportValue<ISystemInfoService>();

            var sysInfo = SysInfoService.GetSystemInfo();
            Init(sysInfo, attachmentPath);
            SelectedCategory = "Feedback";
        }

        /// <summary>
        /// Inits the viewmodel with the specified sys info, and setups the default categories.
        /// </summary>
        /// <param name="sysInfo">The sys info.</param>
        /// <param name="attachtmentPath">path to the attachment</param>
        /// <author>Jurie.smit</author>
        /// <datetime>2013/01/14-09:19 AM</datetime>
        private void Init(SystemInfoTO sysInfo, string attachtmentPath)
        {
            AttachmentPath = attachtmentPath;
            Comment = GenerateDefaultComment(sysInfo);
            SetCaretPosition();
            Categories.AddRange(new List<string> { "General", "Compliment", "Feature request", "Bug", "Feedback" });
            if (!String.IsNullOrWhiteSpace(attachtmentPath)) SelectedCategory = "Feedback";
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
                if (_updateCaretPosition == value) return;

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
                if (_selectedCategory == value) return;

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
                if (_categories == value) return;

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
                if (_comment == value) return;

                _comment = value;
                OnPropertyChanged("Comment");
            }
        }

        /// <summary>
        /// Gets or sets the attachement path.
        /// </summary>
        /// <value>
        /// The attachement path.
        /// </value>
        public string AttachmentPath
        {
            get
            {
                return _attachmentPath;
            }
            private set
            {
                if (_attachmentPath == value) return;

                _attachmentPath = value;
                NotifyOfPropertyChange(() => AttachmentPath);
                NotifyOfPropertyChange(() => HasAttachment);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has an attachment.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has an attachment; otherwise, <c>false</c>.
        /// </value>
        public bool HasAttachment 
        {
            get 
            {
                return File.Exists(AttachmentPath);
            }
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

        public ICommand SendCommand
        {
            get { return _sendCommand ?? (_sendCommand = new RelayCommand(o => Send())); }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(o => Cancel())); }
        }

        public ICommand OpenAttachmentFolderCommand
        {
            get { return _openAttachmentFolderCommand ?? (_openAttachmentFolderCommand = new RelayCommand(o => OpenAttachmentFolder())); }
        }
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
            sb.Append(StringResources.CurrentVersion + " ");
            sb.Append(sysInfo.ApplicationExecutionBits + "-bit");
            return sb.ToString();

        }

        private void OpenAttachmentFolder()
        {
            try
            {
                Process.Start("explorer.exe", "/select, \"" + AttachmentPath + "\"");
            }
            catch
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
            var emailCommService = new MapiEmailCommService<EmailCommMessage>();
            Send(emailCommService);
        }

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
            if (commService == null) throw new NullReferenceException("ICommService<EmailCommMessage>");

            var message = new EmailCommMessage
            {
                To = "Chas.Digal@dev2.co.za",
                Subject = String.Format("Some Real Live Feedback{0}{1}"
                                        , String.IsNullOrWhiteSpace(SelectedCategory) ? "" : " : ", SelectedCategory),
                Content = Comment
            };

            if (HasAttachment)
            {
                message.AttachmentLocation = AttachmentPath;
            }

            commService.SendCommunication(message);
            RequestClose(ViewModelDialogResults.Okay);
        }
        #endregion
    }
}
