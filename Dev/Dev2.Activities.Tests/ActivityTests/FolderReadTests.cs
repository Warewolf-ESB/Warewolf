
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ActivityUnitTests;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable once InconsistentNaming
namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FolderReadTests : BaseActivityUnitTest
    {


        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Get Input/Output Tests

        [TestMethod]

// ReSharper disable InconsistentNaming
        public void FolderReadActivity_GetInputs_Expected_Four_Input()
// ReSharper restore InconsistentNaming
        {
            DsfFolderRead testAct = new DsfFolderRead();

            IBinaryDataList inputs = testAct.GetInputs();

            var res = inputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(inputs.UID);

            Assert.AreEqual(7, res);
        }

        [TestMethod]
        public void FolderReadActivity_GetOutputs_Expected_One_Output()
        {
            DsfFolderRead testAct = new DsfFolderRead();

            IBinaryDataList outputs = testAct.GetOutputs();

            var res = outputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(outputs.UID);

            Assert.AreEqual(1, res);
        }

        #endregion Get Input/Output Tests

        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFolderRead_UpdateForEachInputs")]
        public void DsfFolderRead_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]]");
            var act = new DsfFolderRead { InputPath = inputPath, Result = "[[res]]" };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(inputPath, act.InputPath);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFolderRead_UpdateForEachInputs")]
        public void DsfFolderRead_UpdateForEachInputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]]");
            var act = new DsfFolderRead { InputPath = inputPath, Result = "[[res]]" };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>(inputPath, "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(inputPath, act.InputPath);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFolderRead_UpdateForEachInputs")]
        public void DsfFolderRead_UpdateForEachInputs_1Update_UpdateInputPath()
        {
            //------------Setup for test--------------------------
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]]");
            var act = new DsfFolderRead { InputPath = inputPath, Result = "[[res]]" };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.InputPath);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFolderRead_UpdateForEachOutputs")]
        public void DsfFolderRead_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string result = "[[res]]";
            var act = new DsfFolderRead { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]]"), Result = result };

            act.UpdateForEachOutputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFolderRead_UpdateForEachOutputs")]
        public void DsfFolderRead_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string result = "[[res]]";
            var act = new DsfFolderRead { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]]"), Result = result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFolderRead_UpdateForEachOutputs")]
        public void DsfFolderRead_UpdateForEachOutputs_1Updates_UpdateCommandResult()
        {
            //------------Setup for test--------------------------
            const string result = "[[res]]";
            var act = new DsfFolderRead { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]]"), Result = result };

            var tuple1 = new Tuple<string, string>("[[res]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFolderRead_GetForEachInputs")]
        public void DsfFolderRead_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            var inputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]]");
            var act = new DsfFolderRead { InputPath = inputPath, Result = "[[res]]" };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(inputPath, dsfForEachItems[0].Name);
            Assert.AreEqual(inputPath, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfFolderRead_GetForEachOutputs")]
        public void DsfFolderRead_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            const string result = "[[res]]";
            var act = new DsfFolderRead { InputPath = string.Concat(TestContext.TestRunDirectory, "\\", "[[CompanyName]]"), Result = result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(result, dsfForEachItems[0].Name);
            Assert.AreEqual(result, dsfForEachItems[0].Value);
        }

    }
}
