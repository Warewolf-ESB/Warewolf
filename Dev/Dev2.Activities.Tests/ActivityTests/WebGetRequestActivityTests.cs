
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable CheckNamespace
namespace ActivityUnitTests.ActivityTest
// ReSharper restore CheckNamespace
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
            const string Message = "This is a forced exception";
            mock.Setup(invoker => invoker.ExecuteRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<Tuple<string, string>>>())).Throws(new InvalidDataException(Message));
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
            StringAssert.Contains(errorString, Message);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WebGetRequestActivity_Errors")]
        public void WebGetRequestExecuteWhereErrorVariableAsScalarSpecifiedExpectErrorInsertedInVariable()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IWebRequestInvoker>();
            const string Message = "This is a forced exception";
            mock.Setup(invoker => invoker.ExecuteRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<Tuple<string, string>>>())).Throws(new InvalidDataException(Message));
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
            StringAssert.Contains(actual, Message);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WebGetRequestActivity_Errors")]
        public void WebGetRequestExecuteWhereErrorVariableAsRecordSetSpecifiedExpectErrorInsertedInVariable()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IWebRequestInvoker>();
            const string Message = "This is a forced exception";
            mock.Setup(invoker => invoker.ExecuteRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<Tuple<string, string>>>())).Throws(new InvalidDataException(Message));
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
            StringAssert.Contains(resultData[0].TheValue, Message);
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

            var tuple1 = new Tuple<string, string>("[[res]]", "Test");
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
