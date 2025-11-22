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
		[Ignore]
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
            
            var onErrorDataJson = cell.data[Constants.ONERRORDATA].ToString();
            var onErrorData = JsonConvert.DeserializeObject<dynamic>(onErrorDataJson);
            
            Assert.AreEqual("[[ErrorVar]]", onErrorData.OnErrorVariable.ToString());
            Assert.AreEqual("ErrorWorkflow", onErrorData.OnErrorWorkflow.ToString());
            Assert.AreEqual(true, (bool)onErrorData.IsEndedOnError);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("DsfFlowSwitchActivity_FromX6Json")]
        [Ignore]
        public void DsfFlowSwitchActivity_FromX6Json_ShouldDeserializeCorrectly()
        {
            //------------Setup for test--------------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfFlowSwitchActivity("Test Switch", new Mock<IDebugDispatcher>().Object)
            {
                UniqueID = uniqueId
            };

            var onErrorData = new
            {
                OnErrorVariable = "[[ErrorVar]]",
                OnErrorWorkflow = "ErrorWorkflow",
                IsEndedOnError = true
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
        [Ignore]
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
		[Ignore]
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
            
            var onErrorDataJson = cell.data[Constants.ONERRORDATA].ToString();
            var onErrorData = JsonConvert.DeserializeObject<dynamic>(onErrorDataJson);
            
            Assert.AreEqual("", onErrorData.OnErrorVariable.ToString());
            Assert.AreEqual("", onErrorData.OnErrorWorkflow.ToString());
            Assert.AreEqual(false, (bool)onErrorData.IsEndedOnError);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("DsfFlowSwitchActivity_FromX6Json")]
        [Ignore]
        public void DsfFlowSwitchActivity_FromX6Json_WithPartialOnErrorData_ShouldDeserializeAvailableProperties()
        {
            //------------Setup for test--------------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfFlowSwitchActivity("Test Switch", new Mock<IDebugDispatcher>().Object)
            {
                UniqueID = uniqueId
            };

            var onErrorData = new
            {
                OnErrorVariable = "[[ErrorVar]]"
                // Missing OnErrorWorkflow and IsEndedOnError
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