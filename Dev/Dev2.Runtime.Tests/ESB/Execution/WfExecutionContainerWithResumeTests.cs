using System;
using System.Collections.Generic;
using System.Security.Principal;
using Dev2.Data.TO;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.ESB.Execution;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.DynamicServices;
using Dev2.Activities;
using Warewolf.Storage;
using System.Linq;
using System.Text;

namespace Dev2.Tests.Runtime.ESB.Execution
{
    [TestClass]
    public class WfExecutionContainerWithResumeTests
    {

        [TestMethod]
        public void ResumableExecutionContainer_Given_Start_From_AssignName_Tool()
        {
            var helloWolrdId = Guid.Parse("acb75027-ddeb-47d7-814e-a54c37247ec1");            
            var env = new ExecutionEnvironment();
            var serviceAction = new ServiceAction();
            var dataObj = new DsfDataObject("<DataList></DataList>", Guid.NewGuid())
            {
                ResourceID = helloWolrdId,
                ExecutingUser = SetupUser()
            };
            Mock<IWorkspace> theWorkspace = SetupWorkspace();
            var esbChannel = new Mock<IEsbChannel>();
            var errors = new ErrorResultTO();
            var parser = new ActivityParser();
            CustomContainer.Register<IActivityParser>(parser);
            var assingNameTool = Guid.Parse("bd557ca7-113b-4197-afc3-de5d086dfc69");
            var resumableExecution = new ResumableExecutionContainer(assingNameTool, dataObj.Environment, serviceAction, dataObj, theWorkspace.Object, esbChannel.Object);
            Assert.IsNotNull(resumableExecution);
            resumableExecution.Execute(out errors, 0);
            var hasErrors = dataObj.Environment.HasErrors();
            Assert.IsFalse(hasErrors, GetAllErrors(dataObj.Environment.Errors));
            var output = dataObj.Environment.Eval("[[Message]]", 0);
            var message = WarewolfDataEvaluationCommon.evalResultToString(output);
            Assert.AreEqual("Hello World.", message);
        }

        private string GetAllErrors(HashSet<string> allErrors)
        {
            var errors = new StringBuilder();
            errors.AppendLine("Expected no errors but found; ");
            foreach (var item in allErrors)
            {
                errors.AppendLine(item);
            }
            return errors.ToString();
        }

        [TestMethod]
        public void ResumableExecutionContainer_Given_No_Name_And_Start_From_AssignMessage_Tool()
        {
            var helloWolrdId = Guid.Parse("acb75027-ddeb-47d7-814e-a54c37247ec1");
            var env = new ExecutionEnvironment();
            var serviceAction = new ServiceAction();
            var dataObj = new DsfDataObject("<DataList></DataList>", Guid.NewGuid());
            dataObj.ResourceID = helloWolrdId;
            dataObj.ExecutingUser = SetupUser();
            Mock<IWorkspace> theWorkspace = SetupWorkspace();
            var esbChannel = new Mock<IEsbChannel>();
            var errors = new ErrorResultTO();
            var parser = new ActivityParser();
            CustomContainer.Register<IActivityParser>(parser);
            var assingMessageTool = Guid.Parse("670132e7-80d4-4e41-94af-ba4a71b28118");
            var resumableExecution = new ResumableExecutionContainer(assingMessageTool, env, serviceAction, dataObj, theWorkspace.Object, esbChannel.Object);
            Assert.IsNotNull(resumableExecution);
            resumableExecution.Execute(out errors, 0);
            Assert.IsTrue(dataObj.Environment.HasErrors());
            Assert.AreEqual(1, dataObj.Environment.Errors.Count);
            Assert.IsNotNull(dataObj.Environment, "The environment is null");
            Assert.IsNotNull(dataObj.Environment.Errors, "All Errors in the given Environment is Null");
            Assert.IsTrue(dataObj.Environment.Errors.Count > 0, "There are no Errors in the environmnet");
            Assert.AreEqual("Scalar value { Name } is NULL", dataObj.Environment.Errors.FirstOrDefault());
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
