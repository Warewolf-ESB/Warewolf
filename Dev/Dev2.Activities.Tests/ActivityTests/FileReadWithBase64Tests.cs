/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using ActivityUnitTests;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for FileReadWithBase64Tests
    /// </summary>
    [TestClass]
    public class FileReadWithBase64Tests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }
        private string _filePath;

        [TestInitialize]
        public void Setup()
        {
            var newGuid = Guid.NewGuid();
            _filePath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "2.txt");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new FileReadWithBase64 { InputPath = inputPath, Result = "[[CompanyName]]" };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(inputPath, act.InputPath);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_UpdateForEachInputs_MoreThan1Updates_DoesNotUpdates()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var path = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]].txt");
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new FileReadWithBase64 { InputPath = inputPath, Result = "[[CompanyName]]" };

            var tuple1 = new Tuple<string, string>(inputPath, "Test");
            var tuple2 = new Tuple<string, string>(path, "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual(inputPath, act.InputPath);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_UpdateForEachInputs_1Update_Updates()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new FileReadWithBase64 { InputPath = inputPath, Result = "[[CompanyName]]" };

            var tuple1 = new Tuple<string, string>(inputPath, "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.InputPath);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            const string result = "[[CompanyName]]";
            var act = new FileReadWithBase64 { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"), Result = result };

            act.UpdateForEachOutputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            const string result = "[[CompanyName]]";
            var act = new FileReadWithBase64 { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"), Result = result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_UpdateForEachOutputs_1Updates_UpdateResult()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new FileReadWithBase64 { InputPath = inputPath, Result = "[[CompanyName]]" };

            var tuple1 = new Tuple<string, string>("[[CompanyName]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt");
            var act = new FileReadWithBase64 { InputPath = inputPath, Result = "[[CompanyName]]" };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(inputPath, dsfForEachItems[0].Name);
            Assert.AreEqual(inputPath, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(FileReadWithBase64))]
        public void FileReadWithBase64_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            var newGuid = Guid.NewGuid();
            const string result = "[[CompanyName]]";
            var act = new FileReadWithBase64 { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", newGuid + "[[CompanyName]]2.txt"), Result = result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(result, dsfForEachItems[0].Name);
            Assert.AreEqual(result, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("FileReadWithBase64_Intergration")]
        public void FileReadWithBase64_TryExecuteConcreteAction_IsResultBase64_ReturnsOutputStrings()
        {
            //------------Setup for test--------------------------
            CreateFileTest(_filePath);
            const string result = "[[CompanyName]]";
            var env = new ExecutionEnvironment();

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment).Returns(env);
            mockDataObject.Setup(o => o.IsDebugMode()).Returns(true);

            var act = new TestFileReadWithBase64 { IsResultBase64 = true, InputPath = _filePath, Result = result };

            //------------Execute Test---------------------------
            var dsfOutputStrings = act.TestTryExecuteConcreteAction(mockDataObject.Object, out _, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfOutputStrings.Count);
            Assert.AreEqual("c29tZSBzdHJpbmc=", dsfOutputStrings[0].OutputStrings[0]);
            Assert.AreEqual(4, act.GetDebugInputs(env, 0).Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("FileReadWithBase64_Intergration")]
        public void FileReadWithBase64_TryExecuteConcreteAction_IsResultNotBase64_ReturnsOutputStrings()
        {
            //------------Setup for test--------------------------
            CreateFileTest(_filePath);
            const string result = "[[CompanyName]]";
            var env = new ExecutionEnvironment();

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment).Returns(env);
            mockDataObject.Setup(o => o.IsDebugMode()).Returns(true);

            var act = new TestFileReadWithBase64 { IsResultBase64 = false, InputPath = _filePath, Result = result };
            //------------Execute Test---------------------------
            var dsfOutputStrings = act.TestTryExecuteConcreteAction(mockDataObject.Object, out _, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfOutputStrings.Count);
            Assert.AreEqual("some string", dsfOutputStrings[0].OutputStrings[0]);
            Assert.AreEqual(3, act.GetDebugInputs(env, 0).Count);
        }

        private void CreateFileTest(string filePath)
        {
            System.IO.File.WriteAllText(filePath, "some string");
        }

        [TestCleanup]
        public void Cleanup()
        {
            System.IO.File.Delete(_filePath);
        }

    }

    internal class TestFileReadWithBase64 : FileReadWithBase64
    {
        public IList<OutputTO> TestTryExecuteConcreteAction(IDSFDataObject dataObject, out ErrorResultTO errorResultTO, int update)
        {
            return base.TryExecuteConcreteAction(dataObject, out errorResultTO, update);
        }
    }
}
