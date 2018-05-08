using Dev2.Activities;
using Dev2.Activities.Designers2.AdvancedRecordset;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using Warewolf.Storage;
using Warewolf.Tools.Specs.BaseTypes;

namespace Warewolf.ToolsSpecs.Toolbox.Database.AdvancedRecordset
{
    [Binding]
    public sealed class AdvancedRecordsetSteps : RecordSetBases
    {

        readonly ScenarioContext _scenarioContext;

        public AdvancedRecordsetSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            _scenarioContext = scenarioContext ?? throw new ArgumentNullException(nameof(scenarioContext));
        }

        [Given(@"I drag on an Advanced Recordset tool")]
        [When(@"I drag on an Advanced Recordset tool")]
        [Then(@"I drag on an Advanced Recordset tool")]
        public void GivenIDragOnAnAdvancedRecordsetTool()
        {
            var activity = new AdvancedRecordsetActivity();
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            var mockServiceInputViewModel = new Mock<IManageSqliteInputViewModel>();
            var mockServiceModel = new Mock<ISqliteServiceModel>();

            _scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                _scenarioContext.Add(nameof(variableList), variableList);
            }

            BuildShapeAndTestData();

            var viewModel = new AdvancedRecordsetDesignerViewModel(modelItem, mockServiceModel.Object);

            _scenarioContext.Add(nameof(viewModel), viewModel);
            _scenarioContext.Add(nameof(mockServiceInputViewModel), mockServiceInputViewModel);
            _scenarioContext.Add(nameof(mockServiceModel), mockServiceModel);
        }

        [Given(@"I have the following sql statement ""(.*)""")]
        [When(@"I have the following sql statement ""(.*)""")]
        [Then(@"I have the following sql statement ""(.*)""")]
        public void GivenIHaveTheFollowingSqlStatement(string sqlQuery)
        {
            var viewModel = _scenarioContext.Get<AdvancedRecordsetDesignerViewModel>("viewModel");
            viewModel.SqlQuery = sqlQuery;
        }

        [Given(@"Declare variables as")]
        public void GivenDeclareVariablesAs(Table decalreVariables)
        {
            var viewModel = _scenarioContext.Get<AdvancedRecordsetDesignerViewModel>("viewModel");
            var i = 0;
            foreach (var row in decalreVariables.Rows)
            {
                viewModel.DeclareVariables[i].Name = row["Name"];
                viewModel.DeclareVariables[i].Value = row["Value"];
                i++;
            }
        }

        [Then(@"The declared Variables are")]
        public void ThenTheDeclaredVariablesAre(Table declaredVariables)
        {
            var viewModel = _scenarioContext.Get<AdvancedRecordsetDesignerViewModel>("viewModel");
            foreach (var tableRow in declaredVariables.Rows)
            {                
                var expectedName = tableRow["VariableName"];
                Assert.IsTrue(viewModel.DeclareVariables.Any(p=>p.Name == expectedName));
            }
        }

        [When(@"I click Generate Outputs")]
        public void WhenIClickGenerateOutputs()
        {
            var viewModel = _scenarioContext.Get<AdvancedRecordsetDesignerViewModel>("viewModel");
            viewModel.GenerateOutputsCommand.Execute(null);
        }

        [Given(@"Outputs are as follows")]
        [When(@"Outputs are as follows")]
        [Then(@"Outputs are as follows")]
        public void ThenOutputsAreAsFollows(Table expectedOutputs)
        {
            var viewModel = _scenarioContext.Get<AdvancedRecordsetDesignerViewModel>("viewModel");
            var outputs = viewModel.OutputsRegion.Outputs;
            foreach (var tableRow in expectedOutputs.Rows)
            {
                var mappedFrom = tableRow["Mapped From"];
                var mappedTo = tableRow["Mapped To"];
                var found = outputs.FirstOrDefault(mapping => mapping.MappedFrom == mappedFrom && mapping.MappedTo == mappedTo);
                Assert.IsNotNull(found,"Outputs is null.");
            }
        }

        [Given(@"Recordset is ""(.*)""")]
        [When(@"Recordset is ""(.*)""")]
        [Then(@"Recordset is ""(.*)""")]
        public void ThenRecordsetIs(string expectedRecordsetName)
        {
            var viewModel = _scenarioContext.Get<AdvancedRecordsetDesignerViewModel>("viewModel");
            var recordsetName = viewModel.OutputsRegion.RecordsetName;
            Assert.AreEqual(expectedRecordsetName, recordsetName);
        }

        [When(@"I update Recordset to ""(.*)""")]
        public void WhenIUpdateRecordsetTo(string updateRecordsetName)
        {
            var viewModel = _scenarioContext.Get<AdvancedRecordsetDesignerViewModel>("viewModel");
            viewModel.OutputsRegion.RecordsetName = updateRecordsetName;
        }

        [When(@"Advanced Recordset tool is executed")]
        public void WhenAdvancedRecordsetToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            _scenarioContext.Add("result", result);
        }


        [Given(@"recordset ""(.*)""  will be")]
        [Then(@"recordset ""(.*)""  will be")]
        [When(@"recordset ""(.*)""  will be")]
        public void ThenRecordsetWillBe(string variable, Table table)
        {
            var tableRows = table.Rows.ToList();
            var recordSets = DataObject.Environment.Eval(variable, 0, true);
            if (recordSets.IsWarewolfAtomListresult)
            {

                var recordSetValues = (recordSets as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult).Item.ToList();

                Assert.AreEqual<int>(tableRows.Count, recordSetValues.Count);

                for (int i = 0; i < tableRows.Count; i++)
                {
                    Assert.AreEqual(tableRows[i][1], ExecutionEnvironment.WarewolfAtomToString(recordSetValues[i]), "Values at index " + i + 1 + " Are not equal");
                }
            }
        }


        protected override void BuildDataList()
        {
            var viewModel = _scenarioContext.Get<AdvancedRecordsetDesignerViewModel>("viewModel");
            var activity = viewModel.ModelItem.GetCurrentValue() as AdvancedRecordsetActivity;

            TestStartNode = new FlowStep
            {
                Action = activity
            };
            _scenarioContext.Add("activity", activity);
        }
    }
}
