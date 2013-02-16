using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Management;

namespace Dev2.Integration.Tests
{
    public sealed class ServerFabricationFactory : IDisposable
    {
        #region Constants
        private const string _integrationTestRoot = "C:\\Automated Builds\\_IntegrationTest";
        #endregion

        #region Static Members
        private static void CopyContents(string sourceDirectory, string destinationDirectory)
        {
            if (!destinationDirectory.Contains(".svn"))
            {
                string[] allFiles = Directory.GetFiles(sourceDirectory);
                string[] allSubDirectories = Directory.GetDirectories(sourceDirectory);

                foreach (string file in allFiles)
                {
                    File.Copy(file, Path.Combine(destinationDirectory, Path.GetFileName(file)));
                }

                foreach (string directory in allSubDirectories)
                {
                    string newDestination = Path.Combine(destinationDirectory, directory.Remove(0, sourceDirectory.Length + 1));
                    Directory.CreateDirectory(newDestination);
                    CopyContents(directory, newDestination);
                }
            }
        }
        #endregion

        #region Instance Fields
        private readonly object _syncLock = new object();

        private bool _isDisposed;
        private bool _hasInitialized;
        private string _tempDirectoryName;
        private string _serverExecutable = null;
        private string _serverAppConfigFile = null;
        private string _serverConfigFile = null;
        private string _serverRootDirectory = null;
        private string _serverAddress;
        private string _filePath;
        #endregion

        #region Public Properties
        public bool IsDisposed { get { return _isDisposed; } }
        public string ServerExecutableFilepath { get { return _serverExecutable; } }
        public string ServerAppConfigFilepath { get { return _serverAppConfigFile; } }
        public string ServerLifecycleConfigFilepath { get { return _serverConfigFile; } }
        public string ServerRootDirectory { get { return _serverRootDirectory; } }
        public string BaseServerFilePath { get
        {
            return _filePath;
        } }
        public string ServerAddress { get { return _serverAddress; } }
        #endregion

        #region Constructor
        public ServerFabricationFactory()
        {
            _serverAddress = "http://localhost:8765/services/";
            Initialize();
        }
        #endregion

        #region Initialization Handling
        private void Initialize()
        {

            lock (_syncLock)
            {
                if (_hasInitialized) return;
                _hasInitialized = true;
            }

            string name = "Dev2.Server";

            Process[] allProcess = Process.GetProcesses();
            Process serverProcess = null;
            List<Process> matches = new List<Process>();

            for (int i = 0; i < allProcess.Length; i++)
            {
                Process current = allProcess[i];
                string title = null;

                try
                {
                    //title = current.MainWindowTitle;
                    title = current.ProcessName;
                }
                catch
                {
                    title = null;
                }

                if (title != null)
                {
                    if (title.Contains(name))
                    {
                        matches.Add(current);
                    }
                }
            }

            bool foundDefinate = false;

            for (int i = 0; i < matches.Count; i++)
            {
                if (!foundDefinate)
                {
                    Process previousProcess = serverProcess;
                    string previousFilePath = _filePath;
                    serverProcess = matches[i];

                    var findDev2Servers = System.Diagnostics.Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero && p.ProcessName.StartsWith("Dev2.Server"));
                    if (findDev2Servers == null || findDev2Servers.Count() == 0)
                    {
                        throw new Exception("Error - There is no Server running - Please start one before running the integration tests.");
                    }
                    else if (findDev2Servers.Count() > 1)
                    {
                        throw new Exception("Error - Cannot find location if more than 1 studio is open :(");
                    }
                    else
                    {
                        foreach (Process p in findDev2Servers)
                        {
                            int processID = p.Id;
                            string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processID.ToString();
                            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
                            {
                                using (var results = searcher.Get())
                                {
                                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                                    if (mo != null)
                                    {
                                        serverProcess = p;
                                        _filePath = (string)mo["ExecutablePath"];
                                        string folder = _filePath.Substring(0, _filePath.LastIndexOf(@"\") + 1);
                                    }
                                }
                            }
                        }
                    }

                    if (serverProcess != null)
                    {
                        if (_filePath != null)
                        {
                            string dirName = Path.GetDirectoryName(_filePath);
                            string parentDirName = Path.GetDirectoryName(dirName);
                            string baseDirectory = null;
                            int dirCounter = 0;

                            while (Directory.Exists(baseDirectory = Path.Combine(parentDirName, _tempDirectoryName = "Server Fabrication " + dirCounter.ToString())))
                            {
                                if (dirCounter++ >= 100)
                                {
                                    throw new InvalidOperationException("ServerInstanceFabrication misuse, to many concurrent instances.");
                                }
                            }

                            string destinationDirectory = _serverRootDirectory = baseDirectory;
                            _serverExecutable = Path.Combine(destinationDirectory, Path.GetFileName(_filePath));

                            if (_serverExecutable.EndsWith(".vshost.exe", StringComparison.OrdinalIgnoreCase))
                                _serverExecutable = _serverExecutable.Replace(".vshost.exe", ".exe");

                            _serverAppConfigFile = _serverExecutable + ".config";

                            if (Directory.Exists(destinationDirectory))
                            {
                                Directory.Delete(destinationDirectory, true);
                            }

                            Directory.CreateDirectory(destinationDirectory);
                            CopyContents(dirName, destinationDirectory);

                            if (File.Exists(_serverConfigFile = Path.Combine(destinationDirectory, "LifecycleConfig.xml")))
                                File.Delete(_serverConfigFile);

                            File.WriteAllText(_serverAppConfigFile, TestResource.LifecycleServerAppConfig);
                        }
                    }
                }
            }
        }
        #endregion

        public ServerFabrication CreateFabrication()
        {
            return new ServerFabricationImpl(this);
        }

        #region Execution Handling
        public int ExecuteServer()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<configuration>");
            builder.AppendLine("\t<AssemblyReferenceGroup>");
            builder.AppendLine("\t</AssemblyReferenceGroup>");
            builder.AppendLine("\t<WorkflowGroup Name=\"Initialization\">");
            builder.AppendLine("\t</WorkflowGroup>");
            builder.AppendLine("\t<WorkflowGroup Name=\"Cleanup\">");
            builder.AppendLine("\t</WorkflowGroup>");
            builder.AppendLine("</configuration>");

            string input = builder.ToString();

            return ExecuteServer(input);
        }

        public int ExecuteServer(string lifeCycleConfigXML)
        {
            File.WriteAllText(_serverConfigFile, lifeCycleConfigXML);

            Process serverProcess = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo(_serverExecutable);
            startInfo.WorkingDirectory = _serverRootDirectory;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;
            serverProcess.StartInfo = startInfo;

            bool success = serverProcess.Start();

            StreamWriter writer = serverProcess.StandardInput;

            System.Threading.Thread.Sleep(10000);

            writer.WriteLine((char)(13));

            if (!serverProcess.WaitForExit(15000))
            {
                Assert.Fail("Server process failed to automatically terminate");
            }

            int code = serverProcess.ExitCode;
            return code;
        }
        #endregion

        #region Disposal Handling
        ~ServerFabricationFactory()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                bool hasInitialized;

                lock (_syncLock)
                {
                    hasInitialized = _hasInitialized;
                    _hasInitialized = false;
                }

                if (hasInitialized)
                {
                    if (Directory.Exists(_serverRootDirectory))
                    {
                        Directory.Delete(_serverRootDirectory, true);
                    }
                }
            }
        }
        #endregion

        private sealed class ServerFabricationImpl : ServerFabrication
        {
            internal ServerFabricationImpl(ServerFabricationFactory source)
                : base(source)
            {
            }

            protected override ServerExecutionInstance ExecuteImpl()
            {
                return new ServerExecutionInstanceImpl(this, _source);
            }

            public override string GetCommonDirectoryPath(ServerCommonDirectory commonDirectory)
            {
                switch (commonDirectory)
                {
                    case ServerCommonDirectory.Root: return _source._serverRootDirectory;
                    case ServerCommonDirectory.Plugins: return Path.Combine(_source._serverRootDirectory, "Plugins");
                    case ServerCommonDirectory.Css: return Path.Combine(_source._serverRootDirectory, "css");
                    case ServerCommonDirectory.Sources: return Path.Combine(_source._serverRootDirectory, "Sources");
                    case ServerCommonDirectory.Services: return Path.Combine(_source._serverRootDirectory, "Services");
                    case ServerCommonDirectory.Scripts: return Path.Combine(_source._serverRootDirectory, "scripts");
                    case ServerCommonDirectory.Themes: return Path.Combine(_source._serverRootDirectory, "themes");
                }

                throw new ArgumentException("Invalid common directory", "commonDirectory");
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                }

                _source = null;
            }
        }

        private sealed class ServerExecutionInstanceImpl : ServerExecutionInstance
        {
            private ServerFabrication _fabrication;
            private ServerFabricationFactory _factory;
            private Process _process;
            private StreamWriter _writer;
            private int _exitCode;
            private bool _withinExitImpl;
            private readonly object _syncLock = new object();

            public override int ExitCode { get { return _exitCode; } }

            internal ServerExecutionInstanceImpl(ServerFabrication fabrication, ServerFabricationFactory factory)
            {
                _fabrication = fabrication;
                _factory = factory;
                Start();
            }

            private void Start()
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("<configuration>");
                builder.AppendLine("\t<AssemblyReferenceGroup>");
                builder.AppendLine("\t</AssemblyReferenceGroup>");
                builder.AppendLine("\t<WorkflowGroup Name=\"Initialization\">");
                builder.AppendLine("\t</WorkflowGroup>");
                builder.AppendLine("\t<WorkflowGroup Name=\"Cleanup\">");
                builder.AppendLine("\t</WorkflowGroup>");
                builder.AppendLine("</configuration>");

                string input = builder.ToString();

                File.WriteAllText(_factory._serverConfigFile, input);

                _process = new Process();

                ProcessStartInfo startInfo = new ProcessStartInfo(_factory._serverExecutable);
                startInfo.WorkingDirectory = _factory._serverRootDirectory;
                startInfo.RedirectStandardInput = true;
                startInfo.UseShellExecute = false;
                _process.StartInfo = startInfo;

                bool success = _process.Start();
                _writer = _process.StandardInput;
                System.Threading.Thread.Sleep(10000);
            }

            private void ExitImpl()
            {
                bool withinExitImpl = false;

                lock (_syncLock)
                {
                    if (!(withinExitImpl = _withinExitImpl))
                        _withinExitImpl = true;
                }

                if (withinExitImpl) return;

                _writer.WriteLine((char)(13));

                if (!_process.WaitForExit(15000))
                {
                    lock (_syncLock) _withinExitImpl = false;

                    throw new InvalidOperationException("Server process failed to automatically terminate");
                }
                else
                {
                    _exitCode = _process.ExitCode;
                    lock (_syncLock) _withinExitImpl = false;
                    _writer = null;
                    _process.Dispose();
                    _process = null;

                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_process != null && !_process.HasExited)
                    {
                        ExitImpl();
                    }
                }
            }
        }
    }

    public abstract class ServerFabrication : IDisposable
    {
        #region Instance Fields
        private bool _isDisposed;
        private readonly object _syncLock = new object();
        private ServerExecutionInstance _instance;
        protected ServerFabricationFactory _source;
        #endregion

        #region Public Properties
        public bool IsDisposed { get { return _isDisposed; } }
        #endregion

        #region Constructor
        protected ServerFabrication(ServerFabricationFactory source)
        {
            if (source == null) throw new ArgumentNullException("source");
            _source = source;
        }
        #endregion

        #region Execution Handling
        public ServerExecutionInstance Execute()
        {
            if (_isDisposed) throw new ObjectDisposedException("ServerFabrication");
            if (_source.IsDisposed) throw new ObjectDisposedException("ServerFabricationFactory");
            ServerExecutionInstance instance = null;

            lock (_syncLock)
            {
                if (_instance != null)
                {
                    if (_instance.IsDisposed) _instance = null;
                    else throw new InvalidOperationException("ServerFabricator does not support concurrent executions.");
                }

                _instance = instance = ExecuteImpl();
            }

            return instance;
        }

        protected abstract ServerExecutionInstance ExecuteImpl();
        #endregion

        #region [Create/Write] Handling
        public Stream CreateFile(ServerCommonDirectory relativeDirectory, string filenameWithExtension)
        {
            string fullPath = Path.Combine(GetCommonDirectoryPath(relativeDirectory), filenameWithExtension);
            return File.Create(fullPath);
        }

        public void WriteAllText(ServerCommonDirectory relativeDirectory, string filenameWithExtension, string contents)
        {
            string fullPath = Path.Combine(GetCommonDirectoryPath(relativeDirectory), filenameWithExtension);
            File.WriteAllText(fullPath, contents);
        }

        public void WriteAllBytes(ServerCommonDirectory relativeDirectory, string filenameWithExtension, byte[] contents)
        {
            string fullPath = Path.Combine(GetCommonDirectoryPath(relativeDirectory), filenameWithExtension);
            File.WriteAllBytes(fullPath, contents);
        }

        public void WriteAllLines(ServerCommonDirectory relativeDirectory, string filenameWithExtension, IEnumerable<string> contents)
        {
            string fullPath = Path.Combine(GetCommonDirectoryPath(relativeDirectory), filenameWithExtension);
            File.WriteAllLines(fullPath, contents);
        }

        public void WriteAllLines(ServerCommonDirectory relativeDirectory, string filenameWithExtension, IEnumerable<string> contents, Encoding encoding)
        {
            string fullPath = Path.Combine(GetCommonDirectoryPath(relativeDirectory), filenameWithExtension);
            File.WriteAllLines(fullPath, contents, encoding);
        }

        public void WriteAllLines(ServerCommonDirectory relativeDirectory, string filenameWithExtension, string[] contents)
        {
            string fullPath = Path.Combine(GetCommonDirectoryPath(relativeDirectory), filenameWithExtension);
            File.WriteAllLines(fullPath, contents);
        }

        public void WriteAllLines(ServerCommonDirectory relativeDirectory, string filenameWithExtension, string[] contents, Encoding encoding)
        {
            string fullPath = Path.Combine(GetCommonDirectoryPath(relativeDirectory), filenameWithExtension);
            File.WriteAllLines(fullPath, contents, encoding);
        }
        #endregion

        #region [Exists/Ensure] Handling
        public void EnsureFileDeleted(ServerCommonDirectory relativeDirectory, string filenameWithExtension)
        {
            string fullPath = Path.Combine(GetCommonDirectoryPath(relativeDirectory), filenameWithExtension);

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        public void EnsureDirectoryExists(ServerCommonDirectory relativeDirectory, string directoryName)
        {
            string fullPath = Path.Combine(GetCommonDirectoryPath(relativeDirectory), directoryName);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
        }

        public bool FileExists(ServerCommonDirectory relativeDirectory, string filenameWithExtension)
        {
            string fullPath = Path.Combine(GetCommonDirectoryPath(relativeDirectory), filenameWithExtension);
            return File.Exists(fullPath);
        }

        public bool DirectoryExists(ServerCommonDirectory relativeDirectory, string directoryName)
        {
            string fullPath = Path.Combine(GetCommonDirectoryPath(relativeDirectory), directoryName);
            return Directory.Exists(fullPath);
        }

        public abstract string GetCommonDirectoryPath(ServerCommonDirectory commonDirectory);
        #endregion

        #region Disposal Handling
        ~ServerFabrication()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            Dispose(true);

            lock (_syncLock)
            {
                if (_instance != null)
                {
                    _instance.Dispose();
                }

                _instance = null;
            }

            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
        #endregion
    }

    public abstract class ServerExecutionInstance : IDisposable
    {
        #region Instance Fields
        private bool _isDisposed;
        #endregion

        #region Public Properties
        public bool IsDisposed { get { return _isDisposed; } }
        public abstract int ExitCode { get; }
        #endregion

        #region Disposal Handling
        ~ServerExecutionInstance()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
        #endregion
    }

    public enum ServerCommonDirectory
    {
        /// <summary>
        /// The root directory of the fabricated server, this is the same directory that contains the server executable.
        /// </summary>
        Root = 0,
        /// <summary>
        /// The plugins directory of the fabricated server.
        /// </summary>
        Plugins = 1,
        /// <summary>
        /// The css directory of the fabricated server.
        /// </summary>
        Css = 2,
        /// <summary>
        /// The scripts directory of the fabricated server.
        /// </summary>
        Scripts = 3,
        /// <summary>
        /// The services directory of the fabricated server.
        /// </summary>
        Services = 4,
        /// <summary>
        /// The sources directory of the fabricated server.
        /// </summary>
        Sources = 5,
        /// <summary>
        /// The themes directory of the fabricated server.
        /// </summary>
        Themes = 6
    }
}
