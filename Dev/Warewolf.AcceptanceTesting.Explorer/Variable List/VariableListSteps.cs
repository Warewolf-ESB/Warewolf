using System;
using TechTalk.SpecFlow;

namespace Warewolf.AcceptanceTesting.Explorer.Variable_List
{
    [Binding]
    public class VariableListSteps
    {
        [Given(@"I open workflow ""(.*)""")]
public void GivenIOpenWorkflow(string p0)
{
    ScenarioContext.Current.Pending();
}

        [Given(@"workflow ""(.*)"" contains")]
public void GivenWorkflowContains(string p0, Table table)
{
    ScenarioContext.Current.Pending();
}

        [Given(@"""(.*)"" variable contains")]
public void GivenVariableContains(string p0, Table table)
{
    ScenarioContext.Current.Pending();
}

        [Given(@"I have ""(.*)"" tab")]
public void GivenIHaveTab(string p0)
{
    ScenarioContext.Current.Pending();
}

        [When(@"I delete unassigned variables")]
public void WhenIDeleteUnassignedVariables()
{
    ScenarioContext.Current.Pending();
}

        [When(@"I search for variable ""(.*)""")]
public void WhenISearchForVariable(string p0)
{
    ScenarioContext.Current.Pending();
}

        [When(@"I clear the filter")]
public void WhenIClearTheFilter()
{
    ScenarioContext.Current.Pending();
}

        [When(@"I Sort the variables")]
public void WhenISortTheVariables()
{
    ScenarioContext.Current.Pending();
}

        [Then(@"variables filter box is ""(.*)""")]
public void ThenVariablesFilterBoxIs(string p0)
{
    ScenarioContext.Current.Pending();
}

        [Then(@"Filter Clear button is ""(.*)""")]
public void ThenFilterClearButtonIs(string p0)
{
    ScenarioContext.Current.Pending();
}

        [Then(@"the Variable Names are")]
public void ThenTheVariableNamesAre(Table table)
{
    ScenarioContext.Current.Pending();
}

        [Then(@"the Recordset Names are")]
public void ThenTheRecordsetNamesAre(Table table)
{
    ScenarioContext.Current.Pending();
}

        [Then(@"""(.*)"" order is ""(.*)""")]
public void ThenOrderIs(string p0, string p1)
{
    ScenarioContext.Current.Pending();
}

        [Then(@"Variables box is ""(.*)""")]
public void ThenVariablesBoxIs(string p0)
{
    ScenarioContext.Current.Pending();
}
    }
}
