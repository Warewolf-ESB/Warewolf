using System.Net;
using System.Net.Sockets;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.StandardLibrary.Sockets
{
	[RubyClass("UDPSocket", BuildConfig = "!SILVERLIGHT")]
	public class UDPSocket : IPSocket
	{
		public UDPSocket(RubyContext context)
			: base(context)
		{
		}

		public UDPSocket(RubyContext context, Socket socket)
			: base(context, socket)
		{
		}

		[RubyConstructor]
		public static UDPSocket CreateUDPSocket(ConversionStorage<MutableString> stringCast, ConversionStorage<int> fixnumCast, RubyClass self, object family)
		{
			return new UDPSocket(self.Context, CreateSocket(RubyBasicSocket.ConvertToAddressFamily(stringCast, fixnumCast, family)));
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static UDPSocket Reinitialize(ConversionStorage<MutableString> stringCast, ConversionStorage<int> fixnumCast, UDPSocket self, object family)
		{
			self.Socket = CreateSocket(RubyBasicSocket.ConvertToAddressFamily(stringCast, fixnumCast, family));
			return self;
		}

		private static Socket CreateSocket(AddressFamily addressFamily)
		{
			return new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp);
		}

		[RubyMethod("bind")]
		public static int Bind(ConversionStorage<MutableString> stringCast, ConversionStorage<int> fixnumCast, UDPSocket self, object hostNameOrAddress, object port)
		{
			int port2 = RubyBasicSocket.ConvertToPortNum(stringCast, fixnumCast, port);
			IPAddress address = ((hostNameOrAddress != null) ? RubyBasicSocket.GetHostAddress(RubyBasicSocket.ConvertToHostString(stringCast, hostNameOrAddress)) : IPAddress.Loopback);
			IPEndPoint localEP = new IPEndPoint(address, port2);
			self.Socket.Bind(localEP);
			return 0;
		}

		[RubyMethod("connect")]
		public static int Connect(ConversionStorage<MutableString> stringCast, ConversionStorage<int> fixnumCast, UDPSocket self, object hostname, object port)
		{
			string host = RubyBasicSocket.ConvertToHostString(stringCast, hostname);
			int port2 = RubyBasicSocket.ConvertToPortNum(stringCast, fixnumCast, port);
			self.Socket.Connect(host, port2);
			return 0;
		}

		[RubyMethod("recvfrom_nonblock")]
		public static RubyArray ReceiveFromNonBlocking(ConversionStorage<int> fixnumCast, IPSocket self, int length)
		{
			bool blocking = self.Socket.Blocking;
			try
			{
				self.Socket.Blocking = false;
				return IPSocket.ReceiveFrom(fixnumCast, self, length, null);
			}
			finally
			{
				self.Socket.Blocking = blocking;
			}
		}

		[RubyMethod("recvfrom_nonblock")]
		public static RubyArray ReceiveFromNonBlocking(ConversionStorage<int> fixnumCast, IPSocket self, int length, object flags)
		{
			bool blocking = self.Socket.Blocking;
			try
			{
				self.Socket.Blocking = false;
				return IPSocket.ReceiveFrom(fixnumCast, self, length, flags);
			}
			finally
			{
				self.Socket.Blocking = blocking;
			}
		}

		[RubyMethod("send")]
		public static int Send(ConversionStorage<int> fixnumCast, ConversionStorage<MutableString> stringCast, RubyBasicSocket self, [NotNull][DefaultProtocol] MutableString message, object flags, object hostNameOrAddress, object port)
		{
			Protocols.CheckSafeLevel(fixnumCast.Context, 4, "send");
			int port2 = RubyBasicSocket.ConvertToPortNum(stringCast, fixnumCast, port);
			SocketFlags socketFlags = RubyBasicSocket.ConvertToSocketFlag(fixnumCast, flags);
			IPAddress address = ((hostNameOrAddress != null) ? RubyBasicSocket.GetHostAddress(RubyBasicSocket.ConvertToHostString(stringCast, hostNameOrAddress)) : IPAddress.Loopback);
			EndPoint remoteEP = new IPEndPoint(address, port2);
			return self.Socket.SendTo(message.ConvertToBytes(), socketFlags, remoteEP);
		}

		[RubyMethod("send")]
		public new static int Send(ConversionStorage<int> fixnumCast, RubyBasicSocket self, [NotNull][DefaultProtocol] MutableString message, object flags)
		{
			return RubyBasicSocket.Send(fixnumCast, self, message, flags);
		}

		[RubyMethod("send")]
		public new static int Send(ConversionStorage<int> fixnumCast, RubyBasicSocket self, [DefaultProtocol][NotNull] MutableString message, object flags, [DefaultProtocol][NotNull] MutableString to)
		{
			return RubyBasicSocket.Send(fixnumCast, self, message, flags, to);
		}
	}
}
