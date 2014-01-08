
// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Feedback
{
    public interface IFeedbackInvoker
    {
        void InvokeFeedback(IFeedbackAction feedbackAction);
        void InvokeFeedback(IAsyncFeedbackAction feedbackAction);
        void InvokeFeedback(IFeedbackAction emailFeedbackAction, IAsyncFeedbackAction recordedFeedbackAction);
        IFeedbackAction CurrentAction { get; }
    }
}
