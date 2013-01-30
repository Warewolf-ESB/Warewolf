using System;
using System.IO;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using System.Reflection;
using Dev2.Runtime;
using Dev2.Integration.Tests.Dev2.Application.Server.Tests.Workspace.XML;

namespace Dev2.Integration.Tests.Runtime.Tests
{
    /// <summary>
    /// Summary description for WorkspaceRespositoryTest
    /// </summary>
    [TestClass]
    public class WorkspaceRespositoryTest
    {
        static string _servicesPath;
        static WorkspaceRepository _testInstance;


        static object l = new object();

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            Directory.SetCurrentDirectory(testContext.TestDir);

            #region Copy server services to file system

            _servicesPath = Path.Combine(testContext.TestDir, "Services");
            
            Directory.CreateDirectory(_servicesPath);
            var xml = XmlResource.Fetch("Calculate_RecordSet_Subtract.xml");
            xml.Save(Path.Combine(_servicesPath, "Calculate_RecordSet_Subtract.xml"));

            #endregion

            _testInstance = WorkspaceRepository.Instance;
        }

        [ClassCleanup]
        public static void MyClassCleanup()
        {
            if (Directory.Exists(_testInstance.RepositoryPath))
            {
                Directory.Delete(_testInstance.RepositoryPath, true);
            }
            if (Directory.Exists(_servicesPath))
            {
                Directory.Delete(_servicesPath, true);
            }
        }

        #region CTOR Tests

        [TestMethod]
        public void ServerWorkspaceCreatedAfterInstantiation()
        {
            Assert.IsNotNull(_testInstance.ServerWorkspace);
        }

        [TestMethod]
        public void ServerWorkspaceCreatedWithServerWorkspaceID()
        {
            Assert.AreEqual(WorkspaceRepository.ServerWorkspaceID, _testInstance.ServerWorkspace.ID);
        }

        #endregion CTOR Tests

        #region Get Tests

        [TestMethod]
        public void GetWithEmptyGuidReturnsServerWorkspace()
        {
            var result = _testInstance.Get(Guid.Empty);
            Assert.AreSame(_testInstance.ServerWorkspace, result);
        }

        [TestMethod]
        public void GetWithServerWorkspaceIDReturnsServerWorkspace()
        {
            var result = _testInstance.Get(WorkspaceRepository.ServerWorkspaceID);
            Assert.AreSame(_testInstance.ServerWorkspace, result);
        }

        [TestMethod]
        public void GetWithNewWorkspaceIDIncreasesItemCountByOne()
        {
            var expected = _testInstance.Count + 1;
            var workspace = _testInstance.Get(Guid.NewGuid());
            Assert.AreEqual(expected, _testInstance.Count);
        }

        [TestMethod]
        public void GetWithForceReloadsWorkspace()
        {
            var previous = _testInstance.ServerWorkspace;
            var result = _testInstance.Get(WorkspaceRepository.ServerWorkspaceID, true);
            Assert.AreNotSame(previous, result);
        }

        #endregion Get Tests

        #region Delete

        [TestMethod]
        public void DeleteDecreasesItemCountByOne()
        {
            lock (l)
            {
                // this will add
                var workspace = _testInstance.Get(Guid.NewGuid());
                var expected = _testInstance.Count - 1;
                _testInstance.Delete(workspace);
                Assert.AreEqual(expected, _testInstance.Count);
            }

        }

        [TestMethod]
        public void DeleteNullItem_Expected_NoOperationPerformed()
        {
            // this will add
            lock (l)
            {
                var expected = _testInstance.Count;
                _testInstance.Delete(null);
                Assert.AreEqual(expected, _testInstance.Count);
            }
        }

        #endregion Delete

        #region Save

        [TestMethod]
        public void SaveWithNewWorkspaceIncreasesItemCountByOne()
        {
            lock (l)
            {
                var expected = _testInstance.Count + 1;
                var workspace = new Workspace(Guid.NewGuid());
                _testInstance.Save(workspace);
                Assert.AreEqual(expected, _testInstance.Count);
            }
        }

        [TestMethod]
        public void SaveWithNullWorkspaceIncreasesItemCountByOne()
        {
            lock (l)
            {
                var expected = _testInstance.Count;
                _testInstance.Save(null);
                Assert.AreEqual(expected, _testInstance.Count);
            }
        }

        #endregion Save
    }
}
