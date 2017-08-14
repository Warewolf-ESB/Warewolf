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
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;




namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
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
            var nodes = nodeElements as IList<XElement> ?? nodeElements.ToList();
            Assert.AreEqual(3, nodes.Count);

            var node1 = nodes[0];
            Assert.IsNotNull(node1);
            Assert.AreEqual(parentResourceId.ToString(), node1.Attribute("id").Value);
            var node1Dep = node1.Elements().FirstOrDefault();
            Assert.IsNotNull(node1Dep);
            Assert.AreEqual(parentDepId.ToString(), node1Dep.Attribute("id").Value);

            var node2 = nodes[1];
            Assert.IsNotNull(node2);
            Assert.AreEqual(parentDepId.ToString(), node2.Attribute("id").Value);
            var node2Dep = node2.Elements().FirstOrDefault();
            Assert.IsNotNull(node2Dep);
            Assert.AreEqual(parentDepDepId.ToString(), node2Dep.Attribute("id").Value);

            var node3 = nodes[2];
            Assert.IsNotNull(node3);
            Assert.AreEqual(parentDepDepId.ToString(), node3.Attribute("id").Value);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindDependencies_Execute")]
        
        public void FindDependencies_Execute_GetDependsOnMeFalse_ShouldReturnDependenciesRecursive()
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
            mockParentResource.Setup(resource => resource.Dependencies).Returns(new List<IResourceForTree>
            {
                new ResourceForTree {ResourceID = parentDepId},
                new ResourceForTree {ResourceID = parentDepDepId}
            });
            mockResourceCatalog.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), parentResourceId)).Returns(mockParentResource.Object);
            var mockParentResourceDep = new Mock<IResource>();
            mockParentResourceDep.Setup(resource => resource.ResourceID).Returns(parentDepId);
            mockResourceCatalog.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), parentDepId)).Returns(mockParentResourceDep.Object);
            var mockParentResourceDepDep = new Mock<IResource>();
            mockParentResourceDepDep.Setup(resource => resource.ResourceID).Returns(parentDepDepId);
            mockResourceCatalog.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), parentDepDepId)).Returns(mockParentResourceDepDep.Object);
            findDependencies.ResourceCatalog = mockResourceCatalog.Object;
            //------------Execute Test---------------------------
            var msg = findDependencies.Execute(new Dictionary<string, StringBuilder> { { "ResourceId", new StringBuilder(parentResourceId.ToString()) }, { "GetDependsOnMe", new StringBuilder("false") } }, new Mock<IWorkspace>().Object);
            //------------Assert Results-------------------------
            var execMsg = ConvertToMsg(msg);
            Assert.IsNotNull(execMsg);
            XElement xe = execMsg.Message.ToXElement();
            Assert.IsNotNull(xe);
            var graphElement = xe.AncestorsAndSelf("graph").FirstOrDefault();
            Assert.IsNotNull(graphElement);
            var nodeElements = graphElement.Elements("node");
            Assert.IsNotNull(nodeElements);
            var nodes = nodeElements as IList<XElement> ?? nodeElements.ToList();
            Assert.AreEqual(1, nodes.Count);

            var node1 = nodes[0];
            Assert.IsNotNull(node1);
            Assert.AreEqual(parentResourceId.ToString(), node1.Attribute("id").Value);

            var nodeDep = node1.Elements().ToList();
            var node1Dep = nodeDep[0];
            Assert.IsNotNull(node1Dep);
            Assert.AreEqual(parentDepId.ToString(), node1Dep.Attribute("id").Value);

            var node2Dep = nodeDep[1];
            Assert.IsNotNull(node2Dep);
            Assert.AreEqual(parentDepDepId.ToString(), node2Dep.Attribute("id").Value);
        }



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindDependencies_Execute")]
        
        public void FindDependencies_Execute_GetDependsOnMe_WhenCircular_ShouldReturnDependantsRecursive()
        {
            //------------Setup for test--------------------------
            var findDependencies = new FindDependencies();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var parentResourceId = Guid.NewGuid();
            var parentDepId = Guid.NewGuid();
            var parentDepDepId = Guid.NewGuid();
            mockResourceCatalog.Setup(catalog => catalog.GetDependants(It.IsAny<Guid>(), parentResourceId)).Returns(new List<Guid> { parentDepId });
            mockResourceCatalog.Setup(catalog => catalog.GetDependants(It.IsAny<Guid>(), parentDepId)).Returns(new List<Guid> { parentDepDepId });
            mockResourceCatalog.Setup(catalog => catalog.GetDependants(It.IsAny<Guid>(), parentDepDepId)).Returns(new List<Guid> { parentResourceId });
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
            var nodes = nodeElements as IList<XElement> ?? nodeElements.ToList();
            Assert.AreEqual(3, nodes.Count);

            var node1 = nodes[0];
            Assert.IsNotNull(node1);
            Assert.AreEqual(parentResourceId.ToString(), node1.Attribute("id").Value);
            var node1Dep = node1.Elements().FirstOrDefault();
            Assert.IsNotNull(node1Dep);
            Assert.AreEqual(parentDepId.ToString(), node1Dep.Attribute("id").Value);

            var node2 = nodes[1];
            Assert.IsNotNull(node2);
            Assert.AreEqual(parentDepId.ToString(), node2.Attribute("id").Value);
            var node2Dep = node2.Elements().FirstOrDefault();
            Assert.IsNotNull(node2Dep);
            Assert.AreEqual(parentDepDepId.ToString(), node2Dep.Attribute("id").Value);

            var node3 = nodes[2];
            Assert.IsNotNull(node3);
            Assert.AreEqual(parentDepDepId.ToString(), node3.Attribute("id").Value);
            var node3Dep = node3.Elements().FirstOrDefault();
            Assert.IsNotNull(node3Dep);
            Assert.AreEqual(parentResourceId.ToString(), node3Dep.Attribute("id").Value);
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

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var findDependencies = new FindDependencies();

            //------------Execute Test---------------------------
            var resId = findDependencies.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var findDependencies = new FindDependencies();

            //------------Execute Test---------------------------
            var resId = findDependencies.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Any, resId);
        }
    }
}