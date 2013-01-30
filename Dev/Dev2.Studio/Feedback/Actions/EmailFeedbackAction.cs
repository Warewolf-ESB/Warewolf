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
        }

        [Import(typeof(IDev2WindowManager))]
        public IDev2WindowManager WindowNavigation { get; set; }

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
            WindowNavigation.Show(new FeedbackViewModel(_attachmentPath));
        }
    }
}
