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
        
        public CustomTextWriter(string fileName)
        {
            _fileName = fileName;
            _appSettingsReader = new AppSettingsReader();
            _traceWriter = new StreamWriter(_fileName, true);
        }

        public override void Write(string value)
        {
            CheckRollover();
            _traceWriter.Write(value);
            _traceWriter.Flush();
        }

        public override void WriteLine(string value)
        {
            CheckRollover();
            _traceWriter.WriteLine(value);
            _traceWriter.Flush();
        }

        void CheckRollover()
        {
           var maxFileSize = int.Parse(_appSettingsReader.GetValue("MaxLogFileSizeBytes", typeof(int)).ToString());

           if(maxFileSize > 0)
           {
               if(_traceWriter.BaseStream.Length > maxFileSize)
               {
                   _traceWriter.Close();
                   _traceWriter = new StreamWriter(_fileName, false);
               }
           }
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                _traceWriter.Close();
            }
        }
    }
}