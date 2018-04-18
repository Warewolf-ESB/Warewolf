using Dev2.Common;
using Dev2.Studio;
using Dev2.Utils;
using log4net.Config;
using System;
using System.IO;
using Warewolf.Studio.AntiCorruptionLayer;

namespace Dev2
{
    public static class Startup
    {
        [STAThread]
        public static void Main(string[] args)
        {
            ConfigureLogging();
            Dev2Logger.Info("Studio " + Utils.FetchVersionInfo() + " Starting.", GlobalConstants.WarewolfInfo);
            var wrapper = new SingleInstanceApplicationWrapper();
            wrapper.Run(args);
        }

        static void ConfigureLogging()
        {
            var settingsConfigFile = HelperUtils.GetStudioLogSettingsConfigFile();
            if (!File.Exists(settingsConfigFile))
            {
                File.WriteAllText(settingsConfigFile, GlobalConstants.DefaultStudioLogFileConfig);
            }
            Dev2Logger.AddEventLogging(settingsConfigFile, GlobalConstants.WarewolfStudio);
            XmlConfigurator.ConfigureAndWatch(new FileInfo(settingsConfigFile));
        }
    }

    public class SingleInstanceApplicationWrapper : Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase, IDisposable
    {
        App _app;

        public SingleInstanceApplicationWrapper()
        {
            IsSingleInstance = true;
        }

        public void Dispose()
        {
            ((IDisposable)_app).Dispose();
        }

        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs eventArgs)
        {
            _app = new App();
            _app.Run();

            return false;
        }

        protected override void OnStartupNextInstance(Microsoft.VisualBasic.ApplicationServices.StartupNextInstanceEventArgs eventArgs)
        {
            if (eventArgs.CommandLine.Count > 0)
            {
                _app.OpenBasedOnArguments(new WarwolfStartupEventArgs(eventArgs));
            }
        }
    }
}
