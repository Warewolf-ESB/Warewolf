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

using System;
using Dev2.Interfaces;
using Warewolf.Logger;
using Warewolf.Auditing;

namespace Dev2
{
    public class LogManager : ILogManager
    {
        IStateAuditLogger _logger;
        private static ILogManager _instance;
        private bool _isDisposed = false;
        private static readonly object _lock = new object();

        private LogManager()
        {
        }

        public static ILogManager Instance
        {
            get
            {
                if (_instance is null)
                {
                    lock (_lock)
                    {
                        if (_instance is null)
                        {
                            _instance = new LogManager();
                        }
                    }
                }
                return _instance;
            }
        }

        public IStateNotifier CreateStateNotifier(IDSFDataObject dsfDataObject)
        {
            var stateNotifier = new StateNotifier();

            if (dsfDataObject.Settings.EnableDetailedLogging)
            {
                _logger = new StateAuditLogger(new WebSocketPool());
                stateNotifier.Subscribe(_logger.NewStateListener(dsfDataObject));
            }

            return stateNotifier;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _logger?.Dispose();
                    ((IDisposable)_instance).Dispose();
                }
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
