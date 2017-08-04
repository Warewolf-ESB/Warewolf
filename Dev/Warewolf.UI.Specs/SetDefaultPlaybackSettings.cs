using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.UITests;
using Warewolf.Web.UI.Tests.ScreenRecording;

namespace Warewolf.UISpecs
{
    [Binding]
    class SetDefaultPlaybackSettings
    {
        public static TestContext TestContext { get; set; }

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            TestContext = context;
        }

        private FfMpegVideoRecorder screenRecorder = new FfMpegVideoRecorder();

        [BeforeScenario]
        public void UseDefaultPlaybackSettings()
        {
            screenRecorder.StartRecording(TestContext);
            UIMap.SetPlaybackSettings();
        }

        [AfterScenario]
        public void StopScreenRecording()
        {
            screenRecorder.StopRecording(TestContext);
        }
    }
}
