using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Technical_Assesment;
using Technical_Assesment.File_Import;
using Technical_Assesment.Sorting;
using Technical_Assesment.Value_Objects;

namespace UnitTestProject1
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class RecordImporterTest
    {
        public RecordImporterTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

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
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void CanImportFileWithNameAndAgeSort()
        {
            IRecordImporter importer = new RecordImporter();

            PersonBuilder pb = new PersonBuilder();

            ISortable<Person> routine = new PersonAgeSort();

            BTree<Person> seedTree = new BTree<Person>(routine);

            BTree<Person> result = importer.ImportRecords(@"F:\foo\records.txt", ',', true, pb, seedTree);

            Assert.AreEqual(49998, result.NodeCount);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExcpetionThrownWithNameSort()
        {
            IRecordImporter importer = new RecordImporter();

            PersonBuilder pb = new PersonBuilder();

            ISortable<Person> routine = new PersonNameSort();

            BTree<Person> seedTree = new BTree<Person>(routine);

            BTree<Person> result = importer.ImportRecords(@"F:\foo\records.txt", ',', true, pb, seedTree);

            Assert.AreEqual(49998, result.NodeCount);

        }
    }
}
