using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Sockets
{
	[RubyClass("TCPServer", BuildConfig = "!SILVERLIGHT")]
	public class TCPServer : TCPSocket
	{
		private object _mutex = new object();

		private IAsyncResult _acceptResult;

		public TCPServer(RubyContext context)
			: base(context)
		{
		}

		public TCPServer(RubyContext context, Socket socket)
			: base(context, socket)
		{
		}

		public override WaitHandle CreateReadWaitHandle()
		{
			return GetAcceptResult().AsyncWaitHandle;
		}

		private IAsyncResult GetAcceptResult()
		{
			if (_acceptResult == null)
			{
				lock (_mutex)
				{
					if (_acceptResult == null)
					{
						_acceptResult = base.Socket.BeginAccept(null, null);
					}
				}
			}
			return _acceptResult;
		}

		private Socket Accept()
		{
			IAsyncResult asyncResult = Interlocked.Exchange(ref _acceptResult, null);
			if (asyncResult == null)
			{
				ThreadOps.RubyThreadInfo rubyThreadInfo = ThreadOps.RubyThreadInfo.FromThread(Thread.CurrentThread);
				rubyThreadInfo.Blocked = true;
				try
				{
					return base.Socket.Accept();
				}
				finally
				{
					rubyThreadInfo.Blocked = false;
				}
			}
			return base.Socket.EndAccept(asyncResult);
		}

		[RubyConstructor]
		public static TCPServer CreateTCPServer(ConversionStorage<MutableString> stringCast, ConversionStorage<int> fixnumCast, RubyClass self, [DefaultProtocol] MutableString hostname, object port)
		{
			return new TCPServer(self.Context, CreateSocket(stringCast, fixnumCast, hostname, port));
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static TCPServer Reinitialize(ConversionStorage<MutableString> stringCast, ConversionStorage<int> fixnumCast, TCPServer self, [DefaultProtocol] MutableString hostname, object port)
		{
			self.Socket = CreateSocket(stringCast, fixnumCast, hostname, port);
			return self;
		}

		private static Socket CreateSocket(ConversionStorage<MutableString> stringCast, ConversionStorage<int> fixnumCast, [DefaultProtocol] MutableString hostname, object port)
		{
			IPAddress address = null;
			if (hostname == null)
			{
				address = new IPAddress(0L);
			}
			else if (hostname.IsEmpty)
			{
				address = IPAddress.Any;
			}
			else
			{
				string text = hostname.ConvertToString();
				if (text == IPAddress.Any.ToString())
				{
					address = IPAddress.Any;
				}
				else if (text == IPAddress.Loopback.ToString())
				{
					address = IPAddress.Loopback;
				}
				else if (!IPAddress.TryParse(text, out address))
				{
					IPHostEntry hostEntry = Dns.GetHostEntry(text);
					IPAddress[] addressList = hostEntry.AddressList;
					foreach (IPAddress iPAddress in addressList)
					{
						if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
						{
							address = iPAddress;
							break;
						}
					}
					if (address == null)
					{
						throw new NotImplementedException("TODO: non-inet addresses");
					}
				}
			}
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try
			{
				socket.Bind(new IPEndPoint(address, RubyBasicSocket.ConvertToPortNum(stringCast, fixnumCast, port)));
				socket.Listen(10);
				return socket;
			}
			catch (SocketException ex)
			{
				SocketError socketErrorCode = ex.SocketErrorCode;
				if (socketErrorCode == SocketError.AddressAlreadyInUse)
				{
					throw new Errno.AddressInUseError();
				}
				throw;
			}
		}

		[RubyMethod("accept")]
		public static TCPSocket Accept(RubyContext context, TCPServer self)
		{
			return new TCPSocket(context, self.Accept());
		}

		[RubyMethod("accept_nonblock")]
		public static TCPSocket AcceptNonBlocking(RubyContext context, TCPServer self)
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

		[RubyMethod("sysaccept")]
		public static int SysAccept(RubyContext context, TCPServer self)
		{
			return Accept(context, self).GetFileDescriptor();
		}

		[RubyMethod("listen")]
		public static void Listen(TCPServer self, int backlog)
		{
			self.Socket.Listen(backlog);
		}
	}
}
