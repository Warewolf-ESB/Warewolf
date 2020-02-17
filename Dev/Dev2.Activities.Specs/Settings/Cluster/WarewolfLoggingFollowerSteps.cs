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

namespace Dev2.Activities.Specs.Settings.Cluster
{
    [Binding]
    public class WarewolfLoggingFollowerSteps
    {
        [Given(@"a valid follower server resource")]
        public void GivenAValidFollowerServerResource()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Given(@"log service have received logs")]
        public void GivenLogServiceHaveReceivedLogs()
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"the follower server tries to log and connection to leader not live")]
        public void WhenTheFollowerServerTriesToLogAndConnectionToLeaderNotLive()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the logs should be logged locally")]
        public void ThenTheLogsShouldBeLoggedLocally()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"local log file should contain")]
        public void ThenLocalLogFileShouldContain(Table table)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
