using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Feedback;
using Dev2.Studio.Feedback.Actions;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Factory
{
    public static class FeedbackFactory
    {
        public static IFeedbackAction CreateEmailFeedbackAction(Dictionary<string, string> attachedFiles, IEnvironmentModel server)
        {
            var emailFeedbackAction = new EmailFeedbackAction(attachedFiles, server);
            return emailFeedbackAction;
        }
    }
}
