/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
    public class FlowControllerTests
    {
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

            var flowController = new FlowController();
            var message = new ConfigureDecisionExpressionMessage { ModelItem = source.Object, EnvironmentModel = env.Object, IsNew = true };

            flowController.Handle(message);
        }
    }
}
