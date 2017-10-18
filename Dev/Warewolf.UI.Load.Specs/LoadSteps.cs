using Microsoft.VisualStudio.TestTools.UITest.Common.UIMap;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TechTalk.SpecFlow;


namespace Warewolf.UI.Load.Specs
{
    [Binding]
    class LoadSteps
    {
        [Given(@"I start the timer")]
        [When(@"I start the timer")]
        public void StartTimer()
        {
            ScenarioContext.Current.Add("StartTime", DateTime.Now);
        }

        [Then(@"the timer duration is less than ""(.*)"" seconds")]
        public void StopTimer(string duration)
        {
            var startTime = ScenarioContext.Current.Get<DateTime>("StopTime");
            Assert.IsTrue((startTime - DateTime.Now).TotalSeconds < int.Parse(duration), "Load test failed. Duration is greater than " + duration + " seconds");
        }

        [Given(@"I have ""(.*)"" new workflow tabs open")]
        [When(@"I open ""(.*)"" new workflow tabs")]
        public void OpenManyTabs(string numberOfTabs)
        {
            for(var i = int.Parse(numberOfTabs); i > 0; i--)
            {
                UIMap.Click_NewWorkflow_RibbonButton();
            }
        }

        #region Additional test attributes

        Warewolf.UI.Tests.UIMap UIMap
        {
            get
            {
                if (_UIMap == null)
                {
                    _UIMap = new UIMap();
                }

                return _UIMap;
            }
        }

        private UIMap _UIMap; 

        #endregion
    }
}
