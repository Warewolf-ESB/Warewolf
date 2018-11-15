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
using Dev2.Runtime.ESB.Execution.State;

namespace Dev2.Runtime.ESB.Execution
{
    internal class LogManager : IDisposable
    {
        private static LogManager _instance;
        private Dev2StateAuditLogger _logger;

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

        internal static IStateNotifier CreateStateNotifier(IDSFDataObject dsfDataObject)
        {
            return Instance.CreateStateNotifierImpl(dsfDataObject);
        }

        private IStateNotifier CreateStateNotifierImpl(IDSFDataObject dsfDataObject)
        {
            // TODO: check if there is already a state notifier for this workflow
            //       in this server instance. Re-use either the notifier or its loggers.
            var stateNotifier = new StateNotifier();

            if (dsfDataObject.Settings.EnableDetailedLogging)
            {
                _logger = new Dev2StateAuditLogger(new DatabaseContextFactory(), dsfDataObject);
                stateNotifier.Subscribe(_logger.StateListener);
            }
            return stateNotifier;
        }

        internal static void FlushLogs()
        {
            Instance.FlushLogsImpl();
        }
        private void FlushLogsImpl()
        {
            _logger?.Flush();
        }

        private bool isDisposed = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    _logger.Dispose();
                }
                isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
