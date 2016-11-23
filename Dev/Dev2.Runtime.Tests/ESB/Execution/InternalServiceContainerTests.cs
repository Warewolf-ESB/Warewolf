using System.Linq;
using System.Text;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.ESB.Execution
{
    [TestClass]
    public class InternalServiceContainerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnConstruction_GivenValidArgs_ShouldBuildCorrectly()
        {
            //---------------Set up test pack-------------------
            //InternalServiceContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request)

            const string datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            var serviceAction = new ServiceAction() { DataListSpecification = new StringBuilder(datalist) };
            var dsfObj = new Mock<IDSFDataObject>();
            dsfObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), false))
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
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), false))
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
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), false))
                  .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("Args")));
            var workSpace = new Mock<IWorkspace>();
            var channel = new Mock<IEsbChannel>();
            var esbExecuteRequest = new EsbExecuteRequest();
            var internalServiceContainer = new InternalServiceContainer(serviceAction, dsfObj.Object, workSpace.Object, channel.Object, esbExecuteRequest);
            //---------------Assert Precondition----------------
            Assert.AreEqual(4, esbExecuteRequest.Args.Count);
            //---------------Execute Test ----------------------
            ErrorResultTO errorResultTO;
            internalServiceContainer.Execute(out errorResultTO, 1);
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
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), false))
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
            ErrorResultTO errorResultTO;
            var execute = internalServiceContainer.Execute(out errorResultTO, 1);
            //---------------Test Result -----------------------
            locater.VerifyAll();
            
        }
    }
}
