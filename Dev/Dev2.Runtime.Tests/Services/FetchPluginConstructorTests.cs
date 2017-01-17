using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.ExtMethods;
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

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchPluginConstructorTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var comPluginActions = new FetchPluginConstructors();

            //------------Execute Test---------------------------
            var resId = comPluginActions.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var FetchPluginConstructors = new FetchPluginConstructors();

            //------------Execute Test---------------------------
            var resId = FetchPluginConstructors.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Any, resId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void HandlesType_GivenPluginConstructorService_ShouldRuturnCorrectly()
        {
            //---------------Set up test pack-------------------
            var FetchPluginConstructors = new FetchPluginConstructors();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var handlesType = FetchPluginConstructors.HandlesType();
            //---------------Test Result -----------------------
            Assert.AreEqual("FetchPluginConstructors", handlesType);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenTestInputs_ShouldReturnConstructorList()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var assembly = type.Assembly;
            NamespaceItem namespaceItem = new NamespaceItem()
            {
                AssemblyLocation = assembly.Location,
                AssemblyName = assembly.FullName,
                FullName = type.FullName,

            };
            var pluginSource = new PluginSource()
            {
                AssemblyName = type.AssemblyQualifiedName,
                AssemblyLocation = assembly.Location,

            };

            var pluginSourceDefinition = new PluginSourceDefinition();
            var serializeToJson = pluginSourceDefinition.SerializeToJsonStringBuilder();
            var nameSpace = namespaceItem.SerializeToJsonStringBuilder();
            var resourceCat = new Mock<IResourceCatalog>();
            resourceCat.Setup(catalog => catalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(pluginSource);
            var FetchPluginConstructors = new FetchPluginConstructors(resourceCat.Object);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var mock = new Mock<IWorkspace>();
            var stringBuilder = FetchPluginConstructors.Execute(new Dictionary<string, StringBuilder>()
            {
                { "source", serializeToJson },
                { "namespace", nameSpace },

            },
                mock.Object);
            //---------------Test Result -----------------------
            var executeMessage = stringBuilder.DeserializeToObject<ExecuteMessage>();
            var deserializeToObject = executeMessage.Message.DeserializeToObject<List<IPluginConstructor>>();
            Assert.AreEqual(3, deserializeToObject.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenHasContructorWithInputs_ShouldReturnCorrectInputs()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var assembly = type.Assembly;
            NamespaceItem namespaceItem = new NamespaceItem()
            {
                AssemblyLocation = assembly.Location,
                AssemblyName = assembly.FullName,
                FullName = type.FullName,

            };
            var pluginSource = new PluginSource()
            {
                AssemblyName = type.AssemblyQualifiedName,
                AssemblyLocation = assembly.Location,

            };

            var pluginSourceDefinition = new PluginSourceDefinition();
            var serializeToJson = pluginSourceDefinition.SerializeToJsonStringBuilder();
            var nameSpace = namespaceItem.SerializeToJsonStringBuilder();
            var resourceCat = new Mock<IResourceCatalog>();
            resourceCat.Setup(catalog => catalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(pluginSource);
            var FetchPluginConstructors = new FetchPluginConstructors(resourceCat.Object);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var mock = new Mock<IWorkspace>();
            var stringBuilder = FetchPluginConstructors.Execute(new Dictionary<string, StringBuilder>()
            {
                { "source", serializeToJson },
                { "namespace", nameSpace },

            },
                mock.Object);
            //---------------Test Result -----------------------
            var executeMessage = stringBuilder.DeserializeToObject<ExecuteMessage>();
            var deserializeToObject = executeMessage.Message.DeserializeToObject<List<IPluginConstructor>>();
            Assert.AreEqual(3, deserializeToObject.Count);

            var pluginConstructor = deserializeToObject[1];
            var count = pluginConstructor.Inputs.Count;
            Assert.AreEqual(1, count);
            var serviceInput = pluginConstructor.Inputs[0];
            var name = serviceInput.Name;
            var emptyIsNull = serviceInput.EmptyToNull;
            var requiredField = serviceInput.IsRequired;
            var value = serviceInput.Value;
            var typeName = serviceInput.TypeName;
            Assert.AreEqual("name", name);
            Assert.AreEqual(false, emptyIsNull);
            Assert.AreEqual(true, requiredField);
            Assert.IsTrue(string.IsNullOrEmpty(value));
            Assert.AreEqual(typeof(string), Type.GetType(typeName));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenHasContructorWithInputs_ShouldReturnCorrectConstructorName()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var assembly = type.Assembly;
            NamespaceItem namespaceItem = new NamespaceItem()
            {
                AssemblyLocation = assembly.Location,
                AssemblyName = assembly.FullName,
                FullName = type.FullName,

            };
            var pluginSource = new PluginSource()
            {
                AssemblyName = type.AssemblyQualifiedName,
                AssemblyLocation = assembly.Location,

            };

            var pluginSourceDefinition = new PluginSourceDefinition();
            var serializeToJson = pluginSourceDefinition.SerializeToJsonStringBuilder();
            var nameSpace = namespaceItem.SerializeToJsonStringBuilder();
            var resourceCat = new Mock<IResourceCatalog>();
            resourceCat.Setup(catalog => catalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(pluginSource);
            var FetchPluginConstructors = new FetchPluginConstructors(resourceCat.Object);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var mock = new Mock<IWorkspace>();
            var stringBuilder = FetchPluginConstructors.Execute(new Dictionary<string, StringBuilder>()
            {
                { "source", serializeToJson },
                { "namespace", nameSpace },

            },
                mock.Object);
            //---------------Test Result -----------------------
            var executeMessage = stringBuilder.DeserializeToObject<ExecuteMessage>();
            var deserializeToObject = executeMessage.Message.DeserializeToObject<List<IPluginConstructor>>();
            Assert.AreEqual(3, deserializeToObject.Count);

            var pluginConstructor1 = deserializeToObject[1];
            var constructorName1 = pluginConstructor1.ConstructorName;
            const string expectedName1 = ".ctor (System.String)";
            Assert.AreEqual(expectedName1, constructorName1);

            var pluginConstructor0 = deserializeToObject[0];
            var constructorName0 = pluginConstructor0.ConstructorName;
            const string expectedName0 = ".ctor ";

            Assert.AreEqual(expectedName0, constructorName0);
            var pluginConstructor2 = deserializeToObject[2];
            var constructorName2 = pluginConstructor2.ConstructorName;
            const string expectedName2 = ".ctor (System.String,System.String,TestingDotnetDllCascading.Food)";
            Assert.AreEqual(expectedName2, constructorName2);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenHasError_ShouldReturnErrorState()
        {
            //---------------Set up test pack-------------------
            var type = typeof(Human);
            var assembly = type.Assembly;
            NamespaceItem namespaceItem = new NamespaceItem()
            {
                AssemblyLocation = assembly.Location,
                AssemblyName = assembly.FullName,
                FullName = type.FullName,

            };
            

            var pluginSourceDefinition = new PluginSourceDefinition();
            var serializeToJson = pluginSourceDefinition.SerializeToJsonStringBuilder();
            var nameSpace = namespaceItem.SerializeToJsonStringBuilder();
            var resourceCat = new Mock<IResourceCatalog>();
            resourceCat.Setup(catalog => catalog.GetResource<PluginSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Throws(new Exception("error"));
            var FetchPluginConstructors = new FetchPluginConstructors(resourceCat.Object);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var mock = new Mock<IWorkspace>();
            var stringBuilder = FetchPluginConstructors.Execute(new Dictionary<string, StringBuilder>()
            {
                { "source", serializeToJson },
                { "namespace", nameSpace },

            },
                mock.Object);
            //---------------Test Result -----------------------
            var executeMessage = stringBuilder.DeserializeToObject<ExecuteMessage>();
            Assert.IsTrue(executeMessage.HasError);
            Assert.AreEqual("error", executeMessage.Message.ToString());
        }

    }
}