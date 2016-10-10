using System;
using Dev2.Runtime.ESB.Management;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.ESB
{
    [TestClass]
    public class SecuredCreateManagementEndpointTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Create_GivenAuthService_ShouldWork()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAuthorizationService>();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            // ReSharper disable once ObjectCreationAsStatement
            new SecuredCreateEndpoint(mock.Object);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RunPermissions_GivenAuthServiceContibuteFalse_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAuthorizationService>();
            var newGuid = Guid.NewGuid();
            mock.Setup(service => service.IsAuthorized(AuthorizationContext.Contribute, newGuid.ToString())).Returns(false);
            var managementEndpoint = new SecuredCreateEndpoint(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(managementEndpoint);
            //---------------Execute Test ----------------------
            try
            {
                managementEndpoint.RunPermissions(newGuid);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToCreateException);
                mock.Verify(service => service.IsAuthorized(AuthorizationContext.Contribute, newGuid.ToString()));
            }

            //---------------Test Result -----------------------
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void View_GivenAuthService_ShouldWork()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAuthorizationService>();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            // ReSharper disable once ObjectCreationAsStatement
            new SecuredViewManagementEndpoint(mock.Object);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Contribute_GivenAuthService_ShouldWork()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAuthorizationService>();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            // ReSharper disable once ObjectCreationAsStatement
            new SecuredContributeManagementEndpoint(mock.Object);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DeployFrom_GivenAuthService_ShouldWork()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAuthorizationService>();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            // ReSharper disable once ObjectCreationAsStatement
            new SecuredDeployFromManagementEndpoint(mock.Object);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DeployTo_GivenAuthService_ShouldWork()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAuthorizationService>();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            // ReSharper disable once ObjectCreationAsStatement
            new SecuredDeployToManagementEndpoint(mock.Object);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenAuthService_ShouldWork()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAuthorizationService>();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            // ReSharper disable once ObjectCreationAsStatement
            new SecuredExecuteManagementEndpoint(mock.Object);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Administrator_GivenAuthService_ShouldWork()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAuthorizationService>();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            // ReSharper disable once ObjectCreationAsStatement
            new SecuredAdministratorManagementEndpoint(mock.Object);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RunPermissionsContribute_GivenAuthServiceContibuteFalse_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAuthorizationService>();
            var newGuid = Guid.NewGuid();
            mock.Setup(service => service.IsAuthorized(AuthorizationContext.Contribute, newGuid.ToString())).Returns(false);
            var managementEndpoint = new SecuredContributeManagementEndpoint(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(managementEndpoint);
            //---------------Execute Test ----------------------
            try
            {
                managementEndpoint.RunPermissions(newGuid);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToContributeException);
                mock.Verify(service => service.IsAuthorized(AuthorizationContext.Contribute, newGuid.ToString()));
            }

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RunPermissionsAdministrator_GivenAuthServiceContibuteFalse_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAuthorizationService>();
            var newGuid = Guid.NewGuid();
            mock.Setup(service => service.IsAuthorized(AuthorizationContext.Administrator, newGuid.ToString())).Returns(false);
            var managementEndpoint = new SecuredAdministratorManagementEndpoint(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(managementEndpoint);
            //---------------Execute Test ----------------------
            try
            {
                managementEndpoint.RunPermissions(newGuid);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToAdministratorException);
                mock.Verify(service => service.IsAuthorized(AuthorizationContext.Administrator, newGuid.ToString()));
            }

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RunPermissionsDeployTo_GivenAuthServiceContibuteFalse_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAuthorizationService>();
            var newGuid = Guid.NewGuid();
            mock.Setup(service => service.IsAuthorized(AuthorizationContext.DeployTo, newGuid.ToString())).Returns(false);
            var managementEndpoint = new SecuredDeployToManagementEndpoint(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(managementEndpoint);
            //---------------Execute Test ----------------------
            try
            {
                managementEndpoint.RunPermissions(newGuid);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToDeployToException);
                mock.Verify(service => service.IsAuthorized(AuthorizationContext.DeployTo, newGuid.ToString()));
            }

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RunPermissionsDeployFrom_GivenAuthServiceContibuteFalse_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAuthorizationService>();
            var newGuid = Guid.NewGuid();
            mock.Setup(service => service.IsAuthorized(AuthorizationContext.DeployFrom, newGuid.ToString())).Returns(false);
            var managementEndpoint = new SecuredDeployFromManagementEndpoint(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(managementEndpoint);
            //---------------Execute Test ----------------------
            try
            {
                managementEndpoint.RunPermissions(newGuid);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToDeployFromException);
                mock.Verify(service => service.IsAuthorized(AuthorizationContext.DeployFrom, newGuid.ToString()));
            }

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RunPermissionsExecute_GivenAuthServiceContibuteFalse_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAuthorizationService>();
            var newGuid = Guid.NewGuid();
            mock.Setup(service => service.IsAuthorized(AuthorizationContext.Execute, newGuid.ToString())).Returns(false);
            var managementEndpoint = new SecuredExecuteManagementEndpoint(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(managementEndpoint);
            //---------------Execute Test ----------------------
            try
            {
                managementEndpoint.RunPermissions(newGuid);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToExecuteException);
                mock.Verify(service => service.IsAuthorized(AuthorizationContext.Execute, newGuid.ToString()));
            }

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RunPermissionsView_GivenAuthServiceContibuteFalse_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IAuthorizationService>();
            var newGuid = Guid.NewGuid();
            mock.Setup(service => service.IsAuthorized(AuthorizationContext.View, newGuid.ToString())).Returns(false);
            var managementEndpoint = new SecuredViewManagementEndpoint(mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(managementEndpoint);
            //---------------Execute Test ----------------------
            try
            {
                managementEndpoint.RunPermissions(newGuid);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToViewException);
                mock.Verify(service => service.IsAuthorized(AuthorizationContext.View, newGuid.ToString()));
            }

            //---------------Test Result -----------------------
        }
    }
}
