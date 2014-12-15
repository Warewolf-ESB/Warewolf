
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
using System.Diagnostics.CodeAnalysis;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.Factories;
using Dev2.Data.Interfaces;
using Dev2.Data.Operations;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Data.Tests.Operations
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class Dev2ReplaceOperationTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Replace Tests

        [TestMethod]
        public void Replace_NoCaseMatch_Recorset_Expected_Correct_Data_Returned_ReplaceCount_Twenty()
        {
            ErrorResultTO errors;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            Guid exidx = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), TestStrings.ReplaceDataListWithData.ToStringBuilder(), TestStrings.ReplaceDataListShape.ToStringBuilder(), out errors);
            IDev2DataListUpsertPayloadBuilder<string> payloadBuilder = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
            IDev2ReplaceOperation replaceOperation = Dev2OperationsFactory.CreateReplaceOperation();
            IBinaryDataListEntry entry;
            const string Expected = "World0";
            const string Expression = "[[results(*).resfield]]";
            int replaceCount;
            payloadBuilder = replaceOperation.Replace(exidx, Expression, "hello", "World", false, payloadBuilder, out errors, out replaceCount,out entry);

            var frames = payloadBuilder.FetchFrames();
            var frame = frames[0].FetchNextFrameItem();
            Assert.AreEqual(20, replaceCount);
            Assert.AreEqual("[[results(1).resfield]]", frame.Expression);
            Assert.AreEqual(Expected, frame.Value);
        }

        [TestMethod]
        public void Replace_CaseMatch_Recorset_Expected_No_Replaces()
        {
            ErrorResultTO errors;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            Guid exidx = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), TestStrings.ReplaceDataListWithData, TestStrings.ReplaceDataListShape.ToStringBuilder(), out errors);
            IDev2DataListUpsertPayloadBuilder<string> payloadBuilder = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
            IBinaryDataListEntry entry;
            IDev2ReplaceOperation replaceOperation = Dev2OperationsFactory.CreateReplaceOperation();
            const string Expression = "[[results(*).resfield]]";
            int replaceCount;
            // ReSharper disable RedundantAssignment
            payloadBuilder = replaceOperation.Replace(exidx, Expression, "hello", "World", true, payloadBuilder, out errors, out replaceCount, out entry);
            // ReSharper restore RedundantAssignment

            Assert.AreEqual(0, replaceCount);
        }

        [TestMethod]
        public void Replace_NoCaseMatch_Scalar_Expected_Twenty_Replaces()
        {
            ErrorResultTO errors;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            Guid exidx = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), TestStrings.ReplaceDataListWithData.ToStringBuilder(), TestStrings.ReplaceDataListShape.ToStringBuilder(), out errors);
            IDev2DataListUpsertPayloadBuilder<string> payloadBuilder = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
            IBinaryDataListEntry entry;
            IDev2ReplaceOperation replaceOperation = Dev2OperationsFactory.CreateReplaceOperation();
            const string Expression = "[[scalar]]";
            int replaceCount;
            // ReSharper disable RedundantAssignment
            payloadBuilder = replaceOperation.Replace(exidx, Expression, "test", "World", false, payloadBuilder, out errors, out replaceCount,out entry);
            // ReSharper restore RedundantAssignment

            Assert.AreEqual(20, replaceCount);
        }

        #endregion
    }
}
