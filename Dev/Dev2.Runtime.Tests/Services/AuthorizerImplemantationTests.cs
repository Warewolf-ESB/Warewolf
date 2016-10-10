using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Exceptions;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class AuthorizerImplemantationTests
    {

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SaveDbService_Execute")]
        public void SaveDbService_Execute_GivenNoServerPermissions_ExpectError()
        {
            //------------Setup for test--------------------------
            var authorizer = new Mock<IAuthorizer>();
            var notAuthorizedToCreateException = Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToCreateException;
            authorizer.Setup(authorizer1 => authorizer1.RunPermissions(GlobalConstants.ServerWorkspaceID))
                .Throws(new ServiceNotAuthorizedException(notAuthorizedToCreateException));
            var saveDbService = new SaveDbService(authorizer.Object);
            //---------------Assert Precondition----------------
            Assert.AreEqual("SaveDbService", saveDbService.HandlesType());
            //------------Execute Test---------------------------
            var stringBuilder = saveDbService.Execute(new Dictionary<string, StringBuilder>(), new Mock<IWorkspace>().Object);


            //------------Assert Results-------------------------
            var serializer = new Dev2JsonSerializer();
            var executeMessage = serializer.Deserialize<ExecuteMessage>(stringBuilder);
            Assert.IsTrue(executeMessage.HasError);
            Assert.AreEqual(notAuthorizedToCreateException,executeMessage.Message.ToString());
        }
    }
}