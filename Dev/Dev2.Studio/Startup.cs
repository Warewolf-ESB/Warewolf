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

using Dev2.Common;
using Dev2.Studio;
using Dev2.Utils;
using log4net.Config;
using System;
using System.IO;

namespace Dev2
{
    public static class Startup
    {
        [STAThread]
        public static void Main(string[] args)
        {
            ConfigureLogging();
            Dev2Logger.Info("Studio " + Warewolf.Studio.AntiCorruptionLayer.Utils.FetchVersionInfo() + " Starting.", GlobalConstants.WarewolfInfo);
            try {
                var wrapper = new SingleInstanceApplicationWrapper();
                wrapper.Run(args);
            }
            catch (Exception e)
            {
                Dev2Logger.Fatal("failed starting app", e, GlobalConstants.ServerWorkspaceID.ToString());
            }
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
