using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
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
            var internalServiceContainer = new InternalServiceContainer(serviceAction, dsfObj.Object, workSpace.Object, channel.Object, esbExecuteRequest);
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
            var internalServiceContainer = new InternalServiceContainer(serviceAction, dsfObj.Object, workSpace.Object, channel.Object, esbExecuteRequest);
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
            var internalServiceContainer = new InternalServiceContainer(serviceAction, dsfObj.Object, workSpace.Object, channel.Object, esbExecuteRequest);
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
            var internalServiceContainer = new InternalServiceContainer(serviceAction, dsfObj.Object, workSpace.Object, channel.Object, esbExecuteRequest, locater.Object);
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
            var allowCanExecute = true;
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
            mockEsbManagementEndpoint.Verify(o => o.Execute(It.IsAny<Dictionary<string,StringBuilder>>(), It.IsAny<IWorkspace>()), Times.Once);

            Assert.AreEqual(0, errorResultTO.FetchErrors().Count);

            Assert.IsNull(esbExecuteRequest.ExecuteResult);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        public void InternalServiceContainer_CanExecute_GivenAllowedService_ShouldNotAllowExecute()
        {
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
            Assert.AreEqual(errors[0], ErrorResource.NotAuthorizedToExecuteOnFollower);

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
            Assert.AreEqual(errors[0], ErrorResource.NotAuthorizedToExecuteOnFollower);

            var result = esbExecuteRequest.ExecuteResult.ToString();
            var serializer = new Dev2JsonSerializer();
            var r = serializer.Deserialize<ExecuteMessage>(result);
            Assert.IsTrue(r.HasError);
            Assert.AreEqual(ErrorResource.NotAuthorizedToExecuteOnFollower, r.Message.ToString());
        }

        private ErrorResultTO InternalServiceContainer_CanExecute_GivenAllowedService(IEsbManagementEndpoint esbManagementEndpoint, EsbExecuteRequest esbExecuteRequest)
        {
            var p = new Mock<IPrincipal>();
            var i = new Mock<IIdentity>();
            i.Setup(o => o.Name).Returns("bob");
            p.Setup(o => o.Identity).Returns(i.Object);
            Common.Utilities.OrginalExecutingUser = p.Object;
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
            var internalServiceContainer = new InternalServiceContainer(serviceAction, dsfObj.Object, workSpace, channel.Object, esbExecuteRequest, locater.Object);
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
