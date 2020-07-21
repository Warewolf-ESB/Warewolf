/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Execution;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

namespace Dev2.Tests.Runtime.ESB.Execution
{
    [TestClass]
    [TestCategory("Runtime ESB")]
    public class InternalServiceContainerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnConstruction_GivenValidArgs_ShouldBuildCorrectly()
        {
            //---------------Set up test pack-------------------
            const string datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            var serviceAction = new ServiceAction() { DataListSpecification = new StringBuilder(datalist) };
            var dsfObj = new Mock<IDSFDataObject>();
            dsfObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>()))
                  .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("Args")));
            var workSpace = new Mock<IWorkspace>();
            var channel = new Mock<IEsbChannel>();
            var esbExecuteRequest = new EsbExecuteRequest();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var internalServiceContainer = new InternalServiceContainer(serviceAction, dsfObj.Object, workSpace.Object, channel.Object, esbExecuteRequest, null);
            //---------------Test Result -----------------------
            Assert.AreEqual(4, esbExecuteRequest.Args.Count);
            Assert.IsNotNull(internalServiceContainer, "Cannot create new InternalServiceContainer object.");

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GenerateRequestDictionaryFromDataObject_GivenValidArgs_ShouldClearArgsAndErros()
        {
            const string datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            var serviceAction = new ServiceAction() { DataListSpecification = new StringBuilder(datalist) };
            var dsfObj = new Mock<IDSFDataObject>();
            dsfObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>()))
                  .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("Args")));
            var workSpace = new Mock<IWorkspace>();
            var channel = new Mock<IEsbChannel>();
            var esbExecuteRequest = new EsbExecuteRequest();
            var internalServiceContainer = new InternalServiceContainer(serviceAction, dsfObj.Object, workSpace.Object, channel.Object, esbExecuteRequest, null);
            var privateObject = new PrivateObject(internalServiceContainer);
            //---------------Assert Precondition----------------
            Assert.AreEqual(4, esbExecuteRequest.Args.Count);
            //---------------Execute Test ----------------------
            var errorResultTO = new ErrorResultTO();
            privateObject.Invoke("GenerateRequestDictionaryFromDataObject", errorResultTO);

            //---------------Test Result -----------------------
            Assert.AreEqual(0, esbExecuteRequest.Args.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenNullService_ShouldAddValidError()
        {
            //---------------Set up test pack-------------------
            const string datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            var serviceAction = new ServiceAction() { DataListSpecification = new StringBuilder(datalist) , ServiceName = "name", Name = "Name"};
            var dsfObj = new Mock<IDSFDataObject>();
            dsfObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>()))
                  .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("Args")));
            var workSpace = new Mock<IWorkspace>();
            var channel = new Mock<IEsbChannel>();
            var esbExecuteRequest = new EsbExecuteRequest();
            var internalServiceContainer = new InternalServiceContainer(serviceAction, dsfObj.Object, workSpace.Object, channel.Object, esbExecuteRequest, null);
            //---------------Assert Precondition----------------
            Assert.AreEqual(4, esbExecuteRequest.Args.Count);
            //---------------Execute Test ----------------------
            internalServiceContainer.Execute(out ErrorResultTO errorResultTO, 1);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, errorResultTO.FetchErrors().Count);
            Assert.AreEqual(string.Format(ErrorResource.CouldNotLocateManagementService, "name"), errorResultTO.FetchErrors().Single());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenService_ShouldAddBuildRequestArgs()
        {
            //---------------Set up test pack-------------------
            const string datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            var serviceAction = new ServiceAction() { DataListSpecification = new StringBuilder(datalist) , ServiceName = "name", Name = "Name"};
            var dsfObj = new Mock<IDSFDataObject>();
            dsfObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>()))
                  .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("Args")));
            var workSpace = new Mock<IWorkspace>();
            var channel = new Mock<IEsbChannel>();
            var esbExecuteRequest = new EsbExecuteRequest();
            var locater = new Mock<IEsbManagementServiceLocator>();
            locater.Setup(loc => loc.LocateManagementService("Name")).Returns(new FetchPluginSources());
            var internalServiceContainer = new InternalServiceContainer(serviceAction, dsfObj.Object, workSpace.Object, channel.Object, esbExecuteRequest, locater.Object, null);
            //---------------Assert Precondition----------------
            Assert.AreEqual(4, esbExecuteRequest.Args.Count);
            //---------------Execute Test ----------------------
            var execute = internalServiceContainer.Execute(out ErrorResultTO errorResultTO, 1);
            //---------------Test Result -----------------------
            locater.VerifyAll();            
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void InternalServiceContainer_CanExecute_GivenAllowedService_ShouldAllowExecute()
        {
            const bool allowCanExecute = true;
            const AuthorizationContext endpointAuthorizationLevel = AuthorizationContext.Any;
            const bool isInRole = true;
            var mockPrincipal = new Mock<IPrincipal>();
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(o => o.Name).Returns("bob");
            mockIdentity.Setup(o => o.IsAuthenticated).Returns(isInRole);
            mockPrincipal.Setup(o => o.Identity).Returns(mockIdentity.Object);
            mockPrincipal.Setup(o => o.IsInRole(It.IsAny<string>())).Returns(isInRole);
            Common.Utilities.OrginalExecutingUser = mockPrincipal.Object;

            var mockEsbManagementEndpoint = new Mock<IEsbManagementEndpoint>();
            mockEsbManagementEndpoint.Setup(o => o.CanExecute(It.IsAny<CanExecuteArg>())).Returns(allowCanExecute);
            var resourceId = Guid.NewGuid();
            var requestArgs = new Dictionary<string, StringBuilder>
            {
                {"ResourceID", new StringBuilder(resourceId.ToString())}
            };
            mockEsbManagementEndpoint.Setup(o => o.GetResourceID(requestArgs)).Returns(resourceId);
            mockEsbManagementEndpoint.Setup(o => o.GetAuthorizationContextForService())
                .Returns(endpointAuthorizationLevel);

            var esbExecuteRequest = new EsbExecuteRequest();
            var errorResultTo = InternalServiceContainer_CanExecute_GivenAllowedService(mockEsbManagementEndpoint.Object, esbExecuteRequest);

            mockEsbManagementEndpoint.Verify(o => o.CanExecute(It.IsAny<CanExecuteArg>()), Times.Once);
            mockEsbManagementEndpoint.Verify(o => o.Execute(It.IsAny<Dictionary<string,StringBuilder>>(), It.IsAny<IWorkspace>()), Times.Once);

            Assert.AreEqual(0, errorResultTo.FetchErrors().Count);

            Assert.IsNull(esbExecuteRequest.ExecuteResult);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void InternalServiceContainer_CanExecute_GivenAllowedService_ShouldNotAllowExecute()
        {
            if (string.IsNullOrWhiteSpace(Config.Cluster.LeaderServerKey))
            {
                Config.Cluster.LeaderServerKey = "123456";
            }
            var allowCanExecute = false;
            var endpointAuthorizationLevel = AuthorizationContext.Any;

            var mockEsbManagementEndpoint = new Mock<IEsbManagementEndpoint>();
            mockEsbManagementEndpoint.Setup(o => o.CanExecute(It.IsAny<CanExecuteArg>())).Returns(allowCanExecute);
            var resourceID = Guid.NewGuid();
            var requestArgs = new Dictionary<string, StringBuilder>();
            requestArgs.Add("ResourceID", new StringBuilder(resourceID.ToString()));
            mockEsbManagementEndpoint.Setup(o => o.GetResourceID(requestArgs)).Returns(resourceID);
            mockEsbManagementEndpoint.Setup(o => o.GetAuthorizationContextForService())
                .Returns(endpointAuthorizationLevel);

            var esbExecuteRequest = new EsbExecuteRequest();
            var errorResultTO = InternalServiceContainer_CanExecute_GivenAllowedService(mockEsbManagementEndpoint.Object, esbExecuteRequest);

            mockEsbManagementEndpoint.Verify(o => o.CanExecute(It.IsAny<CanExecuteArg>()), Times.Once);
            mockEsbManagementEndpoint.Verify(o => o.Execute(It.IsAny<Dictionary<string,StringBuilder>>(), It.IsAny<IWorkspace>()), Times.Never);

            var errors = errorResultTO.FetchErrors();
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ErrorResource.NotAuthorizedToExecuteOnFollower, errors[0]);

            var result = esbExecuteRequest.ExecuteResult.ToString();
            var serializer = new Dev2JsonSerializer();
            var r = serializer.Deserialize<ExecuteMessage>(result);
            Assert.IsTrue(r.HasError);
            Assert.AreEqual(ErrorResource.NotAuthorizedToExecuteOnFollower, r.Message.ToString());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void InternalServiceContainer_CanExecute_GivenDirectDeployOnFollowerService_ShouldNotAllowExecute()
        {
            if (string.IsNullOrWhiteSpace(Config.Cluster.LeaderServerKey))
            {
                Config.Cluster.LeaderServerKey = "123456";
            }
            var directDeployEndpoint = new DirectDeploy();
            var mockEsbManagementEndpoint = new Mock<IEsbManagementEndpoint>();
            mockEsbManagementEndpoint
                .Setup(o => o.CanExecute(It.IsAny<CanExecuteArg>()))
                .Returns<CanExecuteArg>((arg) => directDeployEndpoint.CanExecute(arg));
            var resourceID = Guid.NewGuid();
            var requestArgs = new Dictionary<string, StringBuilder>();
            requestArgs.Add("ResourceID", new StringBuilder(resourceID.ToString()));
            mockEsbManagementEndpoint.Setup(o => o.GetResourceID(requestArgs)).Returns(resourceID);
            mockEsbManagementEndpoint.Setup(o => o.GetAuthorizationContextForService())
                .Returns(directDeployEndpoint.GetAuthorizationContextForService());

            var esbExecuteRequest = new EsbExecuteRequest();
            var errorResultTO = InternalServiceContainer_CanExecute_GivenAllowedService(mockEsbManagementEndpoint.Object, esbExecuteRequest);

            mockEsbManagementEndpoint.Verify(o => o.CanExecute(It.IsAny<CanExecuteArg>()), Times.Once);
            mockEsbManagementEndpoint.Verify(o => o.Execute(It.IsAny<Dictionary<string,StringBuilder>>(), It.IsAny<IWorkspace>()), Times.Never);

            var errors = errorResultTO.FetchErrors();
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ErrorResource.NotAuthorizedToExecuteOnFollower, errors[0]);

            var result = esbExecuteRequest.ExecuteResult.ToString();
            var serializer = new Dev2JsonSerializer();
            var r = serializer.Deserialize<ExecuteMessage>(result);
            Assert.IsTrue(r.HasError);
            Assert.AreEqual(ErrorResource.NotAuthorizedToExecuteOnFollower, r.Message.ToString());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void InternalServiceContainer_CanExecute_GivenSaveOnFollowerService_ShouldNotAllowExecute()
        {
            if (string.IsNullOrWhiteSpace(Config.Cluster.LeaderServerKey))
            {
                Config.Cluster.LeaderServerKey = "123456";
            }
            var directDeployEndpoint = new SaveResource();
            var mockEsbManagementEndpoint = new Mock<IEsbManagementEndpoint>();
            mockEsbManagementEndpoint
                .Setup(o => o.CanExecute(It.IsAny<CanExecuteArg>()))
                .Returns<CanExecuteArg>((arg) => directDeployEndpoint.CanExecute(arg));
            var resourceID = Guid.NewGuid();
            var requestArgs = new Dictionary<string, StringBuilder>();
            requestArgs.Add("ResourceID", new StringBuilder(resourceID.ToString()));
            mockEsbManagementEndpoint.Setup(o => o.GetResourceID(requestArgs)).Returns(resourceID);
            mockEsbManagementEndpoint.Setup(o => o.GetAuthorizationContextForService())
                .Returns(directDeployEndpoint.GetAuthorizationContextForService());

            var esbExecuteRequest = new EsbExecuteRequest();
            var errorResultTO = InternalServiceContainer_CanExecute_GivenAllowedService(mockEsbManagementEndpoint.Object, esbExecuteRequest);

            mockEsbManagementEndpoint.Verify(o => o.CanExecute(It.IsAny<CanExecuteArg>()), Times.Once);
            mockEsbManagementEndpoint.Verify(o => o.Execute(It.IsAny<Dictionary<string,StringBuilder>>(), It.IsAny<IWorkspace>()), Times.Never);

            var errors = errorResultTO.FetchErrors();
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ErrorResource.NotAuthorizedToExecuteOnFollower, errors[0]);

            var result = esbExecuteRequest.ExecuteResult.ToString();
            var serializer = new Dev2JsonSerializer();
            var r = serializer.Deserialize<ExecuteMessage>(result);
            Assert.IsTrue(r.HasError);
            Assert.AreEqual(ErrorResource.NotAuthorizedToExecuteOnFollower, r.Message.ToString());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void InternalServiceContainer_CanExecute_GivenDeployOnFollowerService_ShouldNotAllowExecute()
        {
            if (string.IsNullOrWhiteSpace(Config.Cluster.LeaderServerKey))
            {
                Config.Cluster.LeaderServerKey = "123456";
            }
            var directDeployEndpoint = new DeployResource();
            var mockEsbManagementEndpoint = new Mock<IEsbManagementEndpoint>();
            mockEsbManagementEndpoint
                .Setup(o => o.CanExecute(It.IsAny<CanExecuteArg>()))
                .Returns<CanExecuteArg>((arg) => directDeployEndpoint.CanExecute(arg));
            var resourceID = Guid.NewGuid();
            var requestArgs = new Dictionary<string, StringBuilder>();
            requestArgs.Add("ResourceID", new StringBuilder(resourceID.ToString()));
            mockEsbManagementEndpoint.Setup(o => o.GetResourceID(requestArgs)).Returns(resourceID);
            mockEsbManagementEndpoint.Setup(o => o.GetAuthorizationContextForService())
                .Returns(directDeployEndpoint.GetAuthorizationContextForService());

            var esbExecuteRequest = new EsbExecuteRequest();
            var errorResultTO = InternalServiceContainer_CanExecute_GivenAllowedService(mockEsbManagementEndpoint.Object, esbExecuteRequest);

            mockEsbManagementEndpoint.Verify(o => o.CanExecute(It.IsAny<CanExecuteArg>()), Times.Once);
            mockEsbManagementEndpoint.Verify(o => o.Execute(It.IsAny<Dictionary<string,StringBuilder>>(), It.IsAny<IWorkspace>()), Times.Never);

            var errors = errorResultTO.FetchErrors();
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(ErrorResource.NotAuthorizedToExecuteOnFollower, errors[0]);

            var result = esbExecuteRequest.ExecuteResult.ToString();
            var serializer = new Dev2JsonSerializer();
            var r = serializer.Deserialize<ExecuteMessage>(result);
            Assert.IsTrue(r.HasError);
            Assert.AreEqual(ErrorResource.NotAuthorizedToExecuteOnFollower, r.Message.ToString());
        }

        private ErrorResultTO InternalServiceContainer_CanExecute_GivenAllowedService(IEsbManagementEndpoint esbManagementEndpoint, EsbExecuteRequest esbExecuteRequest)
        {
            //---------------Set up test pack-------------------
            const string datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            var serviceAction = new ServiceAction() { DataListSpecification = new StringBuilder(datalist) , ServiceName = "name", Name = "Name"};
            var dsfObj = new Mock<IDSFDataObject>();
            dsfObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("Args")));
            var mockWorkSpace = new Mock<IWorkspace>();
            var channel = new Mock<IEsbChannel>();
            var locater = new Mock<IEsbManagementServiceLocator>();

            locater.Setup(loc => loc.LocateManagementService("Name")).Returns(esbManagementEndpoint);
            var workSpace = mockWorkSpace.Object;
            var internalServiceContainer = new InternalServiceContainer(serviceAction, dsfObj.Object, workSpace, channel.Object, esbExecuteRequest, locater.Object, null);
            //---------------Assert Precondition----------------
            Assert.AreEqual(4, esbExecuteRequest.Args.Count);
            //---------------Execute Test ----------------------
            var execute = internalServiceContainer.Execute(out ErrorResultTO errorResultTO, 1);
            //---------------Test Result -----------------------
            locater.VerifyAll();

            return errorResultTO;
        }
    }
}
