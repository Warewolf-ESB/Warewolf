using System;

namespace Dev2.Studio.Feedback
{
    public interface IAsyncFeedbackAction : IFeedbackAction
    {
        void StartFeedback(Action<Exception> onCompleted);
        void FinishFeedBack();
        void CancelFeedback();
    }
}
