﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Logging;
using Newtonsoft.Json;
using System;
using Warewolf.Interfaces.Auditing;
using Dev2.Data.Interfaces.Enums;

namespace Warewolf.Auditing
{
    public interface IStateAuditLogger : IDisposable
    {
        IStateListener NewStateListener(IExecutionContext dataObject);
    }

    public class StateAuditLogger : IStateAuditLogger, IWarewolfLogWriter
    {
        private IWebSocketWrapper _ws;
        private readonly IWebSocketPool _webSocketFactory;

        public IStateListener NewStateListener(IExecutionContext dataObject) => new StateListener(this, dataObject);

        public StateAuditLogger()
        {
        }

        public StateAuditLogger(IWebSocketPool webSocketFactory)
        {
            _webSocketFactory = webSocketFactory;
        }

        public void LogAuditState(Object logEntry)
        {
            Enum.TryParse(Config.Server.ExecutionLogLevel, out LogLevel executionLogLevel);
            if (logEntry is Audit auditLog && IsValidLogLevel(executionLogLevel, auditLog.LogLevel.ToString()))
            {
                var auditCommand = new AuditCommand
                {
                    Audit = auditLog,
                    Type = "LogEntry"
                };
                var jsonLogEntry = JsonConvert.SerializeObject(auditCommand);
                SendMessage(jsonLogEntry);
            }
        }

        private void SendMessage(string jsonLogEntry)
        {
            IWebSocketWrapper client = null;
            try
            {
                client = _webSocketFactory.Acquire(Config.Auditing.Endpoint);
                if (!client.IsOpen())
                {
                    client = _webSocketFactory.Acquire(Config.Auditing.Endpoint);
                }
                client.SendMessage(jsonLogEntry);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("LogAuditState", ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
            finally
            {
                _webSocketFactory.Release(client);
            }
        }

        public bool IsValidLogLevel(LogLevel executionLogLevel, string auditLogLogLevel)
        {
            switch (executionLogLevel)
            {
                case LogLevel.OFF:
                    return false;
                case LogLevel.TRACE:
                    return true;
                case LogLevel.FATAL:
                case LogLevel.ERROR:
                    switch (auditLogLogLevel.ToUpper())
                    {
                        case "FATAL":
                        case "ERROR":
                            return true;
                        default:
                            return false;
                    }
                case LogLevel.WARN:
                    switch (auditLogLogLevel.ToUpper())
                    {
                        case "FATAL":
                        case "WARN":
                        case "ERROR":
                            return true;
                        default:
                            return false;
                    }
                case LogLevel.INFO:
                    switch (auditLogLogLevel.ToUpper())
                    {
                        case "FATAL":
                        case "WARN":
                        case "ERROR":
                        case "INFO":
                            return true;
                        default:
                            return false;
                    }
                case LogLevel.DEBUG:
                    switch (auditLogLogLevel.ToUpper())
                    {
                        case "FATAL":
                        case "WARN":
                        case "ERROR":
                        case "INFO":
                        case "DEBUG":
                            return true;
                        default:
                            return false;
                    }
                default:
                    return false;
            }
        }

        private bool _isDisposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}