/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Core.Tests.Environments;
using Dev2.Studio.Controller;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class FlowControllerTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("FlowController_ConfigureSwitch")]
        public void FlowController_ConfigureSwitch_Handle_Switch()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            CustomContainer.Register(popupController.Object);

            var env = EnvironmentRepositoryTest.CreateMockEnvironment();

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = new DsfFlowSwitchActivity { ExpressionText = "" };

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Expression", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Expression", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);

            #region setup decision Mock ModelItem

            var crmSwitch = new Mock<IContextualResourceModel>();
            crmSwitch.Setup(r => r.Environment).Returns(env.Object);
            crmSwitch.Setup(r => r.ResourceName).Returns("Test");
            crmSwitch.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var switchProperties = new Dictionary<string, Mock<ModelProperty>>();
            var switchPropertyCollection = new Mock<ModelPropertyCollection>();

            var switchProp = new Mock<ModelProperty>();
            switchProp.Setup(p => p.ComputedValue).Returns(string.Empty);
            switchProperties.Add("Expression", switchProp);

            switchPropertyCollection.Protected().Setup<ModelProperty>("Find", "Expression", true).Returns(switchProp.Object);

            var switchModelItem = new Mock<ModelItem>();
            switchModelItem.Setup(s => s.Properties).Returns(switchPropertyCollection.Object);
            switchModelItem.Setup(s => s.ItemType).Returns(typeof(FlowSwitch<string>));

            prop.Setup(p => p.Value).Returns(switchModelItem.Object);

            #endregion

            #region setup Environment Model

            env.Setup(c => c.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            #endregion

            var flowController = new FlowController();

            var message = new ConfigureSwitchExpressionMessage
            {
                ModelItem = source.Object,
                Server = env.Object,
                IsNew = true
            };
            //------------Execute Test---------------------------
            flowController.Handle(message);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("FlowController_ConfigureSwitch")]
        public void FlowController_ConfigureSwitch_Handle_SwitchCase()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            CustomContainer.Register(popupController.Object);

            var env = EnvironmentRepositoryTest.CreateMockEnvironment();

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = new DsfFlowSwitchActivity { ExpressionText = "" };

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Expression", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Expression", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);

            #region setup decision Mock ModelItem

            var crmSwitch = new Mock<IContextualResourceModel>();
            crmSwitch.Setup(r => r.Environment).Returns(env.Object);
            crmSwitch.Setup(r => r.ResourceName).Returns("Test");
            crmSwitch.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var switchProperties = new Dictionary<string, Mock<ModelProperty>>();
            var switchPropertyCollection = new Mock<ModelPropertyCollection>();

            var switchProp = new Mock<ModelProperty>();
            switchProp.Setup(p => p.ComputedValue).Returns(string.Empty);
            switchProperties.Add("Expression", switchProp);

            switchPropertyCollection.Protected().Setup<ModelProperty>("Find", "Expression", true).Returns(switchProp.Object);

            var switchModelItem = new Mock<ModelItem>();
            switchModelItem.Setup(s => s.Properties).Returns(switchPropertyCollection.Object);
            switchModelItem.Setup(s => s.ItemType).Returns(typeof(FlowSwitch<string>));

            prop.Setup(p => p.Value).Returns(switchModelItem.Object);

            #endregion

            #region setup Environment Model

            env.Setup(c => c.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            #endregion

            var flowController = new FlowController();

            var message = new ConfigureCaseExpressionMessage
            {
                ModelItem = source.Object,
                Server = env.Object
            };
            //------------Execute Test---------------------------
            flowController.Handle(message);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("FlowController_ConfigureSwitch")]
        public void FlowController_ConfigureSwitch_Handle_EditSwitchCase()
        {
            //------------Setup for test--------------------------
            var popupController = new Mock<IPopupController>();
            CustomContainer.Register(popupController.Object);

            var env = EnvironmentRepositoryTest.CreateMockEnvironment();

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = new DsfFlowSwitchActivity { ExpressionText = "" };

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Expression", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Expression", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);

            #region setup decision Mock ModelItem

            var crmSwitch = new Mock<IContextualResourceModel>();
            crmSwitch.Setup(r => r.Environment).Returns(env.Object);
            crmSwitch.Setup(r => r.ResourceName).Returns("Test");
            crmSwitch.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var switchProperties = new Dictionary<string, Mock<ModelProperty>>();
            var switchPropertyCollection = new Mock<ModelPropertyCollection>();

            var switchProp = new Mock<ModelProperty>();
            switchProp.Setup(p => p.ComputedValue).Returns(string.Empty);
            switchProperties.Add("Expression", switchProp);

            switchPropertyCollection.Protected().Setup<ModelProperty>("Find", "Expression", true).Returns(switchProp.Object);

            var switchModelItem = new Mock<ModelItem>();
            switchModelItem.Setup(s => s.Properties).Returns(switchPropertyCollection.Object);
            switchModelItem.Setup(s => s.ItemType).Returns(typeof(FlowSwitch<string>));

            prop.Setup(p => p.Value).Returns(switchModelItem.Object);

            #endregion

            #region setup Environment Model

            env.Setup(c => c.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            #endregion

            var flowController = new FlowController();

            var message = new EditCaseExpressionMessage
            {
                ModelItem = source.Object,
                Server = env.Object
            };
            //------------Execute Test---------------------------
            flowController.Handle(message);
            //------------Assert Results-------------------------
        }
    }
}
