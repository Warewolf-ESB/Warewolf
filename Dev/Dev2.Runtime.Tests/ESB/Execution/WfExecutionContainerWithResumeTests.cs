using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Xml.Linq;
using Castle.Core.Resource;
using Dev2.Common;
using Dev2.Data.TO;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage.Interfaces;
using Dev2.Common.Common;
using Dev2.DynamicServices;
using Dev2.Activities;

namespace Dev2.Tests.Runtime.ESB.Execution
{
    [TestClass]
    public class WfExecutionContainerWithResumeTests
    {

        [TestMethod]
        public void ResumableExecutionContainer_Given_Nodes_And_Nodes_To_Start_From()
        {
            var resumeWorkflowId = Guid.Parse("acb75027-ddeb-47d7-814e-a54c37247ec1");
            var env = new Mock<IExecutionEnvironment>();
            var serviceAction = new ServiceAction();
            var dataObj = new DsfDataObject("<DataList></DataList>", Guid.NewGuid());
            dataObj.ResourceID = resumeWorkflowId;
            dataObj.ExecutingUser = SetupUser();
            Mock<IWorkspace> theWorkspace = SetupWorkspace();
            var esbChannel = new Mock<IEsbChannel>();
            var errors = new ErrorResultTO();
            var parser = new ActivityParser();
            CustomContainer.Register<IActivityParser>(parser);
            var resource = ResourceCatalog.Instance.GetResource(Guid.Empty, resumeWorkflowId);

            var resumeActivityId = Guid.Empty;
            var resumableExecution = new ResumableExecutionContainer(resumeActivityId, env.Object, serviceAction, dataObj, theWorkspace.Object, esbChannel.Object);
            Assert.IsNotNull(resumableExecution);
            resumableExecution.Execute(out errors, 0);
        }        

        private IPrincipal SetupUser()
        {
            var user = new Mock<IPrincipal>();
            var id = new Mock<IIdentity>();
            id.Setup(i => i.Name).Returns("test user");
            user.Setup(p => p.Identity).Returns(id.Object);
            user.Setup(p => p.IsInRole(It.IsAny<string>())).Returns(true);
            return user.Object;
        }

        private static Mock<IWorkspace> SetupWorkspace()
        {
            var theWorkspace = new Mock<IWorkspace>();
            var wsItems = new List<IWorkspaceItem>();
            wsItems.Add(new Mock<IWorkspaceItem>().Object);
            theWorkspace.Setup(ws => ws.Items).Returns(wsItems);
            return theWorkspace;
        }
    }
}
