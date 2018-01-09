using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Web.UI.Tests.ScreenRecording
{
    public interface IVideoRecorder
    {
        bool StartRecording(TestContext TestContext, string browserName);
        void StopRecording(UnitTestOutcome TestContext);
    }
}
