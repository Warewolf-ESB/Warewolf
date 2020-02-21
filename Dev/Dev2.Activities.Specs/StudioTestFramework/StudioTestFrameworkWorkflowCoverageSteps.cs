﻿using System;
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

        [Given(@"two saved tests ""(.*)"" and ""(.*)""")]
        public void GivenTwoSavedTestsAnd(string p0, string p1)
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

    }
}
