using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.ServiceModel;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Test.XML;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    public class ResourceCatalogTests
    {
        // Change this if you change the number of resouces saved by SaveResources()
        const int SaveResourceCount = 6;

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

        [TestMethod]
        public void ThisWithNewWorkspaceIDExpectedAddsCatalogForWorkspace()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            var expectedWorkspaceCount = catalog.WorkspaceCount + 1;

            var actualCount = catalog.GetResourceCount(workspaceID);
            Assert.AreEqual(SaveResourceCount, actualCount);
            Assert.AreEqual(expectedWorkspaceCount, catalog.WorkspaceCount);
        }

        [TestMethod]
        public void ThisWithExistingWorkspaceIDExpectedReturnsExistingCatalogForWorkspace()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            var expectedWorkspaceCount = catalog.WorkspaceCount + 1;

            catalog.LoadWorkspace(workspaceID); // Loads from disk
            var actualCount = catalog.GetResourceCount(workspaceID);
            Assert.AreEqual(SaveResourceCount, actualCount);
            Assert.AreEqual(expectedWorkspaceCount, catalog.WorkspaceCount);
        }

        #endregion

        #region LoadWorkspace

        [TestMethod]
        public void LoadWorkspaceWithNewWorkspaceIDExpectedAddsCatalogForWorkspace()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            var expectedWorkspaceCount = catalog.WorkspaceCount + 1;
            catalog.LoadWorkspace(workspaceID); // Loads from disk

            var actualCount = catalog.GetResourceCount(workspaceID);
            Assert.AreEqual(SaveResourceCount, actualCount);
            Assert.AreEqual(expectedWorkspaceCount, catalog.WorkspaceCount);
        }

        [TestMethod]
        public void LoadWorkspaceWithExistingWorkspaceIDExpectedUpdatesCatalogForWorkspace()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);
            var resource = resources[0];

            var catalog = new ResourceCatalog();
            var expectedWorkspaceCount = catalog.WorkspaceCount + 1;

            catalog.LoadWorkspace(workspaceID); // Loads from disk

            catalog.DeleteResource(workspaceID, resource.ResourceName, ResourceTypeConverter.ToTypeString(resource.ResourceType), resource.AuthorRoles);
            var actualCount = catalog.GetResourceCount(workspaceID);
            Assert.AreEqual(SaveResourceCount - 1, actualCount);
            Assert.AreEqual(expectedWorkspaceCount, catalog.WorkspaceCount);

            catalog.LoadWorkspace(workspaceID); // Loads from disk
            actualCount = catalog.GetResourceCount(workspaceID);
            Assert.AreEqual(SaveResourceCount - 1, actualCount);
            Assert.AreEqual(expectedWorkspaceCount, catalog.WorkspaceCount);
        }

        #endregion
        
        #region LoadWorkspaceAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoadWorkspaceAsyncWithNullWorkspaceArgumentExpectedThrowsArgumentNullException()
        {
            var rc = new ResourceCatalog();

            rc.LoadWorkspaceViaBuilder(null, new string[0]);
        }
        
        [TestMethod]
        public void LoadWorkspaceAsyncWithEmptyFoldersArgumentExpectedReturnsEmptyCatalog()
        {
            var rc = new ResourceCatalog();

            Assert.AreEqual(0, rc.LoadWorkspaceViaBuilder("xx", new string[0]).Count);
        }

        
        [TestMethod]
        public void LoadWorkspaceAsyncWithExistingSourcesPathAndNonExistingServicesPathExpectedReturnsCatalogForSources()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var sourcesPath = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(sourcesPath);
            var resources = SaveResources(sourcesPath, null, false, false, "CatalogSourceAnytingToXmlPlugin", "CatalogSourceCitiesDatabase", "CatalogSourceTestServer").ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            Assert.AreEqual(3, result.Count);

            foreach (var resource in result)
            {
                var expected = resources.First(r => r.ResourceName == resource.ResourceName);
                Assert.AreEqual(expected.FilePath, resource.FilePath);
            }
        }

        
        [TestMethod]
        public void LoadWorkspaceAsyncWithValidWorkspaceIDExpectedReturnsCatalogForWorkspace()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            var workspacePath = SaveResources(workspaceID, out resources);

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            Assert.AreEqual(SaveResourceCount,result.Count);

            foreach (var resource in result)
            {
                var expected = resources.First(r => r.ResourceName == resource.ResourceName);
                Assert.AreEqual(expected.FilePath, resource.FilePath);
            }
        }

        
        [TestMethod]
        public void LoadWorkspaceAsyncWithWithOneSignedAndOneUnsignedServiceExpectedLoadsSignedService()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resources = SaveResources(path, null, false, false, "Calculate_RecordSet_Subtract", "TestDecisionUnsigned").ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            Assert.AreEqual(1, result.Count);

            foreach (var resource in result)
            {
                var expected = resources.First(r => r.ResourceName == resource.ResourceName);
                Assert.AreEqual(expected.FilePath, resource.FilePath);
            }
        }

        
        [TestMethod]
        public void LoadWorkspaceAsyncWithSourceWithoutIDExpectedInjectsID()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var path = Path.Combine(workspacePath, "Sources");
            Directory.CreateDirectory(path);
            var resources = SaveResources(path, null, false, false, "CitiesDatabase").ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            Assert.AreEqual(1, result.Count);

            foreach (var resource in result)
            {
                var expected = resources.First(r => r.ResourceName == resource.ResourceName);
                Assert.AreNotEqual(expected.ResourceID, resource.ResourceID);
            }
        }

        
        [TestMethod]
        public void LoadWorkspaceAsyncWithUpgradableXmlExpectedUpgradesXmlWithoutLocking()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resources = SaveResources(path, null, false, false, "CatalogServiceAllTools").ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Sources", "Services");

            Assert.AreEqual(1, result.Count);

            var actual = result[0];
            Assert.IsTrue(actual.IsUpgraded);

            // TestCleanup will fail if file is locked! 
        }

        #endregion

        

        #region ParallelExecution

        [TestMethod]
        public void LoadInParallelExpectedBehavesConcurrently()
        {
            const int NumWorkspaces = 5;
            const int NumThreadsPerWorkspace = 5;
            const int ExpectedWorkspaceCount = NumWorkspaces + 1; // add 1 for server workspace that is auto-loaded

            var catalog = new ResourceCatalog();

            var threadArray = new Thread[NumWorkspaces * NumThreadsPerWorkspace];

            #region Create threads

            for (var i = 0; i < NumWorkspaces; i++)
            {
                var workspaceID = Guid.NewGuid();

                List<IResource> resources;
                SaveResources(workspaceID, out resources);

                for (var j = 0; j < NumThreadsPerWorkspace; j++)
                {
                    var t = (i * NumThreadsPerWorkspace) + j;
                    if (j == 0)
                    {
                        threadArray[t] = new Thread(() =>
                        {
                            catalog.LoadWorkspace(workspaceID); // Always loads from disk
                            var actualCount = catalog.GetResourceCount(workspaceID); // Loads from disk if not in memory
                            Assert.AreEqual(SaveResourceCount, actualCount);
                        });
                    }
                    else
                    {
                        threadArray[t] = new Thread(() =>
                        {
                            var actualCount = catalog.GetResourceCount(workspaceID); // Loads from disk if not in memory
                            Assert.AreEqual(SaveResourceCount, actualCount);
                        });
                    }
                }
            }

            #endregion


            //Start the threads.
            Parallel.For(0, threadArray.Length, i => threadArray[i].Start());

            //Wait until all the threads spawn out and finish.
            foreach (var t in threadArray)
            {
                t.Join();
            }

            Assert.AreEqual(ExpectedWorkspaceCount, catalog.WorkspaceCount);
        }

        #endregion

        #region SaveResource

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveResourceWithNullResourceArgumentExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, (IResource)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveResourceWithNullResourceXmlArgumentExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, (string)null);
        }

        [TestMethod]
        public void SaveResourceWithUnsignedServiceExpectedSignsFile()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var path = Path.Combine(workspacePath, "Services");

            var xml = XmlResource.Fetch("TestDecisionUnsigned");
            var resource = new Workflow(xml);
            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, resource);

            var signedXml = File.ReadAllText(Path.Combine(path, resource.ResourceName + ".xml"));

            var isValid = HostSecurityProvider.Instance.VerifyXml(signedXml);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void SaveResourceWithSourceWithoutIDExpectedSourceSavedWithID()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var path = Path.Combine(workspacePath, "Sources");

            var xml = XmlResource.Fetch("CitiesDatabase");
            var resource = new DbSource(xml);
            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, resource);

            xml = XElement.Load(Path.Combine(path, resource.ResourceName + ".xml"));
            var attr = xml.Attributes("ID").ToList();

            Assert.AreEqual(1, attr.Count);
        }

        [TestMethod]
        public void SaveResourceWithExistingResourceExpectedResourceOverwritten()
        {
            var workspaceID = Guid.NewGuid();

            var resource1 = new DbSource { ResourceID = Guid.NewGuid(), ResourceName = "TestSource", DatabaseName = "TestOldDb", Server = "TestOldServer", ServerType = enSourceType.SqlDatabase, Version = new Version(1, 0) };

            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, resource1);

            var expected = new DbSource { ResourceID = resource1.ResourceID, ResourceName = resource1.ResourceName, DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase, Version = new Version(1, 1) };
            catalog.SaveResource(workspaceID, expected);

            var contents = catalog.GetResourceContents(workspaceID, expected.ResourceID, expected.Version);
            var actual = new DbSource(XElement.Parse(contents));

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

            var resource1 = new DbSource { ResourceID = Guid.NewGuid(), ResourceName = "TestSource", DatabaseName = "TestOldDb", Server = "TestOldServer", ServerType = enSourceType.SqlDatabase, Version = new Version(1, 0) };

            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, resource1);

            var path = Path.Combine(workspacePath, "Sources", resource1.ResourceName + ".xml");
            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.ReadOnly) != FileAttributes.ReadOnly)
            {
                File.SetAttributes(path, attributes ^ FileAttributes.ReadOnly);
            }

            var expected = new DbSource { ResourceID = resource1.ResourceID, ResourceName = resource1.ResourceName, DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase, Version = new Version(1, 1) };
            catalog.SaveResource(workspaceID, expected);

            var contents = catalog.GetResourceContents(workspaceID, expected.ResourceID, expected.Version);
            var actual = new DbSource(XElement.Parse(contents));

            Assert.AreEqual(expected.ResourceName, actual.ResourceName);
            Assert.AreEqual(expected.DatabaseName, actual.DatabaseName);
            Assert.AreEqual(expected.Server, actual.Server);
            Assert.AreEqual(expected.ServerType, actual.ServerType);
        }

        [TestMethod]
        public void SaveResourceWithNewResourceExpectedResourceWritten()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();

            var expected = new DbSource { ResourceID = Guid.NewGuid(), ResourceName = "TestSource", DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase, Version = new Version(1, 1) };
            catalog.SaveResource(workspaceID, expected);

            var contents = catalog.GetResourceContents(workspaceID, expected.ResourceID, expected.Version);
            var actual = new DbSource(XElement.Parse(contents));

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
            var catalog = new ResourceCatalog();

            var expected = new DbSource { ResourceID = Guid.NewGuid(), ResourceName = "Test\\Source", DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase, Version = new Version(1, 1) };
            catalog.SaveResource(workspaceID, expected);
        }

        [TestMethod]
        public void SaveResourceWithNewResourceXmlExpectedResourceWritten()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();

            var expected = new DbSource { ResourceID = Guid.NewGuid(), ResourceName = "TestSource", DatabaseName = "TestNewDb", Server = "TestNewServer", ServerType = enSourceType.MySqlDatabase, Version = new Version(1, 1) };
            catalog.SaveResource(workspaceID, expected.ToXml().ToString());

            var contents = catalog.GetResourceContents(workspaceID, expected.ResourceID, expected.Version);
            var actual = new DbSource(XElement.Parse(contents));

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
            var catalog = new ResourceCatalog();
            catalog.GetResource(workspaceID, null);
        }

        [TestMethod]
        public void GetResourceWithResourceNameExpectedReturnsResource()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            foreach (var expected in resources)
            {
                var actual = catalog.GetResource(workspaceID, expected.ResourceName);
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.ResourceID, actual.ResourceID);
                Assert.AreEqual(expected.ResourceName, actual.ResourceName);
                Assert.AreEqual(expected.ResourcePath, actual.ResourcePath);
                Assert.AreEqual(expected.ResourcePath, actual.ResourcePath);
            }
        }

        #endregion

        #region GetResourceContents

        [TestMethod]
        public void GetResourceContentsWithNullResourceExpectedReturnsEmptyString()
        {
            var catalog = new ResourceCatalog();
            var result = catalog.GetResourceContents(null);
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void GetResourceContentsWithNullResourceFilePathExpectedReturnsEmptyString()
        {
            var catalog = new ResourceCatalog();
            var result = catalog.GetResourceContents(new Resource());
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void GetResourceContentsWithExistingResourceExpectedReturnsResourceContents()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            foreach (var expected in resources)
            {
                var actual = catalog.GetResourceContents(expected);
                Assert.IsNotNull(actual);
            }
        }

        [TestMethod]
        public void GetResourceContentsWithNonExistentResourceExpectedReturnsEmptyString()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            var actual = catalog.GetResourceContents(new Resource { ResourceID = Guid.NewGuid(), FilePath = Path.GetRandomFileName() });
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        public void GetResourceContentsWithExistingResourceIDExpectedReturnsResourceContents()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            foreach (var expected in resources)
            {
                var xml = catalog.GetResourceContents(workspaceID, expected.ResourceID, expected.Version);

                var actual = new Resource(XElement.Parse(xml));
                Assert.AreEqual(expected.ResourceID, actual.ResourceID);
                Assert.AreEqual(expected.ResourceName, actual.ResourceName);
            }
        }

        [TestMethod]
        public void GetResourceContentsWithNonExistentResourceIDExpectedReturnsEmptyString()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            var actual = catalog.GetResourceContents(workspaceID, Guid.NewGuid());
            Assert.AreEqual(string.Empty, actual);
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
            var sourceFile = new FileInfo(sourceResource.FilePath);
            var targetFile = new FileInfo(targetResource.FilePath);

            sourceFile.Delete();
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
            }, 0, 1);
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
            var sourceFile = new FileInfo(sourceResource.FilePath);
            var targetFile = new FileInfo(targetResource.FilePath);

            sourceFile.Delete();
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

        #region GetPayload

        [TestMethod]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void GetPayloadWithNullResourceNameExpectedThrowsInvalidDataContractException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            catalog.GetPayload(workspaceID, null, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void GetPayloadWithNullTypeExpectedThrowsInvalidDataContractException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            catalog.GetPayload(workspaceID, "xxx", null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetPayloadWithNullTypeExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            catalog.GetPayload(workspaceID, null, null);
        }

        [TestMethod]
        public void GetPayloadWithGuidsExpectedReturnsPayloadForGuids()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var expectedResources = resources.Where(r => r.ResourceType == ResourceType.WorkflowService).ToList();
            var guids = expectedResources.Select(r => r.ResourceID).ToList();

            var catalog = new ResourceCatalog();
            var payloadXml = catalog.GetPayload(workspaceID, string.Join(",", guids), ResourceTypeConverter.TypeWorkflowService);

            VerifyPayload(expectedResources, payloadXml);
        }

        [TestMethod]
        public void GetPayloadWithInvalidGuidsExpectedReturnsEmptyPayload()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var guids = new[] { Guid.NewGuid(), Guid.NewGuid() };

            var catalog = new ResourceCatalog();
            var payloadXml = catalog.GetPayload(workspaceID, string.Join(",", guids), ResourceTypeConverter.TypeWorkflowService);

            VerifyPayload(new List<IResource>(), payloadXml);
        }

        [TestMethod]
        public void GetPayloadWithExistingResourceNameExpectedReturnsPayloadForResources()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            foreach (var expected in resources)
            {
                var payloadXml = catalog.GetPayload(workspaceID, expected.ResourceName, ResourceTypeConverter.ToTypeString(expected.ResourceType), null);
                VerifyPayload(new List<IResource> { expected }, payloadXml);
            }
        }

        [TestMethod]
        public void GetPayloadWithExistingResourceNameAndContainsFalseExpectedReturnsPayloadForResources()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            foreach (var expected in resources)
            {
                var payloadXml = catalog.GetPayload(workspaceID, expected.ResourceName, ResourceTypeConverter.ToTypeString(expected.ResourceType), null, false);
                VerifyPayload(new List<IResource> { expected }, payloadXml);
            }
        }

        [TestMethod]
        public void GetPayloadWithNonExistentResourceNameExpectedReturnsEmptyString()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            var payloadXml = catalog.GetPayload(workspaceID, "xxx", ResourceTypeConverter.TypeService, null);
            VerifyPayload(new List<IResource>(), payloadXml);
        }

        [TestMethod]
        public void GetPayloadWithValidSourceTypeExpectedReturnsResources()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var expected = resources.First(r => r.ResourceType == ResourceType.PluginSource);

            var catalog = new ResourceCatalog();
            var payloadXml = catalog.GetPayload(workspaceID, enSourceType.Plugin);

            VerifyPayload(new List<IResource> { expected }, payloadXml);
        }

        [TestMethod]
        public void GetPayloadWithInvalidSourceTypeExpectedReturnsNothing()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            var payloadXml = catalog.GetPayload(workspaceID, enSourceType.Unknown);

            VerifyPayload(new List<IResource>(), payloadXml);
        }
        #endregion

        #region CopyResource

        [TestMethod]
        public void CopyResourceWithNullResourceExpectedDoesNotCopyResourceToTarget()
        {
            var targetWorkspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            var result = catalog.CopyResource(null, targetWorkspaceID);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CopyResourceWithExistingResourceNameExpectedCopiesResourceToTarget()
        {
            List<IResource> sourceResources;
            var sourceWorkspaceID = Guid.NewGuid();
            SaveResources(sourceWorkspaceID, out sourceResources);

            List<IResource> targetResources;
            var targetWorkspaceID = Guid.NewGuid();
            SaveResources(targetWorkspaceID, out targetResources);

            var sourceResource = sourceResources[0];
            var targetResource = targetResources.First(r => r.ResourceID == sourceResource.ResourceID);
            var targetFile = new FileInfo(targetResource.FilePath);

            targetFile.Delete();
            var result = new ResourceCatalog().CopyResource(sourceResource.ResourceID, sourceWorkspaceID, targetWorkspaceID);
            Assert.IsTrue(result);
            targetFile.Refresh();
            Assert.IsTrue(targetFile.Exists);
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

        #region RollbackResource

        [TestMethod]
        public void RollbackResourceWithUnsignedVersionExpectedDoesNotRollback()
        {
            const string ResourceName = "TestDecisionUnsigned";
            var toVersion = new Version(9999, 0);

            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var path = Path.Combine(workspacePath, "Services");
            var versionControlPath = Path.Combine(path, "VersionControl");
            Directory.CreateDirectory(versionControlPath);

            // Save unsigned version 
            var xml = XmlResource.Fetch(ResourceName);
            xml.Save(Path.Combine(versionControlPath, ResourceName + ".V" + toVersion.Major + ".xml"));

            // Sign and save
            var resource = new Workflow(xml);
            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, resource);

            // Perform test
            var rolledBack = catalog.RollbackResource(workspaceID, resource.ResourceID, resource.Version, toVersion);

            Assert.IsFalse(rolledBack);
        }

        [TestMethod]
        public void RollbackResourceWithSignedVersionExpectedDoesRollback()
        {
            const string ResourceName = "Calculate_RecordSet_Subtract";
            var toVersion = new Version(9999, 0);

            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);

            var path = Path.Combine(workspacePath, "Services");
            var versionControlPath = Path.Combine(path, "VersionControl");
            Directory.CreateDirectory(versionControlPath);

            // Save unsigned version 
            var xml = XmlResource.Fetch(ResourceName);
            xml.Save(Path.Combine(versionControlPath, ResourceName + ".V" + toVersion.Major + ".xml"));

            // Sign and save
            var resource = new Workflow(xml);
            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, resource);

            // Perform test
            var rolledBack = catalog.RollbackResource(workspaceID, resource.ResourceID, resource.Version, toVersion);

            Assert.IsTrue(rolledBack);
        }

        #endregion

        #region DeleteResource

        [TestMethod]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void DeleteResourceWithNullResourceNameExpectedThrowsInvalidDataContractException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            catalog.DeleteResource(workspaceID, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void DeleteResourceWithNullTypeExpectedThrowsInvalidDataContractException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            catalog.DeleteResource(workspaceID, "xxx", null);
        }

        [TestMethod]
        public void DeleteResourceWithWildcardResourceNameExpectedReturnsNoWildcardsAllowed()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            var result = catalog.DeleteResource(workspaceID, "*", ResourceTypeConverter.TypeWorkflowService);
            Assert.AreEqual(ExecStatus.NoWildcardsAllowed, result.Status);
        }

        [TestMethod]
        public void DeleteResourceWithNonExistingResourceNameExpectedReturnsNoMatch()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();
            var result = catalog.DeleteResource(workspaceID, "xxx", ResourceTypeConverter.TypeWorkflowService);
            Assert.AreEqual(ExecStatus.NoMatch, result.Status);
        }

        [TestMethod]
        public void DeleteResourceWithManyExistingResourceNamesExpectedReturnsDuplicateMatch()
        {
            const string ResourceName = "Test";

            var workspaceID = Guid.NewGuid();

            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, new Resource { ResourceID = Guid.NewGuid(), ResourceName = ResourceName, ResourceType = ResourceType.WorkflowService });
            catalog.SaveResource(workspaceID, new Resource { ResourceID = Guid.NewGuid(), ResourceName = ResourceName, ResourceType = ResourceType.WorkflowService });

            var result = catalog.DeleteResource(workspaceID, ResourceName, ResourceTypeConverter.TypeWorkflowService);
            Assert.AreEqual(ExecStatus.DuplicateMatch, result.Status);
        }

        [TestMethod]
        public void DeleteResourceWithOneExistingResourceNameAndInvalidUserRolesExpectedReturnsAccessViolation()
        {
            const string ResourceName = "Test";

            var workspaceID = Guid.NewGuid();

            var catalog = new ResourceCatalog();
            catalog.SaveResource(workspaceID, new Resource { ResourceID = Guid.NewGuid(), ResourceName = ResourceName, ResourceType = ResourceType.WorkflowService, AuthorRoles = "Admins" });

            var result = catalog.DeleteResource(workspaceID, ResourceName, ResourceTypeConverter.TypeWorkflowService, "Power Users");
            Assert.AreEqual(ExecStatus.AccessViolation, result.Status);
        }

        [TestMethod]
        public void DeleteResourceWithOneExistingResourceNameAndValidUserRolesExpectedReturnsSuccessAndDeletesFileFromDisk()
        {
            const string ResourceName = "Test";
            const string UserRoles = "Admins, Power Users";

            var workspaceID = Guid.NewGuid();

            var catalog = new ResourceCatalog();
            var resource = new Resource { ResourceID = Guid.NewGuid(), ResourceName = ResourceName, ResourceType = ResourceType.WorkflowService, AuthorRoles = "Admins, Domain Admins" };
            catalog.SaveResource(workspaceID, resource, UserRoles);

            var file = new FileInfo(resource.FilePath);
            Assert.IsTrue(file.Exists);

            var result = catalog.DeleteResource(workspaceID, ResourceName, ResourceTypeConverter.TypeWorkflowService, UserRoles);
            Assert.AreEqual(ExecStatus.Success, result.Status);

            var actual = catalog.GetResource(workspaceID, resource.ResourceID);
            Assert.IsNull(actual);

            file.Refresh();
            Assert.IsFalse(file.Exists);
        }

        #endregion

        #region GetDynamicObjects

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDynamicObjectsWithNullResourceNameExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            catalog.GetDynamicObjects<DynamicServiceObjectBase>(workspaceID, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDynamicObjectsWithNullResourceNameAndContainsTrueExpectedThrowsArgumentNullException()
        {
            var workspaceID = Guid.NewGuid();
            var catalog = new ResourceCatalog();
            catalog.GetDynamicObjects<DynamicServiceObjectBase>(workspaceID, null, true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDynamicObjectsWithNullResourceExpectedThrowsArgumentNullException()
        {
            var catalog = new ResourceCatalog();
            catalog.GetDynamicObjects((IResource)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDynamicObjectsWithNullResourcesExpectedThrowsArgumentNullException()
        {
            var catalog = new ResourceCatalog();
            catalog.GetDynamicObjects((IEnumerable<IResource>)null);
        }

        [TestMethod]
        public void GetDynamicObjectsWithResourceNameExpectedReturnsObjectGraph()
        {
            var workspaceID = Guid.NewGuid();
            List<IResource> resources;
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();

            var expected = resources.First(r => r.ResourceType == ResourceType.WorkflowService);

            var graph = catalog.GetDynamicObjects<DynamicService>(workspaceID, expected.ResourceName, true);

            VerifyObjectGraph(new List<IResource> { expected }, graph);
        }

        [TestMethod]
        public void GetDynamicObjectsWithResourceExpectedReturnsObjectGraph()
        {
            var workspaceID = Guid.NewGuid();
            List<IResource> resources;
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();

            var expected = resources.First(r => r.ResourceType == ResourceType.WorkflowService);

            var graph = catalog.GetDynamicObjects(expected);

            VerifyObjectGraph(new List<IResource> { expected }, graph);
        }

        [TestMethod]
        public void GetDynamicObjectsWithResourcesExpectedReturnsObjectGraphs()
        {
            var workspaceID = Guid.NewGuid();
            List<IResource> resources;
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();

            var expecteds = resources.Where(r => r.ResourceType == ResourceType.WorkflowService).ToList();

            var graph = catalog.GetDynamicObjects(expecteds);

            VerifyObjectGraph(expecteds, graph);
        }

        [TestMethod]
        public void GetDynamicObjectsWithWorkspaceIDExpectedReturnsObjectGraphsForWorkspace()
        {
            var workspaceID = Guid.NewGuid();
            List<IResource> resources;
            SaveResources(workspaceID, out resources);

            var catalog = new ResourceCatalog();

            var graph = catalog.GetDynamicObjects(workspaceID);

            VerifyObjectGraph(resources, graph);
        }
        #endregion

        #region RemoveWorkspace

        [TestMethod]
        public void RemoveWorkspaceWithInvalidIDExpectedDoesNothing()
        {
            const int WorkspaceCount = 5;
            const int ExpectedWorkspaceCount = WorkspaceCount + 1; // add 1 for server workspace that is auto-loaded

            var catalog = new ResourceCatalog();

            var workspaces = new List<Guid>();
            for (var i = 0; i < WorkspaceCount; i++)
            {
                var id = Guid.NewGuid();
                catalog.LoadWorkspace(id);
                workspaces.Add(id);
            }

            Assert.AreEqual(ExpectedWorkspaceCount, catalog.WorkspaceCount);
            catalog.RemoveWorkspace(Guid.NewGuid());
            Assert.AreEqual(ExpectedWorkspaceCount, catalog.WorkspaceCount);
        }

        [TestMethod]
        public void RemoveWorkspaceWithValidIDExpectedRemovesWorkspace()
        {
            const int WorkspaceCount = 5;
            const int ExpectedWorkspaceCount = WorkspaceCount + 1; // add 1 for server workspace that is auto-loaded

            var catalog = new ResourceCatalog();

            var workspaces = new List<Guid>();
            for (var i = 0; i < WorkspaceCount; i++)
            {
                var id = Guid.NewGuid();
                catalog.LoadWorkspace(id);
                workspaces.Add(id);
            }
            Assert.AreEqual(ExpectedWorkspaceCount, catalog.WorkspaceCount);
            catalog.RemoveWorkspace(workspaces[3]);
            Assert.AreEqual(ExpectedWorkspaceCount - 1, catalog.WorkspaceCount);
        }

        #endregion

        //
        // Static helpers
        //

        #region SaveResources

        public static void SaveResources(Guid sourceWorkspaceID, Guid copyToWorkspaceID, string versionNo, bool injectID, bool signXml, string[] sources, string[] services, out List<IResource> resources)
        {
            var sourceWorkspacePath = SaveResources(sourceWorkspaceID, versionNo, injectID, signXml, sources, services, out resources);
            var targetWorkspacePath = EnvironmentVariables.GetWorkspacePath(copyToWorkspaceID);
            DirectoryHelper.Copy(sourceWorkspacePath, targetWorkspacePath, true);
        }

        public static string SaveResources(Guid workspaceID, string versionNo, bool injectID, bool signXml, string[] sources, string[] services, out List<IResource> resources)
        {
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var sourcesPath = Path.Combine(workspacePath, "Sources");
            var servicesPath = Path.Combine(workspacePath, "Services");

            Directory.CreateDirectory(Path.Combine(sourcesPath, "VersionControl"));
            Directory.CreateDirectory(Path.Combine(servicesPath, "VersionControl"));

            resources = new List<IResource>();
            if (sources != null && sources.Length != 0)
            {
                resources.AddRange(SaveResources(sourcesPath, versionNo, injectID, signXml, sources));
            }
            if (services != null && services.Length != 0)
            {
                resources.AddRange(SaveResources(servicesPath, versionNo, injectID, signXml, services));
            }

            return workspacePath;
        }

        static string SaveResources(Guid workspaceID, out List<IResource> resources)
        {
            return SaveResources(workspaceID, null, false, false,
                new[] { "CatalogSourceAnytingToXmlPlugin", "CatalogSourceCitiesDatabase", "CatalogSourceTestServer" },
                new[] { "CatalogServiceAllTools", "CatalogServiceCitiesDatabase", "CatalogServiceCalculateRecordSetSubtract" },
                out resources);
        }

        static IEnumerable<IResource> SaveResources(string resourcesPath, string versionNo, bool injectID, bool signXml, params string[] resourceNames)
        {
            var result = new List<IResource>();
            foreach (var resourceName in resourceNames)
            {
                var xml = XmlResource.Fetch(resourceName);
                if (injectID)
                {
                    var idAttr = xml.Attribute("ID");
                    if (idAttr == null)
                    {
                        xml.Add(new XAttribute("ID", Guid.NewGuid()));
                    }
                }

                var contents = xml.ToString(SaveOptions.DisableFormatting);
                if (signXml)
                {
                    contents = HostSecurityProvider.Instance.SignXml(contents);
                }
                var res = new Resource(xml)
                {
                    FilePath = Path.Combine(resourcesPath, resourceName + ".xml")
                };

                // Just in case sign the xml

                File.WriteAllText(res.FilePath, contents, Encoding.UTF8);

                if (!string.IsNullOrEmpty(versionNo))
                {
                    File.WriteAllText(Path.Combine(resourcesPath, string.Format("VersionControl\\{0}.V{1}.xml", resourceName, versionNo)), contents, Encoding.UTF8);
                }
                result.Add(res);
            }
            return result;
        }

        #endregion

        
        [TestMethod]
        public void GetDependantsWhereResourceIsDependedOnExpectNonEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new []{"Bug6619",resourceName}).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");

            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependants(workspaceID, resourceName);
            //------------Assert Results-------------------------
            Assert.AreEqual(1,dependants.Count);
        }      
        
        
        [TestMethod]
        public void GetDependantsWhereNoResourcesExpectEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder("xx", new string[0]);
            var resourceName = "resource";
            //------------Assert Precondition-----------------
            Assert.AreEqual(0, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependants(workspaceID, resourceName);
            //------------Assert Results-------------------------
            Assert.AreEqual(0,dependants.Count);
        }   
        
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDependantsWhereResourceNameEmptyStringExpectException()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");

            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependants(workspaceID, "");
            //------------Assert Results-------------------------
            //Exception thrown see attribute
        }

        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDependantsWhereResourceNameNullStringExpectException()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { "Bug6619", resourceName }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");


            //------------Assert Precondition-----------------
            Assert.AreEqual(2, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependants(workspaceID, "");
            //------------Assert Results-------------------------
            //Exception thrown see attribute
        }

        
        [TestMethod]
        public void GetDependantsWhereResourceHasNoDependedOnExpectNonEmptyList()
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(path);
            var resourceName = "Bug6619Dep";
            SaveResources(path, null, false, false, new[] { resourceName }).ToList();

            var rc = new ResourceCatalog();
            var result = rc.LoadWorkspaceViaBuilder(workspacePath, "Services");

            //------------Assert Precondition-----------------
            Assert.AreEqual(1, result.Count);
            //------------Execute Test---------------------------
            var dependants = ResourceCatalog.Instance.GetDependants(workspaceID, resourceName);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, dependants.Count);
        }      

         
        #region VerifyPayload

        static void VerifyPayload(ICollection<IResource> expectedResources, string payloadXml)
        {
            var actualResources = XElement.Parse(payloadXml).Elements().Select(x => new Resource(x)).ToList();

            Assert.AreEqual(expectedResources.Count, actualResources.Count);

            foreach (var expected in expectedResources)
            {
                var actual = actualResources.FirstOrDefault(r => r.ResourceID == expected.ResourceID && r.ResourceName == expected.ResourceName);
                Assert.IsNotNull(actual);
            }
        }

        #endregion

        #region VerifyObjectGraph


        static void VerifyObjectGraph<TGraph>(ICollection<IResource> expectedResources, ICollection<TGraph> actualGraphs)
            where TGraph : DynamicServiceObjectBase
        {
            Assert.AreEqual(expectedResources.Count, actualGraphs.Count);

            foreach (var expected in expectedResources)
            {
                var actualGraph = actualGraphs.FirstOrDefault(g => g.Name == expected.ResourceName);
                Assert.IsNotNull(actualGraph);

                var actual = new Resource(XElement.Parse(actualGraph.ResourceDefinition));
                Assert.AreEqual(expected.ResourceID, actual.ResourceID);
                Assert.AreEqual(expected.ResourceType, actual.ResourceType);
                Assert.AreEqual(expected.ResourcePath, actual.ResourcePath);
            }
        }

        #endregion
    }
}
