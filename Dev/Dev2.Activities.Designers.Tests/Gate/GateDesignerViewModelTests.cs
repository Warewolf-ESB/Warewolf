/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Designers2.Gate;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Warewolf.Data;
using Warewolf.Data.Options;
using Warewolf.Options;

namespace Dev2.Activities.Designers.Tests.Gate
{
    [TestClass]
    public class GateDesignerViewModelTests
    {
        [TestInitialize]
        public void SetupForTest()
        {
            var optionBool = new OptionBool();
            var optionsList = new List<IOption> { optionBool };

            var mockServer = new Mock<IServer>();

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindOptionsBy(mockServer.Object, "")).Returns(optionsList);

            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(shellViewModel => shellViewModel.ActiveServer).Returns(mockServer.Object);

            CustomContainer.Register(mockShellViewModel.Object);
        }

        private Mock<ModelProperty> CreateModelProperty(string name, object value)
        {
            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.Name).Returns(name);
            prop.Setup(p => p.ComputedValue).Returns(value);
            return prop;
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_Constructor()
        {
            //------------Setup for test--------------------------
            var gateFailureProperty = CreateModelProperty("GateFailure", null);
            var gateOptionsProperty = CreateModelProperty("GateOptions", null).Object;
            var conditionsProperty = CreateModelProperty("Conditions", null).Object;
            var retryEntryPointIdProperty = CreateModelProperty("RetryEntryPointId", Guid.Empty).Object;

            var mockProperties = new Mock<ModelPropertyCollection>();
            mockProperties.Protected().Setup<ModelProperty>("Find", "GateFailure", true).Returns(gateFailureProperty.Object);
            mockProperties.Protected().Setup<ModelProperty>("Find", "GateOptions", true).Returns(gateOptionsProperty);
            mockProperties.Protected().Setup<ModelProperty>("Find", "Conditions", true).Returns(conditionsProperty);
            mockProperties.Protected().Setup<ModelProperty>("Find", "RetryEntryPointId", true).Returns(retryEntryPointIdProperty);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(modelItem => modelItem.Properties).Returns(mockProperties.Object);
            var expectedGateFailure = "StopOnError: Stop execution on error";
            //------------Execute Test----------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(gateDesignerViewModel.HasLargeView);
            Assert.AreEqual(expectedGateFailure, gateDesignerViewModel.SelectedGateFailure);
            Assert.IsTrue(gateDesignerViewModel.Enabled);
            Assert.IsTrue(gateDesignerViewModel.ShowLarge);
            Assert.AreEqual(Visibility.Visible, gateDesignerViewModel.ThumbVisibility);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_GateFailureOptions_Retry()
        {
            //------------Setup for test--------------------------
            var gateFailureProperty = CreateModelProperty("GateFailure", null);
            var gateOptionsProperty = CreateModelProperty("GateOptions", null).Object;
            var conditionsProperty = CreateModelProperty("Conditions", null).Object;
            var retryEntryPointIdProperty = CreateModelProperty("RetryEntryPointId", Guid.Empty).Object;

            var mockProperties = new Mock<ModelPropertyCollection>();
            mockProperties.Protected().Setup<ModelProperty>("Find", "GateFailure", true).Returns(gateFailureProperty.Object);
            mockProperties.Protected().Setup<ModelProperty>("Find", "GateOptions", true).Returns(gateOptionsProperty);
            mockProperties.Protected().Setup<ModelProperty>("Find", "Conditions", true).Returns(conditionsProperty);
            mockProperties.Protected().Setup<ModelProperty>("Find", "RetryEntryPointId", true).Returns(retryEntryPointIdProperty);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(modelItem => modelItem.Properties).Returns(mockProperties.Object);
            CustomContainer.Register(mockModelItem.Object);
            //------------Execute Test---------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object)
            {
                SelectedGateFailure = "Retry: Retry execution on error"
            };
            //------------Assert Results-------------------------
            Assert.AreEqual("Retry: Retry execution on error", gateDesignerViewModel.SelectedGateFailure);
            Assert.IsTrue(gateDesignerViewModel.Enabled);
            gateFailureProperty.Verify(prop => prop.SetValue("StopOnError"), Times.Exactly(1));
            gateFailureProperty.Verify(prop => prop.SetValue("Retry"), Times.Exactly(1));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_GateFailureOptions_StopOnError()
        {
            //------------Setup for test--------------------------
            var gateFailureProperty = CreateModelProperty("GateFailure", "Retry");
            var gateOptionsProperty = CreateModelProperty("GateOptions", null).Object;
            var conditionsProperty = CreateModelProperty("Conditions", null).Object;
            var retryEntryPointIdProperty = CreateModelProperty("RetryEntryPointId", Guid.Empty).Object;

            var mockProperties = new Mock<ModelPropertyCollection>();
            mockProperties.Protected().Setup<ModelProperty>("Find", "GateFailure", true).Returns(gateFailureProperty.Object);
            mockProperties.Protected().Setup<ModelProperty>("Find", "GateOptions", true).Returns(gateOptionsProperty);
            mockProperties.Protected().Setup<ModelProperty>("Find", "Conditions", true).Returns(conditionsProperty);
            mockProperties.Protected().Setup<ModelProperty>("Find", "RetryEntryPointId", true).Returns(retryEntryPointIdProperty);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(modelItem => modelItem.Properties).Returns(mockProperties.Object);
            CustomContainer.Register(mockModelItem.Object);
            //------------Execute Test---------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object)
            {
                SelectedGateFailure = "StopOnError: Stop execution on error"
            };
            //------------Assert Results-------------------------
            Assert.AreEqual("StopOnError: Stop execution on error", gateDesignerViewModel.SelectedGateFailure);
            Assert.IsTrue(gateDesignerViewModel.Enabled);
            gateFailureProperty.Verify(prop => prop.SetValue("Retry"), Times.Exactly(1));
            gateFailureProperty.Verify(prop => prop.SetValue("StopOnError"), Times.Exactly(1));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_GateFailureOptions_List()
        {
            //------------Setup for test--------------------------
            var gateFailureProperty = CreateModelProperty("GateFailure", null);
            var gateOptionsProperty = CreateModelProperty("GateOptions", null).Object;
            var conditionsProperty = CreateModelProperty("Conditions", null).Object;
            var retryEntryPointIdProperty = CreateModelProperty("RetryEntryPointId", Guid.Empty).Object;

            var mockProperties = new Mock<ModelPropertyCollection>();
            mockProperties.Protected().Setup<ModelProperty>("Find", "GateFailure", true).Returns(gateFailureProperty.Object);
            mockProperties.Protected().Setup<ModelProperty>("Find", "GateOptions", true).Returns(gateOptionsProperty);
            mockProperties.Protected().Setup<ModelProperty>("Find", "Conditions", true).Returns(conditionsProperty);
            mockProperties.Protected().Setup<ModelProperty>("Find", "RetryEntryPointId", true).Returns(retryEntryPointIdProperty);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(modelItem => modelItem.Properties).Returns(mockProperties.Object);
            //------------Execute Test---------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);
            var gateFailureOptions = gateDesignerViewModel.GateFailureOptions.ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, gateFailureOptions.Count);
            Assert.AreEqual("Retry: Retry execution on error", gateFailureOptions[0]);
            Assert.AreEqual("StopOnError: Stop execution on error", gateFailureOptions[1]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_GatesView()
        {
            var expected = string.Empty;
            var uniqueId = Guid.NewGuid().ToString();
            var activityName = "testActivity";
            var gates = new List<Common.Interfaces.NameValue>
            {
                new Common.Interfaces.NameValue { Name = activityName, Value = uniqueId }
            };

            var mockWorkflowDesignerViewModel = new Mock<IWorkflowDesignerViewModel>();
            mockWorkflowDesignerViewModel.Setup(workflowDesigner => workflowDesigner.GetSelectableGates(uniqueId)).Returns(gates);

            var mockModelProperty = new Mock<ModelProperty>();
            mockModelProperty.Setup(p => p.SetValue(expected)).Verifiable();
            var properties = new Dictionary<string, Mock<ModelProperty>>
            {
                { uniqueId, mockModelProperty }
            };

            var gateFailureProperty = CreateModelProperty("GateFailure", null);
            var gateOptionsProperty = CreateModelProperty("GateOptions", null).Object;
            var conditionsProperty = CreateModelProperty("Conditions", null).Object;
            var retryEntryPointIdProperty = CreateModelProperty("RetryEntryPointId", Guid.Empty).Object;

            var mockPropertyCollection = new Mock<ModelPropertyCollection>();
            mockPropertyCollection.Protected().Setup<ModelProperty>("Find", uniqueId, true).Returns(mockModelProperty.Object);
            mockPropertyCollection.Protected().Setup<ModelProperty>("Find", "GateFailure", true).Returns(gateFailureProperty.Object);
            mockPropertyCollection.Protected().Setup<ModelProperty>("Find", "GateOptions", true).Returns(gateOptionsProperty);
            mockPropertyCollection.Protected().Setup<ModelProperty>("Find", "Conditions", true).Returns(conditionsProperty);
            mockPropertyCollection.Protected().Setup<ModelProperty>("Find", "RetryEntryPointId", true).Returns(retryEntryPointIdProperty);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(modelItem => modelItem.Properties).Returns(mockPropertyCollection.Object);

            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);

            Assert.AreEqual(1, gateDesignerViewModel.Gates.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_Gates_SelectedGate()
        {
            var expected = string.Empty;
            var uniqueId = Guid.NewGuid().ToString();
            var activityName = "testActivity";
            var gates = new List<Common.Interfaces.NameValue>
            {
                new Common.Interfaces.NameValue { Name = activityName, Value = uniqueId }
            };

            var mockModelProperty = new Mock<ModelProperty>();
            mockModelProperty.Setup(p => p.SetValue(expected)).Verifiable();
            var properties = new Dictionary<string, Mock<ModelProperty>>
            {
                { uniqueId, mockModelProperty }
            };

            var gateFailureProperty = CreateModelProperty("GateFailure", null);
            var gateOptionsProperty = CreateModelProperty("GateOptions", null);
            var conditionsProperty = CreateModelProperty("Conditions", null);
            var retryEntryPointIdProperty = CreateModelProperty("RetryEntryPointId", Guid.Empty);

            var mockPropertyCollection = new Mock<ModelPropertyCollection>();
            mockPropertyCollection.Protected().Setup<ModelProperty>("Find", uniqueId, true).Returns(mockModelProperty.Object);
            mockPropertyCollection.Protected().Setup<ModelProperty>("Find", "GateFailure", true).Returns(gateFailureProperty.Object);
            mockPropertyCollection.Protected().Setup<ModelProperty>("Find", "GateOptions", true).Returns(gateOptionsProperty.Object);
            mockPropertyCollection.Protected().Setup<ModelProperty>("Find", "Conditions", true).Returns(conditionsProperty.Object);
            mockPropertyCollection.Protected().Setup<ModelProperty>("Find", "RetryEntryPointId", true).Returns(retryEntryPointIdProperty.Object);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(modelItem => modelItem.Properties).Returns(mockPropertyCollection.Object);

            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);

            Assert.AreEqual(1, gateDesignerViewModel.Gates.Count);

            gateDesignerViewModel.Gates = gates;

            Assert.AreEqual(1, gateDesignerViewModel.Gates.Count);

            Assert.AreEqual(uniqueId, gateDesignerViewModel.Gates[0].Value);
            Assert.AreEqual(activityName, gateDesignerViewModel.Gates[0].Name);

            Assert.IsNull(gateDesignerViewModel.SelectedGate);

            gateDesignerViewModel.SelectedGate = gates[0];

            Assert.AreEqual(uniqueId, gateDesignerViewModel.SelectedGate.Value);

            retryEntryPointIdProperty.Verify(prop => prop.SetValue(Guid.Parse(uniqueId)), Times.Exactly(1));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_LoadOptions()
        {
            //------------Setup for test--------------------------
            var retryEntryPointId = Guid.NewGuid();
            var expectedWorkflow = new WorkflowWithInputs
            {
                Name = "WorkflowName",
                Value = retryEntryPointId,
                Inputs = new List<IServiceInputBase>()
            };
            var gateOptions = new GateOptions
            {
                Resume = Resumable.AllowResumption,
                Count = 3,
                ResumeEndpoint = expectedWorkflow,
                Strategy = new NoBackoff()
            };

            var gateFailureProperty = CreateModelProperty("GateFailure", null);
            var gateOptionsProperty = CreateModelProperty("GateOptions", gateOptions).Object;
            var conditionsProperty = CreateModelProperty("Conditions", null);
            var retryEntryPointIdProperty = CreateModelProperty("RetryEntryPointId", Guid.Empty).Object;

            var mockProperties = new Mock<ModelPropertyCollection>();
            mockProperties.Protected().Setup<ModelProperty>("Find", "GateFailure", true).Returns(gateFailureProperty.Object);
            mockProperties.Protected().Setup<ModelProperty>("Find", "GateOptions", true).Returns(gateOptionsProperty);
            mockProperties.Protected().Setup<ModelProperty>("Find", "Conditions", true).Returns(conditionsProperty.Object);
            mockProperties.Protected().Setup<ModelProperty>("Find", "RetryEntryPointId", true).Returns(retryEntryPointIdProperty);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(modelItem => modelItem.Properties).Returns(mockProperties.Object);
            //------------Execute Test---------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);
            var options = gateDesignerViewModel.Options.Options.ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(4, options.Count);
            Assert.AreEqual(typeof(OptionEnum), options[0].GetType());
            Assert.AreEqual("Resume", options[0].Name);

            Assert.AreEqual(typeof(OptionWorkflow), options[1].GetType());
            Assert.AreEqual("ResumeEndpoint", options[1].Name);

            Assert.AreEqual(typeof(OptionInt), options[2].GetType());
            Assert.AreEqual("Count", options[2].Name);

            Assert.AreEqual(typeof(OptionCombobox), options[3].GetType());
            Assert.AreEqual("Strategy", options[3].Name);
            
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_LoadConditions()
        {
            //------------Setup for test--------------------------
            var retryEntryPointId = Guid.NewGuid();

            var conditionExpression = new ConditionExpression
            {
                Left = "[[a]]"
            };
            conditionExpression.Cond = new ConditionMatch
            {
                MatchType = enDecisionType.IsEqual,
                Right = "10"
            };

            var gateFailureProperty = CreateModelProperty("GateFailure", null);
            var gateOptionsProperty = CreateModelProperty("GateOptions", null).Object;
            var conditionsProperty = CreateModelProperty("Conditions", conditionExpression);
            var retryEntryPointIdProperty = CreateModelProperty("RetryEntryPointId", Guid.Empty).Object;

            var mockProperties = new Mock<ModelPropertyCollection>();
            mockProperties.Protected().Setup<ModelProperty>("Find", "GateFailure", true).Returns(gateFailureProperty.Object);
            mockProperties.Protected().Setup<ModelProperty>("Find", "GateOptions", true).Returns(gateOptionsProperty);
            mockProperties.Protected().Setup<ModelProperty>("Find", "Conditions", true).Returns(conditionsProperty.Object);
            mockProperties.Protected().Setup<ModelProperty>("Find", "RetryEntryPointId", true).Returns(retryEntryPointIdProperty);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(modelItem => modelItem.Properties).Returns(mockProperties.Object);
            //------------Execute Test---------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);

            var conditions = gateDesignerViewModel.ConditionExpressionOptions.Options.ToList();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, conditions.Count);

            var condition = conditions[0] as OptionConditionExpression;
            Assert.AreEqual("[[a]]", condition.Left);
            Assert.AreEqual(enDecisionType.IsEqual, condition.MatchType);
            Assert.AreEqual("10", condition.Right);

            var emptyCondition = conditions[1] as OptionConditionExpression;
            Assert.IsNull(emptyCondition.Left);
            Assert.AreEqual(enDecisionType.Choose, emptyCondition.MatchType);

        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_DeleteCondition()
        {
            //------------Setup for test--------------------------
            var retryEntryPointId = Guid.NewGuid();

            var conditionExpression = new ConditionExpression
            {
                Left = "[[a]]"
            };
            conditionExpression.Cond = new ConditionMatch
            {
                MatchType = enDecisionType.IsEqual,
                Right = "10"
            };

            var gateFailureProperty = CreateModelProperty("GateFailure", null);
            var gateOptionsProperty = CreateModelProperty("GateOptions", null).Object;
            var conditionsProperty = CreateModelProperty("Conditions", conditionExpression);
            var retryEntryPointIdProperty = CreateModelProperty("RetryEntryPointId", Guid.Empty).Object;

            var mockProperties = new Mock<ModelPropertyCollection>();
            mockProperties.Protected().Setup<ModelProperty>("Find", "GateFailure", true).Returns(gateFailureProperty.Object);
            mockProperties.Protected().Setup<ModelProperty>("Find", "GateOptions", true).Returns(gateOptionsProperty);
            mockProperties.Protected().Setup<ModelProperty>("Find", "Conditions", true).Returns(conditionsProperty.Object);
            mockProperties.Protected().Setup<ModelProperty>("Find", "RetryEntryPointId", true).Returns(retryEntryPointIdProperty);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(modelItem => modelItem.Properties).Returns(mockProperties.Object);
            //------------Execute Test---------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);

            var conditions = gateDesignerViewModel.ConditionExpressionOptions.Options.ToList();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, conditions.Count);

            var optionConditionExpression = conditions[0] as OptionConditionExpression;
            optionConditionExpression.SelectedMatchType = new NamedInt { Name = "IsEqual", Value = 19 };
            optionConditionExpression.DeleteCommand.Execute(optionConditionExpression);

            conditions = gateDesignerViewModel.ConditionExpressionOptions.Options.ToList();

            Assert.AreEqual(1, conditions.Count);

            var emptyCondition = conditions[0] as OptionConditionExpression;
            Assert.IsNull(emptyCondition.Left);
            Assert.AreEqual(enDecisionType.Choose, emptyCondition.MatchType);

        }
    }
}
