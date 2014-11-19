
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Linq;
using System.Reflection;
using log4net.Appender;
using System.Xml.Linq;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.Common
{
    /// <summary>
    /// A single common logging location ;)
    /// </summary>
    public static class Dev2Logger
    {
        public static log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public  static void SetLog(log4net.ILog log)
        {
            Log = log;
        }

        public static void UpdateLoggingConfig(string level)
        {

            var repository = log4net.LogManager.GetAllRepositories().First();
            repository.Threshold = repository.LevelMap[level];
            var hier = (log4net.Repository.Hierarchy.Hierarchy)repository;
            var loggers = hier.GetCurrentLoggers();
            foreach (var logger in loggers)
            {
                ((log4net.Repository.Hierarchy.Logger)logger).Level = hier.LevelMap[level];
            }
            var h = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
            var rootLogger = h.Root;
            rootLogger.Level = h.LevelMap[level];
        }

        public static string GetLogLevel()
        {
            var h = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
            var rootLogger = h.Root;
            return rootLogger.Level.DisplayName;
        }

        public static int GetLogMaxSize()
        {
            var h = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
            var rootLogger = h.Root;
            var appender = rootLogger.GetAppender("LogFileAppender") as RollingFileAppender;
            if (appender != null)
            {
                var logSize = appender.MaxFileSize / 1024 / 1024;
                return (int)System.Math.Round((decimal)logSize,0);
            }
            return 0;
        }

        public static void WriteLogSettings(string maxLogSize,string logLevel)
        {
            var settingsDocument = XDocument.Load("Settings.config");
            var log4netElement = settingsDocument.Element("log4net");
            if (log4netElement != null)
            {
                var appenderElement = log4netElement.Element("appender");
                if (appenderElement != null)
                {
                    var maxFileSizeElement = appenderElement.Element("maximumFileSize");
                    if (maxFileSizeElement != null)
                    {
                        var maxFileSizeElementValueAttrib = maxFileSizeElement.Attribute("value");
                        if (maxFileSizeElementValueAttrib != null)
                        {
                            maxFileSizeElementValueAttrib.Value = maxLogSize + "MB";
                        }
                    }
                }

                var rootElement = log4netElement.Element("root");
                if (rootElement != null)
                {
                    var levelElement = rootElement.Element("level");
                    if (levelElement != null)
                    {
                        var levelElementValueAttrib = levelElement.Attribute("value");
                        if (levelElementValueAttrib != null)
                        {
                            levelElementValueAttrib.Value = logLevel;
                        }
                    }
                }
                settingsDocument.Save("Settings.config");
            }
        }
    }


}
