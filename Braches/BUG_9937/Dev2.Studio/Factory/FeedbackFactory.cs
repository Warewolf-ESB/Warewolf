using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Composition;
using Dev2.Studio.Feedback;
using Dev2.Studio.Feedback.Actions;

namespace Dev2.Studio.Factory
{
    public static class FeedbackFactory
    {
        public static IFeedbackAction CreateEmailFeedbackAction(string attachmentPath)
        {
            var emailFeedbackAction = new EmailFeedbackAction(attachmentPath);
            ImportService.SatisfyImports(emailFeedbackAction);
            return emailFeedbackAction;
        }
    }
}
