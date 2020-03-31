using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.UI.Tests;

namespace Warewolf.UISpecs
{
    [Binding]
    [DeploymentItem("Warewolf.UI.Tests.dll")]
    class SetDefaultPlaybackSettings
    {
        static Depends _containerOps;

        [BeforeScenario]
        public void UseDefaultPlaybackSettings()
        {
            UIMap.SetPlaybackSettings();
        }

        [BeforeFeature("Deploy")]
        [BeforeFeature("Explorer")]
        public static void StartRemoteContainer() => _containerOps = new Depends(Depends.ContainerType.CIRemote);

        [AfterFeature("Deploy")]
        [AfterFeature("Explorer")]
        public static void StopRemoteContainer() => _containerOps?.Dispose();
    }
}
