using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Presentation;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Threading;
using Dev2.Common;
using Dev2.Data.Decision;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Tests.Activities;
using Dev2.Tests.Activities.ActivityTests;
using Dev2.Utilities;
using Microsoft.CSharp.Activities;
using Microsoft.VisualBasic.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace ActivityUnitTests.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    public class DsfFlowNodeTests : BaseActivityUnitTest
    {
        #region Decision Tests

        // 2013.02.13: Ashley Lewis - Bug 8725, Task 8913
        // 2013.03.03 : Travis - Refactored to properly test logic required
        [TestMethod]
        public void DecisionWithQuotesInScalarExpectedNoUnhandledExceptions()
        {
            // theValue.Replace("\"", "\\\"")

            Dev2DecisionStack dds = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.AND };
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            dds.AddModelItem(new Dev2Decision() { Col1 = "[[var]]", Col2 = "\"", EvaluationFn = enDecisionType.IsEqual });

            string modelData = dds.ToVBPersistableModel();

            CurrentDl = "<ADL><var/></ADL>";
            TestData = "<root><var>\"</var></root>";
            ErrorResultTO errors;
            Guid exeID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), TestData, CurrentDl, out errors);

            IList<string> getDatalistID = new List<string>() { exeID.ToString() };
            Assert.IsTrue(new Dev2DataListDecisionHandler().ExecuteDecisionStack(modelData, getDatalistID));
        }

        #endregion Decision Tests

        #region GetDebugInputs/Outputs

        #region Decision tests

        [TestMethod]
        // ReSharper disable InconsistentNaming
        //Bug 8104
        public void FileRead_Get_Debug_Input_Output_With_Recordset_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            DsfFlowDecisionActivity act = new DsfFlowDecisionActivity();
            Dev2DecisionStack dds = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR };

            dds.AddModelItem(new Dev2Decision() { Col1 = "[[Customers(1).FirstName]]", Col2 = string.Empty, Col3 = "b", EvaluationFn = enDecisionType.IsContains });

            string modelData = dds.ToVBPersistableModel();
            act.ExpressionText = string.Join("", GlobalConstants.InjectedDecisionHandler, "(\"", modelData, "\",", GlobalConstants.InjectedDecisionDataListVariable, ")"); ;

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(2, inRes.Count);
            Assert.AreEqual(3, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(3, inRes[1].FetchResultsList().Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(1, outRes[0].FetchResultsList().Count);
        }

        //2013.06.06: Ashley Lewis for PBI 9460 - Debug output for starred indexed recordsets
        [TestMethod]
        public void DecisionGetDebugInputOutputWithStarredIndexedRecordsetAndOnePopulatedColumnExpectedCorrectOutput()
        {
            DsfFlowDecisionActivity act = new DsfFlowDecisionActivity();
            Dev2DecisionStack dds = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.AND, TrueArmText = "Passed Test" };

            dds.AddModelItem(new Dev2Decision() { Col1 = "[[Customers(*).FirstName]]", EvaluationFn = enDecisionType.IsText });

            string modelData = dds.ToVBPersistableModel();
            act.ExpressionText = string.Join("", GlobalConstants.InjectedDecisionHandler, "(\"", modelData, "\",", GlobalConstants.InjectedDecisionDataListVariable, ")"); ;

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(2, inRes.Count);
            Assert.AreEqual(30, inRes[0].FetchResultsList().Count);
            Assert.AreEqual("Wallis", inRes[0].ResultsList[2].Value);
            Assert.AreEqual("Barney", inRes[0].ResultsList[5].Value);
            Assert.AreEqual("Trevor", inRes[0].ResultsList[8].Value);
            Assert.AreEqual("Travis", inRes[0].ResultsList[11].Value);
            Assert.AreEqual("If Wallis Is Text AND Barney Is Text AND Trevor Is Text AND Travis Is Text AND Jurie Is Text AND Bre", inRes[1].ResultsList[2].Value);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(1, outRes[0].FetchResultsList().Count);
            Assert.AreEqual("Passed Test", outRes[0].ResultsList[0].Value);
        }
        [TestMethod]
        public void DecisionGetDebugInputOutputWithStarredIndexedRecordsetAndTwoPopulatedColumnsExpectedCorrectOutput()
        {
            DsfFlowDecisionActivity act = new DsfFlowDecisionActivity();
            Dev2DecisionStack dds = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR, FalseArmText = "Passed Test" };

            dds.AddModelItem(new Dev2Decision() { Col1 = "[[Customers(*).FirstName]]", Col2 = "b", EvaluationFn = enDecisionType.IsContains });

            string modelData = dds.ToVBPersistableModel();
            act.ExpressionText = string.Join("", GlobalConstants.InjectedDecisionHandler, "(\"", modelData, "\",", GlobalConstants.InjectedDecisionDataListVariable, ")"); ;

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(2, inRes.Count);
            Assert.AreEqual(30, inRes[0].FetchResultsList().Count);
            Assert.AreEqual("Wallis", inRes[0].ResultsList[2].Value);
            Assert.AreEqual("Barney", inRes[0].ResultsList[5].Value);
            Assert.AreEqual("Trevor", inRes[0].ResultsList[8].Value);
            Assert.AreEqual("Travis", inRes[0].ResultsList[11].Value);
            Assert.AreEqual("If Wallis Contains b OR Barney Contains b OR Trevor Contains b OR Travis Contains b OR Jurie Contain", inRes[1].ResultsList[2].Value);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(1, outRes[0].FetchResultsList().Count);
            Assert.AreEqual("Passed Test", outRes[0].ResultsList[0].Value);
        }
        [TestMethod]
        public void DecisionGetDebugInputOutputWithTwoPopulatedColumnsAndSecondColumnIsStarredIndexedRecordsetExpectedCorrectOutput()
        {
            DsfFlowDecisionActivity act = new DsfFlowDecisionActivity();
            Dev2DecisionStack dds = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR, FalseArmText = "Passed Test" };

            dds.AddModelItem(new Dev2Decision() { Col1 = "b", Col2 = "[[Customers(*).FirstName]]", EvaluationFn = enDecisionType.IsContains });

            string modelData = dds.ToVBPersistableModel();
            act.ExpressionText = string.Join("", GlobalConstants.InjectedDecisionHandler, "(\"", modelData, "\",", GlobalConstants.InjectedDecisionDataListVariable, ")"); ;

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(2, inRes.Count);
            Assert.AreEqual(30, inRes[0].FetchResultsList().Count);
            Assert.AreEqual("Wallis", inRes[0].ResultsList[2].Value);
            Assert.AreEqual("Barney", inRes[0].ResultsList[5].Value);
            Assert.AreEqual("Trevor", inRes[0].ResultsList[8].Value);
            Assert.AreEqual("Travis", inRes[0].ResultsList[11].Value);
            Assert.AreEqual("If b Contains Wallis OR b Contains Barney OR b Contains Trevor OR b Contains Travis OR b Contains Ju", inRes[1].ResultsList[2].Value);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(1, outRes[0].FetchResultsList().Count);
            Assert.AreEqual("Passed Test", outRes[0].ResultsList[0].Value);
        }
        [TestMethod]
        public void DecisionGetDebugInputOutputWithTwoPopulatedColumnsBothOfThemStarredIndexedRecordsetsExpectedCorrectOutput()
        {
            DsfFlowDecisionActivity act = new DsfFlowDecisionActivity();
            Dev2DecisionStack dds = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR, FalseArmText = "Passed Test" };

            dds.AddModelItem(new Dev2Decision() { Col1 = "[[Customers(*).FirstName]]", Col2 = "[[Customers(*).LastName]]", EvaluationFn = enDecisionType.IsEqual });

            string modelData = dds.ToVBPersistableModel();
            act.ExpressionText = string.Join("", GlobalConstants.InjectedDecisionHandler, "(\"", modelData, "\",", GlobalConstants.InjectedDecisionDataListVariable, ")"); ;

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(2, inRes.Count);
            Assert.AreEqual(60, inRes[0].FetchResultsList().Count);
            Assert.AreEqual("Wallis", inRes[0].ResultsList[2].Value);
            Assert.AreEqual("Barney", inRes[0].ResultsList[5].Value);
            Assert.AreEqual("Trevor", inRes[0].ResultsList[8].Value);
            Assert.AreEqual("Travis", inRes[0].ResultsList[11].Value);
            Assert.AreEqual("If Wallis Is Equal Buchan OR Barney Is Equal Buchan OR Trevor Is Equal Williams-Ros OR Travis Is Equ", inRes[1].ResultsList[2].Value);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(1, outRes[0].FetchResultsList().Count);
            Assert.AreEqual("Passed Test", outRes[0].ResultsList[0].Value);
        }
        [TestMethod]
        public void DecisionGetDebugInputOutputWithTwoStarredIndexedRecordsetsExpectedValidOutput()
        {
            DsfFlowDecisionActivity act = new DsfFlowDecisionActivity();
            Dev2DecisionStack dds = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR, FalseArmText = "Passed Test" };

            dds.AddModelItem(new Dev2Decision() { Col1 = "[[Customers(*).FirstName]]", Col2 = "[[Customers(*).LastName]]", EvaluationFn = enDecisionType.IsEqual });

            string modelData = dds.ToVBPersistableModel();
            act.ExpressionText = string.Join("", GlobalConstants.InjectedDecisionHandler, "(\"", modelData, "\",", GlobalConstants.InjectedDecisionDataListVariable, ")"); ;

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(2, inRes.Count);
            Assert.AreEqual(60, inRes[0].FetchResultsList().Count);
            Assert.AreEqual("Wallis", inRes[0].ResultsList[2].Value);
            Assert.AreEqual("Barney", inRes[0].ResultsList[5].Value);
            Assert.AreEqual("Trevor", inRes[0].ResultsList[8].Value);
            Assert.AreEqual("Travis", inRes[0].ResultsList[11].Value);
            Assert.AreEqual("If Wallis Is Equal Buchan OR Barney Is Equal Buchan OR Trevor Is Equal Williams-Ros OR Travis Is Equ", inRes[1].ResultsList[2].Value);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(1, outRes[0].FetchResultsList().Count);
            Assert.AreEqual("Passed Test", outRes[0].ResultsList[0].Value);
        }

        #endregion

        #region Switch Tests

        [TestMethod]
        // ReSharper disable InconsistentNaming
        //Bug 8104
        public void FileRead_Get_Debug_Input_Output_With_Recordset_Using_Star_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            DsfFileRead act = new DsfFileRead { InputPath = "[[Numeric(*).num]]", Result = "[[CompanyName]]" };
            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);


            Assert.AreEqual(3, inRes.Count);
            Assert.AreEqual(31, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(1, inRes[2].FetchResultsList().Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual("[[CompanyName]]", outRes[0].ResultsList[0].Value);
            Assert.AreEqual("=", outRes[0].ResultsList[1].Value);
            Assert.AreEqual("Dev2", outRes[0].ResultsList[2].Value);
        }

        #endregion

        #endregion

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

            const string Shape = "<DataList><A/><B/><C/></DataList>";
            const string Data = "<DataList><A>5</A><B>3</B><C>abc</C></DataList>";

            RunActivity(Shape, Data, "True", new DsfFlowDecisionActivity
            {
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(\"{'TheStack':[{'Col1':'[[A]]','Col2':'[[B]]','Col3':'','PopulatedColumnCount':2,'EvaluationFn':'IsGreaterThan'}],'TotalDecisions':1,'ModelName':'Dev2DecisionStack','Mode':'AND','TrueArmText':'True','FalseArmText':'False','DisplayText':'If [[A]] Is Greater Than [[B]]'}\",AmbientDataList)"
            });

            RunActivity(Shape, Data, "False", new VisualBasicValue<bool>
            {
                ExpressionText = "Dev2DecisionHandler.Instance.ExecuteDecisionStack(\"{!TheStack!:[{!Col1!:![[C]]!,!Col2!:!!,!Col3!:!!,!PopulatedColumnCount!:1,!EvaluationFn!:!IsNumeric!}],!TotalDecisions!:1,!Mode!:!AND!,!TrueArmText!:!True!,!FalseArmText!:!False!}\",AmbientDataList)"
            });
        }

        [TestMethod]
        public void FlowNodeWithValidSwitchExpressionExpectedExpressionIsEvaluatedCorrectly()
        {
            // BUG 9304 - 2013.05.08 - TWR - designer/runtime error can be removed by converting the underlying expression to a CSharpValue

            const string ExpectedValue = "5";
            const string Shape = "<DataList><A/><B/><C/></DataList>";
            const string Data = "<DataList><A>" + ExpectedValue + "</A><B>3</B><C>2</C></DataList>";

            RunActivity(Shape, Data, ExpectedValue, new DsfFlowSwitchActivity
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
            const string Dl = "<DataList></DataList>";

            RunActivity(Dl, Dl, "True", new DsfFlowDecisionActivity
            {
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(\"{'TheStack':[{'Col1':'','Col2':'','Col3':'','PopulatedColumnCount':2,'EvaluationFn':'IsEqual'}],'TotalDecisions':1,'ModelName':'Dev2DecisionStack','Mode':'AND','TrueArmText':'True','FalseArmText':'False','DisplayText':'If null Is Equal To null'}\",AmbientDataList)"
            });
        }

        [TestMethod]
        public void FlowNodeWithBlankIsBinaryExpectedExpressionEvaluatesToFalse()
        {
            const string Dl = "<DataList></DataList>";

            RunActivity(Dl, Dl, "False", new DsfFlowDecisionActivity
            {
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(\"{'TheStack':[{'Col1':'','Col2':'','Col3':'','PopulatedColumnCount':2,'EvaluationFn':'IsBinary'}],'TotalDecisions':1,'ModelName':'Dev2DecisionStack','Mode':'AND','TrueArmText':'True','FalseArmText':'False','DisplayText':'If null Is Binary'}\",AmbientDataList)"
            });
        }

        [TestMethod]
        public void FlowNodeWithIsBlankAnEmailExpectedExpressionEvaluatesToFalse()
        {
            const string Dl = "<DataList></DataList>";

            RunActivity(Dl, Dl, "False", new DsfFlowDecisionActivity
            {
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(\"{'TheStack':[{'Col1':'','Col2':'','Col3':'','PopulatedColumnCount':2,'EvaluationFn':'IsEmail'}],'TotalDecisions':1,'ModelName':'Dev2DecisionStack','Mode':'AND','TrueArmText':'True','FalseArmText':'False','DisplayText':'If null Is Email'}\",AmbientDataList)"
            });
        }

        [TestMethod]
        public void FlowNodeWithIsBlankGreaterThanBlankExpectedExpressionEvaluatesToFalse()
        {
            const string Dl = "<DataList></DataList>";

            RunActivity(Dl, Dl, "False", new DsfFlowDecisionActivity
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

        static void RunActivity<TResult>(string shape, string data, string expectedValue, Activity<TResult> activity)
        {
            const string OutputResultKey = "Result";

            #region Create workflow activity

            // This setup MUST mimick the way we setup workflows - WorkflowHelper.CreateWorkflow() !!
            var flowchart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new Assign<TResult>
                    {
                        To = new ArgumentReference<TResult> { ArgumentName = OutputResultKey },
                        Value = new InArgument<TResult>(activity)
                    }
                }
            };
            var workflow = new DynamicActivity<TResult>
            {
                Name = WorkflowHelper.ToNamespaceTypeString(activity.GetType()),
                Implementation = () => flowchart
            };

            WorkflowHelper.SetProperties(workflow.Properties);
            WorkflowHelper.SetVariables(flowchart.Variables);

            #endregion

            var dataObject = NativeActivityTest.CreateDataObject(false, false);
            var compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors;
            dataObject.DataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), data, shape, out errors);

            WorkflowHelper.Instance.CompileExpressions(workflow);

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
                    actual = outputs[OutputResultKey].ToString();
                    reset.Set();
                });

            reset.WaitOne();

            Assert.AreEqual(expectedValue, actual);
        }

        #endregion
    }
}
