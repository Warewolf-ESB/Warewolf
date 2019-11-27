using System;
using TechTalk.SpecFlow;

namespace Warewolf.Tools.Specs.Toolbox.Utility.Redis.Delete
{
    [Binding]
    public class DeleteRedisSteps
    {
        [Given(@"I have entered (.*) into the calculators")]
        public void GivenIHaveEnteredIntoTheCalculators(int p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I press adds")]
        public void WhenIPressAdds()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the result should be (.*) on the screens")]
        public void ThenTheResultShouldBeOnTheScreens(int p0)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
