using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Wrappers;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Server_Refresh
{
    [TestClass]
    public class FreshlyInstalledServerStartupTest
    {
        const string ResourcesBackup = "C:\\programdata\\warewolf\\resources_BACKUP";
        DirectoryWrapper _directoryWrapper;
        
        [TestInitialize]
        public void Startup()
        {
            _directoryWrapper = new DirectoryWrapper();
            if (_directoryWrapper.Exists(ResourcesBackup))
            {
                _directoryWrapper.Delete(ResourcesBackup, true);
            }
            if (_directoryWrapper.Exists(EnvironmentVariables.ResourcePath)) 
            {
                _directoryWrapper.Move(EnvironmentVariables.ResourcePath, ResourcesBackup);
            }
            var serverUnderTest = Process.GetProcessesByName("Warewolf Server")[0];
            string exePath;
            try 
            {
                exePath = Path.GetDirectoryName(serverUnderTest.MainModule?.FileName);
            }
            catch (Win32Exception)
            {
                exePath = Environment.CurrentDirectory;
            }
            var destDirName = Path.Combine(exePath, "Resources");
            if (!_directoryWrapper.Exists(destDirName))
            {
                _directoryWrapper.Move(Path.Combine(exePath, "Resources - Release", "Resources"), destDirName);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_directoryWrapper.Exists(EnvironmentVariables.ResourcePath))
            {
                _directoryWrapper.Delete(EnvironmentVariables.ResourcePath, true);
            }
            if (_directoryWrapper.Exists(ResourcesBackup)) 
            {
                _directoryWrapper.Move(ResourcesBackup, EnvironmentVariables.ResourcePath);
            }
            ExecuteRequest(new Uri("http://localhost:3142/services/FetchExplorerItemsService.json?ReloadResourceCatalogue=true"));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("Server Startup")]
        public void Run_a_workflow_to_test_server_startup_when_programdata_resources_directory_does_not_exist()
        {
            Assert.IsFalse(Directory.Exists(EnvironmentVariables.ResourcePath), "Cannot prepare for integration test.");

            SetupPermissions();
            RestartServer();

            var url = "http://localhost:3142/services/getserverversion.json";
            var passRequest = ExecuteRequest(new Uri(url));
            Assert.IsFalse(String.IsNullOrEmpty(passRequest));
        }

        void RestartServer()
        {
            ServiceController service = new ServiceController("Warewolf Server");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service");
            ManagementObject ServerService = searcher.Get().OfType<ManagementObject>().FirstOrDefault(obj => { return (string)obj["Name"] == "Warewolf Server"; });
            var fullServicePath = ServerService["PathName"] as string;
            var startingString = "CodeCoverage.exe\" collect /output:\"";
            var endingString = "TestResults\\Snapshot.coverage";
            if (fullServicePath.Contains(startingString) && fullServicePath.Contains(endingString))
            {
                var startingIndex = fullServicePath.IndexOf(startingString) + startingString.Length;
                var findLength = fullServicePath.IndexOf(endingString) + endingString.Length - startingIndex;
                var snapshotPath = fullServicePath.Substring(startingIndex, findLength);
                var TestResultsPath = Path.GetDirectoryName(snapshotPath);
                var ServerCoverageSnapshotBackupPath = Path.Combine(TestResultsPath, "Snapshot_Backup.coverage");

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(300000));

                if (File.Exists(ServerCoverageSnapshotBackupPath))
                {
                    File.Delete(ServerCoverageSnapshotBackupPath);
                }
                if (File.Exists(snapshotPath))
                {
                    if (WaitForFile(snapshotPath))
                    {
                        File.Move(snapshotPath, ServerCoverageSnapshotBackupPath);
                    }
                    else
                    {
                        throw new Exception("Cannot backup existing coverage snapshot.");
                    }
                }
            }
            else
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(300000));
            }

            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(300000));
        }

        class PatientWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                var w = base.GetWebRequest(uri);
                if(w != null)
                {
                    w.Timeout = 20 * 60 * 1000;
                    return w;
                }
                return null;
            }
        }

        bool WaitForFile (string fullPath)
        {
            for (int numTries = 0; numTries < 30; numTries++) {
                FileStream fs = null;
                try {
                    fs = new FileStream (fullPath, FileMode.Open, FileAccess.Write, FileShare.Delete);
                    fs.Close();
                    fs.Dispose();
                    return true;
                }
                catch (IOException) {
                    if (fs != null) {
                        fs.Dispose ();
                    }
                    Thread.Sleep (1000);
                }
            }

            return false;
        }

        string ExecuteRequest(Uri url)
        {
            Task<string> failRequest;
            var client = new PatientWebClient { Credentials = CredentialCache.DefaultNetworkCredentials };
            using (client)
            {
                failRequest = Task.Run(() => client.DownloadString(url));
            }
            string failRequestResult;
            try
            {
                failRequestResult = failRequest.Result;
            }
            catch (AggregateException e)
            {
                return new StreamReader((e.InnerExceptions[0] as WebException)?.Response?.GetResponseStream())?.ReadToEnd();
            }

            return failRequestResult;
        }

        static void SetupPermissions()
        {
            var groupRights = "View, Execute, Contribute, Deploy To, Deploy From, Administrator";
            var groupPermssions = new WindowsGroupPermission
            {
                WindowsGroup = "Public",
                ResourceID = Guid.Empty,
                IsServer = true
            };
            var permissionsStrings = groupRights.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var permissionsString in permissionsStrings)
            {
                Permissions permission;
                if (Enum.TryParse(permissionsString.Replace(" ", ""), true, out permission))
                {
                    groupPermssions.Permissions |= permission;
                }
            }
            var settings = new Data.Settings.Settings
            {
                Security = new SecuritySettingsTO(new List<WindowsGroupPermission> { groupPermssions })
            };
            AppUsageStats.LocalHost = "http://localhost:3142";
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            environmentModel.ResourceRepository.WriteSettings(environmentModel, settings);
        }
    }
}
