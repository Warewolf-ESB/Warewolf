using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestingDotnetDllCascading;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchPluginActionsWithReturnsTypesTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var fetchPluginActionsWithReturnsTypes = new FetchPluginActionsWithReturnsTypes();

            //------------Execute Test---------------------------
            var resId = fetchPluginActionsWithReturnsTypes.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetAuthorizationContextForService_ShouldReturnAuthorizationContext_Any()
        {
            //------------Setup for test--------------------------
            var fetchPluginActionsWithReturnsTypes = new FetchPluginActionsWithReturnsTypes();
            //------------Execute Test---------------------------
            var authorizationContext = fetchPluginActionsWithReturnsTypes.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Any, authorizationContext);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Execute_GivenNoNamespace_ShouldReturnEmpyt()
        {
            //------------Setup for test--------------------------
            var pluginSource = new PluginSource
            {
                ResourceName = "ResourceName",
                FilePath = "ResourcePath",
                ResourceID = Guid.Empty
            };

            var resourceCat = new Mock<IResourceCatalog>();
            resourceCat.Setup(catalog => catalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(pluginSource);
            var fetchPluginActionsWithReturnsTypes = new FetchPluginActionsWithReturnsTypes(resourceCat.Object);            
            
            var jsonSerializer = new Dev2JsonSerializer();
            var sourceDefinition = new PluginSourceDefinition
            {
                Id = Guid.Empty, Name = "SourceName", Path = "SourcePath"
            };
            var serialezedSource = jsonSerializer.SerializeToBuilder(sourceDefinition);
            var values = new Dictionary<string, StringBuilder>
            {
                { "source", serialezedSource },
                { "namespace", new StringBuilder("") }
            };
            var workspace = new Mock<IWorkspace>();
            //-------------------------Test Execution -----------------
            var execute = fetchPluginActionsWithReturnsTypes.Execute(values, workspace.Object);
            var results = jsonSerializer.Deserialize<ExecuteMessage>(execute);
            //------------Assert Results-------------------------
            Assert.IsFalse(results.HasError);
            Assert.AreEqual("[]", results.Message.ToString());
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Execute_GivenSourceAndNamespace_ShouldReturnStringBuilder()
        {
            var personObject = typeof(Person);
            //------------Setup for test--------------------------
            var pluginSource = new PluginSource
            {
                ResourceName = personObject.FullName,
                ResourceID = personObject.GUID,
                AssemblyName = personObject.AssemblyQualifiedName,
                AssemblyLocation = personObject.Assembly.Location
            };

            var resourceCat = new Mock<IResourceCatalog>();
            resourceCat.Setup(catalog => catalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(pluginSource);
            var fetchPluginActionsWithReturnsTypes = new FetchPluginActionsWithReturnsTypes(resourceCat.Object);            
            
            var jsonSerializer = new Dev2JsonSerializer();
            var sourceDefinition = new PluginSourceDefinition
            {
                Id = personObject.GUID, Name = personObject.FullName, Path = personObject.Assembly.Location
            };
            var namespaceItem = new NamespaceItem
            {
                FullName = personObject.FullName,
                AssemblyLocation = personObject.Assembly.Location,
                AssemblyName = personObject.Assembly.FullName
            };

            var serialezedSource = jsonSerializer.SerializeToBuilder(sourceDefinition);
            var serialezedNamespace = jsonSerializer.SerializeToBuilder(namespaceItem);
            var values = new Dictionary<string, StringBuilder>
            {
                { "source", serialezedSource },
                { "namespace", serialezedNamespace }
            };
            var workspace = new Mock<IWorkspace>();
            var execute = fetchPluginActionsWithReturnsTypes.Execute(values, workspace.Object);
            var results = jsonSerializer.Deserialize<ExecuteMessage>(execute);
            //------------Assert Results-------------------------
            Assert.IsFalse(results.HasError);
            var pluginActions = jsonSerializer.Deserialize<List<IPluginAction>>(results.Message);
            Assert.IsNotNull(pluginActions);
            Assert.IsTrue(pluginActions.Any(action => action.FullName == personObject.FullName));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenSourceAndNamespaceWithVoidActions_ShouldReturnAVoidFunction()
        {
            var humanType   = typeof(Human);
            //------------Setup for test--------------------------
            var pluginSource = new PluginSource
            {
                ResourceName = humanType.FullName,
                ResourceID = humanType.GUID,
                AssemblyName = humanType.AssemblyQualifiedName,
                AssemblyLocation = humanType.Assembly.Location
            };

            var resourceCat = new Mock<IResourceCatalog>();
            resourceCat.Setup(catalog => catalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(pluginSource);
            var fetchPluginActionsWithReturnsTypes = new FetchPluginActionsWithReturnsTypes(resourceCat.Object);            
            
            var jsonSerializer = new Dev2JsonSerializer();
            var sourceDefinition = new PluginSourceDefinition
            {
                Id = humanType.GUID, Name = humanType.FullName, Path = humanType.Assembly.Location
            };
            var namespaceItem = new NamespaceItem
            {
                FullName = humanType.FullName,
                AssemblyLocation = humanType.Assembly.Location,
                AssemblyName = humanType.Assembly.FullName
            };

            var serialezedSource = jsonSerializer.SerializeToBuilder(sourceDefinition);
            var serialezedNamespace = jsonSerializer.SerializeToBuilder(namespaceItem);
            var values = new Dictionary<string, StringBuilder>
            {
                { "source", serialezedSource },
                { "namespace", serialezedNamespace }
            };
            var workspace = new Mock<IWorkspace>();
            var execute = fetchPluginActionsWithReturnsTypes.Execute(values, workspace.Object);
            var results = jsonSerializer.Deserialize<ExecuteMessage>(execute);
            //------------Assert Results-------------------------
            Assert.IsFalse(results.HasError);
            var pluginActions = jsonSerializer.Deserialize<List<IPluginAction>>(results.Message);
            Assert.IsTrue(pluginActions.Any(action => action.IsVoid));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenSourceAndNamespaceWithStringActions_ShouldReturnAStringFunction()
        {
            var humanType   = typeof(Human);
            //------------Setup for test--------------------------
            var pluginSource = new PluginSource
            {
                ResourceName = humanType.FullName,
                ResourceID = humanType.GUID,
                AssemblyName = humanType.AssemblyQualifiedName,
                AssemblyLocation = humanType.Assembly.Location
            };

            var resourceCat = new Mock<IResourceCatalog>();
            resourceCat.Setup(catalog => catalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(pluginSource);
            var fetchPluginActionsWithReturnsTypes = new FetchPluginActionsWithReturnsTypes(resourceCat.Object);            
            
            var jsonSerializer = new Dev2JsonSerializer();
            var sourceDefinition = new PluginSourceDefinition
            {
                Id = humanType.GUID, Name = humanType.FullName, Path = humanType.Assembly.Location
            };
            var namespaceItem = new NamespaceItem
            {
                FullName = humanType.FullName,
                AssemblyLocation = humanType.Assembly.Location,
                AssemblyName = humanType.Assembly.FullName
            };

            var serialezedSource = jsonSerializer.SerializeToBuilder(sourceDefinition);
            var serialezedNamespace = jsonSerializer.SerializeToBuilder(namespaceItem);
            var values = new Dictionary<string, StringBuilder>
            {
                { "source", serialezedSource },
                { "namespace", serialezedNamespace }
            };
            var workspace = new Mock<IWorkspace>();
            var execute = fetchPluginActionsWithReturnsTypes.Execute(values, workspace.Object);
            var results = jsonSerializer.Deserialize<ExecuteMessage>(execute);
            //------------Assert Results-------------------------
            Assert.IsFalse(results.HasError);
            var pluginActions = jsonSerializer.Deserialize<List<IPluginAction>>(results.Message);
            Assert.IsTrue(pluginActions.Any(action => action.IsVoid));
            var any = pluginActions.Any(action => action.Dev2ReturnType.Equals("return: String"));
            Assert.IsTrue(any);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenSourceAndNamespaceWithActionsTakingObject_ShouldObjectParameters()
        {
            var humanType   = typeof(Human);
            //------------Setup for test--------------------------
            var pluginSource = new PluginSource
            {
                ResourceName = humanType.FullName,
                ResourceID = humanType.GUID,
                AssemblyName = humanType.AssemblyQualifiedName,
                AssemblyLocation = humanType.Assembly.Location
            };

            var resourceCat = new Mock<IResourceCatalog>();
            resourceCat.Setup(catalog => catalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(pluginSource);
            var fetchPluginActionsWithReturnsTypes = new FetchPluginActionsWithReturnsTypes(resourceCat.Object);            
            
            var jsonSerializer = new Dev2JsonSerializer();
            var sourceDefinition = new PluginSourceDefinition
            {
                Id = humanType.GUID, Name = humanType.FullName, Path = humanType.Assembly.Location
            };
            var namespaceItem = new NamespaceItem
            {
                FullName = humanType.FullName,
                AssemblyLocation = humanType.Assembly.Location,
                AssemblyName = humanType.Assembly.FullName
            };

            var serialezedSource = jsonSerializer.SerializeToBuilder(sourceDefinition);
            var serialezedNamespace = jsonSerializer.SerializeToBuilder(namespaceItem);
            var values = new Dictionary<string, StringBuilder>
            {
                { "source", serialezedSource },
                { "namespace", serialezedNamespace }
            };
            var workspace = new Mock<IWorkspace>();
            var execute = fetchPluginActionsWithReturnsTypes.Execute(values, workspace.Object);
            var results = jsonSerializer.Deserialize<ExecuteMessage>(execute);
            //------------Assert Results-------------------------
            Assert.IsFalse(results.HasError);
            var pluginActions = jsonSerializer.Deserialize<List<IPluginAction>>(results.Message);
            Assert.IsTrue(pluginActions.Any(action => action.IsVoid));
            IEnumerable<IList<IServiceInput>> parameters= pluginActions.Select(action => action.Inputs);
            var enumerable = parameters as IList<IServiceInput>[] ?? parameters.ToArray();
            var containsObjectParameters = enumerable.Any(list => list.Any(input => input.IsObject));
            var shortTypeName = enumerable.All(list => list.All(input => !string.IsNullOrEmpty(input.ShortTypeName)));
            var dev2ReturnType = enumerable.All(list => list.All(input => !string.IsNullOrEmpty(input.Dev2ReturnType)));
            Assert.IsTrue(containsObjectParameters);
            Assert.IsTrue(shortTypeName);
            Assert.IsTrue(dev2ReturnType);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenSourceAndNamespace_ShouldPropertyMethods()
        {
            var humanType   = typeof(Human);
            //------------Setup for test--------------------------
            var pluginSource = new PluginSource
            {
                ResourceName = humanType.FullName,
                ResourceID = humanType.GUID,
                AssemblyName = humanType.AssemblyQualifiedName,
                AssemblyLocation = humanType.Assembly.Location
            };

            var resourceCat = new Mock<IResourceCatalog>();
            resourceCat.Setup(catalog => catalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(pluginSource);
            var fetchPluginActionsWithReturnsTypes = new FetchPluginActionsWithReturnsTypes(resourceCat.Object);            
            
            var jsonSerializer = new Dev2JsonSerializer();
            var sourceDefinition = new PluginSourceDefinition
            {
                Id = humanType.GUID, Name = humanType.FullName, Path = humanType.Assembly.Location
            };
            var namespaceItem = new NamespaceItem
            {
                FullName = humanType.FullName,
                AssemblyLocation = humanType.Assembly.Location,
                AssemblyName = humanType.Assembly.FullName
            };

            var serialezedSource = jsonSerializer.SerializeToBuilder(sourceDefinition);
            var serialezedNamespace = jsonSerializer.SerializeToBuilder(namespaceItem);
            var values = new Dictionary<string, StringBuilder>
            {
                { "source", serialezedSource },
                { "namespace", serialezedNamespace }
            };
            var workspace = new Mock<IWorkspace>();
            var execute = fetchPluginActionsWithReturnsTypes.Execute(values, workspace.Object);
            var results = jsonSerializer.Deserialize<ExecuteMessage>(execute);
            //------------Assert Results-------------------------
            Assert.IsFalse(results.HasError);
            var pluginActions = jsonSerializer.Deserialize<List<IPluginAction>>(results.Message);
            var any = pluginActions.Any(action => action.IsProperty);
            Assert.IsTrue(any);
        }
    }
}
