#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;


namespace Dev2.Activities
{
    [ToolDescriptorInfo("Scripting-CMDScript", "CMD Script", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Scripting", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Scripting_CMD_Script")]
    public class DsfExecuteCommandLineActivity : DsfActivityAbstract<string>,IEquatable<DsfExecuteCommandLineActivity>, IDisposable
    {
        #region Fields

        string _commandFileName;
        string _commandResult;
        static string _fullPath;
        Process _process;
        NativeActivityContext _nativeActivityContext;
        ProcessPriorityClass _commandPriority = ProcessPriorityClass.Normal;

        #endregion
        
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
        public ProcessPriorityClass CommandPriority
        {
            get => _commandPriority;
            set => _commandPriority = value;
        }

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

        public override List<string> GetOutputs() => new List<string> { CommandResult };
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
            var exeToken = _nativeActivityContext.GetExtension<IExecutionToken>();
            if(exeToken != null)
            {
                dataObject.ExecutionToken = exeToken;
            }

            ExecuteTool(dataObject, 0);
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[] {
                new StateVariable
                {
                    Name = "Command",
                    Value = CommandFileName,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "CommandPriority",
                    Value = CommandPriority.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name="CommandResult",
                    Value = CommandResult,
                    Type = StateVariable.StateType.Output
                }
            };
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            Dev2Logger.Debug($"Executing commandline tool with CommandFileName:\n{CommandFileName}", GlobalConstants.WarewolfDebug);
            var exeToken = dataObject.ExecutionToken;
            var allErrors = new ErrorResultTO();

            InitializeDebug(dataObject);
            try
            {
                if(dataObject.IsDebugMode())
                {
                    AddDebugInputItem(new DebugEvalResult(CommandFileName, "Command", dataObject.Environment, update));
                }
                var itr = new WarewolfIterator(dataObject.Environment.Eval(CommandFileName, update));
                if (allErrors.HasErrors())
                {
                    return;
                }
                var counter = 1;
                while (itr.HasMoreData())
                {
                    var val = itr.GetNextValue();
                    {
                        if(string.IsNullOrEmpty(val))
                        {
                            throw new Exception(ErrorResource.EmptyScript);
                        }
                        if (!ExecuteProcess(val, exeToken, out StreamReader errorReader, out StringBuilder outputReader))
                        {
                            return;
                        }

                        allErrors.AddError(errorReader.ReadToEnd());
                        var bytes = Encoding.Default.GetBytes(outputReader.ToString().Trim());
                        var readValue = Encoding.ASCII.GetString(bytes).Replace("?", " ");

                        foreach (var region in DataListCleaningUtils.SplitIntoRegions(CommandResult))
                        {
                            dataObject.Environment?.Assign(region, readValue, update == 0 ? counter : update);
                        }
                        counter++;
                        errorReader.Close();
                    }
                }

                if (dataObject.IsDebugMode() && !allErrors.HasErrors() && !string.IsNullOrEmpty(CommandResult))
                {
                    AddDebugOutputItem(new DebugEvalResult(CommandResult, "", dataObject.Environment, update));
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Error("DSFCommandLine", e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }
            finally
            {

                if (!string.IsNullOrEmpty(_fullPath))
                {
                    File.Delete(_fullPath);
                    var tmpFile = _fullPath.Replace(".bat", "");
                    if (File.Exists(tmpFile))
                    {
                        File.Delete(tmpFile);
                    }
                }
                // Handle Errors    
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfExecuteCommandLineActivity", allErrors);
                    if(dataObject.Environment != null)
                    {
                        var errorString = allErrors.MakeDisplayReady();
                        dataObject.Environment.AddError(errorString);
                        dataObject.Environment.Assign(CommandResult, null, update);
                    }
                }
                if(dataObject.IsDebugMode())
                {
                    if(hasErrors)
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", CommandResult, ""));
                    }
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        bool ExecuteProcess(string val, IExecutionToken executionToken, out StreamReader errorReader, out StringBuilder outputReader)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            outputReader = new StringBuilder();
            _process = new Process();
            using (_process)
            {
                Dev2Logger.Debug($"Creating commandline process for value:\n{val}", GlobalConstants.WarewolfDebug);
                var processStartInfo = CreateProcessStartInfo(val);
                _process.StartInfo = processStartInfo ?? throw new ArgumentNullException("processStartInfo");
                var processStarted = _process.Start();

                var reader = outputReader;
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
                
                while (!_process.HasExited && !executionToken.IsUserCanceled)
                {
                    reader.Append(_process.StandardOutput.ReadToEnd());
                    if (!_process.HasExited && _process.Threads.Cast<ProcessThread>().Any(a => a.ThreadState == System.Diagnostics.ThreadState.Wait && a.WaitReason == ThreadWaitReason.UserRequest) && !_process.HasExited)
                    {
                        _process.Kill();
                    }

                    if (ModalChecker.IsWaitingForUserInput(_process))
                    {
                        _process.Kill();
                        throw new ApplicationException(ErrorResource.UserInputRequired);
                    }
                    Thread.Sleep(10);
                    continue;
                }
                
                if (executionToken.IsUserCanceled)
                {
                    KillProcessAndChildren(_process.Id);
                }
                reader.Append(_process.StandardOutput.ReadToEnd());
                _process.Close();
            }
            return true;
        }

        void KillProcessAndChildren(int pid)
        {
            var searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            var moc = searcher.Get();
            foreach (var mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                var proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException e)
            {
                Dev2Logger.Warn(e.Message, "Warewolf Warn");
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
            if(val.StartsWith("cmd"))
            {
                throw new ArgumentException(ErrorResource.CannotExecuteCMDFromTool);
            }

            if (val.StartsWith("explorer"))
            {
                throw new ArgumentException(ErrorResource.CannotExecuteExplorerFromTool);
            }

            if (val.Contains("explorer"))
            {
                var directoryName = Path.GetFullPath(val);
                {
                    var lowerDirectoryName = directoryName.ToLower(CultureInfo.InvariantCulture);
                    if(lowerDirectoryName.EndsWith("explorer.exe"))
                    {
                        throw new ArgumentException(ErrorResource.CannotExecuteExplorerFromTool);
                    }
                }
            }

            ProcessStartInfo psi;
            if(val.StartsWith("\""))
            {
                psi = ExecuteQuotedExpression(ref val);
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
                    var cmd = val.Substring(0, idx + 1);// keep trailing "
                    if(File.Exists(cmd))
                    {
                        var args = val.Substring(idx + 2);
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
                psi.Verb = "runas";
            }

            return psi;
        }

        private static ProcessStartInfo ExecuteQuotedExpression(ref string val)
        {
            ProcessStartInfo psi;
            // we have a quoted string for the cmd portion
            var idx = val.IndexOf("\" \"", StringComparison.Ordinal);
            if (idx < 0)
            {
                // account for "xxx" arg
                idx = val.IndexOf("\" ", StringComparison.Ordinal);
                if (idx < 0)
                {
                    val += Environment.NewLine + "exit";
                    psi = new ProcessStartInfo("cmd.exe", "/Q /C " + val);
                }
                else
                {
                    var cmd = val.Substring(0, idx + 1);// keep trailing "
                    if (File.Exists(cmd))
                    {
                        var args = val.Substring(idx + 2);
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
                var cmd = val.Substring(0, idx + 1);// keep trailing "
                if (File.Exists(cmd))
                {
                    var args = val.Substring(idx + 2);
                    psi = new ProcessStartInfo("cmd.exe", "/Q /C " + cmd + " " + args);
                }
                else
                {
                    psi = ExecuteSystemCommand(val);
                }
            }

            return psi;
        }

        static ProcessStartInfo ExecuteSystemCommand(string val)
        {
            _fullPath = Path.Combine(GlobalConstants.TempLocation, Path.GetTempFileName() + ".bat");
            File.Create(_fullPath).Close();
            File.WriteAllText(_fullPath, val);
            if (File.Exists(_fullPath))
            {
                var psi = new ProcessStartInfo("cmd.exe", "/Q /C " + _fullPath);
                return psi;
            }
            val = val.Replace(Environment.NewLine, " & ");
            var commandPsi = new ProcessStartInfo("cmd.exe", "/Q /C " + val);
            return commandPsi;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if(updates == null)
            {
                return;
            }
            foreach(var t in updates)
            {
                if (t.Item1 == CommandFileName)
                {
                    CommandFileName = t.Item2;
                    Dev2Logger.Debug($"Got CommandFileName for commandline tool:\n{CommandFileName}", GlobalConstants.WarewolfDebug);
                }
                else
                {
                    if (t.Item1 == CommandPriority.ToString())
                    {
                        CommandPriority = (ProcessPriorityClass)Enum.Parse(typeof(ProcessPriorityClass), t.Item2, true);
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == CommandResult);
            if(itemUpdate != null)
            {
                CommandResult = itemUpdate.Item2;
            }
        }

        #region Overrides of DsfNativeActivity<string>

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(CommandFileName, CommandPriority.ToString());

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(CommandResult);

        public void Dispose()
        {
            ((IDisposable)_process).Dispose();
        }

        #endregion

        #endregion

        public bool Equals(DsfExecuteCommandLineActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) 
                && string.Equals(CommandFileName, other.CommandFileName) 
                && string.Equals(CommandResult, other.CommandResult)
                && CommandPriority == other.CommandPriority;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((DsfExecuteCommandLineActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (CommandFileName != null ? CommandFileName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CommandResult != null ? CommandResult.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_process != null ? _process.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_nativeActivityContext != null ? _nativeActivityContext.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) _commandPriority;
                return hashCode;
            }
        }
    }
}
