using System;
using System.Collections.Generic;
using System.IO;
using Dev2.DynamicServices.Test.XML;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.DynamicServices.Test
{
    [TestClass]
    public class DynamicServiceRepositoryTest
    {
        const string TestFileName = "Calculate_RecordSet_Subtract";
        static string _testPath;

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            Directory.SetCurrentDirectory(testContext.TestDir);
            _testPath = Path.Combine(testContext.TestDir, Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testPath);

            var xml = XmlResource.Fetch(TestFileName);
            xml.Save(Path.Combine(_testPath, TestFileName + ".xml"));
        }

        [ClassCleanup]
        public static void MyClassCleanup()
        {
            if (Directory.Exists(_testPath))
            {
                Directory.Delete(_testPath, true);
            }
        }

        [TestMethod]
        public void LoadWithNull()
        {
            Run(null);
        }

        [TestMethod]
        public void LoadWithOneWorkspaceService()
        {
            Run(CreateList(1));
        }

        [TestMethod]
        public void LoadWithMulitpleWorkspaceServices()
        {
            Run(CreateList(3));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GenerateObjectGraphFromStringWithNull()
        {
            DynamicServiceRepository.GenerateObjectGraphFromString(null);
        }

        [TestMethod]
        public void GenerateObjectGraphFromStringWithXml()
        {
            var xml = XmlResource.Fetch(TestFileName);
            var result = DynamicServiceRepository.GenerateObjectGraphFromString(xml.ToString());
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReadWithNullPath()
        {
            var repo = new DynamicServiceRepository();
            repo.Read();
        }

        [TestMethod]
        public void ReadWithPath()
        {
            var repo = new DynamicServiceRepository { SourcePath = _testPath };
            var result = repo.Read();
            Assert.AreEqual(1, result.Count);
        }

        #region Run

        static void Run(ICollection<IDynamicServiceObject> workspaceRepository)
        {
            var repo = new DynamicServiceRepository { SourcePath = _testPath };
            repo.Load(workspaceRepository);
            var expected = (workspaceRepository == null ? 0 : workspaceRepository.Count);
            Assert.AreEqual(expected, repo.Items.Count);
        }

        #endregion

        #region CreateList

        static IList<IDynamicServiceObject> CreateList(int n)
        {
            return CreateList(0, n);
        }

        static IList<IDynamicServiceObject> CreateList(int start, int stop)
        {
            var result = new List<IDynamicServiceObject>();
            for (var i = start; i < stop; i++)
            {
                var dso = CreateDynamicServiceObject(i);
                result.Add(dso.Object);
            }
            return result;
        }

        #endregion

        #region CreateDynamicServiceRepository

        static DynamicServiceRepository CreateDynamicServiceRepository(int n)
        {
            return CreateDynamicServiceRepository(0, n);
        }

        static DynamicServiceRepository CreateDynamicServiceRepository(int start, int stop)
        {
            var result = new DynamicServiceRepository();
            foreach (var dso in CreateList(start, stop))
            {
                result.Add(dso);
            }
            return result;
        }

        #endregion

        #region CreateDynamicServiceObject

        static Mock<IDynamicServiceObject> CreateDynamicServiceObject(int n)
        {
            var dso = new Mock<IDynamicServiceObject>();
            dso.Setup(d => d.Name).Returns("Service" + n);
            return dso;
        }

        #endregion


    }
}
