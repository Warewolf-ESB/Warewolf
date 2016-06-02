using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FindDependenciesTests
    {
       
        ExecuteMessage ConvertToMsg(StringBuilder msg)
        {
            var serialier = new Dev2JsonSerializer();
            var result = serialier.Deserialize<ExecuteMessage>(msg);
            return result;
        }

        #region CTOR

        [TestMethod]
        public void FindDependenciesConstructor()
        {
            var esb = new FindDependencies();
            Assert.IsNotNull(esb);
            Assert.IsInstanceOfType(esb,typeof(IEsbManagementEndpoint));
        }

        #endregion

        #region Execute

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindDependencies_Execute")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void FindDependencies_Execute_NullResourceId_Exception()
        {
            //------------Setup for test--------------------------
            var findDependencies = new FindDependencies();

            //------------Execute Test---------------------------
            findDependencies.Execute(new Dictionary<string, StringBuilder> { { "ResourceId", null } }, new Mock<IWorkspace>().Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindDependencies_Execute")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void FindDependencies_Execute_EmptyResourceId_Exception()
        {
            //------------Setup for test--------------------------
            var findDependencies = new FindDependencies();

            //------------Execute Test---------------------------
            findDependencies.Execute(new Dictionary<string, StringBuilder> { { "ResourceId", new StringBuilder() } }, new Mock<IWorkspace>().Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindDependencies_Execute")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void FindDependencies_Execute_NonGuidResourceId_Exception()
        {
            //------------Setup for test--------------------------
            var findDependencies = new FindDependencies();

            //------------Execute Test---------------------------
            findDependencies.Execute(new Dictionary<string, StringBuilder> { { "ResourceId", new StringBuilder("bob") } }, new Mock<IWorkspace>().Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindDependencies_Execute")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void FindDependencies_Execute_ResourceIdKeyNotPresent_Exception()
        {
            //------------Setup for test--------------------------
            var findDependencies = new FindDependencies();

            //------------Execute Test---------------------------
            findDependencies.Execute(new Dictionary<string, StringBuilder> { { "Resource", new StringBuilder(Guid.NewGuid().ToString()) } }, new Mock<IWorkspace>().Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindDependencies_Execute")]
        public void FindDependencies_Execute_GetDependsOnMe_ShouldReturnDependantsRecursive()
        {
            //------------Setup for test--------------------------
            var findDependencies = new FindDependencies();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var parentResourceId = Guid.NewGuid();
            var parentDepId = Guid.NewGuid();
            var parentDepDepId = Guid.NewGuid();
            mockResourceCatalog.Setup(catalog => catalog.GetDependants(It.IsAny<Guid>(), parentResourceId)).Returns(new List<Guid> { parentDepId });
            mockResourceCatalog.Setup(catalog => catalog.GetDependants(It.IsAny<Guid>(), parentDepId)).Returns(new List<Guid> { parentDepDepId });
            var mockParentResource = new Mock<IResource>();
            mockParentResource.Setup(resource => resource.ResourceID).Returns(parentResourceId);
            mockResourceCatalog.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), parentResourceId)).Returns(mockParentResource.Object);
            var mockParentResourceDep = new Mock<IResource>();
            mockParentResourceDep.Setup(resource => resource.ResourceID).Returns(parentDepId);
            mockResourceCatalog.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), parentDepId)).Returns(mockParentResourceDep.Object);
            var mockParentResourceDepDep = new Mock<IResource>();
            mockParentResourceDepDep.Setup(resource => resource.ResourceID).Returns(parentDepDepId);
            mockResourceCatalog.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), parentDepDepId)).Returns(mockParentResourceDepDep.Object);
            findDependencies.ResourceCatalog = mockResourceCatalog.Object;
            //------------Execute Test---------------------------
            var msg = findDependencies.Execute(new Dictionary<string, StringBuilder> { { "ResourceId", new StringBuilder(parentResourceId.ToString()) }, { "GetDependsOnMe", new StringBuilder("true") } }, new Mock<IWorkspace>().Object);
            //------------Assert Results-------------------------
            var execMsg = ConvertToMsg(msg);
            Assert.IsNotNull(execMsg);
            XElement xe = execMsg.Message.ToXElement();
            Assert.IsNotNull(xe);
            var graphElement = xe.AncestorsAndSelf("graph").FirstOrDefault();
            Assert.IsNotNull(graphElement);
            var nodeElements = graphElement.Elements("node");
            Assert.IsNotNull(nodeElements);
        }


        #endregion

        #region HandlesType

        [TestMethod]
        public void FindDependenciesLogHandlesTypeExpectedReturnsFindDependencyService()
        {
            var esb = new FindDependencies();
            var result = esb.HandlesType();
            Assert.AreEqual("FindDependencyService", result);
        }

        #endregion

        #region CreateServiceEntry

        [TestMethod]
        public void FetchCurrentServerLogCreateServiceEntryExpectedReturnsDynamicService()
        {
            var esb = new FindDependencies();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual(@"<DataList><ResourceId ColumnIODirection=""Input""/><GetDependsOnMe ColumnIODirection=""Input""/><Dev2System.ManagmentServicePayload ColumnIODirection=""Both""></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification.ToString());
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion
    }
}