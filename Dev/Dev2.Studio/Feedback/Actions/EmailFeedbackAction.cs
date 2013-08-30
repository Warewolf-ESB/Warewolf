using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.ViewModels.Help;
using System.ComponentModel.Composition;

namespace Dev2.Studio.Feedback.Actions
{
    [Export(typeof(IFeedbackAction))]
    public class EmailFeedbackAction : IFeedbackAction
    {
        readonly string _attachmentPath;
        readonly IEnvironmentModel _environmentModel;

        public EmailFeedbackAction(string attachmentPath, IEnvironmentModel activeEnvironment)
        {
            VerifyArgument.IsNotNull("activeEnvironment", activeEnvironment);
            WindowManager = ImportService.GetExportValue<IWindowManager>();
            _attachmentPath = attachmentPath;
            _environmentModel = activeEnvironment;
        }

        public IWindowManager WindowManager { get; private set; }

        public bool CanProvideFeedback
        {
            get { return true; }
        }

        public int Priority
        {
            get { return 2; }
        }

        public void StartFeedback()
        {
            if (CanProvideFeedback)
                DisplayFeedbackWindow();
        }

        public void DisplayFeedbackWindow()
        {
            var feedbackVm = new FeedbackViewModel(_attachmentPath);
            if (string.IsNullOrEmpty(feedbackVm.ServerLogAttachmentPath) && _environmentModel != null)
            {
                feedbackVm.ServerLogAttachmentPath = FileHelper.GetServerLogTempPath(_environmentModel);
            }
            WindowManager.ShowWindow(feedbackVm);
        }
    }
}
