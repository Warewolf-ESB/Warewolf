
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
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Factories;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Runtime.Execution;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Dev2.Activities
{
    public class DsfExecuteCommandLineActivity : DsfActivityAbstract<string>
    {
        #region Fields

        string _commandFileName;
        string _commandResult;
        static string _fullPath;
        Process _process;
        NativeActivityContext _nativeActivityContext;
        ProcessPriorityClass _commandPriority = ProcessPriorityClass.Normal;

        #endregion

        /// <summary>
        /// Gets or sets the name of the recordset.
        /// </summary>  
        [Inputs("CommandFileName")]
        [FindMissing]
        // ReSharper disable ConvertToAutoProperty
        public string CommandFileName
        // ReSharper restore ConvertToAutoProperty
        {
            get
            {
                return _commandFileName;
            }
            set
            {
                _commandFileName = value;
            }
        }

        [Inputs("CommandPriority")]
        public ProcessPriorityClass CommandPriority { get { return _commandPriority; } set { _commandPriority = value; } }

        [Outputs("CommandResult")]
        [FindMissing]
        // ReSharper disable ConvertToAutoProperty
        public string CommandResult
        // ReSharper restore ConvertToAutoProperty
        {
            get
            {
                return _commandResult;
            }
            set
            {
                _commandResult = value;
            }
        }

        #region Overrides of DsfNativeActivity<string>

        public DsfExecuteCommandLineActivity()
            : base("Execute Command Line")
        {
        }

        /// <summary>
        /// When overridden runs the activity's execution logic
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            _nativeActivityContext = context;
            var dataObject = _nativeActivityContext.GetExtension<IDSFDataObject>();
            var compiler = DataListFactory.CreateDataListCompiler();

            var dlId = dataObject.DataListID;
            var allErrors = new ErrorResultTO();
            ErrorResultTO errors;


            var exeToken = _nativeActivityContext.GetExtension<IExecutionToken>();
            InitializeDebug(dataObject);
            try
            {
                IBinaryDataListEntry expressionsEntry = compiler.Evaluate(dlId, enActionType.User, CommandFileName, false, out errors);
                if(dataObject.IsDebugMode())
                {
                    AddDebugInputItem(new DebugItemVariableParams(CommandFileName, "Command", expressionsEntry, dlId));
                }
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);
                IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                toUpsert.IsDebug = dataObject.IsDebugMode();
                if(!allErrors.HasErrors())
                {
                    while(itr.HasMoreRecords())
                    {
                        IList<IBinaryDataListItem> cols = itr.FetchNextRowData();
                        foreach(IBinaryDataListItem c in cols)
                        {
                            if(c.IsDeferredRead)
                            {
                                if(toUpsert != null)
                                {
                                    toUpsert.HasLiveFlushing = true;
                                    toUpsert.LiveFlushingLocation = dlId;
                                }
                            }

                            if(string.IsNullOrEmpty(c.TheValue))
                            {
                                throw new Exception("Empty script to execute");
                            }

                            string val = c.TheValue;
                            StreamReader errorReader;
                            StringBuilder outputReader;
                            if(!ExecuteProcess(val, exeToken, out errorReader, out outputReader)) return;

                            allErrors.AddError(errorReader.ReadToEnd());
                            var bytes = Encoding.Default.GetBytes(outputReader.ToString().Trim());
                            string readValue = Encoding.ASCII.GetString(bytes).Replace("?", " ");

                            //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
                            foreach(var region in DataListCleaningUtils.SplitIntoRegions(CommandResult))
                            {
                                if(toUpsert != null)
                                {
                                    toUpsert.Add(region, readValue);
                                }

                            }

                            errorReader.Close();

                            if(toUpsert != null && toUpsert.HasLiveFlushing)
                            {
                                try
                                {
                                    toUpsert.FlushIterationFrame();
                                    toUpsert = null;
                                }
                                catch(Exception e)
                                {
                                    Dev2Logger.Log.Error("DSFExecuteCommandLine", e);
                                    allErrors.AddError(e.Message);
                                }
                            }
                            else
                            {
                                compiler.Upsert(dlId, toUpsert, out errors);
                                allErrors.MergeErrors(errors);
                            }
                        }
                    }

                    if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                    {
                        if(toUpsert == null)
                        {
                            return;
                        }
                        foreach(var debugOutputTo in toUpsert.DebugOutputs)
                        {
                            AddDebugOutputItem(new DebugItemVariableParams(debugOutputTo));
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error("DSFCommandLine", e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors    
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfExecuteCommandLineActivity", allErrors);
                    compiler.UpsertSystemTag(dlId, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                    compiler.Upsert(dlId, CommandResult, (string)null, out errors);
                }
                if(dataObject.IsDebugMode())
                {
                    if(hasErrors)
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", CommandResult, ""));
                    }
                    DispatchDebugState(_nativeActivityContext, StateType.Before);
                    DispatchDebugState(_nativeActivityContext, StateType.After);
                }

                if(!string.IsNullOrEmpty(_fullPath))
                {
                    File.Delete(_fullPath);
                }
            }
        }

        bool ExecuteProcess(string val, IExecutionToken executionToken, out StreamReader errorReader, out StringBuilder outputReader)
        {
            outputReader = new StringBuilder();
            _process = new Process();
            using (_process)
            {
                var processStartInfo = CreateProcessStartInfo(val);

                if (processStartInfo == null)
                {
                    // ReSharper disable NotResolvedInText
                    throw new ArgumentNullException("processStartInfo");
                    // ReSharper restore NotResolvedInText
                }

                _process.StartInfo = processStartInfo;
                var processStarted = _process.Start();

                _process.BeginOutputReadLine();

                StringBuilder reader = outputReader;
                DataReceivedEventHandler a = (sender, args) => reader.AppendLine(args.Data);
                _process.OutputDataReceived += a;
                errorReader = _process.StandardError;

                if (!ProcessHasStarted(processStarted, _process))
                {
                    return false;
                }
                if (!_process.HasExited)
                {
                    _process.PriorityClass = CommandPriority;
                }
                _process.StandardInput.Close();

                // bubble user termination down the chain ;)
                while (!_process.HasExited && !executionToken.IsUserCanceled)
                {
                    if (!_process.HasExited)
                    {
                        var isWaitingForUserInput = ModalChecker.IsWaitingForUserInput(_process);

                        if (!isWaitingForUserInput)
                        {
                            continue;
                        }
                        _process.OutputDataReceived -= a;
                        _process.Kill();
                        throw new ApplicationException("The process required user input.");
                    }
                    Thread.Sleep(10);
                }

                // user termination exit ;)
                if (executionToken.IsUserCanceled)
                {
                    // darn .Kill() does not kill the process tree ;(
                    // Nor does .CloseMainWindow() as people have claimed, hence the hand rolled process tree killer - WTF M$ ;(
                    KillProcessAndChildren(_process.Id);
                }
                _process.OutputDataReceived -= a;
                _process.Close();
            }
            return true;
        }
        #region Overrides of NativeActivity<string>

        #endregion

        private void KillProcessAndChildren(int pid)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach(var mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch(ArgumentException)
            {
                // Process already exited.
            }
        }

        bool ProcessHasStarted(bool processStarted, Process process)
        {
            if(processStarted)
            {
                return true;
            }
            CommandResult = "Command did not start. " + Environment.NewLine +
                            "Exit Code: " + process.ExitCode;
            return false;
        }

        ProcessStartInfo CreateProcessStartInfo(string val)
        {
            if(val.StartsWith("cmd")) throw new ArgumentException("Cannot execute CMD from tool.");
            if(val.StartsWith("explorer")) throw new ArgumentException("Cannot execute explorer from tool.");
            if(val.Contains("explorer"))
            {
                var directoryName = Path.GetFullPath(val);
                {
                    var lowerDirectoryName = directoryName.ToLower(CultureInfo.InvariantCulture);
                    if(lowerDirectoryName.EndsWith("explorer.exe"))
                    {
                        throw new ArgumentException("Cannot execute explorer from tool.");
                    }
                }
            }

            ProcessStartInfo psi;

            if(val.StartsWith("\""))
            {
                // we have a quoted string for the cmd portion
                var idx = val.IndexOf("\" \"", StringComparison.Ordinal);
                if(idx < 0)
                {
                    // account for "xxx" arg
                    idx = val.IndexOf("\" ", StringComparison.Ordinal);
                    if(idx < 0)
                    {
                        val += Environment.NewLine + "exit";
                        psi = new ProcessStartInfo("cmd.exe", "/Q /C " + val);
                    }
                    else
                    {
                        var cmd = val.Substring(0, (idx + 1));// keep trailing "
                        if(File.Exists(cmd))
                        {
                            var args = val.Substring((idx + 2));
                            psi = new ProcessStartInfo("cmd.exe", "/Q /C " + cmd + " " + args);
                        }
                        else
                        {
                            psi = ExecuteSystemCommand(val);
                        }
                    }
                }
                else
                {
                    var cmd = val.Substring(0, (idx + 1));// keep trailing "
                    if(File.Exists(cmd))
                    {
                        var args = val.Substring((idx + 2));
                        psi = new ProcessStartInfo("cmd.exe", "/Q /C " + cmd + " " + args);
                    }
                    else
                    {
                        psi = ExecuteSystemCommand(val);
                    }
                }
            }
            else
            {
                // no quotes ;)
                var idx = val.IndexOf(" ", StringComparison.Ordinal);
                if(idx < 0)
                {
                    psi = new ProcessStartInfo("cmd.exe", "/Q /C " + val);
                }
                else
                {
                    var cmd = val.Substring(0, (idx + 1));// keep trailing "
                    if(File.Exists(cmd))
                    {
                        var args = val.Substring((idx + 2));
                        psi = new ProcessStartInfo("cmd.exe", "/Q /C " + cmd + " " + args);
                    }
                    else
                    {
                        psi = ExecuteSystemCommand(val);
                    }
                }
            }

            if(psi != null)
            {
                psi.UseShellExecute = false;
                psi.ErrorDialog = false;
                psi.RedirectStandardError = true;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.CreateNoWindow = true;
            }

            return psi;
        }

        static ProcessStartInfo ExecuteSystemCommand(string val)
        {
            _fullPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".bat");
            File.Create(_fullPath).Close();
            File.WriteAllText(_fullPath, val);
            var psi = new ProcessStartInfo("cmd.exe", "/Q /C " + _fullPath);
            return psi;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates == null)
            {
                return;
            }
            foreach(var t in updates)
            {

                if(t.Item1 == CommandFileName)
                {
                    CommandFileName = t.Item2;
                }
                else if(t.Item1 == CommandPriority.ToString())
                {
                    CommandPriority = (ProcessPriorityClass)Enum.Parse(typeof(ProcessPriorityClass), t.Item2, true);
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                var itemUpdate = updates.FirstOrDefault(tuple => tuple.Item1 == CommandResult);
                if(itemUpdate != null)
                {
                    CommandResult = itemUpdate.Item2;
                }
            }
        }

        #region Overrides of DsfNativeActivity<string>

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(CommandFileName, CommandPriority.ToString());
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(CommandResult);
        }

        #endregion

        #endregion
    }
}
