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
using Dev2.Common.Common;
using Dev2.Session;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.DataList
{
    [TestClass]
    public class BrokerTest
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
                var dir = new DirectoryHelper();
                dir.CleanUp(rootFolder + @"\Dev2\");
            }
        }

        [TestMethod]
        public void InitSessionWithNoDataBaseDirectoryIsNull()
        {
            var to = new DebugTO();
            var rootFolder = Path.GetTempPath() + Guid.NewGuid();
            var broker = Dev2StudioSessionFactory.CreateBroker();
            to.RememberInputs = true;
            to.BaseSaveDirectory = null;
            to.DataList = "<DataList><scalar1/><rs><f1/><f2/></rs></DataList>";
            to.XmlData = "<DataList><scalar1>s1</scalar1><rs><f1/>f1Value<f2/>f2Value</rs></DataList>";
            to.ServiceName = "DummyService";
            to.WorkflowID = "DummyService";
            broker.InitDebugSession(to);
            to = broker.PersistDebugSession(to);

            Assert.IsTrue(rootFolder.Contains(to.BaseSaveDirectory));
            DeleteDir(rootFolder);
        }

        [TestMethod]
        public void InitSessionWithNoDataBaseDirectoryIsNullStillInitialises()
        {
            var to = new DebugTO();
            var rootFolder = Path.GetTempPath() + Guid.NewGuid();
            var broker = Dev2StudioSessionFactory.CreateBroker();
            to.RememberInputs = true;
            to.BaseSaveDirectory = null;
            to.DataList = "<DataList><scalar1/><rs><f1/><f2/></rs></DataList>";
            to.XmlData = "<DataList><scalar1>s1</scalar1><rs><f1/>f1Value<f2/>f2Value</rs></DataList>";
            to.ServiceName = "DummyService";
            to.WorkflowID = "DummyService";
            broker.InitDebugSession(to);
            to = broker.PersistDebugSession(to);
            to.BaseSaveDirectory = null;
            var p = new PrivateObject(broker);
            var field = p.GetField("_debugPersistSettings") as IDictionary<string, DebugTO>;
            Assert.IsNotNull(field);
            field.Add("bob", new DebugTO());
            to = broker.PersistDebugSession(to);

            Assert.AreEqual(string.Empty, to.Error);
            Assert.IsNotNull(to.BinaryDataList); // assert not null hence we created it ;)

            DeleteDir(rootFolder);
        }

        [TestMethod]
        public void InitSessionWithSingleScalar()
        {
            var to = new DebugTO();
            var rootFolder = Path.GetTempPath() + Guid.NewGuid();
            var broker = Dev2StudioSessionFactory.CreateBroker();
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

        public void PersistSessionWithSavedData_ExpectSavedData()
        {
            // bootstrap
            var to = new DebugTO();
            var rootFolder = Path.GetTempPath() + Guid.NewGuid();
            var broker = Dev2StudioSessionFactory.CreateBroker();
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

            broker.Dispose();
        }

        [TestMethod]
        public void PersistSessionWithSavedData_ChangedDataList_ExpectPreviousXmlData()
        {
            // bootstrap
            var to = new DebugTO();
            var rootFolder = Path.GetTempPath() + Guid.NewGuid();
            var broker = Dev2StudioSessionFactory.CreateBroker();
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
            var to = new DebugTO();
            var broker = Dev2StudioSessionFactory.CreateBroker();
            var rootFolder = Path.GetTempPath() + Guid.NewGuid();
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

            Assert.AreEqual("<DataList></DataList>", to.XmlData);

            DeleteDir(rootFolder);
        }
    }
}
