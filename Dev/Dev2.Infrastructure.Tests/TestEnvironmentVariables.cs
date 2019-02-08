﻿using Dev2.Common;
using Dev2.Common.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Dev2.Infrastructure.Tests
{
    [TestClass]
    public class TestEnvironmentVariables
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_GetServerSettingsFolder_ShouldReturnProgramDataFolder()
        {
            //------------Setup for test--------------------------
            const string serverSettingsFolderPart = "ProgramData\\Warewolf\\Server Settings";
            //------------Execute Test---------------------------
            var serverSettingsFolder = EnvironmentVariables.ServerSettingsFolder;
            //------------Assert Results-------------------------
            StringAssert.Contains(serverSettingsFolder, serverSettingsFolderPart);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_GetServerSecurityFile_ShouldReturnSecuritySettingsFileInProgramData()
        {
            //------------Setup for test--------------------------
            const string filePart = "ProgramData\\Warewolf\\Server Settings\\Settings.config";
            //------------Execute Test---------------------------
            var filePath = EnvironmentVariables.ServerLogSettingsFile;
            //------------Assert Results-------------------------
            StringAssert.Contains(filePath, filePart);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_GetServerLogSettingsFile_ShouldReturnLogSettingsFileInProgramData()
        {
            //------------Setup for test--------------------------
            const string serverSecurityFilePart = "ProgramData\\Warewolf\\Server Settings\\secure.config";
            //------------Execute Test---------------------------
            var serverSecurityFilePath = EnvironmentVariables.ServerSecuritySettingsFile;
            //------------Assert Results-------------------------
            StringAssert.Contains(serverSecurityFilePath, serverSecurityFilePart);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_GetServerLogFile_ShouldReturnLogFileInProgramData()
        {
            //------------Setup for test--------------------------
            const string filePart = "ProgramData\\Warewolf\\Server Log\\warewolf-Server.log";
            //------------Execute Test---------------------------
            var filePath = EnvironmentVariables.ServerLogFile;
            //------------Assert Results-------------------------
            StringAssert.Contains(filePath, filePart);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_GetResourcePath_ShouldReturnResourceFolderInProgramData()
        {
            //------------Setup for test--------------------------
            const string folderPart = "ProgramData\\Warewolf\\Resources";
            //------------Execute Test---------------------------
            var folderPath = EnvironmentVariables.ResourcePath;
            //------------Assert Results-------------------------
            StringAssert.Contains(folderPath, folderPart);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_GetTestPath_ShouldReturnTestFolderInProgramData()
        {
            //------------Setup for test--------------------------
            const string folderPart = "ProgramData\\Warewolf\\Tests";
            //------------Execute Test---------------------------
            var folderPath = EnvironmentVariables.TestPath;
            //------------Assert Results-------------------------
            StringAssert.Contains(folderPath, folderPart);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_WorkspacePath_ShouldReturnWorkspaceFolderInProgramData()
        {
            //------------Setup for test--------------------------
            const string folderPart = "ProgramData\\Warewolf\\Workspaces";
            //------------Execute Test---------------------------
            var folderPath = EnvironmentVariables.WorkspacePath;
            //------------Assert Results-------------------------
            StringAssert.Contains(folderPath, folderPart);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_GetWorkspacePath_GuidEmpty()
        {
            //------------Setup for test--------------------------
            const string folderPart = "ProgramData\\Warewolf\\Resources";
            //------------Execute Test---------------------------
            var folderPath = EnvironmentVariables.GetWorkspacePath(Guid.Empty);
            //------------Assert Results-------------------------
            StringAssert.Contains(folderPath, folderPart);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_GetWorkspacePath_Guid()
        {
            var guid = new Guid("c550ca0d-d324-45de-92bb-0c91879eb8b3");
            //------------Setup for test--------------------------
            const string folderPart = "ProgramData\\Warewolf\\Workspaces\\c550ca0d-d324-45de-92bb-0c91879eb8b3\\Resources";
            //------------Execute Test---------------------------
            var folderPath = EnvironmentVariables.GetWorkspacePath(guid);
            //------------Assert Results-------------------------
            StringAssert.Contains(folderPath, folderPart);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_ServerPerfmonSettingsFile()
        {
            //------------Setup for test--------------------------
            const string folderPart = "ProgramData\\Warewolf\\Server Settings\\Perfmon.config";
            //------------Execute Test---------------------------
            var path = EnvironmentVariables.ServerPerfmonSettingsFile;
            //------------Assert Results-------------------------
            StringAssert.Contains(path, folderPart);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_ServerResourcePerfmonSettingsFile()
        {
            //------------Setup for test--------------------------
            const string folderPart = "ProgramData\\Warewolf\\Server Settings\\ResourcesPerfmon.config";
            //------------Execute Test---------------------------
            var path = EnvironmentVariables.ServerResourcePerfmonSettingsFile;
            //------------Assert Results-------------------------
            StringAssert.Contains(path, folderPart);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_WorkflowDetailLogArchivePath()
        {
            //------------Setup for test--------------------------
            const string folderPart = "ProgramData\\Warewolf\\DetailedLogs\\Archives\\c550ca0d-d324-45de-92bb-0c91879eb8b3_.zip";
            //------------Execute Test---------------------------
            var path = EnvironmentVariables.WorkflowDetailLogArchivePath(new Guid("c550ca0d-d324-45de-92bb-0c91879eb8b3"), null);
            //------------Assert Results-------------------------
            StringAssert.Contains(path, folderPart);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_WorkflowDetailLogArchivePath_Name()
        {
            //------------Setup for test--------------------------
            const string folderPart = "ProgramData\\Warewolf\\DetailedLogs\\Archives\\c550ca0d-d324-45de-92bb-0c91879eb8b3_testing.zip";
            //------------Execute Test---------------------------
            var path = EnvironmentVariables.WorkflowDetailLogArchivePath(new Guid("c550ca0d-d324-45de-92bb-0c91879eb8b3"), "testing");
            //------------Assert Results-------------------------
            StringAssert.Contains(path,folderPart);
        }
       
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_AppDataPath()
        {
            //------------Setup for test--------------------------
            const string folderPart = "ProgramData\\Warewolf";
            //------------Execute Test---------------------------
            var path = EnvironmentVariables.AppDataPath;
            //------------Assert Results-------------------------
            StringAssert.Contains(path, folderPart);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_VersionsPath()
        {
            //------------Setup for test--------------------------
            const string folderPart = "ProgramData\\Warewolf\\VersionControl";
            //------------Execute Test---------------------------
            var path = EnvironmentVariables.VersionsPath;
            //------------Assert Results-------------------------
            StringAssert.Contains(path, folderPart);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_DetailedLogsArchives()
        {
            //------------Setup for test--------------------------
            const string folderPart = "ProgramData\\Warewolf\\DetailedLogs\\Archives";
            //------------Execute Test---------------------------
            var path = EnvironmentVariables.DetailedLogsArchives;
            //------------Assert Results-------------------------
            StringAssert.Contains(path, folderPart);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_WorkflowDetailLogPath_ShouldReturnDetailedLogsInProgramData()
        {
            //------------Setup for test--------------------------
            const string folderPart = "ProgramData\\Warewolf\\DetailedLogs";
            //------------Execute Test---------------------------
            var folderPath = EnvironmentVariables.WorkflowDetailLogPath(It.IsAny<Guid>(), It.IsAny<string>());
            //------------Assert Results-------------------------
            StringAssert.Contains(folderPath, folderPart);
            var directory = new DirectoryWrapper();
            directory.Delete(folderPath, true);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_IsServerOnline()
        {
            //------------Execute Test---------------------------
            EnvironmentVariables.IsServerOnline = true;
            //------------Assert Results-------------------------
            Assert.IsTrue(EnvironmentVariables.IsServerOnline);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_WebServerUri()
        {
            //------------Execute Test---------------------------
            EnvironmentVariables.WebServerUri = "warewolf:8080/";
            //------------Assert Results-------------------------
            Assert.AreEqual("warewolf:8080/", EnvironmentVariables.WebServerUri);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_DnsName()
        {
            //------------Execute Test---------------------------
            EnvironmentVariables.DnsName = "warewolf";
            //------------Assert Results-------------------------
            Assert.AreEqual("warewolf", EnvironmentVariables.DnsName);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_Port()
        {
            //------------Execute Test---------------------------
            EnvironmentVariables.Port = 8080;
            //------------Assert Results-------------------------
            Assert.AreEqual(8080, EnvironmentVariables.Port);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_RemoteInvokeID()
        {
            Assert.IsNotNull(EnvironmentVariables.RemoteInvokeID);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_PublicWebServerUri()
        {
            EnvironmentVariables.DnsName = "warewolf";
            EnvironmentVariables.Port = 8080;
            Assert.AreEqual("warewolf:8080/", EnvironmentVariables.PublicWebServerUri);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_RootPersistencePath()
        {
            //------------Execute Test---------------------------
            var folderPart = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Warewolf");
            var path = EnvironmentVariables.RootPersistencePath;
            //------------Assert Results-------------------------
            StringAssert.Contains(folderPart, path);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_CharacterMap()
        {
            var defaultEncoding = EnvironmentVariables.CharacterMap.DefaultEncoding;
            var lettersStartNumber = EnvironmentVariables.CharacterMap.LettersStartNumber;
            var lettersLength = lettersStartNumber + EnvironmentVariables.CharacterMap.LettersLength;
            //------------Assert Results-------------------------
            Assert.AreEqual(97, lettersStartNumber);
            Assert.AreEqual(123, lettersLength);
            Assert.AreEqual(Encoding.ASCII, defaultEncoding);

        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(EnvironmentVariables))]
        public void EnvironmentVariables_ApplicationPath()
        {
            //------------Setup for test--------------------------
            var assembly = Assembly.GetExecutingAssembly();
            var loc = assembly.Location;
           var appPath = Path.GetDirectoryName(loc);
            //------------Execute Test---------------------------
            var path = EnvironmentVariables.ApplicationPath;
            //------------Assert Results-------------------------
            Assert.AreEqual(path, appPath);
        }
    }
}
