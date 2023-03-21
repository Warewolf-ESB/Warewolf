using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Sockets
{
	[RubyClass("TCPSocket", BuildConfig = "!SILVERLIGHT")]
	public class TCPSocket : IPSocket
	{
		public TCPSocket(RubyContext context)
			: base(context)
		{
		}

		public TCPSocket(RubyContext context, Socket socket)
			: base(context, socket)
		{
		}

		[RubyConstructor]
		public static TCPSocket CreateTCPSocket(ConversionStorage<MutableString> stringCast, ConversionStorage<int> fixnumCast, RubyClass self, [DefaultProtocol] MutableString remoteHost, object remotePort, [Optional] int localPort)
		{
			if (localPort != 0)
			{
				throw new NotImplementedError();
			}
			return new TCPSocket(self.Context, CreateSocket(remoteHost, RubyBasicSocket.ConvertToPortNum(stringCast, fixnumCast, remotePort)));
		}

		[RubyConstructor]
		public static TCPSocket CreateTCPSocket(ConversionStorage<MutableString> stringCast, ConversionStorage<int> fixnumCast, RubyClass self, [DefaultProtocol] MutableString remoteHost, object remotePort, [DefaultProtocol] MutableString localHost, object localPort)
		{
			return BindLocalEndPoint(CreateTCPSocket(stringCast, fixnumCast, self, remoteHost, remotePort, 0), localHost, RubyBasicSocket.ConvertToPortNum(stringCast, fixnumCast, localPort));
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static TCPServer Reinitialize(ConversionStorage<MutableString> stringCast, ConversionStorage<int> fixnumCast, TCPServer self, [DefaultProtocol] MutableString remoteHost, object remotePort, [Optional] int localPort)
		{
			if (localPort != 0)
			{
				throw new NotImplementedError();
			}
			self.Socket = CreateSocket(remoteHost, RubyBasicSocket.ConvertToPortNum(stringCast, fixnumCast, remotePort));
			return self;
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static TCPServer Reinitialize(ConversionStorage<MutableString> stringCast, ConversionStorage<int> fixnumCast, TCPServer self, [DefaultProtocol] MutableString remoteHost, object remotePort, [DefaultProtocol] MutableString localHost, object localPort)
		{
			self.Socket = CreateSocket(remoteHost, RubyBasicSocket.ConvertToPortNum(stringCast, fixnumCast, remotePort));
			BindLocalEndPoint(self, localHost, RubyBasicSocket.ConvertToPortNum(stringCast, fixnumCast, localPort));
			return self;
		}

		private static Socket CreateSocket(MutableString remoteHost, int port)
		{
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try
			{
				if (remoteHost != null)
				{
					socket.Connect(remoteHost.ConvertToString(), port);
					return socket;
				}
				socket.Connect(IPAddress.Loopback, port);
				return socket;
			}
			catch (SocketException ex)
			{
				SocketError socketErrorCode = ex.SocketErrorCode;
				if (socketErrorCode == SocketError.ConnectionRefused)
				{
					throw new Errno.ConnectionRefusedError();
				}
				throw;
			}
		}

		private static TCPSocket BindLocalEndPoint(TCPSocket socket, MutableString localHost, int localPort)
		{
			IPAddress address = ((localHost != null) ? RubyBasicSocket.GetHostAddress(localHost.ConvertToString()) : IPAddress.Loopback);
			IPEndPoint localEP = new IPEndPoint(address, localPort);
			socket.Socket.Bind(localEP);
			return socket;
		}

		[RubyMethod("gethostbyname", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray GetHostByName(ConversionStorage<MutableString> stringCast, RubyClass self, object hostNameOrAddress)
		{
			return RubyBasicSocket.GetHostByName(self.Context, RubyBasicSocket.ConvertToHostString(stringCast, hostNameOrAddress), false);
		}
	}
}
