using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using TechTalk.SpecFlow;
using Warewolf.Launcher;
using Warewolf.UI.Tests;

namespace Warewolf.UISpecs
{
    [Binding]
    [DeploymentItem("Warewolf.UI.Tests.dll")]
    class SetDefaultPlaybackSettings
    {
        static ContainerLauncher _containerOps;

        [BeforeScenario]
        public void UseDefaultPlaybackSettings()
        {
            UIMap.SetPlaybackSettings();
        }

        [BeforeFeature("Deploy")]
        [BeforeFeature("Explorer")]
        public static void StartRemoteContainer() => _containerOps = TestLauncher.StartLocalCIRemoteContainer(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestResults"));

        [AfterFeature("Deploy")]
        [AfterFeature("Explorer")]
        public static void StopRemoteContainer() => _containerOps?.Dispose();
    }
}
