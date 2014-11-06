
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using CommandLine;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Reflection;
using Dev2.Data;
using Dev2.Data.Storage;
using Dev2.DataList.Contract;
using Dev2.Diagnostics.Debug;
using Dev2.Instrumentation;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer;
using Dev2.Services.Security.MoqInstallerActions;
using Dev2.Workspaces;


// ReSharper disable InconsistentNaming
namespace Dev2
{
    /// <summary>
    /// PBI 5278
    /// Application Server Life-cycle Manager
    /// Facilitates start-up, execution and tear-down of the application server.
    /// </summary>
    sealed class ServerLifecycleManager : IDisposable
    {
   
        #region Constants

        const string DefaultConfigFileName = "LifecycleConfig.xml";

        // set to true when in trace mode ;)
        const bool LogTraceInfo = false;

        #endregion

        #region Static Members

        static ServerLifecycleManager _singleton;

        #endregion

        #region Entry Point

        /// <summary>
        /// Entry Point for application server.
        /// </summary>
        /// <param name="arguments">Command line arguments passed to executable.</param>
        static int Main(string[] arguments)
        {
            var result = 0;

            try
            {

                CommandLineParameters options = new CommandLineParameters();
                CommandLineParser parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
                if(!parser.ParseArguments(arguments, options))
                {
                    return 80;
                }

                bool commandLineParameterProcessed = false;
                Dev2Logger.EnableInfoOutput = true;
                if(options.Install)
                {
                    Dev2Logger.Log.Info("Starting Install");
                    commandLineParameterProcessed = true;

                    if(!EnsureRunningAsAdministrator(arguments))
                    {
                        Dev2Logger.Log.Info("Cannot install because the server is not running as an admin user");
                        return result;
                    }

                    if(!WindowsServiceManager.Install())
                    {
                        result = 81;
                        Dev2Logger.Log.Info("Install Success Result is 81");
                    }
                }

                if(options.StartService)
                {
                    Dev2Logger.Log.Info("Starting Service");
                    commandLineParameterProcessed = true;

                    if(!EnsureRunningAsAdministrator(arguments))
                    {
                        Dev2Logger.Log.Info("Cannot start because the server is not running as an admin user");
                        return result;
                    }

                    if(!WindowsServiceManager.StartService(null))
                    {
                        Dev2Logger.Log.Info("Starting Service success. result 83");
                        result = 83;
                    }
                }

                if(options.StopService)
                {
                    Dev2Logger.Log.Info("Stopping Service");
                    commandLineParameterProcessed = true;

                    if(!EnsureRunningAsAdministrator(arguments))
                    {
                        Dev2Logger.Log.Info("Cannot stop because the server is not running as an admin user");
                        return result;
                    }

                    if(!WindowsServiceManager.StopService(null))
                    {
                        Dev2Logger.Log.Info("Stopping Service success. result 84");
                        result = 84;
                    }
                }

                if(options.Uninstall)
                {
                    Dev2Logger.Log.Info("Uninstall Service");
                    commandLineParameterProcessed = true;

                    if(!EnsureRunningAsAdministrator(arguments))
                    {
                        Dev2Logger.Log.Info("Cannot uninstall because the server is not running as an admin user");
                        return result;
                    }

                    if(!WindowsServiceManager.Uninstall())
                    {
                        Dev2Logger.Log.Info("Uninstall Service success. result 92");
                        result = 82;
                    }
                }

                if(commandLineParameterProcessed)
                {
                    Dev2Logger.Log.Info("Command line processed. Returning");
                    return result;
                }

                if(Environment.UserInteractive || options.IntegrationTestMode)
                {
                    Dev2Logger.Log.Info("** Starting In Interactive Mode ( " + options.IntegrationTestMode + " ) **");
                    using(_singleton = new ServerLifecycleManager(arguments))
                    {
                        result = _singleton.Run(true);
                    }

                    _singleton = null;
                }
                else
                {
                    Dev2Logger.Log.Info("** Starting In Service Mode **");
                    // running as service
                    using(var service = new ServerLifecycleManagerService())
                    {
                        ServiceBase.Run(service);
                    }
                }
            }
            catch(Exception err)
            {
                Dev2Logger.Log.Error("Error Starting Server", err);
// ReSharper disable InvokeAsExtensionMethod
                Dev2Logger.Log.Error("Error Starting Server. Stack trace", err);
// ReSharper restore InvokeAsExtensionMethod
                throw;
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
                Dev2Logger.Log.Info("** Service Started **");
                _singleton = new ServerLifecycleManager(null);
                _singleton.Run(false);
            }

            protected override void OnStop()
            {
                Dev2Logger.Log.Info("** Service Stopped **");
                _singleton.Stop(false, 0);
                _singleton = null;
            }
        }

        #endregion

        #region Instance Fields

        bool _isDisposed;
        bool _isWebServerEnabled;
        bool _isWebServerSslEnabled;
        bool _preloadAssemblies;
        readonly string[] _arguments;
        AssemblyReference[] _externalDependencies;
        readonly Dictionary<string, WorkflowEntry[]> _workflowGroups;
        Dev2Endpoint[] _endpoints;

        string _configFile;

        // START OF GC MANAGEMENT
        bool _enableGcManager;
        long _minimumWorkingSet;
        long _lastKnownWorkingSet;
        volatile bool _gcmRunning;
        DateTime _nextForcedCollection;
        Thread _gcmThread;
        ThreadStart _gcmThreadStart;
        Timer _timer;
        IDisposable _owinServer;
        readonly IPulseLogger _pulseLogger;

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
        ServerLifecycleManager(string[] arguments)
        {
            _pulseLogger = new PulseLogger(60000);
            _pulseLogger.Start();
            _arguments = arguments ?? new string[0];
            _configFile = DefaultConfigFileName;
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
        int Run(bool interactiveMode)
        {
            int result = 0;
            bool didBreak = false;

            Tracker.StartServer();

            // ** Perform Moq Installer Actions For Development ( DEBUG config ) **
#if DEBUG

            try
            {
                var miq = MoqInstallerActionFactory.CreateInstallerActions();
                miq.ExecuteMoqInstallerActions();
            }
            catch(Exception e)
            {
                // Throw new exception to make it easy for developer to understand issue
                throw new Exception("Ensure no Warewolf service is running on this machine. Mocking installer actions for DEBUG config failed to create Warewolf Administrators group and/or to add current user to it [ " + e.Message + " ]");
            }
#endif


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

            // remove due to hanging installer ;) 02.04.2014
            if(!didBreak && !PreloadReferences())
            {
                result = 2;
                didBreak = true;
            }

            if(!didBreak && !StartGcManager())
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

            if(!didBreak && !StartWebServer())
            {
                result = 4;
                didBreak = true;
            }

            // PBI 1018 - Settings Framework (TWR: 2013.03.07)
            if(!didBreak && !LoadSettingsProvider())
            {
                result = 93;
                didBreak = true;
            }

            if(!didBreak && !ConfigureLoggging())
            {
                result = 92;
                didBreak = true;
            }


            if(!didBreak)
            {
                // set background timer to query network computer name list every 15 minutes ;)
                _timer = new Timer(RefreshComputerList, null, 10000, GlobalConstants.NetworkComputerNameQueryFreq);
                result = ServerLoop(interactiveMode);
                StartPulseLogger();
            }
            else
            {
                result = Stop(true, result);
            }

            return result;
        }

        void StartPulseLogger()
        {
            _pulseLogger.Start();
        }

        void RefreshComputerList(object state)
        {
            GetComputerNames.GetComputerNamesList();
        }

        int Stop(bool didBreak, int result)
        {

            // PBI 1018 - Settings Framework (TWR: 2013.03.07)
            UnloadSettingsProvider();

            Tracker.Stop();

            if(!didBreak)
            {
                Dispose();
            }
            else
            {
                TerminateGcManager();
            }

            Write(string.Format("Existing with exitcode {0}", result));

            return result;
        }

        int ServerLoop(bool interactiveMode)
        {
            if(interactiveMode)
            {
                Write("Press <ENTER> to terminate service and/or web server if started");
                if(EnvironmentVariables.IsServerOnline)
                {
                    Console.ReadLine();
                }
                else
                {
                    Write("Failed to start Server");
                }

                return Stop(false, 0);
            }

            return 0;
        }

        bool SetWorkingDirectory()
        {
            bool result = true;

            try
            {
                // Brendon.Page - The following line is has had it warning suppressed because if the working directory can't be set
                //                then it can't be guaranteed that the server will operate correctly, and in this case the desired
                //                behavior is a fail with an exception.
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

        static bool EnsureRunningAsAdministrator(string[] arguments)
        {

            try
            {
                if(!IsElevated())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location) { Verb = "runas", Arguments = string.Join(" ", arguments) };

                    Process process = new Process { StartInfo = startInfo };

                    try
                    {
                        process.Start();
                    }
                    catch(Exception e)
                    {
                        LogException(e);
                    }

                    return false;
                }
            }
            catch(Exception e)
            {
                LogException(e);
            }

            return true;
        }

        static bool IsElevated()
        {
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            if(currentIdentity != null)
            {
                WindowsPrincipal principal = new WindowsPrincipal(currentIdentity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return false;
        }

        #endregion

        #region Configuration Handling

        /// <summary>
        /// Reads the configuration file and records the entries in class level fields.
        /// </summary>
        /// <param name="filePath">The relative or fully qualified file path to the configuration file</param>
        /// <returns>true if the configuration file was loaded correctly, false otherwise</returns>
        bool LoadConfiguration(string filePath)
        {
            bool recreate = false;
            bool result = true;
            XmlDocument document = new XmlDocument();
            if(File.Exists(filePath))
            {
                try
                {
                    document.Load(filePath);
                }
                catch(Exception e)
                {
                    Fail("Configuration load error", e);
                    result = false;
                }
            }
            else
                recreate = true;

            if(recreate)
            {
                WriteLine("Configuration file \"" + filePath + "\" does not exist, creating empty configuration file...");

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("<configuration>");

                // logging info
                builder.AppendLine("\t<Logging>");
                builder.AppendLine("\t\t<Debug Enabled=\"true\" />");
                builder.AppendLine("\t\t<Error Enabled=\"true\" />");
                builder.AppendLine("\t\t<Info Enabled=\"true\" />");
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
                if(!LogTraceInfo)
                // ReSharper restore ConvertIfStatementToConditionalTernaryExpression
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                // ReSharper disable HeuristicUnreachableCode
#pragma warning disable 162
                {
                    builder.AppendLine("\t\t<Trace Enabled=\"false\" />");
                }
#pragma warning restore 162
                // ReSharper restore HeuristicUnreachableCode
                // ReSharper disable RedundantIfElseBlock
                else
                // ReSharper restore RedundantIfElseBlock
                // ReSharper disable HeuristicUnreachableCode
#pragma warning disable 162
                {
                    builder.AppendLine("\t\t<Trace Enabled=\"true\" />");
                }
#pragma warning restore 162
                // ReSharper restore HeuristicUnreachableCode

                builder.AppendLine("\t</Logging>");
                // end logging info

                builder.AppendLine("\t<GCManager Enabled=\"false\">");
                builder.AppendLine("\t\t<MinWorkingSet>60</MinWorkingSet>");
                builder.AppendLine("\t\t<MaxWorkingSet>6144</MaxWorkingSet>");
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
                    document.Load(filePath);
                }
                catch(Exception ex)
                {
                    LogException(ex);
                    result = false;
                }
            }
            if(result)
            {
                result = LoadConfiguration(document);
            }
            return result;
        }

        bool LoadConfiguration(XmlDocument document)
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

                        if(String.Equals(section.Name, "Logging", StringComparison.OrdinalIgnoreCase))
                        {
                            if(!ProcessLoggingConfiguration(section))
                            {
                                result = false;
                            }
                        }

                        if(String.Equals(section.Name, "GCManager", StringComparison.OrdinalIgnoreCase))
                        {
                            if(!ProcessGcManager(section))
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

        internal bool ReadBooleanSection(XmlNode section, string sectionName, ref bool result, ref bool setter)
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


        bool ProcessGcManager(XmlNode section)
        {
            XmlAttributeCollection sectionAttribs = section.Attributes;

            if(sectionAttribs != null)
            {
                foreach(XmlAttribute sAttrib in sectionAttribs)
                {
                    if(String.Equals(sAttrib.Name, "Enabled", StringComparison.OrdinalIgnoreCase))
                    {
                        _enableGcManager = String.Equals(sAttrib.Value, "True", StringComparison.OrdinalIgnoreCase);
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

        bool ProcessLoggingConfiguration(XmlNode section)
        {

            XmlNodeList allReferences = section.HasChildNodes ? section.ChildNodes : null;

            if(allReferences != null)
            {
                foreach(XmlNode current in allReferences)
                {
                    var attr = current.Attributes;

                    if(String.Equals(current.Name, "Debug", StringComparison.OrdinalIgnoreCase))
                    {

                        if(attr != null && !String.IsNullOrEmpty(attr["Enabled"].Value))
                        {
                            bool result;

                            if(Boolean.TryParse(attr["Enabled"].Value, out result))
                            {
                                Dev2Logger.EnableDebugOutput = result;
                            }
                            else
                            {
                                Fail("Configuration error, Debug must be an boolean value.");
                            }
                        }
                        else
                        {
                            Fail("Configuration error, Debug must be given a value.");
                        }
                    }
                    else if(String.Equals(current.Name, "Trace", StringComparison.OrdinalIgnoreCase))
                    {
                        if(attr != null && !String.IsNullOrEmpty(attr["Enabled"].Value))
                        {
                            bool result;

                            if(Boolean.TryParse(attr["Enabled"].Value, out result))
                            {
                                Dev2Logger.EnableTraceOutput = result;
                            }
                            else
                            {
                                Fail("Configuration error, Trace must be an boolean value.");
                            }
                        }
                        else
                        {
                            Fail("Configuration error, Trace must be given a value.");
                        }
                    }
                    else if(String.Equals(current.Name, "Error", StringComparison.OrdinalIgnoreCase))
                    {
                        if(attr != null && !String.IsNullOrEmpty(attr["Enabled"].Value))
                        {
                            bool result;

                            if(Boolean.TryParse(attr["Enabled"].Value, out result))
                            {
                                Dev2Logger.EnableErrorOutput = result;
                            }
                            else
                            {
                                Fail("Configuration error, Error must be an boolean value.");
                            }
                        }
                        else
                        {
                            Fail("Configuration error, Error must be given a value.");
                        }
                    }
                    else if(String.Equals(current.Name, "Info", StringComparison.OrdinalIgnoreCase))
                    {
                        if(attr != null && !String.IsNullOrEmpty(attr["Enabled"].Value))
                        {
                            bool result;

                            if(Boolean.TryParse(attr["Enabled"].Value, out result))
                            {
                                Dev2Logger.EnableInfoOutput = result;
                            }
                            else
                            {
                                Fail("Configuration error, Info must be an boolean value.");
                            }
                        }
                        else
                        {
                            Fail("Configuration error, Info must be given a value.");
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Transforms AssemblyReferenceGroup nodes into AssemblyReference objects.
        /// </summary>
        bool ProcessAssemblyReferenceGroup(XmlNode section)
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

                        // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
                        if(path == null)
                        // ReSharper restore ConvertIfStatementToConditionalTernaryExpression
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
        bool ProcessWorkflowGroup(XmlNode section)
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
                                        string key = null;

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

                                        string value = currentArg.InnerText;

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

                        group.Add(new WorkflowEntry(name));
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
        bool PreloadReferences()
        {
            try
            {
                if(!LoadExternalDependencies())
                {
                    return false;
                }

                const bool Result = true;

                if(_preloadAssemblies)
                {
                    Write("Preloading assemblies...  ");
                    Assembly currentAsm = typeof(ServerLifecycleManager).Assembly;
                    HashSet<string> inspected = new HashSet<string> { currentAsm.GetName().ToString(), "GroupControls" };
                    LoadReferences(currentAsm, inspected);

                    WriteLine("done.");
                }

                return Result;
            }
            catch(Exception e)
            {
                LogException(e);
                return false;
            }
        }

        /// <summary>
        /// Loads the assemblies that are referenced by the input assembly, but only if that assembly has not
        /// already been inspected.
        /// </summary>
        void LoadReferences(Assembly asm, HashSet<string> inspected)
        {
            AssemblyName[] allReferences = asm.GetReferencedAssemblies();

            foreach(AssemblyName toLoad in allReferences)
            {
                if(!inspected.Contains(toLoad.Name))
                {
                    inspected.Add(toLoad.Name);

                    // ReSharper disable ConditionIsAlwaysTrueOrFalse
                    if(LogTraceInfo)
                    // ReSharper restore ConditionIsAlwaysTrueOrFalse
                    // ReSharper disable HeuristicUnreachableCode
#pragma warning disable 162
                    {
                        WriteLine("Loading Reference [ " + toLoad.FullName + " ]");
                    }
#pragma warning restore 162
                    // ReSharper restore HeuristicUnreachableCode
                    Assembly loaded = AppDomain.CurrentDomain.Load(toLoad);
                    LoadReferences(loaded, inspected);
                }
            }
        }

        /// <summary>
        /// Loads any external dependencies specified in the configuration file into the current AppDomain.
        /// </summary>
        bool LoadExternalDependencies()
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
                                try
                                {
                                    // ReSharper disable ConditionIsAlwaysTrueOrFalse
                                    if(LogTraceInfo)
                                    // ReSharper restore ConditionIsAlwaysTrueOrFalse
                                    // ReSharper disable HeuristicUnreachableCode
#pragma warning disable 162
                                    {
                                        WriteLine("Loading External Dependencies [ " + gacName + " ]");
                                    }
#pragma warning restore 162
                                    // ReSharper restore HeuristicUnreachableCode
                                    asm = Assembly.Load(gacName);
                                }
                                catch(Exception e)
                                {
                                    asm = null;
                                    Fail("External assembly \"" + gacName + "\" failed to load from global assembly cache", e);
                                    result = false;
                                }
                            }

                            if(asm == null && result)
                            {
                                Fail("External assembly \"" + gacName + "\" failed to load from global assembly cache");
                                result = false;
                            }
                        }
                        else
                        {
                            string fullPath = Path.Combine(currentReference.Path, currentReference.Name.EndsWith(".dll") ? currentReference.Name : (currentReference.Name + ".dll"));

                            if(File.Exists(fullPath))
                            {
                                try
                                {
                                    // ReSharper disable ConditionIsAlwaysTrueOrFalse
                                    if(LogTraceInfo)
                                    // ReSharper restore ConditionIsAlwaysTrueOrFalse
                                    // ReSharper disable HeuristicUnreachableCode
#pragma warning disable 162
                                    {
                                        WriteLine("Loading [ " + currentReference.Name + " ]");
                                    }
#pragma warning restore 162
                                    // ReSharper restore HeuristicUnreachableCode
                                    asm = Assembly.LoadFrom(fullPath);
                                }
                                catch(Exception e)
                                {
                                    asm = null;
                                    Fail("External assembly failed to load from \"" + fullPath + "\"", e);
                                    result = false;
                                }
                            }

                            if(asm == null && result)
                            {
                                Fail("External assembly failed to load from \"" + fullPath + "\"");
                                result = false;
                            }
                        }

                        if(result)
                        {
                            AppDomain.CurrentDomain.Load(asm.GetName());
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region GC Handling

        bool StartGcManager()
        {
            if(_enableGcManager)
            {
                WriteLine("SLM garbage collection manager enabled.");
                _gcmThreadStart = GcmEntryPoint;
                _lastKnownWorkingSet = -1L;
                _nextForcedCollection = DateTime.Now.AddSeconds(5.0);
                _gcmRunning = true;
                _gcmThread = new Thread(_gcmThreadStart) { IsBackground = false };
                _gcmThread.Start();
            }
            else
            {
                WriteLine("SLM garbage collection manager disabled.");
            }

            return true;
        }

        void GcmEntryPoint()
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
                        bool shouldCollect = (_lastKnownWorkingSet / 1024L / 1024L) > _minimumWorkingSet;

                        if(shouldCollect)
                        {
                            WriteLine("Collecting...");
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

        void TerminateGcManager()
        {
            if(_enableGcManager)
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

        void InitializeCommandLineArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();

            if(_arguments.Any())
            {
                foreach(string t in _arguments)
                {
                    string[] arg = t.Split(new[] { '=' });

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
                }
            }
        }

        /// <summary>
        /// Performs all necessary initialization such that the server is in a state that allows
        /// workflow execution.
        /// </summary>
        /// <returns>false if the initialization failed, otherwise true</returns>
        bool InitializeServer()
        {
            bool result = true;

            try
            {
                string webServerPort = null;
                string webServerSslPort = null;

                Dictionary<string, string> arguments = new Dictionary<string, string>();

                if(_arguments.Any())
                {
                    foreach(string t in _arguments)
                    {
                        string[] arg = t.Split(new[] { '=' });
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
                        continue;
                    }

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
                    }
                }

                GlobalConstants.WebServerPort = webServerPort = webServerPort ?? ConfigurationManager.AppSettings["webServerPort"];
                GlobalConstants.WebServerSslPort = webServerSslPort = webServerSslPort ?? ConfigurationManager.AppSettings["webServerSslPort"];

                _isWebServerEnabled = false;

                Boolean.TryParse(ConfigurationManager.AppSettings["webServerEnabled"], out _isWebServerEnabled);
                Boolean.TryParse(ConfigurationManager.AppSettings["webServerSslEnabled"], out _isWebServerSslEnabled);

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

                    var endpoints = new List<Dev2Endpoint>();

                    var httpEndpoint = new IPEndPoint(IPAddress.Any, realPort);
                    var httpUrl = string.Format("http://*:{0}/", webServerPort);
                    endpoints.Add(new Dev2Endpoint(httpEndpoint, httpUrl));

                    EnvironmentVariables.WebServerUri = httpUrl.Replace("*", Environment.MachineName);


                    // start SSL traffic if it is enabled ;)
                    if(!string.IsNullOrEmpty(webServerSslPort) && _isWebServerSslEnabled)
                    {
                        int realWebServerSslPort;
                        Int32.TryParse(webServerSslPort, out realWebServerSslPort);

                        var sslCertPath = ConfigurationManager.AppSettings["sslCertificateName"];

                        if(!string.IsNullOrEmpty(sslCertPath))
                        {
                            var httpsEndpoint = new IPEndPoint(IPAddress.Any, realWebServerSslPort);
                            var httpsUrl = string.Format("https://*:{0}/", webServerSslPort);
                            var canEnableSsl = HostSecurityProvider.Instance.EnsureSSL(sslCertPath, httpsEndpoint);

                            if(canEnableSsl)
                            {
                                endpoints.Add(new Dev2Endpoint(httpsEndpoint, httpsUrl, sslCertPath));
                            }
                            else
                            {
                                WriteLine("Could not start webserver to listen for SSL traffic with cert [ " + sslCertPath + " ]");
                            }
                        }
                    }

                    _endpoints = endpoints.ToArray();
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
        internal bool CleanupServer()
        {
            bool result = true;

            try
            {
                if(_owinServer != null)
                {
                    _owinServer.Dispose();
                    _owinServer = null;
                }
            }
            catch(Exception ex)
            {
                LogException(ex);
                result = false;
            }

            try
            {
                DebugDispatcher.Instance.Shutdown();
                BackgroundDispatcher.Instance.Shutdown();
            }
            catch(Exception ex)
            {
                LogException(ex);
                result = false;
            }

            // shutdown the storage layer ;)
            try
            {
                BinaryDataListStorageLayer.Teardown();
            }
            catch(Exception e)
            {
                LogException(e);
            }

            TerminateGcManager();

            return result;
        }

        #endregion

        #region Workflow Handling

        /*
        /// <summary>
        /// Executes each workflow contained in the group indicated by <paramref name="groupName"/> in the same order that
        /// they were specified in the configuration file.
        /// </summary>
        /// <param name="groupName">The group of workflows to be executed.</param>
        /// <returns>false if the execution failed, otherwise true</returns>
        bool ExecuteWorkflowGroup(string groupName)
        {
            WorkflowEntry[] entries;

            if(_workflowGroups.TryGetValue(groupName, out entries))
            {
                foreach(WorkflowEntry entry in entries)
                {
                    StringBuilder builder = new StringBuilder();

                    if(entry.Arguments.Length > 0)
                    {
                        builder.AppendLine("<XmlData>");
                        builder.AppendLine("  <ADL>");

                        foreach(KeyValuePair<string, string> t in entry.Arguments)
                        {
                            builder.AppendLine("<" + t.Key + ">" + t.Value + "</" + t.Key + ">");
                        }

                        builder.AppendLine("  </ADL>");
                        builder.AppendLine("</XmlData>");
                    }

                    string requestXML = new UnlimitedObject().GenerateServiceRequest(entry.Name, null, new List<string>(new[] { builder.ToString() }), null);
                    Guid result;

                    try
                    {
                        ErrorResultTO errors;
                        IDSFDataObject dataObj = new DsfDataObject(requestXML, GlobalConstants.NullDataListID);
                        result = _esbEndpoint.ExecuteRequest(dataObj, null, GlobalConstants.ServerWorkspaceID, out errors);
                    }
                    catch(Exception e)
                    {
                        Fail("Workflow \"" + entry.Name + "\" execution failed", e);
                        return false;
                    }

                    if(result == Guid.Empty)
                    {
                        Fail("Workflow \"" + entry.Name + "\" execution failed");
                        return false;
                    }
                }
            }

            return true;
        }
*/

        #endregion

        #region Failure Handling

        static void Fail(string message, Exception e)
        {
            var ex = e;
            var errors = new StringBuilder();
            while(ex != null)
            {
                errors.AppendLine(ex.Message);
                errors.AppendLine(ex.StackTrace);
                ex = ex.InnerException;
            }

            WriteLine("Critical Failure: " + message);
            WriteLine(errors.ToString());

            WriteLine("");
        }

        static void Fail(string message, string details = "")
        {
            WriteLine("Critical Failure: " + message);

            if(!String.IsNullOrEmpty(details))
            {
                WriteLine("Details");
                WriteLine("--");
                WriteLine(details);
            }

            WriteLine("");

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
        void Dispose(bool disposing)
        {

            if(disposing)
            {
                CleanupServer();
            }

            if(_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            _owinServer = null;
        }

        #endregion

        #region AssemblyReference

        sealed class AssemblyReference
        {
            public static readonly AssemblyReference[] EmptyReferences = new AssemblyReference[0];

            readonly string _name;
            readonly string _version;
            readonly string _culture;
            readonly string _publicKeyToken;
            readonly string _path;

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

        sealed class WorkflowEntry
        {
            readonly string _name;
            // ReSharper disable UnusedMember.Local
            public string Name { get { return _name; } }
            // ReSharper restore UnusedMember.Local

            public WorkflowEntry(string name)
            {
                _name = name;
            }
        }

        #endregion

        #region External Services

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/03/13</date>
        bool LoadResourceCatalog()
        {
            MigrateOldResources();
            Write("Loading resource catalog...  ");
            // First call initializes instance
#pragma warning disable 168
            // ReSharper disable UnusedVariable
            var catalog = ResourceCatalog.Instance;
            // ReSharper restore UnusedVariable
#pragma warning restore 168
            WriteLine("done.");
            return true;
        }

        static void MigrateOldResources()
        {
            var oldServicesFolder = Path.Combine(EnvironmentVariables.ApplicationPath, "Services");
            var oldSourcesFolder = Path.Combine(EnvironmentVariables.ApplicationPath, "Sources");
            if(Directory.Exists(oldServicesFolder))
            {
                Write("Migrating old services...  ");
                MigrateResources(oldServicesFolder);
                WriteLine("done.");
            }
            if(Directory.Exists(oldSourcesFolder))
            {
                Write("Migrating old sources...  ");
                MigrateResources(oldSourcesFolder);
                WriteLine("done.");
            }
        }

        static void MigrateResources(string oldResourceFolder)
        {
            var oldResourceFiles = Directory.EnumerateFiles(oldResourceFolder);
            try
            {
                foreach(var oldResourceFile in oldResourceFiles)
                {
                    var oldResourceXml = XElement.Load(oldResourceFile);
                    ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, oldResourceXml.ToStringBuilder());
                }
                DirectoryHelper.CleanUp(oldResourceFolder);
            }
            catch(Exception ex)
            {
                Dev2Logger.Log.Error("Dev2.ServerLifecycleManager - Error Migrating", ex);
            }
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
            Runtime.Configuration.SettingsProvider.WebServerUri = EnvironmentVariables.WebServerUri;
            //            var instance = SettingsProvider.Instance;
            //            instance.Start();
            WriteLine("done.");
            return true;
        }

        void UnloadSettingsProvider()
        {
            try
            {
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                // Called when exiting so no use in throwing error!
            }
        }

        bool ConfigureLoggging()
        {
            try
            {
                Write("Configure logging...  ");

                // First call to instance loads the provider.
                var instance = Runtime.Configuration.SettingsProvider.Instance;
                var settings = instance.Configuration;
                Dev2Logger.LoggingSettings = settings.Logging;
               
                WriteLine("done.");
                return true;
            }
            catch(Exception e)
            {
                Write("fail.");
                WriteLine(e.Message);
                return false;
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

        bool StartDataListServer()
        {
            // PBI : 5376 - Create instance of the Server compiler
            Write("Starting DataList Server...  ");

            DataListFactory.CreateServerDataListCompiler();
            BinaryDataListStorageLayer.Setup();

            var mbReserved = BinaryDataListStorageLayer.GetCapacityMemoryInMb();

            Write(" [ Reserving " + mbReserved.ToString("#") + " MBs of cache ] ");

            Write("done.");
            WriteLine("");
            return true;
        }

        bool StartWebServer()
        {
            bool result = true;

            if(_isWebServerEnabled || _isWebServerSslEnabled)
            {
                try
                {
                    try
                    {
                        _owinServer = WebServerStartup.Start(_endpoints);
                        EnvironmentVariables.IsServerOnline = true; // flag server as active
                        WriteLine("\r\nWeb Server Started");
                        foreach(var endpoint in _endpoints)
                        {
                            WriteLine(string.Format("Web server listening at {0}", endpoint.Url));
                        }
                        SetStarted();
                    }
                    catch(Exception e)
                    {
                        LogException(e);
                        Fail("Webserver failed to start", e);
                        Console.ReadLine();
                    }
                }
                catch(Exception e)
                {
                    result = false;
                    EnvironmentVariables.IsServerOnline = false; // flag server as inactive
                    Fail("Webserver failed to start", e);
                    Console.ReadLine();
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
                Dev2Logger.Log.Info(message);
            }
            else
            {
                Dev2Logger.Log.Info(message);
            }

        }

        internal static void Write(string message)
        {
            if(Environment.UserInteractive)
            {
                Console.Write(message);
                Dev2Logger.Log.Info(message);
            }
            else
            {
                Dev2Logger.Log.Info(message);
            }
        }

        #endregion Output Handling
        static void SetStarted()
        {
            try
            {
                if (File.Exists(".\\ServerStarted"))
                {
                    File.Delete(".\\ServerStarted");
                }
                File.WriteAllText(".\\ServerStarted", DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception err)
            {
                Dev2Logger.Log.Error(err);
            }
        }
        static void LogException(Exception ex)
        {
            Dev2Logger.Log.Error("Dev2.ServerLifecycleManager", ex);
        }
    }
}

