using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Core.Tests.Environments;
using Dev2.Studio.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FlowControllerTests
    {
        [TestMethod]
        [TestCategory("FlowController_UnitTest")]
        [Description("Handling a configure decision expression message with isnew false will display the decision wizard")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void FlowController_HandleConfigureDecisionExpressionMessageAndIsNewFalse_WizardShown()
        // ReSharper restore InconsistentNaming
        {
            #region setup first Mock ModelItem

            var env = EnviromentRepositoryTest.CreateMockEnvironment();

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = new DsfFlowDecisionActivity { ExpressionText = "Not Null Test Value" };

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Condition", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Condition", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);

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

            #region setup Start Decision Wizard

            var flowController = new Mock<FlowController>(new Mock<IPopupController>().Object);
            flowController.Protected().Setup("StartDecisionWizard", ItExpr.IsAny<IEnvironmentModel>(), ItExpr.IsAny<string>()).Verifiable();

            #endregion

            flowController.Object.Handle(new ConfigureDecisionExpressionMessage { ModelItem = source.Object, EnvironmentModel = env.Object, IsNew = false });

            flowController.Protected().Verify("StartDecisionWizard", Times.Once(), ItExpr.IsAny<IEnvironmentModel>(), ItExpr.IsAny<string>());
        }

        [TestMethod]
        [TestCategory("FlowController_UnitTest")]
        [Description("Handling a configure switch expression message with isnew false will display the switch wizard")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void FlowController_HandleConfigureSwitchExpressionMessageAndIsNewFalse_WizardShown()
        // ReSharper restore InconsistentNaming
        {
            #region setup first Mock ModelItem

            var env = EnviromentRepositoryTest.CreateMockEnvironment();

            var crm = new Mock<IContextualResourceModel>();
            crm.Setup(r => r.Environment).Returns(env.Object);
            crm.Setup(r => r.ResourceName).Returns("Test");
            crm.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.xmlServiceDefinition));

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = new DsfFlowSwitchActivity { ExpressionText = "Not Null Test Value" };

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Expression", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Expression", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);

            #endregion

            #region setup switch Mock ModelItem

            var decisionProperties = new Dictionary<string, Mock<ModelProperty>>();
            var decisionPropertyCollection = new Mock<ModelPropertyCollection>();

            var decisionProp = new Mock<ModelProperty>();
            decisionProp.Setup(p => p.ComputedValue).Returns(string.Empty);
            decisionProperties.Add("ExpressionText", decisionProp);

            decisionPropertyCollection.Protected().Setup<ModelProperty>("Find", "ExpressionText", true).Returns(decisionProp.Object);

            var decisionModelItem = new Mock<ModelItem>();
            decisionModelItem.Setup(s => s.Properties).Returns(decisionPropertyCollection.Object);

            prop.Setup(p => p.Value).Returns(decisionModelItem.Object);

            #endregion

            #region setup Environment Model

            env.Setup(c => c.Connection).Returns(new Mock<IEnvironmentConnection>().Object);

            #endregion

            #region setup Start Decision Wizard

            var flowController = new Mock<FlowController>(new Mock<IPopupController>().Object);
            flowController.Protected().Setup("StartSwitchDropWizard", ItExpr.IsAny<IEnvironmentModel>(), ItExpr.IsAny<string>()).Verifiable();

            #endregion

            flowController.Object.Handle(new ConfigureSwitchExpressionMessage { ModelItem = source.Object, EnvironmentModel = env.Object, IsNew = false });

            flowController.Protected().Verify("StartSwitchDropWizard", Times.Once(), ItExpr.IsAny<IEnvironmentModel>(), ItExpr.IsAny<string>());
        }

        [TestMethod]
        [TestCategory("FlowController_UnitTest")]
        [Description("Handling a configure decision expression message with isnew true will not display the decision wizard")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void FlowController_HandleConfigureDecisionExpressionMessageAndIsNewTrue_WizardNotShown()
        // ReSharper restore InconsistentNaming
        {
            #region setup first Mock ModelItem

            var env = EnviromentRepositoryTest.CreateMockEnvironment();

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var testAct = new DsfFlowDecisionActivity { ExpressionText = "Not Null Test Value" };

            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);
            properties.Add("Condition", prop);

            propertyCollection.Protected().Setup<ModelProperty>("Find", "Condition", true).Returns(prop.Object);

            var source = new Mock<ModelItem>();
            source.Setup(s => s.Properties).Returns(propertyCollection.Object);

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

            var flowController = new FlowController(new Mock<IPopupController>().Object);
            var message = new ConfigureDecisionExpressionMessage { ModelItem = source.Object, EnvironmentModel = env.Object, IsNew = true };

            flowController.Handle(message);
        }
    }
}
