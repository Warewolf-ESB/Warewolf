using System;
using System.Activities.Statements;
using Dev2.Communication;
using Dev2.Studio.Interfaces;
using Dev2.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.MergeParser.Tests
{
    public class ParserTestHelper
    {
        public static IContextualResourceModel CreateContextualResourceModel(Flowchart chart)
        {
            var workflowHelper = new WorkflowHelper();
            var tempResource = new Mock<IContextualResourceModel>();

            var builder = workflowHelper.CreateWorkflow("bob");
            builder.Implementation = chart;
            var tempResourceWorkflowXaml = workflowHelper.GetXamlDefinition(builder);
            var mock = new Mock<IServer>();
            mock.Setup(server => server.ResourceRepository.FetchResourceDefinition(It.IsAny<IServer>(),
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .Returns(new ExecuteMessage
                {
                    HasError = false,
                    Message = tempResourceWorkflowXaml
                });
            tempResource.Setup(model => model.Environment).Returns(mock.Object);
            tempResource.Setup(model => model.Category).Returns(@"Unassigned\" + "bob");
            tempResource.Setup(model => model.ResourceName).Returns("bob");
            tempResource.Setup(model => model.DisplayName).Returns("bob");
            tempResource.Setup(model => model.IsNewWorkflow).Returns(true);
            tempResource.Setup(model => model.WorkflowXaml).Returns(tempResourceWorkflowXaml);

            return tempResource.Object;
        }
    }
}
