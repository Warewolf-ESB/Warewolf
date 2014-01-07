using System;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.AppResources.Exceptions
{
    public class FeedbackRecordingNoProcessesExcpetion : Exception
    {
        public FeedbackRecordingNoProcessesExcpetion()
        {

        }

        public FeedbackRecordingNoProcessesExcpetion(string message)
            : base(message)
        {

        }
    }
}
