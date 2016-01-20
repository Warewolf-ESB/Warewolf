
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
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
        private static ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ILog _logEvents = LogManager.GetLogger("EventLogLogger");
        public  static void SetLog(ILog log)
        {
            _log = log;
        }

        public static void Debug(object message)
        {
            _log.Debug(message);
            _logEvents.Debug(message);
        }
        
        public static void Debug(object message,Exception exception)
        {
            _log.Debug(message,exception);
            _logEvents.Debug(message, exception);
        }
        
        public static void Error(object message)
        {
            _log.Error(message);
            _logEvents.Error(message);
        }

        public static void Error(object message, Exception exception)
        {
            _log.Error(message, exception);
            _logEvents.Error(message, exception);

        }

        public static void Warn(object message)
        {
            _log.Warn(message);
            _logEvents.Warn(message);
        }

        public static void Warn(object message, Exception exception)
        {
            _log.Warn(message, exception);
            _logEvents.Warn(message, exception);

        }

        public static void Fatal(object message)
        {
            _log.Fatal(message);
            _logEvents.Fatal(message);
        }

        public static void Fatal(object message, Exception exception)
        {
            _log.Fatal(message, exception);
            _logEvents.Fatal(message, exception);

        }

        public static void Info(object message)
        {
            _log.Info(message);
            _logEvents.Info(message);
        }

        public static void Info(object message, Exception exception)
        {
            _log.Info(message, exception);
            _logEvents.Info(message, exception);

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

        public static string GetFileLogLevel()
        {
            var h = (Hierarchy)LogManager.GetRepository();
            var rootLogger = h.Root;
            return rootLogger.Level.DisplayName;
        }
        public static string GetEventLogLevel()
        {
            var h = (Hierarchy)LogManager.GetRepository();
            var rootLogger = h.GetLogger("EventLogLogger") as Logger;
            if(rootLogger != null)
            {
                return rootLogger.Level.DisplayName;
            }
            return "";
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

        public static void WriteLogSettings(string maxLogSize, string fileLogLevel, string eventLogLevel, string settingsConfigFile,string applicationNameForEventLog)
        {
            var settingsDocument = XDocument.Load(settingsConfigFile);
            var log4netElement = settingsDocument.Element("log4net");
            if (log4netElement != null)
            {
                var appenderElements = log4netElement.Elements("appender");
                var appenders = appenderElements as IList<XElement> ?? appenderElements.ToList();
                var fileAppender = appenders.FirstOrDefault(element => element.Attribute("type").Value=="Log4Net.Async.AsyncRollingFileAppender,Log4Net.Async");
                if (fileAppender != null)
                {
                    var maxFileSizeElement = fileAppender.Element("maximumFileSize");
                    if (maxFileSizeElement != null)
                    {
                        var maxFileSizeElementValueAttrib = maxFileSizeElement.Attribute("value");
                        if (maxFileSizeElementValueAttrib != null)
                        {
                            maxFileSizeElementValueAttrib.Value = maxLogSize + "MB";
                        }
                    }
                }
                var eventAppender = appenders.FirstOrDefault(element => element.Attribute("type").Value == "log4net.Appender.EventLogAppender");
                if(eventAppender == null)
                {
                    var eventAppenderElement = new XElement("appender");
                    eventAppenderElement.SetAttributeValue("name","EventLogLogger");
                    eventAppenderElement.SetAttributeValue("type", "log4net.Appender.EventLogAppender");
                    var logNameElement = new XElement("logName");
                    logNameElement.SetAttributeValue("value","Warewolf");
                    eventAppenderElement.Add(logNameElement);
                    var applicationNameElement = new XElement("applicationName");
                    logNameElement.SetAttributeValue("value", applicationNameForEventLog);
                    eventAppenderElement.Add(applicationNameElement);
                    var layoutElement = new XElement("layout");
                    layoutElement.SetAttributeValue("type", "log4net.Layout.PatternLayout");
                    var conversionPattenElement = new XElement("conversionPattern");
                    conversionPattenElement.SetAttributeValue("value", "%date [%thread] %-5level - %message%newline");
                    layoutElement.Add(conversionPattenElement);
                    eventAppenderElement.Add(layoutElement);
                    log4netElement.Add(eventAppenderElement);
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
                            levelElementValueAttrib.Value = fileLogLevel;
                        }
                    }
                }
                var eventLogElement = log4netElement.Element("logger");
                if(eventLogElement != null)
                {
                    if(eventLogElement.Attribute("name").Value == "EventLogLogger")
                    {
                        var levelElement = eventLogElement.Element("level");
                        if(levelElement != null)
                        {
                            var levelElementValueAttrib = levelElement.Attribute("value");
                            if(levelElementValueAttrib != null)
                            {
                                levelElementValueAttrib.Value = eventLogLevel;
                            }
                        }
                    }
                }
                else
                {
                    var loggerElement = new XElement("logger");
                    loggerElement.SetAttributeValue("name", "EventLogLogger");
                    loggerElement.SetAttributeValue("additivity", "false");
                    var levelElement = new XElement("level");
                    levelElement.SetAttributeValue("value", eventLogLevel);
                    var refElement = new XElement("appender-ref");
                    levelElement.SetAttributeValue("ref", "EventLogLogger");
                    loggerElement.Add(levelElement);
                    loggerElement.Add(refElement);
                    log4netElement.Add(loggerElement);
                }
                settingsDocument.Save(settingsConfigFile);
            }
        }
    }


}
