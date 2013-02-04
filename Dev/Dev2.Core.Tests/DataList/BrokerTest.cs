using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Dev2.Session;

namespace Unlimited.UnitTest.Framework.DataList
{
    /// <summary>
    /// Summary description for BrokerTest
    /// </summary>
    [TestClass]
    public class BrokerTest {

        #region Test Variables
        //private IDev2StudioSessionBroker broker = Dev2StudioSessionFactory.CreateBroker();

        #endregion Test Variables

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

        #region Additional test attributee
        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup() {
        }

        
        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize() {

        }

        private void DeleteDir(string rootFolder)
        {
            if (Directory.Exists(rootFolder + @"\Dev2\"))
            {
                Directory.Delete(rootFolder + @"\Dev2\", true);
            }
        }
        #endregion

        #region InitSession Tests

        [TestMethod]
        public void InitSessionWithSavedData()
        {
            //
            // TODO: Add test logic here
            //
        }

        [TestMethod]
        public void InitSessionWithNoData()
        {
            DebugTO to = new DebugTO();
            string rootFolder = System.IO.Path.GetTempPath() + Guid.NewGuid();
            IDev2StudioSessionBroker broker = Dev2StudioSessionFactory.CreateBroker();
            to.RememberInputs = true;
            to.BaseSaveDirectory = rootFolder;
            to.DataList = "<DataList><scalar1/><rs><f1/><f2/></rs></DataList>";
            to.XmlData = "<DataList><scalar1>s1</scalar1><rs><f1/>f1Value<f2/>f2Value</rs></DataList>";
            to.ServiceName = "DummyService";
            to.WorkflowID = "DummyService";
            broker.InitDebugSession(to);
            to = broker.PersistDebugSession(to);

            // just ensure the operation worked successfully with no errors
            Assert.AreEqual(string.Empty, to.Error);

            DeleteDir(rootFolder);
        }

        [TestMethod]
        public void PersistSessionWithSavedData_ExpectSavedData()
        {
            //DeleteDir();
            // bootstrap
            DebugTO to = new DebugTO();
            string rootFolder = System.IO.Path.GetTempPath() + Guid.NewGuid();
            IDev2StudioSessionBroker broker = Dev2StudioSessionFactory.CreateBroker();
            to.RememberInputs = true;
            to.BaseSaveDirectory = rootFolder;
            to.DataList = "<DataList><scalar1/><rs><f1/><f2/></rs></DataList>";
            to.XmlData = "<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>";
            to.ServiceName = "DummyService";
            to.WorkflowID = "DummyService";
            to = broker.InitDebugSession(to);
            to = broker.PersistDebugSession(to);

        //    // just ensure the operation worked successfully with no errors

        //    to.XmlData = "";

        //    to = broker.InitDebugSession(to);

            Assert.AreEqual("<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>", to.XmlData);

            DeleteDir(rootFolder);
        }

        #endregion InitSession Tests

        #region PersistSession Test

        //Bug 7842 2013.01.29: Ashley Lewis - Altered expected and assert from PreviousXmlData to MergedXmlData
        //[TestMethod]
        //public void PersistSessionWithSavedData_ChangedDataList_Expect_MergedXmlData()
        //{
        //    // bootstrap
        //    DebugTO to = new DebugTO();
        //    string rootFolder = System.IO.Path.GetTempPath() + Guid.NewGuid();
        //    IDev2StudioSessionBroker broker = Dev2StudioSessionFactory.CreateBroker();
        //    to.RememberInputs = true;
        //    to.BaseSaveDirectory = rootFolder;
        //    to.DataList = "<DataList><scalar1/><rs><f1/><f2/></rs></DataList>";
        //    to.XmlData = "<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>";
        //    to.ServiceName = "DummyService";
        //    to.WorkflowID = "DummyService";
        //    to = broker.InitDebugSession(to);
        //    to = broker.PersistDebugSession(to);

        //    // just ensure the operation worked successfully with no errors
        //    to.DataList = "<DataList><rs><field/><f2/></rs></DataList>";
        //    to = broker.InitDebugSession(to);

        //    Assert.AreEqual("<DataList><rs><field></field><f2>f2Value</f2></rs></DataList>", to.XmlData);

        //    DeleteDir(rootFolder);
        //}

        //[TestMethod]
        //public void PersistSessionWithAlotOfSavedData_RadicallyChangedDataList_Expect_MergedXmlData()
        //{
        //    // bootstrap
        //    DebugTO to = new DebugTO();
        //    string rootFolder = System.IO.Path.GetTempPath() + Guid.NewGuid();
        //    IDev2StudioSessionBroker broker = Dev2StudioSessionFactory.CreateBroker();
        //    to.RememberInputs = true;
        //    to.BaseSaveDirectory = rootFolder;
        //    to.DataList = "<DataList><scalar1/><persistantscalar/><rs><f1/><f2/></rs><recset><field1/><field2/></recset></DataList>";
        //    to.XmlData = "<DataList><scalar1>s1</scalar1><persistantscalar>SomeValue</persistantscalar><rs><f1>f1Value</f1><f2>f2Value</f2></rs><recset><field1>somedata</field1><field2>moredata</field2></recset><recset><field1></field1><field2></field2></recset></DataList>";
        //    to.ServiceName = "DummyService";
        //    to.WorkflowID = "DummyService";
        //    to = broker.InitDebugSession(to);
        //    to = broker.PersistDebugSession(to);

        //    // just ensure the operation worked successfully with no errors
        //    to.DataList = "<DataList><persistantscalar/><rs><field/><f2/></rs><recset><newfield/><field1/><changedfieldname/></recset></DataList>";
        //    to = broker.InitDebugSession(to);

        //    Assert.AreEqual("<DataList><persistantscalar>SomeValue</persistantscalar><rs><field></field><f2>f2Value</f2></rs><recset><newfield></newfield><field1>somedata</field1><changedfieldname></changedfieldname></recset><recset><newfield></newfield><field1></field1><changedfieldname></changedfieldname></recset></DataList>", to.XmlData);

        //    DeleteDir(rootFolder);
        //}

        // Bug

        // This test is actually broken - Not just non-thread-safe :p

        
        [TestMethod]
        public void PersistSessionWithSavedData_ToDefaultLocation_ChangedDataList_ExpectPreviousXmlData()
        {
            // bootstrap
            DebugTO to = new DebugTO();
            IDev2StudioSessionBroker broker = Dev2StudioSessionFactory.CreateBroker();
            to.RememberInputs = true;
            to.DataList = "<DataList><scalar1/><rs><f1/><f2/></rs></DataList>";
            to.XmlData = "<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>";
            to.ServiceName = "DummyService";
            to.WorkflowID = "DummyService";
            to = broker.InitDebugSession(to);
            to = broker.PersistDebugSession(to);

            Assert.AreEqual("<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>", to.XmlData);

        }
        

        [TestMethod]
        public void NotPersistSessionWithSavedData_ExpectEmptyDataList()
        {
            // bootstrap
            DebugTO to = new DebugTO();
            IDev2StudioSessionBroker broker = Dev2StudioSessionFactory.CreateBroker();
            string rootFolder = System.IO.Path.GetTempPath() + Guid.NewGuid();
            to.RememberInputs = false;
            to.BaseSaveDirectory = rootFolder;
            to.DataList = "<DataList><scalar1/><rs><f1/><f2/></rs></DataList>";
            to.XmlData = "";
            to.ServiceName = "DummyService";
            to.WorkflowID = "DummyService";
            to = broker.InitDebugSession(to);
            to.XmlData = "<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>";
            to = broker.PersistDebugSession(to);
            to.XmlData = "";
            to = broker.InitDebugSession(to);

            Assert.AreEqual("<DataList><scalar1/><rs><f1/><f2/></rs></DataList>", to.XmlData);

            DeleteDir(rootFolder);
        }

        #endregion PersistSession Tests
    }
}
