using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoTestSharedTools.VideoRecorder
{
    public interface IVideoRecorder
    {
        bool StartRecording(TestContext TestContext);
        void StopRecording(TestContext TestContext);
    }
}
