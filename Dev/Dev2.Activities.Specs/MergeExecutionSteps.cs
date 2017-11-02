using System;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs
{
    [Binding]
    public class MergeExecutionSteps
    {
        [When(@"workflow ""(.*)"" merge is opened")]
        public void WhenWorkflowMergeIsOpened(string mergeWfName)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Current workflow contains ""(.*)"" tools")]
        public void ThenCurrentWorkflowContainsTools(int toolCount)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Different workflow contains ""(.*)"" tools")]
        public void ThenDifferentWorkflowContainsTools(int toolCount)
        {
            ScenarioContext.Current.Pending();
        }

    }
}
