using System;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.AppResources.Exceptions
{
    public class FeedbackRecordingInprogressException : Exception
    {
        public FeedbackRecordingInprogressException()
        {

        }

        public FeedbackRecordingInprogressException(string message)
            : base(message)
        {

        }
    }
}
