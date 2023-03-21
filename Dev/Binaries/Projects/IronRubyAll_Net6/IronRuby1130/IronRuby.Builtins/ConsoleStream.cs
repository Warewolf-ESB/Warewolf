using System;
using System.IO;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	internal sealed class ConsoleStream : Stream
	{
		private ConsoleStreamType _consoleType;

		private readonly SharedIO _io;

		public ConsoleStreamType StreamType
		{
			get
			{
				return _consoleType;
			}
		}

		public override bool CanRead
		{
			get
			{
				if (_consoleType != 0)
				{
					return false;
				}
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
				if (_consoleType != 0)
				{
					return true;
				}
				return false;
			}
		}

		public override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override long Position
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public ConsoleStream(SharedIO io, ConsoleStreamType consoleType)
		{
			_consoleType = consoleType;
			_io = io;
		}

		public override void Flush()
		{
			switch (_consoleType)
			{
			case ConsoleStreamType.ErrorOutput:
				_io.ErrorWriter.Flush();
				break;
			case ConsoleStreamType.Output:
				_io.OutputWriter.Flush();
				break;
			case ConsoleStreamType.Input:
				throw new NotSupportedException();
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _io.InputStream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (_consoleType == ConsoleStreamType.Output)
			{
				_io.OutputStream.Write(buffer, offset, count);
			}
			else
			{
				_io.ErrorStream.Write(buffer, offset, count);
			}
		}
	}
}
