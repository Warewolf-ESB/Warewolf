/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Text;
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
using Warewolf.UnitTestAttributes;

namespace Dev2.Tests.Activities.DsfWebGetRequestWithTimeoutActivityTests
{
    [TestClass]
    public class DsfWebGetRequestWithTimeoutActivityTests : BaseActivityUnitTest 
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = url, Result = result };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(url, act.Url);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_UpdateForEachInputs_MoreThan1Updates_Updates()
        {
            //------------Setup for test--------------------------
            const string Url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = Url, Result = result };

            var tuple1 = new Tuple<string, string>("Test1", "Test1");
            var tuple2 = new Tuple<string, string>(Url, "Test2");

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", act.Url);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string Url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = Url, Result = result };
            
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }
        
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string Url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = Url, Result = result };

            var tuple1 = new Tuple<string, string>("Test1", "Test1");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");

            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_UpdateForEachOutputs_1Update_UpdateCommandResult()
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
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string url = "[[CompanyName]]";
            const string result = "[[res]]";
            var act = new DsfWebGetRequestWithTimeoutActivity { Url = url, Result = result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(url, dsfForEachItems[0].Name);
            Assert.AreEqual(url, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
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
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Constructed_Expect_IsAbstractString()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var activity = GetWebGetRequestWithTimeoutActivity();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(activity, typeof(DsfActivityAbstract<string>));
            Assert.AreEqual(100, activity.TimeoutSeconds);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_WebRequestInvoker_IsNull_Expect_ConcreateImplementation()
        {
            //------------Setup for test--------------------------
            var activity = GetWebGetRequestWithTimeoutActivity();
            //------------Execute Test---------------------------
            var requestInvoker = activity.WebRequestInvoker;
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(requestInvoker, typeof(WebRequestInvoker));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_WebRequestInvoker_Expect_GetGivenValue()
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
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_GetFindMissingType_Expect_StaticActivityType()
        {
            //------------Setup for test--------------------------
            var activity = GetWebGetRequestWithTimeoutActivity();
            //------------Execute Test---------------------------
            var findMissingType = activity.GetFindMissingType();
            //------------Assert Results-------------------------
            Assert.AreEqual(enFindMissingType.StaticActivity, findMissingType);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_ExecuteRequest_WithStaticValues_Expect_CorrectResults()
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
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.ExecuteRequest(activity.Method, activity.Url, It.IsAny<List<Tuple<string, string>>>(), It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_ExecuteRequest_WithError_ExpectErrorAdded()
        {
            //------------Setup for test--------------------------
            const string Message = "This is a forced exception";

            var mock = new Mock<IWebRequestInvoker>();

            mock.Setup(invoker => invoker.ExecuteRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<Tuple<string, string>>>(), It.IsAny<int>())).Throws(new InvalidDataException(Message));

            var activity = GetWebGetRequestWithTimeoutActivity(mock);
            activity.Method = "GET";
            activity.Url = "BodyValue";
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            //------------Execute Test---------------------------
            ExecuteProcess();
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.ExecuteRequest(activity.Method, activity.Url, It.IsAny<List<Tuple<string, string>>>(), It.IsAny<int>()), Times.Once());
            var errorString = DataObject.Environment.FetchErrors();
            StringAssert.Contains(errorString, Message);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_ExecuteRequest_WithError_ExpectTimeOut_IsTrue()
        {
            //------------Setup for test--------------------------
            const string Message = "This is a forced exception";

            var mock = new Mock<IWebRequestInvoker>();
            var dsfDataObject = new DsfDataObject("", Guid.NewGuid());
            var errorResultTO = new ErrorResultTO();


            var environment = new ExecutionEnvironment();

            var activity = GetWebGetRequestWithTimeoutActivity(mock);
            activity.Method = "GET";
            activity.Url = "BodyValue";
            activity.TimeOutText = "-1";

            TestStartNode = new FlowStep
            {
                Action = activity
            };

            mock.Setup(invoker => invoker.ExecuteRequest(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<Tuple<string, string>>>(), It.IsAny<int>())).Throws(new InvalidDataException(Message));

            environment.Assign("[[URL]]", "http://TFSBLD.premier.local:9910/api/values", 0);
            
            dsfDataObject.Environment = environment;
            dsfDataObject.IsDebug = true;
            dsfDataObject.EsbChannel = new MockEsb();

            bool timeoutSecondsError = false;
            PrivateObject obj = new PrivateObject(activity);
            object[] args = new object[] { dsfDataObject, 0, errorResultTO, timeoutSecondsError };

            //------------Execute Test---------------------------
            var act = obj.Invoke("SetTimeoutSecondsError", args);
            //------------Assert Results-------------------------
            Assert.AreEqual(true, act);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_ExecuteRequest_WithError_Expect_ErrorAdded_TimeoutSecondsOutOfRange()
        {
            //------------Setup for test--------------------------
            const string Url = "http://localhost";
            const string ExpectedResult = "Request Made";

            var mock = new Mock<IWebRequestInvoker>();

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
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.ExecuteRequest(activity.Method, Url, It.IsAny<List<Tuple<string, string>>>(), It.IsAny<int>()), Times.Never());
            GetScalarValueFromEnvironment(result.Environment, "Res", out string actual, out string error);
            Assert.AreNotEqual(ExpectedResult, actual);
            Assert.IsNotNull(error);
        }
        
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_ExecuteRequest_WithScalarValues_Expect_CorrectResults()
        {
            //------------Setup for test--------------------------
            const string Url = "http://localhost";
            const string ExpectedResult = "Request Made";

            var mock = new Mock<IWebRequestInvoker>();

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
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            mock.Verify(sender => sender.ExecuteRequest(activity.Method, Url, It.IsAny<List<Tuple<string, string>>>(), It.IsAny<int>()), Times.Once());
            GetScalarValueFromEnvironment(result.Environment, "Res", out string actual, out string error);
            Assert.AreEqual(ExpectedResult, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_GetOutputs_Called_Expect_ListWithResultValueInIt()
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
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Execute_WithHeaders()
        {
            //------------Setup for test--------------------------
            const string response = "[\"value1\",\"value2\"]";
            var dsfWebGetActivity = new DsfWebGetRequestActivity
            {
                Url = "[[URL]]",
                Result = "[[Response]]",
                Headers = "Authorization: Basic 321654987"
            };
            var environment = new ExecutionEnvironment();

            var dataObjectMock = new Mock<IDSFDataObject>();

            environment.Assign("[[URL]]", $"http://{Depends.TFSBLDIP}:9910/api/values", 0);

            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.IsDebugMode()).Returns(true);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new MockEsb());
            //------------Execute Test---------------------------
            dsfWebGetActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual(response, ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Response]]", 0)));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Execute_WithHeaders_WithTimeoutActivity()
        {
            //------------Setup for test--------------------------
            var dataObjectMock = new Mock<IDSFDataObject>();

            var dsfWebGetActivity = new DsfWebGetRequestWithTimeoutActivity
            {
                Url = "[[URL]]",
                Result = "[[Response]]",
                TimeOutText = "hhh",
                Headers = "Authorization: Basic 321654987"
            };
            var environment = new ExecutionEnvironment();

            environment.Assign("[[URL]]", "http://TFSBLD.premier.local:9910/api/values", 0);

            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.IsDebugMode()).Returns(true);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new MockEsb());
            //------------Execute Test---------------------------
            dsfWebGetActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual("Value hhh for TimeoutSecondsText could not be interpreted as a numeric value.\r\nExecution aborted - see error messages.", environment.FetchErrors().ToString());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_dsfWebGetActivity_Null_IsFalse()
        {
            //------------Setup for test--------------------------
            var dsfWebGetActivity = new DsfWebGetRequestWithTimeoutActivity();
            //------------Execute Test---------------------------
            var actual = dsfWebGetActivity.Equals(null);
            //------------Assert Results-------------------------
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_dsfWebGetActivity_NotNull_IsTrue()
        {
            //------------Setup for test--------------------------
            var dsfWebGetActivity = new DsfWebGetRequestWithTimeoutActivity
            {
                Method = "GET",
                Headers = string.Empty,
                TimeoutSeconds = 100,  // default of 100 seconds
                TimeOutText = "100",
            };
            //------------Execute Test---------------------------
            var actual = dsfWebGetActivity.Equals(dsfWebGetActivity);
            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_ObjectType_NotEqualToThis_IsFalse()
        {
            //------------Setup for test--------------------------
            var dsfWebGetActivity = new DsfWebGetRequestWithTimeoutActivity
            {
                Method = "GET",
                Headers = string.Empty,
                TimeoutSeconds = 100,  // default of 100 seconds
                TimeOutText = "100",
            };
            //------------Execute Test---------------------------
            var actual = dsfWebGetActivity.Equals(new object());
            //------------Assert Results-------------------------
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_Object_IsNull_IsFalse()
        {
            //------------Setup for test--------------------------
            var dsfWebGetActivity = new DsfWebGetRequestWithTimeoutActivity
            {
                Method = "GET",
                Headers = string.Empty,
                TimeoutSeconds = 100,  // default of 100 seconds
                TimeOutText = "100",
            };
            var obj = new object();
            obj = null;
            //------------Execute Test---------------------------
            var actual = dsfWebGetActivity.Equals(obj);
            //------------Assert Results-------------------------
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfWebGetRequestWithTimeoutActivity))]
        public void DsfWebGetRequestWithTimeoutActivity_Equals_ObjectType_IsEqualToThis_IsTrue()
        {
            //------------Setup for test--------------------------
            var dsfWebGetActivity = new DsfWebGetRequestWithTimeoutActivity
            {
                Method = "GET",
                Headers = string.Empty,
                TimeoutSeconds = 100,  // default of 100 seconds
                TimeOutText = "100",
            };
            var obj = new object();
            obj = dsfWebGetActivity;
            //------------Execute Test---------------------------
            var actual = dsfWebGetActivity.Equals(obj);
            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
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

        public IExecutionEnvironment ExecuteSubRequest(IDSFDataObject dataObject, Guid workspaceID, string inputDefs, string outputDefs,
                                      out ErrorResultTO errors, int update, bool b)
        {
            errors = new ErrorResultTO();
            return dataObject.Environment;
        }
    }
}
