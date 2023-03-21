using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.StandardLibrary.StringIO
{
	[RubyClass("StringIO", Inherits = typeof(object))]
	[Includes(new Type[] { typeof(Enumerable) })]
	public class StringIO
	{
		private MutableString _content;

		private int _position;

		private IOMode _mode;

		private int _lineNumber;

		private static readonly byte[] ParagraphSeparator = new byte[2] { 10, 10 };

		public StringIO()
			: this(MutableString.CreateBinary(), IOMode.ReadWrite)
		{
		}

		public StringIO(MutableString content, IOMode mode)
		{
			ContractUtils.RequiresNotNull(content, "content");
			_content = content;
			_mode = mode;
		}

		private void SetPosition(long value)
		{
			if (value < 0 || value > int.MaxValue)
			{
				throw RubyExceptions.CreateEINVAL();
			}
			_position = (int)value;
		}

		private void SetContent(MutableString content)
		{
			_content = content;
			_position = 0;
			_lineNumber = 0;
		}

		private MutableString GetContent()
		{
			if (_mode.IsClosed())
			{
				throw RubyExceptions.CreateIOError("closed stream");
			}
			return _content;
		}

		private MutableString GetReadableContent()
		{
			if (!_mode.CanRead())
			{
				throw RubyExceptions.CreateIOError("not opened for reading");
			}
			return _content;
		}

		private MutableString GetWritableContent()
		{
			if (!_mode.CanWrite())
			{
				throw RubyExceptions.CreateIOError("not opened for writing");
			}
			return _content;
		}

		private void Close()
		{
			_mode = _mode.Close();
		}

		private static MutableString CheckContent(MutableString content, IOMode mode)
		{
			if (content.IsFrozen && mode.CanWrite())
			{
				throw Errno.CreateEACCES("Permission denied");
			}
			if ((mode & IOMode.Truncate) != 0)
			{
				content.Clear();
			}
			return content;
		}

		[RubyConstructor]
		public static StringIO Create(RubyClass self)
		{
			return new StringIO();
		}

		[RubyConstructor]
		public static StringIO Create(RubyClass self, [DefaultProtocol][NotNull] MutableString initialString, [Optional][NotNull][DefaultProtocol] MutableString mode)
		{
			IOMode mode2 = IOModeEnum.Parse(mode, (!initialString.IsFrozen) ? IOMode.ReadWrite : IOMode.ReadOnly) | IOMode.PreserveEndOfLines;
			return new StringIO(CheckContent(initialString, mode2), mode2);
		}

		[RubyConstructor]
		public static StringIO Create(RubyClass self, [DefaultProtocol][NotNull] MutableString initialString, int mode)
		{
			IOMode mode2 = (IOMode)(mode | 0x8000);
			return new StringIO(CheckContent(initialString, mode2), mode2);
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static StringIO Reinitialize(StringIO self)
		{
			self.SetContent(MutableString.CreateBinary());
			self._mode = IOMode.ReadWrite;
			return self;
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static StringIO Reinitialize(StringIO self, [DefaultProtocol][NotNull] MutableString content, [Optional][NotNull][DefaultProtocol] MutableString mode)
		{
			IOMode mode2 = IOModeEnum.Parse(mode, (!content.IsFrozen) ? IOMode.ReadWrite : IOMode.ReadOnly) | IOMode.PreserveEndOfLines;
			self.SetContent(CheckContent(content, mode2));
			self._mode = mode2;
			return self;
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static StringIO Reinitialize(StringIO self, [NotNull][DefaultProtocol] MutableString content, int mode)
		{
			IOMode mode2 = (IOMode)(mode | 0x8000);
			self.SetContent(CheckContent(content, mode2));
			self._mode = mode2;
			return self;
		}

		[RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
		public static RuleGenerator Open()
		{
			return RubyIOOps.Open();
		}

		[RubyMethod("reopen")]
		public static StringIO Reopen(StringIO self)
		{
			self.SetContent(MutableString.CreateBinary());
			self._mode = IOMode.ReadWrite;
			return self;
		}

		[RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
		[RubyMethod("reopen")]
		public static StringIO Reopen(RespondToStorage respondToStorage, UnaryOpStorage toStringIoStorage, StringIO self, [NotNull] object other)
		{
			if (!Protocols.RespondTo(respondToStorage, other, "to_strio"))
			{
				throw RubyExceptions.CreateTypeConversionError(respondToStorage.Context.GetClassName(other), "StringIO");
			}
			CallSite<Func<CallSite, object, object>> callSite = toStringIoStorage.GetCallSite("to_strio", 0);
			StringIO stringIO = callSite.Target(callSite, other) as StringIO;
			if (stringIO == null)
			{
				throw RubyExceptions.CreateTypeError("C#to_strio should return StringIO");
			}
			return Reopen(respondToStorage.Context, self, stringIO);
		}

		[RubyMethod("reopen")]
		[RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
		public static StringIO Reopen(RubyContext context, [NotNull] StringIO self, [NotNull] StringIO other)
		{
			self.SetContent(other._content);
			self._mode = other._mode;
			self._lineNumber = other._lineNumber;
			self._position = other._position;
			context.TaintObjectBy(self, other);
			return self;
		}

		[RubyMethod("reopen")]
		public static StringIO Reopen(StringIO self, [NotNull] MutableString content)
		{
			return Reopen(self, content, null);
		}

		[RubyMethod("reopen")]
		public static StringIO Reopen(StringIO self, [NotNull][DefaultProtocol] MutableString content, [NotNull][DefaultProtocol] MutableString mode)
		{
			IOMode mode2 = IOModeEnum.Parse(mode, (!content.IsFrozen) ? IOMode.ReadWrite : IOMode.ReadOnly) | IOMode.PreserveEndOfLines;
			self.SetContent(CheckContent(content, mode2));
			self._mode = mode2;
			return self;
		}

		[RubyMethod("reopen")]
		public static StringIO Reopen(StringIO self, [DefaultProtocol][NotNull] MutableString content, int mode)
		{
			IOMode mode2 = (IOMode)(mode | 0x8000);
			self.SetContent(CheckContent(content, mode2));
			self._mode = mode2;
			return self;
		}

		[RubyMethod("close")]
		public static void Close(StringIO self)
		{
			self.GetContent();
			self.Close();
		}

		[RubyMethod("close_read")]
		public static void CloseRead(StringIO self)
		{
			self.GetReadableContent();
			self._mode = self._mode.CloseRead();
		}

		[RubyMethod("close_write")]
		public static void CloseWrite(StringIO self)
		{
			self.GetWritableContent();
			self._mode = self._mode.CloseWrite();
		}

		[RubyMethod("closed?")]
		public static bool IsClosed(StringIO self)
		{
			return self._mode.IsClosed();
		}

		[RubyMethod("closed_read?")]
		public static bool IsClosedRead(StringIO self)
		{
			return !self._mode.CanRead();
		}

		[RubyMethod("closed_write?")]
		public static bool IsClosedWrite(StringIO self)
		{
			return !self._mode.CanWrite();
		}

		[RubyMethod("size")]
		[RubyMethod("length")]
		public static int GetLength(StringIO self)
		{
			return self.GetContent().GetByteCount();
		}

		[RubyMethod("tell")]
		[RubyMethod("pos")]
		public static int GetPosition(StringIO self)
		{
			return self._position;
		}

		[RubyMethod("pos=")]
		public static void Pos(StringIO self, [DefaultProtocol] int pos)
		{
			self.SetPosition(pos);
		}

		[RubyMethod("truncate")]
		public static object SetLength(ConversionStorage<int> fixnumCast, StringIO self, object lengthObj)
		{
			int num = Protocols.CastToFixnum(fixnumCast, lengthObj);
			if (num < 0)
			{
				throw RubyExceptions.CreateEINVAL("negative length");
			}
			self.GetWritableContent().SetByteCount(num);
			return lengthObj;
		}

		[RubyMethod("rewind")]
		public static int Rewind(StringIO self)
		{
			self.GetContent();
			self._position = 0;
			self._lineNumber = 0;
			return 0;
		}

		[RubyMethod("seek")]
		public static int Seek(StringIO self, [DefaultProtocol] int pos, [DefaultProtocol] int seekOrigin)
		{
			self.SetPosition(RubyIO.GetSeekPosition(self._content.GetByteCount(), self._position, pos, RubyIO.ToSeekOrigin(seekOrigin)));
			return 0;
		}

		[RubyMethod("eof?")]
		[RubyMethod("eof")]
		public static bool Eof(StringIO self)
		{
			MutableString readableContent = self.GetReadableContent();
			return self._position >= readableContent.GetByteCount();
		}

		[RubyMethod("string")]
		public static MutableString GetString(StringIO self)
		{
			return self._content;
		}

		[RubyMethod("string=")]
		public static MutableString SetString(StringIO self, [NotNull][DefaultProtocol] MutableString str)
		{
			self.SetContent(str);
			return str;
		}

		[RubyMethod("<<")]
		public static object Output(BinaryOpStorage writeStorage, object self, object value)
		{
			return PrintOps.Output(writeStorage, self, value);
		}

		[RubyMethod("print")]
		public static void Print(BinaryOpStorage writeStorage, RubyScope scope, object self)
		{
			Print(writeStorage, self, scope.GetInnerMostClosureScope().LastInputLine);
		}

		[RubyMethod("print")]
		public static void Print(BinaryOpStorage writeStorage, object self, params object[] args)
		{
			MutableString outputSeparator = writeStorage.Context.OutputSeparator;
			foreach (object obj in args)
			{
				Protocols.Write(writeStorage, self, obj ?? MutableString.CreateAscii("nil"));
			}
			if (outputSeparator != null)
			{
				Protocols.Write(writeStorage, self, outputSeparator);
			}
		}

		[RubyMethod("print")]
		public static void Print(BinaryOpStorage writeStorage, object self, object value)
		{
			Protocols.Write(writeStorage, self, value ?? MutableString.CreateAscii("nil"));
			MutableString outputSeparator = writeStorage.Context.OutputSeparator;
			if (outputSeparator != null)
			{
				Protocols.Write(writeStorage, self, outputSeparator);
			}
		}

		[RubyMethod("putc")]
		public static MutableString Putc(BinaryOpStorage writeStorage, object self, [NotNull] MutableString val)
		{
			return PrintOps.Putc(writeStorage, self, val);
		}

		[RubyMethod("putc")]
		public static int Putc(BinaryOpStorage writeStorage, object self, [DefaultProtocol] int c)
		{
			return PrintOps.Putc(writeStorage, self, c);
		}

		[RubyMethod("puts")]
		public static void PutsEmptyLine(BinaryOpStorage writeStorage, object self)
		{
			PrintOps.PutsEmptyLine(writeStorage, self);
		}

		[RubyMethod("puts")]
		public static void Puts(BinaryOpStorage writeStorage, object self, [NotNull] MutableString str)
		{
			PrintOps.Puts(writeStorage, self, str);
		}

		[RubyMethod("puts")]
		public static void Puts(BinaryOpStorage writeStorage, ConversionStorage<MutableString> tosConversion, ConversionStorage<IList> tryToAry, object self, [NotNull] object val)
		{
			PrintOps.Puts(writeStorage, tosConversion, tryToAry, self, val);
		}

		[RubyMethod("puts")]
		public static void Puts(BinaryOpStorage writeStorage, ConversionStorage<MutableString> tosConversion, ConversionStorage<IList> tryToAry, object self, params object[] vals)
		{
			PrintOps.Puts(writeStorage, tosConversion, tryToAry, self, vals);
		}

		[RubyMethod("printf")]
		public static void PrintFormatted(StringFormatterSiteStorage storage, ConversionStorage<MutableString> stringCast, BinaryOpStorage writeStorage, StringIO self, [DefaultProtocol][NotNull] MutableString format, params object[] args)
		{
			PrintOps.PrintFormatted(storage, stringCast, writeStorage, self, format, args);
		}

		[RubyMethod("write")]
		[RubyMethod("syswrite")]
		public static int Write(StringIO self, [NotNull] MutableString value)
		{
			MutableString writableContent = self.GetWritableContent();
			int byteCount = writableContent.GetByteCount();
			int byteCount2 = value.GetByteCount();
			int num = (((self._mode & IOMode.WriteAppends) == 0) ? self._position : byteCount);
			try
			{
				writableContent.WriteBytes(num, value, 0, byteCount2);
			}
			catch (InvalidOperationException)
			{
				throw RubyExceptions.CreateIOError("not modifiable string");
			}
			writableContent.TaintBy(value);
			self._position = num + byteCount2;
			return byteCount2;
		}

		[RubyMethod("write")]
		[RubyMethod("syswrite")]
		public static int Write(ConversionStorage<MutableString> tosConversion, StringIO self, object obj)
		{
			return Write(self, Protocols.ConvertToString(tosConversion, obj));
		}

		[RubyMethod("read")]
		public static MutableString Read(StringIO self, [Optional] DynamicNull bytes)
		{
			return Read(self, null, false);
		}

		[RubyMethod("read")]
		public static MutableString Read(StringIO self, DynamicNull bytes, [DefaultProtocol][NotNull] MutableString buffer)
		{
			return Read(self, buffer, false);
		}

		public static MutableString Read(StringIO self, MutableString buffer, bool eofError)
		{
			MutableString readableContent = self.GetReadableContent();
			int position = self._position;
			int byteCount = readableContent.GetByteCount();
			if (buffer != null)
			{
				buffer.Clear();
			}
			else
			{
				buffer = MutableString.CreateBinary();
			}
			if (position < byteCount)
			{
				self._position = byteCount;
				buffer.Append(readableContent, position, byteCount - position).TaintBy(readableContent);
			}
			else if (eofError)
			{
				throw new EOFError("end of file reached");
			}
			return buffer;
		}

		[RubyMethod("read")]
		public static MutableString Read(StringIO self, [DefaultProtocol] int count, [Optional][NotNull][DefaultProtocol] MutableString buffer)
		{
			MutableString readableContent = self.GetReadableContent();
			if (count < 0)
			{
				throw RubyExceptions.CreateArgumentError("negative length -1 given");
			}
			if (buffer != null)
			{
				buffer.Clear();
			}
			int byteCount = readableContent.GetByteCount();
			if (self._position >= byteCount)
			{
				return null;
			}
			if (buffer == null)
			{
				buffer = MutableString.CreateBinary();
			}
			int num = Math.Min(count, byteCount - self._position);
			buffer.Append(readableContent, self._position, num).TaintBy(readableContent);
			self._position += num;
			return buffer;
		}

		[RubyMethod("sysread")]
		public static MutableString SystemRead(StringIO self, [Optional] DynamicNull bytes)
		{
			return Read(self, null, true);
		}

		[RubyMethod("sysread")]
		public static MutableString SystemRead(StringIO self, DynamicNull bytes, [DefaultProtocol][NotNull] MutableString buffer)
		{
			return Read(self, buffer, true);
		}

		[RubyMethod("sysread")]
		public static MutableString SystemRead(StringIO self, [DefaultProtocol] int bytes, [Optional][DefaultProtocol][NotNull] MutableString buffer)
		{
			MutableString mutableString = Read(self, bytes, buffer);
			if (mutableString == null)
			{
				throw new EOFError("end of file reached");
			}
			return mutableString;
		}

		[RubyMethod("getc")]
		public static object GetByte(StringIO self)
		{
			MutableString readableContent = self.GetReadableContent();
			if (self._position >= readableContent.GetByteCount())
			{
				return null;
			}
			return ScriptingRuntimeHelpers.Int32ToObject(readableContent.GetByte(self._position++));
		}

		[RubyMethod("ungetc")]
		public static void SetPreviousByte(StringIO self, [DefaultProtocol] int b)
		{
			MutableString readableContent = self.GetReadableContent();
			int num = self._position - 1;
			if (num < 0)
			{
				return;
			}
			int byteCount = readableContent.GetByteCount();
			try
			{
				if (num >= byteCount)
				{
					readableContent.Append(0, num - byteCount);
					readableContent.Append((byte)b);
				}
				else
				{
					readableContent.SetByte(num, (byte)b);
				}
				self._position = num;
			}
			catch (InvalidOperationException)
			{
				throw RubyExceptions.CreateIOError("not modifiable string");
			}
		}

		[RubyMethod("readchar")]
		public static int ReadChar(StringIO self)
		{
			MutableString readableContent = self.GetReadableContent();
			int byteCount = readableContent.GetByteCount();
			if (self._position >= byteCount)
			{
				throw new EOFError("end of file reached");
			}
			return readableContent.GetByte(self._position++);
		}

		[RubyMethod("lineno")]
		public static int GetLineNo(StringIO self)
		{
			return self._lineNumber;
		}

		[RubyMethod("lineno=")]
		public static void SetLineNo(StringIO self, [DefaultProtocol] int value)
		{
			self._lineNumber = value;
		}

		[RubyMethod("gets")]
		public static MutableString Gets(RubyScope scope, StringIO self)
		{
			return Gets(scope, self, scope.RubyContext.InputSeparator, -1);
		}

		[RubyMethod("gets")]
		public static MutableString Gets(RubyScope scope, StringIO self, DynamicNull separator)
		{
			return Gets(scope, self, null, -1);
		}

		[RubyMethod("gets")]
		public static MutableString Gets(RubyScope scope, StringIO self, [NotNull][DefaultProtocol] Union<MutableString, int> separatorOrLimit)
		{
			if (separatorOrLimit.IsFixnum())
			{
				return Gets(scope, self, scope.RubyContext.InputSeparator, separatorOrLimit.Fixnum());
			}
			return Gets(scope, self, separatorOrLimit.String(), -1);
		}

		[RubyMethod("gets")]
		public static MutableString Gets(RubyScope scope, StringIO self, [DefaultProtocol] MutableString separator, [DefaultProtocol] int limit)
		{
			MutableString readableContent = self.GetReadableContent();
			int position = self._position;
			MutableString mutableString = ReadLine(readableContent, separator, ref position);
			self._position = position;
			scope.GetInnerMostClosureScope().LastInputLine = mutableString;
			self._lineNumber++;
			return mutableString;
		}

		[RubyMethod("readline")]
		public static MutableString ReadLine(RubyScope scope, StringIO self)
		{
			return ReadLine(scope, self, scope.RubyContext.InputSeparator, -1);
		}

		[RubyMethod("readline")]
		public static MutableString ReadLine(RubyScope scope, StringIO self, DynamicNull separator)
		{
			return ReadLine(scope, self, null, -1);
		}

		[RubyMethod("readline")]
		public static MutableString ReadLine(RubyScope scope, StringIO self, [DefaultProtocol][NotNull] Union<MutableString, int> separatorOrLimit)
		{
			if (separatorOrLimit.IsFixnum())
			{
				return ReadLine(scope, self, scope.RubyContext.InputSeparator, separatorOrLimit.Fixnum());
			}
			return ReadLine(scope, self, separatorOrLimit.String(), -1);
		}

		[RubyMethod("readline")]
		public static MutableString ReadLine(RubyScope scope, StringIO self, [DefaultProtocol] MutableString separator, [DefaultProtocol] int limit)
		{
			MutableString mutableString = Gets(scope, self, separator, limit);
			if (mutableString == null)
			{
				throw new EOFError("end of file reached");
			}
			return mutableString;
		}

		[RubyMethod("readlines")]
		public static RubyArray ReadLines(RubyContext context, StringIO self)
		{
			return ReadLines(self, context.InputSeparator, -1);
		}

		[RubyMethod("readlines")]
		public static RubyArray ReadLines(RubyContext context, StringIO self, DynamicNull separator)
		{
			return ReadLines(self, null, -1);
		}

		[RubyMethod("readlines")]
		public static RubyArray ReadLines(RubyContext context, StringIO self, [NotNull][DefaultProtocol] Union<MutableString, int> separatorOrLimit)
		{
			if (separatorOrLimit.IsFixnum())
			{
				return ReadLines(self, context.InputSeparator, separatorOrLimit.Fixnum());
			}
			return ReadLines(self, separatorOrLimit.String(), -1);
		}

		[RubyMethod("readlines")]
		public static RubyArray ReadLines(StringIO self, [DefaultProtocol] MutableString separator, [DefaultProtocol] int limit)
		{
			MutableString readableContent = self.GetReadableContent();
			RubyArray rubyArray = new RubyArray();
			int position = self._position;
			MutableString item;
			while ((item = ReadLine(readableContent, separator, ref position)) != null)
			{
				rubyArray.Add(item);
				self._lineNumber++;
			}
			self._position = position;
			return rubyArray;
		}

		private static MutableString ReadLine(MutableString content, MutableString separator, ref int position)
		{
			int byteCount = content.GetByteCount();
			if (position >= byteCount)
			{
				return null;
			}
			int i = position;
			if (separator == null)
			{
				position = byteCount;
			}
			else if (separator.IsEmpty)
			{
				for (; i < byteCount && content.GetByte(i) == 10; i++)
				{
				}
				position = content.IndexOf(ParagraphSeparator, i);
				position = ((position != -1) ? (position + 1) : byteCount);
			}
			else
			{
				position = content.IndexOf(separator, i);
				position = ((position != -1) ? (position + separator.Length) : byteCount);
			}
			return content.GetSlice(i, position - i);
		}

		[RubyMethod("each")]
		[RubyMethod("each_line")]
		public static object EachLine(RubyContext context, BlockParam block, StringIO self)
		{
			return EachLine(block, self, context.InputSeparator, -1);
		}

		[RubyMethod("each")]
		[RubyMethod("each_line")]
		public static object EachLine(RubyContext context, BlockParam block, StringIO self, DynamicNull separator)
		{
			return EachLine(block, self, null, -1);
		}

		[RubyMethod("each_line")]
		[RubyMethod("each")]
		public static object EachLine(RubyContext context, BlockParam block, StringIO self, [DefaultProtocol][NotNull] Union<MutableString, int> separatorOrLimit)
		{
			if (separatorOrLimit.IsFixnum())
			{
				return EachLine(block, self, context.InputSeparator, separatorOrLimit.Fixnum());
			}
			return EachLine(block, self, separatorOrLimit.String(), -1);
		}

		[RubyMethod("each_line")]
		[RubyMethod("each")]
		public static object EachLine(BlockParam block, StringIO self, [DefaultProtocol] MutableString separator, [DefaultProtocol] int limit)
		{
			MutableString readableContent = self.GetReadableContent();
			object obj = MutableStringOps.EachLine(block, readableContent, separator, self._position);
			if (!object.ReferenceEquals(obj, readableContent))
			{
				return obj;
			}
			return self;
		}

		[RubyMethod("each_byte")]
		public static object EachByte(BlockParam block, StringIO self)
		{
			int position;
			MutableString readableContent;
			while ((position = self._position) < (readableContent = self.GetReadableContent()).GetByteCount())
			{
				if (block == null)
				{
					throw RubyExceptions.NoBlockGiven();
				}
				self._position++;
				object blockResult;
				if (block.Yield(ScriptingRuntimeHelpers.Int32ToObject(readableContent.GetByte(position)), out blockResult))
				{
					return blockResult;
				}
			}
			return null;
		}

		[RubyMethod("binmode")]
		public static StringIO SetBinaryMode(StringIO self)
		{
			return self;
		}

		[RubyMethod("fcntl")]
		public static void FileControl(StringIO self)
		{
			throw new NotImplementedError();
		}

		[RubyMethod("fileno")]
		[RubyMethod("pid")]
		public static object GetDescriptor(StringIO self)
		{
			return null;
		}

		[RubyMethod("fsync")]
		public static int FSync(StringIO self)
		{
			return 0;
		}

		[RubyMethod("sync")]
		public static bool Sync(StringIO self)
		{
			return true;
		}

		[RubyMethod("sync=")]
		public static bool SetSync(StringIO self, bool value)
		{
			return value;
		}

		[RubyMethod("tty?")]
		[RubyMethod("isatty")]
		public static bool IsConsole(StringIO self)
		{
			return false;
		}

		[RubyMethod("flush")]
		public static StringIO Flush(StringIO self)
		{
			return self;
		}
	}
}
