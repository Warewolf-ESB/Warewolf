using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.DynamicServices.Test.XML;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    public class ResourceCatalogTests
    {
        const int SaveResourceCount = 5;

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
            var workspacePath = SaveResources(workspaceID, out resources);
            try
            {
                var catalog = new ResourceCatalog();
                var catalogResources = catalog[workspaceID];

                Assert.AreEqual(1, catalog.Count);
                Assert.AreEqual(SaveResourceCount, catalogResources.Count);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    Directory.Delete(workspacePath, true);
                }
            }
        }

        [TestMethod]
        public void ThisWithExistingWorkspaceIDExpectedReturnsExistingCatalogForWorkspace()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            var workspacePath = SaveResources(workspaceID, out resources);
            try
            {
                var catalog = new ResourceCatalog();
                catalog.Load(workspaceID);
                var catalogResources = catalog[workspaceID];

                Assert.AreEqual(1, catalog.Count);
                Assert.AreEqual(SaveResourceCount, catalogResources.Count);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    Directory.Delete(workspacePath, true);
                }
            }
        }

        #endregion

        #region Load

        [TestMethod]
        public void LoadWithNewWorkspaceIDExpectedAddsCatalogForWorkspace()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            var workspacePath = SaveResources(workspaceID, out resources);
            try
            {
                var catalog = new ResourceCatalog();
                catalog.Load(workspaceID);

                Assert.AreEqual(1, catalog.Count);
                var catalogResources = catalog[workspaceID];
                Assert.AreEqual(SaveResourceCount, catalogResources.Count);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    Directory.Delete(workspacePath, true);
                }
            }
        }

        [TestMethod]
        public void LoadWithExistingWorkspaceIDExpectedUpdatesCatalogForWorkspace()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            var workspacePath = SaveResources(workspaceID, out resources);
            try
            {
                var catalog = new ResourceCatalog();
                catalog.Load(workspaceID);
                catalog.Load(workspaceID);

                Assert.AreEqual(1, catalog.Count);
                var catalogResources = catalog[workspaceID];
                Assert.AreEqual(SaveResourceCount, catalogResources.Count);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    Directory.Delete(workspacePath, true);
                }
            }
        }

        #endregion

        #region LoadAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoadAsyncWithNullWorkspaceArgumentExpectedThrowsArgumentNullException()
        {
            var task = ResourceCatalog.LoadAsync(null, new string[0]);
            try
            {
                task.Wait();
            }
            catch(AggregateException ae)
            {
                throw ae.InnerException;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoadAsyncWithNullFoldersArgumentExpectedThrowsArgumentNullException()
        {
            var task = ResourceCatalog.LoadAsync("xx", null);
            try
            {
                task.Wait();
            }
            catch(AggregateException ae)
            {
                throw ae.InnerException;
            }
        }

        [TestMethod]
        public void LoadAsyncWithEmptyFoldersArgumentExpectedReturnsEmptyCatalog()
        {
            var task = ResourceCatalog.LoadAsync("xx", new string[0]);
            task.Wait();
            Assert.AreEqual(0, task.Result.Count);
        }

        [TestMethod]
        public void LoadAsyncWithExistingSourcesPathAndNonExistingServicesPathExpectedReturnsCatalogForSources()
        {
            var workspaceID = Guid.NewGuid();
            var workspacePath = GlobalConstants.GetWorkspacePath(workspaceID);

            try
            {
                var sourcesPath = Path.Combine(workspacePath, "Sources");
                Directory.CreateDirectory(sourcesPath);
                var resources = SaveResources(sourcesPath, "CatalogSourceAnytingToXmlPlugin", "CatalogSourceCitiesDatabase", "CatalogSourceTestServer").ToList();

                var task = ResourceCatalog.LoadAsync(workspacePath, "Sources", "Services");
                task.Wait();

                Assert.AreEqual(3, task.Result.Count);

                foreach(var resource in task.Result)
                {
                    var expected = resources.First(r => r.ResourceName == resource.ResourceName);
                    Assert.AreEqual(expected.Contents, resource.Contents);
                    Assert.AreEqual(expected.FilePath, resource.FilePath);
                }
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    Directory.Delete(workspacePath, true);
                }
            }
        }

        [TestMethod]
        public void LoadAsyncWithValidWorkspaceIDExpectedReturnsCatalogForWorkspace()
        {
            List<IResource> resources;
            var workspaceID = Guid.NewGuid();
            var workspacePath = SaveResources(workspaceID, out resources);
            try
            {
                var task = ResourceCatalog.LoadAsync(workspacePath, "Sources", "Services");
                task.Wait();

                Assert.AreEqual(SaveResourceCount, task.Result.Count);

                foreach(var resource in task.Result)
                {
                    var expected = resources.First(r => r.ResourceName == resource.ResourceName);
                    Assert.AreEqual(expected.Contents, resource.Contents);
                    Assert.AreEqual(expected.FilePath, resource.FilePath);
                }
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    Directory.Delete(workspacePath, true);
                }
            }
        }

        #endregion

        #region ParallelExecution

        [TestMethod]
        public void LoadInParallelExpectedBehavesConcurrently()
        {
            const int NumWorkspaces = 5;
            const int NumThreadsPerWorkspace = 5;
            try
            {
                var catalog = new ResourceCatalog();

                var threadArray = new Thread[NumWorkspaces * NumThreadsPerWorkspace];

                #region Create threads

                for(var i = 0; i < NumWorkspaces; i++)
                {
                    var workspaceID = Guid.NewGuid();
                    List<IResource> resources;
                    SaveResources(workspaceID, out resources);

                    for(var j = 0; j < NumThreadsPerWorkspace; j++)
                    {
                        var t = (i * NumThreadsPerWorkspace) + j;
                        if(j == 0)
                        {
                            threadArray[t] = new Thread(() =>
                            {
                                catalog.Load(workspaceID); // Always loads from disk
                                var result = catalog[workspaceID]; // Loads from disk if not in memory
                                Assert.AreEqual(SaveResourceCount, result.Count);
                            });
                        }
                        else
                        {
                            threadArray[t] = new Thread(() =>
                            {
                                var result = catalog[workspaceID]; // Loads from disk if not in memory
                                Assert.AreEqual(SaveResourceCount, result.Count);
                            });
                        }
                    }
                }

                #endregion


                //Start the threads.
                Parallel.For(0, threadArray.Length, i => threadArray[i].Start());

                //Wait until all the threads spawn out and finish.
                foreach(var t in threadArray)
                {
                    t.Join();
                }

                Assert.AreEqual(NumWorkspaces, catalog.Count);
            }
            finally
            {
                var workspacePath = GlobalConstants.WorkspacePath;
                if(Directory.Exists(workspacePath))
                {
                    try
                    {
                        Directory.Delete(workspacePath, true);
                    }
                    catch(IOException)
                    {
                        // Don't fail test just because this threw an IO exception!
                    }
                }
            }
        }

        #endregion

        //
        // Static helpers
        //
        #region SaveResources

        static string SaveResources(Guid workspaceID, out List<IResource> resources)
        {
            var workspacePath = GlobalConstants.GetWorkspacePath(workspaceID);
            var sourcesPath = Path.Combine(workspacePath, "Sources");
            var servicesPath = Path.Combine(workspacePath, "Services");
            Directory.CreateDirectory(sourcesPath);
            Directory.CreateDirectory(servicesPath);

            resources = new List<IResource>();
            resources.AddRange(SaveResources(sourcesPath, "CatalogSourceAnytingToXmlPlugin", "CatalogSourceCitiesDatabase", "CatalogSourceTestServer"));
            resources.AddRange(SaveResources(servicesPath, "CatalogServiceAllTools", "CatalogServiceCitiesDatabase"));

            return workspacePath;
        }

        static IEnumerable<IResource> SaveResources(string resourcesPath, params string[] resourceNames)
        {
            var result = new List<IResource>();
            foreach(var resourceName in resourceNames)
            {
                var xml = XmlResource.Fetch(resourceName);
                var res = new Resource(xml) { FilePath = Path.Combine(resourcesPath, resourceName + ".xml"), Contents = xml.ToString(SaveOptions.DisableFormatting) };
                result.Add(res);
                xml.Save(res.FilePath);
            }
            return result;
        }

        #endregion

    }
}
