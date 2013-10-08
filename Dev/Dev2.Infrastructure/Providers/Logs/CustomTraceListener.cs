using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace Dev2.Providers.Logs
{
    public class CustomTextWriter : TraceListener
    {
        readonly string _fileName;
        StreamWriter _traceWriter;
        AppSettingsReader _appSettingsReader;
        bool _streamClosed = false;

        public CustomTextWriter(string fileName)
        {
            _fileName = fileName;
            _appSettingsReader = new AppSettingsReader();
            _traceWriter = new StreamWriter(_fileName, true);
        }

        public override void Write(string value)
        {
            try
            {
                CheckRollover();
                _traceWriter.Write(value);
                _traceWriter.Flush();
            }
            catch(ObjectDisposedException e)
            {
                //ignore this exception
            }
        }

        public override void WriteLine(string value)
        {
            try
            {
                CheckRollover();
                _traceWriter.WriteLine(value);
                _traceWriter.Flush();
            }
            catch(ObjectDisposedException e)
            {
                //ignore this exception
            }
        }

        void CheckRollover()
        {
           var maxFileSize = int.Parse(_appSettingsReader.GetValue("MaxLogFileSizeBytes", typeof(int)).ToString());

           if(maxFileSize > 0)
           {
               try
               {
                   if(_traceWriter.BaseStream.Length > maxFileSize && !_streamClosed)
                   {
                       CloseTraceWriter();
                       _traceWriter = new StreamWriter(_fileName, false);
                       _streamClosed = false;
                   }
               }
               catch(ObjectDisposedException e)
               {
                   //ignore this exception
               }
           }
        }

        void CloseTraceWriter()
        {
            _traceWriter.Close();
            _streamClosed = true;
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