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
using System.Threading;

namespace Warewolf.Triggers
{
    public interface IManualResetEventFactory
    {
        IManualResetEventWrapper New(bool initialState);
    }

    public class ManualResetEventFactory : IManualResetEventFactory
    {
        public IManualResetEventWrapper New(bool initialState)
        {
            return new ManualResetEventWrapper(initialState: initialState);
        }
    }

    public interface IManualResetEventWrapper : IDisposable
    {
        bool WaitOne(TimeSpan timeSpan);
        void Set();
    }

    public class ManualResetEventWrapper : IManualResetEventWrapper
    {
        private ManualResetEvent _manualResetEvent;
        public ManualResetEventWrapper(bool initialState)
        {
            _manualResetEvent = new ManualResetEvent(initialState);
        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                _manualResetEvent.Dispose();

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public bool WaitOne(TimeSpan timeSpan)
        {
            return _manualResetEvent.WaitOne(timeSpan);
        }

        public void Set()
        {
            _manualResetEvent.Set();
        }
    }
}