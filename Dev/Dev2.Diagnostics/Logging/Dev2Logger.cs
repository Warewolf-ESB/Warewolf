
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.Common
{
    /// <summary>
    /// A single common logging location ;)
    /// </summary>
    public static class Dev2Logger
    {
        public static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public  static void SetLog(ILog log)
        {
            Log = log;
        }

        public static void UpdateLoggingConfig(string level)
        {

            var repository = LogManager.GetAllRepositories().First();
            repository.Threshold = repository.LevelMap[level];
            var hier = (Hierarchy)repository;
            var loggers = hier.GetCurrentLoggers();
            foreach (var logger in loggers)
            {
                ((Logger)logger).Level = hier.LevelMap[level];
            }
            var h = (Hierarchy)LogManager.GetRepository();
            var rootLogger = h.Root;
            rootLogger.Level = h.LevelMap[level];
        }

        public static string GetLogLevel()
        {
            var h = (Hierarchy)LogManager.GetRepository();
            var rootLogger = h.Root;
            return rootLogger.Level.DisplayName;
        }

        public static int GetLogMaxSize()
        {
            var h = (Hierarchy)LogManager.GetRepository();
            var rootLogger = h.Root;
            var appender = rootLogger.GetAppender("LogFileAppender") as RollingFileAppender;
            if (appender != null)
            {
                var logSize = appender.MaxFileSize / 1024 / 1024;
                return (int)Math.Round((decimal)logSize,0);
            }
            return 0;
        }

        public static void WriteLogSettings(string maxLogSize,string logLevel, string settingsConfigFile)
        {
            var settingsDocument = XDocument.Load(settingsConfigFile);
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
                settingsDocument.Save(settingsConfigFile);
            }
        }
    }


}
