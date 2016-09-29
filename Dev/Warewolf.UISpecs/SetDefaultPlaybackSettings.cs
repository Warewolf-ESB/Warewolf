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
            Uimap.SetPlaybackSettings();
        }

        UIMap Uimap
        {
            get
            {
                if (_uiMap == null)
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        static UIMap _uiMap;
    }
}
