
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Text;
using ActivityUnitTests;
using Dev2.Common;
using Dev2.Data.Decision;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList.Contract;
using Dev2.Utilities;
using Microsoft.CSharp.Activities;
using Microsoft.VisualBasic.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Presentation;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Unlimited.Applications.BusinessDesignStudio.Activities;
// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DsfFlowNodeTests : BaseActivityUnitTest
    {
        #region Decision Tests

        // 2013.02.13: Ashley Lewis - Bug 8725, Task 8913
        // 2013.03.03 : Travis - Refactored to properly test logic required
        [TestMethod]
        public void DecisionWithQuotesInScalarExpectedNoUnhandledExceptions()
        {

            Dev2DecisionStack dds = new Dev2DecisionStack { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.AND };
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            dds.AddModelItem(new Dev2Decision { Col1 = "[[var]]", Col2 = "\"", EvaluationFn = enDecisionType.IsEqual });

            string modelData = dds.ToVBPersistableModel();

            CurrentDl = "<ADL><var/></ADL>";
            TestData = "<root><var>\"</var></root>";
            ErrorResultTO errors;
            Guid exeID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), new StringBuilder(TestData), new StringBuilder(CurrentDl), out errors);

            IList<string> getDatalistID = new List<string> { exeID.ToString() };

            var res = new Dev2DataListDecisionHandler().ExecuteDecisionStack(modelData, getDatalistID);

            // remove test datalist ;)
            DataListRemoval(exeID);

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void DecisionWithQuotesInDataExpectedNoUnhandledExceptions()
        {

            Dev2DecisionStack dds = new Dev2DecisionStack { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.AND };
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            dds.AddModelItem(new Dev2Decision { Col1 = "[[var]]", Col2 = "[[var]]", EvaluationFn = enDecisionType.IsEqual });

            string modelData = dds.ToVBPersistableModel();

            CurrentDl = "<ADL><var/></ADL>";
            TestData = "<root><var>\"something \"data\" \"</var></root>";
            ErrorResultTO errors;
            Guid exeID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), new StringBuilder(TestData), new StringBuilder(CurrentDl), out errors);

            IList<string> getDatalistID = new List<string> { exeID.ToString() };

            var res = new Dev2DataListDecisionHandler().ExecuteDecisionStack(modelData, getDatalistID);

            // remove test datalist ;)
            DataListRemoval(exeID);

            Assert.IsTrue(res);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Dev2DataListDecisionHandler_ExecuteDecisionStack")]
        public void Dev2DataListDecisionHandler_ExecuteDecisionStack_WithRecordSetIndexed_EvalutesDecisionCorrectly()
        {

            Dev2DecisionStack dds = new Dev2DecisionStack { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.AND };
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            dds.AddModelItem(new Dev2Decision { Col1 = "[[vars(1).var]]", Col2 = "[[vars(2).var]]", EvaluationFn = enDecisionType.IsEqual });

            string modelData = dds.ToVBPersistableModel();

            CurrentDl = "<ADL><vars><var></var></vars></ADL>";
            TestData = "<root><vars><var>\"something \"data\" \"</var></vars><vars><var>\"somthing \"data\" \"</var></vars></root>";
            ErrorResultTO errors;
            Guid exeID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), new StringBuilder(TestData), new StringBuilder(CurrentDl), out errors);

            IList<string> getDatalistID = new List<string> { exeID.ToString() };

            var res = new Dev2DataListDecisionHandler().ExecuteDecisionStack(modelData, getDatalistID);

            // remove test datalist ;)
            DataListRemoval(exeID);

            Assert.IsFalse(res);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Dev2DataListDecisionHandler_ExecuteDecisionStack")]
        public void Dev2DataListDecisionHandler_ExecuteDecisionStack_WithRecordSetBlank_EvalutesDecisionCorrectlyFalse()
        {

            Dev2DecisionStack dds = new Dev2DecisionStack { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.AND };
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            dds.AddModelItem(new Dev2Decision { Col1 = "[[vars(1).var]]", Col2 = "[[vars().var]]", EvaluationFn = enDecisionType.IsEqual });

            string modelData = dds.ToVBPersistableModel();

            CurrentDl = "<ADL><vars><var></var></vars></ADL>";
            TestData = "<root><vars><var>\"something \"data\" \"</var></vars><vars><var>\"somthing \"data\" \"</var></vars></root>";
            ErrorResultTO errors;
            Guid exeID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), new StringBuilder(TestData), new StringBuilder(CurrentDl), out errors);

            IList<string> getDatalistID = new List<string> { exeID.ToString() };

            var res = new Dev2DataListDecisionHandler().ExecuteDecisionStack(modelData, getDatalistID);

            // remove test datalist ;)
            DataListRemoval(exeID);

            Assert.IsFalse(res);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Dev2DataListDecisionHandler_ExecuteDecisionStack")]
        public void Dev2DataListDecisionHandler_ExecuteDecisionStack_WithRecordSetBlank_EvalutesDecisionCorrectlyTrue()
        {

            Dev2DecisionStack dds = new Dev2DecisionStack { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.AND };
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            dds.AddModelItem(new Dev2Decision { Col1 = "[[vars(1).var]]", Col2 = "[[vars().var]]", EvaluationFn = enDecisionType.IsEqual });

            string modelData = dds.ToVBPersistableModel();

            CurrentDl = "<ADL><vars><var></var></vars></ADL>";
            TestData = "<root><vars><var>\"something \"data\" \"</var></vars><vars><var>\"something \"data\" \"</var></vars></root>";
            ErrorResultTO errors;
            Guid exeID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), new StringBuilder(TestData), new StringBuilder(CurrentDl), out errors);

            IList<string> getDatalistID = new List<string> { exeID.ToString() };

            var res = new Dev2DataListDecisionHandler().ExecuteDecisionStack(modelData, getDatalistID);

            // remove test datalist ;)
            DataListRemoval(exeID);

            Assert.IsTrue(res);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Dev2DataListDecisionHandler_ExecuteDecisionStack")]
        public void Dev2DataListDecisionHandler_ExecuteDecisionStack_WithRecordStar_EvalutesDecisionCorrectly()
        {

            Dev2DecisionStack dds = new Dev2DecisionStack { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.AND };
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            dds.AddModelItem(new Dev2Decision { Col1 = "[[vars(*).var]]", Col2 = @"something ""data""", EvaluationFn = enDecisionType.IsEqual });

            string modelData = dds.ToVBPersistableModel();

            CurrentDl = "<ADL><vars><var></var></vars></ADL>";
            TestData = "<root><vars><var>something \"data\"</var></vars><vars><var>something \"data\"</var></vars></root>";
            ErrorResultTO errors;
            Guid exeID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), new StringBuilder(TestData), new StringBuilder(CurrentDl), out errors);

            IList<string> getDatalistID = new List<string> { exeID.ToString() };

            var res = new Dev2DataListDecisionHandler().ExecuteDecisionStack(modelData, getDatalistID);

            // remove test datalist ;)
            DataListRemoval(exeID);

            Assert.IsTrue(res);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Dev2DataListDecisionHandler_ExecuteDecisionStack")]
        public void Dev2DataListDecisionHandler_ExecuteDecisionStack_SlashInVariable_CanDeserialize()
        {

            CurrentDl = "<ADL><down/><resul><t/></resul></ADL>";
            TestData = @"<root><down>1\n2\n3\n4\n</down><resul><t>1234</t></resul><resul><t>1234</t></resul><resul><t>1/2\3/4\</t></resul><resul><t>1\n2\n3\n4\n</t></resul><resul><t>1 2   3   4   5   </t></resul></root>";
            ErrorResultTO errors;

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            Guid exeID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), new StringBuilder(TestData), new StringBuilder(CurrentDl), out errors);
            IList<string> getDatalistID = new List<string> { exeID.ToString() };

            var dev2DataListDecisionHandler = new Dev2DataListDecisionHandler();

            //------------Execute Test---------------------------
            var res = dev2DataListDecisionHandler.ExecuteDecisionStack(@"{""TheStack"":[{""Col1"":""[[resul(1).t]]"",""Col2"":""1234"",""Col3"":"""",""PopulatedColumnCount"":2,""EvaluationFn"":""IsEqual""},{""Col1"":""[[resul(2).t]]"",""Col2"":""1234"",""Col3"":"""",""PopulatedColumnCount"":2,""EvaluationFn"":""IsEqual""},{""Col1"":""[[resul(3).t]]"",""Col2"":""1/2\\3/4\\"",""Col3"":"""",""PopulatedColumnCount"":2,""EvaluationFn"":""IsEqual""},{""Col1"":""[[resul(4).t]]"",""Col2"":""[[down]]"",""Col3"":"""",""PopulatedColumnCount"":2,""EvaluationFn"":""IsEqual""},{""Col1"":""[[resul(5).t]]"",""Col2"":""1 2   3   4   5   "",""Col3"":"""",""PopulatedColumnCount"":2,""EvaluationFn"":""IsEqual""}],""TotalDecisions"":5,""ModelName"":""Dev2DecisionStack"",""Mode"":""AND"",""TrueArmText"":""True"",""FalseArmText"":""False"",""DisplayText"":""Decision""}", getDatalistID);

            // Assert Can Deserialize
            Assert.IsTrue(res);
        }
        #endregion Decision Tests

        #region FlowNodeExpression

        [TestMethod]
        public void FlowNodeExpressionExpectedIsCSharpValue()
        {
            // BUG 9304 - 2013.05.08 - TWR - designer/runtime error can be removed by converting the underlying expression to a CSharpValue

            var flowNode = new TestFlowNodeActivity<string>();
            var expected = typeof(CSharpValue<string>);
            var actual = flowNode.GetTheExpression().GetType();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void FlowNodeWithValidDecisionExpressionExpectedExpressionIsEvaluatedCorrectly()
        {
            // BUG 9304 - 2013.05.08 - TWR - designer/runtime error can be removed by converting the underlying expression to a CSharpValue

            const string shape = "<DataList><A/><B/><C/></DataList>";
            const string data = "<DataList><A>5</A><B>3</B><C>abc</C></DataList>";

            RunActivity(shape, data, "True", new DsfFlowDecisionActivity
            {
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(\"{'TheStack':[{'Col1':'[[A]]','Col2':'[[B]]','Col3':'','PopulatedColumnCount':2,'EvaluationFn':'IsGreaterThan'}],'TotalDecisions':1,'ModelName':'Dev2DecisionStack','Mode':'AND','TrueArmText':'True','FalseArmText':'False','DisplayText':'If [[A]] Is Greater Than [[B]]'}\",AmbientDataList)"
            });

            RunActivity(shape, data, "False", new VisualBasicValue<bool>
            {
                ExpressionText = "Dev2DecisionHandler.Instance.ExecuteDecisionStack(\"{!TheStack!:[{!Col1!:![[C]]!,!Col2!:!!,!Col3!:!!,!PopulatedColumnCount!:1,!EvaluationFn!:!IsNumeric!}],!TotalDecisions!:1,!Mode!:!AND!,!TrueArmText!:!True!,!FalseArmText!:!False!}\",AmbientDataList)"
            });
        }

        [TestMethod]
        public void FlowNodeWithValidSwitchExpressionExpectedExpressionIsEvaluatedCorrectly()
        {
            // BUG 9304 - 2013.05.08 - TWR - designer/runtime error can be removed by converting the underlying expression to a CSharpValue

            const string expectedValue = "5";
            const string shape = "<DataList><A/><B/><C/></DataList>";
            const string data = "<DataList><A>" + expectedValue + "</A><B>3</B><C>2</C></DataList>";

            RunActivity(shape, data, expectedValue, new DsfFlowSwitchActivity
            {
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData(\"[[A]]\",AmbientDataList)"
            });
        }

        #endregion

        #region DecisionFlowNodeExpressionWithNulls
        //2013.05.14: Ashley Lewis for bug 9339 - allow blanks in expression

        [TestMethod]
        public void FlowNodeWithBlankIsEqualToBlankExpectedExpressionEvaluatesToTrue()
        {
            const string dl = "<DataList></DataList>";

            RunActivity(dl, dl, "True", new DsfFlowDecisionActivity
            {
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(\"{'TheStack':[{'Col1':'','Col2':'','Col3':'','PopulatedColumnCount':2,'EvaluationFn':'IsEqual'}],'TotalDecisions':1,'ModelName':'Dev2DecisionStack','Mode':'AND','TrueArmText':'True','FalseArmText':'False','DisplayText':'If null Is Equal To null'}\",AmbientDataList)"
            });
        }

        [TestMethod]
        public void FlowNodeWithBlankIsBinaryExpectedExpressionEvaluatesToFalse()
        {
            const string dl = "<DataList></DataList>";

            RunActivity(dl, dl, "False", new DsfFlowDecisionActivity
            {
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(\"{'TheStack':[{'Col1':'','Col2':'','Col3':'','PopulatedColumnCount':2,'EvaluationFn':'IsBinary'}],'TotalDecisions':1,'ModelName':'Dev2DecisionStack','Mode':'AND','TrueArmText':'True','FalseArmText':'False','DisplayText':'If null Is Binary'}\",AmbientDataList)"
            });
        }

        [TestMethod]
        public void FlowNodeWithIsBlankAnEmailExpectedExpressionEvaluatesToFalse()
        {
            const string dl = "<DataList></DataList>";

            RunActivity(dl, dl, "False", new DsfFlowDecisionActivity
            {
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(\"{'TheStack':[{'Col1':'','Col2':'','Col3':'','PopulatedColumnCount':2,'EvaluationFn':'IsEmail'}],'TotalDecisions':1,'ModelName':'Dev2DecisionStack','Mode':'AND','TrueArmText':'True','FalseArmText':'False','DisplayText':'If null Is Email'}\",AmbientDataList)"
            });
        }

        [TestMethod]
        public void FlowNodeWithIsBlankGreaterThanBlankExpectedExpressionEvaluatesToFalse()
        {
            const string dl = "<DataList></DataList>";

            RunActivity(dl, dl, "False", new DsfFlowDecisionActivity
            {
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(\"{'TheStack':[{'Col1':'','Col2':'','Col3':'','PopulatedColumnCount':2,'EvaluationFn':'IsGreaterThan'}],'TotalDecisions':1,'ModelName':'Dev2DecisionStack','Mode':'AND','TrueArmText':'True','FalseArmText':'False','DisplayText':'If null Is Greater Than null'}\",AmbientDataList)"
            });
        }

        #endregion

        [TestMethod]
        [TestCategory("DsfFlowNodeActivity_IActivityTemplateFactory")]
        [Description("DsfFlowNodeActivity IActivityTemplateFactory implementation must return itself.")]
        [Owner("Trevor Williams-Ros")]
        public void DsfFlowNodeActivity_UnitTest_IActivityTemplateFactoryCreate_ReturnsThis()
        {
            var expected = new TestFlowNodeActivity<string>();

            var atf = expected as IActivityTemplateFactory;
            Assert.IsNotNull(atf, "DsfFlowNodeActivity does not implement interface IActivityTemplateFactory.");

            var actual = expected.Create(null);
            Assert.AreSame(expected, actual, "DsfFlowNodeActivity Create did not return itself.");
        }

        #region RunActivity

        void RunActivity<TResult>(string shape, string data, string expectedValue, Activity<TResult> activity)
        {
            const string outputResultKey = "Result";

            #region Create workflow activity

            // This setup MUST mimick the way we setup workflows - WorkflowHelper.CreateWorkflow() !!
            var flowchart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new Assign<TResult>
                    {
                        To = new ArgumentReference<TResult> { ArgumentName = outputResultKey },
                        Value = new InArgument<TResult>(activity)
                    }
                }
            };
            var workflow = new DynamicActivity<TResult>
            {
                Name = new WorkflowHelper().ToNamespaceTypeString(activity.GetType()),
                Implementation = () => flowchart
            };

            new WorkflowHelper().SetProperties(workflow.Properties);
            new WorkflowHelper().SetVariables(flowchart.Variables);

            #endregion

            var dataObject = NativeActivityTest.CreateDataObject(false, false);
            var compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors;
            dataObject.DataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), new StringBuilder(data), new StringBuilder(shape), out errors);

            // we need to set this now ;)
            dataObject.ParentThreadID = 1;

            new WorkflowHelper().CompileExpressions(workflow);

            var actual = string.Empty;
            var reset = new AutoResetEvent(false);

            var inputArgs = new Dictionary<string, object> { { "AmbientDataList", new List<string> { dataObject.DataListID.ToString() } } };

            NativeActivityTest.Run(workflow, dataObject, inputArgs,
                (ex, outputs) =>
                {
                    if(ex != null)
                    {
                        reset.Set();
                        throw ex;
                    }
                    actual = outputs[outputResultKey].ToString();
                    reset.Set();
                });

            reset.WaitOne();

            Assert.AreEqual(expectedValue, actual);
        }

        #endregion
    }
}
