using System;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.StudioTestFramework
{
    [Binding]
    public class StudioTestFrameworkWorkflowCoverageSteps
    {
        [Given(@"a saved test is run")]
        public void GivenASavedTestIsRun()
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"generate test coverage is selected")]
        public void GivenGenerateTestCoverageIsSelected()
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"workflow only tests")]
        public void GivenWorkflowOnlyTests(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"test coverage is generated")]
        public void WhenTestCoverageIsGenerated()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the covered nodes are")]
        public void ThenTheCoveredNodesAre(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the test coverage is ""(.*)""")]
        public void ThenTheTestCoverageIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I run all the tests")]
        public void WhenIRunAllTheTests()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"the test coverage is")]
        public void WhenTheTestCoverageIs(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the total workflow test coverage is ""(.*)""")]
        public void ThenTheTotalWorkflowTestCoverageIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"saved test\(s\) below is run")]
        public void GivenSavedTestSBelowIsRun(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the nodes covered are")]
        public void ThenTheNodesCoveredAre(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"I run all the tests with generate coverage selected")]
        public void GivenIRunAllTheTestsWithGenerateCoverageSelected()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the nodes not covered are")]
        public void ThenTheNodesNotCoveredAre(Table table)
        {
            ScenarioContext.Current.Pending();
        }

    }
}
