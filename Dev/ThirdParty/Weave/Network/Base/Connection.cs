
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Net;
using System.Net.Sockets;

namespace System.Network
{
    public abstract class Connection : INetworkOperator, IDisposable
    {
        #region Constants
        private const byte PHFMask = 1 + 2 + 4 + 8;
        private const byte PHIMask = 16 + 32 + 64 + 128;

        protected static byte AppendFlags(byte encoded, PacketHeaderFlags flags)
        {
            int channel = encoded & ~PHIMask;
            flags |= (PacketHeaderFlags)(encoded & ~PHFMask);
            return (byte)((((byte)channel) & ~PHIMask) | ((byte)flags & ~PHFMask));
        }
        #endregion

        #region Instance Fields
        protected bool _disposed;
        protected bool _alive;

        protected NetworkHost _host;
        protected Socket _socket;
        protected NetworkDirection _direction;

        protected IPAddress _address;
        protected int _port;
        protected string _addressString;

        protected PacketAssembler _assembler;
        protected AuthenticationBroker _authBroker;
        protected ICryptProvider _crypt;
        protected NetworkContext _context;
        protected bool _wasAlive;
        #endregion

        #region Internal Properties
        public AuthenticationBroker AuthBroker { get { return _authBroker; } set { _authBroker = value; } }
        public ICryptProvider Crypt { get { return _crypt; } set { _crypt = value; } }
        public PacketAssembler Assembler { get { return _assembler; } set { _assembler = value; } }
        internal bool WasAlive { get { return _wasAlive; } }
        #endregion

        #region Public Properties
        public bool Disposed { get { return _disposed; } }
        public bool Alive { get { return _alive; } }
        public NetworkHost Host { get { return _host; } }
        public NetworkDirection Direction { get { return _direction; } }
        public NetworkContext Context { get { return _context; } set { _context = value; } }
        public IPAddress Address { get { return _address; } }
        public int Port { get { return _port; } }
        public Socket Socket { get { return _socket; } }

        #endregion

        #region Constructor
        public Connection(NetworkHost host, Socket socket, NetworkDirection direction)
        {
            _crypt = NullCryptProvider.Singleton;
            _host = host;
            _socket = socket;
            _direction = direction;

            try
            {
                IPEndPoint ipEP = (IPEndPoint)_socket.RemoteEndPoint;
                _address = ipEP.Address;
                _port = ipEP.Port;
                _addressString = _address.ToString();
            }
            catch
            {
                _address = IPAddress.None;
                _port = -1;
                _addressString = "(error)";
            }
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return _addressString;
        }
        #endregion

        #region Start Handling
        public abstract void Start();
        #endregion

        #region Outbound Handling
        public abstract void Send(Packet p);
        public abstract void Send(byte[] data, int index, int length);
        public abstract void SendRange(params Packet[] packets);

        public abstract void SendExtended(Packet p, byte[] extension, int extensionLength);
        #endregion

        #region Disposal Handling
        ~Connection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected abstract void Dispose(bool disposing);
        #endregion

        #region PendingData
        protected sealed class PendingData
        {
            public byte[] Header;
            public byte[] Data;
            public int Index;
            public int Length;

            public PendingData(byte[] header, byte[] data, int index, int length)
            {
                Header = header;
                Data = data;
                Index = index;
                Length = length;
            }
        }
        #endregion
    }
}
