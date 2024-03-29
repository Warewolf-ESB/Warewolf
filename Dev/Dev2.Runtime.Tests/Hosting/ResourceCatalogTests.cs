/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Xml.Linq;
using Dev2.Activities;
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
using System.Collections.Concurrent;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    [TestCategory("Runtime Hosting")]
    public class ResourceCatalogTests
    {
        // Change this if you change the number of resources saved by SaveResources()
        const int SaveResourceCount = 6;
        static readonly object SyncRoot = new object();
        
        const int _numOfTestWFs = 2000;
        const string _resourceName = "wolf-Test_WF_";

        static List<string> _testSourceWFs = new List<string>();
        static List<Guid> _resourceIds = new List<Guid>();

        [TestInitialize]
        public void Initialise()
        {
            var workspacePath = EnvironmentVariables.ResourcePath;
            if (Directory.Exists(workspacePath))
            {
                try
                {
                    Directory.Delete(workspacePath, true);
                }
                catch(IOException)
                { //Best effort
                }
            }
            if (!Directory.Exists(EnvironmentVariables.ResourcePath))
            {
                Directory.CreateDirectory(EnvironmentVariables.ResourcePath);
            }

            try
            {
                CustomContainer.Register<IActivityParser>(new ActivityParser());
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine("Error putting IActivityParser into the DI container:\n" + e.Message);
            }

            if (EnvironmentVariables.ApplicationPath == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var loc = assembly.Location;
                EnvironmentVariables.ApplicationPath = Path.GetDirectoryName(loc);
            }

            try
            {
                var (testSourceWFs, resourceIds) = CalculateTestWFs(_resourceName, _numOfTestWFs);
                _testSourceWFs = testSourceWFs;
                _resourceIds = resourceIds;
            }
            catch (Exception ex)
            {

                throw new Exception("TEST: ResourceCatalogTests failed to Calculate Test Wolrkflows: " + ex.Message);
            }

            (List<string> testWFs, List<Guid> ResourceIds) CalculateTestWFs(string resourceName, int numOfTestWFs)
            {
                var resourceIds = new List<Guid>();
                var workflows = new List<string>();
                for (int i = 0; i < numOfTestWFs; i++)
                {
                    workflows.Add((resourceName + (i + 1)).ToString());
                    resourceIds.Add(Guid.NewGuid());
                }
                return (workflows, resourceIds);
            }

        }



        #region Instance

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void LoadWorkspaceAsyncWithNullWorkspaceArgumentExpectedThrowsArgumentNullException()
        {
            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);

            rc.LoadWorkspaceViaBuilder(null, false);
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void LoadWorkspaceAsyncWithEmptyFoldersArgumentExpectedReturnsEmptyCatalog()
        {
            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);

            Assert.AreEqual(0, rc.LoadWorkspaceViaBuilder("xx", false).Count);
        }


        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
                var currentResource = resource;
                var expected = resources.First(r => r.ResourceName == currentResource.ResourceName);
                Assert.AreEqual(expected.FilePath, resource.FilePath);
            }
        }


        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void LoadWorkspaceAsyncWithValidWorkspaceIDExpectedReturnsCatalogForWorkspace()
        {
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out List<IResource> resources);

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var result = rc.GetResources(workspaceID);

            Assert.AreEqual(SaveResourceCount, result.Count);

            foreach (var resource in result)
            {
                var currentResource = resource;
                var expected = resources.First(r => r.ResourceName == currentResource.ResourceName);
                Assert.AreEqual(expected.FilePath, resource.FilePath);
            }
        }


        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void GetResourceCount_ExpectedReturnsCount()
        {
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out List<IResource> resources);

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);

            Assert.AreEqual(SaveResourceCount, rc.GetResourceCount(workspaceID));
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void Reload_ExpectedReturnsCount()
        {
            var workspaceID = GlobalConstants.ServerWorkspaceID;
            SaveResources(workspaceID, out List<IResource> resources);

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.Reload();

            Assert.AreEqual(SaveResourceCount, rc.GetResourceCount(workspaceID));
        }


        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
                var currentResource = resource;
                var expected = resources.First(r => r.ResourceName == currentResource.ResourceName);
                Assert.AreEqual(expected.FilePath, resource.FilePath);
            }
        }


        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
                var currentResource = resource;
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void SaveResourceWithNullResourceArgumentExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.SaveResource(workspaceID, (IResource)null, "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void SaveResourceWithNullResourceXmlArgumentExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.SaveResource(workspaceID, (Resource)null, "");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceCatalog_SaveResource")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            xml = XElement.Load(Path.Combine(path, resource.ResourceName + ".bite"));
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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

            catch (Exception)

            { }
            var res = catalog.GetResourceContents(workspaceID, expected.ResourceID).ToString();
            Assert.IsFalse(res.Contains("federatedresource"));

        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceCatalog_SaveResource")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            xml = XElement.Load(Path.Combine(path, resource.ResourceName + ".bite"));
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            xml = XElement.Load(path + "\\" + resourceName + ".bite");
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ResourceCatalog_SaveResource")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void SaveResource_WithSameResourcePath_ExpectedNotDeleteOfExisting_ReasonDeploy()
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
            catalog.SaveResource(workspaceID, resource, resourcePath, GlobalConstants.SaveReasonForDeploy, "");
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            xml = XElement.Load(path + "\\" + resourceName + ".bite");
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            xml = XElement.Load(path + "\\CitiesDatabase" + ".bite");
            Assert.IsNotNull(xml);
            var idAttr = xml.Attributes("ID").ToList();
            Assert.AreEqual(1, idAttr.Count);
            var nameAttribute = xml.Attribute("Name");
            Assert.IsNotNull(nameAttribute);
            Assert.AreEqual(resourceName, nameAttribute.Value);
        }


        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void SaveResourceWithSourceWithoutIDExpectedSourceSavedWithID()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var path = workspacePath;

            var xml = XmlResource.Fetch("CitiesDatabase");
            var resource = new DbSource(xml);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.SaveResource(workspaceID, resource, "");

            xml = XElement.Load(Path.Combine(path, "CitiesDatabase" + ".bite"));
            var attr = xml.Attributes("ID").ToList();

            Assert.AreEqual(1, attr.Count);
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void SaveResourceWithExistingResourceAndReadonlyExpectedResourceOverwritten()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var resource1 = new DbSource { ResourceID = Guid.NewGuid(), ResourceName = "TestSource", DatabaseName = "TestOldDb", Server = "TestOldServer", ServerType = enSourceType.SqlDatabase };

            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.SaveResource(workspaceID, resource1, "");

            var path = Path.Combine(workspacePath, "TestSource" + ".bite");
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void GetResourceWithNullResourceNameExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.GetResource(workspaceID, null);
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void GetResourceWithResourceNameExpectedReturnsResource()
        {
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out List<IResource> resources);

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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var retrievedResource = catalog.GetResourceList<DbSource>(workspaceID);
            //------------Assert Results-------------------------
            Assert.IsNotNull(retrievedResource);
            Assert.AreEqual(1, retrievedResource.Count);


        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var retrievedResource = catalog.GetResourceList<DropBoxSource>(workspaceID);
            //------------Assert Results-------------------------
            Assert.IsNotNull(retrievedResource);
            Assert.AreEqual(0, retrievedResource.Count);


        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void LoadResourceActivityCache_GivenServerId_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {
                var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
                catalog.LoadServerActivityCache();

            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void GetResourceContentsWithNullResourceExpectedReturnsEmptyString()
        {
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var result = catalog.GetResourceContents(null);
            Assert.AreEqual(string.Empty, result.ToString());
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void GetResourceContentsWithNullResourceFilePathExpectedReturnsEmptyString()
        {
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var result = catalog.GetResourceContents(new Resource());
            Assert.AreEqual(string.Empty, result.ToString());
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void GetResourceContentsWithExistingResourceExpectedReturnsResourceContents()
        {
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out List<IResource> resources);

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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void GetResourceContentsWithNonExistentResourceExpectedReturnsEmptyString()
        {
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out List<IResource> resources);

            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var actual = catalog.GetResourceContents(new Resource { ResourceID = Guid.NewGuid(), FilePath = Path.GetRandomFileName() });
            Assert.AreEqual(string.Empty, actual.ToString());
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void GetResourceContentsWithNonExistentResourceIDExpectedReturnsEmptyString()
        {
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out List<IResource> resources);

            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var actual = catalog.GetResourceContents(workspaceID, Guid.NewGuid());
            Assert.AreEqual(string.Empty, actual.ToString());
        }

        #endregion

        #region SyncTo

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void SyncToWithDeleteIsFalseAndFileDeletedFromSourceExpectedFileNotDeletedInDestination()
        {
            var sourceWorkspaceID = Guid.NewGuid();
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, out List<IResource> sourceResources);

            var targetWorkspaceID = Guid.NewGuid();
            var targetWorkspacePath = SaveResources(targetWorkspaceID, out List<IResource> targetResources);

            var sourceResource = sourceResources[0];
            var targetResource = targetResources.First(r => r.ResourceID == sourceResource.ResourceID);

            var targetFile = new FileInfo(targetResource.FilePath);


            new ResourceCatalog().SyncTo(sourceWorkspacePath, targetWorkspacePath, true, false);
            targetFile.Refresh();
            Assert.IsTrue(targetFile.Exists);
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void SyncToWithOverwriteIsTrueExpectedFileInDestinationOverwritten()
        {
            var sourceWorkspaceID = Guid.NewGuid();
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, out List<IResource> sourceResources);

            var targetWorkspaceID = Guid.NewGuid();
            var targetWorkspacePath = SaveResources(targetWorkspaceID, out List<IResource> targetResources);

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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void SyncToWithOverwriteIsFalseExpectedFileInDestinationUnchanged()
        {
            var sourceWorkspaceID = Guid.NewGuid();
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, out List<IResource> sourceResources, true);

            var targetWorkspaceID = Guid.NewGuid();
            var targetWorkspacePath = SaveResources(targetWorkspaceID, out List<IResource> targetResources, true);

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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void SyncToWithFilesToIgnoreSpecifiedExpectedIgnoredFilesAreNotCopied()
        {
            var sourceWorkspaceID = Guid.NewGuid();
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, out List<IResource> sourceResources);

            var targetWorkspaceID = Guid.NewGuid();
            var targetWorkspacePath = SaveResources(targetWorkspaceID, out List<IResource> targetResources);

            var sourceResource = sourceResources[0];
            var targetResource = targetResources.First(r => r.ResourceID == sourceResource.ResourceID);
            var targetFile = new FileInfo(targetResource.FilePath);

            targetFile.Delete();
            new ResourceCatalog().SyncTo(sourceWorkspacePath, targetWorkspacePath, false, false, new List<string> { targetFile.Name });
            targetFile.Refresh();
            Assert.IsFalse(targetFile.Exists);
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void SyncToWithFilesToIgnoreSpecifiedExpectedIgnoredFilesAreNotDeleted()
        {
            var sourceWorkspaceID = Guid.NewGuid();
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, out List<IResource> sourceResources);

            var targetWorkspaceID = Guid.NewGuid();
            var targetWorkspacePath = SaveResources(targetWorkspaceID, out List<IResource> targetResources);

            var sourceResource = sourceResources[0];
            var targetResource = targetResources.First(r => r.ResourceID == sourceResource.ResourceID);

            var targetFile = new FileInfo(targetResource.FilePath);


            new ResourceCatalog().SyncTo(sourceWorkspacePath, targetWorkspacePath, false, false, new List<string> { targetFile.Name });
            targetFile.Refresh();
            Assert.IsTrue(targetFile.Exists);
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void SyncToWithNonExistingDestinationDirectoryExpectedDestinationDirectoryCreated()
        {
            var sourceWorkspaceID = Guid.NewGuid();
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, out List<IResource> sourceResources);

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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void ResourceCatalog_ToPayload_GetServiceNormalPayload_ConnectionStringAsAttributeOfRootTag()
        {
            //------------Setup for test--------------------------

            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var saveResources = SaveResources(sourcesPath, null, false, false, new[] { "ServerConnection1", "ServerConnection2" }, new[] { Guid.NewGuid(), Guid.NewGuid() }).ToList();

            //------------Execute Test---------------------------

            var payload = catalog.ToPayload(saveResources[0]);

            //------------Assert Results-------------------------

            Assert.IsTrue(payload.ToString().StartsWith("<Source"));
            var payloadElement = XElement.Parse(payload.ToString());
            var connectionStringattribute = payloadElement.AttributeSafe("ConnectionString");
            Assert.IsFalse(string.IsNullOrEmpty(connectionStringattribute));
        }

        #endregion

        #region ResourceCatalogResultBuilder

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void DeleteResourceWithNullResourceNameExpectedThrowsInvalidDataContractException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.DeleteResource(workspaceID, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataContractException))]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void DeleteResourceWithNullTypeExpectedThrowsInvalidDataContractException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.DeleteResource(workspaceID, "xxx", null);
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void DeleteResourceWithWildcardResourceNameExpectedReturnsNoWildcardsAllowed()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var result = catalog.DeleteResource(workspaceID, "*", "WorkflowService");
            Assert.AreEqual(ExecStatus.NoWildcardsAllowed, result.Status);
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void DeleteResourceWithNonExistingResourceNameExpectedReturnsNoMatch()
        {
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out List<IResource> resources);

            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var result = catalog.DeleteResource(workspaceID, "xxx", "WorkflowService");
            Assert.AreEqual(ExecStatus.NoMatch, result.Status);
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void GetDynamicObjectsWithNullResourceNameExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.GetDynamicObjects<DynamicServiceObjectBase>(workspaceID, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void GetDynamicObjectsWithNullResourceNameAndContainsTrueExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.GetDynamicObjects<DynamicServiceObjectBase>(workspaceID, null, true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void GetDynamicObjectsWithNullResourceExpectedThrowsArgumentNullException()
        {
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.GetDynamicObjects((IResource)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void GetDynamicObjectsWithNullResourcesExpectedThrowsArgumentNullException()
        {
            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalog.GetDynamicObjects((IEnumerable<IResource>)null);
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void GetDynamicObjectsWithResourceNameExpectedReturnsObjectGraph()
        {
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out List<IResource> resources);

            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);

            var expected = resources.First(r => r.ResourceType == "WorkflowService");

            var graph = catalog.GetDynamicObjects<DynamicService>(workspaceID, expected.ResourceName, true);

            VerifyObjectGraph(new List<IResource> { expected }, graph);
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void GetDynamicObjectsWithResourceExpectedReturnsObjectGraph()
        {
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out List<IResource> resources);

            var catalog = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);

            var expected = resources.First(r => r.ResourceType == "WorkflowService");

            var graph = catalog.GetDynamicObjects(expected);

            VerifyObjectGraph(new List<IResource> { expected }, graph);
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void GetDynamicObjectsWithResourcesExpectedReturnsObjectGraphs()
        {
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out List<IResource> resources);

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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceID.ToString() == resourceID);
            //------------Assert Precondition--------------------
            Assert.AreEqual(1, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            var resourceCatalogResult = rc.RenameResource(workspaceID, Guid.Parse(resourceID), "TestName", "TestName");
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual("Renamed Resource '" + resourceID + "' to 'TestName'", resourceCatalogResult.Message);
            var resourceContents = rc.GetResourceContents(workspaceID, oldResource.ResourceID).ToString();
            var xElement = XElement.Load(new StringReader(resourceContents), LoadOptions.None);
            var element = xElement.Attribute("Name");
            Assert.IsNotNull(element);
            Assert.AreEqual("TestName", element.Value);
            serverVersionRepository.Verify(a => a.StoreVersion(It.IsAny<IResource>(), "unknown", "Rename", workspaceID, "TestName"));
            var actionElem = xElement.Element("Action");
            Assert.IsNotNull(actionElem);
            var xamlElem = actionElem.Element("XamlDefinition");

            Assert.IsTrue(xamlElem.Value.Contains("DisplayName=\"TestName\""));

        }

        [TestMethod]
        [Description("Needs valid arguments")]
        [Owner("Ashley Lewis")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceID.ToString() == resourceID);
            //------------Assert Precondition--------------------
            Assert.AreEqual(1, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            var resourceCatalogResult = rc.RenameResource(workspaceID, oldResource.ResourceID, null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual("<CompilerMessage>Updated Resource '50fef451-b41e-4bdf-92a1-4a41e254cde2' renamed to ''</CompilerMessage>", resourceCatalogResult.Message);
            var resourceContents = rc.GetResourceContents(workspaceID, oldResource.ResourceID).ToString();
            var xElement = XElement.Load(new StringReader(resourceContents), LoadOptions.None);
            var element = xElement.Element("Name");
            Assert.IsNotNull(element);
            Assert.AreEqual("Bug6619Dep", element.Value);
        }

        [TestMethod]
        [Description("Needs valid arguments")]
        [Owner("Ashley Lewis")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceID.ToString() == resourceID);
            //------------Assert Precondition--------------------
            Assert.AreEqual(1, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            var resourceCatalogResult = rc.RenameResource(workspaceID, oldResource.ResourceID, "", "");
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual("<CompilerMessage>Updated Resource '50fef451-b41e-4bdf-92a1-4a41e254cde2' renamed to ''</CompilerMessage>", resourceCatalogResult.Message);
            var resourceContents = rc.GetResourceContents(workspaceID, oldResource.ResourceID).ToString();
            var xElement = XElement.Load(new StringReader(resourceContents), LoadOptions.None);
            var element = xElement.Element("Name");
            Assert.IsNotNull(element);
            Assert.AreEqual("Bug6619Dep", element.Value);
            var elementCat = xElement.Element("Category");
            Assert.IsNotNull(elementCat);
            Assert.AreEqual("TestCategory\\Bug6619Dep", element.Value);
        }

        [TestMethod]
        [Description("Updates the Category of the resource")]
        [Owner("Huggs")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            var resourceCatalogResult = rc.RenameCategory(workspaceID, "Bugs", "TestCategory");
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual("<CompilerMessage>Updated Category from 'Bugs' to 'TestCategory'</CompilerMessage>", resourceCatalogResult.Message);
            Assert.AreEqual("TestCategory\\Bug6619Dep", oldResource.GetResourcePath(workspaceID));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            var resourceCatalogResult = rc.RenameCategory(workspaceID, "SomeNonExistentCategory", "TestCategory");
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.NoMatch, resourceCatalogResult.Status);
            var resourceContents = rc.GetResourceContents(workspaceID, oldResource.ResourceID).ToString();
            var xElement = XElement.Load(new StringReader(resourceContents), LoadOptions.None);
            var element = xElement.Element("Category");
            Assert.IsNotNull(element);
            Assert.AreEqual("Bugs\\Bug6619Dep", element.Value);
        }

        [TestMethod]
        [Description("Updates the Category of the resource")]
        [Owner("Huggs")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            var resourceCatalogResult = rc.RenameCategory(workspaceID, "Bugs", "TestCategory");
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);

            Assert.AreEqual("TestCategory\\Bug6619Dep", oldResource.GetResourcePath(workspaceID));
        }

        [TestMethod]
        [Owner("Huggs")]
        [ExpectedException(typeof(InvalidDataContractException))]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.IsTrue(result.Count > 0, "Cannot save a resource to test with.");
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------
            rc.DeleteResource(workspaceID, Guid.NewGuid(), null);
            //------------Assert Results-------------------------            
        }

        [TestMethod]
        [Owner("Huggs")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            var resourceToUpdate = result.FirstOrDefault(resource => resource.ResourceName == "Bug6619");
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(r => r.ResourceName == resourceName);
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
            lock (SyncRoot)
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

        static IEnumerable<IResource> SaveResources(string resourcesPath, string versionNo, bool injectID, bool signXml, IEnumerable<string> resourceNames, Guid[] resourceIDs, bool createWorDef = false, bool renameWorkflow = false)
        {
            lock (SyncRoot)
            {
                var result = new List<IResource>();
                var count = 0;
                foreach (var resourceName in resourceNames)
                {
                    var xml = (XElement)null;
                    if (renameWorkflow)
                    {
                        xml = XmlResource.Fetch("Bug6619Dep"); //this will be used as template most of its values will be edited
                        
                        xml.Attribute("ID").Value = resourceIDs[count].ToString();
                        xml.Attribute("Name").Value = resourceName;
                        
                        xml.Element("DisplayName").Value = resourceName;
                        xml.Element("Category").Value = string.Empty;

                    }
                    else
                    {
                       xml = XmlResource.Fetch(resourceName);
                    }
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
                    if (renameWorkflow)
                    {
                        res.ResourceName = resourceName;
                    }
                    var resourceDirectory = resourcesPath + "\\";
                    res.FilePath = resourceDirectory + res.ResourceName + ".bite";
                    var f = new FileInfo(res.FilePath);
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
                                $"VersionControl\\{resourceName}.V{versionNo}.bite"),
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var beforeAction = beforeService.Actions.FirstOrDefault();
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var beforeAction = beforeService.Actions.FirstOrDefault();

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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
                var resource = expected;
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void ResourceCatalogPluginContainer_GivenVersion_ShouldCreateInstancesWithVersion()
        {
            //---------------Set up test pack-------------------

            var catalogPluginContainer = new ResourceCatalogPluginContainer(new Mock<IServerVersionRepository>().Object, new ConcurrentDictionary<Guid, List<IResource>>());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(catalogPluginContainer);
            //---------------Execute Test ----------------------
            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalogPluginContainer.Build(rc);
            //---------------Test Result -----------------------
            Assert.IsNotNull(catalogPluginContainer.SaveProvider);
            Assert.IsNotNull(catalogPluginContainer.SyncProvider);
            Assert.IsNotNull(catalogPluginContainer.RenameProvider);
            Assert.IsNotNull(catalogPluginContainer.DeleteProvider);
            Assert.IsNotNull(catalogPluginContainer.LoadProvider);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void ResourceCatalogPluginContainer_GivenVersionAndManagementServices_ShouldCreateInstances()
        {
            //---------------Set up test pack-------------------
            IEnumerable<DynamicService> services = new List<DynamicService>();
            var catalogPluginContainer = new ResourceCatalogPluginContainer(new Mock<IServerVersionRepository>().Object, new ConcurrentDictionary<Guid, List<IResource>>(), services);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(catalogPluginContainer);
            //---------------Execute Test ----------------------
            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            catalogPluginContainer.Build(rc);
            //---------------Test Result -----------------------
            Assert.IsNotNull(catalogPluginContainer.SaveProvider);
            Assert.IsNotNull(catalogPluginContainer.SyncProvider);
            Assert.IsNotNull(catalogPluginContainer.RenameProvider);
            Assert.IsNotNull(catalogPluginContainer.DeleteProvider);
            Assert.IsNotNull(catalogPluginContainer.LoadProvider);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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

            var _parsers = new ConcurrentDictionary<Guid, IResourceActivityCache>();
            var mock = new Mock<IResourceActivityCache>();

            _parsers.AddOrUpdate(workspaceID, mock.Object, (key, cache) =>
            {
                return cache;
            });
            const string propertyName = "_parsers";
            var fieldInfo = typeof(ResourceCatalog).GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Static);
            fieldInfo?.SetValue(rc, _parsers);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                rc.LoadServerActivityCache();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void LoadResourceActivityCache_GivenServiceName_ShouldAddActivityToParserCache()
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

            var _parsers = new ConcurrentDictionary<Guid, IResourceActivityCache>();

            const string propertyName = "_parsers";
            var fieldInfo = typeof(ResourceCatalog).GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Static);
            fieldInfo?.SetValue(rc, _parsers);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            rc.LoadServerActivityCache();
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void LoadResourceActivityCache_GivenServiceNameWithActivityInCache_ShouldReturnFromCache()
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

            var _parsers = new ConcurrentDictionary<Guid, IResourceActivityCache>();

            const string propertyName = "_parsers";
            var fieldInfo = typeof(ResourceCatalog).GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Static);
            fieldInfo?.SetValue(rc, _parsers);
            rc.LoadServerActivityCache();
            var actId = Guid.Parse("1736ca6e-b870-467f-8d25-262972d8c3e8");
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var act1 = rc.Parse(workspaceID, actId);
            //---------------Test Result -----------------------
            Assert.IsNotNull(act1);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void LoadResourceActivityCache_GivenServiceName_ShouldPopulateServiceActionRepo()
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

            var _parsers = new ConcurrentDictionary<Guid, IResourceActivityCache>();

            const string propertyName = "_parsers";
            var fieldInfo = typeof(ResourceCatalog).GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Static);
            fieldInfo?.SetValue(rc, _parsers);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            rc.LoadServerActivityCache();
            //---------------Test Result -----------------------
            var actId = Guid.Parse("1736ca6e-b870-467f-8d25-262972d8c3e8");
            var actId2 = Guid.Parse("ec636256-5f11-40ab-a044-10e731d87555");
            var ds1 = ServiceActionRepo.Instance.ReadCache(actId);
            var ds2 = ServiceActionRepo.Instance.ReadCache(actId2);
            Assert.IsNotNull(ds1);
            Assert.IsNotNull(ds2);
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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

            var _parsers = new ConcurrentDictionary<Guid, IResourceActivityCache>();

            const string propertyName = "_parsers";
            var fieldInfo = typeof(ResourceCatalog).GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Static);
            fieldInfo?.SetValue(rc, _parsers);
            rc.LoadServerActivityCache();
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
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
            var dupResource = result.FirstOrDefault(resource => resource.ResourceName == destinationPath);
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
        [Timeout(60000)]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void ResourceCatalog_DuplicateFolder_ResourceWithValidArgs_And_FixReferences_False_ExpectSuccess()
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
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
        [DoNotParallelize]
        [Timeout(60000 * 14)]
        [Owner("Siphamandla Dube")]
        [TestCategory("ResourceCatalog_LoadTests")]
        public void ResourceCatalog_DuplicateFolder_ResourceWithValidArgs_And_FixReferences_False_ExpectSuccesResult_LoadTest()
        {
            //Note: this intergration test proves the timeout issue caused by the multiple calls to the method
            //Note: at this point the time is reduced to a little less then 14 minutes from the initial 25 minutes
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var sourceLocation = "Duplicate_Source";
            var path = EnvironmentVariables.ResourcePath + "\\" + sourceLocation;
            Directory.CreateDirectory(path);
            
            SaveResources(path, null, true, false, _testSourceWFs, _resourceIds.ToArray(), true, true);

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var resultBeforeDuplicateF = rc.GetResources(workspaceID);
            var oldResource1 = resultBeforeDuplicateF.FirstOrDefault(resource => resource.ResourceName == _resourceName + (1 + 1));
            //------------Assert Precondition-----------------
            Assert.AreEqual(_numOfTestWFs, resultBeforeDuplicateF.Count, "Number of test workflows should equal to GetResources result to prove that the WF ids are all unique - BEFORE DuplicateFolder");
            Assert.IsNotNull(oldResource1);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.DuplicateFolder(sourceLocation, "Duplicate_Destination", string.Empty, false);

            var resultAfterDuplicateF = rc.GetResources(workspaceID);
            var oldResource = resultAfterDuplicateF.FirstOrDefault(resource => resource.ResourceName == _resourceName + (1 + 1));
            //------------Assert Precondition-----------------
            Assert.AreEqual(_numOfTestWFs * 2, resultAfterDuplicateF.Count, "Number of test workflows should equal to 2 times the GetResources result to prove that the WF ids are all unique - AFTER DuplicateFolder");
            //------------Assert Results-------------------------
            //TODO: These should be equal after the Refactor of DuplicateFolder method
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual(@"Duplicated Successfully".Replace(Environment.NewLine, ""), resourceCatalogResult.Message.Replace(Environment.NewLine, ""));

            //TODO: this will be changed into a unit test and this exception tested in a unit test setup
            Assert.AreNotEqual("Duplicated UnsuccessfullyFailure Fixing references", resourceCatalogResult.Message.Replace(Environment.NewLine, ""));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [Timeout(60000 * 16)]
        [DoNotParallelize]
        [TestCategory("ResourceCatalog_LoadTests")]
        public void ResourceCatalog_DuplicateFolder_ResourceWithValidArgs_And_FixReferences_True_ExpectSuccesResult_LoadTest()
        {
            //Note: this intergration test proves the timeout issue caused by the multiple calls to the method
            //Note: at this point the time is reduced to a little less then 16 minutes from the initial 25 minutes
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var sourceLocation = "Duplicate_Source";
            var path = EnvironmentVariables.ResourcePath + "\\"+sourceLocation;
            Directory.CreateDirectory(path);

            SaveResources(path, null, true, false, _testSourceWFs, _resourceIds.ToArray(), true, true);

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var resultBeforeDuplicateF = rc.GetResources(workspaceID);
            var oldResource1 = resultBeforeDuplicateF.FirstOrDefault(resource => resource.ResourceName == _resourceName + (1+1));
            //------------Assert Precondition-----------------
            Assert.AreEqual(_numOfTestWFs, resultBeforeDuplicateF.Count, "Number of test workflows should equal to GetResources result to prove that the WF ids are all unique - BEFORE DuplicateFolder");
            Assert.IsNotNull(oldResource1);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.DuplicateFolder(sourceLocation, "Duplicate_Destination", string.Empty, true);
            
            var resultAfterDuplicateF = rc.GetResources(workspaceID);
            var oldResource = resultAfterDuplicateF.FirstOrDefault(resource => resource.ResourceName == _resourceName + (1 + 1));
            //------------Assert Precondition-----------------
            Assert.AreEqual(_numOfTestWFs * 2, resultAfterDuplicateF.Count, "Number of test workflows should equal to 2 times the GetResources result to prove that the WF ids are all unique - AFTER DuplicateFolder");
            //------------Assert Results-------------------------
            //TODO: These should be equal after the Refactor of DuplicateFolder method
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual(@"Duplicated Successfully".Replace(Environment.NewLine, ""), resourceCatalogResult.Message.Replace(Environment.NewLine, ""));
            
            //TODO: this will be changed into a unit test and this exception tested in a unit test setup
            Assert.AreNotEqual("Duplicated UnsuccessfullyFailure Fixing references", resourceCatalogResult.Message.Replace(Environment.NewLine, ""));
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceCatalog))]
        public void ResourceCatalog_DuplicateFolder_GivenInnerFolder_ResourceWithValidArgs_And_FixReferences_True_ExpectSuccesResult()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var sourceLocation = "Duplicate_Source";
            var path = EnvironmentVariables.ResourcePath + "\\" + sourceLocation;
            Directory.CreateDirectory(path);
            const string resourceName = "wolf-Test_WF_";
            const string resourceName2 = "wolf-Test_Inner_WF_";

            const int numOfTestWFs = 2;
            const int numOfTestWFs2 = 2; 
            SaveTestResources(path, resourceName, out List<string> workflows, out List<Guid> resourceIds, numOfTestWFs);
            
            SaveTestResources(path + "\\Duplicate_Source_Inner", resourceName2, out List<string> workflows2, out List<Guid> resourceIds2, numOfTestWFs2);

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspace(workspaceID);
            var resultBeforeDuplicateF = rc.GetResources(workspaceID);
            var oldResource1 = resultBeforeDuplicateF.FirstOrDefault(resource => resource.ResourceName == resourceName + (1 + 1));
            //------------Assert Precondition-----------------
            var numOfWfs = numOfTestWFs + numOfTestWFs;
            Assert.AreEqual(numOfWfs, resultBeforeDuplicateF.Count, "Number of test workflows should equal to GetResources result to prove that the WF ids are all unique - BEFORE DuplicateFolder");
            Assert.IsNotNull(oldResource1);
            //------------Execute Test---------------------------
            ResourceCatalogResult resourceCatalogResult = rc.DuplicateFolder(sourceLocation, "Duplicate_Destination", string.Empty, true);

            var resultAfterDuplicateF = rc.GetResources(workspaceID);
            var oldResource = resultAfterDuplicateF.FirstOrDefault(resource => resource.ResourceName == resourceName + (1 + 1));
            //------------Assert Precondition-----------------
            Assert.AreEqual(numOfWfs * 2, resultAfterDuplicateF.Count, "Number of test workflows should equal to 8 on GetResources result to prove that the WF ids are all unique - AFTER DuplicateFolder");
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Success, resourceCatalogResult.Status);
            Assert.AreEqual(@"Duplicated Successfully".Replace(Environment.NewLine, ""), resourceCatalogResult.Message.Replace(Environment.NewLine, ""));
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceCatalog))]
        public void ResourceCatalog_Parse_GivenNonExistantResource_ShouldReturnNull()
        {
            //------------Setup for test--------------------------
            var resource = new Resource 
            { 
                ResourceName = "test_resource", 
                ResourceID = Guid.Empty 
            };

            var sut = new ResourceCatalog
            {
                ResourceActivityCache = new Mock<IResourceActivityCache>().Object
            };
            //------------Assert Precondition-----------------
            //------------Execute Test---------------------------
            var result = sut.Parse(GlobalConstants.ServerWorkspaceID, resource);
            //------------Assert Precondition-----------------
            //------------Assert Results-------------------------
            Assert.IsNull(result);
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceCatalog))]
        public void ResourceCatalog_Parse_GivenHasActivityInCache_ShouldReturnFromExistingActivityCacheEntry()
        {
            //Note: there seems to be a race condition with: ResourceCatalog_Parse_GivenHasActivityInCache_And_GetActivityFails_ShouldReturnNull
            //------------Setup for test--------------------------
            var resource = new Resource
            {
                ResourceName = "test_resource",
                ResourceID = Guid.Empty
            };

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockActivityParser = new Mock<IActivityParser>();
            var mockResourceActivityCache = new Mock<IResourceActivityCache>();
            var mockDev2Activity = new Mock<IDev2Activity>();
            mockResourceActivityCache.Setup(o => o.HasActivityInCache(resource.ResourceID))
                .Returns(true);
            mockResourceActivityCache.Setup(o => o.GetActivity(resource.ResourceID))
                .Returns(mockDev2Activity.Object);

            var sut = new ResourceCatalog
            {
                ResourceActivityCache = mockResourceActivityCache.Object
            };
            //------------Assert Precondition-----------------
            //------------Execute Test---------------------------
            var result = sut.Parse(GlobalConstants.ServerWorkspaceID, resource.ResourceID);
            //------------Assert Precondition-----------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.AreEqual(mockDev2Activity.Object, result);
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory("ResourceCatalog_Intergation")]
        public void ResourceCatalog_Parse_GivenHasActivityInCache_ShouldReturnExistingActivityCacheEntry_Intergation()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var sourceLocation = "ResourceCatalogParseMethodTests";
            var path = EnvironmentVariables.ResourcePath + "\\" + sourceLocation;
            Directory.CreateDirectory(path);
            const string resourceName = "wolf-Test_WF_";

            const int numOfTestWFs = 2;
            SaveTestResources(path, resourceName, out List<string> workflows, out List<Guid> resourceIds, numOfTestWFs);

            var resource = new Resource
            {
                ResourceName = resourceName,
                ResourceID = resourceIds.First()
            };

            var sut = ResourceCatalog.Instance;
            //------------Assert Precondition-----------------
            //------------Execute Test---------------------------
            var result = sut.Parse(workspaceID, resource.ResourceID);
            //------------Assert Precondition-----------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.AreEqual("Bugs\\Bug6619Dep2", result.GetDisplayName());
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceCatalog))]
        public void ResourceCatalog_Parse_GivenHasActivityInCache_And_GetActivityFails_ShouldReturnNull()
        {
            //Note: there seems to be a race condition with: ResourceCatalog_Parse_GivenHasActivityInCache_ShouldReturnFromExistingActivityCacheEntry
            //------------Setup for test--------------------------
            var resource = new Resource
            {
                ResourceName = "test_resource",
                ResourceID = Guid.Empty
            };

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockActivityParser = new Mock<IActivityParser>();
            var mockResourceActivityCache = new Mock<IResourceActivityCache>();
            var mockDev2Activity = new Mock<IDev2Activity>();
            mockResourceActivityCache.Setup(o => o.HasActivityInCache(resource.ResourceID))
                .Returns(true);
            mockResourceActivityCache.Setup(o => o.GetActivity(resource.ResourceID));

            var sut = new ResourceCatalog
            {
                ResourceActivityCache = mockResourceActivityCache.Object
            };
            //------------Assert Precondition-----------------
            //------------Execute Test---------------------------
            var result = sut.Parse(GlobalConstants.ServerWorkspaceID, Guid.Empty);
            //------------Assert Precondition-----------------
            //------------Assert Results-------------------------
            Assert.IsNull(result);
        }

        [TestMethod]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResourceCatalog))]
        public void ResourceCatalog_Parse_GivenHasActivityInCache_And_GetServiceFails_ShouldReturnNull()
        {
            //------------Setup for test--------------------------
            var resource = new Resource
            {
                ResourceName = "test_resource",
                ResourceID = Guid.Empty
            };

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockActivityParser = new Mock<IActivityParser>();
            var mockResourceActivityCache = new Mock<IResourceActivityCache>();
            var mockDev2Activity = new Mock<IDev2Activity>();
            mockResourceActivityCache.Setup(o => o.HasActivityInCache(resource.ResourceID))
                .Returns(true);
            mockResourceActivityCache.Setup(o => o.GetActivity(resource.ResourceID));

            var sut = new ResourceCatalog
            {
                ResourceActivityCache = mockResourceActivityCache.Object
            };
            //------------Assert Precondition-----------------
            //------------Execute Test---------------------------
            var result = sut.Parse(GlobalConstants.ServerWorkspaceID, resource);
            //------------Assert Precondition-----------------
            //------------Assert Results-------------------------
            Assert.IsNull(result);
        }

        public static IEnumerable<IResource> SaveTestResources(string path, string resourceName, out List<string> workflows, out List<Guid> resourceIds, int numOfTestWFs)
        {
            workflows = new List<string>();
            resourceIds = new List<Guid>();
            for (int i = 0; i < numOfTestWFs; i++)
            {
                workflows.Add(resourceName + (i + 1).ToString());
                resourceIds.Add(Guid.NewGuid());
            }

            return SaveResources(path, null, true, false, workflows, resourceIds.ToArray(), true, true);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void ResourceCatalog_UnitTest_GetResourceOfTNotExist_ExpectNull()
        {
            //------------Setup for test--------------------------
            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);

            //------------Execute Test---------------------------
            var resourceList = rc.GetResource<PluginSource>(Guid.NewGuid(), Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.IsNull(resourceList);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
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
            var oldResource = result.FirstOrDefault(resource => resource.ResourceName == resourceName);
            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(oldResource);
            //------------Execute Test---------------------------

            rc.DuplicateFolder("", oldResource.GetResourcePath(workspaceID), "Null", true);
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void ResourceCatalog_UnitTest_GetXmlResource_UpdatesResource_To_Bite()
        {
            //------------Setup for test--------------------------
            var workspaceID = GlobalConstants.ServerWorkspaceID;

            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "testingExtension";
            var resourceId = Guid.Parse("aff19795-fafc-43bb-b6a9-c7c88b3cd93c");
            SaveResources(path, null, false, false, new[] { resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });
            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var resourceCount = rc.GetResourceCount(workspaceID);
            var resource = rc.GetResource(workspaceID, resourceId);

            //------------Assert Precondition-----------------            
            //------------Execute Test--------------------------
            //------------Assert Results------------------------
            Assert.AreEqual(1, resourceCount);
            Assert.IsTrue(resource.FilePath.EndsWith(".bite"));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void ResourceCatalog_UnitTest_CopyMissingResources()
        {
            //------------Setup for test--------------------------
            var rcBuilder = new ResourceCatalogBuilder();
            var privateObject = new Warewolf.Testing.PrivateObject(rcBuilder);
            var fileHelperObject = new Mock<IFile>();
            var serverReleaseResources = Path.Combine(EnvironmentVariables.ApplicationPath, "Resources");
            fileHelperObject.Setup(o => o.Copy(serverReleaseResources +"\\asdf\\asdf2.xml",
                                               EnvironmentVariables.ResourcePath + "\\asdf\\asdf2.bite", false)).Verifiable();
            fileHelperObject.Setup(o => o.DirectoryName(EnvironmentVariables.ResourcePath + "\\asdf\\asdf2.bite")).Returns(EnvironmentVariables.ResourcePath + "\\asdf").Verifiable();
            var fileHelper = fileHelperObject.Object;
            var mockDirectory = new Mock<IDirectory>();
            mockDirectory.Setup(o => o.CreateIfNotExists("C:\\ProgramData\\Warewolf\\Resources\\asdf")).Verifiable();
            var existingId = Guid.NewGuid().ToString();
            var programDataIds = new[] {
                existingId
            };

            ResourceBuilderTO newResourceBuilderTO(string filename, string id)
            {
                return new ResourceBuilderTO
                {
                    _filePath = serverReleaseResources + "\\" + filename,
                    _fileStream = new MemoryStream(Encoding.ASCII.GetBytes($"<node ID=\"{id}\"></node>"))
                };
            }

            var programFilesBuilders = new List<ResourceBuilderTO>
            {
                newResourceBuilderTO("asdf\\asdf.xml", existingId)
            };

            //------------Execute Test--------------------------
            var result = privateObject.Invoke("CopyMissingResources", programDataIds, programFilesBuilders, mockDirectory.Object, fileHelper);
            //------------Assert Results------------------------
            Assert.IsNotNull(result);
            var hadMissing = (bool)result;
            Assert.IsFalse(hadMissing);


            //------------Execute Test--------------------------
            programFilesBuilders = new List<ResourceBuilderTO> {
                newResourceBuilderTO("asdf\\asdf.xml", existingId),
                newResourceBuilderTO("asdf\\asdf2.xml", Guid.NewGuid().ToString())
            };

            result = privateObject.Invoke("CopyMissingResources", programDataIds, programFilesBuilders, mockDirectory.Object, fileHelper);

            //------------Assert Results------------------------
            Assert.IsNotNull(result);
            hadMissing = (bool)result;
            Assert.IsTrue(hadMissing);

            fileHelperObject.Verify();
            mockDirectory.Verify();
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void ResourceCatalog_UnitTest_IsWarewolfResource_Given_NonWarewolf_Resource_Retunrs_False()
        {
            //------------Setup for test--------------------------
            var rcBuilder = new ResourceCatalogBuilder();
            var privateObject = new Warewolf.Testing.PrivateObject(rcBuilder);
            var xml = XmlResource.Fetch("fileThatsNotWarewolfResource");
            var results = privateObject.Invoke("IsWarewolfResource", xml);
            //------------Assert Precondition-----------------            
            Assert.IsNotNull(results);
            //------------Execute Test--------------------------
            //------------Assert Results------------------------
            Assert.IsFalse((bool)results);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void ResourceCatalog_UnitTest_Save_Resource_Saves_In_Bite_Extension()
        {
            //------------Setup for test--------------------------
            var rcBuilder = new ResourceCatalogBuilder();
            var privateObject = new Warewolf.Testing.PrivateObject(rcBuilder);
            var workspaceID = GlobalConstants.ServerWorkspaceID;
            var path = EnvironmentVariables.ResourcePath;
            Directory.CreateDirectory(path);
            const string resourceName = "WarewolfResourceFile";
            var resourceSaved = SaveResources(path, null, false, false, new[] { resourceName }, new[] { Guid.NewGuid(), Guid.NewGuid() });
            var paths = new List<string>() { resourceSaved.First().FilePath };
            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            var result = rc.GetResources(workspaceID);
            //------------Assert Precondition-----------------
            var expectedFile = resourceSaved.First().FilePath;
            Assert.IsTrue(File.Exists(expectedFile));
            File.Delete(expectedFile);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void ResourceCatalog_UnitTest_UpdateExtensions_Given_WW_Resource_Updates_The_Extension()
        {
            //------------Setup for test--------------------------
            var sourcesPath = EnvironmentVariables.GetWorkspacePath(GlobalConstants.ServerWorkspaceID);
            var rcBuilder = new ResourceCatalogBuilder();
            var privateObject = new Warewolf.Testing.PrivateObject(rcBuilder);
            var xml = XmlResource.Fetch("fileThatsNotWarewolfResource");
            var filePath = sourcesPath + "\\" + "fileThatsNotWarewolfResource.xml";
            var filePathToUpdate = new List<string> { sourcesPath + "\\" + "fileThatsNotWarewolfResource.xml" };
            File.WriteAllText(filePath, xml.ToString());
            privateObject.Invoke("UpdateExtensions", filePathToUpdate);
            //------------Assert Precondition-----------------
            var expectedFile = Path.ChangeExtension(filePath, ".bite");
            Assert.IsTrue(File.Exists(expectedFile));
            File.Delete(expectedFile);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceCatalog_Load")]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void ResourceCatalog_Load_WhenFileIsReadOnly_ShouldUpdateToNormal()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            SaveResources(sourcesPath, null, false, false, new[] { "EmailSource" }, new[] { Guid.NewGuid() });
            var allFiles = Directory.GetFiles(sourcesPath);
            File.SetAttributes(allFiles[0], FileAttributes.ReadOnly);

            var attributes = File.GetAttributes(allFiles[0]);
            Assert.AreEqual(FileAttributes.ReadOnly, attributes);

            var rc = new ResourceCatalog(null, new Mock<IServerVersionRepository>().Object);
            rc.LoadWorkspaceViaBuilder(workspacePath, false, "Sources", "Services");

            attributes = File.GetAttributes(allFiles[0]);
            Assert.AreNotEqual(FileAttributes.ReadOnly, attributes);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void ResourceCatalog_TryBuildCatalogFromWorkspace_WithNullFolders_ThrowsException()
        {
            //------------Setup for test--------------------------
            var rc = new ResourceCatalogBuilder(ResourceUpgraderFactory.GetUpgrader(), new DirectoryWrapper(), new FileWrapper());
            rc.TryBuildCatalogFromWorkspace("some value", null);
        }

        [TestMethod]
        [ExpectedException(typeof(DirectoryNotFoundException))]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void ResourceCatalog_BuildReleaseExamples_CannotFindReleaseExamples()
        {
            //------------Setup for test--------------------------
            var rc = new ResourceCatalogBuilder(ResourceUpgraderFactory.GetUpgrader(), new DirectoryWrapper(), new FileWrapper());
            rc.BuildReleaseExamples("some value");
        }

        //TODO: move this test to ResourceCatalogBuilderTests
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("CannotParallelize")]
        public void ResourceCatalogBuilder_BuildReleaseExamples_CreateMissingDestinationDirectory()
        {
            var resourcePath = EnvironmentVariables.ResourcePath;

            var mockDirectory = new Mock<IDirectory>();
            mockDirectory.Setup(o => o.Exists(resourcePath))
                .Returns(false);
            mockDirectory.Setup(o => o.EnumerateDirectories(resourcePath, "*", SearchOption.AllDirectories))
                .Returns(new List<string> { "item_one", "item_two" });

            //------------Setup for test--------------------------
            var rc = new ResourceCatalogBuilder(ResourceUpgraderFactory.GetUpgrader(), mockDirectory.Object, new FileWrapper());
            //------------Assert Precondition-------------------
            //------------Execute Test--------------------------
            rc.BuildReleaseExamples("release");
            //------------Assert Results------------------------
            mockDirectory.Verify(o => o.Exists(@"C:\ProgramData\Warewolf\Resources"), Times.Once);
            mockDirectory.Verify(o => o.CreateDirectory(@"C:\ProgramData\Warewolf\Resources"), Times.Once);
            mockDirectory.Verify(o => o.EnumerateDirectories(@"C:\ProgramData\Warewolf\Resources", "*", SearchOption.AllDirectories), Times.Once);
            mockDirectory.Verify(o => o.EnumerateDirectories("release", "*", SearchOption.AllDirectories), Times.Once);
           
        }

        class ResourceSaveProviderMock : ResourceSaveProvider
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
