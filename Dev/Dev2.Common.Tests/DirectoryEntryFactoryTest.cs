using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using System.DirectoryServices;

namespace Dev2.Common.Tests
{
    /// <summary>
    /// Summary description for DirectoryEntryFactoryTest
    /// </summary>
    [TestClass]
    public class DirectoryEntryFactoryTest
    {
        public DirectoryEntryFactoryTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private DirectoryEntryFactory _directoryEntryFactory;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public DirectoryEntryFactory DirectoryEntryFactory
        {
            get
            {
                return _directoryEntryFactory;
            }
            set
            {
                _directoryEntryFactory = value;
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
        [Owner("Siphamandla Dube")]
        [TestCategory("DirectoryEntryTest_Create")]
        public void DirectoryEntryFactoryTest_EntryNameAndMachineName_AreEqual()
        {
            //-----------------Arrage------------------
            var path = "WinNT://" + Environment.MachineName + ",computer";
            IDirectoryEntryFactory _directoryEntryFactory = new DirectoryEntryFactory();
            //-----------------Act------------------
            var entry = _directoryEntryFactory.Create(path);
            //-----------------Assert------------------
            Assert.IsNotNull(entry);
            Assert.AreEqual(Environment.MachineName, entry.Name);
        }

    }
}
