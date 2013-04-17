using System;

namespace Dev2.Studio.AppResources.Exceptions
{
    public class FeedbackRecordingTimeoutException : Exception
    {
        public FeedbackRecordingTimeoutException() 
        {
            
        }

        public FeedbackRecordingTimeoutException(string message) 
            : base(message)
        {
            
        }
    }
}
