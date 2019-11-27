using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml.Linq;
using System.Linq;

namespace Warewolf.Common
{
    [ExcludeFromCodeCoverage]
    public class WarewolfLogger
    {
        static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Debug(object message, string executionId)
        {
            var customMessage = UpdateCustomMessage(message, executionId);
            _log.Debug(customMessage);
        }

        public static void Debug(object message, Exception exception, string executionId)
        {
            var customMessage = UpdateCustomMessage(message, executionId);
            _log.Debug(customMessage, exception);
        }

        public static void Error(object message, string executionId)
        {
            var customMessage = UpdateCustomMessage(message, executionId);
            _log.Error(customMessage);
        }

        public static void Error(object message, Exception exception, string executionId)
        {
            var customMessage = UpdateCustomMessage(message, executionId);
            _log.Error(customMessage, exception);
        }

        public static void Warn(object message, string executionId)
        {
            var customMessage = UpdateCustomMessage(message, executionId);
            _log.Warn(customMessage);
        }

        public static void Warn(object message, Exception exception, string executionId)
        {
            var customMessage = UpdateCustomMessage(message, executionId);
            _log.Warn(customMessage, exception);
        }

        public static void Fatal(object message, string executionId)
        {
            var customMessage = UpdateCustomMessage(message, executionId);
            _log.Fatal(customMessage);
        }

        public static void Fatal(object message, Exception exception, string executionId)
        {
            var customMessage = UpdateCustomMessage(message, executionId);
            _log.Fatal(customMessage, exception);
        }

        public static void Info(object message, string executionId)
        {
            var customMessage = UpdateCustomMessage(message, executionId);
            _log.Info(customMessage);
        }

        public static void Info(object message, Exception exception, string executionId)
        {
            var customMessage = UpdateCustomMessage(message, executionId);
            _log.Info(customMessage, exception);
        }
        static string UpdateCustomMessage(object message, string executionId) => $"[{executionId}] - {message}";
    }
}
