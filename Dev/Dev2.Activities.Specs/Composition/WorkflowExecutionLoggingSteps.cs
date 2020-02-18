/*
*  Warewolf - Once bitten, there's no going bac
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Composition
{
    [Binding]
    public class WorkflowExecutionLoggingSteps
    {
        [Given(@"a valid workflow")]
        public void GivenAValidWorkflow()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Given(@"workflow execution entry point detailed logs are logged")]
        public void GivenWorkflowExecutionEntryPointDetailedLogsAreLogged()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Given(@"a workflow stops on error has no logs")]
        public void GivenAWorkflowStopsOnErrorHasNoLogs()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"a detailed execution completed log entry is created")]
        public void ThenADetailedExecutionCompletedLogEntryIsCreated(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"it has these output parameter values")]
        public void ThenItHasTheseOutputParameterValues(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"a workflow stops on error")]
        public void WhenAWorkflowStopsOnError()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"a detailed on error log entry is created")]
        public void ThenADetailedOnErrorLogEntryIsCreated(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"a workflow execution has an exception")]
        public void WhenAWorkflowExecutionHasAnException()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"a detailed execution exception log entry is created")]
        public void ThenADetailedExecutionExceptionLogEntryIsCreated(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"a detailed execution completed log entry is has no logs")]
        public void ThenADetailedExecutionCompletedLogEntryIsHasNoLogs()
        {
            ScenarioContext.Current.Pending();
        }

    }
}
