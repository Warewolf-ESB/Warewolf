using System;
using System.Net.Sockets;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Math;

namespace IronRuby.StandardLibrary.Sockets
{
	public sealed class SocketsLibraryInitializer : LibraryInitializer
	{
		protected override void LoadModules()
		{
			RubyClass @class = GetClass(typeof(RubyIO));
			RubyClass class2 = GetClass(typeof(SystemException));
			RubyClass super = DefineGlobalClass("BasicSocket", typeof(RubyBasicSocket), 8, @class, LoadBasicSocket_Instance, LoadBasicSocket_Class, null, RubyModule.EmptyArray);
			RubyModule value = DefineModule("Socket::Constants", typeof(RubySocket.SocketConstants), 8, null, null, LoadSocket__Constants_Constants, RubyModule.EmptyArray);
			DefineGlobalClass("SocketError", typeof(SocketException), 0, class2, LoadSocketError_Instance, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(SocketErrorOps.Create));
			RubyClass super2 = DefineGlobalClass("IPSocket", typeof(IPSocket), 8, super, LoadIPSocket_Instance, LoadIPSocket_Class, null, RubyModule.EmptyArray);
			RubyClass module = DefineGlobalClass("Socket", typeof(RubySocket), 8, super, LoadSocket_Instance, LoadSocket_Class, LoadSocket_Constants, RubyModule.EmptyArray, new Func<ConversionStorage<MutableString>, ConversionStorage<int>, RubyClass, object, int, int, RubySocket>(RubySocket.CreateSocket));
			RubyClass super3 = DefineGlobalClass("TCPSocket", typeof(TCPSocket), 8, super2, LoadTCPSocket_Instance, LoadTCPSocket_Class, null, RubyModule.EmptyArray, new Func<ConversionStorage<MutableString>, ConversionStorage<int>, RubyClass, MutableString, object, int, TCPSocket>(TCPSocket.CreateTCPSocket), new Func<ConversionStorage<MutableString>, ConversionStorage<int>, RubyClass, MutableString, object, MutableString, object, TCPSocket>(TCPSocket.CreateTCPSocket));
			DefineGlobalClass("UDPSocket", typeof(UDPSocket), 8, super2, LoadUDPSocket_Instance, null, null, RubyModule.EmptyArray, new Func<ConversionStorage<MutableString>, ConversionStorage<int>, RubyClass, object, UDPSocket>(UDPSocket.CreateUDPSocket));
			DefineGlobalClass("TCPServer", typeof(TCPServer), 8, super3, LoadTCPServer_Instance, null, null, RubyModule.EmptyArray, new Func<ConversionStorage<MutableString>, ConversionStorage<int>, RubyClass, MutableString, object, TCPServer>(TCPServer.CreateTCPServer));
			LibraryInitializer.SetConstant(module, "Constants", value);
		}

		private static void LoadBasicSocket_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "close_read", 17, 0u, new Action<RubyContext, RubyBasicSocket>(RubyBasicSocket.CloseRead));
			LibraryInitializer.DefineLibraryMethod(module, "close_write", 17, 0u, new Action<RubyContext, RubyBasicSocket>(RubyBasicSocket.CloseWrite));
			LibraryInitializer.DefineLibraryMethod(module, "do_not_reverse_lookup", 17, 0u, new Func<RubyBasicSocket, bool>(RubyBasicSocket.GetDoNotReverseLookup));
			LibraryInitializer.DefineLibraryMethod(module, "do_not_reverse_lookup=", 17, 0u, new Action<RubyBasicSocket, bool>(RubyBasicSocket.SetDoNotReverseLookup));
			LibraryInitializer.DefineLibraryMethod(module, "getpeername", 17, 0u, new Func<RubyBasicSocket, MutableString>(RubyBasicSocket.GetPeerName));
			LibraryInitializer.DefineLibraryMethod(module, "getsockname", 17, 0u, new Func<RubyBasicSocket, MutableString>(RubyBasicSocket.GetSocketName));
			LibraryInitializer.DefineLibraryMethod(module, "getsockopt", 17, 786432u, new Func<ConversionStorage<int>, RubyContext, RubyBasicSocket, int, int, MutableString>(RubyBasicSocket.GetSocketOption));
			LibraryInitializer.DefineLibraryMethod(module, "recv", 17, 131072u, new Func<ConversionStorage<int>, RubyBasicSocket, int, object, MutableString>(RubyBasicSocket.Receive));
			LibraryInitializer.DefineLibraryMethod(module, "recv_nonblock", 17, 131072u, new Func<ConversionStorage<int>, RubyBasicSocket, int, object, MutableString>(RubyBasicSocket.ReceiveNonBlocking));
			LibraryInitializer.DefineLibraryMethod(module, "send", 17, 131076u, 655380u, new Func<ConversionStorage<int>, RubyBasicSocket, MutableString, object, int>(RubyBasicSocket.Send), new Func<ConversionStorage<int>, RubyBasicSocket, MutableString, object, MutableString, int>(RubyBasicSocket.Send));
			LibraryInitializer.DefineLibraryMethod(module, "setsockopt", 17, 786432u, 786432u, 917520u, new Action<ConversionStorage<int>, RubyContext, RubyBasicSocket, int, int, int>(RubyBasicSocket.SetSocketOption), new Action<ConversionStorage<int>, RubyContext, RubyBasicSocket, int, int, bool>(RubyBasicSocket.SetSocketOption), new Action<RubyContext, RubyBasicSocket, int, int, MutableString>(RubyBasicSocket.SetSocketOption));
			LibraryInitializer.DefineLibraryMethod(module, "shutdown", 17, 131072u, new Func<RubyContext, RubyBasicSocket, int, int>(RubyBasicSocket.Shutdown));
		}

		private static void LoadBasicSocket_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "do_not_reverse_lookup", 33, 0u, new Func<RubyClass, bool>(RubyBasicSocket.GetDoNotReverseLookup));
			LibraryInitializer.DefineLibraryMethod(module, "do_not_reverse_lookup=", 33, 0u, new Action<RubyClass, bool>(RubyBasicSocket.SetDoNotReverseLookup));
			LibraryInitializer.DefineRuleGenerator(module, "for_fd", 33, RubyBasicSocket.ForFileDescriptor());
		}

		private static void LoadIPSocket_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "addr", 17, 0u, new Func<RubyContext, IPSocket, RubyArray>(IPSocket.GetLocalAddress));
			LibraryInitializer.DefineLibraryMethod(module, "peeraddr", 17, 0u, new Func<RubyContext, IPSocket, object>(IPSocket.GetPeerAddress));
			LibraryInitializer.DefineLibraryMethod(module, "recvfrom", 17, 0u, new Func<ConversionStorage<int>, IPSocket, int, object, RubyArray>(IPSocket.ReceiveFrom));
		}

		private static void LoadIPSocket_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "getaddress", 33, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, MutableString>(IPSocket.GetAddress));
		}

		private static void LoadSocket_Constants(RubyModule module)
		{
			LoadSocket__Constants_Constants(module);
		}

		private static void LoadSocket_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "accept", 17, 0u, new Func<RubyContext, RubySocket, RubyArray>(RubySocket.Accept));
			LibraryInitializer.DefineLibraryMethod(module, "accept_nonblock", 17, 0u, new Func<RubyContext, RubySocket, RubyArray>(RubySocket.AcceptNonBlocking));
			LibraryInitializer.DefineLibraryMethod(module, "bind", 17, 0u, new Func<RubyContext, RubySocket, MutableString, int>(RubySocket.Bind));
			LibraryInitializer.DefineLibraryMethod(module, "connect", 17, 0u, new Func<RubyContext, RubySocket, MutableString, int>(RubySocket.Connect));
			LibraryInitializer.DefineLibraryMethod(module, "connect_nonblock", 17, 0u, new Func<RubyContext, RubySocket, MutableString, int>(RubySocket.ConnectNonBlocking));
			LibraryInitializer.DefineLibraryMethod(module, "listen", 17, 0u, new Func<RubyContext, RubySocket, int, int>(RubySocket.Listen));
			LibraryInitializer.DefineLibraryMethod(module, "recvfrom", 17, 0u, 0u, new Func<ConversionStorage<int>, RubySocket, int, RubyArray>(RubySocket.ReceiveFrom), new Func<ConversionStorage<int>, RubySocket, int, object, RubyArray>(RubySocket.ReceiveFrom));
			LibraryInitializer.DefineLibraryMethod(module, "sysaccept", 17, 0u, new Func<RubyContext, RubySocket, RubyArray>(RubySocket.SysAccept));
		}

		private static void LoadSocket_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "getaddrinfo", 33, 0u, new Func<ConversionStorage<MutableString>, ConversionStorage<int>, RubyClass, object, object, object, object, object, object, RubyArray>(RubySocket.GetAddressInfo));
			LibraryInitializer.DefineLibraryMethod(module, "gethostbyaddr", 33, 262152u, new Func<ConversionStorage<MutableString>, ConversionStorage<int>, RubyClass, MutableString, object, RubyArray>(RubySocket.GetHostByAddress));
			LibraryInitializer.DefineLibraryMethod(module, "gethostbyname", 33, 0u, 2u, 65536u, new Func<RubyClass, int, RubyArray>(RubySocket.GetHostByName), new Func<RubyClass, BigInteger, RubyArray>(RubySocket.GetHostByName), new Func<RubyClass, MutableString, RubyArray>(RubySocket.GetHostByName));
			LibraryInitializer.DefineLibraryMethod(module, "gethostname", 33, 0u, new Func<RubyClass, MutableString>(RubySocket.GetHostname));
			LibraryInitializer.DefineLibraryMethod(module, "getnameinfo", 33, 8u, 65538u, new Func<ConversionStorage<MutableString>, ConversionStorage<int>, RubyClass, RubyArray, object, RubyArray>(RubySocket.GetNameInfo), new Func<RubyClass, MutableString, object, RubyArray>(RubySocket.GetNameInfo));
			LibraryInitializer.DefineLibraryMethod(module, "getservbyname", 33, 196610u, new Func<RubyClass, MutableString, MutableString, int>(RubySocket.GetServiceByName));
			LibraryInitializer.DefineLibraryMethod(module, "pack_sockaddr_in", 33, 0u, new Func<ConversionStorage<MutableString>, ConversionStorage<int>, RubyClass, object, object, MutableString>(RubySocket.PackInetSockAddr));
			LibraryInitializer.DefineLibraryMethod(module, "pair", 33, 0u, new Func<RubyClass, object, object, object, RubyArray>(RubySocket.CreateSocketPair));
			LibraryInitializer.DefineLibraryMethod(module, "sockaddr_in", 33, 0u, new Func<ConversionStorage<MutableString>, ConversionStorage<int>, RubyClass, object, object, MutableString>(RubySocket.PackInetSockAddr));
			LibraryInitializer.DefineLibraryMethod(module, "socketpair", 33, 0u, new Func<RubyClass, object, object, object, RubyArray>(RubySocket.CreateSocketPair));
			LibraryInitializer.DefineLibraryMethod(module, "unpack_sockaddr_in", 33, 65538u, new Func<RubyClass, MutableString, RubyArray>(RubySocket.UnPackInetSockAddr));
		}

		private static void LoadSocket__Constants_Constants(RubyModule module)
		{
			LibraryInitializer.SetConstant(module, "AF_APPLETALK", 16);
			LibraryInitializer.SetConstant(module, "AF_ATM", 22);
			LibraryInitializer.SetConstant(module, "AF_CCITT", 10);
			LibraryInitializer.SetConstant(module, "AF_CHAOS", 5);
			LibraryInitializer.SetConstant(module, "AF_DATAKIT", 9);
			LibraryInitializer.SetConstant(module, "AF_DLI", 13);
			LibraryInitializer.SetConstant(module, "AF_ECMA", 8);
			LibraryInitializer.SetConstant(module, "AF_HYLINK", 15);
			LibraryInitializer.SetConstant(module, "AF_IMPLINK", 3);
			LibraryInitializer.SetConstant(module, "AF_INET", 2);
			LibraryInitializer.SetConstant(module, "AF_INET6", 23);
			LibraryInitializer.SetConstant(module, "AF_IPX", 6);
			LibraryInitializer.SetConstant(module, "AF_ISO", 7);
			LibraryInitializer.SetConstant(module, "AF_LAT", 14);
			LibraryInitializer.SetConstant(module, "AF_MAX", 29);
			LibraryInitializer.SetConstant(module, "AF_NETBIOS", 17);
			LibraryInitializer.SetConstant(module, "AF_NS", 6);
			LibraryInitializer.SetConstant(module, "AF_OSI", 7);
			LibraryInitializer.SetConstant(module, "AF_PUP", 4);
			LibraryInitializer.SetConstant(module, "AF_SNA", 11);
			LibraryInitializer.SetConstant(module, "AF_UNIX", 1);
			LibraryInitializer.SetConstant(module, "AF_UNSPEC", 0);
			LibraryInitializer.SetConstant(module, "AI_CANONNAME", 2);
			LibraryInitializer.SetConstant(module, "AI_NUMERICHOST", 4);
			LibraryInitializer.SetConstant(module, "AI_PASSIVE", 1);
			LibraryInitializer.SetConstant(module, "EAI_AGAIN", 2);
			LibraryInitializer.SetConstant(module, "EAI_BADFLAGS", 3);
			LibraryInitializer.SetConstant(module, "EAI_FAIL", 4);
			LibraryInitializer.SetConstant(module, "EAI_FAMILY", 5);
			LibraryInitializer.SetConstant(module, "EAI_MEMORY", 6);
			LibraryInitializer.SetConstant(module, "EAI_NODATA", 7);
			LibraryInitializer.SetConstant(module, "EAI_NONAME", 8);
			LibraryInitializer.SetConstant(module, "EAI_SERVICE", 9);
			LibraryInitializer.SetConstant(module, "EAI_SOCKTYPE", 10);
			LibraryInitializer.SetConstant(module, "INADDR_ALLHOSTS_GROUP", 3758096385u);
			LibraryInitializer.SetConstant(module, "INADDR_ANY", 0);
			LibraryInitializer.SetConstant(module, "INADDR_BROADCAST", uint.MaxValue);
			LibraryInitializer.SetConstant(module, "INADDR_LOOPBACK", 2130706433);
			LibraryInitializer.SetConstant(module, "INADDR_MAX_LOCAL_GROUP", 3758096639u);
			LibraryInitializer.SetConstant(module, "INADDR_NONE", uint.MaxValue);
			LibraryInitializer.SetConstant(module, "INADDR_UNSPEC_GROUP", 3758096384u);
			LibraryInitializer.SetConstant(module, "INET_ADDRSTRLEN", 16);
			LibraryInitializer.SetConstant(module, "INET6_ADDRSTRLEN", 46);
			LibraryInitializer.SetConstant(module, "IP_ADD_MEMBERSHIP", 12);
			LibraryInitializer.SetConstant(module, "IP_ADD_SOURCE_MEMBERSHIP", 15);
			LibraryInitializer.SetConstant(module, "IP_BLOCK_SOURCE", 17);
			LibraryInitializer.SetConstant(module, "IP_DEFAULT_MULTICAST_LOOP", 1);
			LibraryInitializer.SetConstant(module, "IP_DEFAULT_MULTICAST_TTL", 1);
			LibraryInitializer.SetConstant(module, "IP_DROP_MEMBERSHIP", 13);
			LibraryInitializer.SetConstant(module, "IP_DROP_SOURCE_MEMBERSHIP", 16);
			LibraryInitializer.SetConstant(module, "IP_HDRINCL", 2);
			LibraryInitializer.SetConstant(module, "IP_MAX_MEMBERSHIPS", 20);
			LibraryInitializer.SetConstant(module, "IP_MULTICAST_IF", 9);
			LibraryInitializer.SetConstant(module, "IP_MULTICAST_LOOP", 11);
			LibraryInitializer.SetConstant(module, "IP_MULTICAST_TTL", 10);
			LibraryInitializer.SetConstant(module, "IP_OPTIONS", 1);
			LibraryInitializer.SetConstant(module, "IP_PKTINFO", 19);
			LibraryInitializer.SetConstant(module, "IP_TOS", 3);
			LibraryInitializer.SetConstant(module, "IP_TTL", 4);
			LibraryInitializer.SetConstant(module, "IP_UNBLOCK_SOURCE", 18);
			LibraryInitializer.SetConstant(module, "IPPORT_RESERVED", 1024);
			LibraryInitializer.SetConstant(module, "IPPORT_USERRESERVED", 5000);
			LibraryInitializer.SetConstant(module, "IPPROTO_AH", 51);
			LibraryInitializer.SetConstant(module, "IPPROTO_DSTOPTS", 60);
			LibraryInitializer.SetConstant(module, "IPPROTO_ESP", 50);
			LibraryInitializer.SetConstant(module, "IPPROTO_FRAGMENT", 44);
			LibraryInitializer.SetConstant(module, "IPPROTO_GGP", 3);
			LibraryInitializer.SetConstant(module, "IPPROTO_HOPOPTS", 0);
			LibraryInitializer.SetConstant(module, "IPPROTO_ICMP", 1);
			LibraryInitializer.SetConstant(module, "IPPROTO_ICMPV6", 58);
			LibraryInitializer.SetConstant(module, "IPPROTO_IDP", 22);
			LibraryInitializer.SetConstant(module, "IPPROTO_IGMP", 2);
			LibraryInitializer.SetConstant(module, "IPPROTO_IP", 0);
			LibraryInitializer.SetConstant(module, "IPPROTO_IPV6", 41);
			LibraryInitializer.SetConstant(module, "IPPROTO_MAX", 256);
			LibraryInitializer.SetConstant(module, "IPPROTO_ND", 77);
			LibraryInitializer.SetConstant(module, "IPPROTO_NONE", 59);
			LibraryInitializer.SetConstant(module, "IPPROTO_PUP", 12);
			LibraryInitializer.SetConstant(module, "IPPROTO_RAW", 255);
			LibraryInitializer.SetConstant(module, "IPPROTO_ROUTING", 43);
			LibraryInitializer.SetConstant(module, "IPPROTO_TCP", 6);
			LibraryInitializer.SetConstant(module, "IPPROTO_UDP", 17);
			LibraryInitializer.SetConstant(module, "IPV6_JOIN_GROUP", 12);
			LibraryInitializer.SetConstant(module, "IPV6_LEAVE_GROUP", 13);
			LibraryInitializer.SetConstant(module, "IPV6_MULTICAST_HOPS", 10);
			LibraryInitializer.SetConstant(module, "IPV6_MULTICAST_IF", 9);
			LibraryInitializer.SetConstant(module, "IPV6_MULTICAST_LOOP", 11);
			LibraryInitializer.SetConstant(module, "IPV6_PKTINFO", 19);
			LibraryInitializer.SetConstant(module, "IPV6_UNICAST_HOPS", 4);
			LibraryInitializer.SetConstant(module, "MSG_DONTROUTE", 4);
			LibraryInitializer.SetConstant(module, "MSG_OOB", 1);
			LibraryInitializer.SetConstant(module, "MSG_PEEK", 2);
			LibraryInitializer.SetConstant(module, "NI_DGRAM", 16);
			LibraryInitializer.SetConstant(module, "NI_MAXHOST", 1025);
			LibraryInitializer.SetConstant(module, "NI_MAXSERV", 32);
			LibraryInitializer.SetConstant(module, "NI_NAMEREQD", 4);
			LibraryInitializer.SetConstant(module, "NI_NOFQDN", 1);
			LibraryInitializer.SetConstant(module, "NI_NUMERICHOST", 2);
			LibraryInitializer.SetConstant(module, "NI_NUMERICSERV", 8);
			LibraryInitializer.SetConstant(module, "PF_APPLETALK", 16);
			LibraryInitializer.SetConstant(module, "PF_ATM", 22);
			LibraryInitializer.SetConstant(module, "PF_CCITT", 10);
			LibraryInitializer.SetConstant(module, "PF_CHAOS", 5);
			LibraryInitializer.SetConstant(module, "PF_DATAKIT", 9);
			LibraryInitializer.SetConstant(module, "PF_DLI", 13);
			LibraryInitializer.SetConstant(module, "PF_ECMA", 8);
			LibraryInitializer.SetConstant(module, "PF_HYLINK", 15);
			LibraryInitializer.SetConstant(module, "PF_IMPLINK", 3);
			LibraryInitializer.SetConstant(module, "PF_INET", 2);
			LibraryInitializer.SetConstant(module, "PF_INET6", 23);
			LibraryInitializer.SetConstant(module, "PF_IPX", 6);
			LibraryInitializer.SetConstant(module, "PF_ISO", 7);
			LibraryInitializer.SetConstant(module, "PF_LAT", 14);
			LibraryInitializer.SetConstant(module, "PF_MAX", 29);
			LibraryInitializer.SetConstant(module, "PF_NS", 6);
			LibraryInitializer.SetConstant(module, "PF_OSI", 7);
			LibraryInitializer.SetConstant(module, "PF_PUP", 4);
			LibraryInitializer.SetConstant(module, "PF_SNA", 11);
			LibraryInitializer.SetConstant(module, "PF_UNIX", 1);
			LibraryInitializer.SetConstant(module, "PF_UNSPEC", 0);
			LibraryInitializer.SetConstant(module, "SHUT_RD", 0);
			LibraryInitializer.SetConstant(module, "SHUT_RDWR", 2);
			LibraryInitializer.SetConstant(module, "SHUT_WR", 1);
			LibraryInitializer.SetConstant(module, "SO_ACCEPTCONN", 2);
			LibraryInitializer.SetConstant(module, "SO_BROADCAST", 32);
			LibraryInitializer.SetConstant(module, "SO_DEBUG", 1);
			LibraryInitializer.SetConstant(module, "SO_DONTROUTE", 16);
			LibraryInitializer.SetConstant(module, "SO_ERROR", 4103);
			LibraryInitializer.SetConstant(module, "SO_KEEPALIVE", 8);
			LibraryInitializer.SetConstant(module, "SO_LINGER", 128);
			LibraryInitializer.SetConstant(module, "SO_OOBINLINE", 256);
			LibraryInitializer.SetConstant(module, "SO_RCVBUF", 4098);
			LibraryInitializer.SetConstant(module, "SO_RCVLOWAT", 4100);
			LibraryInitializer.SetConstant(module, "SO_RCVTIMEO", 4102);
			LibraryInitializer.SetConstant(module, "SO_REUSEADDR", 4);
			LibraryInitializer.SetConstant(module, "SO_SNDBUF", 4097);
			LibraryInitializer.SetConstant(module, "SO_SNDLOWAT", 4099);
			LibraryInitializer.SetConstant(module, "SO_SNDTIMEO", 4101);
			LibraryInitializer.SetConstant(module, "SO_TYPE", 4104);
			LibraryInitializer.SetConstant(module, "SO_USELOOPBACK", 64);
			LibraryInitializer.SetConstant(module, "SOCK_DGRAM", 2);
			LibraryInitializer.SetConstant(module, "SOCK_RAW", 3);
			LibraryInitializer.SetConstant(module, "SOCK_RDM", 4);
			LibraryInitializer.SetConstant(module, "SOCK_SEQPACKET", 5);
			LibraryInitializer.SetConstant(module, "SOCK_STREAM", 1);
			LibraryInitializer.SetConstant(module, "SOL_SOCKET", 65535);
			LibraryInitializer.SetConstant(module, "SOMAXCONN", int.MaxValue);
			LibraryInitializer.SetConstant(module, "TCP_NODELAY", 1);
		}

		private static void LoadSocketError_Instance(RubyModule module)
		{
			module.HideMethod("message");
		}

		private static void LoadTCPServer_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "accept", 17, 0u, new Func<RubyContext, TCPServer, TCPSocket>(TCPServer.Accept));
			LibraryInitializer.DefineLibraryMethod(module, "accept_nonblock", 17, 0u, new Func<RubyContext, TCPServer, TCPSocket>(TCPServer.AcceptNonBlocking));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 18, 262144u, new Func<ConversionStorage<MutableString>, ConversionStorage<int>, TCPServer, MutableString, object, TCPServer>(TCPServer.Reinitialize));
			LibraryInitializer.DefineLibraryMethod(module, "listen", 17, 0u, new Action<TCPServer, int>(TCPServer.Listen));
			LibraryInitializer.DefineLibraryMethod(module, "sysaccept", 17, 0u, new Func<RubyContext, TCPServer, int>(TCPServer.SysAccept));
		}

		private static void LoadTCPSocket_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 18, 262144u, 1310720u, new Func<ConversionStorage<MutableString>, ConversionStorage<int>, TCPServer, MutableString, object, int, TCPServer>(TCPSocket.Reinitialize), new Func<ConversionStorage<MutableString>, ConversionStorage<int>, TCPServer, MutableString, object, MutableString, object, TCPServer>(TCPSocket.Reinitialize));
		}

		private static void LoadTCPSocket_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "gethostbyname", 33, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, RubyArray>(TCPSocket.GetHostByName));
		}

		private static void LoadUDPSocket_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "bind", 17, 0u, new Func<ConversionStorage<MutableString>, ConversionStorage<int>, UDPSocket, object, object, int>(UDPSocket.Bind));
			LibraryInitializer.DefineLibraryMethod(module, "connect", 17, 0u, new Func<ConversionStorage<MutableString>, ConversionStorage<int>, UDPSocket, object, object, int>(UDPSocket.Connect));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 18, 0u, new Func<ConversionStorage<MutableString>, ConversionStorage<int>, UDPSocket, object, UDPSocket>(UDPSocket.Reinitialize));
			LibraryInitializer.DefineLibraryMethod(module, "recvfrom_nonblock", 17, 0u, 0u, new Func<ConversionStorage<int>, IPSocket, int, RubyArray>(UDPSocket.ReceiveFromNonBlocking), new Func<ConversionStorage<int>, IPSocket, int, object, RubyArray>(UDPSocket.ReceiveFromNonBlocking));
			LibraryInitializer.DefineLibraryMethod(module, "send", 17, 262152u, 131076u, 655380u, new Func<ConversionStorage<int>, ConversionStorage<MutableString>, RubyBasicSocket, MutableString, object, object, object, int>(UDPSocket.Send), new Func<ConversionStorage<int>, RubyBasicSocket, MutableString, object, int>(UDPSocket.Send), new Func<ConversionStorage<int>, RubyBasicSocket, MutableString, object, MutableString, int>(UDPSocket.Send));
		}
	}
}
