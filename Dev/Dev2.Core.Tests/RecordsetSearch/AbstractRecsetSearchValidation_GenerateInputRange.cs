
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.RecordsetSearch
{
    /// <summary>
    /// Summary description for AbstractRecsetSearchValidation_GenerateInputRange
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class AbstractRecsetSearchValidation_GenerateInputRange
    {


        //        #region Test Variables

        //        static string currentDl = @"<ADL><TestVarFirstName>Massimo</TestVarFirstName><TestVarSurname>Guerrera</TestVarSurname><TestVarTel>0827746117</TestVarTel><Variable></Variable><recset><rec>record1</rec><field>field1</field></recset><recset><rec>record2</rec><field>field2</field></recset><recset><rec>record3</rec><field>field3</field></recset><index>5</index></ADL>";
        //        static string DlShape = @"<ADL>
        //  <TestVarFirstName Description="""" />
        //  <TestVarSurname Description="""" />
        //  <TestVarTel Description="""" />
        //  <Variable Description="""" />
        //  <recset Description="""">
        //    <rec Description="""" />
        //	<field Description="""" />
        //  </recset>
        //  <index Description="""" />
        //</ADL>";


        //        IRecordsetScopingObject recsetScopingObj = DataListFactory.CreateRecordsetScopingObject(DlShape, currentDl);


        //        #endregion Test Variables

        //        private TestContext testContextInstance;

        //        /// <summary>
        //        ///Gets or sets the test context which provides
        //        ///information about and functionality for the current test run.
        //        ///</summary>
        //        public TestContext TestContext {
        //            get {
        //                return testContextInstance;
        //            }
        //            set {
        //                testContextInstance = value;
        //            }
        //        }

        //        #region Additional test attributes
        //        //
        //        // You can use the following additional attributes as you write your tests:
        //        //
        //        // Use ClassInitialize to run code before running the first test in the class
        //        // [ClassInitialize()]
        //        // public static void MyClassInitialize(TestContext testContext) { }
        //        //
        //        // Use ClassCleanup to run code after all tests in a class have run
        //        // [ClassCleanup()]
        //        // public static void MyClassCleanup() { }
        //        //
        //        // Use TestInitialize to run code before running each test 
        //        // [TestInitialize()]
        //        // public void MyTestInitialize() { }
        //        //
        //        // Use TestCleanup to run code after each test has run
        //        // [TestCleanup()]
        //        // public void MyTestCleanup() { }
        //        //
        //        #endregion

        //        #region Positive Test

        //        [TestMethod]
        //        public void GenerateInputRage_RecordsetWithValidField_Expected_PopulatedList() {
        //            IFindRecsetOptions opt = FindRecsetOptions.FindMatch("Equal");
        //            IRecsetSearch to = DataListFactory.CreateSearchTO("[[recset().rec]]", "Equal", "", "");
        //            Func<IList<RecordSetSearchPayload>> fn = opt.GenerateInputRange(to, recsetScopingObj);

        //            IList<RecordSetSearchPayload> result = fn.Invoke();

        //            Assert.IsTrue(result.Count == 3);
        //        }

        //        [TestMethod]
        //        public void GenerateInputRage_RecordsetWithValidFields_Expected_PopulatedList() {
        //            IFindRecsetOptions opt = FindRecsetOptions.FindMatch("Equal");
        //            IRecsetSearch to = DataListFactory.CreateSearchTO("[[recset().rec]],[[recset().field]]", "Equal", "", "");
        //            Func<IList<RecordSetSearchPayload>> fn = opt.GenerateInputRange(to, recsetScopingObj);

        //            IList<RecordSetSearchPayload> result = fn.Invoke();

        //            Assert.IsTrue(result.Count == 6);
        //        }

        //        [TestMethod]
        //        public void GenerateInputRage_ValidStartIndex_Expected_PopulatedList() {
        //            IFindRecsetOptions opt = FindRecsetOptions.FindMatch("Equal");
        //            IRecsetSearch to = DataListFactory.CreateSearchTO("[[recset().rec]],[[recset().field]]", "Equal", "", "2", "", false);
        //            Func<IList<RecordSetSearchPayload>> fn = opt.GenerateInputRange(to, recsetScopingObj);

        //            IList<RecordSetSearchPayload> result = fn.Invoke();

        //            Assert.IsTrue(result.Count == 4);
        //        }

        //        [TestMethod]
        //        public void GenerateInputRage_RecordsetWithNoField_Expected_PopulatedList()
        //        {
        //            IFindRecsetOptions opt = FindRecsetOptions.FindMatch("Equal");
        //            IRecsetSearch to = DataListFactory.CreateSearchTO("[[recset()]]", "Equal", "", "2", "", false);
        //            Func<IList<RecordSetSearchPayload>> fn = opt.GenerateInputRange(to, recsetScopingObj);

        //            IList<RecordSetSearchPayload> result = fn.Invoke();

        //            Assert.IsTrue(result.Count == 6);
        //        }

        //        #endregion

        //        #region Negative Test
        //        [TestMethod]
        //        public void GenerateInputRage_InvalidStartIndex_ExpectEmptyList() {
        //            IFindRecsetOptions opt = FindRecsetOptions.FindMatch("Equal");
        //            IRecsetSearch to = DataListFactory.CreateSearchTO("[[recset().rec]]", "Equal", "", "4", "", false);
        //            Func<IList<RecordSetSearchPayload>> fn = opt.GenerateInputRange(to, recsetScopingObj);

        //            IList<RecordSetSearchPayload> result = fn.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }

        //        [TestMethod]
        //        [ExpectedException(typeof(RecordsetNotFoundException))]
        //        public void GenerateInputRage_RecordsetNotExist_ExpectException() {
        //           IFindRecsetOptions opt = FindRecsetOptions.FindMatch("Equal");
        //           IRecsetSearch to = DataListFactory.CreateSearchTO("[[recset2().rec]]", "Equal", "", "1", "", false);
        //           Func<IList<RecordSetSearchPayload>> fn = opt.GenerateInputRange(to, recsetScopingObj);
        //           IList<RecordSetSearchPayload> result = fn.Invoke();
        //        }

        //        [TestMethod]
        //        public void GenerateInputRage_FieldsNotExist_ExpectEmptyList() {
        //            IFindRecsetOptions opt = FindRecsetOptions.FindMatch("Equal");
        //            IRecsetSearch to = DataListFactory.CreateSearchTO("[[recset().rec2]]", "Equal", "", "1", "", false);
        //            Func<IList<RecordSetSearchPayload>> fn = opt.GenerateInputRange(to, recsetScopingObj);

        //            IList<RecordSetSearchPayload> result = fn.Invoke();

        //            Assert.IsTrue(result.Count == 0);
        //        }

        //        [TestMethod]
        //        [ExpectedException(typeof(RecordsetNotFoundException))]
        //        public void GenerateInputRage_FieldsEmptyString_ExpectException() {
        //            IFindRecsetOptions opt = FindRecsetOptions.FindMatch("Equal");
        //            IRecsetSearch to = DataListFactory.CreateSearchTO("", "Equal", "", "1", "", false);
        //            Func<IList<RecordSetSearchPayload>> fn = opt.GenerateInputRange(to, recsetScopingObj);
        //            IList<RecordSetSearchPayload> result = fn.Invoke();
        //        }

        //        [TestMethod]
        //        public void GenerateInputRage_NullScopingObject_ExpectEmptyList() {
        //            IFindRecsetOptions opt = FindRecsetOptions.FindMatch("Equal");
        //            IRecsetSearch to = DataListFactory.CreateSearchTO("", "Equal", "", "1", "", false);
        //            Func<IList<RecordSetSearchPayload>> fn = opt.GenerateInputRange(to, null);

        //            try {
        //                IList<RecordSetSearchPayload> result = fn.Invoke();
        //                Assert.Fail();
        //            }
        //            catch (Exception e) {
        //                Assert.AreEqual("Object reference not set to an instance of an object.", e.Message);
        //            }
        //        }

        //        #endregion

    }
}
