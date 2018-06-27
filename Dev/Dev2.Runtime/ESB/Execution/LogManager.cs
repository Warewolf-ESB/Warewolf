/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Enums;
using Dev2.Interfaces;

namespace Dev2.Runtime.ESB.Execution
{
    internal class LogManager
    {
        private static LogManager _instance;
        private static LogManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LogManager();
                }
                return _instance;
            }
        }

        internal static IDev2StateLogger CreateDetailedLoggerForWorkflow(IDSFDataObject dsfDataObject)
        {
            return Instance.CreateLogger(dsfDataObject);
        }

        private IDev2StateLogger CreateLogger(IDSFDataObject dsfDataObject)
        {
            if (!dsfDataObject.Settings.EnableDetailedLogging)
            {
                return new DummyStateLogger();
            }

            IDev2StateLogger logger;
            if (dsfDataObject.Settings.LoggerType == LoggerType.JSON)
            {
                logger = new Dev2JsonStateLogger(dsfDataObject);
                return logger;
            }
            else
            {
                throw new Exception("logger not implemented");
            }
        }

        class DummyStateLogger : IDev2StateLogger
        {
            public void Subscribe(IStateLoggerListener listener)
            {
            }
            public void Close()
            {
            }

            public void Dispose()
            {
            }

            public void LogAdditionalDetail(object detail, string callerName)
            {
            }

            public void LogExecuteCompleteState()
            {
            }

            public void LogExecuteException(Exception e, IDev2Activity activity)
            {
            }

            public void LogPostExecuteState(IDev2Activity previousActivity, IDev2Activity nextActivity)
            {
            }

            public void LogPreExecuteState(IDev2Activity nextActivity)
            {
            }

            public void LogStopExecutionState()
            {
            }
        }
    }
}