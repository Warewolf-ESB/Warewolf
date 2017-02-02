/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Dev2;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable InconsistentNaming

// ReSharper disable CheckNamespace
namespace ActivityUnitTests.ActivityTest
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Summary description for CountRecordsTest
    /// </summary>
    [TestClass]
    public class WebGetRequestWithTimeoutActivityTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        static readonly object TestGuard = new object();
        [TestInitialize]
        public void TestInit()
        {
            Monitor.Enter(TestGuard);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            Monitor.Exit(TestGuard);
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
            Assert.AreEqual(100, activity.TimeoutSeconds);
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
            ExecuteProcess();
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.ExecuteRequest(activity.Method, activity.Url, It.IsAny<List<Tuple<string, string>>>(), It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WebGetRequestActivity_Errors")]
        public void WebGetRequestExecuteWhereErrorExpectErrorAdded()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IWebRequestInvoker>();
            const string Message = "This is a forced exception";
            mock.Setup(invoker => invoker.ExecuteRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<Tuple<string, string>>>(), It.IsAny<int>())).Throws(new InvalidDataException(Message));
            var activity = GetWebGetRequestActivity(mock);
            activity.Method = "GET";
            activity.Url = "BodyValue";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><testVar /></root>";
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.ExecuteRequest(activity.Method, activity.Url, It.IsAny<List<Tuple<string, string>>>(), It.IsAny<int>()), Times.Once());
            string errorString = DataObject.Environment.FetchErrors();
            StringAssert.Contains(errorString, Message);
        }

        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("WebGetRequestActivity_Errors")]
        public void WebGetRequestExecuteWhereErrorExpectErrorAdded_TimeoutSecondsOutOfRange()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IWebRequestInvoker>();
            const string Url = "http://localhost";
            const string ExpectedResult = "Request Made";
            mock.Setup(invoker => invoker.ExecuteRequest("GET", Url, It.IsAny<List<Tuple<string, string>>>(), It.IsAny<int>())).Returns(ExpectedResult);
            var activity = GetWebGetRequestActivity(mock);
            activity.Method = "GET";
            activity.Url = "[[Url]]";
            activity.Result = "[[Res]]";
            activity.TimeOutText = "-1";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = string.Format("<root><Url>{0}</Url></root>", Url);
            CurrentDl = "<ADL><Res></Res><Url></Url></ADL>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.ExecuteRequest(activity.Method, Url, It.IsAny<List<Tuple<string, string>>>(), It.IsAny<int>()), Times.Never());
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "Res", out actual, out error);
            Assert.AreNotEqual(ExpectedResult, actual);
            Assert.IsNotNull(error);
        }


        [TestMethod]
        public void WebGetRequestExecuteWhereScalarValuesExpectCorrectResults()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IWebRequestInvoker>();
            const string Url = "http://localhost";
            const string ExpectedResult = "Request Made";
            mock.Setup(invoker => invoker.ExecuteRequest("GET", Url, It.IsAny<List<Tuple<string, string>>>(), It.IsAny<int>())).Returns(ExpectedResult);
            var activity = GetWebGetRequestActivity(mock);
            activity.Method = "GET";
            activity.Url = "[[Url]]";
            activity.Result = "[[Res]]";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = string.Format("<root><Url>{0}</Url></root>", Url);
            CurrentDl = "<ADL><Res></Res><Url></Url></ADL>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.ExecuteRequest(activity.Method, Url, It.IsAny<List<Tuple<string, string>>>(), It.IsAny<int>()), Times.Once());
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "Res", out actual, out error);
            Assert.AreEqual(ExpectedResult, actual);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebGetRequestActivity_UpdateForEachInputs")]
        public void DsfWebGetRequestActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string Url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = Url, Result = result };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(Url, act.Url);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetOutputs")]
        public void GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            const string Url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = Url, Result = result };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[[res]]", outputs[0]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebGetRequestActivity_UpdateForEachInputs")]
        public void DsfWebGetRequestActivity_UpdateForEachInputs_MoreThan1Updates_Updates()
        {
            //------------Setup for test--------------------------
            const string Url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = Url, Result = result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>(Url, "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 });
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
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = Url, Result = result };

            act.UpdateForEachOutputs(null);
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
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = Url, Result = result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 });
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
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = Url, Result = result };

            var tuple1 = new Tuple<string, string>("[[res]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
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
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = Url, Result = result };

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
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = Url, Result = result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(result, dsfForEachItems[0].Name);
            Assert.AreEqual(result, dsfForEachItems[0].Value);
        }


        static DsfWebGetRequestWithTimeoutActivity GetWebGetRequestActivity(Mock<IWebRequestInvoker> mockWebRequestInvoker)
        {
            var webRequestInvoker = mockWebRequestInvoker.Object;
            var activity = GetWebGetRequestActivity();
            activity.WebRequestInvoker = webRequestInvoker;
            return activity;
        }

        static DsfWebGetRequestWithTimeoutActivity GetWebGetRequestActivity()
        {
            var activity = new DsfWebGetRequestWithTimeoutActivity();
            return activity;
        }
    }
}
