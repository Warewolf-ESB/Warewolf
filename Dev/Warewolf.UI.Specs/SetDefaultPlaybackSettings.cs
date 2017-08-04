using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.UITests;

namespace Warewolf.UISpecs
{
    [Binding]
    [DeploymentItem("Warewolf.UITests.dll")]
    class SetDefaultPlaybackSettings
    {
        [BeforeScenario]
        public void UseDefaultPlaybackSettings()
        {
            UIMap.SetPlaybackSettings();
        }
    }
}
