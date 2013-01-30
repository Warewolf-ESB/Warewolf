using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Dev2.Network.Messages;
using Dev2.Network.Messaging.Messages;

namespace Dev2.Network.Messaging
{
    public class SynchronousNetworkMessageToken
    {
        #region Class Members

        private DispatcherTimer _timeoutTimer;
        private long _handle;
        private INetworkMessage _result;
        private ManualResetEventSlim _waitToken;
        private CancellationTokenSource _cancelTokenSource;

        #endregion Class Members

        #region Constructor

        public SynchronousNetworkMessageToken(long handle)
        {
            _timeoutTimer = new DispatcherTimer();
            _timeoutTimer.Tick += _timeoutTimer_Tick;
            _timeoutTimer.Interval = new TimeSpan(0, 0, 10);

            _handle = handle;
            _waitToken = new ManualResetEventSlim();
            _cancelTokenSource = new CancellationTokenSource();
        }

        private void _timeoutTimer_Tick(object sender, EventArgs e)
        {
            _timeoutTimer.Stop();
            if (!_waitToken.IsSet)
            {
                SetResponse(new ErrorMessage(_handle, "Send message timeout."));
            }
        }

        #endregion Constructor

        #region Methods

        public INetworkMessage WaitForResponse()
        {
            bool cancelled = false;

            try
            {
                _timeoutTimer.Start();
                _waitToken.Wait(_cancelTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
            }

            _timeoutTimer.Stop();

            if (cancelled)
            {
                _result = null;
            }
            else
            {
                _waitToken.Dispose();
            }

            _waitToken.Dispose();

            INetworkMessage tmpResult = _result;
            _result = null;
            return tmpResult;
        }

        public void SetResponse(INetworkMessage result)
        {
            _result = result;
            _waitToken.Set();
        }

        public void Cancel()
        {
            _cancelTokenSource.Cancel();
            _cancelTokenSource.Dispose();
        }

        #endregion Methods
    }
}
