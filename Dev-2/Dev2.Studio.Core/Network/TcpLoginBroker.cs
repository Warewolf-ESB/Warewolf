using System;
using System.Network;
using System.Threading.Tasks;

namespace Dev2.Studio.Core.Network
{
    sealed class TcpLoginBroker : OutboundSRPAuthenticationBroker
    {
        public TcpLoginBroker(string username, string password, TaskCompletionSource<bool> tcs = null)
            : base(username, password)
        {
            TaskCompletionSource = tcs;
            _remoteFirewall = true;
            _localIdentifier = new FourOctetUnion('D', 'E', 'V', '2').Int32;
            _localVersion = new Version(1, 0, 0, 0);
            _localPlatform = Environment.OSVersion.Platform;
            _localServicePack = Environment.OSVersion.ServicePack;
            _localFingerprint = WeaveUtility.GetVolumeSerial(System.IO.Path.GetPathRoot(Environment.CurrentDirectory)[0].ToString().ToUpper());
        }

        public TaskCompletionSource<bool> TaskCompletionSource { get; private set; }

        protected override void OnAuthenticated(ByteBuffer buffer)
        {
        }
    }
}