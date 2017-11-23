using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.UI.Tests;

namespace Warewolf.UISpecs
{
    [Binding]
    [DeploymentItem("Warewolf.UI.Tests.dll")]
    class SetDefaultPlaybackSettings
    {
        [BeforeScenario]
        public void UseDefaultPlaybackSettings()
        {
            UIMap.SetPlaybackSettings();
        }
    }
}
