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
    }
}
