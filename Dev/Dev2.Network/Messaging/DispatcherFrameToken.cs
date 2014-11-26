
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows.Threading;

namespace Dev2.Network.Messaging
{
    public class DispatcherFrameToken<T> where T : class 
    {
        #region Class Members

        private T _result;
        private DispatcherFrame _dispatcherFrame;
        private readonly object _dispatcherFrameLock;
        private DispatcherTimer _timeoutTimer;
        private bool _cancelled;
        private T _timeoutResult;

        #endregion Class Members

        #region Constructor

        public DispatcherFrameToken(T timeoutResult)
        {
            _timeoutResult = timeoutResult;
            _dispatcherFrame = new DispatcherFrame();
            _dispatcherFrameLock = new object();

            _timeoutTimer = new DispatcherTimer();
        }

        #endregion Constructor

        #region Methods

        public T WaitForResponse(int timeOut)
        {
            try
            {
                DispatcherFrame frame;

                lock (_dispatcherFrameLock)
                {
                    frame = _dispatcherFrame;
                }

                if (frame != null)
                {
                    StartTimeoutTimer(timeOut);
                    Dispatcher.PushFrame(frame);
                }
            }
            finally
            {
                ExitFrame();
            }

            if (_cancelled)
            {
                _result = null;
            }

            T tmpResult = _result;
            _result = null;
            return tmpResult;
        }

        public void SetResponse(T result)
        {
            _result = result;
            ExitFrame();
        }

        public void Cancel()
        {
            _cancelled = true;
            ExitFrame();
        }

        private void ExitFrame()
        {
            _timeoutTimer.Stop();
            DispatcherFrame frame;

            lock (_dispatcherFrameLock)
            {
                frame = _dispatcherFrame;
                _dispatcherFrame = null;
            }

            if (frame != null)
            {
                frame.Continue = false;
            }
        }

        private void StartTimeoutTimer(int timeOut)
        {
            _timeoutTimer.Stop();
            _timeoutTimer.Interval = TimeSpan.FromMilliseconds(timeOut);
            _timeoutTimer.Tick += (sender, args) =>
            {
                lock (_dispatcherFrameLock)
                {
                    if (_dispatcherFrame != null && _dispatcherFrame.Continue)
                    {
                        SetResponse(_timeoutResult);
                    }
                }
            };
            _timeoutTimer.Start();
        }

        #endregion Methods
    }
}
