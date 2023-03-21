using System.Net;
using System.Net.Sockets;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Sockets
{
	[RubyClass("IPSocket", BuildConfig = "!SILVERLIGHT")]
	public abstract class IPSocket : RubyBasicSocket
	{
		protected IPSocket(RubyContext context)
			: base(context)
		{
		}

		protected IPSocket(RubyContext context, Socket socket)
			: base(context, socket)
		{
		}

		[RubyMethod("getaddress", RubyMethodAttributes.PublicSingleton)]
		public static MutableString GetAddress(ConversionStorage<MutableString> stringCast, RubyClass self, object hostNameOrAddress)
		{
			return MutableString.CreateAscii(RubyBasicSocket.GetHostAddress(RubyBasicSocket.ConvertToHostString(stringCast, hostNameOrAddress)).ToString());
		}

		[RubyMethod("addr")]
		public static RubyArray GetLocalAddress(RubyContext context, IPSocket self)
		{
			return self.GetAddressArray(self.Socket.LocalEndPoint);
		}

		[RubyMethod("peeraddr")]
		public static object GetPeerAddress(RubyContext context, IPSocket self)
		{
			return self.GetAddressArray(self.Socket.RemoteEndPoint);
		}

		[RubyMethod("recvfrom")]
		public static RubyArray ReceiveFrom(ConversionStorage<int> conversionStorage, IPSocket self, int length, object flags)
		{
			SocketFlags socketFlags = RubyBasicSocket.ConvertToSocketFlag(conversionStorage, flags);
			byte[] array = new byte[length];
			EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
			int count = self.Socket.ReceiveFrom(array, socketFlags, ref remoteEP);
			MutableString mutableString = MutableString.CreateBinary();
			mutableString.Append(array, 0, count);
			mutableString.IsTainted = true;
			return RubyOps.MakeArray2(mutableString, self.GetAddressArray(remoteEP));
		}
	}
}
