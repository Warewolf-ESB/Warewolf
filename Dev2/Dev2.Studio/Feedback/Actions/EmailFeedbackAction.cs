using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Help;
using System.ComponentModel.Composition;

namespace Dev2.Studio.Feedback.Actions
{
    [Export(typeof(IFeedbackAction))]
    public class EmailFeedbackAction : IFeedbackAction
    {
        private string _attachmentPath;

        public EmailFeedbackAction(): this("")
        {
            
        }

        public EmailFeedbackAction(string attachmentPath)
        {
            _attachmentPath = attachmentPath;
            ImportService.SatisfyImports(this);
        }

        [Import(typeof(IWindowManager))]
        public IWindowManager WindowManager { get; set; }

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
            WindowManager.ShowWindow(new FeedbackViewModel(_attachmentPath));
        }
    }
}
