/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Dev2;
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Communication;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Activities.ActivityTests.Web;
using Dev2.Tests.Activities.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace ActivityUnitTests.ActivityTest

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
            var activity = GetWebGetRequestWithTimeoutActivity();
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
            var activity = GetWebGetRequestWithTimeoutActivity();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(activity, typeof(DsfActivityAbstract<string>));
            Assert.AreEqual(100, activity.TimeoutSeconds);
        }

        [TestMethod]
        public void WebGetRequestWhereGivenAnIWebRequestInvokerExpectGetGivenValue()
        {
            //------------Setup for test--------------------------
            var activity = GetWebGetRequestWithTimeoutActivity();
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
            var activity = GetWebGetRequestWithTimeoutActivity();
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
            var activity = GetWebGetRequestWithTimeoutActivity(mock);
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
            var activity = GetWebGetRequestWithTimeoutActivity(mock);
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
            var errorString = DataObject.Environment.FetchErrors();
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
            var activity = GetWebGetRequestWithTimeoutActivity(mock);
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
            GetScalarValueFromEnvironment(result.Environment, "Res", out string actual, out string error);
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
            var activity = GetWebGetRequestWithTimeoutActivity(mock);
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
            GetScalarValueFromEnvironment(result.Environment, "Res", out string actual, out string error);
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
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("DsfWebGetRequestActivity_Execute")]
        public void WebGetRequestExecuteWithHeaders()
        {
            const string response = "[\"value1\",\"value2\"]";
            var dsfWebGetActivity = new DsfWebGetRequestActivity
            {
                Url = "[[URL]]",
                Result = "[[Response]]",
                Headers = "Authorization: Basic 321654987"
            };
            var environment = new ExecutionEnvironment();
            environment.Assign("[[URL]]", "http://rsaklfsvrtfsbld:9910/api/values", 0);
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.IsDebugMode()).Returns(true);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new MockEsb());
            //------------Execute Test---------------------------
            dsfWebGetActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual(response, ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Response]]", 0)));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("DsfWebGetRequestActivity_Execute")]
        public void WebGetRequestWithTimeoutActivity_ExecuteWithHeaders()
        {
            var dsfWebGetActivity = new DsfWebGetRequestWithTimeoutActivity
            {
                Url = "[[URL]]",
                Result = "[[Response]]",
                TimeOutText = "hhh",
                Headers = "Authorization: Basic 321654987"
            };
            var environment = new ExecutionEnvironment();
            environment.Assign("[[URL]]", "http://rsaklfsvrtfsbld:9910/api/values", 0);
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.IsDebugMode()).Returns(true);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new MockEsb());
            //------------Execute Test---------------------------
            dsfWebGetActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual("Value hhh for TimeoutSecondsText could not be interpreted as a numeric value.\r\nExecution aborted - see error messages.", environment.FetchErrors().ToString());
        }
        static DsfWebGetRequestWithTimeoutActivity GetWebGetRequestWithTimeoutActivity(Mock<IWebRequestInvoker> mockWebRequestInvoker)
        {
            var webRequestInvoker = mockWebRequestInvoker.Object;
            var activity = GetWebGetRequestWithTimeoutActivity();
            activity.WebRequestInvoker = webRequestInvoker;
            return activity;
        }

        static DsfWebGetRequestWithTimeoutActivity GetWebGetRequestWithTimeoutActivity()
        {
            var activity = new DsfWebGetRequestWithTimeoutActivity();
            return activity;
        }
        
    }
    public class MockEsb : IEsbChannel
    {

        #region Not Implemented

        public Guid ExecuteRequest(IDSFDataObject dataObject, EsbExecuteRequest request, Guid workspaceID,
                                   out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            return Guid.NewGuid();
        }

        public T FetchServerModel<T>(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors, int update)
        {
            throw new NotImplementedException();
        }

        public string FindServiceShape(Guid workspaceID, string serviceName, int update)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the service shape.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="resourceID">Name of the service.</param>
        /// <returns></returns>
        public StringBuilder FindServiceShape(Guid workspaceID, Guid resourceID)
        {
            return null;
        }

        public IList<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>> ShapeForSubRequest(
            IDSFDataObject dataObject, string inputDefs, string outputDefs, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        public Guid CorrectDataList(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors,
                                    IDataListCompiler compiler)
        {
            throw new NotImplementedException();
        }

        public void ExecuteLogErrorRequest(IDSFDataObject dataObject, Guid workspaceID, string uri,
                                           out ErrorResultTO errors, int update)
        {
            throw new NotImplementedException();
        }

        public IExecutionEnvironment UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(IDSFDataObject dataObject, string outputDefs, int update, bool handleErrors, ErrorResultTO errors)
        {
            return null;
        }

        public void CreateNewEnvironmentFromInputMappings(IDSFDataObject dataObject, string inputDefs, int update)
        {
        }

        #endregion

        public IExecutionEnvironment ExecuteSubRequest(IDSFDataObject dataObject, Guid workspaceID, string inputDefs, string outputDefs,
                                      out ErrorResultTO errors, int update, bool b)
        {

            errors = new ErrorResultTO();
            return dataObject.Environment;
        }
    }
}
