﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestingDotnetDllCascading;

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
                { "source", new StringBuilder(serialezedSource.ToString()) },
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
                { "source", new StringBuilder(serialezedSource.ToString()) },
                { "namespace", new StringBuilder(serialezedNamespace.ToString()) }
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
    }
}
