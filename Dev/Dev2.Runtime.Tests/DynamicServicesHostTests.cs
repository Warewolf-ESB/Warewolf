
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.DynamicServices.Test.XML;
using Dev2.Runtime.Security;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.DynamicServices.Test
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DynamicServicesHostTests
    {
        const int VersionNo = 9999;
        const string ServiceName = "Calculate_RecordSet_Subtract";
        const string ServiceNameUnsigned = "TestDecisionUnsigned";

        const string SourceName = "CitiesDatabase";

        public const string ServerConnection1Name = "ServerConnection1";
        public const string ServerConnection1ResourceName = "MyDevServer";
        public const string ServerConnection1ID = "{68F5B4FE-4573-442A-BA0C-5303F828344F}";

        public const string ServerConnection2Name = "ServerConnection2";
        public const string ServerConnection2ResourceName = "MySecondDevServer";
        public const string ServerConnection2ID = "{70238921-FDC7-4F7A-9651-3104EEDA1211}";

        static string _testDir;
        static WorkspaceRepository _testInstance;

        static string _servicesPath;
        static string _sourcesPath;
        static IWorkspace _workspace;

        static string _testServiceDefinition;
        static string _testSourceDefinition;

        #region Class Initialize/Cleanup

        static readonly object InitLock = new object();

        static bool _isInitialized;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            //
            // This is called from other tests e.g. DynamicServicesInvokerTest
            //
            lock(InitLock)
            {
                if(_isInitialized)
                {
                    return;
                }

                Directory.SetCurrentDirectory(testContext.TestDir);
                _testDir = testContext.TestDir;

                var workspaceID = Guid.NewGuid();

                #region Copy server services to file system

                var workspacePath = Path.Combine(GlobalConstants.WorkspacePath, workspaceID.ToString());

                #region Initialize services

                _servicesPath = Path.Combine(workspacePath, "Services");
                var servicesVersionControlPath = Path.Combine(_servicesPath, "VersionControl");
                var serverServicesPath = Path.Combine(_testDir, "Services");

                Directory.CreateDirectory(_servicesPath);
                Directory.CreateDirectory(servicesVersionControlPath);
                Directory.CreateDirectory(serverServicesPath);

                var xml = XmlResource.Fetch(ServiceName);
                xml.Save(Path.Combine(_servicesPath, ServiceName + ".xml"));
                xml.Save(Path.Combine(servicesVersionControlPath, ServiceName + ".V" + VersionNo + ".xml"));
                xml.Save(Path.Combine(serverServicesPath, ServiceName + ".xml"));

                xml = XmlResource.Fetch(ServiceNameUnsigned);
                xml.Save(Path.Combine(_servicesPath, ServiceNameUnsigned + ".xml"));
                xml.Save(Path.Combine(servicesVersionControlPath, ServiceNameUnsigned + ".V" + VersionNo + ".xml"));
                xml.Save(Path.Combine(serverServicesPath, ServiceNameUnsigned + ".xml"));

                _testServiceDefinition = xml.ToString();

                #endregion

                #region Initialize sources

                _sourcesPath = Path.Combine(workspacePath, "Sources");
                var sourcesVersionControlPath = Path.Combine(_sourcesPath, "VersionControl");
                var serverSourcesPath = Path.Combine(_testDir, "Sources");

                Directory.CreateDirectory(_sourcesPath);
                Directory.CreateDirectory(sourcesVersionControlPath);
                Directory.CreateDirectory(serverSourcesPath);

                xml = XmlResource.Fetch(SourceName);
                xml.Save(Path.Combine(_sourcesPath, SourceName + ".xml"));
                xml.Save(Path.Combine(sourcesVersionControlPath, SourceName + ".V" + VersionNo + ".xml"));
                xml.Save(Path.Combine(serverSourcesPath, SourceName + ".xml"));

                _testSourceDefinition = xml.ToString();

                xml = XmlResource.Fetch(ServerConnection1Name);
                xml.Save(Path.Combine(_sourcesPath, ServerConnection1ResourceName + ".xml"));
                xml.Save(Path.Combine(_sourcesPath, ServerConnection1ResourceName + ".xml"));
                xml.Save(Path.Combine(sourcesVersionControlPath, ServerConnection1ResourceName + ".V" + VersionNo + ".xml"));
                xml.Save(Path.Combine(serverSourcesPath, ServerConnection1ResourceName + ".xml"));

                xml = XmlResource.Fetch(ServerConnection2Name);
                xml.Save(Path.Combine(_sourcesPath, ServerConnection2ResourceName + ".xml"));
                xml.Save(Path.Combine(sourcesVersionControlPath, ServerConnection2ResourceName + ".V" + VersionNo + ".xml"));
                xml.Save(Path.Combine(serverSourcesPath, ServerConnection2ResourceName + ".xml"));

                #endregion

                #endregion

                _workspace = WorkspaceRepository.Instance.Get(workspaceID);
                _testInstance = WorkspaceRepository.Instance;

                _isInitialized = true;
            }
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            //if(Directory.Exists(_servicesPath))
            //{
            //    Directory.Delete(_servicesPath, true);
            //}
        }

        #endregion

        #region SyncTo

        //Commented out because it was decided that all the local files should be kept
        //[TestMethod]
        //public void SyncTo_Where_DeleteIsTrue_And_FileDeletedFromSource_Expected_FileDeletedInDestination()
        //{
        //    IWorkspace workspaceSource = _testInstance.Get(Guid.NewGuid());
        //    IWorkspace workspaceDestination = _testInstance.Get(Guid.NewGuid());

        //    DirectoryInfo sourceDirectory = new DirectoryInfo(Path.Combine(workspaceSource.Host.WorkspacePath, "Services"));
        //    DirectoryInfo destinationDirectory = new DirectoryInfo(Path.Combine(workspaceDestination.Host.WorkspacePath, "Services"));
        //    FileInfo sourceFile = sourceDirectory.GetFiles()[0];
        //    FileInfo destFile = new FileInfo(Path.Combine(destinationDirectory.FullName, sourceFile.Name));

        //    sourceFile.Delete();

        //    workspaceSource.Host.SyncTo(workspaceDestination.Host.WorkspacePath);

        //    destFile.Refresh();

        //    int expected = workspaceSource.Host.Services.Count;
        //    int actual = workspaceDestination.Host.Services.Count;

        //    Assert.IsFalse(destFile.Exists);
        //}

        [TestMethod]
        public void SyncTo_Where_DeleteIsFalse_And_FileDeletedFromSource_Expected_FileNotDeletedInDestination()
        {
            IWorkspace workspaceSource = _testInstance.Get(Guid.NewGuid());
            IWorkspace workspaceDestination = _testInstance.Get(Guid.NewGuid());

            DirectoryInfo sourceDirectory = new DirectoryInfo(Path.Combine(workspaceSource.Host.WorkspacePath, "Services"));
            DirectoryInfo destinationDirectory = new DirectoryInfo(Path.Combine(workspaceDestination.Host.WorkspacePath, "Services"));
            FileInfo sourceFile = sourceDirectory.GetFiles()[0];
            FileInfo destFile = new FileInfo(Path.Combine(destinationDirectory.FullName, sourceFile.Name));

            sourceFile.Delete();

            workspaceSource.Host.SyncTo(workspaceDestination.Host.WorkspacePath, true, false);

            destFile.Refresh();

            Assert.IsTrue(destFile.Exists);
        }

        [TestMethod]
        public void SyncTo_Where_OverrideIsTrue_Expected_FileInDestinationOverridden()
        {
            IWorkspace workspaceSource = _testInstance.Get(Guid.NewGuid());
            IWorkspace workspaceDestination = _testInstance.Get(Guid.NewGuid());

            DirectoryInfo sourceDirectory = new DirectoryInfo(Path.Combine(workspaceSource.Host.WorkspacePath, "Services"));
            DirectoryInfo destinationDirectory = new DirectoryInfo(Path.Combine(workspaceDestination.Host.WorkspacePath, "Services"));
            FileInfo sourceFile = sourceDirectory.GetFiles()[0];
            FileInfo destFile = new FileInfo(Path.Combine(destinationDirectory.FullName, sourceFile.Name));

            using(FileStream fs = sourceFile.Open(FileMode.Append, FileAccess.Write, FileShare.None))
            {
                fs.Write(new byte[]
            {
                200
            }, 0, 1);
                fs.Close();
            }

            workspaceSource.Host.SyncTo(workspaceDestination.Host.WorkspacePath, true, false);

            sourceFile.Refresh();
            destFile.Refresh();

            long expected = sourceFile.Length;
            long actual = destFile.Length;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SyncTo_Where_OverrideIsFalse_Expected_FileInDestinationUnchanged()
        {
            IWorkspace workspaceSource = _testInstance.Get(Guid.NewGuid());
            IWorkspace workspaceDestination = _testInstance.Get(Guid.NewGuid());

            DirectoryInfo sourceDirectory = new DirectoryInfo(Path.Combine(workspaceSource.Host.WorkspacePath, "Services"));
            DirectoryInfo destinationDirectory = new DirectoryInfo(Path.Combine(workspaceDestination.Host.WorkspacePath, "Services"));
            FileInfo sourceFile = sourceDirectory.GetFiles()[0];
            FileInfo destFile = new FileInfo(Path.Combine(destinationDirectory.FullName, sourceFile.Name));

            using(FileStream fs = sourceFile.Open(FileMode.Append, FileAccess.Write, FileShare.None))
            {
                fs.Write(new byte[]
            {
                200
            }, 0, 1);
                fs.Close();
            }

            destFile.Refresh();
            long originalFileSize = destFile.Length;

            workspaceSource.Host.SyncTo(workspaceDestination.Host.WorkspacePath, false, false);

            destFile.Refresh();

            long expected = originalFileSize;
            long actual = destFile.Length;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SyncTo_Where_FilesToIgnoreAreSpecified_Expected_IgnoredFilesArentCopied()
        {
            IWorkspace workspaceSource = _testInstance.Get(Guid.NewGuid());
            IWorkspace workspaceDestination = _testInstance.Get(Guid.NewGuid());

            DirectoryInfo sourceDirectory = new DirectoryInfo(Path.Combine(workspaceSource.Host.WorkspacePath, "Services"));
            DirectoryInfo destinationDirectory = new DirectoryInfo(Path.Combine(workspaceDestination.Host.WorkspacePath, "Services"));
            FileInfo sourceFile = sourceDirectory.GetFiles()[0];
            FileInfo destFile = new FileInfo(Path.Combine(destinationDirectory.FullName, sourceFile.Name));

            destFile.Delete();

            workspaceSource.Host.SyncTo(workspaceDestination.Host.WorkspacePath, false, false, new List<string>
            {
                destFile.Name
            });

            destFile.Refresh();

            Assert.IsFalse(destFile.Exists);
        }

        [TestMethod]
        public void SyncTo_Where_FilesToIgnoreAreSpecified_Expected_IgnoredFilesArentDeleted()
        {
            IWorkspace workspaceSource = _testInstance.Get(Guid.NewGuid());
            IWorkspace workspaceDestination = _testInstance.Get(Guid.NewGuid());

            DirectoryInfo sourceDirectory = new DirectoryInfo(Path.Combine(workspaceSource.Host.WorkspacePath, "Services"));
            DirectoryInfo destinationDirectory = new DirectoryInfo(Path.Combine(workspaceDestination.Host.WorkspacePath, "Services"));
            FileInfo sourceFile = sourceDirectory.GetFiles()[0];
            FileInfo destFile = new FileInfo(Path.Combine(destinationDirectory.FullName, sourceFile.Name));

            sourceFile.Delete();

            workspaceSource.Host.SyncTo(workspaceDestination.Host.WorkspacePath, false, false, new List<string>
            {
                destFile.Name
            });

            destFile.Refresh();

            Assert.IsTrue(destFile.Exists);
        }

        [TestMethod]
        public void SyncTo_Where_DestinationDirectoryDoesntExist_Expected_DestinationDirectoryCreated()
        {
            IWorkspace workspaceSource = _testInstance.Get(Guid.NewGuid());

            DirectoryInfo sourceDirectory = new DirectoryInfo(Path.Combine(workspaceSource.Host.WorkspacePath, "Services"));

            DirectoryInfo tmpDir = new DirectoryInfo(Path.Combine(_testDir, "tmp"));
            workspaceSource.Host.SyncTo(tmpDir.FullName, false, false);

            tmpDir.Refresh();
            Assert.IsTrue(tmpDir.Exists);
        }

        #endregion

        #region RestoreResources

        [TestMethod]
        public void DynamicServicesHost_RestoreResourcesWithOneSignedAndOneUnsignedService_Expected_LoadsSignedService()
        {
            // Class initialization copies 2 services one signed, one unsigned.
            var workspace = WorkspaceRepository.Instance.Get(Guid.NewGuid());
            var host = workspace.Host;

            var signedService = host.Services.Find(s => s.Name == ServiceName);
            var unsignedService = host.Services.Find(s => s.Name == ServiceNameUnsigned);

            Assert.IsNotNull(signedService);
            Assert.IsNull(unsignedService);
        }


        [TestMethod]
        public void DynamicServicesHost_RestoreResources_Expected_LoadsSource()
        {
            // Class initialization copies 1 source
            var workspace = WorkspaceRepository.Instance.Get(Guid.NewGuid());
            var host = workspace.Host;

            var source = host.Sources.Find(s => s.Name == SourceName);

            Assert.IsNotNull(source);
        }

        [TestMethod]
        public void DynamicServicesHost_RestoreResources_WithSourceWithoutID_Expected_InjectsID()
        {
            // Class initialization copies 1 source
            var workspace = WorkspaceRepository.Instance.Get(Guid.NewGuid());
            var host = workspace.Host;

            var source = host.Sources.Find(s => s.Name == SourceName);

            Assert.AreNotEqual(source.ID, Guid.Empty);
        }

        #endregion

        #region SaveResources

        [TestMethod]
        public void DynamicServicesHost_SaveResources_WithUnsignedService_Expected_SignsFile()
        {
            var host = _workspace.Host;
            var resources = host.GenerateObjectGraphFromString(_testServiceDefinition);

            // This invokes SaveResources under the hood.
            host.AddResources(resources, "Domain Admins");

            var signedXml = File.ReadAllText(Path.Combine(_servicesPath, ServiceName + ".xml"));
            var isValid = HostSecurityProvider.Instance.VerifyXml(signedXml);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void DynamicServicesHost_SaveResources_WithSourceWithoutID_Expected_SourceSavedWithID()
        {
            var host = _workspace.Host;
            var resources = host.GenerateObjectGraphFromString(_testSourceDefinition);

            // This invokes SaveResources under the hood.
            host.AddResources(resources, "Domain Admins");

            //var xml = File.ReadAllText(Path.Combine(_sourcesPath, SourceName + ".xml"));
            var xml = XElement.Load(Path.Combine(_sourcesPath, SourceName + ".xml"));
            var attr = xml.Attributes("ID").ToList();

            Assert.AreEqual(1, attr.Count);
        }
        #endregion

        #region RollbackResources

        [TestMethod]
        public void DynamicServicesHost_RollbackResourcesWithUnsignedVersion_Expected_DoesNotRollback()
        {
            var host = _workspace.Host;
            var rolledBack = host.RollbackResource(ServiceNameUnsigned, VersionNo);
            Assert.IsFalse(rolledBack);
        }

        [TestMethod]
        public void DynamicServicesHost_RollbackResourcesWithSignedVersion_Expected_DoesRollback()
        {
            var host = _workspace.Host;
            var rolledBack = host.RollbackResource(ServiceName, VersionNo);
            Assert.IsTrue(rolledBack);
        }

        #endregion

    }
}
