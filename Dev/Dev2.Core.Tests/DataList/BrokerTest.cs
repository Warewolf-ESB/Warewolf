using System;
using System.IO;
using Dev2.Common.Common;
using Dev2.Session;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.DataList
{
    /// <summary>
    /// Summary description for BrokerTest
    /// </summary>
    [TestClass]    
    public class BrokerTest
    {

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attribute
        
        private void DeleteDir(string rootFolder)
        {
            if(Directory.Exists(rootFolder + @"\Dev2\"))
            {
                DirectoryHelper.CleanUp(rootFolder + @"\Dev2\");
            }
        }

        #endregion

        #region InitSession Tests

        [TestMethod]
        public void InitSessionWithNoData()
        {
            DebugTO to = new DebugTO();
            string rootFolder = Path.GetTempPath() + Guid.NewGuid();
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
            Assert.IsNotNull(to.BinaryDataList); // assert not null hence we created it ;)

            DeleteDir(rootFolder);
        }

        [TestMethod]
        public void InitSessionWithSingleScalar()
        {
            DebugTO to = new DebugTO();
            string rootFolder = Path.GetTempPath() + Guid.NewGuid();
            IDev2StudioSessionBroker broker = Dev2StudioSessionFactory.CreateBroker();
            to.RememberInputs = true;
            to.BaseSaveDirectory = rootFolder;
            to.DataList = "<DataList><scalar1/><rs><f1/><f2/></rs></DataList>";
            to.XmlData = "<DataList><scalar1>s1</scalar1></DataList>";
            to.ServiceName = "DummyService";
            to.WorkflowID = "DummyService";
            broker.InitDebugSession(to);
            to = broker.PersistDebugSession(to);

            const string expected = "<DataList><scalar1>s1</scalar1></DataList>";

            // just ensure the operation worked successfully with no errors
            Assert.AreEqual(string.Empty, to.Error);
            Assert.AreEqual(expected, to.XmlData, "Got  [ " + to.XmlData + "] , but expected [ " + expected + " ]");

            DeleteDir(rootFolder);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void PersistSessionWithSavedData_ExpectSavedData()
        // ReSharper restore InconsistentNaming
        {
            //DeleteDir();
            // bootstrap
            DebugTO to = new DebugTO();
            string rootFolder = Path.GetTempPath() + Guid.NewGuid();
            IDev2StudioSessionBroker broker = Dev2StudioSessionFactory.CreateBroker();
            to.RememberInputs = true;
            to.BaseSaveDirectory = rootFolder;
            to.DataList = "<DataList><scalar1/><rs><f1/><f2/></rs></DataList>";
            to.XmlData = "<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>";
            to.ServiceName = "DummyService";
            to.WorkflowID = "DummyService";
            to = broker.InitDebugSession(to);
            to = broker.PersistDebugSession(to);

            Assert.AreEqual("<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>", to.XmlData);

            DeleteDir(rootFolder);
        }

        #endregion InitSession Tests

        #region PersistSession Test

        // ReSharper disable InconsistentNaming

        //Bug 7842 2013.01.29: Ashley Lewis - Altered expected and assert from PreviousXmlData to MergedXmlData
        [TestMethod]

        public void PersistSessionWithSavedData_ChangedDataList_Expect_MergedXmlData()
        {
            // bootstrap
            DebugTO to = new DebugTO();
            string rootFolder = Path.GetTempPath() + Guid.NewGuid();
            IDev2StudioSessionBroker broker = Dev2StudioSessionFactory.CreateBroker();
            to.RememberInputs = true;
            to.BaseSaveDirectory = rootFolder;
            to.DataList = "<DataList><scalar1/><rs><f1/><f2/></rs></DataList>";
            to.XmlData = "<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>";
            to.ServiceName = "DummyService";
            to.WorkflowID = "DummyService";
            to = broker.InitDebugSession(to);
            to = broker.PersistDebugSession(to);

            // just ensure the operation worked successfully with no errors
            to.DataList = "<DataList><rs><field ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs></DataList>";
            to = broker.InitDebugSession(to);

            Assert.AreEqual("<DataList><rs><field></field><f2>f2Value</f2></rs></DataList>", to.XmlData);
            Assert.IsNotNull(to.BinaryDataList); // assert not null binary datalist since it is used to create the input model ;)

            DeleteDir(rootFolder);
        }

        [TestMethod]
        public void PersistSessionWithAlotOfSavedData_RadicallyChangedDataList_Expect_MergedXmlData()
        {
            // bootstrap
            DebugTO to = new DebugTO();
            string rootFolder = Path.GetTempPath() + Guid.NewGuid();
            IDev2StudioSessionBroker broker = Dev2StudioSessionFactory.CreateBroker();
            to.RememberInputs = true;
            to.BaseSaveDirectory = rootFolder;
            to.DataList = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            to.XmlData = "<DataList><scalar1>s1</scalar1><persistantscalar>SomeValue</persistantscalar><rs><f1>f1Value</f1><f2>f2Value</f2></rs><recset><field1>somedata</field1><field2>moredata</field2></recset><recset><field1></field1><field2></field2></recset></DataList>";
            to.ServiceName = "DummyService";
            to.WorkflowID = "DummyService";
            to = broker.InitDebugSession(to);
            to = broker.PersistDebugSession(to);

            // just ensure the operation worked successfully with no errors
            to.DataList = "<DataList><persistantscalar ColumnIODirection=\"Input\"/><rs><field ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><newfield ColumnIODirection=\"Input\"/><field1 ColumnIODirection=\"Input\"/><changedfieldname ColumnIODirection=\"Input\"/></recset></DataList>";
            to = broker.InitDebugSession(to);

            Assert.AreEqual("<DataList><persistantscalar>SomeValue</persistantscalar><rs><field></field><f2>f2Value</f2></rs><recset><newfield></newfield><field1>somedata</field1><changedfieldname></changedfieldname></recset><recset><newfield></newfield><field1></field1><changedfieldname></changedfieldname></recset></DataList>", to.XmlData);

            DeleteDir(rootFolder);
        }

        // BUG : Removing field names from datalist does not retain current datalist values
        [TestMethod]
        public void PersistSessionWithSavedData_SubtlyChangedDataList_Expect_MergedXmlData()
        {
            // bootstrap
            DebugTO to = new DebugTO();
            string rootFolder = Path.GetTempPath() + Guid.NewGuid();
            IDev2StudioSessionBroker broker = Dev2StudioSessionFactory.CreateBroker();
            to.RememberInputs = true;
            to.BaseSaveDirectory = rootFolder;
            to.DataList = "<DataList><rs><f1/><f2/></rs></DataList>";
            to.XmlData = "<DataList><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>";
            to.ServiceName = "DummyService";
            to.WorkflowID = "DummyService";
            to = broker.InitDebugSession(to);
            to = broker.PersistDebugSession(to);

            // just ensure the operation worked successfully with no errors
            to.DataList = "<DataList><rs><f2 ColumnIODirection=\"Input\"/></rs></DataList>";
            to = broker.InitDebugSession(to);

            Assert.AreEqual("<DataList><rs><f2>f2Value</f2></rs></DataList>", to.XmlData);

            DeleteDir(rootFolder);
        }

        [TestMethod]
        public void PersistSessionWithSavedData_ChangedDataList_ExpectPreviousXmlData()
        {
            // bootstrap
            DebugTO to = new DebugTO();
            string rootFolder = Path.GetTempPath() + Guid.NewGuid();
            IDev2StudioSessionBroker broker = Dev2StudioSessionFactory.CreateBroker();
            to.RememberInputs = true;
            to.BaseSaveDirectory = rootFolder;
            to.DataList = "<DataList><scalar1/><rs><f1/><f2/></rs></DataList>";
            to.XmlData = "<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>";
            to.ServiceName = "DummyService";
            to.WorkflowID = "DummyService";
            to = broker.InitDebugSession(to);
            to = broker.PersistDebugSession(to);

            Assert.AreEqual("<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>", to.XmlData);

            DeleteDir(rootFolder);
        }
        

        [TestMethod]
        public void NotPersistSessionWithSavedData_ExpectEmptyDataList()
        {
            // bootstrap
            DebugTO to = new DebugTO();
            IDev2StudioSessionBroker broker = Dev2StudioSessionFactory.CreateBroker();
            string rootFolder = Path.GetTempPath() + Guid.NewGuid();
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

        // ReSharper restore InconsistentNaming
    }
}
