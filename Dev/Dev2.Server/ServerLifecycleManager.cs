using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Xml;
using CommandLine;
using Dev2;
using Dev2.Common;
using Dev2.Common.Reflection;
using Dev2.Data;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Network;
using Dev2.DynamicServices.Network.DataList;
using Dev2.DynamicServices.Network.Execution;
using Dev2.Network.Execution;
using Dev2.Runtime.Security;
using Dev2.Workspaces;

namespace Unlimited.Applications.DynamicServicesHost
{
    /// <summary>
    /// PBI 5278
    /// Application Server Lifecycle Manager
    /// Facilitates start-up, execution and tear-down of the application server.
    /// </summary>
    internal sealed class ServerLifecycleManager : IDisposable
    {
        #region Constants
        private const string _defaultConfigFileName = "LifecycleConfig.xml";

        #endregion

        #region Static Members

        private static ServerLifecycleManager _singleton;

        #endregion

        #region Entry Point
        /// <summary>
        /// Entry Point for application server.
        /// </summary>
        /// <param name="arguments">Command line arguments passed to executable.</param>
        private static int Main(string[] arguments)
        {
            int result = 0;

            CommandLineParameters options = new CommandLineParameters();
            CommandLineParser parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
            if(!parser.ParseArguments(arguments, options))
            {
                return 80;
            }

            bool commandLineParameterProcessed = false;
            if(options.Install)
            {
                commandLineParameterProcessed = true;

                if(!EnsureRunningAsAdministrator(arguments))
                {
                    return result;
                }

                if(!WindowsServiceManager.Install())
                {
                    result = 81;
                }
            }

            if(options.StartService)
            {
                commandLineParameterProcessed = true;

                if(!EnsureRunningAsAdministrator(arguments))
                {
                    return result;
                }

                if(!WindowsServiceManager.StartService(null))
                {
                    result = 83;
                }
            }

            if(options.StopService)
            {
                commandLineParameterProcessed = true;

                if(!EnsureRunningAsAdministrator(arguments))
                {
                    return result;
                }

                if(!WindowsServiceManager.StopService(null))
                {
                    result = 84;
                }
            }

            if(options.Uninstall)
            {
                commandLineParameterProcessed = true;

                if(!EnsureRunningAsAdministrator(arguments))
                {
                    return result;
                }

                if(!WindowsServiceManager.Uninstall())
                {
                    result = 82;
                }
            }

            if(commandLineParameterProcessed)
            {
                return result;
            }

            //Type type = null;

            //using (System.Emission.NetworkProxyGenerator generator = new System.Emission.NetworkProxyGenerator(false)) {
            //    type = generator.CreateNetworkProxy(typeof(Dev2.DataList.Contract.IDataListCompiler));
            //    generator.SaveAssembly();
            //}

            //IDataListCompiler compiler = (Dev2.DataList.Contract.IDataListCompiler)Activator.CreateInstance(type, new object[] { new __DatalistCompiledNetworkTransparentProxy(), InternalTemplates.Server_OnExecuteStringCommandReceived });

            if(Environment.UserInteractive)
            {
                using(_singleton = new ServerLifecycleManager(arguments))
                {
                    result = _singleton.Run();
                }

                _singleton = null;
            }
            else
            {
                // running as service
                using(var service = new ServerLifecycleManagerService())
                {
                    ServiceBase.Run(service);
                }
            }

            return result;
        }
        #endregion

        #region Nested classes to support running as service

        public class ServerLifecycleManagerService : ServiceBase
        {
            public ServerLifecycleManagerService()
            {
                ServiceName = ServiceName;
                CanPauseAndContinue = false;
            }

            protected override void OnStart(string[] args)
            {
                _singleton = new ServerLifecycleManager(null);
                _singleton.Run();
            }

            protected override void OnStop()
            {
                _singleton.Stop(false, 0);
                _singleton = null;
            }
        }

        #endregion

        //private sealed class __DatalistCompiledNetworkTransparentProxy : __BaseNetworkTransparentProxy
        //{
        //    public __DatalistCompiledNetworkTransparentProxy()
        //    {

        //    }

        //    public override IByteReaderBase SendDuplexPacket(Packet packet)
        //    {
        //        return base.SendDuplexPacket(packet);
        //    }

        //    public override void SendSimplexPacket(Packet packet)
        //    {
        //        base.SendSimplexPacket(packet);
        //    }

        //    public override object ConstructUnhandled(Type type)
        //    {

        //        return base.ConstructUnhandled(type);
        //    }

        //    public override object ReadUnhandled(IByteReaderBase reader, Type type)
        //    {

        //        return base.ReadUnhandled(reader, type);
        //    }

        //    public override void WriteUnhandled(IByteWriterBase writer, object unhandled)
        //    {

        //        base.WriteUnhandled(writer, unhandled);
        //    }
        //}

        #region Instance Fields
        private bool _isDisposed;
        private bool _isWebServerEnabled;
        private bool _preloadAssemblies;
        private string[] _arguments;
        private AssemblyReference[] _externalDependencies;
        private Dictionary<string, WorkflowEntry[]> _workflowGroups;
        private IPEndPoint[] _endpoints;
        private IFrameworkWebServer _webserver;
        private EsbServicesEndpoint _esbEndpoint;

        private StudioNetworkServer _networkServer;
        private ExecutionServerChannel _executionChannel;
        private DataListServerChannel _dataListChannel;

        private string[] _prefixes;
        private string _uriAddress;
        private string _configFile;

        //private Guid _serverID;

        // START OF GC MANAGEMENT
        private bool _enableGCManager;
        private long _minimumWorkingSet;
        private long _maximumWorkingSet;
        private long _lastKnownWorkingSet;
        private volatile bool _gcmRunning;
        private DateTime _nextForcedCollection;
        private Thread _gcmThread;
        private ThreadStart _gcmThreadStart;
        //Process _redisProcess;
        // END OF GC MANAGEMENT
        #endregion

        #region Public Properties
        /// <summary>
        /// Get a value indicating if the lifecycle manager has been disposed.
        /// </summary>
        public bool IsDisposed { get { return _isDisposed; } }
        /// <summary>
        /// Gets a value indicating if the webserver is enabled.
        /// </summary>
        public bool IsWebServerEnabled { get { return _isWebServerEnabled; } }
        /// <summary>
        /// Gets a Guid that represents the ID of the current server.
        /// </summary>
        public Guid ServerID { get { return HostSecurityProvider.Instance.ServerID; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructors an instance of the ServerLifecycleManager class, ServerLifecycleManager is essentially a singleton but implemented as an instance type
        /// to ensure proper finalization occurs.
        /// </summary>
        private ServerLifecycleManager(string[] arguments)
        {
            _arguments = arguments ?? new string[0];
            _configFile = _defaultConfigFileName;
            _externalDependencies = AssemblyReference.EmptyReferences;
            _workflowGroups = new Dictionary<string, WorkflowEntry[]>(StringComparer.OrdinalIgnoreCase);

            InitializeCommandLineArguments();
        }
        #endregion

        #region Run Handling
        /// <summary>
        /// Runs the application server, and handles all initialization, execution and cleanup logic required.
        /// </summary>
        /// <returns></returns>
        private int Run()
        {
            int result = 0;
            bool didBreak = false;


            if(!SetWorkingDirectory())
            {
                result = 95;
                didBreak = true;
            }

            // PBI 5389 - Resources Assigned and Allocated to Server
            if(!didBreak && !LoadHostSecurityProvider())
            {
                result = 1;
                didBreak = true;
            }

            if(!didBreak && !LoadConfiguration(_configFile))
            {
                result = 1;
                didBreak = true;
            }

            if(!didBreak && !PreloadReferences())
            {
                result = 2;
                didBreak = true;
            }

            if(!didBreak && !StartGCManager())
            {
                result = 7;
                didBreak = true;
            }

            if(!didBreak && !InitializeServer())
            {
                result = 3;
                didBreak = true;
            }

            // Start DataList Server
            if(!didBreak && !StartDataListServer())
            {
                result = 99;
                didBreak = true;
            }

            // BUG 7850 - Resource catalog (TWR: 2013.03.13)
            if(!didBreak && !LoadResourceCatalog())
            {
                result = 94;
                didBreak = true;
            }

            // PBI 5389 - Resources Assigned and Allocated to Server
            if(!didBreak && !LoadServerWorkspace())
            {
                result = 98; // ????
                didBreak = true;
            }

            // PBI 1018 - Settings Framework (TWR: 2013.03.07)
            if(!didBreak && !LoadSettingsProvider())
            {
                result = 95;
                didBreak = true;
            }

            if(!didBreak && !OpenNetworkExecutionChannel())
            {
                result = 97;
                didBreak = true;
            }

            if(!didBreak && !OpenNetworkDataListChannel())
            {
                result = 96;
                didBreak = true;
            }

            if(!didBreak && !StartWebServer())
            {
                result = 4;
                didBreak = true;
            }

            //if(!didBreak && !ExecuteWorkflowGroup("Initialization"))
            //{
            //    result = 5;
            //    didBreak = true;
            //}

            if(!didBreak)
            {
                result = ServerLoop();
            }
            else
            {
                result = Stop(true, result);
            }

            return result;
        }

        private int Stop(bool didBreak, int result)
        {
            //Dev2RedisClient.StopRedis();
            //if(!ExecuteWorkflowGroup("Cleanup"))
            //{
            //    didBreak = true;
            //    result = 6;
            //}

            // PBI 1018 - Settings Framework (TWR: 2013.03.07)
            UnloadSettingsProvider();

            if(!didBreak)
            {
                Dispose();
            }
            else
            {
                TerminateGCManager();
            }

            Write(string.Format("Existing with exitcode {0}", result));
            return result;
        }

        private int ServerLoop()
        {
            if(Environment.UserInteractive)
            {
                Console.WriteLine("Press <ENTER> to terminate service and/or web server if started");
                Console.ReadLine();
                return Stop(false, 0);
            }

            return 0;
        }

        private bool SetWorkingDirectory()
        {
            bool result = true;

            try
            {
                // Brendon.Page - The following line is has had it warning supressed because if the working dirctory can't be set
                //                then it can't be garunteed that the server will operate correctly, and in this case the desired
                //                behaviour is a fail with an exception.
                // ReSharper disable AssignNullToNotNullAttribute
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                // ReSharper restore AssignNullToNotNullAttribute
            }
            catch(Exception e)
            {
                Fail("Unable to set working directory.", e);
                result = false;
            }

            return result;
        }

        private static bool EnsureRunningAsAdministrator(string[] arguments)
        {
            if(!IsElevated())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location);
                startInfo.Verb = "runas";
                startInfo.Arguments = string.Join(" ", arguments);

                Process process = new Process();
                process.StartInfo = startInfo;

                try
                {
                    process.Start();
                }
                catch(Exception)
                {
                    //Intentionally left blank incase the user denies the app admin
                    //privilidges.
                }

                return false;
            }

            return true;
        }

        private static bool IsElevated()
        {
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(currentIdentity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        #endregion

        #region Configuration Handling
        /// <summary>
        /// Reads the configuration file and records the entries in class level fields.
        /// </summary>
        /// <param name="filePath">The relative or fully qualified file path to the configuration file</param>
        /// <returns>true if the configuration file was loaded correctly, false otherwise</returns>
        private bool LoadConfiguration(string filePath)
        {
            bool recreate = false;
            bool result = true;

            if(File.Exists(filePath))
            {
                XmlDocument document = new XmlDocument();

                try
                {
                    document.Load(filePath);
                }
                catch(Exception e)
                {
                    Fail("Configuration load error", e);
                    result = false;
                }

                if(result)
                {
                    result = LoadConfiguration(document);
                }
            }
            else
                recreate = true;

            if(recreate)
            {
                WriteLine("Configuration file \"" + filePath + "\" does not exist, creating empty configuration file...");

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("<configuration>");
                builder.AppendLine("\t<GCManager Enabled=\"false\">");
                builder.AppendLine("\t\t<MinWorkingSet>60</MinWorkingSet>");
                builder.AppendLine("\t\t<MaxWorkingSet>1536</MaxWorkingSet>");
                builder.AppendLine("\t</GCManager>");
                builder.AppendLine("\t<PreloadAssemblies>true</PreloadAssemblies>");
                builder.AppendLine("\t<AssemblyReferenceGroup>");
                builder.AppendLine("\t</AssemblyReferenceGroup>");
                builder.AppendLine("\t<WorkflowGroup Name=\"Initialization\">");
                builder.AppendLine("\t</WorkflowGroup>");
                builder.AppendLine("\t<WorkflowGroup Name=\"Cleanup\">");
                builder.AppendLine("\t</WorkflowGroup>");
                builder.AppendLine("</configuration>");

                try
                {
                    File.WriteAllText(filePath, builder.ToString());
                }
                catch(Exception)
                {
                    result = false;
                }
            }

            return result;
        }

        private bool LoadConfiguration(XmlDocument document)
        {
            bool result = true;

            XmlNodeList allSections = document.HasChildNodes ? (document.FirstChild.HasChildNodes ? document.FirstChild.ChildNodes : null) : null;

            if(allSections != null)
            {
                foreach(XmlNode section in allSections)
                {
                    if(result)
                    {
                        ReadBooleanSection(section, "PreloadAssemblies", ref result, ref _preloadAssemblies);

                        if(String.Equals(section.Name, "GCManager", StringComparison.OrdinalIgnoreCase))
                        {
                            if(!ProcessGCManager(section))
                            {
                                result = false;
                            }
                        }

                        if(String.Equals(section.Name, "AssemblyReferenceGroup", StringComparison.OrdinalIgnoreCase))
                        {
                            if(!ProcessAssemblyReferenceGroup(section))
                            {
                                result = false;
                            }
                        }
                        else if(String.Equals(section.Name, "WorkflowGroup", StringComparison.OrdinalIgnoreCase))
                        {
                            if(!ProcessWorkflowGroup(section))
                            {
                                result = false;
                            }
                        }
                    }

                }
            }

            return result;
        }

        private bool ReadBooleanSection(XmlNode section, string sectionName, ref bool result, ref bool setter)
        {
            bool output = false;

            if(String.Equals(section.Name, sectionName, StringComparison.OrdinalIgnoreCase))
            {
                output = true;

                if(!String.IsNullOrEmpty(section.InnerText))
                {
                    if(String.Equals(section.InnerText, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        setter = true;
                    }
                    else if(String.Equals(section.InnerText, "false", StringComparison.OrdinalIgnoreCase))
                    {
                        setter = false;
                    }
                    else
                    {
                        Fail("Configuration error, " + sectionName);
                        result = false;
                    }
                }
                else
                {
                    Fail("Configuration error, " + sectionName);
                    result = false;
                }
            }

            return output;
        }


        private bool ProcessGCManager(XmlNode section)
        {
            XmlAttributeCollection sectionAttribs = section.Attributes;

            if(sectionAttribs != null)
            {
                foreach(XmlAttribute sAttrib in sectionAttribs)
                {
                    if(String.Equals(sAttrib.Name, "Enabled", StringComparison.OrdinalIgnoreCase))
                    {
                        _enableGCManager = String.Equals(sAttrib.Value, "True", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }

            XmlNodeList allReferences = section.HasChildNodes ? section.ChildNodes : null;

            if(allReferences != null)
            {
                foreach(XmlNode current in allReferences)
                {
                    if(String.Equals(current.Name, "MinWorkingSet", StringComparison.OrdinalIgnoreCase))
                    {
                        if(!String.IsNullOrEmpty(current.InnerText))
                        {
                            long tempWorkingSet;

                            if(Int64.TryParse(current.InnerText, out tempWorkingSet))
                            {
                                _minimumWorkingSet = tempWorkingSet;
                            }
                            else
                            {
                                Fail("Configuration error, MinWorkingSet must be an integral value.");
                            }
                        }
                        else
                        {
                            Fail("Configuration error, MinWorkingSet must be given a value.");
                        }
                    }
                    else if(String.Equals(current.Name, "MaxWorkingSet", StringComparison.OrdinalIgnoreCase))
                    {
                        if(!String.IsNullOrEmpty(current.InnerText))
                        {
                            long tempWorkingSet;

                            if(Int64.TryParse(current.InnerText, out tempWorkingSet))
                            {
                                _maximumWorkingSet = tempWorkingSet;
                            }
                            else
                            {
                                Fail("Configuration error, MaxWorkingSet must be an integral value.");
                            }
                        }
                        else
                        {
                            Fail("Configuration error, MaxWorkingSet must be given a value.");
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Transforms AssemblyReferenceGroup nodes into AssemblyReference objects.
        /// </summary>
        private bool ProcessAssemblyReferenceGroup(XmlNode section)
        {
            XmlNodeList allReferences = section.HasChildNodes ? section.ChildNodes : null;

            if(allReferences != null)
            {
                List<AssemblyReference> group = new List<AssemblyReference>();

                foreach(XmlNode current in allReferences)
                    if(String.Equals(current.Name, "AssemblyReference", StringComparison.OrdinalIgnoreCase))
                    {
                        XmlAttributeCollection allAttribs = current.Attributes;
                        string path = null, culture = null, version = null, publicKeyToken = null;

                        if(allAttribs != null)
                        {
                            foreach(XmlAttribute currentAttrib in allAttribs)
                            {
                                if(String.Equals(currentAttrib.Name, "Path", StringComparison.OrdinalIgnoreCase))
                                {
                                    path = currentAttrib.Value;
                                }
                                else if(String.Equals(currentAttrib.Name, "Culture", StringComparison.OrdinalIgnoreCase))
                                {
                                    culture = currentAttrib.Value;
                                }
                                else if(String.Equals(currentAttrib.Name, "Version", StringComparison.OrdinalIgnoreCase))
                                {
                                    version = currentAttrib.Value;
                                }
                                else if(String.Equals(currentAttrib.Name, "PublicKeyToken", StringComparison.OrdinalIgnoreCase))
                                {
                                    publicKeyToken = currentAttrib.Value;
                                }
                            }
                        }

                        if(path == null)
                        {
                            group.Add(new AssemblyReference(current.InnerText, version, culture, publicKeyToken));
                        }
                        else
                        {
                            group.Add(new AssemblyReference(current.InnerText, path));
                        }
                    }

                if(group.Count > 0)
                {
                    if(_externalDependencies.Length != 0)
                    {
                        group.AddRange(_externalDependencies);
                    }

                    _externalDependencies = group.ToArray();
                }
            }

            return true;
        }

        /// <summary>
        /// Transforms WorkflowGroup nodes into WorkflowEntry objects.
        /// </summary>
        private bool ProcessWorkflowGroup(XmlNode section)
        {
            XmlNodeList allWorkflows = section.HasChildNodes ? section.ChildNodes : null;

            if(allWorkflows != null)
            {
                XmlAttributeCollection allAttribs = section.Attributes;
                string groupName = null;

                if(allAttribs != null)
                {
                    foreach(XmlAttribute currentAttrib in allAttribs)
                    {
                        if(String.Equals(currentAttrib.Name, "Name", StringComparison.OrdinalIgnoreCase))
                        {
                            groupName = currentAttrib.Value;
                        }
                    }
                }

                if(groupName == null)
                {
                    Fail("Configuration error, WorkflowGroup has no Name attribute.");
                    return false;
                }

                List<WorkflowEntry> group = new List<WorkflowEntry>();

                foreach(XmlNode current in allWorkflows)
                {
                    if(String.Equals(current.Name, "Workflow", StringComparison.OrdinalIgnoreCase))
                    {
                        allAttribs = current.Attributes;
                        string name = null;

                        if(allAttribs != null)
                        {
                            foreach(XmlAttribute currentAttrib in allAttribs)
                            {
                                if(String.Equals(currentAttrib.Name, "Name", StringComparison.OrdinalIgnoreCase))
                                {
                                    name = currentAttrib.Value;
                                }
                            }
                        }

                        if(name == null)
                        {
                            Fail("Configuration error, Workflow has no Name attribute.");
                            return false;
                        }

                        Dictionary<string, string> arguments = new Dictionary<string, string>(StringComparer.Ordinal);

                        if(current.HasChildNodes)
                        {
                            XmlNodeList allArguments = current.ChildNodes;

                            foreach(XmlNode currentArg in allArguments)
                            {
                                if(String.Equals(currentArg.Name, "Argument", StringComparison.OrdinalIgnoreCase))
                                {
                                    allAttribs = currentArg.Attributes;

                                    if(allAttribs != null)
                                    {
                                        string key = null, value = null;

                                        foreach(XmlAttribute argAttrib in allAttribs)
                                        {
                                            if(String.Equals(argAttrib.Name, "Key", StringComparison.OrdinalIgnoreCase))
                                            {
                                                key = argAttrib.Value;
                                            }
                                        }

                                        if(key == null)
                                        {
                                            Fail("Configuration error, Argument has no Key attribute.");
                                            return false;
                                        }

                                        value = currentArg.InnerText ?? "";

                                        if(arguments.ContainsKey(key))
                                        {
                                            arguments[key] = value;
                                        }
                                        else
                                        {
                                            arguments.Add(key, value);
                                        }
                                    }
                                }
                            }
                        }

                        group.Add(new WorkflowEntry(name, arguments.ToArray()));
                    }
                }

                if(group.Count > 0)
                {
                    if(_workflowGroups.ContainsKey(groupName))
                    {
                        group.InsertRange(0, _workflowGroups[groupName]);
                        _workflowGroups[groupName] = group.ToArray();
                    }
                    else
                    {
                        _workflowGroups.Add(groupName, group.ToArray());
                    }
                }
            }

            return true;
        }
        #endregion

        #region Assembly Handling
        /// <summary>
        /// Ensures all external dependencies have been loaded, then loads all referenced assemblies by the 
        /// currently executing assembly, and recursively loads each of the referenced assemblies of the 
        /// initial dependency set until all dependencies have been loaded.
        /// </summary>
        private bool PreloadReferences()
        {
            if(!LoadExternalDependencies())
                return false;
            bool result = true;

            if(_preloadAssemblies)
            {
                Write("Preloading assemblies...  ");
                Assembly currentAsm = typeof(ServerLifecycleManager).Assembly;
                HashSet<string> inspected = new HashSet<string>();
                inspected.Add(currentAsm.GetName().ToString());
                LoadReferences(currentAsm, inspected);

                WriteLine("done.");
            }

            return result;
        }

        /// <summary>
        /// Loads the assemblies that are referenced by the input assembly, but only if that assembly has not
        /// already been inspected.
        /// </summary>
        private void LoadReferences(Assembly asm, HashSet<string> inspected)
        {
            AssemblyName[] allReferences = asm.GetReferencedAssemblies();

            foreach(AssemblyName toLoad in allReferences)
            {
                if(inspected.Add(toLoad.ToString()))
                {
                    Assembly loaded = AppDomain.CurrentDomain.Load(toLoad);
                    LoadReferences(loaded, inspected);
                }
            }
        }

        /// <summary>
        /// Loads any external dependencies specified in the configuration file into the current AppDomain.
        /// </summary>
        private bool LoadExternalDependencies()
        {
            bool result = true;

            if(_externalDependencies != null && _externalDependencies.Length > 0)
            {
                foreach(AssemblyReference currentReference in _externalDependencies)
                {
                    if(result)
                    {
                        Assembly asm = null;

                        if(currentReference.IsGlobalAssemblyCache)
                        {
                            GAC.RebuildGACAssemblyCache(false);
                            string gacName = GAC.TryResolveGACAssembly(currentReference.Name, currentReference.Culture, currentReference.Version, currentReference.PublicKeyToken);

                            if(gacName == null)
                                if(GAC.RebuildGACAssemblyCache(true))
                                    gacName = GAC.TryResolveGACAssembly(currentReference.Name, currentReference.Culture, currentReference.Version, currentReference.PublicKeyToken);

                            if(gacName != null)
                            {
                                try { asm = Assembly.Load(gacName); }
                                catch(Exception e)
                                {
                                    asm = null;
                                    Fail("External assembly \"" + gacName + "\" failed to load from global assembly cache", e);
                                    result = false;
                                }
                            }

                            if(asm == null && result)
                            {
                                asm = null;
                                Fail("External assembly \"" + gacName + "\" failed to load from global assembly cache");
                                result = false;
                            }
                        }
                        else
                        {
                            string fullPath = Path.Combine(currentReference.Path, currentReference.Name.EndsWith(".dll") ? currentReference.Name : (currentReference.Name + ".dll"));

                            if(File.Exists(fullPath))
                            {
                                try { asm = Assembly.LoadFrom(fullPath); }
                                catch(Exception e)
                                {
                                    asm = null;
                                    Fail("External assembly failed to load from \"" + fullPath + "\"", e);
                                    result = false;
                                }
                            }

                            if(asm == null && result)
                            {
                                asm = null;
                                Fail("External assembly failed to load from \"" + fullPath + "\"");
                                result = false;
                            }
                        }

                        if(result)
                            AppDomain.CurrentDomain.Load(asm.GetName());
                    }
                }
            }

            return result;
        }
        #endregion

        #region GC Handling
        private bool StartGCManager()
        {
            if(_enableGCManager)
            {
                TraceWriter.WriteTrace("SLM garbage collection manager enabled.");
                _gcmThreadStart = GCM_EntryPoint;
                _lastKnownWorkingSet = -1L;
                _nextForcedCollection = DateTime.Now.AddSeconds(5.0);
                _gcmRunning = true;
                _gcmThread = new Thread(_gcmThreadStart);
                _gcmThread.IsBackground = false;
                _gcmThread.Start();
            }
            else
            {
                TraceWriter.WriteTrace("SLM garbage collection manager disabled.");
            }

            return true;
        }

        private void GCM_EntryPoint()
        {
            while(_gcmRunning)
            {
                DateTime now = DateTime.Now;

                if(now >= _nextForcedCollection)
                {
                    if(_lastKnownWorkingSet == -1L)
                    {
                        _lastKnownWorkingSet = GC.GetTotalMemory(true);
                    }
                    else
                    {
                        bool shouldCollect = false;

                        if((_lastKnownWorkingSet / 1024L / 1024L) > _minimumWorkingSet)
                        {
                            shouldCollect = true;
                        }

                        if(shouldCollect)
                        {
                            TraceWriter.WriteTrace("Collecting...");
                            _lastKnownWorkingSet = GC.GetTotalMemory(true);
                            now = DateTime.Now;
                        }
                        else
                            _lastKnownWorkingSet = GC.GetTotalMemory(false);
                    }

                    _nextForcedCollection = now.AddSeconds(5.0);
                }
                else
                {
                    Thread.Sleep(512);
                }
            }
        }

        private void TerminateGCManager()
        {
            if(_enableGCManager)
            {
                if(_gcmThread != null)
                {
                    _gcmRunning = false;
                    _gcmThread.Join();
                }

                _gcmThread = null;
                _gcmThreadStart = null;
            }
        }
        #endregion

        #region Initialization Handling
        private void InitializeCommandLineArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();

            if(_arguments.Any())
            {
                for(int i = 0; i < _arguments.Length; i++)
                {
                    string[] arg = _arguments[i].Split(new[] { '=' });

                    if(arg.Length == 2)
                    {
                        arguments.Add(arg[0].Replace("/", string.Empty), arg[1]);
                    }
                }
            }

            foreach(KeyValuePair<string, string> argument in arguments)
            {
                if(argument.Key.Equals("lifecycleConfigFile", StringComparison.InvariantCultureIgnoreCase))
                {
                    _configFile = argument.Value;
                    continue;
                }
            }
        }

        /// <summary>
        /// Performs all necessary initialization such that the server is in a state that allows
        /// workflow execution.
        /// </summary>
        /// <returns>false if the initialization failed, otherwise true</returns>
        private bool InitializeServer()
        {
            bool result = true;

            try
            {
                string uriAddress = null;
                //string netTcpAddress = null;
                //string managementUri = null;
                string webServerPort = null;
                string webServerSslPort = null;

                Dictionary<string, string> arguments = new Dictionary<string, string>();

                if(_arguments.Any())
                {
                    for(int i = 0; i < _arguments.Length; i++)
                    {
                        string[] arg = _arguments[i].Split(new[] { '=' });
                        if(arg.Length == 2)
                        {
                            arguments.Add(arg[0].Replace("/", string.Empty), arg[1]);
                        }
                    }
                }

                foreach(KeyValuePair<string, string> argument in arguments)
                {
                    if(argument.Key.Equals("endpointAddress", StringComparison.InvariantCultureIgnoreCase))
                    {
                        uriAddress = argument.Value;
                        continue;
                    }

                    //if(argument.Key.Equals("netTcpAddress", StringComparison.InvariantCultureIgnoreCase))
                    //{
                    //    netTcpAddress = argument.Value;
                    //    continue;
                    //}

                    //if(argument.Key.Equals("managementEndpointAddress", StringComparison.InvariantCultureIgnoreCase))
                    //{
                    //    managementUri = argument.Value;
                    //    continue;
                    //}

                    if(argument.Key.Equals("webServerPort", StringComparison.InvariantCultureIgnoreCase))
                    {
                        webServerPort = argument.Value;
                        continue;
                    }

                    if(argument.Key.Equals("webServerSslPort", StringComparison.InvariantCultureIgnoreCase))
                    {
                        webServerSslPort = argument.Value;
                        continue;
                    }

                    if(argument.Key.Equals("lifecycleConfigFile", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _configFile = argument.Value;
                        continue;
                    }
                }


                uriAddress = uriAddress ?? ConfigurationManager.AppSettings["endpointAddress"];
                //netTcpAddress = netTcpAddress ?? ConfigurationManager.AppSettings["netTcpAddress"];
                //managementUri = managementUri ?? ConfigurationManager.AppSettings["managementEndpointAddress"];
                webServerPort = webServerPort ?? ConfigurationManager.AppSettings["webServerPort"];
                webServerSslPort = webServerSslPort ?? ConfigurationManager.AppSettings["webServerSslPort"];

                //if(string.IsNullOrEmpty(uriAddress))
                //{
                //    throw new FrameworkException("No Endpoint Address found in configuration file",
                //                                 "Dynamic Service Host", null, null);
                //}

                //if(string.IsNullOrEmpty(managementUri))
                //{
                //    throw new FrameworkException("No Endpoint Address found in configuration file",
                //                                 "Dynamic Service Host", null, null);
                //}

                _esbEndpoint = new EsbServicesEndpoint();

                StudioFileSystem fileSystem = new StudioFileSystem(Path.Combine(Environment.CurrentDirectory, "Studio Server"), new List<string>());


                _networkServer = new StudioNetworkServer("Studio Server", fileSystem, _esbEndpoint, ServerID);
                _isWebServerEnabled = false;

                Boolean.TryParse(ConfigurationManager.AppSettings["webServerEnabled"], out _isWebServerEnabled);

                if(_isWebServerEnabled)
                {
                    if(string.IsNullOrEmpty(webServerPort) && _isWebServerEnabled)
                    {
                        throw new ArgumentException(
                            "Web server port not set but web server is enabled. Please set the webServerPort value in the configuration file.");
                    }

                    int realPort;

                    if(!Int32.TryParse(webServerPort, out realPort))
                    {
                        throw new ArgumentException("Web server port is not valid. Please set the webServerPort value in the configuration file.");
                    }

                    List<IPEndPoint> endpoints = new List<IPEndPoint>();
                    var prefixes = new List<string>();
                    prefixes.Add(string.Format("http://*:{0}/", webServerPort));
                    endpoints.Add(new IPEndPoint(IPAddress.Any, realPort));

                    if(!string.IsNullOrEmpty(webServerSslPort) && _isWebServerEnabled)
                    {
                        prefixes.Add(string.Format("https://*:{0}/", webServerSslPort));
                    }

                    _prefixes = prefixes.ToArray();
                    _endpoints = endpoints.ToArray();
                    _uriAddress = uriAddress;
                }
            }
            catch(Exception ex)
            {
                result = false;
                Fail("Server initialization failed", ex);
            }

            return result;
        }
        #endregion

        #region Cleanup Handling
        /// <summary>
        /// Performs all necessary cleanup such that the server is gracefully moved to a state that does not allow
        /// workflow execution.
        /// </summary>
        /// <returns>false if the cleanup failed, otherwise true</returns>
        private bool CleanupServer()
        {
            bool result = true;

            try
            {
                _webserver.Stop();
            }
            catch(Exception)
            {
                result = false;
            }

            try
            {
                DebugDispatcher.Instance.Shutdown();
                BackgroundDispatcher.Instance.Shutdown();
            }
            catch(Exception)
            {
                result = false;
            }

            try
            {
                if(_networkServer != null)
                {
                    _networkServer.Stop();
                    _networkServer.Dispose();
                }
            }
            catch(Exception)
            {
                result = false;
            }

            try
            {
                if(_executionChannel != null)
                {
                    _executionChannel.Dispose();
                }
            }
            catch(Exception)
            {
                result = false;
            }

            try
            {
                if(_dataListChannel != null)
                {
                    _dataListChannel.Dispose();
                }
            }
            catch(Exception)
            {
                result = false;
            }

            TerminateGCManager();

            return result;
        }
        #endregion

        #region Workflow Handling
        /// <summary>
        /// Executes each workflow contained in the group indicated by <paramref name="groupName"/> in the same order that
        /// they were specified in the configuration file.
        /// </summary>
        /// <param name="groupName">The group of workflows to be executed.</param>
        /// <returns>false if the execution failed, otherwise true</returns>
        //private bool ExecuteWorkflowGroup(string groupName)
        //{
        //    WorkflowEntry[] entries;

        //    if(_workflowGroups.TryGetValue(groupName, out entries))
        //    {
        //        for(int i = 0; i < entries.Length; i++)
        //        {
        //            WorkflowEntry entry = entries[i];
        //            StringBuilder builder = new StringBuilder();

        //            if(entry.Arguments.Length > 0)
        //            {
        //                builder.AppendLine("<XmlData>");
        //                builder.AppendLine("  <ADL>");

        //                for(int k = 0; k < entry.Arguments.Length; k++)
        //                {
        //                    builder.AppendLine("<" + entry.Arguments[k].Key + ">" + entry.Arguments[k].Value + "</" + entry.Arguments[k].Key + ">");
        //                }

        //                builder.AppendLine("  </ADL>");
        //                builder.AppendLine("</XmlData>");
        //            }

        //            string requestXML = UnlimitedObject.GenerateServiceRequest(entry.Name, null, new List<string>(new string[] { builder.ToString() }), null);
        //            string result = null;

        //            try { result = _esbEndpoint.ExecuteCommand(requestXML, GlobalConstants.NullDataListID); }
        //            catch(Exception e)
        //            {
        //                Fail("Workflow \"" + entry.Name + "\" execution failed", e);
        //                return false;
        //            }

        //            if(ResultContainsError(result))
        //            {
        //                Fail("Workflow \"" + entry.Name + "\" execution failed", result);
        //                return false;
        //            }
        //        }
        //    }

        //    return true;
        //}

        /// <summary>
        /// Checks if any of the xml nodes in result are named "Error"
        /// </summary>
        /// <returns>true if result contained an xml node named error, otherwise false.</returns>
        private bool ResultContainsError(string result)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);

            if(doc.Name == "Error")
            {
                return true;
            }

            if(doc.HasChildNodes)
            {
                Queue<XmlNodeList> pendingLists = new Queue<XmlNodeList>();
                pendingLists.Enqueue(doc.ChildNodes);

                while(pendingLists.Count > 0)
                {
                    XmlNodeList list = pendingLists.Dequeue();

                    foreach(XmlNode node in list)
                    {
                        if(node.Name == "Error")
                        {
                            return true;
                        }

                        if(node.HasChildNodes)
                            pendingLists.Enqueue(node.ChildNodes);
                    }
                }
            }

            return false;
        }
        #endregion

        #region Failure Handling
        private void Fail(string message)
        {
            Fail(message, "");
        }

        private void Fail(string message, Exception e)
        {
            WriteLine("Critical Failure: " + message);

            if(e != null)
            {
                WriteLine("Details");
                WriteLine("--");
                WriteLine(e.Message);
                Write(e.StackTrace);
            }

            WriteLine("");
            WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        private void Fail(string message, string details)
        {
            WriteLine("Critical Failure: " + message);

            if(!String.IsNullOrEmpty(details))
            {
                WriteLine("Details");
                WriteLine("--");
                WriteLine(details);
            }

            WriteLine("");
            WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
        #endregion

        #region Disposal Handling
        /// <summary>
        /// Finalizer for ServerLifecycleManager called when garbage collected.
        /// </summary>
        ~ServerLifecycleManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// Public facing implementation of the Dispose interface
        /// </summary>
        public void Dispose()
        {
            if(_isDisposed)
                return;
            _isDisposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  Proper dispose pattern implementation to ensure Application Server is terminated correctly, even from finalizer.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed, otherwise false.</param>
        private void Dispose(bool disposing)
        {

            if(disposing)
            {
                //if(_redisProcess != null)
                //{
                //    _redisProcess.Kill();
                //    _redisProcess.Dispose();
                //}
                CleanupServer();
            }
            _webserver = null;
            _esbEndpoint = null;
            _executionChannel = null;
            //_redisProcess = null;
        }
        #endregion

        #region AssemblyReference
        private sealed class AssemblyReference
        {
            public static readonly AssemblyReference[] EmptyReferences = new AssemblyReference[0];

            private string _name;
            private string _version;
            private string _culture;
            private string _publicKeyToken;
            private string _path;

            public string Name { get { return _name; } }
            public string Version { get { return _version; } }
            public string Culture { get { return _culture; } }
            public string PublicKeyToken { get { return _publicKeyToken; } }
            public string Path { get { return _path; } }
            public bool IsGlobalAssemblyCache { get { return _path == null; } }

            public AssemblyReference(string name, string version, string culture, string publicKeyToken)
            {
                _name = name;
                _version = version;
                _culture = culture;
                _publicKeyToken = publicKeyToken;
            }

            public AssemblyReference(string name, string path)
            {
                _name = name;
                _path = path;
            }
        }
        #endregion

        #region WorkflowEntry
        private sealed class WorkflowEntry
        {
            private string _name;
            private KeyValuePair<string, string>[] _arguments;

            public string Name { get { return _name; } }
            public KeyValuePair<string, string>[] Arguments { get { return _arguments; } }

            public WorkflowEntry(string name, KeyValuePair<string, string>[] arguments)
            {
                _name = name;
                _arguments = arguments;
            }
        }
        #endregion

        #region External Services

        /// <summary>
        /// BUG 7850 - Loads the resource catalog.
        /// </summary>
        /// <returns></returns>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/03/13</date>
        bool LoadResourceCatalog()
        {
            Write("Loading resource catalog...  ");
            // First call to instance loads the catalog.
            var instance = Dev2.Runtime.Hosting.ResourceCatalog.Instance;
            WriteLine("done.");
            return true;
        }

        /// <summary>
        /// PBI 1018 - Loads the settings provider.
        /// </summary>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/03/07</date>
        bool LoadSettingsProvider()
        {
            Write("Loading settings provider...  ");
            // First call to instance loads the provider.
            var instance = Dev2.Runtime.Configuration.SettingsProvider.Instance;
            instance.Start(StudioMessaging.MessageAggregator, StudioMessaging.MessageBroker);
            WriteLine("done.");
            return true;
        }

        void UnloadSettingsProvider()
        {
            try
            {
                var instance = Dev2.Runtime.Configuration.SettingsProvider.Instance;
                instance.Stop(StudioMessaging.MessageAggregator);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                // Called when exiting so no use in throwing error!
            }
        }

        // PBI 5389 - Resources Assigned and Allocated to Server
        bool LoadServerWorkspace()
        {
            Write("Loading server workspace...  ");
            // First call to instance loads the server workspace.
            // ReSharper disable UnusedVariable
            var instance = WorkspaceRepository.Instance;
            // ReSharper restore UnusedVariable
            WriteLine("done.");
            return true;
        }

        // PBI 5389 - Resources Assigned and Allocated to Server
        bool LoadHostSecurityProvider()
        {
            // First call to instance loads the provider.
            // ReSharper disable UnusedVariable
            var instance = HostSecurityProvider.Instance;
            // ReSharper restore UnusedVariable
            return true;
        }

        private bool StartDataListServer()
        {
            // PBI : 5376 - Create instance of the Server compiler
            Write("Starting DataList Server...  ");

            DataListFactory.CreateServerDataListCompiler();
            // Now bootstrap the BinaryDataListStorage object since it can be silly ;(
            //BinaryDataListStorage binaryDataListStorage = new BinaryDataListStorage(GlobalConstants.NullEntryNamespace, Guid.NewGuid());
            //Thread.Sleep(1500);
            //binaryDataListStorage.Dispose();

            Write("done.");
            WriteLine("");
            return true;
        }

        private bool OpenNetworkExecutionChannel()
        {
            Write("Opening Execution Channel...  ");

            try
            {
                _executionChannel = new ExecutionServerChannel(StudioMessaging.MessageBroker, StudioMessaging.MessageAggregator, ExecutionStatusCallbackDispatcher.Instance);
                Write("done.");
                WriteLine("");
                return true;
            }
            catch(Exception e)
            {
                Write("fail.");
                WriteLine(e.Message);
                return false;
            }
        }

        private bool OpenNetworkDataListChannel()
        {
            Write("Opening DataList Channel...  ");

            try
            {
                IDataListServer datalListServer = DataListFactory.CreateDataListServer();
                _dataListChannel = new DataListServerChannel(StudioMessaging.MessageBroker, StudioMessaging.MessageAggregator, datalListServer);
                Write("done.");
                WriteLine("");
                return true;
            }
            catch(Exception e)
            {
                Write("fail.");
                WriteLine(e.Message);
                return false;
            }
        }

        private bool StartWebServer()
        {
            bool result = true;

            if(_isWebServerEnabled)
            {
                try
                {
                    _webserver = new Dev2.WebServer(_endpoints, _networkServer);
                    _webserver.Start(_uriAddress);
                    WriteLine("\r\nWeb Server Started");
                    new List<string>(_prefixes).ForEach(c => WriteLine(string.Format("Web server listening at {0}", c)));
                }
                catch(Exception e)
                {
                    result = false;
                    Fail("Webserver failed to start", e);
                }
            }

            return result;
        }

        #endregion

        #region Output Handling

        internal static void WriteLine(string message)
        {
            if(Environment.UserInteractive)
            {
                Console.WriteLine(message);
            }
            else
            {
                TraceWriter.WriteTrace(message);
            }
        }

        internal static void Write(string message)
        {
            if(Environment.UserInteractive)
            {
                Console.Write(message);
            }
            else
            {
                TraceWriter.WriteTrace(message);
            }
        }

        #endregion Output Handling
    }
}

