using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using Dev2.Activities;
using Dev2.Common;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace ActivityUnitTests.ActivityTest
{
    /// <summary>
    /// Summary description for CountRecordsTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WebGetRequestActivityTests : BaseActivityUnitTest
    {
        private TestContext _testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return _testContextInstance;
            }
            set
            {
                _testContextInstance = value;
            }
        }

        #region Additional test attributes
        static object _testGuard = new object();
        [TestInitialize]
        public void TestInit()
        {
            Monitor.Enter(_testGuard);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            Monitor.Exit(_testGuard);
        }

        #endregion

        [TestMethod]
        public void WebGetRequestActivityWhereWebRequestInvokerIsNullExpectConcreateImplementation()
        {
            //------------Setup for test--------------------------
            var activity = GetWebGetRequestActivity();
            //------------Execute Test---------------------------
            var requestInvoker = activity.WebRequestInvoker;
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(requestInvoker, typeof(WebRequestInvoker));
        }

        [TestMethod]
        public void WebGetRequestActivityWhereConstructedExpectIsAbstractString()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var activity = GetWebGetRequestActivity();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(activity, typeof(DsfActivityAbstract<string>));
        }

        [TestMethod]
        public void WebGetRequestWhereGivenAnIWebRequestInvokerExpectGetGivenValue()
        {
            //------------Setup for test--------------------------
            var activity = GetWebGetRequestActivity();
            var webRequestInvoker = new Mock<IWebRequestInvoker>().Object;
            activity.WebRequestInvoker = webRequestInvoker;
            //------------Execute Test---------------------------
            var actual = activity.WebRequestInvoker;
            //------------Assert Results-------------------------
            Assert.AreEqual(webRequestInvoker, actual);
            Assert.IsNotInstanceOfType(actual, typeof(WebRequestInvoker));
        }


        [TestMethod]
        public void GetFindMissingTypeExpectStaticActivityType()
        {
            //------------Setup for test--------------------------
            var activity = GetWebGetRequestActivity();
            //------------Execute Test---------------------------
            var findMissingType = activity.GetFindMissingType();
            //------------Assert Results-------------------------
            Assert.AreEqual(enFindMissingType.StaticActivity, findMissingType);
        }


        [TestMethod]
        public void WebGetRequestExecuteWhereStaticValuesExpectCorrectResults()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IWebRequestInvoker>();
            var activity = GetWebGetRequestActivity(mock);
            activity.Method = "GET";
            activity.Url = "BodyValue";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><testVar /></root>";
            //------------Execute Test---------------------------
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.ExecuteRequest(activity.Method, activity.Url, It.IsAny<List<Tuple<string, string>>>()), Times.Once());
            Assert.IsFalse(Compiler.HasErrors(executeProcess.DataListID));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WebGetRequestActivity_Errors")]
        public void WebGetRequestExecuteWhereErrorExpectErrorAdded()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IWebRequestInvoker>();
            const string message = "This is a forced exception";
            mock.Setup(invoker => invoker.ExecuteRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<Tuple<string, string>>>())).Throws(new InvalidDataException(message));
            var activity = GetWebGetRequestActivity(mock);
            activity.Method = "GET";
            activity.Url = "BodyValue";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><testVar /></root>";
            //------------Execute Test---------------------------
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.ExecuteRequest(activity.Method, activity.Url, It.IsAny<List<Tuple<string, string>>>()), Times.Once());
            Assert.IsTrue(Compiler.HasErrors(executeProcess.DataListID));
            string errorString = Compiler.FetchErrors(executeProcess.DataListID, false);
            StringAssert.Contains(errorString, message);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WebGetRequestActivity_Errors")]
        public void WebGetRequestExecuteWhereErrorVariableAsScalarSpecifiedExpectErrorInsertedInVariable()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IWebRequestInvoker>();
            const string message = "This is a forced exception";
            mock.Setup(invoker => invoker.ExecuteRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<Tuple<string, string>>>())).Throws(new InvalidDataException(message));
            var activity = GetWebGetRequestActivity(mock);
            activity.OnErrorVariable = "[[Err]]";
            activity.Method = "GET";
            activity.Url = "BodyValue";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><Err/></root>";
            //------------Execute Test---------------------------
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.ExecuteRequest(activity.Method, activity.Url, It.IsAny<List<Tuple<string, string>>>()), Times.Once());
            Assert.IsTrue(Compiler.HasErrors(executeProcess.DataListID));

            string actual;
            string error;
            GetScalarValueFromDataList(executeProcess.DataListID, "Err", out actual, out error);
            StringAssert.Contains(actual, message);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WebGetRequestActivity_Errors")]
        public void WebGetRequestExecuteWhereErrorVariableAsRecordSetSpecifiedExpectErrorInsertedInVariable()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IWebRequestInvoker>();
            const string message = "This is a forced exception";
            mock.Setup(invoker => invoker.ExecuteRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<Tuple<string, string>>>())).Throws(new InvalidDataException(message));
            var activity = GetWebGetRequestActivity(mock);
            activity.OnErrorVariable = "[[Errors().Error]]";
            activity.Method = "GET";
            activity.Url = "BodyValue";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root></root>";
            CurrentDl = "<ADL><Errors><Error></Error></Errors></ADL>";
            //------------Execute Test---------------------------
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.ExecuteRequest(activity.Method, activity.Url, It.IsAny<List<Tuple<string, string>>>()), Times.Once());
            Assert.IsTrue(Compiler.HasErrors(executeProcess.DataListID));

            IList<IBinaryDataListItem> actual;
            string error;
            GetRecordSetFieldValueFromDataList(executeProcess.DataListID, "Errors", "Error", out actual, out error);
            List<IBinaryDataListItem> resultData = actual.ToList();
            StringAssert.Contains(resultData[0].TheValue, message);
        }

        [TestMethod]
        public void WebGetRequestExecuteWhereScalarValuesExpectCorrectResults()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IWebRequestInvoker>();
            string url = "http://localhost";
            string expectedResult = "Request Made";
            mock.Setup(invoker => invoker.ExecuteRequest("GET", url, It.IsAny<List<Tuple<string, string>>>())).Returns(expectedResult);
            var activity = GetWebGetRequestActivity(mock);
            activity.Method = "GET";
            activity.Url = "[[Url]]";
            activity.Result = "[[Res]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = string.Format("<root><Url>{0}</Url></root>", url);
            CurrentDl = "<ADL><Res></Res><Url></Url></ADL>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.ExecuteRequest(activity.Method, url, It.IsAny<List<Tuple<string, string>>>()), Times.Once());
            Assert.IsFalse(Compiler.HasErrors(result.DataListID));
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "Res", out actual, out error);
            Assert.AreEqual(expectedResult, actual);
        }

        [TestMethod]
        public void WebGetRequestExecuteWhereMixedScalarsRecordsetDataExpectCorrectExcecution()
        {
            var mock = new Mock<IWebRequestInvoker>();
            string url1 = "http://localhost";
            string url2 = "http://local";
            string url3 = "http://host";
            string expectedResult1 = "Request Made 1";
            string expectedResult2 = "Request Made 2";
            string expectedResult3 = "Request Made 3";
            mock.Setup(invoker => invoker.ExecuteRequest("GET", url1, It.IsAny<List<Tuple<string, string>>>())).Returns(expectedResult1);
            mock.Setup(invoker => invoker.ExecuteRequest("GET", url2, It.IsAny<List<Tuple<string, string>>>())).Returns(expectedResult2);
            mock.Setup(invoker => invoker.ExecuteRequest("GET", url3, It.IsAny<List<Tuple<string, string>>>())).Returns(expectedResult3);
            var activity = GetWebGetRequestActivity(mock);
            activity.Method = "GET";
            activity.Url = "[[Urls(*).U1]]";
            activity.Result = "[[Res(*).R1]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = string.Format("<root><Urls><U1>{0}</U1></Urls><Urls><U1>{1}</U1></Urls><Urls><U1>{2}</U1></Urls></root>", url1, url2, url3);
            CurrentDl = "<ADL><Res><R1></R1></Res><Urls><U1></U1></Urls></ADL>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            IList<IBinaryDataListItem> actual;
            string error;
            GetRecordSetFieldValueFromDataList(result.DataListID, "Res", "R1", out actual, out error);
            mock.Verify(sender => sender.ExecuteRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<Tuple<string, string>>>()), Times.Exactly(3));
            List<IBinaryDataListItem> resultData = actual.ToList();
            Assert.AreEqual(3, resultData.Count);
            Assert.AreEqual("Request Made 1", resultData[0].TheValue);
            Assert.AreEqual("Res(1).R1", resultData[0].DisplayValue);
            Assert.AreEqual("Request Made 2", resultData[1].TheValue);
            Assert.AreEqual("Res(2).R1", resultData[1].DisplayValue);
            Assert.AreEqual("Request Made 3", resultData[2].TheValue);
            Assert.AreEqual("Res(3).R1", resultData[2].DisplayValue);
        }

        [TestMethod]
        public void WebGetRequestExecuteWhereMixedScalarsRecordsetDataExpectCorrectDebug()
        {
            var mock = new Mock<IWebRequestInvoker>();
            string url1 = "http://localhost";
            string url2 = "http://local";
            string url3 = "http://host";
            string expectedResult1 = "Request Made 1";
            string expectedResult2 = "Request Made 2";
            string expectedResult3 = "Request Made 3";
            mock.Setup(invoker => invoker.ExecuteRequest("GET", url1)).Returns(expectedResult1);
            mock.Setup(invoker => invoker.ExecuteRequest("GET", url2)).Returns(expectedResult2);
            mock.Setup(invoker => invoker.ExecuteRequest("GET", url3)).Returns(expectedResult3);
            var activity = GetWebGetRequestActivity(mock);
            activity.Method = "GET";
            activity.Url = "[[Urls(*).U1]]";
            activity.Result = "[[Res(*).R1]]";
            TestData = string.Format("<root><Urls><U1>{0}</U1></Urls><Urls><U1>{1}</U1></Urls><Urls><U1>{2}</U1></Urls></root>", url1, url2, url3);
            CurrentDl = "<ADL><Res><R1></R1></Res><Urls><U1></U1></Urls></ADL>";
            //------------Execute Test---------------------------
            List<DebugItem> inRes;
            List<DebugItem> outRes;
            CheckActivityDebugInputOutput(activity, CurrentDl,
               TestData, out inRes, out outRes);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, inRes.Count);
            DebugItem inputDebugItem = inRes[0];
            List<DebugItemResult> debugInputItemResults = inputDebugItem.ResultsList;
            Assert.AreEqual(10, debugInputItemResults.Count);
            Assert.AreEqual("URL To Execute", debugInputItemResults[0].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugInputItemResults[0].Type);
            Assert.AreEqual("[[Urls(1).U1]]", debugInputItemResults[1].Value);
            Assert.AreEqual(DebugItemResultType.Variable, debugInputItemResults[1].Type);
            Assert.AreEqual(GlobalConstants.EqualsExpression, debugInputItemResults[2].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugInputItemResults[2].Type);
            Assert.AreEqual(url1, debugInputItemResults[3].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugInputItemResults[3].Type);

            Assert.AreEqual("[[Urls(2).U1]]", debugInputItemResults[4].Value);
            Assert.AreEqual(DebugItemResultType.Variable, debugInputItemResults[4].Type);
            Assert.AreEqual(GlobalConstants.EqualsExpression, debugInputItemResults[5].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugInputItemResults[5].Type);
            Assert.AreEqual(url2, debugInputItemResults[6].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugInputItemResults[6].Type);

            Assert.AreEqual("[[Urls(3).U1]]", debugInputItemResults[7].Value);
            Assert.AreEqual(DebugItemResultType.Variable, debugInputItemResults[7].Type);
            Assert.AreEqual(GlobalConstants.EqualsExpression, debugInputItemResults[8].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugInputItemResults[8].Type);
            Assert.AreEqual(url3, debugInputItemResults[9].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugInputItemResults[9].Type);

            Assert.AreEqual(3, outRes.Count);
            List<DebugItemResult> debugOutResults = outRes[0].ResultsList;
            Assert.AreEqual(4, debugOutResults.Count);
            Assert.AreEqual("1", debugOutResults[0].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugOutResults[0].Type);
            Assert.AreEqual("[[Res(1).R1]]", debugOutResults[1].Value);
            Assert.AreEqual(DebugItemResultType.Variable, debugOutResults[1].Type);
            Assert.AreEqual(GlobalConstants.EqualsExpression, debugOutResults[2].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugOutResults[2].Type);
            Assert.AreEqual("", debugOutResults[3].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugOutResults[3].Type);

            debugOutResults = outRes[1].ResultsList;
            Assert.AreEqual(4, debugOutResults.Count);
            Assert.AreEqual("2", debugOutResults[0].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugOutResults[0].Type);
            Assert.AreEqual("[[Res(2).R1]]", debugOutResults[1].Value);
            Assert.AreEqual(DebugItemResultType.Variable, debugOutResults[1].Type);
            Assert.AreEqual(GlobalConstants.EqualsExpression, debugOutResults[2].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugOutResults[2].Type);
            Assert.AreEqual("", debugOutResults[3].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugOutResults[3].Type);

            debugOutResults = outRes[2].ResultsList;
            Assert.AreEqual(4, debugOutResults.Count);
            Assert.AreEqual("3", debugOutResults[0].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugOutResults[0].Type);
            Assert.AreEqual("[[Res(3).R1]]", debugOutResults[1].Value);
            Assert.AreEqual(DebugItemResultType.Variable, debugOutResults[1].Type);
            Assert.AreEqual(GlobalConstants.EqualsExpression, debugOutResults[2].Value);
            Assert.AreEqual(DebugItemResultType.Label, debugOutResults[2].Type);
            Assert.AreEqual("", debugOutResults[3].Value);
            Assert.AreEqual(DebugItemResultType.Value, debugOutResults[3].Type);
        }

        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebGetRequestActivity_UpdateForEachInputs")]
        public void DsfWebGetRequestActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string Url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestActivity { Url = Url, Result = result };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(Url, act.Url);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebGetRequestActivity_UpdateForEachInputs")]
        public void DsfWebGetRequestActivity_UpdateForEachInputs_MoreThan1Updates_Updates()
        {
            //------------Setup for test--------------------------
            const string Url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestActivity { Url = Url, Result = result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>(Url, "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", act.Url);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebGetRequestActivity_UpdateForEachOutputs")]
        public void DsfWebGetRequestActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string Url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestActivity { Url = Url, Result = result };

            act.UpdateForEachOutputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebGetRequestActivity_UpdateForEachOutputs")]
        public void DsfWebGetRequestActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string Url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestActivity { Url = Url, Result = result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebGetRequestActivity_UpdateForEachOutputs")]
        public void DsfWebGetRequestActivity_UpdateForEachOutputs_1Updates_UpdateCommandResult()
        {
            //------------Setup for test--------------------------
            const string Url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestActivity { Url = Url, Result = result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebGetRequestActivity_GetForEachInputs")]
        public void DsfWebGetRequestActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string Url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestActivity { Url = Url, Result = result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(Url, dsfForEachItems[0].Name);
            Assert.AreEqual(Url, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebGetRequestActivity_GetForEachOutputs")]
        public void DsfWebGetRequestActivity_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            const string Url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestActivity { Url = Url, Result = result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(result, dsfForEachItems[0].Name);
            Assert.AreEqual(result, dsfForEachItems[0].Value);
        }


        static DsfWebGetRequestActivity GetWebGetRequestActivity(Mock<IWebRequestInvoker> mockWebRequestInvoker)
        {
            var webRequestInvoker = mockWebRequestInvoker.Object;
            var activity = GetWebGetRequestActivity();
            activity.WebRequestInvoker = webRequestInvoker;
            return activity;
        }

        static DsfWebGetRequestActivity GetWebGetRequestActivity()
        {
            var activity = new DsfWebGetRequestActivity();
            return activity;
        }
    }
}