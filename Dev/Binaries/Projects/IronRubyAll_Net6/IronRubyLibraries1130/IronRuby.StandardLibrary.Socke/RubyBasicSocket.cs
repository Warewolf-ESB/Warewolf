using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.StandardLibrary.Sockets
{
	[RubyClass("BasicSocket", BuildConfig = "!SILVERLIGHT")]
	public abstract class RubyBasicSocket : RubyIO
	{
		private struct AddressFamilyName
		{
			public readonly MutableString Name;

			public readonly AddressFamily Family;

			public AddressFamilyName(string name, AddressFamily family)
			{
				Name = MutableString.CreateAscii(name);
				Family = family;
			}
		}

		internal sealed class ServiceName
		{
			public readonly int Port;

			public readonly MutableString Protocol;

			public readonly MutableString Name;

			public ServiceName(int port, string protocol, string name)
			{
				Port = port;
				Protocol = MutableString.CreateAscii(protocol);
				Name = MutableString.CreateAscii(name);
			}
		}

		private static readonly MutableString BROADCAST_STRING = MutableString.CreateAscii("<broadcast>").Freeze();

		private Socket _socket;

		private bool _doNotReverseLookup;

		private static readonly object BasicSocketClassKey = new object();

		private static AddressFamilyName[] FamilyNames = new AddressFamilyName[23]
		{
			new AddressFamilyName("AF_INET", AddressFamily.InterNetwork),
			new AddressFamilyName("AF_UNIX", AddressFamily.Unix),
			new AddressFamilyName("AF_IPX", AddressFamily.NS),
			new AddressFamilyName("AF_APPLETALK", AddressFamily.AppleTalk),
			new AddressFamilyName("AF_UNSPEC", AddressFamily.Unspecified),
			new AddressFamilyName("AF_INET6", AddressFamily.InterNetworkV6),
			new AddressFamilyName("AF_IMPLINK", AddressFamily.ImpLink),
			new AddressFamilyName("AF_PUP", AddressFamily.Pup),
			new AddressFamilyName("AF_CHAOS", AddressFamily.Chaos),
			new AddressFamilyName("AF_NS", AddressFamily.NS),
			new AddressFamilyName("AF_ISO", AddressFamily.Iso),
			new AddressFamilyName("AF_OSI", AddressFamily.Iso),
			new AddressFamilyName("AF_ECMA", AddressFamily.Ecma),
			new AddressFamilyName("AF_DATAKIT", AddressFamily.DataKit),
			new AddressFamilyName("AF_CCITT", AddressFamily.Ccitt),
			new AddressFamilyName("AF_SNA", AddressFamily.Sna),
			new AddressFamilyName("AF_DEC", AddressFamily.DecNet),
			new AddressFamilyName("AF_DLI", AddressFamily.DataLink),
			new AddressFamilyName("AF_LAT", AddressFamily.Lat),
			new AddressFamilyName("AF_HYLINK", AddressFamily.HyperChannel),
			new AddressFamilyName("AF_NETBIOS", AddressFamily.NetBios),
			new AddressFamilyName("AF_ATM", AddressFamily.Atm),
			new AddressFamilyName("AF_MAX", AddressFamily.Max)
		};

		private static ServiceName[] ServiceNames = new ServiceName[143]
		{
			new ServiceName(7, "tcp", "echo"),
			new ServiceName(7, "udp", "echo"),
			new ServiceName(9, "tcp", "discard"),
			new ServiceName(9, "udp", "discard"),
			new ServiceName(11, "tcp", "systat"),
			new ServiceName(11, "udp", "systat"),
			new ServiceName(13, "tcp", "daytime"),
			new ServiceName(13, "udp", "daytime"),
			new ServiceName(15, "tcp", "netstat"),
			new ServiceName(17, "tcp", "qotd"),
			new ServiceName(17, "udp", "qotd"),
			new ServiceName(19, "tcp", "chargen"),
			new ServiceName(19, "udp", "chargen"),
			new ServiceName(20, "tcp", "ftp-data"),
			new ServiceName(21, "tcp", "ftp"),
			new ServiceName(23, "tcp", "telnet"),
			new ServiceName(25, "tcp", "smtp"),
			new ServiceName(37, "tcp", "time"),
			new ServiceName(37, "udp", "time"),
			new ServiceName(39, "udp", "rlp"),
			new ServiceName(42, "tcp", "name"),
			new ServiceName(42, "udp", "name"),
			new ServiceName(43, "tcp", "whois"),
			new ServiceName(53, "tcp", "domain"),
			new ServiceName(53, "udp", "domain"),
			new ServiceName(53, "tcp", "nameserver"),
			new ServiceName(53, "udp", "nameserver"),
			new ServiceName(57, "tcp", "mtp"),
			new ServiceName(67, "udp", "bootp"),
			new ServiceName(69, "udp", "tftp"),
			new ServiceName(77, "tcp", "rje"),
			new ServiceName(79, "tcp", "finger"),
			new ServiceName(80, "tcp", "http"),
			new ServiceName(87, "tcp", "link"),
			new ServiceName(95, "tcp", "supdup"),
			new ServiceName(101, "tcp", "hostnames"),
			new ServiceName(102, "tcp", "iso-tsap"),
			new ServiceName(103, "tcp", "dictionary"),
			new ServiceName(103, "tcp", "x400"),
			new ServiceName(104, "tcp", "x400-snd"),
			new ServiceName(105, "tcp", "csnet-ns"),
			new ServiceName(109, "tcp", "pop"),
			new ServiceName(109, "tcp", "pop2"),
			new ServiceName(110, "tcp", "pop3"),
			new ServiceName(111, "tcp", "portmap"),
			new ServiceName(111, "udp", "portmap"),
			new ServiceName(111, "tcp", "sunrpc"),
			new ServiceName(111, "udp", "sunrpc"),
			new ServiceName(113, "tcp", "auth"),
			new ServiceName(115, "tcp", "sftp"),
			new ServiceName(117, "tcp", "path"),
			new ServiceName(117, "tcp", "uucp-path"),
			new ServiceName(119, "tcp", "nntp"),
			new ServiceName(123, "udp", "ntp"),
			new ServiceName(137, "udp", "nbname"),
			new ServiceName(138, "udp", "nbdatagram"),
			new ServiceName(139, "tcp", "nbsession"),
			new ServiceName(144, "tcp", "NeWS"),
			new ServiceName(153, "tcp", "sgmp"),
			new ServiceName(158, "tcp", "tcprepo"),
			new ServiceName(161, "tcp", "snmp"),
			new ServiceName(162, "tcp", "snmp-trap"),
			new ServiceName(170, "tcp", "print-srv"),
			new ServiceName(175, "tcp", "vmnet"),
			new ServiceName(315, "udp", "load"),
			new ServiceName(400, "tcp", "vmnet0"),
			new ServiceName(500, "udp", "sytek"),
			new ServiceName(512, "udp", "biff"),
			new ServiceName(512, "tcp", "exec"),
			new ServiceName(513, "tcp", "login"),
			new ServiceName(513, "udp", "who"),
			new ServiceName(514, "tcp", "shell"),
			new ServiceName(514, "udp", "syslog"),
			new ServiceName(515, "tcp", "printer"),
			new ServiceName(517, "udp", "talk"),
			new ServiceName(518, "udp", "ntalk"),
			new ServiceName(520, "tcp", "efs"),
			new ServiceName(520, "udp", "route"),
			new ServiceName(525, "udp", "timed"),
			new ServiceName(526, "tcp", "tempo"),
			new ServiceName(530, "tcp", "courier"),
			new ServiceName(531, "tcp", "conference"),
			new ServiceName(531, "udp", "rvd-control"),
			new ServiceName(532, "tcp", "netnews"),
			new ServiceName(533, "udp", "netwall"),
			new ServiceName(540, "tcp", "uucp"),
			new ServiceName(543, "tcp", "klogin"),
			new ServiceName(544, "tcp", "kshell"),
			new ServiceName(550, "udp", "new-rwho"),
			new ServiceName(556, "tcp", "remotefs"),
			new ServiceName(560, "udp", "rmonitor"),
			new ServiceName(561, "udp", "monitor"),
			new ServiceName(600, "tcp", "garcon"),
			new ServiceName(601, "tcp", "maitrd"),
			new ServiceName(602, "tcp", "busboy"),
			new ServiceName(700, "udp", "acctmaster"),
			new ServiceName(701, "udp", "acctslave"),
			new ServiceName(702, "udp", "acct"),
			new ServiceName(703, "udp", "acctlogin"),
			new ServiceName(704, "udp", "acctprinter"),
			new ServiceName(704, "udp", "elcsd"),
			new ServiceName(705, "udp", "acctinfo"),
			new ServiceName(706, "udp", "acctslave2"),
			new ServiceName(707, "udp", "acctdisk"),
			new ServiceName(750, "tcp", "kerberos"),
			new ServiceName(750, "udp", "kerberos"),
			new ServiceName(751, "tcp", "kerberos_master"),
			new ServiceName(751, "udp", "kerberos_master"),
			new ServiceName(752, "udp", "passwd_server"),
			new ServiceName(753, "udp", "userreg_server"),
			new ServiceName(754, "tcp", "krb_prop"),
			new ServiceName(888, "tcp", "erlogin"),
			new ServiceName(1109, "tcp", "kpop"),
			new ServiceName(1167, "udp", "phone"),
			new ServiceName(1524, "tcp", "ingreslock"),
			new ServiceName(1666, "udp", "maze"),
			new ServiceName(2049, "udp", "nfs"),
			new ServiceName(2053, "tcp", "knetd"),
			new ServiceName(2105, "tcp", "eklogin"),
			new ServiceName(5555, "tcp", "rmt"),
			new ServiceName(5556, "tcp", "mtb"),
			new ServiceName(9535, "tcp", "man"),
			new ServiceName(9536, "tcp", "w"),
			new ServiceName(9537, "tcp", "mantst"),
			new ServiceName(10000, "tcp", "bnews"),
			new ServiceName(10000, "udp", "rscs0"),
			new ServiceName(10001, "tcp", "queue"),
			new ServiceName(10001, "udp", "rscs1"),
			new ServiceName(10002, "tcp", "poker"),
			new ServiceName(10002, "udp", "rscs2"),
			new ServiceName(10003, "tcp", "gateway"),
			new ServiceName(10003, "udp", "rscs3"),
			new ServiceName(10004, "tcp", "remp"),
			new ServiceName(10004, "udp", "rscs4"),
			new ServiceName(10005, "udp", "rscs5"),
			new ServiceName(10006, "udp", "rscs6"),
			new ServiceName(10007, "udp", "rscs7"),
			new ServiceName(10008, "udp", "rscs8"),
			new ServiceName(10009, "udp", "rscs9"),
			new ServiceName(10010, "udp", "rscsa"),
			new ServiceName(10011, "udp", "rscsb"),
			new ServiceName(10012, "tcp", "qmaster"),
			new ServiceName(10012, "udp", "qmaster")
		};

		protected internal Socket Socket
		{
			get
			{
				if (_socket == null)
				{
					throw RubyExceptions.CreateIOError("uninitialized stream");
				}
				return _socket;
			}
			set
			{
				ContractUtils.RequiresNotNull(value, "value");
				Reset(new SocketStream(value), IOMode.ReadWrite | IOMode.PreserveEndOfLines);
				_socket = value;
			}
		}

		internal static StrongBox<bool> DoNotReverseLookup(RubyContext context)
		{
			return (StrongBox<bool>)context.GetOrCreateLibraryData(BasicSocketClassKey, () => new StrongBox<bool>(false));
		}

		protected RubyBasicSocket(RubyContext context)
			: base(context)
		{
			base.Mode = IOMode.ReadWrite | IOMode.PreserveEndOfLines;
			base.ExternalEncoding = RubyEncoding.Binary;
			base.InternalEncoding = null;
		}

		protected RubyBasicSocket(RubyContext context, Socket socket)
			: base(context, new SocketStream(socket), IOMode.ReadWrite | IOMode.PreserveEndOfLines)
		{
			_socket = socket;
			base.ExternalEncoding = RubyEncoding.Binary;
			base.InternalEncoding = null;
		}

		public override int SetReadTimeout(int timeout)
		{
			int receiveTimeout = _socket.ReceiveTimeout;
			_socket.ReceiveTimeout = timeout;
			return receiveTimeout;
		}

		public override void NonBlockingOperation(Action operation, bool isRead)
		{
			bool blocking = _socket.Blocking;
			try
			{
				_socket.Blocking = false;
				operation();
			}
			catch (SocketException ex)
			{
				if (ex.SocketErrorCode == SocketError.WouldBlock)
				{
					throw RubyIOOps.NonBlockingError(base.Context, new Errno.WouldBlockError(), isRead);
				}
				throw;
			}
			finally
			{
				_socket.Blocking = blocking;
			}
		}

		public override WaitHandle CreateReadWaitHandle()
		{
			return Socket.BeginReceive(Utils.EmptyBytes, 0, 0, SocketFlags.Peek, null, null).AsyncWaitHandle;
		}

		public override WaitHandle CreateWriteWaitHandle()
		{
			return Socket.BeginSend(Utils.EmptyBytes, 0, 0, SocketFlags.Peek, null, null).AsyncWaitHandle;
		}

		public override WaitHandle CreateErrorWaitHandle()
		{
			throw new NotSupportedException();
		}

		private int SetFileControlFlags(int flags)
		{
			Socket.Blocking = (flags & RubyFileOps.Constants.NONBLOCK) != 0;
			return 0;
		}

		public override int FileControl(int commandId, int arg)
		{
			if (commandId == 1)
			{
				return SetFileControlFlags(arg);
			}
			throw new NotSupportedException();
		}

		public override int FileControl(int commandId, byte[] arg)
		{
			throw new NotSupportedException();
		}

		[RubyMethod("do_not_reverse_lookup", RubyMethodAttributes.PublicSingleton)]
		public static bool GetDoNotReverseLookup(RubyClass self)
		{
			return DoNotReverseLookup(self.Context).Value;
		}

		[RubyMethod("do_not_reverse_lookup=", RubyMethodAttributes.PublicSingleton)]
		public static void SetDoNotReverseLookup(RubyClass self, bool value)
		{
			Protocols.CheckSafeLevel(self.Context, 4);
			DoNotReverseLookup(self.Context).Value = value;
		}

		[RubyMethod("do_not_reverse_lookup")]
		public static bool GetDoNotReverseLookup(RubyBasicSocket self)
		{
			return self._doNotReverseLookup;
		}

		[RubyMethod("do_not_reverse_lookup=")]
		public static void SetDoNotReverseLookup(RubyBasicSocket self, bool value)
		{
			Protocols.CheckSafeLevel(self.Context, 4);
			self._doNotReverseLookup = value;
		}

		[RubyMethod("for_fd", RubyMethodAttributes.PublicSingleton)]
		public static RuleGenerator ForFileDescriptor()
		{
			return RuleGenerators.InstanceConstructor;
		}

		[RubyMethod("close_read")]
		public static void CloseRead(RubyContext context, RubyBasicSocket self)
		{
			CheckSecurity(context, self, "can't close socket");
			self.Socket.Shutdown(SocketShutdown.Receive);
		}

		[RubyMethod("close_write")]
		public static void CloseWrite(RubyContext context, RubyBasicSocket self)
		{
			CheckSecurity(context, self, "can't close socket");
			self.Socket.Shutdown(SocketShutdown.Send);
		}

		[RubyMethod("shutdown")]
		public static int Shutdown(RubyContext context, RubyBasicSocket self, [DefaultProtocol] int how)
		{
			CheckSecurity(context, self, "can't shutdown socket");
			if (how < 0 || 2 < how)
			{
				throw RubyExceptions.CreateArgumentError("`how' should be either 0, 1, 2");
			}
			self.Socket.Close();
			return 0;
		}

		[RubyMethod("setsockopt")]
		public static void SetSocketOption(ConversionStorage<int> conversionStorage, RubyContext context, RubyBasicSocket self, [DefaultProtocol] int level, [DefaultProtocol] int optname, int value)
		{
			Protocols.CheckSafeLevel(context, 2, "setsockopt");
			self.Socket.SetSocketOption((SocketOptionLevel)level, (SocketOptionName)optname, value);
		}

		[RubyMethod("setsockopt")]
		public static void SetSocketOption(ConversionStorage<int> conversionStorage, RubyContext context, RubyBasicSocket self, [DefaultProtocol] int level, [DefaultProtocol] int optname, bool value)
		{
			Protocols.CheckSafeLevel(context, 2, "setsockopt");
			self.Socket.SetSocketOption((SocketOptionLevel)level, (SocketOptionName)optname, value);
		}

		[RubyMethod("setsockopt")]
		public static void SetSocketOption(RubyContext context, RubyBasicSocket self, [DefaultProtocol] int level, [DefaultProtocol] int optname, [NotNull][DefaultProtocol] MutableString value)
		{
			Protocols.CheckSafeLevel(context, 2, "setsockopt");
			self.Socket.SetSocketOption((SocketOptionLevel)level, (SocketOptionName)optname, value.ConvertToBytes());
		}

		[RubyMethod("getsockopt")]
		public static MutableString GetSocketOption(ConversionStorage<int> conversionStorage, RubyContext context, RubyBasicSocket self, [DefaultProtocol] int level, [DefaultProtocol] int optname)
		{
			Protocols.CheckSafeLevel(context, 2, "getsockopt");
			byte[] socketOption = self.Socket.GetSocketOption((SocketOptionLevel)level, (SocketOptionName)optname, 4);
			return MutableString.CreateBinary(socketOption);
		}

		[RubyMethod("getsockname")]
		public static MutableString GetSocketName(RubyBasicSocket self)
		{
			SocketAddress socketAddress = self.Socket.LocalEndPoint.Serialize();
			byte[] array = new byte[socketAddress.Size];
			for (int i = 0; i < socketAddress.Size; i++)
			{
				array[i] = socketAddress[i];
			}
			return MutableString.CreateBinary(array);
		}

		[RubyMethod("getpeername")]
		public static MutableString GetPeerName(RubyBasicSocket self)
		{
			SocketAddress socketAddress = self.Socket.RemoteEndPoint.Serialize();
			byte[] array = new byte[socketAddress.Size];
			for (int i = 0; i < socketAddress.Size; i++)
			{
				array[i] = socketAddress[i];
			}
			return MutableString.CreateBinary(array);
		}

		[RubyMethod("send")]
		public static int Send(ConversionStorage<int> fixnumCast, RubyBasicSocket self, [NotNull][DefaultProtocol] MutableString message, object flags)
		{
			Protocols.CheckSafeLevel(fixnumCast.Context, 4, "send");
			SocketFlags socketFlags = ConvertToSocketFlag(fixnumCast, flags);
			return self.Socket.Send(message.ConvertToBytes(), socketFlags);
		}

		[RubyMethod("send")]
		public static int Send(ConversionStorage<int> fixnumCast, RubyBasicSocket self, [NotNull][DefaultProtocol] MutableString message, object flags, [NotNull][DefaultProtocol] MutableString to)
		{
			Protocols.CheckSafeLevel(fixnumCast.Context, 4, "send");
			SocketFlags socketFlags = ConvertToSocketFlag(fixnumCast, flags);
			SocketAddress socketAddress = new SocketAddress(AddressFamily.InterNetwork);
			for (int i = 0; i < to.GetByteCount(); i++)
			{
				socketAddress[i] = to.GetByte(i);
			}
			EndPoint remoteEP = self.Socket.LocalEndPoint.Create(socketAddress);
			return self.Socket.SendTo(message.ConvertToBytes(), socketFlags, remoteEP);
		}

		[RubyMethod("recv")]
		public static MutableString Receive(ConversionStorage<int> fixnumCast, RubyBasicSocket self, [DefaultProtocol] int length, object flags)
		{
			SocketFlags socketFlags = ConvertToSocketFlag(fixnumCast, flags);
			byte[] array = new byte[length];
			int num = self.Socket.Receive(array, 0, length, socketFlags);
			MutableString mutableString = MutableString.CreateBinary(num);
			mutableString.Append(array, 0, num);
			mutableString.IsTainted = true;
			return mutableString;
		}

		[RubyMethod("recv_nonblock")]
		public static MutableString ReceiveNonBlocking(ConversionStorage<int> fixnumCast, RubyBasicSocket self, [DefaultProtocol] int length, object flags)
		{
			bool blocking = self.Socket.Blocking;
			try
			{
				self.Socket.Blocking = false;
				return Receive(fixnumCast, self, length, flags);
			}
			finally
			{
				self.Socket.Blocking = blocking;
			}
		}

		internal static void CheckSecurity(RubyContext context, object self, string message)
		{
			if (context.CurrentSafeLevel >= 4 && context.IsObjectTainted(self))
			{
				throw RubyExceptions.CreateSecurityError("Insecure: " + message);
			}
		}

		internal static IPHostEntry GetHostEntry(IPAddress address, bool doNotReverseLookup)
		{
			if (address.Equals(IPAddress.Any) || address.Equals(IPAddress.Loopback))
			{
				return MakeEntry(address, doNotReverseLookup);
			}
			return Dns.GetHostEntry(address);
		}

		internal static IPHostEntry GetHostEntry(string hostNameOrAddress, bool doNotReverseLookup)
		{
			if (hostNameOrAddress == IPAddress.Any.ToString())
			{
				return MakeEntry(IPAddress.Any, doNotReverseLookup);
			}
			if (hostNameOrAddress == IPAddress.Loopback.ToString())
			{
				return MakeEntry(IPAddress.Loopback, doNotReverseLookup);
			}
			return Dns.GetHostEntry(hostNameOrAddress);
		}

		internal static IPAddress GetHostAddress(string hostNameOrAddress)
		{
			IPAddress address;
			if (IPAddress.TryParse(hostNameOrAddress, out address))
			{
				return address;
			}
			IPAddress[] hostAddresses = Dns.GetHostAddresses(hostNameOrAddress);
			IPAddress[] array = hostAddresses;
			foreach (IPAddress iPAddress in array)
			{
				if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					return iPAddress;
				}
			}
			return hostAddresses[0];
		}

		internal static IPHostEntry MakeEntry(IPAddress address, bool doNotReverseLookup)
		{
			string text = IPAddressToHostName(address, doNotReverseLookup);
			IPHostEntry iPHostEntry = new IPHostEntry();
			iPHostEntry.AddressList = new IPAddress[1] { address };
			iPHostEntry.Aliases = new string[1] { text };
			iPHostEntry.HostName = text;
			return iPHostEntry;
		}

		internal static RubyArray GetHostByName(RubyContext context, string hostNameOrAddress, bool packIpAddresses)
		{
			return CreateHostEntryArray(context, GetHostEntry(hostNameOrAddress, DoNotReverseLookup(context).Value), packIpAddresses);
		}

		internal RubyArray GetAddressArray(EndPoint endPoint)
		{
			return GetAddressArray(base.Context, endPoint, _doNotReverseLookup);
		}

		internal static RubyArray GetAddressArray(RubyContext context, EndPoint endPoint)
		{
			return GetAddressArray(context, endPoint, DoNotReverseLookup(context).Value);
		}

		internal static RubyArray GetAddressArray(RubyContext context, EndPoint endPoint, bool doNotReverseLookup)
		{
			RubyArray rubyArray = new RubyArray(4);
			IPEndPoint iPEndPoint = (IPEndPoint)endPoint;
			rubyArray.Add(MutableString.CreateAscii(AddressFamilyToString(iPEndPoint.AddressFamily)));
			rubyArray.Add(iPEndPoint.Port);
			rubyArray.Add(HostNameToMutableString(context, IPAddressToHostName(iPEndPoint.Address, doNotReverseLookup)));
			rubyArray.Add(MutableString.CreateAscii(iPEndPoint.Address.ToString()));
			return rubyArray;
		}

		internal static MutableString HostNameToMutableString(RubyContext context, string str)
		{
			if (str.IsAscii())
			{
				return MutableString.CreateAscii(str);
			}
			return MutableString.Create(str, context.GetPathEncoding());
		}

		internal static string IPAddressToHostName(IPAddress address, bool doNotReverseLookup)
		{
			if (address.Equals(IPAddress.Any) || doNotReverseLookup)
			{
				return address.ToString();
			}
			return Dns.GetHostEntry(address).HostName;
		}

		private static string AddressFamilyToString(AddressFamily af)
		{
			switch (af)
			{
			case AddressFamily.InterNetwork:
				return "AF_INET";
			case AddressFamily.DataLink:
				return "AF_DLI";
			case AddressFamily.HyperChannel:
				return "AF_HYLINK";
			case AddressFamily.Banyan:
				return "AF_BAN";
			case AddressFamily.InterNetworkV6:
				return "AF_INET6";
			case AddressFamily.Ieee12844:
				return "AF_12844";
			case AddressFamily.NetworkDesigners:
				return "AF_NETDES";
			default:
			{
				string name = Enum.GetName(typeof(AddressFamily), af);
				if (name == null)
				{
					int num = (int)af;
					return "unknown:" + num.ToString(CultureInfo.InvariantCulture);
				}
				return "AF_" + name.ToUpperInvariant();
			}
			}
		}

		internal static SocketFlags ConvertToSocketFlag(ConversionStorage<int> conversionStorage, object flags)
		{
			if (flags == null)
			{
				return SocketFlags.None;
			}
			return (SocketFlags)Protocols.CastToFixnum(conversionStorage, flags);
		}

		internal static AddressFamily ConvertToAddressFamily(ConversionStorage<MutableString> stringCast, ConversionStorage<int> fixnumCast, object family)
		{
			if (family == null)
			{
				return AddressFamily.InterNetwork;
			}
			if (family is int)
			{
				return (AddressFamily)(int)family;
			}
			MutableString mutableString = Protocols.CastToString(stringCast, family);
			AddressFamilyName[] familyNames = FamilyNames;
			for (int i = 0; i < familyNames.Length; i++)
			{
				AddressFamilyName addressFamilyName = familyNames[i];
				if (addressFamilyName.Name.Equals(mutableString))
				{
					return addressFamilyName.Family;
				}
			}
			return (AddressFamily)Protocols.CastToFixnum(fixnumCast, mutableString);
		}

		internal static MutableString ToAddressFamilyString(AddressFamily family)
		{
			AddressFamilyName[] familyNames = FamilyNames;
			for (int i = 0; i < familyNames.Length; i++)
			{
				AddressFamilyName addressFamilyName = familyNames[i];
				if (addressFamilyName.Family == family)
				{
					return addressFamilyName.Name;
				}
			}
			throw new SocketException(10047);
		}

		internal static string ConvertToHostString(uint address)
		{
			byte[] array = new byte[4];
			for (int num = array.Length - 1; num >= 0; num--)
			{
				array[num] = (byte)(address & 0xFFu);
				address >>= 8;
			}
			return new IPAddress(array).ToString();
		}

		internal static string ConvertToHostString(BigInteger address)
		{
			ulong ret;
			if (address.AsUInt64(out ret))
			{
				if (ret <= uint.MaxValue)
				{
					return ConvertToHostString((uint)ret);
				}
				byte[] array = new byte[8];
				for (int num = array.Length - 1; num >= 0; num--)
				{
					array[num] = (byte)(ret & 0xFF);
					ret >>= 8;
				}
				return new IPAddress(array).ToString();
			}
			throw RubyExceptions.CreateRangeError("bignum too big to convert into `quad long'");
		}

		internal static string ConvertToHostString(MutableString hostName)
		{
			if (hostName == null)
			{
				throw new SocketException(11001);
			}
			if (hostName.IsEmpty)
			{
				return IPAddress.Any.ToString();
			}
			if (hostName.Equals(BROADCAST_STRING))
			{
				return IPAddress.Broadcast.ToString();
			}
			return hostName.ConvertToString();
		}

		internal static string ConvertToHostString(ConversionStorage<MutableString> stringCast, object hostName)
		{
			if (hostName is int)
			{
				return ConvertToHostString((int)hostName);
			}
			BigInteger address;
			if (!object.ReferenceEquals(address = hostName as BigInteger, null))
			{
				return ConvertToHostString(address);
			}
			if (hostName != null)
			{
				return ConvertToHostString(Protocols.CastToString(stringCast, hostName));
			}
			return ConvertToHostString((MutableString)null);
		}

		internal static bool IntegerAsFixnum(object value, out int result)
		{
			if (value is int)
			{
				result = (int)value;
				return true;
			}
			BigInteger bigInteger = value as BigInteger;
			if ((object)bigInteger != null)
			{
				if (!bigInteger.AsInt32(out result))
				{
					throw RubyExceptions.CreateRangeError("bignum too big to convert into `long'");
				}
				return true;
			}
			result = 0;
			return false;
		}

		internal static int ConvertToPortNum(ConversionStorage<MutableString> stringCast, ConversionStorage<int> fixnumCast, object port)
		{
			if (port is int)
			{
				return (int)port;
			}
			if (port == null)
			{
				return 0;
			}
			MutableString mutableString = Protocols.CastToString(stringCast, port);
			ServiceName serviceName = SearchForService(mutableString);
			if (serviceName != null)
			{
				return serviceName.Port;
			}
			int result = 0;
			if (int.TryParse(mutableString.ToString(), out result))
			{
				return result;
			}
			throw SocketErrorOps.Create(MutableString.FormatMessage("Invalid port number or service name: `{0}'.", mutableString));
		}

		internal static ServiceName SearchForService(int port)
		{
			ServiceName[] serviceNames = ServiceNames;
			foreach (ServiceName serviceName in serviceNames)
			{
				if (serviceName.Port == port)
				{
					return serviceName;
				}
			}
			return null;
		}

		internal static ServiceName SearchForService(MutableString serviceName)
		{
			ServiceName[] serviceNames = ServiceNames;
			foreach (ServiceName serviceName2 in serviceNames)
			{
				if (serviceName2.Name.Equals(serviceName))
				{
					return serviceName2;
				}
			}
			return null;
		}

		internal static ServiceName SearchForService(MutableString serviceName, MutableString protocol)
		{
			ServiceName[] serviceNames = ServiceNames;
			foreach (ServiceName serviceName2 in serviceNames)
			{
				if (serviceName2.Name.Equals(serviceName) && serviceName2.Protocol.Equals(protocol))
				{
					return serviceName2;
				}
			}
			return null;
		}

		internal static RubyArray CreateHostEntryArray(RubyContext context, IPHostEntry hostEntry, bool packIpAddresses)
		{
			RubyArray rubyArray = new RubyArray(4);
			rubyArray.Add(HostNameToMutableString(context, hostEntry.HostName));
			RubyArray rubyArray2 = new RubyArray(hostEntry.Aliases.Length);
			string[] aliases = hostEntry.Aliases;
			foreach (string str in aliases)
			{
				rubyArray2.Add(HostNameToMutableString(context, str));
			}
			rubyArray.Add(rubyArray2);
			IPAddress[] addressList = hostEntry.AddressList;
			foreach (IPAddress iPAddress in addressList)
			{
				if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					rubyArray.Add((int)iPAddress.AddressFamily);
					if (packIpAddresses)
					{
						byte[] addressBytes = iPAddress.GetAddressBytes();
						MutableString mutableString = MutableString.CreateBinary();
						mutableString.Append(addressBytes, 0, addressBytes.Length);
						rubyArray.Add(mutableString);
					}
					else
					{
						rubyArray.Add(MutableString.CreateAscii(iPAddress.ToString()));
					}
					break;
				}
			}
			return rubyArray;
		}
	}
}
