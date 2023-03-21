using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using IronRuby.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	public class RubyIO : IDisposable
	{
		[DebuggerDisplay("{_immediateClass.GetDebuggerDisplayValue(this),nq}", Type = "{_immediateClass.GetDebuggerDisplayType(),nq}")]
		[DebuggerTypeProxy(typeof(RubyObjectDebugView))]
		public sealed class Subclass : RubyIO, IRubyObject, IRubyObjectState
		{
			private RubyInstanceData _instanceData;

			private RubyClass _immediateClass;

			public RubyClass ImmediateClass
			{
				get
				{
					return _immediateClass;
				}
				set
				{
					_immediateClass = value;
				}
			}

			public bool IsFrozen
			{
				get
				{
					if (_instanceData != null)
					{
						return _instanceData.IsFrozen;
					}
					return false;
				}
			}

			public bool IsTainted
			{
				get
				{
					if (_instanceData != null)
					{
						return _instanceData.IsTainted;
					}
					return false;
				}
				set
				{
					GetInstanceData().IsTainted = value;
				}
			}

			public bool IsUntrusted
			{
				get
				{
					if (_instanceData != null)
					{
						return _instanceData.IsUntrusted;
					}
					return false;
				}
				set
				{
					GetInstanceData().IsUntrusted = value;
				}
			}

			public RubyInstanceData GetInstanceData()
			{
				return RubyOps.GetInstanceData(ref _instanceData);
			}

			public RubyInstanceData TryGetInstanceData()
			{
				return _instanceData;
			}

			public void Freeze()
			{
				GetInstanceData().Freeze();
			}

			public int BaseGetHashCode()
			{
				return base.GetHashCode();
			}

			public bool BaseEquals(object other)
			{
				return base.Equals(other);
			}

			public string BaseToString()
			{
				return ToString();
			}

			public Subclass(RubyClass rubyClass)
				: base(rubyClass.Context)
			{
				ImmediateClass = rubyClass;
			}
		}

		public const int SEEK_SET = 0;

		public const int SEEK_CUR = 1;

		public const int SEEK_END = 2;

		private RubyContext _context;

		private RubyEncoding _externalEncoding;

		private RubyEncoding _internalEncoding;

		private int _fileDescriptor;

		private RubyBufferedStream _stream;

		private bool _autoFlush;

		private IOMode _mode;

		public int LineNumber { get; set; }

		public RubyContext Context
		{
			get
			{
				return _context;
			}
		}

		public RubyEncoding ExternalEncoding
		{
			get
			{
				return _externalEncoding;
			}
			set
			{
				_externalEncoding = value;
			}
		}

		public RubyEncoding InternalEncoding
		{
			get
			{
				return _internalEncoding;
			}
			set
			{
				_internalEncoding = value;
			}
		}

		public ConsoleStreamType? ConsoleStreamType
		{
			get
			{
				RubyBufferedStream stream = GetStream();
				ConsoleStream consoleStream = stream.BaseStream as ConsoleStream;
				if (consoleStream == null)
				{
					return null;
				}
				return consoleStream.StreamType;
			}
		}

		public bool Closed
		{
			get
			{
				return _mode.IsClosed();
			}
		}

		public bool Initialized
		{
			get
			{
				if (!Closed)
				{
					return _stream != null;
				}
				return true;
			}
		}

		public bool PreserveEndOfLines
		{
			get
			{
				return (_mode & IOMode.PreserveEndOfLines) != 0;
			}
			set
			{
				if (value)
				{
					_mode |= IOMode.PreserveEndOfLines;
				}
				else
				{
					_mode &= ~IOMode.PreserveEndOfLines;
				}
			}
		}

		public bool AutoFlush
		{
			get
			{
				return _autoFlush;
			}
			set
			{
				_autoFlush = value;
			}
		}

		public long Position
		{
			get
			{
				RubyBufferedStream stream = GetStream();
				try
				{
					return stream.Position;
				}
				catch (ObjectDisposedException)
				{
					throw RubyExceptions.CreateEBADF();
				}
			}
			set
			{
				RubyBufferedStream stream = GetStream();
				try
				{
					stream.Position = value;
				}
				catch (ObjectDisposedException)
				{
					throw RubyExceptions.CreateEBADF();
				}
			}
		}

		public long Length
		{
			get
			{
				RubyBufferedStream stream = GetStream();
				try
				{
					return stream.Length;
				}
				catch (ObjectDisposedException)
				{
					throw RubyExceptions.CreateEBADF();
				}
			}
			set
			{
				RubyBufferedStream stream = GetStream();
				try
				{
					stream.SetLength(value);
				}
				catch (ObjectDisposedException)
				{
					throw RubyExceptions.CreateIOError("closed stream");
				}
				catch (NotSupportedException)
				{
					throw RubyExceptions.CreateIOError("not opened for writing");
				}
			}
		}

		public IOMode Mode
		{
			get
			{
				return _mode;
			}
			set
			{
				_mode = value;
			}
		}

		public static SeekOrigin ToSeekOrigin(int rubySeekOrigin)
		{
			switch (rubySeekOrigin)
			{
			case 0:
				return SeekOrigin.Begin;
			case 2:
				return SeekOrigin.End;
			case 1:
				return SeekOrigin.Current;
			default:
				throw RubyExceptions.CreateArgumentError("Invalid argument");
			}
		}

		public static long GetSeekPosition(long length, long position, long seekOffset, SeekOrigin origin)
		{
			switch (origin)
			{
			case SeekOrigin.Begin:
				return seekOffset;
			case SeekOrigin.End:
				return length + seekOffset;
			case SeekOrigin.Current:
				return position + seekOffset;
			default:
				throw Assert.Unreachable;
			}
		}

		public RubyIO(RubyContext context)
		{
			ContractUtils.RequiresNotNull(context, "context");
			_context = context;
			_fileDescriptor = -1;
			_stream = null;
			_externalEncoding = context.DefaultExternalEncoding;
			_internalEncoding = context.DefaultInternalEncoding;
		}

		public RubyIO(RubyContext context, Stream stream, IOMode mode)
			: this(context, stream, context.AllocateFileDescriptor(stream), mode)
		{
		}

		public RubyIO(RubyContext context, StreamReader reader, StreamWriter writer, IOMode mode)
			: this(context, new DuplexStream(reader, writer), mode)
		{
		}

		public RubyIO(RubyContext context, Stream stream, int descriptor, IOMode mode)
			: this(context)
		{
			ContractUtils.RequiresNotNull(context, "context");
			ContractUtils.RequiresNotNull(stream, "stream");
			SetStream(stream);
			_mode = mode;
			_fileDescriptor = descriptor;
		}

		public void Reset(Stream stream, IOMode mode)
		{
			_mode = mode;
			SetStream(stream);
			SetFileDescriptor(Context.AllocateFileDescriptor(stream));
		}

		public int GetFileDescriptor()
		{
			RequireOpen();
			return _fileDescriptor;
		}

		public void SetFileDescriptor(int value)
		{
			ContractUtils.Requires(value >= 0);
			RequireOpen();
			_fileDescriptor = value;
		}

		internal static bool IsConsoleDescriptor(int fileDescriptor)
		{
			if (fileDescriptor >= 0)
			{
				return fileDescriptor < 3;
			}
			return false;
		}

		public bool IsConsoleDescriptor()
		{
			return IsConsoleDescriptor(_fileDescriptor);
		}

		public RubyBufferedStream GetStream()
		{
			if (Closed)
			{
				throw RubyExceptions.CreateIOError("closed stream");
			}
			RequireInitialized();
			return _stream;
		}

		public void SetStream(Stream stream)
		{
			ContractUtils.RequiresNotNull(stream, "stream");
			_stream = new RubyBufferedStream(stream, _context.RubyOptions.Compatibility >= RubyCompatibility.Default);
		}

		public void RequireInitialized()
		{
			if (!Closed && _stream == null)
			{
				throw RubyExceptions.CreateIOError("uninitialized stream");
			}
		}

		public void RequireOpen()
		{
			GetStream();
		}

		public void RequireWritable()
		{
			GetWritableStream();
		}

		public void RequireReadable()
		{
			GetReadableStream();
		}

		public RubyBufferedStream GetWritableStream()
		{
			RubyBufferedStream stream = GetStream();
			if (!_mode.CanWrite())
			{
				throw RubyExceptions.CreateIOError("not opened for writing");
			}
			if (!stream.CanWrite)
			{
				throw RubyExceptions.CreateEBADF();
			}
			return stream;
		}

		public RubyBufferedStream GetReadableStream()
		{
			RubyBufferedStream stream = GetStream();
			if (!_mode.CanRead())
			{
				throw RubyExceptions.CreateIOError("not opened for reading");
			}
			if (!stream.CanRead)
			{
				throw RubyExceptions.CreateEBADF();
			}
			return stream;
		}

		public void Seek(long offset, SeekOrigin origin)
		{
			RubyBufferedStream stream = GetStream();
			try
			{
				stream.Seek(offset, origin);
			}
			catch (IOException)
			{
				throw RubyExceptions.CreateEINVAL();
			}
			catch (ObjectDisposedException)
			{
				throw RubyExceptions.CreateEBADF();
			}
		}

		public void Flush()
		{
			RubyBufferedStream stream = GetStream();
			try
			{
				stream.Flush();
			}
			catch (ObjectDisposedException)
			{
				throw RubyExceptions.CreateEBADF();
			}
		}

		public int WriteBytes(byte[] buffer, int index, int count)
		{
			ContractUtils.RequiresNotNull(buffer, "buffer");
			return WriteBytes(buffer, null, index, count);
		}

		public int WriteBytes(MutableString buffer, int index, int count)
		{
			ContractUtils.RequiresNotNull(buffer, "buffer");
			return WriteBytes(null, buffer, index, count);
		}

		private int WriteBytes(byte[] bytes, MutableString str, int index, int count)
		{
			RubyBufferedStream writableStream = GetWritableStream();
			if ((_mode & IOMode.WriteAppends) != 0 && writableStream.CanSeek)
			{
				writableStream.Seek(0L, SeekOrigin.End);
			}
			try
			{
				if (bytes != null)
				{
					return writableStream.WriteBytes(bytes, index, count, PreserveEndOfLines);
				}
				return writableStream.WriteBytes(str, index, count, PreserveEndOfLines);
			}
			catch (ObjectDisposedException)
			{
				throw RubyExceptions.CreateEBADF();
			}
		}

		public void Dispose()
		{
			Close();
		}

		public void Close()
		{
			int fileDescriptor = _fileDescriptor;
			_mode = _mode.Close();
			_fileDescriptor = -1;
			if (_stream != null)
			{
				_stream = null;
				_context.CloseStream(fileDescriptor);
			}
		}

		public void CloseWriter()
		{
			DuplexStream duplexStream = GetStream().BaseStream as DuplexStream;
			if ((duplexStream == null && _mode.CanRead()) || (duplexStream != null && !_mode.CanWrite()))
			{
				throw RubyExceptions.CreateIOError("closing non-duplex IO for writing");
			}
			if (duplexStream != null)
			{
				duplexStream.Writer.Close();
			}
			_mode = _mode.CloseWrite();
			if (_mode.IsClosed())
			{
				Close();
			}
		}

		public void CloseReader()
		{
			DuplexStream duplexStream = GetStream().BaseStream as DuplexStream;
			if ((duplexStream == null && _mode.CanWrite()) || (duplexStream != null && !_mode.CanRead()))
			{
				throw RubyExceptions.CreateIOError("closing non-duplex IO for reading");
			}
			if (duplexStream != null)
			{
				duplexStream.Reader.Close();
			}
			_mode = _mode.CloseRead();
			if (_mode.IsClosed())
			{
				Close();
			}
		}

		public virtual WaitHandle CreateReadWaitHandle()
		{
			throw new NotSupportedException();
		}

		public virtual WaitHandle CreateWriteWaitHandle()
		{
			throw new NotSupportedException();
		}

		public virtual WaitHandle CreateErrorWaitHandle()
		{
			throw new NotSupportedException();
		}

		public virtual int SetReadTimeout(int timeout)
		{
			if (timeout > 0)
			{
				throw RubyExceptions.CreateEBADF();
			}
			return 0;
		}

		public virtual void NonBlockingOperation(Action operation, bool isRead)
		{
			throw RubyExceptions.CreateEBADF();
		}

		public virtual int FileControl(int commandId, int arg)
		{
			GetStream();
			throw new NotSupportedException();
		}

		public virtual int FileControl(int commandId, byte[] arg)
		{
			GetStream();
			throw new NotSupportedException();
		}

		public BinaryReader GetBinaryReader()
		{
			return new BinaryReader(GetReadableStream());
		}

		public BinaryWriter GetBinaryWriter()
		{
			return new BinaryWriter(GetWritableStream());
		}

		public bool IsEndOfStream()
		{
			return GetReadableStream().PeekByte() == -1;
		}

		public int WriteBytes(char[] buffer, int index, int count)
		{
			byte[] bytes = _externalEncoding.StrictEncoding.GetBytes(buffer, index, count);
			return WriteBytes(bytes, 0, bytes.Length);
		}

		public int WriteBytes(string value)
		{
			byte[] bytes = _externalEncoding.StrictEncoding.GetBytes(value);
			return WriteBytes(bytes, 0, bytes.Length);
		}

		public int AppendBytes(MutableString buffer, int count)
		{
			RubyBufferedStream readableStream = GetReadableStream();
			try
			{
				return readableStream.AppendBytes(buffer, count, PreserveEndOfLines);
			}
			catch (ObjectDisposedException)
			{
				throw RubyExceptions.CreateEBADF();
			}
		}

		public MutableString ReadLineOrParagraph(MutableString separator, int limit)
		{
			RubyBufferedStream readableStream = GetReadableStream();
			try
			{
				return readableStream.ReadLineOrParagraph(separator, _externalEncoding, PreserveEndOfLines, (limit >= 0) ? limit : int.MaxValue);
			}
			catch (ObjectDisposedException)
			{
				throw RubyExceptions.CreateEBADF();
			}
		}

		public int ReadByteNormalizeEoln()
		{
			RubyBufferedStream readableStream = GetReadableStream();
			try
			{
				return readableStream.ReadByteNormalizeEoln(PreserveEndOfLines);
			}
			catch (ObjectDisposedException)
			{
				throw RubyExceptions.CreateEBADF();
			}
		}

		public int PeekByteNormalizeEoln()
		{
			RubyBufferedStream readableStream = GetReadableStream();
			try
			{
				return readableStream.PeekByteNormalizeEoln(PreserveEndOfLines);
			}
			catch (ObjectDisposedException)
			{
				throw RubyExceptions.CreateEBADF();
			}
		}

		public void PushBack(byte b)
		{
			GetStream().PushBack(b);
		}

		public override string ToString()
		{
			return RubyUtils.ObjectToMutableString(_context, this).ToString();
		}
	}
}
