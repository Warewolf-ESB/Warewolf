/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Dev2.Activities;
using Dev2.Collections;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.Data.ServiceModel.Messages;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.DynamicServices.Objects.Base;
using Dev2.Explorer;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ResourceCatalogImpl;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.XML;
using Dev2.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Unlimited.Framework.Converters.Graph.Ouput;
using Warewolf.ResourceManagement;

// ReSharper disable PossibleMultipleEnumeration

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    public class ResourceCatalogTests
    {
        // Change this if you change the number of resources saved by SaveResources()
        const int SaveResourceCount = 6;
        static readonly object syncRoot = new object();

        static string _testDir;

        [ClassInitialize]
        public static void MyClassInit(TestContext context)
        {
            _testDir = context.DeploymentDirectory;
        }

        [TestInitialize]
        public void Initialise()
        {
            var workspacePath = EnvironmentVariables.ResourcePath;
            if (Directory.Exists(workspacePath))
            {
                Directory.Delete(workspacePath, true);
            }
            if (!Directory.Exists(EnvironmentVariables.ResourcePath))
            {
                Directory.CreateDirectory(EnvironmentVariables.ResourcePath);
            }
            CustomContainer.Register<IActivityParser>(new ActivityParser());
        }

        #region Instance

        [TestMethod]
        public void InstanceExpectedIsSingleton()
        {
            var instance1 = ResourceCatalog.Instance;
            var instance2 = ResourceCatalog.Instance;
            Assert.IsNotNull(instance1);
            Assert.IsNotNull(instance2);
            Assert.AreSame(instance1, instance2);
        }

        #endregion

        #region This


        #region LoadWorkspaceAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoadWorkspaceAsyncWithNullWorkspaceArgumentExpectedThrowsArgumentNullException()
        {
            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);

            rc.LoadWorkspaceViaBuilder(null, false);
        }

        [TestMethod]
        public void LoadWorkspaceAsyncWithEmptyFoldersArgumentExpectedReturnsEmptyCatalog()
        {
            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);

            Assert.AreEqual(0, rc.LoadWorkspaceViaBuilder("xx", false).Count);
        }


        [TestMethod]
        public void LoadWorkspaceAsyncWithExistingSourcesPathAndNonExistingServicesPathExpectedReturnsCatalogForSources()
        {
            var workspaceID = Guid.NewGuid();
            EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            Directory.CreateDirectory(sourcesPath);
            var resources = SaveResources(sourcesPath, null, false, false, new[] { "CatalogSourceAnytingToXmlPlugin", "CatalogSourceCitiesDatabase", "CatalogSourceTestServer" }, new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);

            Assert.AreEqual(3, result.Count);

            foreach (var resource in result)
            {
                IResource currentResource = resource;
                var expected = resources.First(r => r.ResourceName == currentResource.ResourceName);
                Assert.AreEqual(expected.FilePath, resource.FilePath);
            }
        }


        [TestMethod]
        public void LoadWorkspaceAsyncWithValidWorkspaceIDExpectedReturnsCatalogForWorkspace()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);

            Assert.AreEqual(SaveResourceCount, result.Count);

            foreach (var resource in result)
            {
                IResource currentResource = resource;
                var expected = resources.First(r => r.ResourceName == currentResource.ResourceName);
                Assert.AreEqual(expected.FilePath, resource.FilePath);
            }
        }


        [TestMethod]
        public void GetResourceCount_ExpectedReturnsCount()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);

            Assert.AreEqual(SaveResourceCount, rc.GetResourceCount(workspaceID));            
        }

        [TestMethod]
        public void Reload_ExpectedReturnsCount()
        {
            List<IResource> resources;
            var workspaceID = GlobalConstants.ServerWorkspaceID;
            SaveResources(workspaceID, out resources);

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.Reload();

            Assert.AreEqual(SaveResourceCount, rc.GetResourceCount(workspaceID));            
        }


        [TestMethod]
        public void LoadWorkspaceAsyncWithWithOneSignedAndOneUnsignedServiceExpectedLoadsSignedService()
        {
            var workspaceID = GlobalConstants.ServerWorkspaceID;


            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            var resources = SaveResources(path, null, false, false, new[] { "Calculate_RecordSet_Subtract", "TestDecisionUnsigned" }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);

            Assert.AreEqual(2, result.Count);

            foreach (var resource in result)
            {
                IResource currentResource = resource;
                var expected = resources.First(r => r.ResourceName == currentResource.ResourceName);
                Assert.AreEqual(expected.FilePath, resource.FilePath);
            }
        }


        [TestMethod]
        public void LoadWorkspaceAsyncWithSourceWithoutIDExpectedInjectsID()
        {
            var workspaceID = GlobalConstants.ServerWorkspaceID;


            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            var resources = SaveResources(path, null, false, false, new[] { "CitiesDatabase" }, new[] { Guid.NewGuid() }).ToList();

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);

            Assert.AreEqual(1, result.Count);

            foreach (var resource in result)
            {
                IResource currentResource = resource;
                var expected = resources.First(r => r.ResourceName == currentResource.ResourceName);
                Assert.AreNotEqual(expected.ResourceID, resource.ResourceID);
            }
        }

        #endregion

        #region ParallelExecution

        #endregion

        #region SaveResource

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveResourceWithNullResourceArgumentExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.SaveResource(workspaceID, (IResource)null, "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveResourceWithNullResourceXmlArgumentExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.SaveResource(workspaceID, (Resource)null, "");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceCatalog_SaveResource")]
        public void SaveResource_WithNoResourcePath_ExpectedSavedAtRootLevel()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = workspacePath;
            const string resourceName = "CitiesDatabase";
            var xml = XmlResource.Fetch(resourceName);
            var resource = new DbSource(xml);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            //------------Execute Test---------------------------
            catalog.SaveResource(workspaceID, resource, "");
            //------------Assert Results-------------------------
            xml = XElement.Load(Path.Combine(path, resource.ResourceName + ".xml"));
            Assert.IsNotNull(xml);
            var idAttr = xml.Attributes("ID").ToList();
            Assert.AreEqual(1, idAttr.Count);
            var nameAttribute = xml.Attribute("Name");
            Assert.IsNotNull(nameAttribute);
            Assert.AreEqual(resourceName, nameAttribute.Value);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceCatalog_SaveResource")]
        public void SaveResource_Expects_A_VersionToBeSaved()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            const string resourceName = "CitiesDatabase";
            var xml = XmlResource.Fetch(resourceName);
            var resource = new DbSource(xml);
            var version = new Mock<IServerVersionRepository>();
            var catalog = new ResourceCatalog(null, version.Object);
            //------------Execute Test---------------------------
            catalog.SaveResource(workspaceID, resource, "", reason: "reason", user: "bob");
            //------------Assert Results-------------------------
            version.Verify(a => a.StoreVersion(It.IsAny<IResource>(), "bob", "reason", workspaceID, ""));
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceCatalog_SaveResource")]
        public void SaveResource_Expects_A_VersionToBeSaved_Xml()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var version = new Mock<IServerVersionRepository>();
            var catalog = new ResourceCatalog(null, version.Object);

            var expected = new DbSource { ResourceID = Guid.NewGuid(), ResourceName = "TestSource", DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase };

            //------------Execute Test---------------------------
            catalog.SaveResource(workspaceID, new StringBuilder(expected.ToXml().ToString()), "", reason: "reason", user: "bob");


            //------------Assert Results-------------------------
            version.Verify(a => a.StoreVersion(It.IsAny<Resource>(), "bob", "reason", workspaceID, ""));


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceCatalog_SaveResource")]
        public void SaveResource_Expects_A_RollbackOnError()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var version = new Mock<IServerVersionRepository>();
            var catalog = new ResourceCatalog(null, version.Object);

            var resourceID = Guid.NewGuid();
            var expected = new DbSource { ResourceID = resourceID, ResourceName = "TestSource", DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase };

            //------------Execute Test---------------------------
            catalog.SaveResource(workspaceID, expected.ToStringBuilder(), "", reason: "reason", user: "bob");
            expected.ResourceName = "federatedresource";

            try
            {
                expected.ResourceName = "";
                catalog.SaveResource(workspaceID, expected.ToStringBuilder(), "", reason: "reason", user: "bob");
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
            // ReSharper restore EmptyGeneralCatchClause
            { }
            var res = catalog.GetResourceContents(workspaceID, expected.ResourceID).ToString();
            Assert.IsFalse(res.Contains("federatedresource"));

        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceCatalog_SaveResource")]
        public void SaveResource_WithNoResourcePath_ServerWorkspace_ExpectedResourceSavedEventFired()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.Empty;
            const string resourceName = "CitiesDatabase";
            var xml = XmlResource.Fetch(resourceName);
            var resource = new DbSource(xml);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var _called = false;
            IResource _resourceInEvent = null;
            catalog.ResourceSaved = resource1 =>
            {
                _called = true;
                _resourceInEvent = resource1;
            };
            //------------Execute Test---------------------------
            catalog.SaveResource(workspaceID, resource, "");
            //------------Assert Results-------------------------
            Assert.IsTrue(_called);
            Assert.IsNotNull(_resourceInEvent);
            Assert.AreEqual(resource.ResourceID, _resourceInEvent.ResourceID);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceCatalog_SaveResource")]
        public void SaveResource_WithNullResourcePath_ExpectedSavedAtRootLevel()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = workspacePath;
            const string resourceName = "CitiesDatabase";
            var xml = XmlResource.Fetch(resourceName);
            var resource = new DbSource(xml);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            //------------Execute Test---------------------------
            catalog.SaveResource(workspaceID, resource, "");
            //------------Assert Results-------------------------
            xml = XElement.Load(Path.Combine(path, resource.ResourceName + ".xml"));
            Assert.IsNotNull(xml);
            var idAttr = xml.Attributes("ID").ToList();
            Assert.AreEqual(1, idAttr.Count);
            var nameAttribute = xml.Attribute("Name");
            Assert.IsNotNull(nameAttribute);
            Assert.AreEqual(resourceName, nameAttribute.Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceCatalog_SaveResource")]
        public void SaveResource_WithResourcePath_ExpectedSavedInCorrectFolder()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            const string resourcePath = "MyTest\\Folder1";
            var path = Path.Combine(workspacePath, resourcePath);
            const string resourceName = "CitiesDatabase";
            var xml = XmlResource.Fetch(resourceName);
            var resource = new DbSource(xml);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            //------------Execute Test---------------------------
            catalog.SaveResource(workspaceID, resource, resourcePath);
            //------------Assert Results-------------------------
            xml = XElement.Load(path + "\\" + resourceName + ".xml");
            Assert.IsNotNull(xml);
            var idAttr = xml.Attributes("ID").ToList();
            Assert.AreEqual(1, idAttr.Count);
            var nameAttribute = xml.Attribute("Name");
            Assert.IsNotNull(nameAttribute);
            Assert.AreEqual(resourceName, nameAttribute.Value);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceCatalog_SaveResource")]
        public void SaveResource_WithDifferentResourcePath_ExpectedDeleteOfExisting()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            const string resourcePath = "MyTest\\Folder1\\CitiesDatabase";
            const string resourceName = "CitiesDatabase";
            var xml = XmlResource.Fetch(resourceName);
            var resource = new DbSource(xml);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            //------------Execute Test---------------------------
            var resource2 = new DbSource(xml) { ResourceID = resource.ResourceID };
            catalog.SaveResource(workspaceID, resource, resourcePath, "", "");
            var pathToDelete = resource.FilePath;
            resource2.FilePath = resource.FilePath.Replace("Folder1", "Foldler1");
            resource2.ResourceName = "CitiesDatabase2";
            Assert.IsTrue(File.Exists(pathToDelete));
            catalog.SaveResource(workspaceID, resource2, "MyTest\\Folder1\\CitiesDatabase2", "", "");
            //------------Assert Results-------------------------
            Assert.IsFalse(File.Exists(pathToDelete));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceCatalog_SaveResource")]
        public void SaveResource_WithSameResourcePath_ExpectedNotDeleteOfExisting()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            const string resourcePath = "MyTest\\Folder1\\CitiesDatabase";
            const string resourceName = "CitiesDatabase";
            var xml = XmlResource.Fetch(resourceName);
            var resource = new DbSource(xml);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            //------------Execute Test---------------------------
            var resource2 = new DbSource(xml) { ResourceID = resource.ResourceID };
            catalog.SaveResource(workspaceID, resource, resourcePath, "", "");
            var pathToDelete = resource.FilePath;
            resource2.FilePath = resource.FilePath.Replace("Folder1", "Foldler1");
            resource2.ResourceName = "CitiesDatabase";
            Assert.IsTrue(File.Exists(pathToDelete));
            catalog.SaveResource(workspaceID, resource2, "MyTest\\FOLDER1\\CitiesDatabase", "", "");
            //------------Assert Results-------------------------
            Assert.IsTrue(File.Exists(pathToDelete));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceCatalog_SaveResource")]
        public void SaveResource_WithSameResourceNameDifferentResourcePath_ExpectedSavedInCorrectFolder()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            const string resourcePath = "MyTest\\Folder1\\";
            const string resourcePath1 = "MyTest\\Folder2\\";
            var path = Path.Combine(workspacePath, resourcePath);
            const string resourceName = "CitiesDatabase";
            var xml = XmlResource.Fetch(resourceName);
            var resource = new DbSource(xml);
            var resource1 = new DbSource(xml);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.SaveResource(workspaceID, resource, resourcePath, "", "");
            //------------Execute Test---------------------------
            catalog.SaveResource(workspaceID, resource1, resourcePath1, "", "");
            //------------Assert Results-------------------------
            xml = XElement.Load(path + "\\" + resourceName + ".xml");
            Assert.IsNotNull(xml);
            var idAttr = xml.Attributes("ID").ToList();
            Assert.AreEqual(1, idAttr.Count);
            var nameAttribute = xml.Attribute("Name");
            Assert.IsNotNull(nameAttribute);
            Assert.AreEqual(resourceName, nameAttribute.Value);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceCatalog_SaveResource")]
        public void SaveResource_WithSameResourceNameSameResourcePath_ExpectedSavedInCorrectFolder()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            const string resourcePath = "MyTest\\Folder1";
            var path = Path.Combine(workspacePath, resourcePath);
            const string resourceName = "CitiesDatabase";
            var xml = XmlResource.Fetch(resourceName);
            var resource = new DbSource(xml);
            var resource1 = new DbSource(xml);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.SaveResource(workspaceID, resource, resourcePath, "", "");
            //------------Execute Test---------------------------
            var resourceCatalogResult = catalog.SaveResource(workspaceID, resource1, resourcePath, "", "");
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.DuplicateMatch, resourceCatalogResult.Status);
            xml = XElement.Load(path + "\\CitiesDatabase" + ".xml");
            Assert.IsNotNull(xml);
            var idAttr = xml.Attributes("ID").ToList();
            Assert.AreEqual(1, idAttr.Count);
            var nameAttribute = xml.Attribute("Name");
            Assert.IsNotNull(nameAttribute);
            Assert.AreEqual(resourceName, nameAttribute.Value);
        }


        [TestMethod]
        public void SaveResourceWithSourceWithoutIDExpectedSourceSavedWithID()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var path = workspacePath;

            var xml = XmlResource.Fetch("CitiesDatabase");
            var resource = new DbSource(xml);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.SaveResource(workspaceID, resource, "");

            xml = XElement.Load(Path.Combine(path, "CitiesDatabase" + ".xml"));
            var attr = xml.Attributes("ID").ToList();

            Assert.AreEqual(1, attr.Count);
        }

        [TestMethod]
        public void SaveResourceWithExistingResourceExpectedResourceOverwritten()
        {
            var workspaceID = Guid.NewGuid();

            var resource1 = new DbSource { ResourceID = Guid.NewGuid(), ResourceName = "TestSource", DatabaseName = "TestOldDb", Server = "TestOldServer", ServerType = enSourceType.SqlDatabase };

            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.SaveResource(workspaceID, resource1, "");

            var expected = new DbSource { ResourceID = resource1.ResourceID, ResourceName = resource1.ResourceName, DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase };
            catalog.SaveResource(workspaceID, expected, "");

            var contents = catalog.GetResourceContents(workspaceID, expected.ResourceID);
            var actual = new DbSource(XElement.Parse(contents.ToString()));

            Assert.AreEqual(expected.ResourceName, actual.ResourceName);
            Assert.AreEqual(expected.DatabaseName, actual.DatabaseName);
            Assert.AreEqual(expected.Server, actual.Server);
            Assert.AreEqual(expected.ServerType, actual.ServerType);
        }

        [TestMethod]
        public void SaveResourceWithExistingResourceAndReadonlyExpectedResourceOverwritten()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var resource1 = new DbSource { ResourceID = Guid.NewGuid(), ResourceName = "TestSource", DatabaseName = "TestOldDb", Server = "TestOldServer", ServerType = enSourceType.SqlDatabase };

            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.SaveResource(workspaceID, resource1, "");

            var path = Path.Combine(workspacePath, "TestSource" + ".xml");
            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.ReadOnly) != FileAttributes.ReadOnly)
            {
                File.SetAttributes(path, attributes ^ FileAttributes.ReadOnly);
            }

            var expected = new DbSource { ResourceID = resource1.ResourceID, ResourceName = resource1.ResourceName, DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase };
            catalog.SaveResource(workspaceID, expected, "");

            var contents = catalog.GetResourceContents(workspaceID, expected.ResourceID);
            var actual = new DbSource(XElement.Parse(contents.ToString()));

            Assert.AreEqual(expected.ResourceName, actual.ResourceName);
            Assert.AreEqual(expected.DatabaseName, actual.DatabaseName);
            Assert.AreEqual(expected.Server, actual.Server);
            Assert.AreEqual(expected.ServerType, actual.ServerType);
        }

        [TestMethod]
        public void SaveResourceWithNewResourceExpectedResourceWritten()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);

            var expected = new DbSource { ResourceID = Guid.NewGuid(), ResourceName = "TestSource", DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase };
            catalog.SaveResource(workspaceID, expected, "");

            var contents = catalog.GetResourceContents(workspaceID, expected.ResourceID);
            var actual = new DbSource(XElement.Parse(contents.ToString()));

            Assert.AreEqual(expected.ResourceName, actual.ResourceName);
            Assert.AreEqual(expected.DatabaseName, actual.DatabaseName);
            Assert.AreEqual(expected.Server, actual.Server);
            Assert.AreEqual(expected.ServerType, actual.ServerType);
        }

        [TestMethod]
        [ExpectedException(typeof(DirectoryNotFoundException))]
        public void SaveResourceWithSlashesInResourceNameExpectedThrowsDirectoryNotFoundException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);

            var expected = new DbSource { ResourceID = Guid.NewGuid(), ResourceName = "Test\\Source", DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase };
            catalog.SaveResource(workspaceID, expected, "");
        }

        [TestMethod]
        public void SaveResourceWithNewResourceXmlExpectedResourceWritten()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);

            var expected = new DbSource { ResourceID = Guid.NewGuid(), ResourceName = "TestSource", DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase };
            catalog.SaveResource(workspaceID, new StringBuilder(expected.ToXml().ToString()), "");

            var contents = catalog.GetResourceContents(workspaceID, expected.ResourceID);
            var actual = new DbSource(XElement.Parse(contents.ToString()));

            Assert.AreEqual(expected.ResourceName, actual.ResourceName);
            Assert.AreEqual(expected.DatabaseName, actual.DatabaseName);
            Assert.AreEqual(expected.Server, actual.Server);
            Assert.AreEqual(expected.ServerType, actual.ServerType);
        }

        #endregion

        #region GetResource

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetResourceWithNullResourceNameExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.GetResource(workspaceID, null);
        }

        [TestMethod]
        public void GetResourceWithResourceNameExpectedReturnsResource()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            foreach (var expected in resources)
            {
                var actual = catalog.GetResource(workspaceID, expected.ResourceName);
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.ResourceID, actual.ResourceID);
                Assert.AreEqual(expected.ResourceName, actual.ResourceName);
            }
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceCatalog_SaveResource")]
        public void GetResource_WithResourceContainsResourcePath_ExpectedCorrectResource()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            const string resourcePath = "MyTest\\Folder1";
            const string resourcePath1 = "MyTest\\Folder2";
            const string resourceName = "CitiesDatabase";
            var xml = XmlResource.Fetch(resourceName);
            var resource = new DbSource(xml);
            var resource1 = new DbSource(xml);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.SaveResource(workspaceID, resource, resourcePath, "", "");
            catalog.SaveResource(workspaceID, resource1, resourcePath1, "", "");
            //------------Execute Test---------------------------
            var retrievedResource = catalog.GetResource(workspaceID, "MyTest\\Folder2\\CitiesDatabase");
            //------------Assert Results-------------------------
            Assert.IsNotNull(retrievedResource);
            Assert.AreEqual(resourcePath1, "MyTest\\Folder2");

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetResourcepath_WithResourceContainsResourcePath_ExpectedCorrectResourcePath()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            const string resourcePath = "MyTest\\Folder1";
            const string resourceName = "CitiesDatabase";
            var xml = XmlResource.Fetch(resourceName);
            var resource = new DbSource(xml);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var resourceCatalogResult = catalog.SaveResource(workspaceID, resource, resourcePath, "", "");
            //------------Execute Test---------------------------
            Assert.IsTrue(resourceCatalogResult.Status == ExecStatus.Success);
            var retrievedResource = catalog.GetResourcePath(workspaceID, resource.ResourceID);
            //------------Assert Results-------------------------
            Assert.IsNotNull(retrievedResource);
            Assert.AreEqual(resourcePath + "\\" + resourceName, retrievedResource);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetResourceList_WithGivenWorkspaceWith1Resource_ShouldReturn1Resource()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            const string resourcePath = "MyTest\\Folder1";
            const string resourceName = "CitiesDatabase";
            var xml = XmlResource.Fetch(resourceName);
            var resource = new DbSource(xml);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var resourceCatalogResult = catalog.SaveResource(workspaceID, resource, resourcePath, "", "");
            //------------Execute Test---------------------------
            Assert.IsTrue(resourceCatalogResult.Status == ExecStatus.Success);
            var retrievedResource = catalog.GetResourceList(workspaceID);
            //------------Assert Results-------------------------
            Assert.IsNotNull(retrievedResource);
            Assert.AreEqual(1, retrievedResource.Count);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetResourceListGeneric_GivenWorkspaceWith1Resource_ShouldReturn1Resource()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            const string resourcePath = "MyTest\\Folder1";
            const string resourceName = "CitiesDatabase";
            var xml = XmlResource.Fetch(resourceName);
            var resource = new DbSource(xml);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var resourceCatalogResult = catalog.SaveResource(workspaceID, resource, resourcePath, "", "");
            //------------Execute Test---------------------------
            Assert.IsTrue(resourceCatalogResult.Status == ExecStatus.Success);
            IList<IResource> retrievedResource = catalog.GetResourceList<DbSource>(workspaceID);
            //------------Assert Results-------------------------
            Assert.IsNotNull(retrievedResource);
            Assert.AreEqual(1, retrievedResource.Count);


        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetResourceListGeneric_GivenWorkspaceWith1ResourceAndWrongTypr_ShouldReturnNothing()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            const string resourcePath = "MyTest\\Folder1";
            const string resourceName = "CitiesDatabase";
            var xml = XmlResource.Fetch(resourceName);
            var resource = new DbSource(xml);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var resourceCatalogResult = catalog.SaveResource(workspaceID, resource, resourcePath, "", "");
            //------------Execute Test---------------------------
            Assert.IsTrue(resourceCatalogResult.Status == ExecStatus.Success);
            IList<IResource> retrievedResource = catalog.GetResourceList<DropBoxSource>(workspaceID);
            //------------Assert Results-------------------------
            Assert.IsNotNull(retrievedResource);
            Assert.AreEqual(0, retrievedResource.Count);


        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void LoadResourceActivityCache_GivenServerId_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {
                var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
                catalog.LoadResourceActivityCache(GlobalConstants.ServerWorkspaceID);

            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        public void GetResource_UnitTest_WhereTypeIsProvided_ExpectTypedResourceWorkflow()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();

            var path = EnvironmentVariables.GetWorkspacePath(workspaceID) + "\\Bugs";
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            //------------Execute Test---------------------------
            var workflow = ResourceCatalog.Instance.GetResource<Workflow>(workspaceID, "Bugs\\" + resourceName);
            //------------Assert Results-------------------------
            Assert.IsNotNull(workflow);
            Assert.IsInstanceOfType(workflow, typeof(Workflow));
        }

        [TestMethod]
        public void GetResource_UnitTest_WhereTypeIsProvided_ExpectTypedResourceWebSource()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            const string resourceName = "WebService";
            SaveResources(path, null, false, true, new[] { "WebService", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var xml = XmlResource.Fetch("WeatherWebSource");
            var resource = new WebSource(xml);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.SaveResource(workspaceID, resource, "");
            //------------Assert Precondition-----------------
            //------------Execute Test---------------------------
            var webSource = catalog.GetResource<WebSource>(workspaceID, resource.ResourceID);
            //------------Assert Results-------------------------
            Assert.IsNotNull(webSource);
            Assert.IsInstanceOfType(webSource, typeof(WebSource));
        }

        #endregion

        #region GetResourceContents

        [TestMethod]
        public void GetResourceContentsWithNullResourceExpectedReturnsEmptyString()
        {
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var result = catalog.GetResourceContents(null);
            Assert.AreEqual(string.Empty, result.ToString());
        }

        [TestMethod]
        public void GetResourceContentsWithNullResourceFilePathExpectedReturnsEmptyString()
        {
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var result = catalog.GetResourceContents(new Resource());
            Assert.AreEqual(string.Empty, result.ToString());
        }

        [TestMethod]
        public void GetResourceContentsWithExistingResourceExpectedReturnsResourceContents()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            foreach (var expected in resources)
            {
                var actual = catalog.GetResourceContents(expected);
                Assert.IsNotNull(actual);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceCatalog_GetResourceContents")]
        public void ResourceCatalog_GetResourceContents_WhenHasNewLine_ShouldReturnWithNewLine()
        {
            //------------Setup for test--------------------------
            var resourceCatalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var webService = new WebService
            {
                Source = new WebSource
                {
                    ResourceID = Guid.NewGuid(),
                    ResourceName = "TestWebSource",
                },
                ResourceID = Guid.NewGuid(),
                ResourceName = "DummyWebServiceWithNewLine",
                RequestUrl = "pqr",
                RequestMethod = WebRequestMethod.Get,
                RequestHeaders = "Content-Type: text/xml\nBearer: Trusted",
                RequestBody = "abc\nhas an enter\nin it",
                RequestResponse = "xyz",
                JsonPath = "$.somepath",
                OutputDescription = new OutputDescription()
            };
            resourceCatalog.SaveResource(Guid.Empty, webService, "");
            //------------Execute Test---------------------------
            var resourceContents = resourceCatalog.GetResourceContents(webService);
            //------------Assert Results-------------------------
            StringAssert.Contains(resourceContents.ToString(), "\n");
        }

        [TestMethod]
        public void GetResourceContentsWithNonExistentResourceExpectedReturnsEmptyString()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var actual = catalog.GetResourceContents(new Resource { ResourceID = Guid.NewGuid(), FilePath = Path.GetRandomFileName() });
            Assert.AreEqual(string.Empty, actual.ToString());
        }

        [TestMethod]
        public void GetResourceContentsWithNonExistentResourceIDExpectedReturnsEmptyString()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var actual = catalog.GetResourceContents(workspaceID, Guid.NewGuid());
            Assert.AreEqual(string.Empty, actual.ToString());
        }

        #endregion

        #region SyncTo

        [TestMethod]
        public void SyncToWithDeleteIsFalseAndFileDeletedFromSourceExpectedFileNotDeletedInDestination()
        {
            List<IResource> sourceResources;
            var sourceWorkspaceID = Guid.NewGuid();
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, out sourceResources);

            List<IResource> targetResources;
            var targetWorkspaceID = Guid.NewGuid();
            var targetWorkspacePath = SaveResources(targetWorkspaceID, out targetResources);

            var sourceResource = sourceResources[0];
            var targetResource = targetResources.First(r => r.ResourceID == sourceResource.ResourceID);

            var targetFile = new FileInfo(targetResource.FilePath);


            new ResourceCatalog().SyncTo(sourceWorkspacePath, targetWorkspacePath, true, false);
            targetFile.Refresh();
            Assert.IsTrue(targetFile.Exists);
        }

        [TestMethod]
        public void SyncToWithOverwriteIsTrueExpectedFileInDestinationOverwritten()
        {
            List<IResource> sourceResources;
            var sourceWorkspaceID = Guid.NewGuid();
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, out sourceResources);

            List<IResource> targetResources;
            var targetWorkspaceID = Guid.NewGuid();
            var targetWorkspacePath = SaveResources(targetWorkspaceID, out targetResources);

            var sourceResource = sourceResources[0];
            var targetResource = targetResources.First(r => r.ResourceID == sourceResource.ResourceID);
            var sourceFile = new FileInfo(sourceResource.FilePath);
            var targetFile = new FileInfo(targetResource.FilePath);

            var fs = sourceFile.Open(FileMode.Append, FileAccess.Write, FileShare.None);
            fs.Write(new byte[]
            {
                200
            }, 0, 0);
            fs.Close();

            new ResourceCatalog().SyncTo(sourceWorkspacePath, targetWorkspacePath, true, false);

            sourceFile.Refresh();
            targetFile.Refresh();

            var expected = sourceFile.Length;
            var actual = targetFile.Length;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SyncToWithOverwriteIsFalseExpectedFileInDestinationUnchanged()
        {
            List<IResource> sourceResources;
            var sourceWorkspaceID = Guid.NewGuid();
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, out sourceResources, true);

            List<IResource> targetResources;
            var targetWorkspaceID = Guid.NewGuid();
            var targetWorkspacePath = SaveResources(targetWorkspaceID, out targetResources, true);

            var sourceResource = sourceResources[0];
            var targetResource = targetResources.First(r => r.ResourceID == sourceResource.ResourceID);
            var sourceFile = new FileInfo(sourceResource.FilePath);
            var targetFile = new FileInfo(targetResource.FilePath);

            var fs = sourceFile.Open(FileMode.Append, FileAccess.Write, FileShare.None);
            fs.Write(new byte[]
            {
                200
            }, 0, 1);
            fs.Close();

            targetFile.Refresh();
            var expected = targetFile.Length;

            new ResourceCatalog().SyncTo(sourceWorkspacePath, targetWorkspacePath, false, false);

            targetFile.Refresh();

            var actual = targetFile.Length;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SyncToWithFilesToIgnoreSpecifiedExpectedIgnoredFilesAreNotCopied()
        {
            List<IResource> sourceResources;
            var sourceWorkspaceID = Guid.NewGuid();
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, out sourceResources);

            List<IResource> targetResources;
            var targetWorkspaceID = Guid.NewGuid();
            var targetWorkspacePath = SaveResources(targetWorkspaceID, out targetResources);

            var sourceResource = sourceResources[0];
            var targetResource = targetResources.First(r => r.ResourceID == sourceResource.ResourceID);
            var targetFile = new FileInfo(targetResource.FilePath);

            targetFile.Delete();
            new ResourceCatalog().SyncTo(sourceWorkspacePath, targetWorkspacePath, false, false, new List<string> { targetFile.Name });
            targetFile.Refresh();
            Assert.IsFalse(targetFile.Exists);
        }

        [TestMethod]
        public void SyncToWithFilesToIgnoreSpecifiedExpectedIgnoredFilesAreNotDeleted()
        {
            List<IResource> sourceResources;
            var sourceWorkspaceID = Guid.NewGuid();
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, out sourceResources);

            List<IResource> targetResources;
            var targetWorkspaceID = Guid.NewGuid();
            var targetWorkspacePath = SaveResources(targetWorkspaceID, out targetResources);

            var sourceResource = sourceResources[0];
            var targetResource = targetResources.First(r => r.ResourceID == sourceResource.ResourceID);

            var targetFile = new FileInfo(targetResource.FilePath);


            new ResourceCatalog().SyncTo(sourceWorkspacePath, targetWorkspacePath, false, false, new List<string> { targetFile.Name });
            targetFile.Refresh();
            Assert.IsTrue(targetFile.Exists);
        }

        [TestMethod]
        public void SyncToWithNonExistingDestinationDirectoryExpectedDestinationDirectoryCreated()
        {
            List<IResource> sourceResources;
            var sourceWorkspaceID = Guid.NewGuid();
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, out sourceResources);

            var targetWorkspaceID = Guid.NewGuid();
            var targetWorkspacePath = EnvironmentVariables.GetWorkspacePath(targetWorkspaceID);

            var targetDir = new DirectoryInfo(targetWorkspacePath);

            new ResourceCatalog().SyncTo(sourceWorkspacePath, targetWorkspacePath, false, false);
            targetDir.Refresh();
            Assert.IsTrue(targetDir.Exists);

        }

        #endregion

        #region ToPayload

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ResourceCatalog_ToPayload")]
        public void ResourceCatalog_ToPayload_GetServiceNormalPayload_ConnectionStringAsAttributeOfRootTag()
        {
            //------------Setup for test--------------------------

            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            List<IResource> saveResources = SaveResources(sourcesPath, null, false, false, new[] { "ServerConnection1", "ServerConnection2" }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            //------------Execute Test---------------------------

            var payload = catalog.ToPayload(saveResources[0]);

            //------------Assert Results-------------------------

            Assert.IsTrue(payload.ToString().StartsWith("<Source"));
            XElement payloadElement = XElement.Parse(payload.ToString());
            string connectionStringattribute = payloadElement.AttributeSafe("ConnectionString");
            Assert.IsFalse(string.IsNullOrEmpty(connectionStringattribute));
        }

        #endregion

        #region CopyResource

        [TestMethod]
        public void CopyResourceWithNullResourceExpectedDoesNotCopyResourceToTarget()
        {
            var targetWorkspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var result = catalog.CopyResource(null, targetWorkspaceID);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CopyResourceWithNonExistingResourceNameExpectedDoesNotCopyResourceToTarget()
        {
            List<IResource> sourceResources;
            var sourceWorkspaceID = Guid.NewGuid();
            SaveResources(sourceWorkspaceID, out sourceResources);

            List<IResource> targetResources;
            var targetWorkspaceID = Guid.NewGuid();
            SaveResources(targetWorkspaceID, out targetResources);

            var result = new ResourceCatalog().CopyResource(Guid.Empty, sourceWorkspaceID, targetWorkspaceID);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CopyResourceWithNonExistingResourceFilePathExpectedDoesNotCopyResourceToTarget()
        {
            List<IResource> sourceResources;
            var sourceWorkspaceID = Guid.NewGuid();
            SaveResources(sourceWorkspaceID, out sourceResources);

            List<IResource> targetResources;
            var targetWorkspaceID = Guid.NewGuid();
            SaveResources(targetWorkspaceID, out targetResources);

            var sourceResource = sourceResources[0];
            var targetResource = targetResources.First(r => r.ResourceID == sourceResource.ResourceID);
            var sourceFile = new FileInfo(sourceResource.FilePath);
            var targetFile = new FileInfo(targetResource.FilePath);

            sourceFile.Delete();
            targetFile.Delete();

            var result = new ResourceCatalog().CopyResource(sourceResource.ResourceID, sourceWorkspaceID, targetWorkspaceID);
            Assert.IsFalse(result);
            targetFile.Refresh();
            Assert.IsFalse(targetFile.Exists);
        }

        #endregion

        #region ResourceCatalogResultBuilder

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ResourceCatalogResultBuilder_GivenMessage_ShouldReturnCorrectResults()
        {
            var accessViolationResult = ResourceCatalogResultBuilder.CreateAccessViolationResult("a");
            Assert.AreEqual(ExecStatus.AccessViolation, accessViolationResult.Status);
            Assert.AreEqual("a", accessViolationResult.Message);
            var duplicate = ResourceCatalogResultBuilder.CreateDuplicateMatchResult("b");
            Assert.AreEqual(ExecStatus.DuplicateMatch, duplicate.Status);
            Assert.AreEqual("b", duplicate.Message);
            var fail = ResourceCatalogResultBuilder.CreateFailResult("c");
            Assert.AreEqual(ExecStatus.Fail, fail.Status);
            Assert.AreEqual("c", fail.Message);
            var noMatch = ResourceCatalogResultBuilder.CreateNoMatchResult("d");
            Assert.AreEqual(ExecStatus.NoMatch, noMatch.Status);
            Assert.AreEqual("d", noMatch.Message);
            var wild = ResourceCatalogResultBuilder.CreateNoWildcardsAllowedhResult("e");
            Assert.AreEqual(ExecStatus.NoWildcardsAllowed, wild.Status);
            Assert.AreEqual("e", wild.Message);
            var succes = ResourceCatalogResultBuilder.CreateSuccessResult("f");
            Assert.AreEqual(ExecStatus.Success, succes.Status);
            Assert.AreEqual("f", succes.Message);


        }

        #endregion

        #region DeleteResource

        [TestMethod]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void DeleteResourceWithNullResourceNameExpectedThrowsInvalidDataContractException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.DeleteResource(workspaceID, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void DeleteResourceWithNullTypeExpectedThrowsInvalidDataContractException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.DeleteResource(workspaceID, "xxx", null);
        }

        [TestMethod]
        public void DeleteResourceWithWildcardResourceNameExpectedReturnsNoWildcardsAllowed()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var result = catalog.DeleteResource(workspaceID, "*", "WorkflowService");
            Assert.AreEqual(ExecStatus.NoWildcardsAllowed, result.Status);
        }

        [TestMethod]
        public void DeleteResourceWithNonExistingResourceNameExpectedReturnsNoMatch()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var result = catalog.DeleteResource(workspaceID, "xxx", "WorkflowService");
            Assert.AreEqual(ExecStatus.NoMatch, result.Status);
        }

        [TestMethod]
        public void DeleteResourceWithManyExistingResourceNamesExpectedReturnsDuplicateMatch()
        {
            const string ResourceName = "Test";

            var workspaceID = Guid.NewGuid();

            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.SaveResource(workspaceID, new Resource { ResourceID = Guid.NewGuid(), ResourceName = ResourceName, ResourceType = "WorkflowService" }, "");
            catalog.SaveResource(workspaceID, new Resource { ResourceID = Guid.NewGuid(), ResourceName = ResourceName, ResourceType = "WorkflowService" }, "");

            var result = catalog.DeleteResource(workspaceID, ResourceName, "WorkflowService");
            Assert.AreEqual(ExecStatus.DuplicateMatch, result.Status);
        }

        

        #endregion

        #region GetDynamicObjects

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDynamicObjectsWithNullResourceNameExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.GetDynamicObjects<DynamicServiceObjectBase>(workspaceID, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDynamicObjectsWithNullResourceNameAndContainsTrueExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.GetDynamicObjects<DynamicServiceObjectBase>(workspaceID, null, true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDynamicObjectsWithNullResourceExpectedThrowsArgumentNullException()
        {
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.GetDynamicObjects((IResource)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDynamicObjectsWithNullResourcesExpectedThrowsArgumentNullException()
        {
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.GetDynamicObjects((IEnumerable<IResource>)null);
        }

        [TestMethod]
        public void GetDynamicObjectsWithResourceNameExpectedReturnsObjectGraph()
        {
            var workspaceID = Guid.NewGuid();
            List<IResource> resources;
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);

            var expected = resources.First(r => r.ResourceType == "WorkflowService");

            var graph = catalog.GetDynamicObjects<DynamicService>(workspaceID, expected.ResourceName, true);

            VerifyObjectGraph(new List<IResource> { expected }, graph);
        }

        [TestMethod]
        public void GetDynamicObjectsWithResourceExpectedReturnsObjectGraph()
        {
            var workspaceID = Guid.NewGuid();
            List<IResource> resources;
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);

            var expected = resources.First(r => r.ResourceType == "WorkflowService");

            var graph = catalog.GetDynamicObjects(expected);

            VerifyObjectGraph(new List<IResource> { expected }, graph);
        }

        [TestMethod]
        public void GetDynamicObjectsWithResourcesExpectedReturnsObjectGraphs()
        {
            var workspaceID = Guid.NewGuid();
            List<IResource> resources;
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);

            var expecteds = resources.Where(r => r.ResourceType == "WorkflowService").ToList();

            var graph = catalog.GetDynamicObjects(expecteds);

            VerifyObjectGraph(expecteds, graph);
        }

        #endregion

        #endregion

        #region GetModels

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumDev2Server_ExpectConnectionObjects()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "ServerConnection1", "ServerConnection2" }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, false, "Sources", "Services");

            Assert.AreEqual(2, result.Count);

            //------------Execute Test---------------------------

            var models = rc.GetModels(workspaceID, enSourceType.Dev2Server);
            //------------Assert Results-------------------------

            foreach (var model in models)
            {
                Assert.AreEqual(typeof(Connection), model.GetType());
            }

            var payload = JsonConvert.SerializeObject(models);

            Assert.IsNotNull(payload);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumEmailSource_ExpectEmailSourceObjects()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();


            var sourcesPath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "EmailSource" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);

            Assert.AreEqual(1, result.Count);

            //------------Execute Test---------------------------
            var models = rc.GetModels(workspaceID, enSourceType.EmailSource);

            //------------Assert Results-------------------------
            foreach (var model in models)
            {
                Assert.AreEqual(typeof(EmailSource), model.GetType());
            }

            var payload = JsonConvert.SerializeObject(models);

            Assert.IsNotNull(payload);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumDropBoxSource_ExpectDropBoxSourceSourceObjects()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();


            var sourcesPath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "OauthSource" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);

            Assert.AreEqual(1, result.Count);

            //------------Execute Test---------------------------
            var models = rc.GetModels(workspaceID, enSourceType.OauthSource);

            //------------Assert Results-------------------------
            foreach (var model in models)
            {
                Assert.AreEqual(typeof(DropBoxSource), model.GetType());
            }

            var payload = JsonConvert.SerializeObject(models);

            Assert.IsNotNull(payload);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumSqlDatabase_ExpectDbSourceObjects()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;


            var sourcesPath = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "DbSource" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            Assert.AreEqual(1, result.Count);

            //------------Execute Test---------------------------
            var models = rc.GetModels(workspaceID, enSourceType.SqlDatabase);

            //------------Assert Results-------------------------
            foreach (var model in models)
            {
                Assert.AreEqual(typeof(DbSource), model.GetType());
            }

            var payload = JsonConvert.SerializeObject(models);
            Assert.IsNotNull(payload);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumPlugin_ExpectPluginSourceObjects()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;


            var sourcesPath = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "PluginSource" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            Assert.AreEqual(1, result.Count);

            //------------Execute Test---------------------------
            var models = rc.GetModels(workspaceID, enSourceType.PluginSource);

            //------------Assert Results-------------------------
            foreach (var model in models)
            {
                Assert.AreEqual(typeof(PluginSource), model.GetType());
            }

            var payload = JsonConvert.SerializeObject(models);

            Assert.IsNotNull(payload);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumWebSource_ExpectWebSourceObjects()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;


            var sourcesPath = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "WebSourceWithoutInputs" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            Assert.AreEqual(1, result.Count);

            //------------Execute Test---------------------------
            var models = rc.GetModels(workspaceID, enSourceType.WebSource);

            //------------Assert Results-------------------------
            foreach (var model in models)
            {
                Assert.AreEqual(typeof(WebSource), model.GetType());
            }

            var payload = JsonConvert.SerializeObject(models);

            Assert.IsNotNull(payload);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumWebService_ExpectNull()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "EmailSource" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspaceViaBuilder(workspacePath, false, "Sources", "Services");

            var models = rc.GetModels(workspaceID, enSourceType.WebService);

            Assert.IsNull(models);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumDynamicService_ExpectNull()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "EmailSource" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspaceViaBuilder(workspacePath, false, "Sources", "Services");

            var models = rc.GetModels(workspaceID, enSourceType.DynamicService);

            Assert.IsNull(models);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumMySqlDatabase_ExpectNull()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "EmailSource" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspaceViaBuilder(workspacePath, false, "Sources", "Services");

            var models = rc.GetModels(workspaceID, enSourceType.MySqlDatabase);

            Assert.AreEqual(0, models.Cast<object>().Count());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumManagementDynamicService_ExpectNull()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "EmailSource" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspaceViaBuilder(workspacePath, false, "Sources", "Services");

            var models = rc.GetModels(workspaceID, enSourceType.ManagementDynamicService);

            Assert.IsNull(models);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetModels")]
        public void ResourceCatalog_GetModels_WhenEnumUnknown_ExpectNullModels()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "EmailSource" }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspaceViaBuilder(workspacePath, false, "Sources", "Services");

            var models = rc.GetModels(workspaceID, enSourceType.Unknown);

            Assert.IsNull(models);
        }


        #endregion

        //
        // Static helpers
        //

        #region UpdateResourceCatalog            

        [TestMethod]
        [Description("Requires Valid arguments")]
        [Owner("Ashley Lewis")]
        public void ResourceCatalog_UnitTest_UpdateResourceNameWithNullOldName_ExpectRename()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();

            var path = EnvironmentVariables.GetWorkspacePath(workspaceID);
            Directory.CreateDirectory(path);
            const string resourceID = "ec636256-5f11-40ab-a044-10e731d87555";
            SaveResources(path, null, false, false, new[] { "Bug6619Dep" }, new[] { Guid.Parse(resourceID) });

            var serverVersionRepository = new Mock<IServerVersionRepository>();
            var rc = new ResourceCatalog(null, serverVersionRepository.Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceID.ToString() == resourceID);
            //------------Assert Precondition--------------------
            Assert.AreEqual(1, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.RenameResource(workspaceID, Guid.Parse(resourceID), "TestName", "TestName");
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual("Renamed Resource '" + resourceID + "' to 'TestName'", resourceCatalogResult.Message);
            string resourceContents = rc.GetResourceContents(workspaceID, oldResource.ResourceID).ToString();
            XElement xElement = XElement.Load(new StringReader(resourceContents), LoadOptions.None);
            var element = xElement.Attribute("Name");
            Assert.IsNotNull(element);
            Assert.AreEqual("TestName", element.Value);
            serverVersionRepository.Verify(a => a.StoreVersion(It.IsAny<IResource>(), "unknown", "Rename", workspaceID, "TestName"));
            var actionElem = xElement.Element("Action");
            Assert.IsNotNull(actionElem);
            var xamlElem = actionElem.Element("XamlDefinition");
            // ReSharper disable PossibleNullReferenceException
            Assert.IsTrue(xamlElem.Value.Contains("DisplayName=\"TestName\""));
            // ReSharper restore PossibleNullReferenceException
        }

        [TestMethod]
        [Description("Needs valid arguments")]
        [Owner("Ashley Lewis")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourceCatalog_UnitTest_UpdateResourceWithNullNewName_ExpectArgumentNullException()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceID = "ec636256-5f11-40ab-a044-10e731d87555";
            SaveResources(path, null, false, false, new[] { "Bug6619Dep" }, new[] { Guid.Parse(resourceID) });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceID.ToString() == resourceID);
            //------------Assert Precondition--------------------
            Assert.AreEqual(1, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.RenameResource(workspaceID, oldResource.ResourceID, null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual("<CompilerMessage>Updated Resource '50fef451-b41e-4bdf-92a1-4a41e254cde2' renamed to ''</CompilerMessage>", resourceCatalogResult.Message);
            string resourceContents = rc.GetResourceContents(workspaceID, oldResource.ResourceID).ToString();
            XElement xElement = XElement.Load(new StringReader(resourceContents), LoadOptions.None);
            XElement element = xElement.Element("Name");
            Assert.IsNotNull(element);
            Assert.AreEqual("Bug6619Dep", element.Value);
        }

        [TestMethod]
        [Description("Needs valid arguments")]
        [Owner("Ashley Lewis")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourceCatalog_UnitTest_UpdateResourceNameWithEmptyNewName_ExpectArgumentNullException()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceID = "ec636256-5f11-40ab-a044-10e731d87555";
            SaveResources(path, null, false, false, new[] { "Bug6619Dep" }, new[] { Guid.Parse(resourceID) });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceID.ToString() == resourceID);
            //------------Assert Precondition--------------------
            Assert.AreEqual(1, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.RenameResource(workspaceID, oldResource.ResourceID, "", "");
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual("<CompilerMessage>Updated Resource '50fef451-b41e-4bdf-92a1-4a41e254cde2' renamed to ''</CompilerMessage>", resourceCatalogResult.Message);
            string resourceContents = rc.GetResourceContents(workspaceID, oldResource.ResourceID).ToString();
            XElement xElement = XElement.Load(new StringReader(resourceContents), LoadOptions.None);
            XElement element = xElement.Element("Name");
            Assert.IsNotNull(element);
            Assert.AreEqual("Bug6619Dep", element.Value);
            XElement elementCat = xElement.Element("Category");
            Assert.IsNotNull(elementCat);
            Assert.AreEqual("TestCategory\\Bug6619Dep", element.Value);
        }

        [TestMethod]
        [Description("Updates the Category of the resource")]
        [Owner("Huggs")]
        public void ResourceCatalog_UnitTest_UpdateResourceCategoryValidArguments_ExpectFileContentsUpdated()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();

            var path = EnvironmentVariables.GetWorkspacePath(workspaceID) + "\\Bugs";
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.RenameCategory(workspaceID, "Bugs", "TestCategory");
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual("<CompilerMessage>Updated Category from 'Bugs' to 'TestCategory'</CompilerMessage>", resourceCatalogResult.Message);
            Assert.AreEqual("TestCategory\\Bug6619Dep", oldResource.GetResourcePath(workspaceID));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RenameCategory_GivenValidParams_ShouldReturnSucces()
        {
            //---------------Set up test pack-------------------
            var workspaceID = Guid.NewGuid();

            var path = EnvironmentVariables.GetWorkspacePath(workspaceID) + "\\Bugs";
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var resourceCatalogResult = rc.RenameCategory(workspaceID, "Bugs", "TestCategory", result);
            //---------------Test Result -----------------------
            Assert.AreEqual(resourceCatalogResult.Status, ExecStatus.Success);
            foreach (var resource in result)
            {
                Assert.AreEqual("TestCategory\\" + resource.ResourceName, resource.GetResourcePath(workspaceID));
            }
        }


        [TestMethod]
        [Description("Updates the Category of the resource")]
        [Owner("Huggs")]
        public void ResourceCatalog_RenameCategory_NoResources_ExpectErrorNoMatching()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();

            var path = EnvironmentVariables.GetWorkspacePath(workspaceID);
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.RenameCategory(workspaceID, "SomeNonExistentCategory", "TestCategory");
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.NoMatch, resourceCatalogResult.Status);
            string resourceContents = rc.GetResourceContents(workspaceID, oldResource.ResourceID).ToString();
            XElement xElement = XElement.Load(new StringReader(resourceContents), LoadOptions.None);
            XElement element = xElement.Element("Category");
            Assert.IsNotNull(element);
            Assert.AreEqual("Bugs\\Bug6619Dep", element.Value);
        }

        [TestMethod]
        [Description("Updates the Category of the resource")]
        [Owner("Huggs")]
        public void ResourceCatalog_UnitTest_UpdateResourceCategoryValidArgumentsDifferentCasing_ExpectFileContentsUpdated()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();

            var path = EnvironmentVariables.GetWorkspacePath(workspaceID) + "\\Bugs";
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.RenameCategory(workspaceID, "Bugs", "TestCategory");
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);

            Assert.AreEqual("TestCategory\\Bug6619Dep", oldResource.GetResourcePath(workspaceID));
        }

        [TestMethod]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void ResourceCatalog_DeleteResource_ResourceIDEmptyGuid_ExpectException()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            rc.DeleteResource(workspaceID, Guid.Empty, "TypeWorkflowService");
            //------------Assert Results-------------------------            
        }

        [TestMethod]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void ResourceCatalog_DeleteResource_TypeEmptyString_ExpectException()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            rc.DeleteResource(workspaceID, Guid.NewGuid(), "");
            //------------Assert Results-------------------------            
        }

        [TestMethod]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void ResourceCatalog_DeleteResource_TypeNull_ExpectException()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            rc.DeleteResource(workspaceID, Guid.NewGuid(), null);
            //------------Assert Results-------------------------            
        }

        [TestMethod]
        [Owner("Huggs")]
        public void ResourceCatalog_DeleteResource_ResourceNotFound_ExpectNoMatchResult()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            var resourceCatalogResult = rc.DeleteResource(workspaceID, Guid.NewGuid(), "TypeWorkflowService");
            //------------Assert Results-------------------------        
            Assert.AreEqual(ExecStatus.NoMatch, resourceCatalogResult.Status);
        }

        [TestMethod]
        [Owner("Huggs")]
        public void ResourceCatalog_DeleteResource_FoundMultipleResources_ExpectDuplicateMatchResult()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            IResource resourceToUpdate = result.FirstOrDefault(resource => resource.ResourceName == "Bug6619");
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            Assert.IsNotNull(resourceToUpdate);
            //------------Execute Test---------------------------
            resourceToUpdate.ResourceID = oldResource.ResourceID;
            var resourceCatalogResult = rc.DeleteResource(workspaceID, resourceToUpdate.ResourceID, "WorkflowService");
            //------------Assert Results-------------------------        
            Assert.AreEqual(ExecStatus.DuplicateMatch, resourceCatalogResult.Status);
        }

        [TestMethod]
        [Owner("Huggs")]
        public void ResourceCatalog_DeleteResource_FoundResource_ExpectResourceDeleted()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var serverVersionRepository = new Mock<IServerVersionRepository>();
            serverVersionRepository.Setup(a => a.GetVersions(It.IsAny<Guid>())).Returns(new List<IExplorerItem> { new ServerExplorerItem("bob", Guid.NewGuid(), "Server", null, Permissions.Administrator, "") { VersionInfo = new VersionInfo(DateTime.Now, "reason", "", "1", Guid.NewGuid(), Guid.NewGuid()) } });
            var rc = new ResourceCatalog(null, serverVersionRepository.Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            var resourceCatalogResult = rc.DeleteResource(workspaceID, oldResource.ResourceID, "WorkflowService");
            //------------Assert Results-------------------------        
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            var resourceToFind = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            serverVersionRepository.Verify(a => a.DeleteVersion(It.IsAny<Guid>(), "1", "Bug6619Dep"), Times.Once());
            Assert.IsNull(resourceToFind);
        }



        [TestMethod]
        [Owner("Huggs")]
        public void ResourceCatalog_DeleteResource_FoundResource_ExpectResourceDeleted_VersionsNotDeleted()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var serverVersionRepository = new Mock<IServerVersionRepository>();
            serverVersionRepository.Setup(a => a.GetVersions(It.IsAny<Guid>())).Returns(new List<IExplorerItem> { new ServerExplorerItem("bob", Guid.NewGuid(), "Server", null, Permissions.Administrator, "") { VersionInfo = new VersionInfo(DateTime.Now, "reason", "", "1", Guid.NewGuid(), Guid.NewGuid()) } });
            var rc = new ResourceCatalog(null, serverVersionRepository.Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            var resourceCatalogResult = rc.DeleteResource(workspaceID, oldResource.ResourceID, "WorkflowService", false);
            //------------Assert Results-------------------------        
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            var resourceToFind = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            serverVersionRepository.Verify(a => a.DeleteVersion(It.IsAny<Guid>(), "1", ""), Times.Never());
            Assert.IsNull(resourceToFind);
        }

        [TestMethod]
        [Owner("Huggs")]
        public void ResourceCatalog_GetResource_Workflow_ExpectResource()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            var resourceFound = rc.GetResource<Workflow>(workspaceID, oldResource.ResourceID);
            //------------Assert Results-------------------------        
            Assert.IsNotNull(resourceFound);
            Assert.AreEqual(oldResource.ResourceID, resourceFound.ResourceID);
        }

        [TestMethod]
        [Owner("Huggs")]
        public void ResourceCatalog_GetResource_DbSource_ExpectResource()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "CitiesDatabase";
            SaveResources(path, null, false, false, new[] { "Bug6619" }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var xml = XmlResource.Fetch("CitiesDatabase");
            var resource = new DbSource(xml);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.SaveResource(workspaceID, resource, "");
            var result = catalog.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(r => r.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            var resourceFound = catalog.GetResource<DbSource>(workspaceID, oldResource.ResourceID);
            //------------Assert Results-------------------------        
            Assert.IsNotNull(resourceFound);
            Assert.AreEqual(oldResource.ResourceID, resourceFound.ResourceID);
        }


        #endregion

        #region SaveResources

        public static string SaveResources(Guid workspaceID, string versionNo, bool injectID, bool signXml, string[] sources, string[] services, out List<IResource> resources, Guid[] sourceIDs, Guid[] serviceIDs, bool loadws = false)
        {
            lock (syncRoot)
            {

                var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
                var sourcesPath = EnvironmentVariables.ResourcePath;
                var servicesPath = EnvironmentVariables.ResourcePath;
                if (loadws)
                {
                    workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
                    sourcesPath = Path.Combine(workspacePath, "Resources");
                    servicesPath = Path.Combine(workspacePath, "Resources");

                }

                Directory.CreateDirectory(Path.Combine(sourcesPath, "VersionControl"));
                Directory.CreateDirectory(Path.Combine(servicesPath, "VersionControl"));

                Directory.CreateDirectory(workspacePath);

                resources = new List<IResource>();
                if (sources != null && sources.Length != 0)
                {
                    resources.AddRange(SaveResources(workspacePath, versionNo, injectID, signXml, sources, sourceIDs));
                }
                if (services != null && services.Length != 0)
                {
                    resources.AddRange(SaveResources(workspacePath, versionNo, injectID, signXml, services, serviceIDs));
                }

                return workspacePath;
            }
        }

        static string SaveResources(Guid workspaceID, out List<IResource> resources, bool saveToWSPath = false)
        {
            return SaveResources(workspaceID, null, false, false,
                new[] { "CatalogSourceAnytingToXmlPlugin", "CatalogSourceCitiesDatabase", "CatalogSourceTestServer" },
                new[] { "CatalogServiceAllTools", "CatalogServiceCitiesDatabase", "CatalogServiceCalculateRecordSetSubtract" },
                out resources,
                new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() },
                new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }, saveToWSPath);
        }

        static IEnumerable<IResource> SaveResources(string resourcesPath, string versionNo, bool injectID, bool signXml, IEnumerable<string> resourceNames, Guid[] resourceIDs, bool createWorDef = false)
        {
            lock (syncRoot)
            {
                var result = new List<IResource>();
                int count = 0;
                foreach (var resourceName in resourceNames)
                {
                    var xml = XmlResource.Fetch(resourceName);
                    if (injectID)
                    {
                        var idAttr = xml.Attribute("ID");
                        if (idAttr == null)
                        {
                            xml.Add(new XAttribute("ID", resourceIDs[count]));
                        }
                    }
                    if (createWorDef)
                    {
                        var workflowHelper = new WorkflowHelper();
                        var builder = workflowHelper.CreateWorkflow(resourceName);
                        var workflowXaml = workflowHelper.GetXamlDefinition(builder);
                        xml.Add(workflowXaml);
                    }

                    var contents = xml.ToString(SaveOptions.DisableFormatting);

                    if (signXml)
                    {
                        contents = HostSecurityProvider.Instance.SignXml(new StringBuilder(contents)).ToString();
                    }
                    var res = new Resource(xml);
                    var resourceDirectory = resourcesPath + "\\";
                    res.FilePath = resourceDirectory + res.ResourceName + ".xml";
                    FileInfo f = new FileInfo(res.FilePath);
                    if (f.Directory != null && !f.Directory.Exists)
                    {
                        Directory.CreateDirectory(resourceDirectory);
                    }
                    // Just in case sign the xml

                    File.WriteAllText(res.FilePath, contents, Encoding.UTF8);

                    if (!string.IsNullOrEmpty(versionNo))
                    {
                        var versionControlPath = Path.Combine(resourcesPath, "VersionControl");
                        if (!Directory.Exists(versionControlPath))
                        {
                            Directory.CreateDirectory(versionControlPath);
                        }
                        File.WriteAllText(
                            Path.Combine(resourcesPath,
                                $"VersionControl\\{resourceName}.V{versionNo}.xml"),
                            contents, Encoding.UTF8);
                    }
                    result.Add(res);
                    count++;

                }
                return result;
            }
        }





        #endregion

        #region GetDependants

        [TestMethod]
        public void GetDependantsWhereResourceIsDependedOnExpectNonEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();

            var path = EnvironmentVariables.GetWorkspacePath(workspaceID);
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependants(workspaceID, Guid.Parse("ec636256-5f11-40ab-a044-10e731d87555"));
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dependants.Count);
            Assert.AreEqual(Guid.Parse("1736ca6e-b870-467f-8d25-262972d8c3e8"), dependants[0]);
        }

        [TestMethod]
        public void UpdateResourceWhereResourceIsDependedOnExpectNonEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var path = EnvironmentVariables.GetWorkspacePath(workspaceID);
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            const string depResourceName = "Bug6619";
            SaveResources(path, null, false, false, new[] { depResourceName, resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var sv = new Mock<IServerVersionRepository>();
            var rc = new ResourceCatalog(null, sv.Object);
            var rcSaveProvider = new ResourceSaveProviderMock(rc, sv.Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            var resource = result.FirstOrDefault(r => r.ResourceName == resourceName);
            var depresource = result.FirstOrDefault(r => r.ResourceName == depResourceName);
            Assert.IsNotNull(resource);
            var beforeService = rc.GetDynamicObjects<DynamicService>(workspaceID, resourceName).FirstOrDefault();
            Assert.IsNotNull(beforeService);
            ServiceAction beforeAction = beforeService.Actions.FirstOrDefault();
            var xElement = rc.GetResourceContents(resource).ToXElement();
            var element = xElement.Element("DataList");
            Assert.IsNotNull(element);
            element.Add(new XElement("a"));
            element.Add(new XElement("b"));
            var s = xElement.ToString();
            var messages = new CompileMessageList();
            rc.SendResourceMessages += (guid, tos) =>
            {
                messages.MessageList = tos;
            };
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            rcSaveProvider.CompileTheResourceAfterSave(workspaceID, resource, new StringBuilder(s), beforeAction);
            //------------Assert Results-------------------------

            xElement = rc.GetResourceContents(depresource).ToXElement();
            Assert.IsNotNull(xElement);
            var errorMessages = xElement.Element("ErrorMessages");
            Assert.IsNotNull(errorMessages);
            var errorElement = errorMessages.Element("ErrorMessage");
            var isValid = xElement.AttributeSafe("IsValid");
            var messageType = Enum.Parse(typeof(CompileMessageType), errorElement.AttributeSafe("MessageType"), true);
            Assert.AreEqual("false", isValid);
            Assert.AreEqual(CompileMessageType.MappingChange, messageType);
            Assert.IsNotNull(depresource);
            var message = messages.MessageList.FirstOrDefault(msg => msg.WorkspaceID != Guid.Empty);
            Assert.IsNotNull(message, "No valid update resource messages published");
            Assert.AreEqual(workspaceID, message.WorkspaceID);
            Assert.AreEqual(depresource.ResourceName, message.ServiceName);
            Assert.AreEqual(depresource.ResourceID, message.ServiceID);
            Assert.AreNotEqual(depresource.ResourceID, message.UniqueID);
            Assert.AreNotEqual(resource.ResourceID, message.UniqueID);
        }

        [TestMethod]
        public void UpdateResourceWhereResourceIsDependedOnExpectNonEmptyListForResource()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();

            var path = EnvironmentVariables.GetWorkspacePath(workspaceID);
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var sv = new Mock<IServerVersionRepository>();
            var rc = new ResourceCatalog(null, sv.Object);
            var rcSaveProvider = new ResourceSaveProviderMock(rc, sv.Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            var resource = result.FirstOrDefault(r => r.ResourceName == "Bug6619Dep");
            var depresource = result.FirstOrDefault(r => r.ResourceName == "Bug6619");
            Assert.IsNotNull(resource);
            var beforeService = rc.GetDynamicObjects<DynamicService>(workspaceID, resourceName).FirstOrDefault();
            Assert.IsNotNull(beforeService);
            ServiceAction beforeAction = beforeService.Actions.FirstOrDefault();

            var xElement = rc.GetResourceContents(resource).ToXElement();

            var element = xElement.Element("DataList");
            Assert.IsNotNull(element);
            element.Add(new XElement("a"));
            element.Add(new XElement("b"));
            var s = xElement.ToString();
            var messages = new CompileMessageList();
            rc.SendResourceMessages += (guid, tos) =>
            {
                messages.MessageList = tos;
            };
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            rcSaveProvider.CompileTheResourceAfterSave(workspaceID, resource, new StringBuilder(s), beforeAction);
            //------------Assert Results-------------------------

            xElement = rc.GetResourceContents(depresource).ToXElement();
            Assert.IsNotNull(xElement);
            var errorMessages = xElement.Element("ErrorMessages");
            Assert.IsNotNull(errorMessages);
            var errorElement = errorMessages.Element("ErrorMessage");
            var isValid = xElement.AttributeSafe("IsValid");
            var messageType = Enum.Parse(typeof(CompileMessageType), errorElement.AttributeSafe("MessageType"), true);
            Assert.AreEqual("false", isValid);
            Assert.AreEqual(CompileMessageType.MappingChange, messageType);
            Assert.IsNotNull(depresource);
            var message = messages.MessageList.FirstOrDefault(msg => msg.WorkspaceID != Guid.Empty);
            Assert.IsNotNull(message, "No valid update resource messages published");
            Assert.AreEqual(workspaceID, message.WorkspaceID);
            Assert.AreEqual(depresource.ResourceName, message.ServiceName);
            Assert.AreEqual(depresource.ResourceID, message.ServiceID);
            Assert.AreNotEqual(depresource.ResourceID, message.UniqueID);
            Assert.AreNotEqual(resource.ResourceID, message.UniqueID);
        }

        [TestMethod]
        public void GetDependantsWhereNoResourcesExpectEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var result = rc.LoadWorkspaceViaBuilder("xx", false);
            //const string resourceName = "resource";
            //------------Assert Precondition-----------------
            Assert.AreEqual(0, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependants(workspaceID, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, dependants.Count);
        }

        [TestMethod]
        public void GetDependantsWhereResourceHasNoDependedOnExpectNonEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();

            var path = EnvironmentVariables.GetWorkspacePath(workspaceID);
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { resourceName }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            //------------Assert Precondition-----------------
            Assert.AreEqual(1, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependants(workspaceID, Guid.Parse("ec636256-5f11-40ab-a044-10e731d87555"));
            //------------Assert Results-------------------------
            Assert.AreEqual(0, dependants.Count);
        }

        #endregion

        #region GetDependantsAsResourceForTrees

        [TestMethod]
        public void GetDependantsAsResourceForTreesWhereResourceIsDependedOnExpectNonEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();

            var path = EnvironmentVariables.GetWorkspacePath(workspaceID);
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependentsAsResourceForTrees(workspaceID, Guid.Parse("ec636256-5f11-40ab-a044-10e731d87555"));
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dependants.Count);
            Assert.AreEqual("Bug6619", dependants[0].ResourceName);
        }

        [TestMethod]
        public void GetDependantsAsResourceForTreesWhereNoResourcesExpectEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var result = rc.LoadWorkspaceViaBuilder("xx", false);
            //  const string resourceName = "resource";
            //------------Assert Precondition-----------------
            Assert.AreEqual(0, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependentsAsResourceForTrees(workspaceID, Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.AreEqual(0, dependants.Count);
        }

        [TestMethod]
        public void GetDependantsAsResourceForTreesWhereResourceHasNoDependedOnExpectNonEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();

            var path = EnvironmentVariables.GetWorkspacePath(workspaceID);
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { resourceName }, new[] { Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            //------------Assert Precondition-----------------
            Assert.AreEqual(1, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependentsAsResourceForTrees(workspaceID, Guid.Parse("7b8c9b6e-16f4-4771-8605-655bbfab7543"));
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dependants.Count);
        }
        #endregion

        #region VerifyPayload

        #endregion

        #region VerifyObjectGraph

        static void VerifyObjectGraph<TGraph>(ICollection<IResource> expectedResources, ICollection<TGraph> actualGraphs)
            where TGraph : DynamicServiceObjectBase
        {
            Assert.AreEqual(expectedResources.Count, actualGraphs.Count);

            foreach (var expected in expectedResources)
            {
                IResource resource = expected;
                var actualGraph = actualGraphs.FirstOrDefault(g => g.Name == resource.ResourceName);
                Assert.IsNotNull(actualGraph);

                var actual = new Resource(actualGraph.ResourceDefinition.ToXElement());
                Assert.AreEqual(expected.ResourceID, actual.ResourceID);
                Assert.AreEqual(expected.ResourceType, actual.ResourceType);
            }
        }

        #endregion

        #region Rename Resource

        #endregion

        #region GetResourceList

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetResourceList")]
        public void ResourceCatalog_GetResourceList_WhenUsingNameAndResourcesNotPresent_ExpectEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspaceViaBuilder(workspacePath, false, "Services");
            //------------Execute Test---------------------------
            var filterParams = new Dictionary<string, string>()
            {
                {"resourceName","Bob" },
                {"type","*" }
            };
            var workflow = rc.GetResourceList(workspaceID, filterParams);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, workflow.Count);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetResourceList")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void ResourceCatalog_GetResourceList_WhenNameAndResourceNameAndTypeNull_ExpectException()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspaceViaBuilder(workspacePath, false, "Bugs");
            //------------Execute Test---------------------------
            var filterParams = new Dictionary<string, string>()
            {
                {"resourceName",null },
                {"type",null }
            };
            rc.GetResourceList(workspaceID, filterParams);

        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetResourceList")]
        public void ResourceCatalog_GetResourceList_WhenUsingIdAndResourcesPresent_ExpectResourceList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var resources = SaveResources(workspacePath, null, false, false, new[] { "Bug6619", resourceName }, new[] { id1, id2 }).ToList();

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspaceViaBuilder(workspacePath, false, "Workflows");

            var searchString = resources.Aggregate(string.Empty, (current, res) => current + (res.ResourceID + ","));

            //------------Execute Test---------------------------
            var filterParams = new Dictionary<string, string>()
            {
                {"guidCsv",searchString },
                {"type","*" }
            };
            var workflow = rc.GetResourceList(workspaceID, filterParams);
            //------------Assert Results-------------------------
            Assert.IsNotNull(workflow);
            Assert.AreEqual(2, workflow.Count);

            // the ordering swaps around - hence the contains assert ;)

            StringAssert.Contains(workflow[0].ResourceName, "Bug6619");
            StringAssert.Contains(workflow[1].ResourceName, "Bug6619");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetResourceList")]
        public void ResourceCatalog_GetResourceList_WhenUsingIdAndResourcesNotPresent_ExpectEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var resources = SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { id1, id2 }).ToList();

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspaceViaBuilder(workspacePath, false, "Workflows");

            var searchString = resources.Aggregate(string.Empty, (current, res) => current + (Guid.NewGuid() + ","));

            //------------Execute Test---------------------------
            var filterParams = new Dictionary<string, string>
            {
                {"guidCsv",searchString },
                {"type","*" }
            };
            var workflow = rc.GetResourceList(workspaceID, filterParams);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, workflow.Count);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetResourceList")]
        public void ResourceCatalog_GetResourceList_WhenUsingIdAndTypeNull_ShouldStillReturn()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var resources = SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { id1, id2 }).ToList();

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspaceViaBuilder(workspacePath, false, "Workflows");

            var searchString = resources.Aggregate(string.Empty, (current, res) => current + (Guid.NewGuid() + ","));

            //------------Execute Test---------------------------
            var filterParams = new Dictionary<string, string>()
            {
                {"guidCsv",searchString },
                {"type",null }
            };
            var resourceList = rc.GetResourceList(workspaceID, filterParams);
            Assert.IsNotNull(resourceList);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_GetResourceList")]
        public void ResourceCatalog_GetResourceList_WhenUsingIdAndGuidCsvNull_ExpectEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { id1, id2 });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspaceViaBuilder(workspacePath, false, "Workflows");

            //------------Execute Test---------------------------
            var filterParams = new Dictionary<string, string>()
            {
                {"guidCsv",null },
                {"type","*" }
            };
            var workflow = rc.GetResourceList(workspaceID, filterParams);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, workflow.Count);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ResourceCatalogPluginContainer_GivenVersion_ShouldCreateInstancesWithVersion()
        {
            //---------------Set up test pack-------------------

            ResourceCatalogPluginContainer catalogPluginContainer = new ResourceCatalogPluginContainer(new Mock<IServerVersionRepository>().Object, new ConcurrentDictionarySafe<Guid, List<IResource>>());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(catalogPluginContainer);
            //---------------Execute Test ----------------------
            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalogPluginContainer.Build(rc);
            //---------------Test Result -----------------------
            Assert.IsNotNull(catalogPluginContainer.CopyProvider);
            Assert.IsNotNull(catalogPluginContainer.SaveProvider);
            Assert.IsNotNull(catalogPluginContainer.SyncProvider);
            Assert.IsNotNull(catalogPluginContainer.RenameProvider);
            Assert.IsNotNull(catalogPluginContainer.DeleteProvider);
            Assert.IsNotNull(catalogPluginContainer.LoadProvider);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ResourceCatalogPluginContainer_GivenVersionAndManagementServices_ShouldCreateInstances()
        {
            //---------------Set up test pack-------------------
            IEnumerable<DynamicService> services = new List<DynamicService>();
            ResourceCatalogPluginContainer catalogPluginContainer = new ResourceCatalogPluginContainer(new Mock<IServerVersionRepository>().Object, new ConcurrentDictionarySafe<Guid, List<IResource>>(), services);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(catalogPluginContainer);
            //---------------Execute Test ----------------------
            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalogPluginContainer.Build(rc);
            //---------------Test Result -----------------------
            Assert.IsNotNull(catalogPluginContainer.CopyProvider);
            Assert.IsNotNull(catalogPluginContainer.SaveProvider);
            Assert.IsNotNull(catalogPluginContainer.SyncProvider);
            Assert.IsNotNull(catalogPluginContainer.RenameProvider);
            Assert.IsNotNull(catalogPluginContainer.DeleteProvider);
            Assert.IsNotNull(catalogPluginContainer.LoadProvider);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dispose_GivenInstance_ShouldCleaup()
        {
            //---------------Set up test pack-------------------
            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            rc.Dispose();
            //---------------Test Result -----------------------
            Assert.AreEqual(0, rc.WorkspaceResources.Count);
            Assert.AreEqual(0, rc.WorkspaceLocks.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToPayload_GivenIsReservedService_ShouldAppendTypeAndName()
        {
            //---------------Set up test pack-------------------
            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var mockResource = new Mock<IResource>();
            const string reservedservice = "ReservedService";
            mockResource.Setup(resource => resource.ResourceType).Returns(reservedservice);
            const string s = "ReservedName";
            mockResource.Setup(resource => resource.ResourceName).Returns(s);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var stringBuilder = rc.ToPayload(mockResource.Object);
            //---------------Test Result -----------------------
            Assert.IsTrue(stringBuilder.Contains(reservedservice));
            Assert.IsTrue(stringBuilder.Contains(s));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MapServiceActionDependencies_GivenServiceName_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { id1, id2 });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspaceViaBuilder(workspacePath, false, "Workflows");
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                rc.MapServiceActionDependencies(workspaceID, new ServiceAction() { SourceName = "SourceName", ServiceID = id1 });
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void LoadResourceActivityCache_GivenServiceName_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { id1, id2 });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspaceViaBuilder(workspacePath, false, "Workflows");

            Dictionary<Guid, IResourceActivityCache> _parsers = new Dictionary<Guid, IResourceActivityCache>();
            var mock = new Mock<IResourceActivityCache>();

            _parsers.Add(workspaceID, mock.Object);
            const string propertyName = "_parsers";
            FieldInfo fieldInfo = typeof(ResourceCatalog).GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Static);
            fieldInfo?.SetValue(rc, _parsers);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                rc.LoadResourceActivityCache(GlobalConstants.ServerWorkspaceID);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void LoadResourceActivityCache_GivenServiceName_ShouldAddActivityToParserCache()
        {
            //---------------Set up test pack-------------------
            var workspaceID =GlobalConstants.ServerWorkspaceID;
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { id1, id2 });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspaceViaBuilder(workspacePath, false, "Workflows");

            Dictionary<Guid, IResourceActivityCache> _parsers = new Dictionary<Guid, IResourceActivityCache>();

            const string propertyName = "_parsers";
            FieldInfo fieldInfo = typeof(ResourceCatalog).GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Static);
            fieldInfo?.SetValue(rc, _parsers);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            rc.LoadResourceActivityCache(workspaceID);
            //---------------Test Result -----------------------
            var resourceActivityCache = _parsers[GlobalConstants.ServerWorkspaceID];
            var actId = Guid.Parse("1736ca6e-b870-467f-8d25-262972d8c3e8");
            var actId2 = Guid.Parse("ec636256-5f11-40ab-a044-10e731d87555");
            Assert.IsTrue(resourceActivityCache.HasActivityInCache(actId));
            Assert.IsTrue(resourceActivityCache.HasActivityInCache(actId2));
            var act1 = resourceActivityCache.GetActivity(actId);
            var act2 = resourceActivityCache.GetActivity(actId2);
            Assert.IsNotNull(act1);
            Assert.IsNotNull(act2);            
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void LoadResourceActivityCache_GivenServiceNameWithActivityInCache_ShouldReturnFromCache()
        {
            //---------------Set up test pack-------------------
            var workspaceID =GlobalConstants.ServerWorkspaceID;
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { id1, id2 });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspaceViaBuilder(workspacePath, false, "Workflows");

            Dictionary<Guid, IResourceActivityCache> _parsers = new Dictionary<Guid, IResourceActivityCache>();

            const string propertyName = "_parsers";
            FieldInfo fieldInfo = typeof(ResourceCatalog).GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Static);
            fieldInfo?.SetValue(rc, _parsers);
            rc.LoadResourceActivityCache(workspaceID);
            var actId = Guid.Parse("1736ca6e-b870-467f-8d25-262972d8c3e8");
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var act1 = rc.Parse(workspaceID, actId);
            //---------------Test Result -----------------------
            Assert.IsNotNull(act1);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void LoadResourceActivityCache_GivenServiceName_ShouldPopulateServiceActionRepo()
        {
            //---------------Set up test pack-------------------
            var workspaceID =GlobalConstants.ServerWorkspaceID;
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { id1, id2 });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspaceViaBuilder(workspacePath, false, "Workflows");

            Dictionary<Guid, IResourceActivityCache> _parsers = new Dictionary<Guid, IResourceActivityCache>();

            const string propertyName = "_parsers";
            FieldInfo fieldInfo = typeof(ResourceCatalog).GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Static);
            fieldInfo?.SetValue(rc, _parsers);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            rc.LoadResourceActivityCache(workspaceID);
            //---------------Test Result -----------------------
            var actId = Guid.Parse("1736ca6e-b870-467f-8d25-262972d8c3e8");
            var actId2 = Guid.Parse("ec636256-5f11-40ab-a044-10e731d87555");
            var ds1 = ServiceActionRepo.Instance.ReadCache(actId);
            var ds2 = ServiceActionRepo.Instance.ReadCache(actId2);
            Assert.IsNotNull(ds1);
            Assert.IsNotNull(ds2);
        }

        [TestMethod]
        public void DeleteResourceWithSingleExistingResourceName_ShouldRemoveFromCache()
        {
            //---------------Set up test pack-------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { id1, id2 });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspaceViaBuilder(workspacePath, false, "Workflows");

            Dictionary<Guid, IResourceActivityCache> _parsers = new Dictionary<Guid, IResourceActivityCache>();

            const string propertyName = "_parsers";
            FieldInfo fieldInfo = typeof(ResourceCatalog).GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Static);
            fieldInfo?.SetValue(rc, _parsers);
            rc.LoadResourceActivityCache(workspaceID);
            var actId = Guid.Parse("1736ca6e-b870-467f-8d25-262972d8c3e8");
            //---------------Assert Precondition----------------
            Assert.IsNotNull(ServiceActionRepo.Instance.ReadCache(actId));
            //---------------Execute Test ----------------------
            rc.DeleteResource(workspaceID, actId, "WorkflowService");
            Assert.IsNull(ServiceActionRepo.Instance.ReadCache(actId));


        }
        #endregion


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ResourceCatalog_GetResourceDuplicate_Give2SameFilesInDifferentFolders_ShouldReturnPaths()
        {
            var path = EnvironmentVariables.ResourcePath;
            var sameResourceId = Guid.NewGuid();
            //------------Setup for test--------------------------            
            var resourcePath = path + "\\MyTest\\Folder1";
            var resourcePath1 = path + "\\MyTest\\Folder2";
            //------------Setup for test--------------------------         
            var catalog = new ResourceCatalog();
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(resourcePath, null, false, false, new[]
            {
                "Bug6619", resourceName, "Bug6619"
            }, new[]
            {
                sameResourceId, Guid.NewGuid(), sameResourceId
            });
            SaveResources(resourcePath1, null, false, false, new[]
            {
                "Bug6619"
            }, new[]
            {
                sameResourceId
            });
            //------------Execute Test---------------------------
            var folders = Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories);
            var allFolders = folders.ToList();
            allFolders.Add(path);
            catalog.LoadWorkspaceViaBuilder(path, true, allFolders.ToArray());
            var duplicateResources = catalog.GetDuplicateResources();
            //------------Assert Results-------------------------
            Assert.IsTrue(duplicateResources.Count > 0);
            var duplicateResource = duplicateResources.Single();
            Assert.IsTrue(duplicateResource.ResourcePath.Count == 2);
            Assert.IsTrue(duplicateResource.ResourcePath[0].Contains(resourcePath));
            Assert.IsTrue(duplicateResource.ResourcePath[1].Contains(resourcePath1));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ResourceCatalog_UnitTest_DuplicateResourceResourceWithNullDestination_ExpectArgumentNullException()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            //ResourceCatalogResult resourceCatalogResult = rc.DuplicateResource(oldResource.ResourceID, oldResource.GetResourcePath(workspaceID), null);
            ////------------Assert Results-------------------------
            //Assert.AreEqual(ExecStatus.Fail, resourceCatalogResult.Status);
            //Assert.AreEqual(@"Duplicated Failure Value cannot be null.Parameter name: key".Replace(Environment.NewLine, ""), resourceCatalogResult.Message.Replace(Environment.NewLine, ""));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ResourceCatalog_UnitTest_DuplicateResourceResourceWithValidArgs_ExpectSuccesResult()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            //ResourceCatalogResult resourceCatalogResult = rc.DuplicateResource(oldResource.ResourceID, oldResource.GetResourcePath(workspaceID), "SomeName");
            ////------------Assert Results-------------------------
            //Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            //Assert.AreEqual(@"Duplicated Successfully".Replace(Environment.NewLine, ""), resourceCatalogResult.Message.Replace(Environment.NewLine, ""));
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ResourceCatalog_UnitTest_DuplicateResourceResourceWithValidArgs_ExpectNewDisplayName()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() }, true);

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            var xElement = oldResource.ToXml();

            var containsOrgName = xElement.ToString(SaveOptions.DisableFormatting).Contains(resourceName);
            Assert.IsTrue(containsOrgName);

            //------------Execute Test---------------------------
            const string destinationPath = "SomeName";
            var resourceCatalogResult = rc.DuplicateResource(oldResource.ResourceID, oldResource.GetResourcePath(workspaceID), destinationPath);
            //------------Assert Results-------------------------
            result = rc.GetResources(workspaceID);
            IResource dupResource = result.FirstOrDefault(resource => resource.ResourceName == destinationPath);
            Assert.IsNotNull(dupResource);
            var dupXelement = dupResource.ToXml();
            var newNamecontains = dupXelement.ToString(SaveOptions.DisableFormatting).Contains(destinationPath);
            containsOrgName = dupXelement.ToString(SaveOptions.DisableFormatting).Contains(resourceName);
            Assert.IsTrue(newNamecontains);
            Assert.IsNotNull(resourceCatalogResult);
            Assert.IsFalse(containsOrgName);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ResourceCatalog_UnitTest_DuplicateFolderResourceWithValidArgs_ExpectSuccesResult()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.DuplicateFolder(oldResource.GetResourcePath(GlobalConstants.ServerWorkspaceID), "Destination", "NewName", false);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual(@"Duplicated Successfully".Replace(Environment.NewLine, ""), resourceCatalogResult.Message.Replace(Environment.NewLine, ""));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ResourceCatalog_UnitTest_DuplicateFolderResourceWithInvalidArgs_ExpectExceptions()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            try
            {
                Assert.Fail("No Exceptions Thrown");
            }
            catch (Exception)
            {
                //
            }

            //------------Assert Results-------------------------

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ResourceCatalog_UnitTest_GetResourceListGivenWorkspace_ExpectResources()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var resourceList = rc.GetResourceList(workspaceID);
            //------------Assert Precondition-----------------
            Assert.IsNotNull(resourceList);
            //------------Execute Test---------------------------
            var count = resourceList.Count;

            //------------Assert Results-------------------------
            Assert.AreEqual(2, count);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ResourceCatalog_UnitTest_GetResourceCountGivenWorkspace_ExpectCorrectResources()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var resourceList = rc.GetResourceCount(workspaceID);
            //------------Assert Precondition-----------------
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual(2, resourceList);

        }
     

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(XmlException))]
        public void ResourceCatalog_UnitTest_GetResourceOfTNotExist_ExpectNull()
        {
            //------------Setup for test--------------------------

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);

            //------------Assert Precondition-----------------
            //------------Execute Test---------------------------
            var resourceList = rc.GetResource<PluginSource>(Guid.NewGuid(), Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.IsNull(resourceList);

        }
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourceCatalog_UnitTest_GetDependantsInvalidArgs_ExpectExceptions()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;


            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);

            //------------Assert Precondition-----------------
            //------------Execute Test---------------------------
            rc.GetDependants(workspaceID, null);

            //------------Assert Results-------------------------

        }
        /// <summary>
        /// Integration through the Fix references expecting no exception
        /// </summary>
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ResourceCatalog_UnitTest_GivenFixRefsTrue_ExpectResourceContentsChanges()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);
            IResource oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------


            rc.DuplicateFolder("", oldResource.GetResourcePath(workspaceID), "Null", true);
        }



        private ExecuteMessage ConvertToMsg(string payload)
        {
            return JsonConvert.DeserializeObject<ExecuteMessage>(payload);
        }

        private class ResourceSaveProviderMock : ResourceSaveProvider
        {
            public ResourceSaveProviderMock(IResourceCatalog resourceCatalog, IServerVersionRepository serverVersionRepository)
                : base(resourceCatalog, serverVersionRepository)
            {
            }

            public new void CompileTheResourceAfterSave(Guid workspaceID, IResource resource, StringBuilder contents, ServiceAction beforeAction)
            {
                base.CompileTheResourceAfterSave(workspaceID, resource, contents, beforeAction);
            }
        }
    }
}
