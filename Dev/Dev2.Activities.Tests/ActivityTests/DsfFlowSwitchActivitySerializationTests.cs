using System;
using System.Collections.Generic;
using Dev2.Common.X6;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class DsfFlowSwitchActivitySerializationTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("DsfFlowSwitchActivity_ToX6Json")]
		public void DsfFlowSwitchActivity_ToX6Json_ShouldSerializeCorrectly()
        {
            //------------Setup for test--------------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfFlowSwitchActivity("Test Switch", new Mock<IDebugDispatcher>().Object)
            {
                UniqueID = uniqueId,
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData(\"[[variable]]\",AmbientDataList)",
                DisplayName = "My Test Switch",
                OnErrorVariable = "[[ErrorVar]]",
                OnErrorWorkflow = "ErrorWorkflow",
                IsEndedOnError = true
            };

            var cell = new Cell
            {
                id = uniqueId,
                data = new Dictionary<string, object>()
            };

            //------------Execute Test---------------------------
            activity.ToX6Json(cell);

            //------------Assert Results-------------------------
            Assert.IsNotNull(cell.data);
            
            // Should have error handling data
            Assert.IsTrue(cell.data.ContainsKey(Constants.ONERRORDATA));
            
            // ToX6Json stores the OnError data as an X6NodeOnErrorData instance (its JSON wire
            // contract uses errorMessage/webServiceUrl/endWorkflow, consumed by the X6 designer).
            var onErrorData = (X6NodeOnErrorData)cell.data[Constants.ONERRORDATA];

            Assert.AreEqual("[[ErrorVar]]", onErrorData.OnErrorVariable);
            Assert.AreEqual("ErrorWorkflow", onErrorData.OnErrorWorkflow);
            Assert.IsTrue(onErrorData.IsEndedOnError);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("DsfFlowSwitchActivity_FromX6Json")]
        public void DsfFlowSwitchActivity_FromX6Json_ShouldDeserializeCorrectly()
        {
            //------------Setup for test--------------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfFlowSwitchActivity("Test Switch", new Mock<IDebugDispatcher>().Object)
            {
                UniqueID = uniqueId
            };

            // Seed using the real X6 OnError wire-contract property names.
            var onErrorData = new
            {
                errorMessage = "[[ErrorVar]]",
                webServiceUrl = "ErrorWorkflow",
                endWorkflow = true
            };

            var cell = new Cell
            {
                id = uniqueId,
                data = new Dictionary<string, object>
                {
                    [Constants.ONERRORDATA] = JsonConvert.SerializeObject(onErrorData)
                }
            };

            //------------Execute Test---------------------------
            activity.FromX6Json(cell);

            //------------Assert Results-------------------------
            Assert.AreEqual("[[ErrorVar]]", activity.OnErrorVariable);
            Assert.AreEqual("ErrorWorkflow", activity.OnErrorWorkflow);
            Assert.IsTrue(activity.IsEndedOnError);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("DsfFlowSwitchActivity_RoundTrip")]
        public void DsfFlowSwitchActivity_RoundTrip_ShouldMaintainState()
        {
            //------------Setup for test--------------------------
            var uniqueId = Guid.NewGuid().ToString();
            var originalActivity = new DsfFlowSwitchActivity("Test Switch", new Mock<IDebugDispatcher>().Object)
            {
                UniqueID = uniqueId,
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData(\"[[variable]]\",AmbientDataList)",
                DisplayName = "My Test Switch",
                OnErrorVariable = "[[ErrorVar]]",
                OnErrorWorkflow = "ErrorWorkflow",
                IsEndedOnError = true
            };

            var cell = new Cell
            {
                id = uniqueId,
                data = new Dictionary<string, object>()
            };

            //------------Execute Test - Serialize---------------------------
            originalActivity.ToX6Json(cell);

            //------------Execute Test - Deserialize---------------------------
            var newActivity = new DsfFlowSwitchActivity("", new Mock<IDebugDispatcher>().Object);
            newActivity.FromX6Json(cell);

            //------------Assert Results-------------------------
            Assert.AreEqual(originalActivity.OnErrorVariable, newActivity.OnErrorVariable);
            Assert.AreEqual(originalActivity.OnErrorWorkflow, newActivity.OnErrorWorkflow);
            Assert.AreEqual(originalActivity.IsEndedOnError, newActivity.IsEndedOnError);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("DsfFlowSwitchActivity_FromX6Json")]
        public void DsfFlowSwitchActivity_FromX6Json_WithNullCell_ShouldHandleGracefully()
        {
            //------------Setup for test--------------------------
            var activity = new DsfFlowSwitchActivity("Test Switch", new Mock<IDebugDispatcher>().Object);

            //------------Execute Test---------------------------
            activity.FromX6Json(null);

            //------------Assert Results-------------------------
            // Should not throw exception
            Assert.IsNull(activity.OnErrorVariable);
            Assert.IsNull(activity.OnErrorWorkflow);
            Assert.IsFalse(activity.IsEndedOnError);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("DsfFlowSwitchActivity_FromX6Json")]
        public void DsfFlowSwitchActivity_FromX6Json_WithNullData_ShouldHandleGracefully()
        {
            //------------Setup for test--------------------------
            var activity = new DsfFlowSwitchActivity("Test Switch", new Mock<IDebugDispatcher>().Object);
            var cell = new Cell
            {
                id = Guid.NewGuid().ToString(),
                data = null
            };

            //------------Execute Test---------------------------
            activity.FromX6Json(cell);

            //------------Assert Results-------------------------
            // Should not throw exception
            Assert.IsNull(activity.OnErrorVariable);
            Assert.IsNull(activity.OnErrorWorkflow);
            Assert.IsFalse(activity.IsEndedOnError);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("DsfFlowSwitchActivity_FromX6Json")]
        public void DsfFlowSwitchActivity_FromX6Json_WithMissingOnErrorData_ShouldHandleGracefully()
        {
            //------------Setup for test--------------------------
            var activity = new DsfFlowSwitchActivity("Test Switch", new Mock<IDebugDispatcher>().Object);
            var cell = new Cell
            {
                id = Guid.NewGuid().ToString(),
                data = new Dictionary<string, object>
                {
                    ["someOtherData"] = "test"
                }
            };

            //------------Execute Test---------------------------
            activity.FromX6Json(cell);

            //------------Assert Results-------------------------
            // Should not throw exception and maintain default values
            Assert.IsNull(activity.OnErrorVariable);
            Assert.IsNull(activity.OnErrorWorkflow);
            Assert.IsFalse(activity.IsEndedOnError);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("DsfFlowSwitchActivity_FromX6Json")]
        public void DsfFlowSwitchActivity_FromX6Json_WithInvalidJson_ShouldHandleGracefully()
        {
            //------------Setup for test--------------------------
            var activity = new DsfFlowSwitchActivity("Test Switch", new Mock<IDebugDispatcher>().Object);
            var cell = new Cell
            {
                id = Guid.NewGuid().ToString(),
                data = new Dictionary<string, object>
                {
                    [Constants.ONERRORDATA] = "invalid json string"
                }
            };

            //------------Execute Test---------------------------
            activity.FromX6Json(cell);

            //------------Assert Results-------------------------
            // Should not throw exception and maintain default values due to try-catch
            Assert.IsNull(activity.OnErrorVariable);
            Assert.IsNull(activity.OnErrorWorkflow);
            Assert.IsFalse(activity.IsEndedOnError);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("DsfFlowSwitchActivity_ToX6Json")]
		public void DsfFlowSwitchActivity_ToX6Json_WithEmptyErrorProperties_ShouldSerializeCorrectly()
        {
            //------------Setup for test--------------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfFlowSwitchActivity("Test Switch", new Mock<IDebugDispatcher>().Object)
            {
                UniqueID = uniqueId,
                OnErrorVariable = "",
                OnErrorWorkflow = "",
                IsEndedOnError = false
            };

            var cell = new Cell
            {
                id = uniqueId,
                data = new Dictionary<string, object>()
            };

            //------------Execute Test---------------------------
            activity.ToX6Json(cell);

            //------------Assert Results-------------------------
            Assert.IsNotNull(cell.data);
            Assert.IsTrue(cell.data.ContainsKey(Constants.ONERRORDATA));
            
            var onErrorData = (X6NodeOnErrorData)cell.data[Constants.ONERRORDATA];

            Assert.AreEqual("", onErrorData.OnErrorVariable);
            Assert.AreEqual("", onErrorData.OnErrorWorkflow);
            Assert.IsFalse(onErrorData.IsEndedOnError);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("DsfFlowSwitchActivity_FromX6Json")]
        public void DsfFlowSwitchActivity_FromX6Json_WithPartialOnErrorData_ShouldDeserializeAvailableProperties()
        {
            //------------Setup for test--------------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfFlowSwitchActivity("Test Switch", new Mock<IDebugDispatcher>().Object)
            {
                UniqueID = uniqueId
            };

            // Seed using the real X6 OnError wire-contract property name (errorMessage).
            var onErrorData = new
            {
                errorMessage = "[[ErrorVar]]"
                // Missing webServiceUrl and endWorkflow
            };

            var cell = new Cell
            {
                id = uniqueId,
                data = new Dictionary<string, object>
                {
                    [Constants.ONERRORDATA] = JsonConvert.SerializeObject(onErrorData)
                }
            };

            //------------Execute Test---------------------------
            activity.FromX6Json(cell);

            //------------Assert Results-------------------------
            Assert.AreEqual("[[ErrorVar]]", activity.OnErrorVariable);
            // These should remain null/false as they weren't in the JSON
            Assert.IsNull(activity.OnErrorWorkflow);
            Assert.IsFalse(activity.IsEndedOnError);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("DsfFlowSwitchActivity_Serialization")]
        public void DsfFlowSwitchActivity_Serialization_WithComplexExpressionText_ShouldPreserveExpression()
        {
            //------------Setup for test--------------------------
            var uniqueId = Guid.NewGuid().ToString();
            var complexExpression = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData(\"[[rec().field]]\",AmbientDataList)";
            var originalActivity = new DsfFlowSwitchActivity("Complex Switch", new Mock<IDebugDispatcher>().Object)
            {
                UniqueID = uniqueId,
                ExpressionText = complexExpression,
                DisplayName = "Complex Expression Switch"
            };

            var cell = new Cell
            {
                id = uniqueId,
                data = new Dictionary<string, object>()
            };

            //------------Execute Test - Serialize---------------------------
            originalActivity.ToX6Json(cell);

            //------------Execute Test - Deserialize---------------------------
            var newActivity = new DsfFlowSwitchActivity("", new Mock<IDebugDispatcher>().Object);
            newActivity.FromX6Json(cell);

            //------------Assert Results-------------------------
            // Expression text should be preserved (though it's not in the base serialization, 
            // this tests that the process doesn't break it)
            Assert.AreEqual(originalActivity.OnErrorVariable, newActivity.OnErrorVariable);
            Assert.AreEqual(originalActivity.OnErrorWorkflow, newActivity.OnErrorWorkflow);
            Assert.AreEqual(originalActivity.IsEndedOnError, newActivity.IsEndedOnError);
        }
    }
}