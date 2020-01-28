/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Designers2.Core;
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
using Warewolf.Data.Options.Enums;
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
        public void GateDesignerViewModel_Constructor_ShowLarge_True()
        {
            var conditionExpressionList = new List<ConditionExpression>();
            //------------Setup for test--------------------------
            var gateOptionsProperty = CreateModelProperty("GateOptions", null).Object;
            var conditionsProperty = CreateModelProperty("Conditions", conditionExpressionList).Object;
            var retryEntryPointIdProperty = CreateModelProperty("RetryEntryPointId", Guid.Empty).Object;

            var mockProperties = new Mock<ModelPropertyCollection>();
            mockProperties.Protected().Setup<ModelProperty>("Find", "GateOptions", true).Returns(gateOptionsProperty);
            mockProperties.Protected().Setup<ModelProperty>("Find", "Conditions", true).Returns(conditionsProperty);
            mockProperties.Protected().Setup<ModelProperty>("Find", "RetryEntryPointId", true).Returns(retryEntryPointIdProperty);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(modelItem => modelItem.Properties).Returns(mockProperties.Object);
            IsItemDragged.Instance.IsDragged = true;
            //------------Execute Test----------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(gateDesignerViewModel.HasLargeView);
            Assert.IsTrue(gateDesignerViewModel.Enabled);
            Assert.IsTrue(gateDesignerViewModel.ShowLarge);
            Assert.AreEqual(Visibility.Visible, gateDesignerViewModel.ThumbVisibility);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_Constructor_ShowLarge_False()
        {
            var conditionExpressionList = new List<ConditionExpression>();
            //------------Setup for test--------------------------
            var gateOptionsProperty = CreateModelProperty("GateOptions", null).Object;
            var conditionsProperty = CreateModelProperty("Conditions", conditionExpressionList).Object;
            var retryEntryPointIdProperty = CreateModelProperty("RetryEntryPointId", Guid.Empty).Object;

            var mockProperties = new Mock<ModelPropertyCollection>();
            mockProperties.Protected().Setup<ModelProperty>("Find", "GateOptions", true).Returns(gateOptionsProperty);
            mockProperties.Protected().Setup<ModelProperty>("Find", "Conditions", true).Returns(conditionsProperty);
            mockProperties.Protected().Setup<ModelProperty>("Find", "RetryEntryPointId", true).Returns(retryEntryPointIdProperty);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(modelItem => modelItem.Properties).Returns(mockProperties.Object);
            //------------Execute Test----------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(gateDesignerViewModel.HasLargeView);
            Assert.IsTrue(gateDesignerViewModel.Enabled);
            Assert.IsFalse(gateDesignerViewModel.ShowLarge);
            Assert.AreEqual(Visibility.Collapsed, gateDesignerViewModel.ThumbVisibility);
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
            var conditionExpressionList = new List<ConditionExpression>();

            var gateOptionsProperty = CreateModelProperty("GateOptions", null).Object;
            var conditionsProperty = CreateModelProperty("Conditions", conditionExpressionList).Object;
            var retryEntryPointIdProperty = CreateModelProperty("RetryEntryPointId", Guid.Empty).Object;

            var mockPropertyCollection = new Mock<ModelPropertyCollection>();
            mockPropertyCollection.Protected().Setup<ModelProperty>("Find", uniqueId, true).Returns(mockModelProperty.Object);
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
            var conditionExpressionList = new List<ConditionExpression>();

            var gateOptionsProperty = CreateModelProperty("GateOptions", null);
            var conditionsProperty = CreateModelProperty("Conditions", conditionExpressionList);
            var retryEntryPointIdProperty = CreateModelProperty("RetryEntryPointId", Guid.Empty);

            var mockPropertyCollection = new Mock<ModelPropertyCollection>();
            mockPropertyCollection.Protected().Setup<ModelProperty>("Find", uniqueId, true).Returns(mockModelProperty.Object);
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

            Assert.IsNotNull(gateDesignerViewModel.SelectedGate);
            Assert.AreEqual(Guid.Empty.ToString(), gateDesignerViewModel.SelectedGate.Value);

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
            var gateOptions = new GateOptions();
            var conditionExpressionList = new List<ConditionExpression>();

            var gateOptionsProperty = CreateModelProperty("GateOptions", gateOptions).Object;
            var conditionsProperty = CreateModelProperty("Conditions", conditionExpressionList);
            var retryEntryPointIdProperty = CreateModelProperty("RetryEntryPointId", Guid.Empty).Object;

            var mockProperties = new Mock<ModelPropertyCollection>();
            mockProperties.Protected().Setup<ModelProperty>("Find", "GateOptions", true).Returns(gateOptionsProperty);
            mockProperties.Protected().Setup<ModelProperty>("Find", "Conditions", true).Returns(conditionsProperty.Object);
            mockProperties.Protected().Setup<ModelProperty>("Find", "RetryEntryPointId", true).Returns(retryEntryPointIdProperty);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(modelItem => modelItem.Properties).Returns(mockProperties.Object);
            //------------Execute Test---------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);
            var options = gateDesignerViewModel.Options.Options.ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, options.Count);
            Assert.AreEqual(typeof(OptionRadioButtons), options[0].GetType());
            Assert.AreEqual("GateOpts", options[0].Name);

            var comboOptions = (options[0] as OptionRadioButtons).Options.ToList();

            Assert.AreEqual(2, comboOptions.Count);
            Assert.AreEqual("Continue", comboOptions[0].Key);
            Assert.AreEqual("EndWorkflow", comboOptions[1].Key);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_LoadConditions()
        {
            //------------Setup for test--------------------------
            var conditionExpression = new ConditionExpression
            {
                Left = "[[a]]"
            };
            conditionExpression.Cond = new ConditionMatch
            {
                MatchType = enDecisionType.IsEqual,
                Right = "10"
            };
            var conditionExpressionList = new List<ConditionExpression>
            {
                conditionExpression
            };

            var gateOptionsProperty = CreateModelProperty("GateOptions", null).Object;
            var conditionsProperty = CreateModelProperty("Conditions", conditionExpressionList);
            var retryEntryPointIdProperty = CreateModelProperty("RetryEntryPointId", Guid.Empty).Object;

            var mockProperties = new Mock<ModelPropertyCollection>();
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
            var conditionExpression = new ConditionExpression
            {
                Left = "[[a]]"
            };
            conditionExpression.Cond = new ConditionMatch
            {
                MatchType = enDecisionType.IsEqual,
                Right = "10"
            };
            var conditionExpressionList = new List<ConditionExpression>
            {
                conditionExpression
            };

            var gateOptionsProperty = CreateModelProperty("GateOptions", null).Object;
            var conditionsProperty = CreateModelProperty("Conditions", conditionExpressionList);
            var retryEntryPointIdProperty = CreateModelProperty("RetryEntryPointId", Guid.Empty).Object;

            var mockProperties = new Mock<ModelPropertyCollection>();
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
