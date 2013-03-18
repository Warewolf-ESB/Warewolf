using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;
using Dev2.Integration.Tests.MEF;
using System.IO;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests
{
    [TestClass]
    public class DeleteResourceTest
    {
        private readonly string _webserverURI = ServerSettings.WebserverURI;
        private const string _initialDeleteResourceServiceXmlString = "<XmlData><Service></Service><ResourceName></ResourceName><ResourceType></ResourceType><Roles></Roles></XmlData>";
        private static ServerFabricationFactory _serverFactory;
        private object _lock = new object();

        //private static ServerFabricationFactory _serverFactory;
        private DataListValueInjector _dataListValueInjector = new DataListValueInjector();

        [ClassCleanup]
        public static void Cleanup()
        {
            if (_serverFactory != null)
            {
                _serverFactory.Dispose();
                _serverFactory = null;
            }
        }

        private TestContext _context;

        public TestContext TestContext { get { return _context; } set { _context = value; } }


        [TestMethod]
        public void DeleteWorkflowSuccess_FileIsDeleted_Test()
        {
            lock(_lock)
            {
                _serverFactory = new ServerFabricationFactory();
                //conn.DataChannel.ExecuteCommand()   
                var serviceName = "DeleteWorkflowTest2";
                string xmlString = BuildDeleteRequestXml(serviceName, "WorkflowService");
                string postData = String.Format("{0}{1}?{2}", _serverFactory.ServerAddress, "DeleteResourceService", xmlString);

                using(ServerFabrication fabrication = _serverFactory.CreateFabrication())
                {
                    using(ServerExecutionInstance execution = fabrication.Execute())
                    {
                        string[] initialFiles = Directory.GetFiles(fabrication.GetCommonDirectoryPath(ServerCommonDirectory.Services));
                        var originalcount = initialFiles.Count(s => s.Contains(serviceName));

                        //string versionDirectory = Path.Combine(, "VersionControl");
                        string actualResult = TestHelper.PostDataToWebserver(postData);

                        Assert.AreEqual(true, actualResult.Contains("Success"));

                        initialFiles = Directory.GetFiles(fabrication.GetCommonDirectoryPath(ServerCommonDirectory.Services));
                        var newcount = initialFiles.Count(s => s.Contains(serviceName));
                        Assert.IsTrue(originalcount == newcount + 1);
                    }
                }
            }
        }
        
        [TestMethod]
        public void DeleteWorkflowSuccess_BackupIsCreated_Test()
        {
            lock(_lock)
            {
                _serverFactory = new ServerFabricationFactory();
                var serviceName = "DeleteWorkflowTest2";
                string xmlString = BuildDeleteRequestXml(serviceName, "WorkflowService");
                string postData = String.Format("{0}{1}?{2}", _serverFactory.ServerAddress, "DeleteResourceService", xmlString);

                using(ServerFabrication fabrication = _serverFactory.CreateFabrication())
                {
                    using(ServerExecutionInstance execution = fabrication.Execute())
                    {
                        string versionDirectory = Path.Combine(fabrication.GetCommonDirectoryPath(ServerCommonDirectory.Services), "VersionControl");

                        int initialCount = 0;
                        string[] initialFiles;

                        if (Directory.GetDirectories(fabrication.GetCommonDirectoryPath(ServerCommonDirectory.Services))
                                    .Contains(versionDirectory))
                        {
                            initialFiles = Directory.GetFiles(versionDirectory);

                            for (int i = 0; i < initialFiles.Length; i++)
                                if (initialFiles[i].Contains(serviceName + ".V"))
                                    initialCount++;
                        }

                        string actualResult = TestHelper.PostDataToWebserver(postData);

                        if (!Directory.GetDirectories(fabrication.GetCommonDirectoryPath(ServerCommonDirectory.Services))
                                     .Contains(versionDirectory))
                        {
                            Assert.Fail("Directory not created.");
                        }

                        initialFiles = Directory.GetFiles(versionDirectory);
                        int postCount = 0;

                        for (int i = 0; i < initialFiles.Length; i++)
                            if (initialFiles[i].Contains(serviceName + ".V"))
                                postCount++;

                        Assert.IsTrue(postCount == initialCount + 1);
                    }
                }
            }
        }

        [TestMethod]
        public void DeleteWorkflowSuccess_CantCallDeletedWorkflow_Test()
        {
            lock(_lock)
            {
                _serverFactory = new ServerFabricationFactory();
                using(ServerFabrication fabrication = _serverFactory.CreateFabrication())
                {
                    using(ServerExecutionInstance execution = fabrication.Execute())
                    {
                        var serviceName = "DeleteWorkflowTest2";
                        string postData = String.Format("{0}{1}", _serverFactory.ServerAddress, serviceName);
                        string actualResult = TestHelper.PostDataToWebserver(postData);
                        Assert.AreEqual(true, actualResult.Contains("DeleteWorkflowTest2"));

                        string xmlString = BuildDeleteRequestXml(serviceName, "WorkflowService");
                        postData = String.Format("{0}{1}?{2}", _serverFactory.ServerAddress, "DeleteResourceService", xmlString);
                        actualResult = TestHelper.PostDataToWebserver(postData);

                        postData = String.Format("{0}{1}", _serverFactory.ServerAddress, serviceName);
                        actualResult = TestHelper.PostDataToWebserver(postData);
                        string expectedNotFound = "Error: Service not found in the catalog";
                        actualResult = actualResult.Replace("\r", "").Replace("\n", "");
                        StringAssert.Contains(actualResult, expectedNotFound);
                    }
                }
            }
        }

        [TestMethod]
        public void DeleteWorkflowInvalidType_CanCallWorkflow_Test()
        {
            lock(_lock)
            {
                _serverFactory = new ServerFabricationFactory();
                using (ServerFabrication fabrication = _serverFactory.CreateFabrication())
                {
                    using (ServerExecutionInstance execution = fabrication.Execute())
                    {
                        var serviceName = "DeleteWorkflowTest2";

                        string xmlString = BuildDeleteRequestXml(serviceName, "WorkerService");
                        string postData = String.Format("{0}{1}?{2}", _serverFactory.ServerAddress, "DeleteResourceService", xmlString);
                        string actualResult = TestHelper.PostDataToWebserver(postData);
         
                        postData = String.Format("{0}{1}", _serverFactory.ServerAddress, serviceName);
                        actualResult = TestHelper.PostDataToWebserver(postData);
                        Assert.AreEqual(true, actualResult.Contains("DeleteWorkflowTest2"));
                    }
                }
            }
        }

        private string BuildDeleteRequestXml(string resourceName, string resourceType, string roles = "Domain Admins,Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Business Design Studio Developers,Build Configuration Engineers,Test Engineers,DEV2 Limited Internet Access")
        {
            string xmlString = _dataListValueInjector.InjectDataListValue(_initialDeleteResourceServiceXmlString, "Service", "DeleteResourceService");
            xmlString = _dataListValueInjector.InjectDataListValue(xmlString, "ResourceName", resourceName);
            xmlString = _dataListValueInjector.InjectDataListValue(xmlString, "ResourceType", resourceType);
            xmlString = _dataListValueInjector.InjectDataListValue(xmlString, "Roles", roles);
            return xmlString;
        }
    }
}
