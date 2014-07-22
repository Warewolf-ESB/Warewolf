using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs
{
    [Binding]
    public class UnknownSteps
    {
        [Given(@"I have a schedule ""(.*)""")]
        public void GivenIHaveASchedule(string scheduleName)
        {
            ScenarioContext.Current.Add("ScheduleName",scheduleName);
        }
        
        [Given(@"""(.*)"" executes an Workflow ""(.*)""")]
        public void GivenExecutesAnWorkflow(string scheduleName, string workFlow)
        {
            ScenarioContext.Current.Add("WorkFlow", workFlow);
        }
        
        [Given(@"""(.*)"" has a username of ""(.*)"" and a Password of ""(.*)""")]
        public void GivenHasAUsernameOfAndAPasswordOf(string scheduleName, string userName, string password)
        {
            ScenarioContext.Current.Add("WorkFlow", scheduleName);
            ScenarioContext.Current.Add("UserName", userName);
            ScenarioContext.Current.Add("Password", password);
        }
        
        [Given(@"""(.*)"" has a Schedule of")]
        public void GivenHasAScheduleOf(string scheduleName, Table table)
        {
            //SchedulerViewModel scheduler = new SchedulerViewModel(); 
        }
        
        [When(@"the ""(.*)"" is executed")]
        public void WhenTheIsExecuted(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the schedule status is ""(.*)""")]
        public void ThenTheScheduleStatusIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"""(.*)"" has ""(.*)"" row of history")]
        public void ThenHasRowOfHistory(string p0, int p1)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the history debug output for '(.*)' for row ""(.*)"" is")]
        public void ThenTheHistoryDebugOutputForForRowIs(string p0, int p1, Table table)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
