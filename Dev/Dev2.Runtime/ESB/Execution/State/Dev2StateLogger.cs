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

namespace Dev2.Runtime.ESB.Execution
{
    class Dev2JsonStateLogger : IStateListener
    {
        readonly StreamWriter _writer;
        readonly JsonTextWriter _jsonTextWriter;
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
            _writer = GetDetailedLogWriter();
            _jsonTextWriter = new JsonTextWriter(_writer);
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
            _writer.WriteLine("header:LogPreExecuteState");
            WriteHeader(null, nextActivity);
            _dsfDataObject.LogState(_jsonTextWriter);
            _jsonTextWriter.Flush();
            _writer.WriteLine();
            _writer.Flush();
        }

        public void LogAdditionalDetail(object detail, string callerName)
        {
            _writer.WriteLine($"header:LogAdditionalDetail:{callerName}");
            _jsonTextWriter.WriteStartObject();
            _jsonTextWriter.WritePropertyName("Detail");
            var serializer = new Dev2JsonSerializer();
            _jsonTextWriter.WriteRawValue(serializer.Serialize(detail, Formatting.None));
            _jsonTextWriter.WriteEndObject();
            _jsonTextWriter.Flush();
            _writer.WriteLine();
            _writer.Flush();
        }

        public void LogPostExecuteState(IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            _writer.WriteLine("header:LogPostExecuteState");
            WriteHeader(previousActivity, nextActivity);
            _dsfDataObject.LogState(_jsonTextWriter);
            _jsonTextWriter.Flush();
            _writer.WriteLine();
            _writer.Flush();
        }

        public void LogExecuteException(Exception e, IDev2Activity activity)
        {
            _writer.WriteLine("header:LogExecuteException");
            WriteHeader(activity, e);
            _dsfDataObject.LogState(_jsonTextWriter);
            _jsonTextWriter.Flush();
            _writer.WriteLine();
            _writer.Flush();
        }

        public void LogExecuteCompleteState()
        {
            _writer.WriteLine("header:LogExecuteCompleteState");
            _jsonTextWriter.WriteStartObject();
            _jsonTextWriter.WritePropertyName("timestamp");
            _jsonTextWriter.WriteValue(DateTime.Now);
            _jsonTextWriter.WriteEndObject();
            _writer.WriteLine();
            _writer.Flush();
            _dsfDataObject.LogState(_jsonTextWriter);
            _jsonTextWriter.Flush();
            _writer.WriteLine();
            _writer.Flush();
        }

        public void LogStopExecutionState()
        {
            _writer.WriteLine("header:LogStopExecutionState");
            _jsonTextWriter.WriteStartObject();
            _jsonTextWriter.WritePropertyName("timestamp");
            _jsonTextWriter.WriteValue(DateTime.Now);
            _jsonTextWriter.WriteEndObject();
            _writer.WriteLine();
            _writer.Flush();
            _dsfDataObject.LogState(_jsonTextWriter);
            _jsonTextWriter.Flush();
            _writer.WriteLine();
            _writer.Flush();
        }

        private void WriteHeader(IDev2Activity previousActivity, IDev2Activity nextActivity)
        {
            _jsonTextWriter.WriteStartObject();
            _jsonTextWriter.WritePropertyName("timestamp");
            _jsonTextWriter.WriteValue(DateTime.Now);
            if (!(previousActivity is null))
            {
                _jsonTextWriter.WritePropertyName("PreviousActivity");
                _jsonTextWriter.WriteValue(previousActivity.UniqueID);
            }
            if (!(nextActivity is null))
            {
                _jsonTextWriter.WritePropertyName("NextActivity");

                _jsonTextWriter.WriteStartObject();
                _jsonTextWriter.WritePropertyName("Id");
                _jsonTextWriter.WriteValue(nextActivity.UniqueID);
                _jsonTextWriter.WritePropertyName("Type");
                _jsonTextWriter.WriteValue(nextActivity.GetType().ToString());
                _jsonTextWriter.WritePropertyName("DisplayName");
                _jsonTextWriter.WriteValue(nextActivity.GetDisplayName());
                if (nextActivity is DsfActivity dsfActivity)
                {
                    _jsonTextWriter.WritePropertyName("Inputs");
                    var jsonSerializer = new Dev2JsonSerializer();
                    _jsonTextWriter.WriteRawValue(jsonSerializer.Serialize(dsfActivity.Inputs, Formatting.None));
                    _jsonTextWriter.WritePropertyName("Outputs");
                    _jsonTextWriter.WriteRawValue(jsonSerializer.Serialize(dsfActivity.Outputs, Formatting.None));
                }
                _jsonTextWriter.WriteEndObject();
            }
            _jsonTextWriter.WriteEndObject();
            _jsonTextWriter.Flush();
            _writer.WriteLine();
            _writer.Flush();
        }

        private void WriteHeader(IDev2Activity activity, Exception exception)
        {
            _jsonTextWriter.WriteStartObject();
            _jsonTextWriter.WritePropertyName("timestamp");
            _jsonTextWriter.WriteValue(DateTime.Now);
            _jsonTextWriter.WritePropertyName("PreviousActivity");
            _jsonTextWriter.WriteValue(activity.UniqueID);
            _jsonTextWriter.WritePropertyName("Exception");
            _jsonTextWriter.WriteValue(exception.Message);
            _jsonTextWriter.WriteEndObject();
            _jsonTextWriter.Flush();
            _writer.WriteLine();
            _writer.Flush();
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

        public void Dispose()
        {
            _jsonTextWriter.Close();
            ((IDisposable)_jsonTextWriter).Dispose();
        }
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