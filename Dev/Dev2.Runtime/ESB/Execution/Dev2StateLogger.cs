using System;
using System.IO;
using Newtonsoft.Json;
using Dev2.Interfaces;
using Dev2.Common;
using Dev2.Common.Wrappers;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Interfaces;
using System.IO.Compression;
using Dev2.Communication;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Security.Principal;
using System.Collections.Generic;

namespace Dev2.Runtime.ESB.Execution
{
    class Dev2JsonStateLogger : IDev2StateLogger
    {
        readonly StreamWriter writer;
        readonly JsonTextWriter jsonTextWriter;
        readonly IDSFDataObject _dsfDataObject;
        readonly IFile _fileWrapper;
        readonly IZipFile _zipWrapper;
        readonly DetailedLogFile _detailedLogFile;

        public Dev2JsonStateLogger(IDSFDataObject dsfDataObject)
            : this(dsfDataObject, new FileWrapper(), new ZipFileWrapper())
        {
        }

        public Dev2JsonStateLogger(IDSFDataObject dsfDataObject, IFile fileWrapper, IZipFile zipWrapper)
        {
            _dsfDataObject = dsfDataObject;
            _fileWrapper = fileWrapper;
            _zipWrapper = zipWrapper;
            _detailedLogFile = new DetailedLogFile(_dsfDataObject, _fileWrapper);
            writer = GetDetailedLogWriter();
            jsonTextWriter = new JsonTextWriter(writer);
        }

        private StreamWriter GetDetailedLogWriter()
        {
            if (_detailedLogFile.IsOlderThanToday)
            {
                var compress = _dsfDataObject.Settings.ShouldCompressFile(_detailedLogFile);
                MoveLogFileIfOld();
                RunBackgroundLogTasks(compress);
            }
            return _fileWrapper.AppendText(_detailedLogFile.LogFilePath);
        }


        public void LogPreExecuteState(IDev2Activity nextActivity)
        {
            Notify(nameof(LogPreExecuteState), nextActivity);
            writer.WriteLine("header:LogPreExecuteState");
            WriteHeader(null, nextActivity);
            _dsfDataObject.LogState(jsonTextWriter);
            jsonTextWriter.Flush();
            writer.WriteLine();
            writer.Flush();
        }

        public void LogAdditionalDetail(object detail, string callerName)
        {
            Notify(nameof(LogAdditionalDetail), new
            {
                CallerName = callerName,
                Detail = detail,
            });
            writer.WriteLine($"header:LogAdditionalDetail:{callerName}");
            jsonTextWriter.WriteStartObject();
            jsonTextWriter.WritePropertyName("Detail");
            var serializer = new Dev2JsonSerializer();
            jsonTextWriter.WriteRawValue(serializer.Serialize(detail, Formatting.None));
            jsonTextWriter.WriteEndObject();
            jsonTextWriter.Flush();
            writer.WriteLine();
            writer.Flush();
        }

        public void LogPostExecuteState(IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            Notify(nameof(LogPostExecuteState), nextActivity);
            writer.WriteLine("header:LogPostExecuteState");
            WriteHeader(previousActivity, nextActivity);
            _dsfDataObject.LogState(jsonTextWriter);
            jsonTextWriter.Flush();
            writer.WriteLine();
            writer.Flush();
        }

        public void LogExecuteException(Exception e, IDev2Activity activity)
        {
            Notify(nameof(LogExecuteException), new
            {
                Activity = activity,
                Exception = e,
            });
            writer.WriteLine("header:LogExecuteException");
            WriteHeader(activity, e);
            _dsfDataObject.LogState(jsonTextWriter);
            jsonTextWriter.Flush();
            writer.WriteLine();
            writer.Flush();
        }

        public void LogExecuteCompleteState()
        {
            Notify(nameof(LogExecuteCompleteState), null);
            writer.WriteLine("header:LogExecuteCompleteState");
            jsonTextWriter.WriteStartObject();
            jsonTextWriter.WritePropertyName("timestamp");
            jsonTextWriter.WriteValue(DateTime.Now);
            jsonTextWriter.WriteEndObject();
            writer.WriteLine();
            writer.Flush();
            _dsfDataObject.LogState(jsonTextWriter);
            jsonTextWriter.Flush();
            writer.WriteLine();
            writer.Flush();
        }

        public void LogStopExecutionState()
        {
            Notify(nameof(LogStopExecutionState), null);
            writer.WriteLine("header:LogStopExecutionState");
            jsonTextWriter.WriteStartObject();
            jsonTextWriter.WritePropertyName("timestamp");
            jsonTextWriter.WriteValue(DateTime.Now);
            jsonTextWriter.WriteEndObject();
            writer.WriteLine();
            writer.Flush();
            _dsfDataObject.LogState(jsonTextWriter);
            jsonTextWriter.Flush();
            writer.WriteLine();
            writer.Flush();
        }

        private void WriteHeader(IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            jsonTextWriter.WriteStartObject();
            jsonTextWriter.WritePropertyName("timestamp");
            jsonTextWriter.WriteValue(DateTime.Now);
            if (!(previousActivity is null))
            {
                jsonTextWriter.WritePropertyName("PreviousActivity");
                jsonTextWriter.WriteValue(previousActivity.UniqueID);
            }
            if (!(nextActivity is null))
            {
                jsonTextWriter.WritePropertyName("NextActivity");

                jsonTextWriter.WriteStartObject();
                jsonTextWriter.WritePropertyName("Id");
                jsonTextWriter.WriteValue(nextActivity.UniqueID);
                jsonTextWriter.WritePropertyName("Type");
                jsonTextWriter.WriteValue(nextActivity.GetType().ToString());
                jsonTextWriter.WritePropertyName("DisplayName");
                jsonTextWriter.WriteValue(nextActivity.GetDisplayName());
                if (nextActivity is DsfActivity dsfActivity)
                {
                    jsonTextWriter.WritePropertyName("Inputs");
                    var jsonSerializer = new Dev2JsonSerializer();
                    jsonTextWriter.WriteRawValue(jsonSerializer.Serialize(dsfActivity.Inputs, Formatting.None));
                    jsonTextWriter.WritePropertyName("Outputs");
                    jsonTextWriter.WriteRawValue(jsonSerializer.Serialize(dsfActivity.Outputs, Formatting.None));
                }
                jsonTextWriter.WriteEndObject();
            }
            jsonTextWriter.WriteEndObject();
            jsonTextWriter.Flush();
            writer.WriteLine();
            writer.Flush();
        }

        private void WriteHeader(IDev2Activity activity, Exception exception)
        {
            jsonTextWriter.WriteStartObject();
            jsonTextWriter.WritePropertyName("timestamp");
            jsonTextWriter.WriteValue(DateTime.Now);
            jsonTextWriter.WritePropertyName("PreviousActivity");
            jsonTextWriter.WriteValue(activity.UniqueID);
            jsonTextWriter.WritePropertyName("Exception");
            jsonTextWriter.WriteValue(exception.Message);
            jsonTextWriter.WriteEndObject();
            jsonTextWriter.Flush();
            writer.WriteLine();
            writer.Flush();
        }

        private void MoveLogFileIfOld()
        {
            var newFilePath = _detailedLogFile.GetNewFileName();
            _fileWrapper.Copy(_detailedLogFile.LogFilePath, Path.Combine(_detailedLogFile.LogFilePath, newFilePath));
            _fileWrapper.Delete(_detailedLogFile.LogFilePath);
        }

        private void RunBackgroundLogTasks(bool compress)
        {
            if (compress)
            {
                _fileWrapper.Delete(_detailedLogFile.LogFilePath);
                FileCompressor.Compress(_detailedLogFile, _zipWrapper);
            }
        }

        public void Close()
        {
            jsonTextWriter.Close();
        }

        public void Dispose()
        {
            ((IDisposable)jsonTextWriter).Dispose();
        }

        readonly IList<IStateLoggerListener> stateListeners = new List<IStateLoggerListener>();
        public void Subscribe(IStateLoggerListener listener)
        {
            stateListeners.Add(listener);
        }
        private void Notify(string callerName, object payload)
        {
            foreach (var stateListener in stateListeners)
            {
                var result = stateListener.Notify(callerName, payload);
                if (!result)
                {
                    throw new StateLoggerStoppedException();
                }
            }
        }
        class StateLoggerStoppedException : Exception {}
    }

    class DetailedLogFile
    {
        readonly IFile _fileWrapper;
        readonly IDirectory _directoryWrapper;
        readonly IDSFDataObject _dsfDataObject;
        public DetailedLogFile(IDSFDataObject dsfDataObject, IFile fileWrapper)
        {
            _dsfDataObject = dsfDataObject;
            LogFilePath = GetDetailLogFilePath(_dsfDataObject);
            _fileWrapper = fileWrapper;
            _directoryWrapper = new DirectoryWrapper();
        }

        public string LogFilePath { get; set; }
        public string LogFileDirectory => Path.GetDirectoryName(LogFilePath);
        public DateTime LogFileLastModifiedDate => _fileWrapper.GetLastWriteTime(LogFilePath);
        public int LogFileAge => (DateTime.Today - LogFileLastModifiedDate).Days;
        public bool IsOlderThanToday => _fileWrapper.Exists(LogFilePath) && LogFileLastModifiedDate.Date < DateTime.Today.Date;
        public string ArchiveFolder =>
            EnvironmentVariables.WorkflowDetailLogArchivePath(_dsfDataObject.ResourceID, _dsfDataObject.ServiceName);
        public bool ArchiveFolderExist => _directoryWrapper.Exists(ArchiveFolder);

        public string LogFileParentFolder => _directoryWrapper.GetParent(LogFilePath).Name;

        internal string GetNewFileName()
        {
            const string dateFormat = "yyyyMMdd";
            var fileInfo = new FileInfo(LogFilePath);
            var newName = fileInfo.Name.Replace(".log", " ") + DateTime.Today.AddDays(-1).ToString(dateFormat) + ".log";
            return Path.Combine(fileInfo.DirectoryName, newName);
        }

        internal static string GetDetailLogFilePath(IDSFDataObject dsfDataObject) =>
            Path.Combine(EnvironmentVariables.WorkflowDetailLogPath(dsfDataObject.ResourceID, dsfDataObject.ServiceName)
                         , "Detail.log");
    }
    static class FileCompressor
    {
        public static void Compress(DetailedLogFile logFile, IZipFile zipWrapper)
        {
            if (logFile.ArchiveFolderExist)
            {
                AddEntry(logFile);
            }
            else
            {
                zipWrapper.CreateFromDirectory(logFile.LogFileDirectory, logFile.ArchiveFolder);
            }
        }
        public static void AddEntry(DetailedLogFile logFile)
        {
            using (FileStream zipToOpen = new FileStream(logFile.ArchiveFolder, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    archive.CreateEntry(logFile.LogFilePath);
                }
            }
        }
    }
    static class DsfDataObjectMethods
    {
        public static void LogState(this IDSFDataObject dsfDataObject, JsonTextWriter jsonTextWriter)
        {
            jsonTextWriter.WriteRaw("{\"DsfDataObject\":");
            jsonTextWriter.WriteStartObject();

            jsonTextWriter.WritePropertyName("ServerID");
            jsonTextWriter.WriteValue(dsfDataObject.ServerID);

            jsonTextWriter.WritePropertyName("ParentID");
            jsonTextWriter.WriteValue(dsfDataObject.ParentID);

            jsonTextWriter.WritePropertyName("ClientID");
            jsonTextWriter.WriteValue(dsfDataObject.ClientID);

            jsonTextWriter.WritePropertyName("ExecutingUser");
            jsonTextWriter.WriteValue(dsfDataObject.ExecutingUser.Identity.ToJson());

            jsonTextWriter.WritePropertyName("ExecutionID");
            jsonTextWriter.WriteValue(dsfDataObject.ExecutionID);

            jsonTextWriter.WritePropertyName("ExecutionOrigin");
            jsonTextWriter.WriteValue(dsfDataObject.ExecutionOrigin);

            jsonTextWriter.WritePropertyName("ExecutionOriginDescription");
            jsonTextWriter.WriteValue(dsfDataObject.ExecutionOriginDescription);

            jsonTextWriter.WritePropertyName("ExecutionToken");
            jsonTextWriter.WriteValue(dsfDataObject.ExecutionToken.ToJson());

            jsonTextWriter.WritePropertyName("IsSubExecution");
            jsonTextWriter.WriteValue(dsfDataObject.IsSubExecution);

            jsonTextWriter.WritePropertyName("IsRemoteWorkflow");
            jsonTextWriter.WriteValue(dsfDataObject.IsRemoteWorkflow());

            jsonTextWriter.WritePropertyName("Environment");
            jsonTextWriter.WriteRawValue(dsfDataObject.Environment.ToJson());

            jsonTextWriter.WriteEndObject();
            jsonTextWriter.WriteRaw("}");
        }
    }

    static class IIdentityExtensionMethods
    {
        public static string ToJson(this IIdentity identity)
        {
            var json = new Dev2JsonSerializer();
            return json.Serialize(identity, Formatting.None);
        }
    }

    static class ExecutionTokenExtensionMethods
    {
        public static string ToJson(this IExecutionToken executionToken)
        {
            var json = new Dev2JsonSerializer();
            return json.Serialize(executionToken, Formatting.None);
        }
    }

    static class Dev2WorkflowSettingsExtensionMethods
    {
        public static bool ShouldCompressFile(this IDev2WorkflowSettings settings, DetailedLogFile detailedLogFile)
            => detailedLogFile.LogFileAge > 2;
    }
}