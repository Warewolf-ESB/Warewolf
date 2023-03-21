using System;
using System.IO;
using System.Net.Sockets;

namespace IronRuby.StandardLibrary.Sockets
{
	internal class SocketStream : Stream
	{
		internal readonly Socket _socket;

		public override bool CanRead
		{
			get
			{
				return true;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return true;
			}
		}

		public override long Length
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		public override long Position
		{
			get
			{
				throw new InvalidOperationException();
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public SocketStream(Socket s)
		{
			_socket = s;
		}

		public override void Close()
		{
			base.Close();
			_socket.Close();
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _socket.Receive(buffer, offset, count, SocketFlags.None);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new InvalidOperationException();
		}

		public override void SetLength(long value)
		{
			throw new InvalidOperationException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_socket.Send(buffer, offset, count, SocketFlags.None);
		}
	}
}
