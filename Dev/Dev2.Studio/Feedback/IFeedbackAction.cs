
// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Feedback
{
    public interface IFeedbackAction
    {
        bool CanProvideFeedback { get; }
        int Priority { get; }
        void StartFeedback();
        string ToString();
    }
}
