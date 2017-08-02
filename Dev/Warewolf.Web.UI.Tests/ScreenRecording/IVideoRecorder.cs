using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoTestSharedTools.VideoRecorder
{
    public interface IVideoRecorder
    {
        bool StartRecord(TestContext TestContext);
        void StopRecord(TestContext TestContext);
    }
}
