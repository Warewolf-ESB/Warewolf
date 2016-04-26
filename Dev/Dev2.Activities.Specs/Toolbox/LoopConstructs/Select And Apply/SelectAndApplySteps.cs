using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Activities.Designers2.SelectAndApply;
using Dev2.Activities.SelectAndApply;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Activities.Specs.Toolbox.LoopConstructs.ForEach;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces;
using Warewolf.Storage;
using Dev2.Studio.Core.Activities.Utils;
using Moq;
using System.Linq.Expressions;

namespace Dev2.Activities.Specs
{
    class SelectTestTool : DsfNativeActivity<string>
    {
        private bool _throws;

        public SelectTestTool()
            : base(false, "")
        {
            DisplayName = "Comment";
        }

        public bool Throws
        {
            get
            {
                return _throws;
            }
            set
            {
                _throws = value;
            }
        }
        public int Called { get; private set; }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            Called++;
            if (Throws)
                throw new Exception();
        }

        protected override void OnExecute(NativeActivityContext context)
        {

        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {

        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {

        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return new List<DsfForEachItem>();
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return new List<DsfForEachItem>();
        }
    }
    [Binding]
    public class SelectAndApplySteps : RecordSetBases
    {

        [Given(@"I drag a new Select and Apply tool to the design surface")]
        public void GivenIDragANewSelectAndApplyToolToTheDesignSurface()
        {

            var selectAndApplyTool = new DsfSelectAndApplyActivity();
            var modelItem = ModelItemUtils.CreateModelItem(selectAndApplyTool);
            var mockEnvironmentRepo = new Mock<IEnvironmentRepository>();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            var mockResourcRepositorySetUp = new Mock<IResourceRepository>();
            var mockEventAggregator = new Mock<IEventAggregator>();

            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.ID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);



            mockEnvironmentRepo.Setup(repository => repository.ActiveEnvironment).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(mockEnvironmentModel.Object);

            var selectAndApplyVm = new SelectAndApplyDesignerViewModel(modelItem);
            ScenarioContext.Current.Add("downloadViewModel", selectAndApplyVm);
            ScenarioContext.Current.Add("mockEnvironmentModel", mockEnvironmentModel);
            ScenarioContext.Current.Add("eventAggrMock", mockEventAggregator);
            ScenarioContext.Current.Add("selectAndApplyTool", selectAndApplyTool);
        }

        private DsfSelectAndApplyActivity GetSelectAndApplyActivity()
        {
            var activity = ScenarioContext.Current.Get<DsfSelectAndApplyActivity>("selectAndApplyTool");
            return activity;
        }

        private SelectAndApplyDesignerViewModel GetViewModel()
        {
            var designerViewModel = ScenarioContext.Current.Get<SelectAndApplyDesignerViewModel>("downloadViewModel");
            return designerViewModel;
        }
        private Mock<IEnvironmentModel> GetEnvModel()
        {
            var mock = ScenarioContext.Current.Get<Mock<IEnvironmentModel>>("mockEnvironmentModel");
            return mock;
        }

        private const string ResultRecordsetVariable = "[[r().v]]";
        protected override void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultRecordsetVariable, ""));

            string outMapTo;
            if (ScenarioContext.Current.TryGetValue("outMapTo", out outMapTo))
            {
                variableList.Add(new Tuple<string, string>(outMapTo, ""));
            }

            BuildShapeAndTestData();
            string recordSet;
            if (!ScenarioContext.Current.TryGetValue("Datasource", out recordSet))
            {
                recordSet = string.Empty;
            }


            var innerActivity = ScenarioContext.Current.Get<Activity>("innerActivity");
            var dsfNumberFormatActivity = new SelectTestTool();
            var alias = ScenarioContext.Current.Get<string>("Alias");
            //ScenarioContext.Current.Add("InnerActivity", dsfNumberFormatActivity);
            bool error = ScenarioContext.Current.Any(a => a.Key == "throws");
            dsfNumberFormatActivity.Throws = error;

            GetSelectAndApplyActivity().DataSource = recordSet;
            GetSelectAndApplyActivity().Alias = alias;
            GetSelectAndApplyActivity().ApplyActivityFunc.Handler = innerActivity ?? dsfNumberFormatActivity;
            TestStartNode = new FlowStep
            {
                Action = GetSelectAndApplyActivity()
            };

            ScenarioContext.Current.Add("activity", GetSelectAndApplyActivity());
        }

        [Given(@"I use a Number Format tool configured as")]
        public void GivenIUseANumberFormatToolConfiguredAs(Table table)
        {
            var activity = new DsfNumberFormatActivity();
            activity.Expression = table.Rows[0]["Number"];
            activity.DecimalPlacesToShow = table.Rows[0]["Decimals to show"];
            activity.RoundingType = table.Rows[0]["Rounding"];
            activity.RoundingDecimalPlaces = table.Rows[0]["Rounding Value"];
            activity.Result = table.Rows[0]["Result"];

            ScenarioContext.Current.Add("innerActivity",activity);
        }
        [Then(@"""(.*)"" has a value of ""(.*)""")]
        public void ThenHasAValueOf(string p0, string p1)
        {
            var warewolfEvalResult = DataObject.Environment.Eval(p0, 0);
            if (warewolfEvalResult.IsWarewolfAtomResult)
            {
                var result = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                if (result != null)
                {
                    Assert.AreEqual(p1,result.Item.ToString());
                }
            }

            if (warewolfEvalResult.IsWarewolfAtomListresult)
            {
                var result = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                if (result != null && result.Item.Count == 1)
                {
                    var warewolfAtom = result.Item[0];
                    Assert.AreEqual(p1, warewolfAtom.ToString());
                }
            }
        }

        /*protected void BuildObjectDataList()
        {
            List<Tuple<string, string>> objList;
            ScenarioContext.Current.TryGetValue("objList", out objList);

            if (objList == null)
            {
                objList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("objList", objList);
            }

            objList.Add(new Tuple<string, string>(ResultRecordsetVariable, ""));

            string outMapTo;
            if (ScenarioContext.Current.TryGetValue("outMapTo", out outMapTo))
            {
                objList.Add(new Tuple<string, string>(outMapTo, ""));
            }

            BuildShapeAndTestData();

            var activityType = ScenarioContext.Current.Get<string>("activityType");

            dynamic activity;

            activity = new SelectTestTool();
            string recordSet;
            if (!ScenarioContext.Current.TryGetValue("Datasource", out recordSet))
            {
                recordSet = string.Empty;
            }



            var dsfNumberFormatActivity = new SelectTestTool();
            var alias = ScenarioContext.Current.Get<string>("Alias");
            bool error = ScenarioContext.Current.Any(a => a.Key == "throws");
            dsfNumberFormatActivity.Throws = error;
            /*var dsfSelectAndApply = new DsfSelectAndApplyActivity
            {
                DataSource = recordSet,
                Alias = alias,
               // ApplyActivity = dsfNumberFormatActivity
            };

            GetSelectAndApplyActivity().DataSource = recordSet;
            GetSelectAndApplyActivity().Alias = alias;
            GetSelectAndApplyActivity().ApplyActivityFunc.Handler = dsfNumberFormatActivity;
            TestStartNode = new FlowStep
            {
                Action = GetSelectAndApplyActivity()
            };

        }*/

        private string BuildInputMappings()
        {
            var inputMappings = new StringBuilder();
            inputMappings.Append("<Inputs>");

            var inMapTo = ScenarioContext.Current.Get<string>("inMapTo");
            string inRecordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, inMapTo);
            string inColumn = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, inMapTo);

            var inMapFrom = ScenarioContext.Current.Get<string>("inMapFrom");
            inputMappings.Append(string.Format("<Input Name=\"{0}\" Source=\"{1}\" Recordset=\"{2}\"/>", inColumn,
                                               inMapFrom, inRecordset));

            inputMappings.Append("</Inputs>");
            return inputMappings.ToString();
        }

        private string BuildOutputMappings()
        {
            var outputMappings = new StringBuilder();
            outputMappings.Append("<Outputs>");

            var outMapFrom = ScenarioContext.Current.Get<string>("outMapFrom");
            string inRecordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, outMapFrom);
            string inColumn = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, outMapFrom);

            var outMapTo = ScenarioContext.Current.Get<string>("outMapTo");
            outputMappings.Append(string.Format(
                "<Output Name=\"{0}\" MapsTo=\"{1}\" Value=\"{1}\" Recordset=\"{2}\"/>", inColumn,
                outMapTo, inRecordset));

            outputMappings.Append("</Outputs>");
            return outputMappings.ToString();
        }

        [Given(@"There is a complexobject in the datalist with this shape")]
        public void GivenThereIsAComplexobjectInTheDatalistWithThisShape(Table table)
        {
            List<TableRow> rows = table.Rows.ToList();

            if (rows.Count == 0)
            {
                var obj = table.Header.ToArray()[0];
                var field = table.Header.ToArray()[1];

                List<Tuple<string, string>> emptyRecordset;

                bool isAdded = ScenarioContext.Current.TryGetValue("obj", out emptyRecordset);
                if (!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("obj", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(obj, field));
            }

            foreach (TableRow tableRow in rows)
            {
                List<Tuple<string, string>> objList;
                ScenarioContext.Current.TryGetValue("objList", out objList);

                if (objList == null)
                {
                    objList = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("objList", objList);
                }
                objList.Add(new Tuple<string, string>(tableRow[0], tableRow[1]));
            }
        }


        [When(@"the selectAndApply tool is executed")]
        public void WhenTheSelectAndApplyToolIsExecuted()
        {
            BuildDataList();
            //BuildObjectDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false, channel: new mockEsb());
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the selectAndApply executes (.*) times")]
        public void ThenTheSelectAndApplyExecutesTimes(int numOfIterations)
        {
            var act = ScenarioContext.Current.Get<SelectTestTool>("InnerActivity");
            Assert.AreEqual(act.Called, numOfIterations);
        }

        [Given(@"Alias is ""(.*)""")]
        public void GivenAliasIs(string alias)
        {
            ScenarioContext.Current["Alias"] = alias;
        }


        [Given(@"Datasource is ""(.*)""")]
        public void GivenDatasourceIs(string p0)
        {
            ScenarioContext.Current["Datasource"] = p0;
        }

        [Given(@"the underlying dropped activity is ""(.*)""")]
        public void GivenTheUnderlyingDroppedActivityIs(string activityType)
        {
            ScenarioContext.Current.Add("activityType", activityType);
        }

        [Given(@"the tool throws an error")]
        public void GivenTheToolThrowsAnError()
        {
            ScenarioContext.Current.Add("throws", true);
        }

    }
}
