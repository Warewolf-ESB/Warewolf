using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests
{
    /// <summary>
    /// PBI 5278 ServerLifecycleManagerTest
    /// </summary>
    [TestClass]
    public class ServerLifecycleManagerTest
    {
        #region Constants
        private const string _integrationTestRoot = "C:\\Automated Builds\\_IntegrationTest";
        #endregion

        #region Static Members
        private static readonly object _syncLock = new object();
        private static string _lifecycleServerExecutable;
        private static string _lifecycleServerAppConfigFile;
        private static string _lifecycleServerConfigFile;
        private static string _lifecycleServerRootDirectory;
        private static bool _hasInitialized;
        #endregion

        #region Instance Fields

        #endregion

        #region Public Properties

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion

        #region Initialization Handling
        [TestInitialize]
        public void InitializeTestSuite()
        {
            string filePath = null;

            lock(_syncLock)
            {
                if(_hasInitialized) return;
                _hasInitialized = true;
            }

            const string name = "Dev2.Server";

            Process[] allProcess = Process.GetProcesses();
            Process serverProcess = null;
            List<Process> matches = new List<Process>();

            for(int i = 0; i < allProcess.Length; i++)
            {
                Process current = allProcess[i];
                string title;

                try
                {
                    //title = current.MainWindowTitle;
                    title = current.ProcessName;
                }
                catch
                {
                    title = null;
                }

                if(title != null)
                {
                    if(title.Contains(name))
                    {
                        matches.Add(current);
                    }
                }
            }

            bool foundDefinate = false;

            for(int i = 0; i < matches.Count; i++)
            {
                if(!foundDefinate)
                {
                    Process previousProcess = serverProcess;
                    string previousFilePath = filePath;
                    serverProcess = matches[i];


                    try
                    {
                        // Due to being unable to access a 64-bit process
                        // (an issue with MSTest not being able to execute a test built in x64)
                        // we have to find the process path by using the title of the window
                        // which for now will work, but will not if we decide to custom name our window title
                        // or, if we have the server run as a service

                        // This is not the correct way to search for process information

                        //filePath = serverProcess.MainWindowTitle;

                        // This is the correct way to retrieve process information from a 64-bit process using a 32-bit process
                        int processID = serverProcess.Id;

                        string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processID.ToString(CultureInfo.InvariantCulture);
                        using(var searcher = new ManagementObjectSearcher(wmiQueryString))
                        {
                            using(var results = searcher.Get())
                            {
                                ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                                if(mo != null)
                                {
                                    filePath = (string)mo["ExecutablePath"];
                                }
                            }
                        }

                        //filePath = serverProcess.MainModule.FileName;
                    }
                    catch
                    {
                        filePath = null;
                    }

                    if(filePath != null)
                    {
                        if(File.Exists(filePath + ".config"))
                        {
                            if(filePath.Contains(_integrationTestRoot))
                                foundDefinate = true;
                        }
                        else
                        {
                            serverProcess = previousProcess;
                            filePath = previousFilePath;
                        }
                    }
                    else
                    {
                        serverProcess = previousProcess;
                        filePath = previousFilePath;
                    }
                }
            }

            if(serverProcess != null)
            {
                if(filePath != null)
                {
                    string dirName = Path.GetDirectoryName(filePath);
                    string parentDirName = Path.GetDirectoryName(dirName);
                    if(parentDirName != null)
                    {
                        string destinationDirectory = _lifecycleServerRootDirectory = Path.Combine(parentDirName, "LifecycleTestTemp");
                        _lifecycleServerExecutable = Path.Combine(destinationDirectory, Path.GetFileName(filePath));

                        if(_lifecycleServerExecutable.EndsWith(".vshost.exe", StringComparison.OrdinalIgnoreCase))
                            _lifecycleServerExecutable = _lifecycleServerExecutable.Replace(".vshost.exe", ".exe");

                        _lifecycleServerAppConfigFile = _lifecycleServerExecutable + ".config";

                        if(Directory.Exists(destinationDirectory))
                        {
                            Directory.Delete(destinationDirectory, true);
                        }

                        Directory.CreateDirectory(destinationDirectory);
                        CopyContents(dirName, destinationDirectory);

                        if(File.Exists(_lifecycleServerConfigFile = Path.Combine(destinationDirectory, "LifecycleConfig.xml")))
                            File.Delete(_lifecycleServerConfigFile);
                    }

                    File.WriteAllText(_lifecycleServerAppConfigFile, TestResource.LifecycleServerAppConfig);
                }
            }

        }

        private static void CopyContents(string sourceDirectory, string destinationDirectory)
        {
            if(!destinationDirectory.Contains(".svn"))
            {
                string[] allFiles = Directory.GetFiles(sourceDirectory);
                string[] allSubDirectories = Directory.GetDirectories(sourceDirectory);

                foreach(string file in allFiles)
                {
                    File.Copy(file, Path.Combine(destinationDirectory, Path.GetFileName(file)));
                }

                foreach(string directory in allSubDirectories)
                {
                    string newDestination = Path.Combine(destinationDirectory, directory.Remove(0, sourceDirectory.Length + 1));
                    Directory.CreateDirectory(newDestination);
                    CopyContents(directory, newDestination);
                }
            }
        }

        private static int ExecuteServer(string configurationXML)
        {
            File.WriteAllText(_lifecycleServerConfigFile, configurationXML);

            Process serverProcess = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo(_lifecycleServerExecutable) { WorkingDirectory = _lifecycleServerRootDirectory, RedirectStandardInput = true, RedirectStandardOutput = true, UseShellExecute = false };

            serverProcess.StartInfo = startInfo;

            serverProcess.Start();

            StreamWriter writer = serverProcess.StandardInput;
            StreamReader reader = serverProcess.StandardOutput;
            string theLine = "";
            while(theLine != null && theLine.Contains("Press any key to exit"))
            {
                theLine = reader.ReadLine();
            }

            writer.WriteLine((char)(13));

            if(!serverProcess.WaitForExit(15000))
            {
                Assert.Fail("Server process failed to automatically terminate");
            }

            int code = serverProcess.ExitCode;
            return code;
        }
        #endregion


        // Bug 8930 - Sashen To Fix these three tests
        [TestMethod]
        [Ignore]
        // Broken Test
        public void ExternalDependencies_Test()
        {
            if(File.Exists(_lifecycleServerConfigFile))
            {
                File.Delete(_lifecycleServerConfigFile);
            }

            StringBuilder builder = new StringBuilder();
            // This is not in TestResources as it very specific to this particular test and the ServerLifecycleManager
            builder.AppendLine("<configuration>");
            builder.AppendLine("\t<AssemblyReferenceGroup>");

            builder.AppendLine("\t\t<AssemblyReference>System</AssemblyReference>");
            builder.AppendLine("\t\t<AssemblyReference Path=\"C:\\Test\\Path\">Core</AssemblyReference>");
            builder.AppendLine("\t\t<AssemblyReference Version=\"4.0.0.0\">Unlimited</AssemblyReference>");
            builder.AppendLine("\t\t<AssemblyReference Version=\"4.0.0.0\" Culture=\"Neutral\">PresentationFramework</AssemblyReference>");

            builder.AppendLine("\t</AssemblyReferenceGroup>");
            builder.AppendLine("\t<WorkflowGroup Name=\"Initialization\">");
            builder.AppendLine("\t</WorkflowGroup>");
            builder.AppendLine("\t<WorkflowGroup Name=\"Cleanup\">");
            builder.AppendLine("\t</WorkflowGroup>");
            builder.AppendLine("</configuration>");

            string input = builder.ToString();
            int errorCode = ExecuteServer(input);
            Assert.AreEqual(2, errorCode);
        }


        [TestMethod]
        [Ignore]
        // Broken Test
        public void MalformedXML_Test()
        {
            StringBuilder builder = new StringBuilder();

            // This is not in TestResources as it very specific to this particular test and the ServerLifecycleManager
            builder.AppendLine("<configuration>");
            builder.AppendLine("\t<AssemblyReferenceGroup>");

            builder.AppendLine("\t\t<AssemblyRefaerence>System</AssemblyReference>");
            builder.AppendLine("\t\t<AssemblyReference Path=\"C:\\Test\\Path\">Core</AssemblyReference>");
            builder.AppendLine("\t\t<AssemblyReference Version=\"4.0.0.0\">Unlimited</AssemblyReference>");
            builder.AppendLine("\t\t<AssemblyReference Version=\"4.0.0.0\" Culture=\"Neutral\">PresentationFramework</AssemblyReference>");

            builder.AppendLine("\t</AssemblyReferenceGroup>");
            builder.AppendLine("\t<WorkflowGroup Name=\"Initialization\">");
            builder.AppendLine("\t</WorkflowGroup>");
            builder.AppendLine("\t<WorkflowGroup Name=\"Cleanup\">");
            builder.AppendLine("\t</WorkflowGroup>");
            builder.AppendLine("</configuration>");

            string input = builder.ToString();
            int errorCode = ExecuteServer(input);
            Assert.AreEqual(1, errorCode);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            if(_hasInitialized)
            {
                if(Directory.Exists(_lifecycleServerRootDirectory))
                {
                    Directory.Delete(_lifecycleServerRootDirectory, true);
                }
            }
        }
    }
}
