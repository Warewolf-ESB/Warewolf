using System;
using System.IO;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Runtime.Calls;

namespace IronRuby.Runtime
{
	public class IOWrapper : Stream
	{
		private readonly CallSite<Func<CallSite, object, object, object>> _writeSite;

		private readonly CallSite<Func<CallSite, object, object, object>> _readSite;

		private readonly CallSite<Func<CallSite, object, object, object, object>> _seekSite;

		private readonly CallSite<Func<CallSite, object, object>> _tellSite;

		private readonly object _obj;

		private readonly bool _canRead;

		private readonly bool _canWrite;

		private readonly bool _canSeek;

		private readonly bool _canFlush;

		private readonly bool _canBeClosed;

		private readonly byte[] _buffer;

		private int _writePos;

		private int _readPos;

		private int _readLen;

		public object UnderlyingObject
		{
			get
			{
				return _obj;
			}
		}

		public override bool CanRead
		{
			get
			{
				return _canRead;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return _canSeek;
			}
		}

		public bool CanBeClosed
		{
			get
			{
				return _canBeClosed;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return _canWrite;
			}
		}

		public override long Length
		{
			get
			{
				long position = Position;
				Seek(0L, SeekOrigin.End);
				long position2 = Position;
				Position = position;
				return position2;
			}
		}

		public override long Position
		{
			get
			{
				if (!_canSeek)
				{
					throw new NotSupportedException();
				}
				return (long)_tellSite.Target(_tellSite, _obj);
			}
			set
			{
				if (!_canSeek)
				{
					throw new NotSupportedException();
				}
				Seek(value, SeekOrigin.Begin);
			}
		}

		public IOWrapper(RubyContext context, object io, bool canRead, bool canWrite, bool canSeek, bool canFlush, bool canBeClosed, int bufferSize)
		{
			_writeSite = CallSite<Func<CallSite, object, object, object>>.Create(RubyCallAction.Make(context, "write", RubyCallSignature.WithImplicitSelf(1)));
			_readSite = CallSite<Func<CallSite, object, object, object>>.Create(RubyCallAction.Make(context, "read", RubyCallSignature.WithImplicitSelf(1)));
			_seekSite = CallSite<Func<CallSite, object, object, object, object>>.Create(RubyCallAction.Make(context, "seek", RubyCallSignature.WithImplicitSelf(2)));
			_tellSite = CallSite<Func<CallSite, object, object>>.Create(RubyCallAction.Make(context, "tell", RubyCallSignature.WithImplicitSelf(0)));
			_obj = io;
			_canRead = canRead;
			_canWrite = canWrite;
			_canSeek = canSeek;
			_canFlush = canFlush;
			_canBeClosed = canBeClosed;
			_buffer = new byte[bufferSize];
			_writePos = 0;
			_readPos = 0;
			_readLen = 0;
		}

		public override void Flush()
		{
			FlushWrite();
			FlushRead();
		}

		public void Flush(UnaryOpStorage flushStorage, RubyContext context)
		{
			Flush();
			if (_canFlush)
			{
				CallSite<Func<CallSite, object, object>> callSite = flushStorage.GetCallSite("flush");
				callSite.Target(callSite, _obj);
			}
		}

		private void FlushWrite()
		{
			if (_writePos > 0)
			{
				WriteToObject();
			}
		}

		private void FlushRead()
		{
			if (_canSeek && _readPos < _readLen)
			{
				Seek(_readPos - _readLen, SeekOrigin.Current);
			}
			_readPos = 0;
			_readLen = 0;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (!_canRead)
			{
				throw new NotSupportedException();
			}
			int num = _readLen - _readPos;
			if (num == 0)
			{
				FlushWrite();
				if (count > _buffer.Length)
				{
					num = ReadFromObject(buffer, offset, count);
					_readPos = 0;
					_readLen = 0;
					return num;
				}
				num = ReadFromObject(_buffer, 0, _buffer.Length);
				if (num == 0)
				{
					return 0;
				}
				_readPos = 0;
				_readLen = num;
			}
			if (num > count)
			{
				num = count;
			}
			Buffer.BlockCopy(_buffer, _readPos, buffer, offset, num);
			_readPos += num;
			if (num < count)
			{
				int num2 = ReadFromObject(buffer, offset + num, count - num);
				num += num2;
				_readPos = 0;
				_readLen = 0;
			}
			return num;
		}

		public override int ReadByte()
		{
			if (!_canRead)
			{
				throw new NotSupportedException();
			}
			if (_readPos == _readLen)
			{
				FlushWrite();
				_readLen = ReadFromObject(_buffer, 0, _buffer.Length);
				_readPos = 0;
				if (_readLen == 0)
				{
					return -1;
				}
			}
			return _buffer[_readPos++];
		}

		private int ReadFromObject(byte[] buffer, int offset, int count)
		{
			MutableString mutableString = (MutableString)_readSite.Target(_readSite, _obj, count);
			if (mutableString == null)
			{
				return 0;
			}
			byte[] array = mutableString.ConvertToBytes();
			Buffer.BlockCopy(array, 0, buffer, offset, array.Length);
			return array.Length;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (!_canSeek)
			{
				throw new NotSupportedException();
			}
			int num = 0;
			switch (origin)
			{
			case SeekOrigin.Begin:
				num = 0;
				break;
			case SeekOrigin.Current:
				num = 1;
				break;
			case SeekOrigin.End:
				num = 2;
				break;
			}
			_seekSite.Target(_seekSite, _obj, offset, num);
			return Position;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (!_canWrite)
			{
				throw new NotSupportedException();
			}
			if (_writePos == 0)
			{
				FlushRead();
			}
			else
			{
				int num = _buffer.Length - _writePos;
				if (num > 0)
				{
					if (num > count)
					{
						num = count;
					}
					Buffer.BlockCopy(buffer, offset, _buffer, _writePos, num);
					_writePos += num;
					if (num == count)
					{
						return;
					}
					offset += num;
					count -= num;
				}
				WriteToObject();
			}
			if (count >= _buffer.Length)
			{
				WriteToObject(buffer, offset, count);
			}
			else if (count > 0)
			{
				Buffer.BlockCopy(buffer, offset, _buffer, 0, count);
				_writePos = count;
			}
		}

		public override void WriteByte(byte value)
		{
			if (!_canWrite)
			{
				throw new NotSupportedException();
			}
			if (_writePos == 0)
			{
				FlushRead();
			}
			if (_writePos == _buffer.Length)
			{
				WriteToObject();
			}
			_buffer[_writePos++] = value;
		}

		private void WriteToObject()
		{
			WriteToObject(_buffer, 0, _writePos);
			_writePos = 0;
		}

		private void WriteToObject(byte[] buffer, int offset, int count)
		{
			MutableString mutableString = MutableString.CreateBinary(count);
			mutableString.Append(buffer, offset, count);
			_writeSite.Target(_writeSite, _obj, mutableString);
		}
	}
}
