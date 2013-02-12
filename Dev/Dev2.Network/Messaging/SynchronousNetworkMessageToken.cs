using Dev2.Network.Messages;
using Dev2.Network.Messaging.Messages;
using System;
using System.Threading;

namespace Dev2.Network.Messaging
{
    public class SynchronousNetworkMessageToken
    {
        #region Class Members

        private long _handle;
        private INetworkMessage _result;
        private ManualResetEventSlim _waitToken;
        private CancellationTokenSource _cancelTokenSource;

        #endregion Class Members

        #region Constructor

        public SynchronousNetworkMessageToken(long handle)
        {
            _handle = handle;
            _waitToken = new ManualResetEventSlim();
            _cancelTokenSource = new CancellationTokenSource();
        }

        #endregion Constructor

        #region Methods

        public INetworkMessage WaitForResponse(int timeOut)
        {
            bool cancelled = false;

            try
            {
                _waitToken.Wait(TimeSpan.FromMilliseconds(timeOut), _cancelTokenSource.Token); //Bug 8796, added timeout on wait
                if (!_waitToken.IsSet)
                {
                    SetResponse(new ErrorMessage(_handle, "Send message timeout."));
                }
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
            }

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
