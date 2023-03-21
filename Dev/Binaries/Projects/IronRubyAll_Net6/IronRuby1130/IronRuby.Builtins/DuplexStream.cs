using System;
using System.IO;

namespace IronRuby.Builtins
{
	internal sealed class DuplexStream : Stream
	{
		private readonly StreamReader _reader;

		private readonly StreamWriter _writer;

		public StreamReader Reader
		{
			get
			{
				return _reader;
			}
		}

		public StreamWriter Writer
		{
			get
			{
				return _writer;
			}
		}

		public override bool CanRead
		{
			get
			{
				return _reader != null;
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
				return _writer != null;
			}
		}

		public override long Length
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override long Position
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public DuplexStream(StreamReader reader, StreamWriter writer)
		{
			_reader = reader;
			_writer = writer;
		}

		public override void Close()
		{
			if (_reader != null)
			{
				_reader.Close();
			}
			if (_writer != null)
			{
				_writer.Close();
			}
		}

		public override void Flush()
		{
			if (_reader != null)
			{
				_reader.BaseStream.Flush();
			}
			if (_writer != null)
			{
				_writer.Flush();
			}
		}

		public override int ReadByte()
		{
			if (_reader == null)
			{
				throw new InvalidOperationException();
			}
			return _reader.Read();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (_reader == null)
			{
				throw new InvalidOperationException();
			}
			return _reader.BaseStream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (_writer == null)
			{
				throw new InvalidOperationException();
			}
			_writer.Write(_writer.Encoding.GetString(buffer, offset, count));
		}
	}
}
