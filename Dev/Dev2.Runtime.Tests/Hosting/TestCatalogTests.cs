using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable ObjectCreationAsStatement
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    public class TestCatalogTests
    {

        [TestInitialize]
        public void CleanupTestDirectory()
        {
            if (Directory.Exists(EnvironmentVariables.TestPath))
            {
                DirectoryHelper.CleanUp(EnvironmentVariables.TestPath);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestCatalog_Constructor")]
        public void TestCatalog_Constructor_TestPathDoesNotExist_ShouldCreateIt()
        {
            //------------Setup for test--------------------------
            //------------Assert Preconditions-------------------
            Assert.IsFalse(Directory.Exists(EnvironmentVariables.TestPath));
            //------------Execute Test---------------------------
            new TestCatalog();
            //------------Assert Results-------------------------
            Assert.IsTrue(Directory.Exists(EnvironmentVariables.TestPath));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestCatalog_SaveTests")]
        public void TestCatalog_SaveTests_WhenNullList_ShouldDoNothing()
        {
            //------------Setup for test--------------------------
            var testCatalog = new TestCatalog();
            var resourceID = Guid.NewGuid();
            //------------Execute Test---------------------------
            testCatalog.SaveTests(resourceID,null);
            //------------Assert Results-------------------------
            Assert.IsFalse(Directory.Exists(EnvironmentVariables.TestPath+"\\"+resourceID));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestCatalog_SaveTests")]
        public void TestCatalog_SaveTests_WhenEmptyList_ShouldDoNothing()
        {
            //------------Setup for test--------------------------
            var testCatalog = new TestCatalog();
            var resourceID = Guid.NewGuid();
            //------------Execute Test---------------------------
            testCatalog.SaveTests(resourceID,new List<IServiceTestModelTO>());
            //------------Assert Results-------------------------
            Assert.IsFalse(Directory.Exists(EnvironmentVariables.TestPath+"\\"+resourceID));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestCatalog_SaveTests")]
        public void TestCatalog_SaveTests_WhenNotEmptyList_ShouldSaveTestsAsFiles()
        {
            //------------Setup for test--------------------------
            var testCatalog = new TestCatalog();
            var resourceID = Guid.NewGuid();
            var serviceTestModelTos = new List<IServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    Enabled = true,
                    TestName = "Test 1"
                },
                new ServiceTestModelTO
                {
                    Enabled = false,
                    TestName = "Test 2"
                }
            };
            //------------Execute Test---------------------------
            testCatalog.SaveTests(resourceID,serviceTestModelTos);
            //------------Assert Results-------------------------
            var path = EnvironmentVariables.TestPath+"\\"+resourceID;
            Assert.IsTrue(Directory.Exists(path));
            var testFiles = Directory.EnumerateFiles(path).ToList();
            var test1FilePath = path+"\\"+"Test 1.test";
            Assert.AreEqual(test1FilePath,testFiles[0]);
            var test2FilePath = path+"\\"+"Test 2.test";
            Assert.AreEqual(test2FilePath,testFiles[1]);

            var test1String = File.ReadAllText(test1FilePath);
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var test1 = serializer.Deserialize<IServiceTestModelTO>(test1String);
            Assert.AreEqual("Test 1",test1.TestName);
            Assert.IsTrue(test1.Enabled);

            var test2String = File.ReadAllText(test2FilePath);
            var test2 = serializer.Deserialize<IServiceTestModelTO>(test2String);
            Assert.AreEqual("Test 2", test2.TestName);
            Assert.IsFalse(test2.Enabled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestCatalog_DeleteTest")]
        public void TestCatalog_DeleteTest_WhenResourceIdTestName_ShouldDeleteTest()
        {
            //------------Setup for test--------------------------
            var testCatalog = new TestCatalog();
            var resourceID = Guid.NewGuid();
            var serviceTestModelTos = new List<IServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    Enabled = true,
                    TestName = "Test 1"
                },
                new ServiceTestModelTO
                {
                    Enabled = false,
                    TestName = "Test 2"
                }
            };
            testCatalog.SaveTests(resourceID, serviceTestModelTos);
            //------------Assert Preconditions-------------------
            var path = EnvironmentVariables.TestPath + "\\" + resourceID;
            Assert.IsTrue(Directory.Exists(path));
            var testFiles = Directory.EnumerateFiles(path).ToList();
            var test1FilePath = path + "\\" + "Test 1.test";
            Assert.AreEqual(test1FilePath, testFiles[0]);
            var test2FilePath = path + "\\" + "Test 2.test";
            Assert.AreEqual(test2FilePath, testFiles[1]);

            var test1String = File.ReadAllText(test1FilePath);
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var test1 = serializer.Deserialize<IServiceTestModelTO>(test1String);
            Assert.AreEqual("Test 1", test1.TestName);
            Assert.IsTrue(test1.Enabled);

            var test2String = File.ReadAllText(test2FilePath);
            var test2 = serializer.Deserialize<IServiceTestModelTO>(test2String);
            Assert.AreEqual("Test 2", test2.TestName);
            Assert.IsFalse(test2.Enabled);
            //------------Execute Test---------------------------
            testCatalog.DeleteTest(resourceID, "Test 2");
            //------------Assert Results-------------------------
            Assert.IsTrue(File.Exists(test1FilePath));
            Assert.IsFalse(File.Exists(test2FilePath));

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestCatalog_Load")]
        public void TestCatalog_Load_WhenTests_ShouldLoadDictionaryWithResourceIdAndTests()
        {
            //------------Setup for test--------------------------
            var testCatalog = new TestCatalog();
            var resourceID = Guid.NewGuid();
            var resourceID2 = Guid.NewGuid();
            var serviceTestModelTos = new List<IServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    Enabled = true,
                    TestName = "Test 1"
                },
                new ServiceTestModelTO
                {
                    Enabled = false,
                    TestName = "Test 2"
                }
            };


            var res2ServiceTestModelTos = new List<IServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    Enabled = true,
                    TestName = "Test 21"
                },
                new ServiceTestModelTO
                {
                    Enabled = false,
                    TestName = "Test 22"
                }
            };
            testCatalog.SaveTests(resourceID, serviceTestModelTos);
            testCatalog.SaveTests(resourceID2, res2ServiceTestModelTos);
            //------------Execute Test---------------------------
            testCatalog.Load();
            //------------Assert Results-------------------------
            Assert.AreEqual(2,testCatalog.Tests.Count);
            var res1Tests = testCatalog.Tests[resourceID];
            Assert.AreEqual(2,res1Tests.Count);
            Assert.AreEqual("Test 1",res1Tests[0].TestName);
            Assert.AreEqual("Test 2",res1Tests[1].TestName);
            var res2Tests = testCatalog.Tests[resourceID2];
            Assert.AreEqual(2, res2Tests.Count);
            Assert.AreEqual("Test 21", res2Tests[0].TestName);
            Assert.AreEqual("Test 22", res2Tests[1].TestName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestCatalog_Fetch")]
        public void TestCatalog_Fetch_WhenResourceIdValid_ShouldReturnListOfTestsForResourceId()
        {
            //------------Setup for test--------------------------
            var testCatalog = new TestCatalog();
            var resourceID = Guid.NewGuid();
            var resourceID2 = Guid.NewGuid();
            var serviceTestModelTos = new List<IServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    Enabled = true,
                    TestName = "Test 1"
                },
                new ServiceTestModelTO
                {
                    Enabled = false,
                    TestName = "Test 2"
                }
            };


            var res2ServiceTestModelTos = new List<IServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    Enabled = true,
                    TestName = "Test 21"
                },
                new ServiceTestModelTO
                {
                    Enabled = false,
                    TestName = "Test 22"
                }
            };
            testCatalog.SaveTests(resourceID, serviceTestModelTos);
            testCatalog.SaveTests(resourceID2, res2ServiceTestModelTos);
            testCatalog.Load();
            //------------Execute Test---------------------------
            var tests = testCatalog.Fetch(resourceID2);
            //------------Assert Results-------------------------
            var res2Tests = tests;
            Assert.AreEqual(2, res2Tests.Count);
            Assert.AreEqual("Test 21", res2Tests[0].TestName);
            Assert.AreEqual("Test 22", res2Tests[1].TestName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestCatalog_Fetch")]
        public void TestCatalog_Fetch_WhenResourceIdNotLoaded_ShouldReturnListOfTestsForResourceId()
        {
            //------------Setup for test--------------------------
            var testCatalog = new TestCatalog();
            var resourceID = Guid.NewGuid();
            var resourceID2 = Guid.NewGuid();
            var serviceTestModelTos = new List<IServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    Enabled = true,
                    TestName = "Test 1"
                },
                new ServiceTestModelTO
                {
                    Enabled = false,
                    TestName = "Test 2"
                }
            };


            var res2ServiceTestModelTos = new List<IServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    Enabled = true,
                    TestName = "Test 21"
                },
                new ServiceTestModelTO
                {
                    Enabled = false,
                    TestName = "Test 22"
                }
            };
            testCatalog.SaveTests(resourceID, serviceTestModelTos);
            testCatalog.SaveTests(resourceID2, res2ServiceTestModelTos);
            //------------Execute Test---------------------------
            var tests = testCatalog.Fetch(resourceID2);
            //------------Assert Results-------------------------
            var res2Tests = tests;
            Assert.AreEqual(2, res2Tests.Count);
            Assert.AreEqual("Test 21", res2Tests[0].TestName);
            Assert.AreEqual("Test 22", res2Tests[1].TestName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestCatalog_Fetch")]
        public void TestCatalog_Fetch_WhenResourceIdNotValid_ShouldReturnListOfTestsForResourceId()
        {
            //------------Setup for test--------------------------
            var testCatalog = new TestCatalog();
            var resourceID = Guid.NewGuid();
            var resourceID2 = Guid.NewGuid();
            var serviceTestModelTos = new List<IServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    Enabled = true,
                    TestName = "Test 1"
                },
                new ServiceTestModelTO
                {
                    Enabled = false,
                    TestName = "Test 2"
                }
            };


            var res2ServiceTestModelTos = new List<IServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    Enabled = true,
                    TestName = "Test 21"
                },
                new ServiceTestModelTO
                {
                    Enabled = false,
                    TestName = "Test 22"
                }
            };
            testCatalog.SaveTests(resourceID, serviceTestModelTos);
            testCatalog.SaveTests(resourceID2, res2ServiceTestModelTos);
            //------------Execute Test---------------------------
            var tests = testCatalog.Fetch(Guid.NewGuid());
            //------------Assert Results-------------------------
            var res2Tests = tests;
            Assert.AreEqual(0, res2Tests.Count);
        }
    }
}
