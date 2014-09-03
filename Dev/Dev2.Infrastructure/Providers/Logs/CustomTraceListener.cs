using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using Dev2.Common;

namespace Dev2.Providers.Logs
{
    /// <summary>
    /// This is the trace writer used by the studio. Note other than testing there are no usages
    /// for this class as it is initialized from the app.config
    /// </summary>
    public class CustomTextWriter : TraceListener
    {
        public static string LoggingFileName
        {
            get
            {

                return Path.Combine(StudioLogPath, "Warewolf Studio.log");
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
                Dev2Logger.Log.Info(value);
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

                Dev2Logger.Log.Info(value);

            }
            catch(ObjectDisposedException)
            {
                //ignore this exception
            }
        }

     

  

        protected override void Dispose(bool disposing)
        {
      
        }
    }
}