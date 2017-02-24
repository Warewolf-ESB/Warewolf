using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Text;
using Dev2.Activities.Designers2.Switch;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Core.Tests;
using Dev2.Core.Tests.Environments;
using Dev2.Interfaces;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Switch
{
    [TestClass]
    public class SwitchDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SwitchDesignerViewModel_Initialize")]
        public void SwitchDesignerViewModel_Initialize_Setup_NotNull()
        {
            //------------Setup for test--------------------------
            #region setup first Mock ModelItem
            var popupController = new Mock<IPopupController>();
            CustomContainer.Register(popupController.Object);

            var env = EnviromentRepositoryTest.CreateMockEnvironment();

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = new DsfFlowDecisionActivity { ExpressionText = "Not Null Test Value" };

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Condition", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Condition", true).Returns(prop.Object);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(s => s.Properties).Returns(propertyCollection.Object);

            #endregion

            #region setup decision Mock ModelItem

            var crmDecision = new Mock<IContextualResourceModel>();
            crmDecision.Setup(r => r.Environment).Returns(env.Object);
            crmDecision.Setup(r => r.ResourceName).Returns("Test");
            crmDecision.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var decisionProperties = new Dictionary<string, Mock<ModelProperty>>();
            var decisionPropertyCollection = new Mock<ModelPropertyCollection>();

            var decisionProp = new Mock<ModelProperty>();
            decisionProp.Setup(p => p.ComputedValue).Returns(string.Empty);
            decisionProperties.Add("Condition", decisionProp);

            decisionPropertyCollection.Protected().Setup<ModelProperty>("Find", "Condition", true).Returns(decisionProp.Object);

            var decisionModelItem = new Mock<ModelItem>();
            decisionModelItem.Setup(s => s.Properties).Returns(decisionPropertyCollection.Object);
            decisionModelItem.Setup(s => s.ItemType).Returns(typeof(FlowDecision));

            prop.Setup(p => p.Value).Returns(decisionModelItem.Object);

            #endregion

            #region setup Environment Model

            env.Setup(c => c.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            #endregion
            
            //------------Execute Test---------------------------
            var switchDesigner = new SwitchDesignerViewModel(mockModelItem.Object, "");

            //------------Assert Results-------------------------
            Assert.IsNotNull(switchDesigner);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SwitchDesignerViewModel_DisplayName")]
        public void SwitchDesignerViewModel_DisplayName_Setup_HasValue()
        {
            //------------Setup for test--------------------------
            #region setup first Mock ModelItem
            var popupController = new Mock<IPopupController>();
            CustomContainer.Register(popupController.Object);

            var env = EnviromentRepositoryTest.CreateMockEnvironment();

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = new DsfFlowDecisionActivity { ExpressionText = "Not Null Test Value" };

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Condition", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Condition", true).Returns(prop.Object);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(s => s.Properties).Returns(propertyCollection.Object);

            #endregion

            #region setup decision Mock ModelItem

            var crmDecision = new Mock<IContextualResourceModel>();
            crmDecision.Setup(r => r.Environment).Returns(env.Object);
            crmDecision.Setup(r => r.ResourceName).Returns("Test");
            crmDecision.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var decisionProperties = new Dictionary<string, Mock<ModelProperty>>();
            var decisionPropertyCollection = new Mock<ModelPropertyCollection>();

            var decisionProp = new Mock<ModelProperty>();
            decisionProp.Setup(p => p.ComputedValue).Returns(string.Empty);
            decisionProperties.Add("Condition", decisionProp);

            decisionPropertyCollection.Protected().Setup<ModelProperty>("Find", "Condition", true).Returns(decisionProp.Object);

            var decisionModelItem = new Mock<ModelItem>();
            decisionModelItem.Setup(s => s.Properties).Returns(decisionPropertyCollection.Object);
            decisionModelItem.Setup(s => s.ItemType).Returns(typeof(FlowDecision));

            prop.Setup(p => p.Value).Returns(decisionModelItem.Object);

            #endregion

            #region setup Environment Model

            env.Setup(c => c.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            #endregion
            
            //------------Execute Test---------------------------
            var switchDesigner = new SwitchDesignerViewModel(mockModelItem.Object, "TrueArm");

            //------------Assert Results-------------------------
            Assert.IsNotNull(switchDesigner);
            Assert.AreEqual("TrueArm", switchDesigner.DisplayText);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SwitchDesignerViewModel_Handle")]
        public void SwitchDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            #region setup first Mock ModelItem
            var popupController = new Mock<IPopupController>();
            CustomContainer.Register(popupController.Object);

            var env = EnviromentRepositoryTest.CreateMockEnvironment();

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = new DsfFlowDecisionActivity { ExpressionText = "Not Null Test Value" };

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Condition", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Condition", true).Returns(prop.Object);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(s => s.Properties).Returns(propertyCollection.Object);

            #endregion

            #region setup decision Mock ModelItem

            var crmDecision = new Mock<IContextualResourceModel>();
            crmDecision.Setup(r => r.Environment).Returns(env.Object);
            crmDecision.Setup(r => r.ResourceName).Returns("Test");
            crmDecision.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var decisionProperties = new Dictionary<string, Mock<ModelProperty>>();
            var decisionPropertyCollection = new Mock<ModelPropertyCollection>();

            var decisionProp = new Mock<ModelProperty>();
            decisionProp.Setup(p => p.ComputedValue).Returns(string.Empty);
            decisionProperties.Add("Condition", decisionProp);

            decisionPropertyCollection.Protected().Setup<ModelProperty>("Find", "Condition", true).Returns(decisionProp.Object);

            var decisionModelItem = new Mock<ModelItem>();
            decisionModelItem.Setup(s => s.Properties).Returns(decisionPropertyCollection.Object);
            decisionModelItem.Setup(s => s.ItemType).Returns(typeof(FlowSwitch<string>));

            prop.Setup(p => p.Value).Returns(decisionModelItem.Object);

            #endregion

            #region setup Environment Model

            env.Setup(c => c.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            #endregion

            var switchDesigner = new SwitchDesignerViewModel(mockModelItem.Object, "");
            //------------Execute Test---------------------------
            switchDesigner.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SwitchDesignerViewModel_Switch")]
        public void SwitchDesignerViewModel_Switch_SetSwitchVariable()
        {
            //------------Setup for test--------------------------      
            #region setup first Mock ModelItem
            var popupController = new Mock<IPopupController>();
            CustomContainer.Register(popupController.Object);

            var env = EnviromentRepositoryTest.CreateMockEnvironment();

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = new DsfFlowSwitchActivity { ExpressionText = "Not Null Test Value" };

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Condition", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Condition", true).Returns(prop.Object);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(s => s.Properties).Returns(propertyCollection.Object);

            #endregion

            #region setup decision Mock ModelItem

            var crmDecision = new Mock<IContextualResourceModel>();
            crmDecision.Setup(r => r.Environment).Returns(env.Object);
            crmDecision.Setup(r => r.ResourceName).Returns("Test");
            crmDecision.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var decisionProperties = new Dictionary<string, Mock<ModelProperty>>();
            var decisionPropertyCollection = new Mock<ModelPropertyCollection>();

            var decisionProp = new Mock<ModelProperty>();
            decisionProp.Setup(p => p.ComputedValue).Returns(string.Empty);
            decisionProperties.Add("Condition", decisionProp);

            decisionPropertyCollection.Protected().Setup<ModelProperty>("Find", "Condition", true).Returns(decisionProp.Object);

            var decisionModelItem = new Mock<ModelItem>();
            decisionModelItem.Setup(s => s.Properties).Returns(decisionPropertyCollection.Object);
            decisionModelItem.Setup(s => s.ItemType).Returns(typeof(FlowSwitch<string>));

            prop.Setup(p => p.Value).Returns(decisionModelItem.Object);

            #endregion

            #region setup Environment Model

            env.Setup(c => c.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            #endregion

            //------------Execute Test---------------------------
            var switchDesigner = new SwitchDesignerViewModel(mockModelItem.Object, "Switch")
            {
                SwitchVariable = "TrueArm"
            };

            //------------Assert Results-------------------------

            Assert.AreEqual("TrueArm", switchDesigner.DisplayText);
            Assert.AreEqual("TrueArm", switchDesigner.SwitchVariable);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SwitchDesignerViewModel_Switch")]
        public void SwitchDesignerViewModel_Switch_Dev2Switch()
        {
            //------------Setup for test--------------------------      
            #region setup first Mock ModelItem
            var popupController = new Mock<IPopupController>();
            CustomContainer.Register(popupController.Object);

            var env = EnviromentRepositoryTest.CreateMockEnvironment();

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = new DsfFlowSwitchActivity { ExpressionText = "Not Null Test Value" };

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Condition", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Condition", true).Returns(prop.Object);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(s => s.Properties).Returns(propertyCollection.Object);

            #endregion

            #region setup decision Mock ModelItem

            var crmDecision = new Mock<IContextualResourceModel>();
            crmDecision.Setup(r => r.Environment).Returns(env.Object);
            crmDecision.Setup(r => r.ResourceName).Returns("Test");
            crmDecision.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var decisionProperties = new Dictionary<string, Mock<ModelProperty>>();
            var decisionPropertyCollection = new Mock<ModelPropertyCollection>();

            var decisionProp = new Mock<ModelProperty>();
            decisionProp.Setup(p => p.ComputedValue).Returns(string.Empty);
            decisionProperties.Add("Condition", decisionProp);

            decisionPropertyCollection.Protected().Setup<ModelProperty>("Find", "Condition", true).Returns(decisionProp.Object);

            var decisionModelItem = new Mock<ModelItem>();
            decisionModelItem.Setup(s => s.Properties).Returns(decisionPropertyCollection.Object);
            decisionModelItem.Setup(s => s.ItemType).Returns(typeof(FlowSwitch<string>));

            prop.Setup(p => p.Value).Returns(decisionModelItem.Object);

            #endregion

            #region setup Environment Model

            env.Setup(c => c.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            #endregion

            var switchDesigner = new SwitchDesignerViewModel(mockModelItem.Object, "TrueArm");
            //------------Execute Test---------------------------
            var dev2Switch = switchDesigner.Switch;
            //------------Assert Results-------------------------
            Assert.AreEqual("TrueArm", dev2Switch.DisplayText);
            Assert.AreEqual("", dev2Switch.SwitchVariable);
            Assert.IsNull(dev2Switch.SwitchExpression);
        }
    }
}
