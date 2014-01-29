using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text;
using System.Threading;
using Dev2.Common;
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
        Process _process;
        NativeActivityContext _nativeActivityContext;
        ProcessPriorityClass _commandPriority = ProcessPriorityClass.Normal;

        #endregion

        /// <summary>
        /// Gets or sets the name of the recordset.
        /// </summary>  
        [Inputs("CommandFileName")]
        [FindMissing]
        public string CommandFileName
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
        public string CommandResult
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
            _nativeActivityContext = context;
            var dataObject = _nativeActivityContext.GetExtension<IDSFDataObject>();
            var compiler = DataListFactory.CreateDataListCompiler();

            var dlID = dataObject.DataListID;
            var allErrors = new ErrorResultTO();
            ErrorResultTO errors;


            var exeToken = _nativeActivityContext.GetExtension<IExecutionToken>();

            try
            {
                IBinaryDataListEntry expressionsEntry = compiler.Evaluate(dlID, enActionType.User, CommandFileName, false, out errors);
                if(dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {
                    AddDebugInputItem(CommandFileName, "Command to execute", expressionsEntry, dlID);
                }
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);
                IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                while(itr.HasMoreRecords())
                {
                    IList<IBinaryDataListItem> cols = itr.FetchNextRowData();
                    foreach(IBinaryDataListItem c in cols)
                    {
                        if(c.IsDeferredRead)
                        {
                            toUpsert.HasLiveFlushing = true;
                            toUpsert.LiveFlushingLocation = dlID;
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
                            toUpsert.Add(region, readValue);
                            if(dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID))
                            {
                                AddDebugOutputItem(region, readValue, dlID);
                            }
                        }

                        errorReader.Close();

                        if(toUpsert.HasLiveFlushing)
                        {
                            try
                            {
                                toUpsert.FlushIterationFrame(true);
                                toUpsert = null;
                            }
                            catch(Exception e)
                            {
                                allErrors.AddError(e.Message);
                            }
                        }
                        else
                        {
                            compiler.Upsert(dlID, toUpsert, out errors);
                            allErrors.MergeErrors(errors);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors

                if(allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfExecuteCommandLineActivity", allErrors);
                    compiler.UpsertSystemTag(dlID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if(dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {
                    DispatchDebugState(_nativeActivityContext, StateType.Before);
                    DispatchDebugState(_nativeActivityContext, StateType.After);
                }
            }
        }

        bool ExecuteProcess(string val, IExecutionToken executionToken, out StreamReader errorReader, out StringBuilder outputReader)
        {
            outputReader = new StringBuilder();
            _process = new Process();
            using(_process)
            {
                var processStartInfo = CreateProcessStartInfo(val);

                if(processStartInfo == null)
                {
                    throw new ArgumentNullException("processStartInfo");
                }

                _process.StartInfo = processStartInfo;
                var processStarted = _process.Start();

                _process.BeginOutputReadLine();

                StringBuilder reader = outputReader;

                _process.OutputDataReceived += (sender, args) => reader.AppendLine(args.Data);

                errorReader = _process.StandardError;

                if(!ProcessHasStarted(processStarted, _process))
                {
                    return false;
                }
                if(!_process.HasExited)
                {
                    _process.PriorityClass = CommandPriority;
                }
                _process.StandardInput.Close();

                // bubble user termination down the chain ;)
                while(!_process.HasExited && !executionToken.IsUserCanceled)
                {
                    if(!_process.HasExited)
                    {
                        var isWaitingForUserInput = ModalChecker.IsWaitingForUserInput(_process);

                        if(!isWaitingForUserInput)
                        {
                            continue;
                        }

                        _process.Kill();
                        throw new ApplicationException("The process required user input.");
                    }
                    Thread.Sleep(10);
                }

                // user termination exit ;)
                if(executionToken.IsUserCanceled)
                {
                    // darn .Kill() does not kill the process tree ;(
                    // Nor does .CloseMainWindow() as people have claimed, hence the hand rolled process tree killer - WTF M$ ;(
                    KillProcessAndChildren(_process.Id);
                }

                _process.Close();
            }
            return true;
        }

        #region Overrides of NativeActivity<string>

        void StopProcess()
        {
            _process.Close();
            _process.Dispose();
        }

        #endregion

        private void KillProcessAndChildren(int pid)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach(ManagementObject mo in moc)
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
            if(val.Contains("cmd")) throw new ArgumentException("Cannot execute CMD from tool.");
            if(val.Contains("explorer")) throw new ArgumentException("Cannot execute explorer from tool.");

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
            var fullPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".bat");
            File.Create(fullPath).Close();
            File.WriteAllText(fullPath, val);
            var psi = new ProcessStartInfo("cmd.exe", "/Q /C " + fullPath);
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
            if(updates != null && updates.Count == 1)
            {
                CommandResult = updates[0].Item2;
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

        #region Private Methods

        private void AddDebugInputItem(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            var itemToAdd = new DebugItem();

            if(!string.IsNullOrWhiteSpace(labelText))
            {
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = labelText });
            }

            if(valueEntry != null)
            {
                itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input));
            }

            _debugInputs.Add(itemToAdd);
        }

        private void AddDebugOutputItem(string expression, string value, Guid dlId)
        {
            var itemToAdd = new DebugItem();
            itemToAdd.AddRange(CreateDebugItemsFromString(expression, value, dlId, 0, enDev2ArgumentType.Output));
            _debugOutputs.Add(itemToAdd);
        }

        #endregion

        #endregion
    }
}