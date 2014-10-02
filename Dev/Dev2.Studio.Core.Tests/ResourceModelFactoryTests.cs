
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces.Security;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests
{
    [TestClass]
    public class ResourceModelFactoryTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceModelFactory_CreateResourceModel")]
        public void ResourceModelFactory_CreateResourceModel_UserPermissions_Contribute()
        {
            Verify_CreateResourceModel_UserPermissions(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel));
            Verify_CreateResourceModel_UserPermissions(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, ResourceType.WorkflowService, "iconPath", "displayName"));
            Verify_CreateResourceModel_UserPermissions(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "WorkflowService"));
            Verify_CreateResourceModel_UserPermissions(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "WorkflowService", "displayName"));
            Verify_CreateResourceModel_UserPermissions(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "WorkflowService", "resourceName", "displayName"));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceModelFactory_CreateResourceModel")]
        public void ResourceModelFactory_CreateResourceModel_WorkflowService()
        {
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, ResourceType.WorkflowService, "iconPath", "displayName"), ResourceType.WorkflowService, "displayName", null, null);
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "WorkflowService"), ResourceType.WorkflowService, "WorkflowService", "", "WorkflowService");
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "WorkflowService", "displayName"), ResourceType.WorkflowService, "displayName", "", "WorkflowService");
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "WorkflowService", "resourceName", "displayName"), ResourceType.WorkflowService, "displayName", "resourceName", "WorkflowService");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceModelFactory_CreateResourceModel")]
        public void ResourceModelFactory_CreateResourceModel_DatabaseService()
        {
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, ResourceType.Service, "iconPath", "displayName"), ResourceType.Service, "displayName", null, null);
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "DatabaseService"), ResourceType.Service, "Service", "", "DbService");
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "DatabaseService", "displayName"), ResourceType.Service, "Service", "", "DbService");
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "DatabaseService", "resourceName", "displayName"), ResourceType.Service, "Service", "resourceName", "DbService");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceModelFactory_CreateResourceModel")]
        public void ResourceModelFactory_CreateResourceModel_ResourceService()
        {
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, ResourceType.Service, "iconPath", "displayName"), ResourceType.Service, "displayName", null, null);
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "ResourceService"), ResourceType.Service, "PluginService", "", "PluginService");
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "ResourceService", "displayName"), ResourceType.Service, "PluginService", "", "PluginService");
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "ResourceService", "resourceName", "displayName"), ResourceType.Service, "PluginService", "resourceName", "PluginService");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceModelFactory_CreateResourceModel")]
        public void ResourceModelFactory_CreateResourceModel_WebService()
        {
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, ResourceType.Service, "iconPath", "displayName"), ResourceType.Service, "displayName", null, null);
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "WebService"), ResourceType.Service, "WebService", "", "WebService");
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "WebService", "displayName"), ResourceType.Service, "displayName", "", "WebService");
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "WebService", "resourceName", "displayName"), ResourceType.Service, "displayName", "resourceName", "WebService");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceModelFactory_CreateResourceModel")]
        public void ResourceModelFactory_CreateResourceModel_Server()
        {
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, ResourceType.Server, "iconPath", "displayName"), ResourceType.Server, "displayName", null, null);
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "Server"), ResourceType.Server, "Server", "", "ServerSource");
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "Server", "displayName"), ResourceType.Server, "displayName", "", "ServerSource");
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "Server", "resourceName", "displayName"), ResourceType.Server, "displayName", "resourceName", "ServerSource");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceModelFactory_CreateResourceModel")]
        public void ResourceModelFactory_CreateResourceModel_DbSource()
        {
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, ResourceType.Source, "iconPath", "displayName"), ResourceType.Source, "displayName", null, null);
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "DbSource"), ResourceType.Source, "DbSource", "", "DbSource");
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "DbSource", "displayName"), ResourceType.Source, "displayName", "", "DbSource");
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "DbSource", "resourceName", "displayName"), ResourceType.Source, "displayName", "resourceName", "DbSource");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceModelFactory_CreateResourceModel")]
        public void ResourceModelFactory_CreateResourceModel_ResourceSource()
        {
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, ResourceType.Source, "iconPath", "displayName"), ResourceType.Source, "displayName", null, null);
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "ResourceSource"), ResourceType.Source, "Plugin", "", "PluginSource");
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "ResourceSource", "displayName"), ResourceType.Source, "Plugin", "", "PluginSource");
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "ResourceSource", "resourceName", "displayName"), ResourceType.Source, "Plugin", "resourceName", "PluginSource");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceModelFactory_CreateResourceModel")]
        public void ResourceModelFactory_CreateResourceModel_WebSource()
        {
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, ResourceType.Source, "iconPath", "displayName"), ResourceType.Source, "displayName", null, null);
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "WebSource"), ResourceType.Source, "WebSource", "", "WebSource");
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "WebSource", "displayName"), ResourceType.Source, "displayName", "", "WebSource");
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "WebSource", "resourceName", "displayName"), ResourceType.Source, "displayName", "resourceName", "WebSource");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceModelFactory_CreateResourceModel")]
        public void ResourceModelFactory_CreateResourceModel_EmailSource()
        {
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, ResourceType.Source, "iconPath", "displayName"), ResourceType.Source, "displayName", null, null);
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "EmailSource"), ResourceType.Source, "EmailSource", "", "EmailSource");
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "EmailSource", "displayName"), ResourceType.Source, "displayName", "", "EmailSource");
            Verify_CreateResourceModel_ResourceType(environmentModel => ResourceModelFactory.CreateResourceModel(environmentModel, "EmailSource", "resourceName", "displayName"), ResourceType.Source, "displayName", "resourceName", "EmailSource");
        }

        static void Verify_CreateResourceModel_UserPermissions(Func<IEnvironmentModel, IContextualResourceModel> createResourceModel)
        {
            //------------Setup for test--------------------------
            var environmentModel = new Mock<IEnvironmentModel>();

            //------------Execute Test---------------------------
            var resourceModel = createResourceModel(environmentModel.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(Permissions.Contribute, resourceModel.UserPermissions);
        }

        static void Verify_CreateResourceModel_ResourceType(Func<IEnvironmentModel, IContextualResourceModel> createResourceModel, ResourceType resourceType, string displayName, string resourceName, string serverResourceName)
        {
            //------------Setup for test--------------------------
            var environmentModel = new Mock<IEnvironmentModel>();

            //------------Execute Test---------------------------
            var resourceModel = createResourceModel(environmentModel.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(resourceType, resourceModel.ResourceType);
            Assert.AreEqual(displayName, resourceModel.DisplayName);
            Assert.AreEqual(resourceName, resourceModel.ResourceName);
            Assert.AreEqual(serverResourceName, resourceModel.ServerResourceType);
        }
    }
}
