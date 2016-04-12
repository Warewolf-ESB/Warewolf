using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Activities.SelectAndApply;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Activities.Specs.Toolbox.LoopConstructs.ForEach;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Data.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs
{
    class SelectTestTool:DsfNativeActivity<string>
    {
        private bool _throws;

        public SelectTestTool():base(false,"")
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
            ScenarioContext.Current.Add("InnerActivity",dsfNumberFormatActivity);
            bool error = ScenarioContext.Current.Any(a => a.Key == "throws");
            dsfNumberFormatActivity.Throws = error;
            var dsfSelectAndApply = new DsfSelectAndApplyActivity
            {
                DataSource = recordSet,
                Alias = alias,
                ApplyActivity = dsfNumberFormatActivity
            };

            TestStartNode = new FlowStep
            {
                Action = dsfSelectAndApply
            };

            ScenarioContext.Current.Add("activity", dsfSelectAndApply);
        }

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

        [When(@"the selectAndApply tool is executed")]
        public void WhenTheSelectAndApplyToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false, channel: new mockEsb());
            ScenarioContext.Current.Add("result", result);
        }
        
        [Then(@"the selectAndApply executes (.*) times")]
        public void ThenTheSelectAndApplyExecutesTimes(int numOfIterations)
        {
            var act = ScenarioContext.Current.Get<SelectTestTool>("InnerActivity");
                Assert.AreEqual(act.Called,numOfIterations);
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
