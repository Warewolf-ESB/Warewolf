using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace Dev2.Providers.Logs
{
    /// <summary>
    /// This is the trace writer used by the studio. Note other than testing there are no usages
    /// for this class as it is initialized from the app.config
    /// </summary>
    public class CustomTextWriter : TraceListener
    {
        const int DefaultMaxFileSize = 1048576;
        static string FileName;
        StreamWriter _traceWriter;
        readonly AppSettingsReader _appSettingsReader;
        bool _streamClosed;

        public CustomTextWriter(string fileName)
        {
            FileName = fileName;
            _appSettingsReader = new AppSettingsReader();
        }

        public static string LoggingFileName
        {
            get
            {
                if(String.IsNullOrEmpty(FileName))
                {
                    FileName = "Warewolf Studio.log";
                }
                return Path.Combine(StudioLogPath, FileName);
            }
        }

        public static string WarewolfAppPath
        {
            get
            {
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var warewolfAppPath = Path.Combine(appDataFolder, "Warewolf");
                if(!Directory.Exists(warewolfAppPath))
                {
                    Directory.CreateDirectory(warewolfAppPath);
                }
                return warewolfAppPath;
            }
        }

        public static string StudioLogPath
        {
            get
            {
                var studioLogPath = Path.Combine(WarewolfAppPath, "Studio Logs");
                if(!Directory.Exists(studioLogPath))
                {
                    Directory.CreateDirectory(studioLogPath);
                }
                return studioLogPath;
            }
        }

        public override void Write(string value)
        {
            try
            {
                EnsureStream();
                CheckRollover();
                _traceWriter.Write(value);
                _traceWriter.Flush();
            }
            catch(ObjectDisposedException)
            {
                //ignore this exception
            }
        }

        public override void WriteLine(string value)
        {
            try
            {
                EnsureStream();
                CheckRollover();
                _traceWriter.WriteLine(value);
                _traceWriter.Flush();
            }
            catch(ObjectDisposedException)
            {
                //ignore this exception
            }
        }

        void EnsureStream()
        {
            if(_traceWriter == null)
            {
                _traceWriter = new StreamWriter(LoggingFileName, true);
            }
        }

        void CheckRollover()
        {
            var maxFileSize = GetMaxFileSize();

            if(maxFileSize > 0)
            {
                try
                {
                    if(_traceWriter.BaseStream.Length > maxFileSize && !_streamClosed)
                    {
                        var fileName = FileName;
                        CloseTraceWriter();
                        FileName = fileName;
                        _traceWriter = new StreamWriter(LoggingFileName, false);
                        _streamClosed = false;
                    }
                }
                catch(ObjectDisposedException)
                {
                    //ignore this exception
                }
            }
        }

        int GetMaxFileSize()
        {
            try
            {
                int maxFileSize = int.Parse(_appSettingsReader.GetValue("MaxLogFileSizeBytes", typeof(int)).ToString());
                return maxFileSize;
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch(Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {
                //Could not read setttings. Use default.
            }
            return DefaultMaxFileSize;
        }

        public void CloseTraceWriter()
        {
            if(_traceWriter != null)
            {
                _traceWriter.Close();
                _traceWriter.Dispose();
            }
            _streamClosed = true;
            FileName = null;
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                CloseTraceWriter();
            }
        }
    }
}