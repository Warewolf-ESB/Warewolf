/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using Dev2.Common.Wrappers;
using Dev2.Session;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.DataList
{
    [TestClass]
    public class Dev2StudioSessionBrokerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        static void DeleteDir(string rootFolder)
        {
            if (Directory.Exists(rootFolder + @"\Dev2\"))
            {
                var dir = new DirectoryWrapper();
                dir.CleanUp(rootFolder + @"\Dev2\");
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2StudioSessionBroker))]
        [DoNotParallelize]
        [TestCategory("CannotParallelize")]
        public void Dev2StudioSessionBroker_InitSessionWithNoDataAndNullBaseDirectory()
        {
            var serviceName = "DummyService" + Guid.NewGuid();
            var to = new DebugTO();
            var rootFolder = Guid.NewGuid().ToString();
            if (Path.GetTempPath() != "C:\\WINDOWS\\TEMP")
            {
                rootFolder = Path.GetTempPath() + rootFolder;
            }
            else
            {
                rootFolder = "C:\\WINDOWS\\system32\\config\\systemprofile\\AppData\\Local\\TEMP" + rootFolder;
            }

            var broker = Dev2StudioSessionFactory.CreateBroker();
            to.RememberInputs = true;
            to.BaseSaveDirectory = null;
            to.DataList = "<DataList><scalar1/><rs><f1/><f2/></rs></DataList>";
            to.XmlData = "<DataList><scalar1>s1</scalar1><rs><f1/>f1Value<f2/>f2Value</rs></DataList>";
            to.ServiceName = serviceName;
            to.WorkflowID = serviceName;
            broker.InitDebugSession(to);
            to = broker.PersistDebugSession(to);

            Assert.IsTrue(rootFolder.Contains(to.BaseSaveDirectory), "\"" + rootFolder + "\" does not contain: \"" + to.BaseSaveDirectory + "\"");
            DeleteDir(rootFolder);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2StudioSessionBroker))]
        public void Dev2StudioSessionBroker_InitSessionWithNoDataBaseDirectoryIsNullStillInitialises()
        {
            var serviceName = "DummyService" + Guid.NewGuid();
            var to = new DebugTO();
            var rootFolder = Path.GetTempPath() + Guid.NewGuid();
            var broker = Dev2StudioSessionFactory.CreateBroker();
            to.RememberInputs = true;
            to.BaseSaveDirectory = null;
            to.DataList = "<DataList><scalar1/><rs><f1/><f2/></rs></DataList>";
            to.XmlData = "<DataList><scalar1>s1</scalar1><rs><f1/>f1Value<f2/>f2Value</rs></DataList>";
            to.ServiceName = serviceName;
            to.WorkflowID = serviceName;
            broker.InitDebugSession(to);
            to = broker.PersistDebugSession(to);
            to.BaseSaveDirectory = null;
            var p = new Warewolf.Testing.PrivateObject(broker);
            var field = p.GetField("_debugPersistSettings") as IDictionary<string, DebugTO>;
            Assert.IsNotNull(field);
            field.Add("bob", new DebugTO());
            to = broker.PersistDebugSession(to);

            Assert.AreEqual(string.Empty, to.Error);
            Assert.IsNotNull(to.BinaryDataList);

            DeleteDir(rootFolder);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2StudioSessionBroker))]
        public void Dev2StudioSessionBroker_InitSessionWithSingleScalar()
        {
            var serviceName = "DummyService" + Guid.NewGuid();
            var to = new DebugTO();
            var rootFolder = Path.GetTempPath() + Guid.NewGuid();
            var broker = Dev2StudioSessionFactory.CreateBroker();
            to.RememberInputs = true;
            to.BaseSaveDirectory = rootFolder;
            to.DataList = "<DataList><scalar1/><rs><f1/><f2/></rs></DataList>";
            to.XmlData = "<DataList><scalar1>s1</scalar1></DataList>";
            to.ServiceName = serviceName;
            to.WorkflowID = serviceName;
            broker.InitDebugSession(to);
            to = broker.PersistDebugSession(to);

            const string expected = "<DataList><scalar1>s1</scalar1></DataList>";

            Assert.AreEqual(string.Empty, to.Error);
            Assert.AreEqual(expected, to.XmlData, "Got  [ " + to.XmlData + "] , but expected [ " + expected + " ]");

            DeleteDir(rootFolder);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2StudioSessionBroker))]
        public void Dev2StudioSessionBroker_PersistSessionWithSavedData_ExpectSavedData()
        {
            var serviceName = "DummyService" + Guid.NewGuid();
            var to = new DebugTO();
            var rootFolder = Path.GetTempPath() + Guid.NewGuid();
            var broker = Dev2StudioSessionFactory.CreateBroker();
            to.RememberInputs = true;
            to.BaseSaveDirectory = rootFolder;
            to.DataList = "<DataList><scalar1/><rs><f1/><f2/></rs></DataList>";
            to.XmlData = "<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>";
            to.ServiceName = serviceName;
            to.WorkflowID = serviceName;
            to = broker.InitDebugSession(to);
            to = broker.PersistDebugSession(to);

            Assert.AreEqual("<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>", to.XmlData);

            DeleteDir(rootFolder);

            broker.Dispose();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2StudioSessionBroker))]
        public void Dev2StudioSessionBroker_PersistSessionWithSavedData_ChangedDataList_ExpectPreviousXmlData()
        {
            var serviceName = "DummyService" + Guid.NewGuid();
            var to = new DebugTO();
            var rootFolder = Path.GetTempPath() + Guid.NewGuid();
            var broker = Dev2StudioSessionFactory.CreateBroker();
            to.RememberInputs = true;
            to.BaseSaveDirectory = rootFolder;
            to.DataList = "<DataList><scalar1/><rs><f1/><f2/></rs></DataList>";
            to.XmlData = "<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>";
            to.ServiceName = serviceName;
            to.WorkflowID = serviceName;
            to = broker.InitDebugSession(to);
            to = broker.PersistDebugSession(to);

            Assert.AreEqual("<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>", to.XmlData);

            DeleteDir(rootFolder);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2StudioSessionBroker))]
        public void Dev2StudioSessionBrokerNotPersistSessionWithSavedData_ExpectEmptyDataList()
        {
            var serviceName = "DummyService" + Guid.NewGuid();
            var to = new DebugTO();
            var broker = Dev2StudioSessionFactory.CreateBroker();
            var rootFolder = Path.GetTempPath() + Guid.NewGuid();
            to.RememberInputs = false;
            to.BaseSaveDirectory = rootFolder;
            to.DataList = "<DataList><scalar1/><rs><f1/><f2/></rs></DataList>";
            to.XmlData = "";
            to.ServiceName = serviceName;
            to.WorkflowID = serviceName;
            to = broker.InitDebugSession(to);
            to.XmlData = "<DataList><scalar1>s1</scalar1><rs><f1>f1Value</f1><f2>f2Value</f2></rs></DataList>";
            to = broker.PersistDebugSession(to);
            to.XmlData = "";
            to = broker.InitDebugSession(to);

            Assert.AreEqual("<DataList></DataList>", to.XmlData);

            DeleteDir(rootFolder);
        }
        
        [TestMethod]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(Dev2StudioSessionBroker))]
        public void Dev2StudioSessionBroker_PersistSessionWithSavedData_RecordSet()
        {
            var to = new DebugTO();
            var rootFolder = Path.GetTempPath() + Guid.NewGuid();
            var broker = Dev2StudioSessionFactory.CreateBroker();
            to.RememberInputs = true;
            to.BaseSaveDirectory = rootFolder;
            to.DataList = "<DataList><rec><name></name><age></age></rec></DataList>";
            to.XmlData = "<DataList><rec><name>Bob</name><age></age></rec><rec><name>Bob</name><age>30</age></rec></DataList>";
            to.ServiceName = "DummyService";
            to.WorkflowID = "DummyService";
            to = broker.InitDebugSession(to);
            to = broker.PersistDebugSession(to);

            Assert.AreEqual("<DataList><rec><name>Bob</name><age></age></rec><rec><name>Bob</name><age>30</age></rec></DataList>", to.XmlData);

            DeleteDir(rootFolder);
        }
    }
}
