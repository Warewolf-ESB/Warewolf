using System;
using System.IO;
using IronRuby.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	public class RubyBufferedStream : Stream
	{
		private const byte CR = 13;

		private const byte LF = 10;

		private readonly Stream _stream;

		private byte[] _buffer;

		private int _defaultBufferSize;

		private int _bufferStart;

		private int _bufferCount;

		private int _pushedBackCount;

		private bool _pushBackPreservesPosition;

		public Stream BaseStream
		{
			get
			{
				return _stream;
			}
		}

		public bool DataBuffered
		{
			get
			{
				return _bufferCount > 0;
			}
		}

		private int ReadAheadCount
		{
			get
			{
				return _bufferCount - _pushedBackCount;
			}
		}

		public override long Position
		{
			get
			{
				if (_pushBackPreservesPosition)
				{
					return _stream.Position - ReadAheadCount;
				}
				return Math.Max(_stream.Position - _bufferCount, 0L);
			}
			set
			{
				ContractUtils.Requires(value >= 0, "value", "Value must be positive");
				Seek(value, SeekOrigin.Begin);
			}
		}

		public override bool CanRead
		{
			get
			{
				return _stream.CanRead;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return _stream.CanSeek;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return _stream.CanWrite;
			}
		}

		public override long Length
		{
			get
			{
				return _stream.Length;
			}
		}

		public RubyBufferedStream(Stream stream)
			: this(stream, false)
		{
		}

		public RubyBufferedStream(Stream stream, bool pushBackPreservesPosition)
			: this(stream, pushBackPreservesPosition, 4096)
		{
		}

		public RubyBufferedStream(Stream stream, bool pushBackPreservesPosition, int bufferSize)
		{
			ContractUtils.RequiresNotNull(stream, "stream");
			ContractUtils.Requires(bufferSize > 0, "bufferSize", "Buffer size must be positive.");
			_stream = stream;
			_defaultBufferSize = bufferSize;
			_pushBackPreservesPosition = pushBackPreservesPosition;
		}

		private int LoadBuffer(int count)
		{
			if (_buffer == null)
			{
				_buffer = new byte[_defaultBufferSize];
			}
			else if (_bufferStart + _bufferCount + count > _buffer.Length)
			{
				Buffer.BlockCopy(_buffer, _bufferStart, _buffer, 0, _bufferCount);
				_bufferStart = 0;
			}
			int num = _stream.Read(_buffer, _bufferCount, count);
			_bufferCount += num;
			return num;
		}

		private void ConsumeBuffered(int count)
		{
			_bufferCount -= count;
			_pushedBackCount -= Math.Min(_pushedBackCount, count);
			if (_bufferCount == 0)
			{
				_bufferStart = 0;
			}
			else
			{
				_bufferStart += count;
			}
		}

		public void PushBack(byte b)
		{
			if (_bufferStart > 0)
			{
				_buffer[--_bufferStart] = b;
			}
			else if (_buffer != null)
			{
				Utils.InsertAt(ref _buffer, _bufferCount, 0, b, 1);
			}
			else
			{
				_buffer = new byte[_defaultBufferSize];
				_buffer[0] = b;
			}
			_pushedBackCount++;
			_bufferCount++;
		}

		public override void Close()
		{
			_buffer = null;
			_bufferCount = (_bufferStart = (_pushedBackCount = 0));
			_stream.Close();
		}

		public override long Seek(long pos, SeekOrigin origin)
		{
			if (origin == SeekOrigin.Current)
			{
				if (_pushBackPreservesPosition)
				{
					pos -= ReadAheadCount;
				}
				else
				{
					origin = SeekOrigin.Begin;
					pos += Position;
				}
			}
			long result = _stream.Seek(pos, origin);
			_bufferStart = (_bufferCount = (_pushedBackCount = 0));
			return result;
		}

		private void FlushRead()
		{
			if (ReadAheadCount > 0)
			{
				Seek(-ReadAheadCount, SeekOrigin.Current);
			}
			_bufferStart = (_bufferCount = (_pushedBackCount = 0));
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			FlushRead();
			_stream.Write(buffer, offset, count);
		}

		public int WriteBytes(MutableString buffer, int offset, int count, bool preserveEndOfLines)
		{
			return WriteBytes(buffer.SwitchToBytes().GetByteArrayChecked(offset, count), offset, count, preserveEndOfLines);
		}

		public int WriteBytes(byte[] buffer, int offset, int count, bool preserveEndOfLines)
		{
			ContractUtils.RequiresArrayRange(buffer.Length, offset, count, "offset", "count");
			FlushRead();
			if (preserveEndOfLines)
			{
				_stream.Write(buffer, offset, count);
				return count;
			}
			int num = 0;
			int num2 = offset;
			int num3 = offset + count;
			while (num2 < num3)
			{
				int i;
				for (i = num2; i < num3 && buffer[i] != 10; i++)
				{
				}
				_stream.Write(buffer, num2, i - num2);
				num += i - num2;
				if (i < num3)
				{
					_stream.WriteByte(13);
					_stream.WriteByte(10);
					num += 2;
				}
				num2 = i + 1;
			}
			return num;
		}

		public int PeekByte()
		{
			return PeekByte(0);
		}

		private int PeekByte(int i)
		{
			if (i >= _bufferCount)
			{
				LoadBuffer(i + 1 - _bufferCount);
			}
			if (i >= _bufferCount)
			{
				return -1;
			}
			return _buffer[_bufferStart + i];
		}

		private byte ReadBufferByte()
		{
			byte result = _buffer[_bufferStart];
			ConsumeBuffered(1);
			return result;
		}

		public override int ReadByte()
		{
			if (_bufferCount <= 0)
			{
				return _stream.ReadByte();
			}
			return ReadBufferByte();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int num = Math.Min(_bufferCount, count);
			if (num > 0)
			{
				Buffer.BlockCopy(_buffer, _bufferStart, buffer, offset, num);
				ConsumeBuffered(num);
			}
			return num + _stream.Read(buffer, offset + num, count - num);
		}

		public int AppendBytes(MutableString buffer, int count, bool preserveEndOfLines)
		{
			ContractUtils.RequiresNotNull(buffer, "buffer");
			ContractUtils.Requires(count >= 0, "count");
			if (count == 0)
			{
				return 0;
			}
			bool flag = count == int.MaxValue;
			buffer.SwitchToBytes();
			int byteCount = buffer.GetByteCount();
			if (preserveEndOfLines)
			{
				AppendRawBytes(buffer, count);
			}
			else
			{
				byte[] emptyBytes = Utils.EmptyBytes;
				int num = byteCount;
				bool flag2;
				do
				{
					AppendRawBytes(buffer, flag ? 1024 : count);
					int byteCount2 = buffer.GetByteCount();
					int num2 = byteCount2 - num;
					if (num2 == 0)
					{
						break;
					}
					flag2 = num2 < count;
					buffer.EnsureCapacity(byteCount2 + 3);
					int count2;
					emptyBytes = buffer.GetByteArray(out count2);
					if (emptyBytes[byteCount2 - 1] == 13 && PeekByte(0) == 10)
					{
						ReadByte();
						emptyBytes[byteCount2++] = 10;
					}
					emptyBytes[byteCount2] = 13;
					emptyBytes[byteCount2 + 1] = 10;
					int num3 = IndexOfCrLf(emptyBytes, num);
					count -= num3 - num;
					num = num3;
					while (num3 < byteCount2)
					{
						int num4 = IndexOfCrLf(emptyBytes, num3 + 2);
						int num5 = num4 - num3 - 1;
						Buffer.BlockCopy(emptyBytes, num3 + 1, emptyBytes, num, num5);
						num += num5;
						count -= num5;
						num3 = num4;
					}
					buffer.Remove(num);
				}
				while (flag || (count > 0 && !flag2));
			}
			if (flag)
			{
				buffer.TrimExcess();
			}
			return buffer.GetByteCount() - byteCount;
		}

		private void AppendRawBytes(MutableString buffer, int count)
		{
			int num = count;
			if (_bufferCount > 0)
			{
				int num2 = Math.Min(_bufferCount, count);
				buffer.Append(_buffer, _bufferStart, num2);
				ConsumeBuffered(num2);
				num -= num2;
			}
			if (count == int.MaxValue)
			{
				int num3 = buffer.GetByteCount();
				int num4;
				do
				{
					buffer.Append(_stream, 1024);
					num4 = buffer.GetByteCount() - num3;
					num3 += num4;
				}
				while (num4 == 1024);
			}
			else
			{
				buffer.Append(_stream, num);
			}
		}

		private static int IndexOfCrLf(byte[] array, int i)
		{
			while (array[i++] != 13 || array[i] != 10)
			{
			}
			return i - 1;
		}

		public int ReadByteNormalizeEoln(bool preserveEndOfLines)
		{
			int num = ReadByte();
			if (num == 13 && !preserveEndOfLines)
			{
				int num2 = PeekByte(0);
				if (num2 == 10)
				{
					return ReadByte();
				}
			}
			return num;
		}

		public int PeekByteNormalizeEoln(bool preserveEndOfLines)
		{
			int num = PeekByte(0);
			switch (num)
			{
			case -1:
				return -1;
			case 13:
				if (!preserveEndOfLines && PeekByte(1) == 10)
				{
					return 10;
				}
				break;
			}
			return num;
		}

		public MutableString ReadLineOrParagraph(MutableString separator, RubyEncoding encoding, bool preserveEndOfLines, int limit)
		{
			ContractUtils.Requires(limit >= 0);
			if (limit == 0)
			{
				return MutableString.CreateEmpty();
			}
			if (separator == null)
			{
				MutableString mutableString = MutableString.CreateBinary();
				if (AppendBytes(mutableString, limit, preserveEndOfLines) != 0)
				{
					return mutableString;
				}
				return null;
			}
			if (separator.StartsWith('\n') && separator.GetLength() == 1)
			{
				return ReadLine(encoding, preserveEndOfLines, limit);
			}
			if (separator.IsEmpty)
			{
				return ReadParagraph(encoding, preserveEndOfLines, limit);
			}
			return ReadLine(separator, encoding, preserveEndOfLines, limit);
		}

		public MutableString ReadLine(RubyEncoding encoding, bool preserveEndOfLines, int limit)
		{
			if (_bufferCount == 0 && LoadBuffer(_defaultBufferSize) == 0)
			{
				return null;
			}
			bool bufferResized = false;
			int num = Array.IndexOf(_buffer, (byte)10, _bufferStart, _bufferCount);
			while (num < 0)
			{
				int bufferCount = _bufferCount;
				LoadBuffer(_buffer.Length - _bufferCount);
				num = Array.IndexOf(_buffer, (byte)10, bufferCount, _bufferCount - bufferCount);
				if (num >= 0)
				{
					break;
				}
				if (_bufferCount < _buffer.Length)
				{
					return ConsumeLine(encoding, _bufferCount, _bufferCount, bufferResized);
				}
				Array.Resize(ref _buffer, _buffer.Length << 1);
				bufferResized = true;
				_bufferStart = 0;
			}
			int num2 = num + 1 - _bufferStart;
			int lineLength;
			if (!preserveEndOfLines && num - 1 >= _bufferStart && _buffer[num - 1] == 13)
			{
				_buffer[num - 1] = 10;
				lineLength = num2 - 1;
			}
			else
			{
				lineLength = num2;
			}
			return ConsumeLine(encoding, lineLength, num2, bufferResized);
		}

		private MutableString ConsumeLine(RubyEncoding encoding, int lineLength, int consume, bool bufferResized)
		{
			MutableString result;
			if (bufferResized || (_bufferStart == 0 && !Utils.IsSparse(lineLength, _buffer.Length)))
			{
				result = new MutableString(_buffer, lineLength, encoding);
				if (_bufferCount > consume)
				{
					byte[] array = new byte[Math.Max(_defaultBufferSize, _bufferCount - consume)];
					Buffer.BlockCopy(_buffer, consume, array, 0, _bufferCount - consume);
					_buffer = array;
				}
				else
				{
					_buffer = null;
				}
				ConsumeBuffered(consume);
				_bufferStart = 0;
			}
			else
			{
				result = MutableString.CreateBinary(encoding).Append(_buffer, _bufferStart, lineLength);
				ConsumeBuffered(consume);
			}
			return result;
		}

		public MutableString ReadParagraph(RubyEncoding encoding, bool preserveEndOfLines, int limit)
		{
			MutableString result = ReadLine(MutableString.CreateAscii("\n\n"), encoding, preserveEndOfLines, limit);
			int num;
			while ((num = PeekByteNormalizeEoln(preserveEndOfLines)) != -1 && num == 10)
			{
				ReadByteNormalizeEoln(preserveEndOfLines);
			}
			return result;
		}

		public MutableString ReadLine(MutableString separator, RubyEncoding encoding, bool preserveEndOfLines, int limit)
		{
			int num = ReadByteNormalizeEoln(preserveEndOfLines);
			if (num == -1)
			{
				return null;
			}
			int num2 = 0;
			int byteCount = separator.GetByteCount();
			MutableString mutableString = MutableString.CreateBinary(encoding);
			do
			{
				mutableString.Append((byte)num);
				if (num == separator.GetByte(num2))
				{
					if (num2 == byteCount - 1)
					{
						break;
					}
					num2++;
				}
				else if (num2 > 0)
				{
					num2 = 0;
				}
				num = ReadByteNormalizeEoln(preserveEndOfLines);
			}
			while (num != -1);
			return mutableString;
		}

		public override void Flush()
		{
			FlushRead();
			_stream.Flush();
		}

		public override void SetLength(long value)
		{
			_stream.SetLength(value);
		}
	}
}
