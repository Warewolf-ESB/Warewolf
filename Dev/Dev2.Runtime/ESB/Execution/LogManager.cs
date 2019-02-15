﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Container;
using Dev2.Interfaces;
using Dev2.Runtime.Auditing;
using Dev2.Common.Interfaces.Logging;

namespace Dev2.Runtime.ESB.Execution
{
    public class LogManagerImplementation : ILogManager, IDisposable
    {
        readonly Dev2StateAuditLogger _logger = new Dev2StateAuditLogger(new DatabaseContextFactory(), new WarewolfQueue());

        public IStateNotifier CreateStateNotifierImpl(IDSFDataObject dsfDataObject)
        {
            var stateNotifier = new StateNotifier();

            if (dsfDataObject.Settings.EnableDetailedLogging)
            {
                stateNotifier.Subscribe(_logger.NewStateListener(dsfDataObject));
            }
            return stateNotifier;
        }

        public void FlushLogs()
        {
            _logger.Flush();
        }

        private bool _isDisposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _logger.Dispose();
                }
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }

    public class LogManager : IDisposable
    {
        private static LogManagerImplementation _instance;
        private static readonly object _lock = new object();


        private static LogManagerImplementation Instance
        {
            get
            {
                if (_instance is null)
                {
                    lock (_lock)
                    {
                        if (_instance is null)
                        {
                            _instance = new LogManagerImplementation();
                        }
                    }
                }
                return _instance;
            }
        }


        internal static IStateNotifier CreateStateNotifier(IDSFDataObject dsfDataObject)
        {
            return Instance.CreateStateNotifierImpl(dsfDataObject);
        }


        public static void FlushLogs()
        {
            Instance.FlushLogs();
        }

        public void Dispose()
        {
            ((IDisposable)_instance).Dispose();
        }
    }
}
