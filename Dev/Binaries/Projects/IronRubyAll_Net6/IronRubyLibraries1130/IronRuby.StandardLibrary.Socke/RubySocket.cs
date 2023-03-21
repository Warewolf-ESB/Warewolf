using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronRuby.StandardLibrary.Sockets
{
	[Includes(new Type[] { typeof(SocketConstants) }, Copy = true)]
	[RubyClass("Socket", BuildConfig = "!SILVERLIGHT")]
	public class RubySocket : RubyBasicSocket
	{
		[RubyModule("Constants", BuildConfig = "!SILVERLIGHT")]
		public class SocketConstants
		{
			[RubyConstant]
			public const int AF_APPLETALK = 16;

			[RubyConstant]
			public const int AF_ATM = 22;

			[RubyConstant]
			public const int AF_CCITT = 10;

			[RubyConstant]
			public const int AF_CHAOS = 5;

			[RubyConstant]
			public const int AF_DATAKIT = 9;

			[RubyConstant]
			public const int AF_DLI = 13;

			[RubyConstant]
			public const int AF_ECMA = 8;

			[RubyConstant]
			public const int AF_HYLINK = 15;

			[RubyConstant]
			public const int AF_IMPLINK = 3;

			[RubyConstant]
			public const int AF_INET = 2;

			[RubyConstant]
			public const int AF_INET6 = 23;

			[RubyConstant]
			public const int AF_IPX = 6;

			[RubyConstant]
			public const int AF_ISO = 7;

			[RubyConstant]
			public const int AF_LAT = 14;

			[RubyConstant]
			public const int AF_MAX = 29;

			[RubyConstant]
			public const int AF_NETBIOS = 17;

			[RubyConstant]
			public const int AF_NS = 6;

			[RubyConstant]
			public const int AF_OSI = 7;

			[RubyConstant]
			public const int AF_PUP = 4;

			[RubyConstant]
			public const int AF_SNA = 11;

			[RubyConstant]
			public const int AF_UNIX = 1;

			[RubyConstant]
			public const int AF_UNSPEC = 0;

			[RubyConstant]
			public const int AI_PASSIVE = 1;

			[RubyConstant]
			public const int AI_CANONNAME = 2;

			[RubyConstant]
			public const int AI_NUMERICHOST = 4;

			[RubyConstant]
			public const int EAI_AGAIN = 2;

			[RubyConstant]
			public const int EAI_BADFLAGS = 3;

			[RubyConstant]
			public const int EAI_FAIL = 4;

			[RubyConstant]
			public const int EAI_FAMILY = 5;

			[RubyConstant]
			public const int EAI_MEMORY = 6;

			[RubyConstant]
			public const int EAI_NODATA = 7;

			[RubyConstant]
			public const int EAI_NONAME = 8;

			[RubyConstant]
			public const int EAI_SERVICE = 9;

			[RubyConstant]
			public const int EAI_SOCKTYPE = 10;

			[RubyConstant]
			public const int IPPORT_RESERVED = 1024;

			[RubyConstant]
			public const int IPPORT_USERRESERVED = 5000;

			[RubyConstant]
			public const int INET_ADDRSTRLEN = 16;

			[RubyConstant]
			public const int INET6_ADDRSTRLEN = 46;

			[RubyConstant]
			public const uint INADDR_ALLHOSTS_GROUP = 3758096385u;

			[RubyConstant]
			public const int INADDR_ANY = 0;

			[RubyConstant]
			public const uint INADDR_BROADCAST = uint.MaxValue;

			[RubyConstant]
			public const int INADDR_LOOPBACK = 2130706433;

			[RubyConstant]
			public const uint INADDR_MAX_LOCAL_GROUP = 3758096639u;

			[RubyConstant]
			public const uint INADDR_NONE = uint.MaxValue;

			[RubyConstant]
			public const uint INADDR_UNSPEC_GROUP = 3758096384u;

			[RubyConstant]
			public const int IP_DEFAULT_MULTICAST_TTL = 1;

			[RubyConstant]
			public const int IP_DEFAULT_MULTICAST_LOOP = 1;

			[RubyConstant]
			public const int IP_OPTIONS = 1;

			[RubyConstant]
			public const int IP_HDRINCL = 2;

			[RubyConstant]
			public const int IP_TOS = 3;

			[RubyConstant]
			public const int IP_TTL = 4;

			[RubyConstant]
			public const int IP_MULTICAST_IF = 9;

			[RubyConstant]
			public const int IP_MULTICAST_TTL = 10;

			[RubyConstant]
			public const int IP_MULTICAST_LOOP = 11;

			[RubyConstant]
			public const int IP_ADD_MEMBERSHIP = 12;

			[RubyConstant]
			public const int IP_DROP_MEMBERSHIP = 13;

			[RubyConstant]
			public const int IP_ADD_SOURCE_MEMBERSHIP = 15;

			[RubyConstant]
			public const int IP_DROP_SOURCE_MEMBERSHIP = 16;

			[RubyConstant]
			public const int IP_BLOCK_SOURCE = 17;

			[RubyConstant]
			public const int IP_UNBLOCK_SOURCE = 18;

			[RubyConstant]
			public const int IP_PKTINFO = 19;

			[RubyConstant]
			public const int IP_MAX_MEMBERSHIPS = 20;

			[RubyConstant]
			public const int IPPROTO_GGP = 3;

			[RubyConstant]
			public const int IPPROTO_ICMP = 1;

			[RubyConstant]
			public const int IPPROTO_IDP = 22;

			[RubyConstant]
			public const int IPPROTO_IGMP = 2;

			[RubyConstant]
			public const int IPPROTO_IP = 0;

			[RubyConstant]
			public const int IPPROTO_MAX = 256;

			[RubyConstant]
			public const int IPPROTO_ND = 77;

			[RubyConstant]
			public const int IPPROTO_PUP = 12;

			[RubyConstant]
			public const int IPPROTO_RAW = 255;

			[RubyConstant]
			public const int IPPROTO_TCP = 6;

			[RubyConstant]
			public const int IPPROTO_UDP = 17;

			[RubyConstant]
			public const int IPPROTO_AH = 51;

			[RubyConstant]
			public const int IPPROTO_DSTOPTS = 60;

			[RubyConstant]
			public const int IPPROTO_ESP = 50;

			[RubyConstant]
			public const int IPPROTO_FRAGMENT = 44;

			[RubyConstant]
			public const int IPPROTO_HOPOPTS = 0;

			[RubyConstant]
			public const int IPPROTO_ICMPV6 = 58;

			[RubyConstant]
			public const int IPPROTO_IPV6 = 41;

			[RubyConstant]
			public const int IPPROTO_NONE = 59;

			[RubyConstant]
			public const int IPPROTO_ROUTING = 43;

			[RubyConstant]
			public const int IPV6_JOIN_GROUP = 12;

			[RubyConstant]
			public const int IPV6_LEAVE_GROUP = 13;

			[RubyConstant]
			public const int IPV6_MULTICAST_HOPS = 10;

			[RubyConstant]
			public const int IPV6_MULTICAST_IF = 9;

			[RubyConstant]
			public const int IPV6_MULTICAST_LOOP = 11;

			[RubyConstant]
			public const int IPV6_UNICAST_HOPS = 4;

			[RubyConstant]
			public const int IPV6_PKTINFO = 19;

			[RubyConstant]
			public const int MSG_DONTROUTE = 4;

			[RubyConstant]
			public const int MSG_OOB = 1;

			[RubyConstant]
			public const int MSG_PEEK = 2;

			[RubyConstant]
			public const int NI_DGRAM = 16;

			[RubyConstant]
			public const int NI_MAXHOST = 1025;

			[RubyConstant]
			public const int NI_MAXSERV = 32;

			[RubyConstant]
			public const int NI_NAMEREQD = 4;

			[RubyConstant]
			public const int NI_NOFQDN = 1;

			[RubyConstant]
			public const int NI_NUMERICHOST = 2;

			[RubyConstant]
			public const int NI_NUMERICSERV = 8;

			[RubyConstant]
			public const int PF_APPLETALK = 16;

			[RubyConstant]
			public const int PF_ATM = 22;

			[RubyConstant]
			public const int PF_CCITT = 10;

			[RubyConstant]
			public const int PF_CHAOS = 5;

			[RubyConstant]
			public const int PF_DATAKIT = 9;

			[RubyConstant]
			public const int PF_DLI = 13;

			[RubyConstant]
			public const int PF_ECMA = 8;

			[RubyConstant]
			public const int PF_HYLINK = 15;

			[RubyConstant]
			public const int PF_IMPLINK = 3;

			[RubyConstant]
			public const int PF_INET = 2;

			[RubyConstant]
			public const int PF_INET6 = 23;

			[RubyConstant]
			public const int PF_IPX = 6;

			[RubyConstant]
			public const int PF_ISO = 7;

			[RubyConstant]
			public const int PF_LAT = 14;

			[RubyConstant]
			public const int PF_MAX = 29;

			[RubyConstant]
			public const int PF_NS = 6;

			[RubyConstant]
			public const int PF_OSI = 7;

			[RubyConstant]
			public const int PF_PUP = 4;

			[RubyConstant]
			public const int PF_SNA = 11;

			[RubyConstant]
			public const int PF_UNIX = 1;

			[RubyConstant]
			public const int PF_UNSPEC = 0;

			[RubyConstant]
			public const int SHUT_RD = 0;

			[RubyConstant]
			public const int SHUT_RDWR = 2;

			[RubyConstant]
			public const int SHUT_WR = 1;

			[RubyConstant]
			public const int SOCK_DGRAM = 2;

			[RubyConstant]
			public const int SOCK_RAW = 3;

			[RubyConstant]
			public const int SOCK_RDM = 4;

			[RubyConstant]
			public const int SOCK_SEQPACKET = 5;

			[RubyConstant]
			public const int SOCK_STREAM = 1;

			[RubyConstant]
			public const int SO_ACCEPTCONN = 2;

			[RubyConstant]
			public const int SO_BROADCAST = 32;

			[RubyConstant]
			public const int SO_DEBUG = 1;

			[RubyConstant]
			public const int SO_DONTROUTE = 16;

			[RubyConstant]
			public const int SO_ERROR = 4103;

			[RubyConstant]
			public const int SO_KEEPALIVE = 8;

			[RubyConstant]
			public const int SO_LINGER = 128;

			[RubyConstant]
			public const int SO_OOBINLINE = 256;

			[RubyConstant]
			public const int SO_RCVBUF = 4098;

			[RubyConstant]
			public const int SO_RCVLOWAT = 4100;

			[RubyConstant]
			public const int SO_RCVTIMEO = 4102;

			[RubyConstant]
			public const int SO_REUSEADDR = 4;

			[RubyConstant]
			public const int SO_SNDBUF = 4097;

			[RubyConstant]
			public const int SO_SNDLOWAT = 4099;

			[RubyConstant]
			public const int SO_SNDTIMEO = 4101;

			[RubyConstant]
			public const int SO_TYPE = 4104;

			[RubyConstant]
			public const int SO_USELOOPBACK = 64;

			[RubyConstant]
			public const int SOL_SOCKET = 65535;

			[RubyConstant]
			public const int SOMAXCONN = int.MaxValue;

			[RubyConstant]
			public const int TCP_NODELAY = 1;
		}

		private static readonly MutableString _DefaultProtocol = MutableString.CreateAscii("tcp").Freeze();

		public RubySocket(RubyContext context, Socket socket)
			: base(context, socket)
		{
		}

		[RubyConstructor]
		public static RubySocket CreateSocket(ConversionStorage<MutableString> stringCast, ConversionStorage<int> fixnumCast, RubyClass self, [NotNull] object domain, [DefaultProtocol] int type, [DefaultProtocol] int protocol)
		{
			AddressFamily addressFamily = RubyBasicSocket.ConvertToAddressFamily(stringCast, fixnumCast, domain);
			return new RubySocket(self.Context, new Socket(addressFamily, (SocketType)type, (ProtocolType)protocol));
		}

		[RubyMethod("getaddrinfo", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray GetAddressInfo(ConversionStorage<MutableString> stringCast, ConversionStorage<int> fixnumCast, RubyClass self, object hostNameOrAddress, object port, object family, object socktype, object protocol, object flags)
		{
			RubyContext context = self.Context;
			IPHostEntry iPHostEntry = ((hostNameOrAddress != null) ? RubyBasicSocket.GetHostEntry(RubyBasicSocket.ConvertToHostString(stringCast, hostNameOrAddress), RubyBasicSocket.DoNotReverseLookup(context).Value) : RubyBasicSocket.MakeEntry(IPAddress.Any, RubyBasicSocket.DoNotReverseLookup(context).Value));
			int num = RubyBasicSocket.ConvertToPortNum(stringCast, fixnumCast, port);
			RubyBasicSocket.ConvertToAddressFamily(stringCast, fixnumCast, family);
			int num2 = Protocols.CastToFixnum(fixnumCast, socktype);
			int num3 = Protocols.CastToFixnum(fixnumCast, protocol);
			RubyArray rubyArray = new RubyArray(iPHostEntry.AddressList.Length);
			for (int i = 0; i < iPHostEntry.AddressList.Length; i++)
			{
				IPAddress iPAddress = iPHostEntry.AddressList[i];
				RubyArray rubyArray2 = new RubyArray(9);
				rubyArray2.Add(RubyBasicSocket.ToAddressFamilyString(iPAddress.AddressFamily));
				rubyArray2.Add(num);
				rubyArray2.Add(RubyBasicSocket.HostNameToMutableString(context, RubyBasicSocket.IPAddressToHostName(iPAddress, RubyBasicSocket.DoNotReverseLookup(context).Value)));
				rubyArray2.Add(MutableString.CreateAscii(iPAddress.ToString()));
				rubyArray2.Add((int)iPAddress.AddressFamily);
				rubyArray2.Add(num2);
				rubyArray2.Add(num3);
				rubyArray.Add(rubyArray2);
			}
			return rubyArray;
		}

		[RubyMethod("gethostbyaddr", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray GetHostByAddress(ConversionStorage<MutableString> stringCast, ConversionStorage<int> fixnumCast, RubyClass self, [NotNull][DefaultProtocol] MutableString address, object type)
		{
			RubyBasicSocket.ConvertToAddressFamily(stringCast, fixnumCast, type);
			IPHostEntry hostEntry = RubyBasicSocket.GetHostEntry(new IPAddress(address.ConvertToBytes()), RubyBasicSocket.DoNotReverseLookup(self.Context).Value);
			return RubyBasicSocket.CreateHostEntryArray(self.Context, hostEntry, true);
		}

		[RubyMethod("gethostbyname", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray GetHostByName(RubyClass self, int address)
		{
			return RubyBasicSocket.GetHostByName(self.Context, RubyBasicSocket.ConvertToHostString(address), true);
		}

		[RubyMethod("gethostbyname", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray GetHostByName(RubyClass self, [NotNull] BigInteger address)
		{
			return RubyBasicSocket.GetHostByName(self.Context, RubyBasicSocket.ConvertToHostString(address), true);
		}

		[RubyMethod("gethostbyname", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray GetHostByName(RubyClass self, [DefaultProtocol] MutableString name)
		{
			return RubyBasicSocket.GetHostByName(self.Context, RubyBasicSocket.ConvertToHostString(name), true);
		}

		[RubyMethod("gethostname", RubyMethodAttributes.PublicSingleton)]
		public static MutableString GetHostname(RubyClass self)
		{
			return RubyBasicSocket.HostNameToMutableString(self.Context, Dns.GetHostName());
		}

		[RubyMethod("getservbyname", RubyMethodAttributes.PublicSingleton)]
		public static int GetServiceByName(RubyClass self, [NotNull][DefaultProtocol] MutableString name, [Optional][DefaultProtocol] MutableString protocol)
		{
			if (protocol == null)
			{
				protocol = _DefaultProtocol;
			}
			ServiceName serviceName = RubyBasicSocket.SearchForService(name, protocol);
			if (serviceName != null)
			{
				return serviceName.Port;
			}
			try
			{
				return ParseInteger(self.Context, name.ConvertToString());
			}
			catch (InvalidOperationException)
			{
				throw SocketErrorOps.Create(MutableString.FormatMessage("no such service {0} {1}", name, protocol));
			}
		}

		[RubyMethod("socketpair", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("pair", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray CreateSocketPair(RubyClass self, object domain, object type, object protocol)
		{
			throw new NotImplementedError();
		}

		[RubyMethod("getnameinfo", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray GetNameInfo(ConversionStorage<MutableString> stringCast, ConversionStorage<int> fixnumCast, RubyClass self, [NotNull] RubyArray hostInfo, [Optional] object flags)
		{
			if (hostInfo.Count < 3 || hostInfo.Count > 4)
			{
				throw RubyExceptions.CreateArgumentError("First parameter must be a 3 or 4 element array");
			}
			RubyContext context = self.Context;
			AddressFamily addressFamily = RubyBasicSocket.ConvertToAddressFamily(stringCast, fixnumCast, hostInfo[0]);
			if (addressFamily != AddressFamily.InterNetwork)
			{
				throw new SocketException(10047);
			}
			int num = RubyBasicSocket.ConvertToPortNum(stringCast, fixnumCast, hostInfo[1]);
			ServiceName serviceName = RubyBasicSocket.SearchForService(num);
			object hostName = ((hostInfo.Count > 3 && hostInfo[3] != null) ? hostInfo[3] : hostInfo[2]);
			IPHostEntry hostEntry = RubyBasicSocket.GetHostEntry(RubyBasicSocket.ConvertToHostString(stringCast, hostName), false);
			RubyArray rubyArray = new RubyArray(2);
			rubyArray.Add(RubyBasicSocket.HostNameToMutableString(context, hostEntry.HostName));
			if (serviceName != null)
			{
				rubyArray.Add(MutableString.Create(serviceName.Name));
			}
			else
			{
				rubyArray.Add(num);
			}
			return rubyArray;
		}

		[RubyMethod("getnameinfo", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray GetNameInfo(RubyClass self, [DefaultProtocol][NotNull] MutableString address, [Optional] object flags)
		{
			IPEndPoint iPEndPoint = UnpackSockAddr(address);
			IPHostEntry hostEntry = RubyBasicSocket.GetHostEntry(iPEndPoint.Address, false);
			ServiceName serviceName = RubyBasicSocket.SearchForService(iPEndPoint.Port);
			RubyArray rubyArray = new RubyArray(2);
			rubyArray.Add(RubyBasicSocket.HostNameToMutableString(self.Context, hostEntry.HostName));
			if (serviceName != null)
			{
				rubyArray.Add(MutableString.Create(serviceName.Name));
			}
			else
			{
				rubyArray.Add(iPEndPoint.Port);
			}
			return rubyArray;
		}

		[RubyMethod("sockaddr_in", RubyMethodAttributes.PublicSingleton)]
		[RubyMethod("pack_sockaddr_in", RubyMethodAttributes.PublicSingleton)]
		public static MutableString PackInetSockAddr(ConversionStorage<MutableString> stringCast, ConversionStorage<int> fixnumCast, RubyClass self, object port, object hostNameOrAddress)
		{
			int port2 = RubyBasicSocket.ConvertToPortNum(stringCast, fixnumCast, port);
			IPAddress address = ((hostNameOrAddress != null) ? RubyBasicSocket.GetHostAddress(RubyBasicSocket.ConvertToHostString(stringCast, hostNameOrAddress)) : IPAddress.Loopback);
			SocketAddress socketAddress = new IPEndPoint(address, port2).Serialize();
			MutableString mutableString = MutableString.CreateBinary(socketAddress.Size);
			for (int i = 0; i < socketAddress.Size; i++)
			{
				mutableString.Append(socketAddress[i]);
			}
			return mutableString;
		}

		[RubyMethod("unpack_sockaddr_in", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray UnPackInetSockAddr(RubyClass self, [DefaultProtocol][NotNull] MutableString address)
		{
			IPEndPoint iPEndPoint = UnpackSockAddr(address);
			RubyArray rubyArray = new RubyArray(2);
			rubyArray.Add(iPEndPoint.Port);
			rubyArray.Add(MutableString.CreateAscii(iPEndPoint.Address.ToString()));
			return rubyArray;
		}

		internal static IPEndPoint UnpackSockAddr(MutableString stringAddress)
		{
			byte[] array = stringAddress.ConvertToBytes();
			SocketAddress socketAddress = new SocketAddress(AddressFamily.InterNetwork, array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				socketAddress[i] = array[i];
			}
			IPEndPoint iPEndPoint = new IPEndPoint(0L, 0);
			return (IPEndPoint)iPEndPoint.Create(socketAddress);
		}

		[RubyMethod("accept")]
		public static RubyArray Accept(RubyContext context, RubySocket self)
		{
			RubyArray rubyArray = new RubyArray(2);
			RubySocket rubySocket = new RubySocket(context, self.Socket.Accept());
			rubyArray.Add(rubySocket);
			SocketAddress socketAddress = rubySocket.Socket.RemoteEndPoint.Serialize();
			rubyArray.Add(MutableString.CreateAscii(socketAddress.ToString()));
			return rubyArray;
		}

		[RubyMethod("accept_nonblock")]
		public static RubyArray AcceptNonBlocking(RubyContext context, RubySocket self)
		{
			bool blocking = self.Socket.Blocking;
			try
			{
				self.Socket.Blocking = false;
				return Accept(context, self);
			}
			finally
			{
				self.Socket.Blocking = blocking;
			}
		}

		[RubyMethod("bind")]
		public static int Bind(RubyContext context, RubySocket self, MutableString sockaddr)
		{
			IPEndPoint localEP = UnpackSockAddr(sockaddr);
			self.Socket.Bind(localEP);
			return 0;
		}

		[RubyMethod("connect")]
		public static int Connect(RubyContext context, RubySocket self, MutableString sockaddr)
		{
			IPEndPoint remoteEP = UnpackSockAddr(sockaddr);
			self.Socket.Connect(remoteEP);
			return 0;
		}

		[RubyMethod("connect_nonblock")]
		public static int ConnectNonBlocking(RubyContext context, RubySocket self, MutableString sockaddr)
		{
			bool blocking = self.Socket.Blocking;
			try
			{
				self.Socket.Blocking = false;
				return Connect(context, self, sockaddr);
			}
			finally
			{
				self.Socket.Blocking = blocking;
			}
		}

		[RubyMethod("listen")]
		public static int Listen(RubyContext context, RubySocket self, int backlog)
		{
			self.Socket.Listen(backlog);
			return 0;
		}

		[RubyMethod("recvfrom")]
		public static RubyArray ReceiveFrom(ConversionStorage<int> fixnumCast, RubySocket self, int length)
		{
			return ReceiveFrom(fixnumCast, self, length, null);
		}

		[RubyMethod("recvfrom")]
		public static RubyArray ReceiveFrom(ConversionStorage<int> fixnumCast, RubySocket self, int length, object flags)
		{
			SocketFlags socketFlags = RubyBasicSocket.ConvertToSocketFlag(fixnumCast, flags);
			byte[] array = new byte[length];
			EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
			int count = self.Socket.ReceiveFrom(array, socketFlags, ref remoteEP);
			MutableString mutableString = MutableString.CreateBinary();
			mutableString.Append(array, 0, count);
			mutableString.IsTainted = true;
			return RubyOps.MakeArray2(mutableString, self.GetAddressArray(remoteEP));
		}

		[RubyMethod("sysaccept")]
		public static RubyArray SysAccept(RubyContext context, RubySocket self)
		{
			RubyArray rubyArray = new RubyArray(2);
			RubySocket rubySocket = new RubySocket(context, self.Socket.Accept());
			rubyArray.Add(rubySocket.GetFileDescriptor());
			SocketAddress socketAddress = rubySocket.Socket.RemoteEndPoint.Serialize();
			rubyArray.Add(MutableString.CreateAscii(socketAddress.ToString()));
			return rubyArray;
		}

		private static int ParseInteger(RubyContext context, string str)
		{
			bool flag = false;
			if (str[0] == '-')
			{
				flag = true;
				str = str.Remove(0, 1);
			}
			Tokenizer tokenizer = new Tokenizer();
			tokenizer.Initialize(new StringReader(str));
			Tokens nextToken = tokenizer.GetNextToken();
			TokenValue tokenValue = tokenizer.TokenValue;
			Tokens nextToken2 = tokenizer.GetNextToken();
			if (nextToken == Tokens.Integer && nextToken2 == Tokens.Integer)
			{
				if (!flag)
				{
					return tokenValue.Integer1;
				}
				return -tokenValue.Integer1;
			}
			throw RubyExceptions.CreateTypeConversionError("String", "Integer");
		}
	}
}
