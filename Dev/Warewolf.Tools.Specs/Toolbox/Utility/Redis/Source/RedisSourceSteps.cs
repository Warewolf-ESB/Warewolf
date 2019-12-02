using System;
using TechTalk.SpecFlow;

namespace Warewolf.Tools.Specs.Toolbox.Utility.Redis.Source
{
    [Binding]
    public class RedisSourceSteps
    {
        [Given(@"I have entered (.*) into the calculatorss")]
        public void GivenIHaveEnteredIntoTheCalculatorss(int p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I press addss")]
        public void WhenIPressAddss()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the result should be (.*) on the screenss")]
        public void ThenTheResultShouldBeOnTheScreenss(int p0)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
