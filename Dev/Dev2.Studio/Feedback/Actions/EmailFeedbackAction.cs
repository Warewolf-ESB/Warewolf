using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Help;
using System.ComponentModel.Composition;

namespace Dev2.Studio.Feedback.Actions
{
    [Export(typeof(IFeedbackAction))]
    public class EmailFeedbackAction : IFeedbackAction
    {
        readonly string _attachmentPath;
        readonly IEnvironmentModel _environmentModel;

        public EmailFeedbackAction(IEnvironmentModel activeEnvironment = null): this("")
        {
            _environmentModel = activeEnvironment;
        }

        public EmailFeedbackAction(string attachmentPath)
        {

            WindowManager = ImportService.GetExportValue<IWindowManager>();
            _attachmentPath = attachmentPath;
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
            if (_environmentModel != null)
            {
                feedbackVm.ServerLogAttachmentPath = FileHelper.GetServerLogTempPath(_environmentModel);
            }
            WindowManager.ShowWindow(feedbackVm);
        }
    }
}
