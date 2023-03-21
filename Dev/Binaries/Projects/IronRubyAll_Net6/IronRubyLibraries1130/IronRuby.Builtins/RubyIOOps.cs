using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[Includes(new Type[] { typeof(PrintOps) }, Copy = true)]
	[RubyClass("IO", Extends = typeof(RubyIO))]
	[Includes(new Type[]
	{
		typeof(RubyFileOps.Constants),
		typeof(Enumerable)
	})]
	public class RubyIOOps
	{
		[RubyModule("WaitReadable")]
		public static class WaitReadable
		{
		}

		[RubyModule("WaitWritable")]
		public static class WaitWritable
		{
		}

		[RubyConstant]
		public const int SEEK_SET = 0;

		[RubyConstant]
		public const int SEEK_CUR = 1;

		[RubyConstant]
		public const int SEEK_END = 2;

		private const int FILE_TYPE_CHAR = 2;

		private const int STD_INPUT_HANDLE = -10;

		private const int STD_OUTPUT_HANDLE = -11;

		private const int STD_ERROR_HANDLE = -12;

		internal static Stream GetDescriptorStream(RubyContext context, int descriptor)
		{
			Stream stream = context.GetStream(descriptor);
			if (stream == null)
			{
				throw RubyExceptions.CreateEBADF();
			}
			return stream;
		}

		public static Exception NonBlockingError(RubyContext context, Exception exception, bool isRead)
		{
			RubyModule result;
			if (context.TryGetModule(isRead ? typeof(WaitReadable) : typeof(WaitWritable), out result))
			{
				ModuleOps.ExtendObject(result, exception);
			}
			return exception;
		}

		[RubyConstructor]
		public static RubyIO CreateFile(ConversionStorage<int?> toInt, ConversionStorage<IDictionary<object, object>> toHash, ConversionStorage<MutableString> toStr, RubyClass self, object descriptor, [Optional] object optionsOrMode, [DefaultProtocol] IDictionary<object, object> options)
		{
			return Reinitialize(toInt, toHash, toStr, new RubyIO(self.Context), descriptor, optionsOrMode, options);
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static RubyIO Reinitialize(ConversionStorage<int?> toInt, ConversionStorage<IDictionary<object, object>> toHash, ConversionStorage<MutableString> toStr, RubyIO self, object descriptor, [Optional] object optionsOrMode, [DefaultProtocol] IDictionary<object, object> options)
		{
			RubyContext context = self.Context;
			object param = Missing.Value;
			Protocols.TryConvertToOptions(toHash, ref options, ref optionsOrMode, ref param);
			CallSite<Func<CallSite, object, int?>> site = toInt.GetSite(ProtocolConversionAction<TryConvertToFixnumAction>.Make(toInt.Context));
			IOInfo info = default(IOInfo);
			if (optionsOrMode != Missing.Value)
			{
				int? num = site.Target(site, optionsOrMode);
				info = (num.HasValue ? new IOInfo((IOMode)num.Value) : IOInfo.Parse(context, Protocols.CastToString(toStr, optionsOrMode)));
			}
			if (options != null)
			{
				info = info.AddOptions(toStr, options);
			}
			int? num2 = site.Target(site, descriptor);
			if (!num2.HasValue)
			{
				throw RubyExceptions.CreateTypeConversionError(context.GetClassDisplayName(descriptor), "Fixnum");
			}
			Reinitialize(self, num2.Value, info);
			return self;
		}

		internal static RubyIO Reinitialize(RubyIO io, int descriptor, IOInfo info)
		{
			io.Mode = info.Mode;
			io.SetStream(GetDescriptorStream(io.Context, descriptor));
			io.SetFileDescriptor(descriptor);
			if (info.HasEncoding)
			{
				io.ExternalEncoding = info.ExternalEncoding;
				io.InternalEncoding = info.InternalEncoding;
			}
			return io;
		}

		[RubyMethod("initialize_copy", RubyMethodAttributes.PrivateInstance)]
		public static RubyIO InitializeCopy(RubyIO self, [NotNull] RubyIO source)
		{
			Stream stream = source.GetStream();
			int fileDescriptor = self.Context.DuplicateFileDescriptor(source.GetFileDescriptor());
			self.SetStream(stream);
			self.SetFileDescriptor(fileDescriptor);
			self.Mode = source.Mode;
			self.ExternalEncoding = source.ExternalEncoding;
			self.InternalEncoding = source.InternalEncoding;
			return self;
		}

		[RubyMethod("for_fd", RubyMethodAttributes.PublicSingleton)]
		public static RuleGenerator ForFileDescriptor()
		{
			return RuleGenerators.InstanceConstructor;
		}

		[RubyMethod("reopen")]
		public static RubyIO Reopen(RubyIO self, [NotNull] RubyIO source)
		{
			self.Context.RedirectFileDescriptor(self.GetFileDescriptor(), source.GetFileDescriptor());
			self.SetStream(source.GetStream());
			self.Mode = source.Mode;
			return self;
		}

		[RubyMethod("reopen")]
		public static RubyIO Reopen(ConversionStorage<MutableString> toPath, RubyIO self, object path, [Optional][DefaultProtocol][NotNull] MutableString mode)
		{
			return Reopen(toPath, self, path, (mode != null) ? IOInfo.Parse(self.Context, mode) : new IOInfo(self.Mode));
		}

		[RubyMethod("reopen")]
		public static RubyIO Reopen(ConversionStorage<MutableString> toPath, RubyIO self, object path, int mode)
		{
			return Reopen(toPath, self, path, new IOInfo((IOMode)mode));
		}

		private static RubyIO Reopen(ConversionStorage<MutableString> toPath, RubyIO io, object pathObj, IOInfo info)
		{
			MutableString mutableString = Protocols.CastToPath(toPath, pathObj);
			Stream stream = RubyFile.OpenFileStream(io.Context, mutableString.ToString(mutableString.Encoding.Encoding), info.Mode);
			io.Context.SetStream(io.GetFileDescriptor(), stream);
			io.SetStream(stream);
			io.Mode = info.Mode;
			if (info.HasEncoding)
			{
				io.ExternalEncoding = info.ExternalEncoding;
				io.InternalEncoding = info.InternalEncoding;
			}
			return io;
		}

		[RubyMethod("sysopen", RubyMethodAttributes.PublicSingleton)]
		public static int SysOpen(RubyClass self, [NotNull] MutableString path, [Optional] MutableString mode, [Optional] int perm)
		{
			if (FileTest.DirectoryExists(self.Context, path))
			{
				return -1;
			}
			RubyIO rubyIO = new RubyFile(self.Context, path.ToString(), IOModeEnum.Parse(mode));
			int fileDescriptor = rubyIO.GetFileDescriptor();
			rubyIO.Close();
			return fileDescriptor;
		}

		internal static object TryInvokeOpenBlock(RubyContext context, BlockParam block, RubyIO io)
		{
			if (block == null)
			{
				return io;
			}
			using (io)
			{
				object blockResult;
				block.Yield(io, out blockResult);
				return blockResult;
			}
		}

		[RubyMethod("open", RubyMethodAttributes.PublicSingleton)]
		public static RuleGenerator Open()
		{
			return delegate(MetaObjectBuilder metaBuilder, CallArguments args, string name)
			{
				RubyClass rubyClass = (RubyClass)args.Target;
				rubyClass.BuildObjectConstructionNoFlow(metaBuilder, args, name);
				if (args.Signature.HasBlock)
				{
					metaBuilder.ControlFlowBuilder = null;
					if (metaBuilder.BfcVariable == null)
					{
						metaBuilder.BfcVariable = metaBuilder.GetTemporary(typeof(BlockParam), "#bfc");
					}
					metaBuilder.Result = Expression.Call(new Func<UnaryOpStorage, BlockParam, object, object>(InvokeOpenBlock).Method, Expression.Constant(new UnaryOpStorage(args.RubyContext)), metaBuilder.BfcVariable, metaBuilder.Result);
					RubyMethodGroupBase.RuleControlFlowBuilder(metaBuilder, args);
				}
				else
				{
					metaBuilder.BuildControlFlow(args);
				}
			};
		}

		public static object InvokeOpenBlock(UnaryOpStorage closeStorage, BlockParam block, object obj)
		{
			object blockResult = obj;
			if (!RubyOps.IsRetrySingleton(obj) && block != null)
			{
				try
				{
					block.Yield(obj, out blockResult);
					return blockResult;
				}
				finally
				{
					try
					{
						CallSite<Func<CallSite, object, object>> callSite = closeStorage.GetCallSite("close");
						callSite.Target(callSite, obj);
					}
					catch (SystemException)
					{
					}
				}
			}
			return blockResult;
		}

		[RubyMethod("pipe", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		public static RubyArray OpenPipe(RubyClass self)
		{
			Stream reader;
			Stream writer;
			RubyPipe.CreatePipe(out reader, out writer);
			RubyArray rubyArray = new RubyArray(2);
			rubyArray.Add(new RubyIO(self.Context, reader, IOMode.ReadOnly));
			rubyArray.Add(new RubyIO(self.Context, writer, IOMode.WriteOnly));
			return rubyArray;
		}

		[RubyMethod("popen", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		public static object OpenPipe(RubyContext context, BlockParam block, RubyClass self, [DefaultProtocol][NotNull] MutableString command, [Optional][NotNull][DefaultProtocol] MutableString modeString)
		{
			return TryInvokeOpenBlock(context, block, OpenPipe(context, self, command, modeString));
		}

		[RubyMethod("popen", RubyMethodAttributes.PublicSingleton, BuildConfig = "!SILVERLIGHT")]
		public static RubyIO OpenPipe(RubyContext context, RubyClass self, [NotNull][DefaultProtocol] MutableString command, [Optional][DefaultProtocol][NotNull] MutableString modeString)
		{
			return OpenPipe(context, command, IOModeEnum.Parse(modeString));
		}

		public static RubyIO OpenPipe(RubyContext context, MutableString command, IOMode mode)
		{
			bool flag = mode.CanWrite();
			bool flag2 = mode.CanRead();
			Process process = RubyProcess.CreateProcess(context, command, flag, flag2, false);
			StreamReader reader = null;
			StreamWriter writer = null;
			if (flag2)
			{
				reader = process.StandardOutput;
			}
			if (flag)
			{
				writer = process.StandardInput;
			}
			return new RubyIO(context, reader, writer, mode);
		}

		[RubyMethod("select", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray Select(RubyContext context, object self, RubyArray read, [Optional] RubyArray write, [Optional] RubyArray error)
		{
			return SelectInternal(context, read, write, error, new TimeSpan(0, 0, 0, 0, -1));
		}

		[RubyMethod("select", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray Select(RubyContext context, object self, RubyArray read, RubyArray write, RubyArray error, int timeoutInSeconds)
		{
			if (timeoutInSeconds < 0)
			{
				throw RubyExceptions.CreateArgumentError("time interval must be positive");
			}
			return SelectInternal(context, read, write, error, new TimeSpan(0, 0, timeoutInSeconds));
		}

		[RubyMethod("select", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray Select(RubyContext context, object self, RubyArray read, RubyArray write, RubyArray error, double timeoutInSeconds)
		{
			if (timeoutInSeconds < 0.0)
			{
				throw RubyExceptions.CreateArgumentError("time interval must be positive");
			}
			return SelectInternal(context, read, write, error, TimeSpan.FromSeconds(timeoutInSeconds));
		}

		private static RubyArray SelectInternal(RubyContext context, RubyArray read, RubyArray write, RubyArray error, TimeSpan timeout)
		{
			WaitHandle[] array = null;
			if (read == null && write == null && error == null)
			{
				Thread.Sleep(timeout);
				return null;
			}
			array = GetWaitHandles(context, read, write, error);
			int num;
			try
			{
				num = WaitHandle.WaitAny(array, timeout, false);
				if (num == 258)
				{
					return null;
				}
			}
			catch (Exception ex)
			{
				throw RubyExceptions.CreateEINVAL(ex.Message, ex);
			}
			RubyArray rubyArray = new RubyArray();
			int handleIndex = 0;
			rubyArray.Add(MakeResult(array, ref handleIndex, num, read));
			rubyArray.Add(MakeResult(array, ref handleIndex, num, write));
			rubyArray.Add(MakeResult(array, ref handleIndex, num, error));
			return rubyArray;
		}

		private static RubyArray MakeResult(WaitHandle[] handles, ref int handleIndex, int signaling, RubyArray ioObjects)
		{
			RubyArray rubyArray = new RubyArray();
			if (ioObjects != null)
			{
				for (int i = 0; i < ioObjects.Count; i++)
				{
					if (handleIndex == signaling || handles[handleIndex].WaitOne(0, false))
					{
						rubyArray.Add(ioObjects[i]);
					}
					handleIndex++;
				}
			}
			return rubyArray;
		}

		private static WaitHandle[] GetWaitHandles(RubyContext context, RubyArray read, RubyArray write, RubyArray error)
		{
			WaitHandle[] array = new WaitHandle[((read != null) ? read.Count : 0) + ((write != null) ? write.Count : 0) + ((error != null) ? error.Count : 0)];
			int num = 0;
			if (read != null)
			{
				foreach (object item in read)
				{
					array[num++] = ToIo(context, item).CreateReadWaitHandle();
				}
			}
			if (write != null)
			{
				foreach (object item2 in write)
				{
					array[num++] = ToIo(context, item2).CreateWriteWaitHandle();
				}
			}
			if (error != null)
			{
				foreach (object item3 in error)
				{
					array[num++] = ToIo(context, item3).CreateErrorWaitHandle();
				}
				return array;
			}
			return array;
		}

		private static RubyIO ToIo(RubyContext context, object obj)
		{
			RubyIO rubyIO = obj as RubyIO;
			if (rubyIO == null)
			{
				throw RubyExceptions.CreateTypeConversionError(context.GetClassDisplayName(obj), "IO");
			}
			return rubyIO;
		}

		[RubyMethod("close")]
		public static void Close(RubyIO self)
		{
			if (self.Closed)
			{
				throw RubyExceptions.CreateIOError("closed stream");
			}
			self.Close();
		}

		[RubyMethod("close_read")]
		public static void CloseReader(RubyIO self)
		{
			if (self.Closed)
			{
				throw RubyExceptions.CreateIOError("closed stream");
			}
			self.CloseReader();
		}

		[RubyMethod("close_write")]
		public static void CloseWriter(RubyIO self)
		{
			if (self.Closed)
			{
				throw RubyExceptions.CreateIOError("closed stream");
			}
			self.CloseWriter();
		}

		[RubyMethod("closed?")]
		public static bool Closed(RubyIO self)
		{
			return self.Closed;
		}

		[RubyMethod("fcntl")]
		[RubyMethod("ioctl")]
		public static int FileControl(RubyIO self, [DefaultProtocol] int commandId, [Optional] MutableString arg)
		{
			return self.FileControl(commandId, (arg != null) ? arg.ConvertToBytes() : null);
		}

		[RubyMethod("fcntl")]
		[RubyMethod("ioctl")]
		public static int FileControl(RubyIO self, [DefaultProtocol] int commandId, int arg)
		{
			return self.FileControl(commandId, arg);
		}

		[RubyMethod("fsync")]
		[RubyMethod("flush")]
		public static void Flush(RubyIO self)
		{
			self.Flush();
		}

		[RubyMethod("eof")]
		[RubyMethod("eof?")]
		public static bool Eof(RubyIO self)
		{
			self.RequireReadable();
			return self.IsEndOfStream();
		}

		[RubyMethod("pid")]
		public static object Pid(RubyIO self)
		{
			return null;
		}

		[RubyMethod("fileno")]
		[RubyMethod("to_i")]
		public static int FileNo(RubyIO self)
		{
			return self.GetFileDescriptor();
		}

		[RubyMethod("binmode")]
		public static RubyIO Binmode(RubyIO self)
		{
			if (!self.Closed && self.Position == 0)
			{
				self.PreserveEndOfLines = true;
			}
			return self;
		}

		[RubyMethod("sync")]
		public static bool Sync(RubyIO self)
		{
			self.RequireOpen();
			return self.AutoFlush;
		}

		[RubyMethod("sync=")]
		public static bool Sync(RubyIO self, bool sync)
		{
			self.RequireOpen();
			self.AutoFlush = sync;
			return sync;
		}

		[RubyMethod("to_io")]
		public static RubyIO ToIO(RubyIO self)
		{
			return self;
		}

		[RubyMethod("inspect")]
		public static MutableString Inspect(RubyIO self)
		{
			MutableString mutableString = MutableString.CreateMutable(self.Context.GetIdentifierEncoding());
			mutableString.Append("#<");
			mutableString.Append(self.Context.GetClassOf(self).GetName(self.Context));
			mutableString.Append(':');
			if (self.Initialized)
			{
				switch (self.ConsoleStreamType)
				{
				case ConsoleStreamType.Input:
					mutableString.Append("<STDIN>");
					break;
				case ConsoleStreamType.Output:
					mutableString.Append("<STDOUT>");
					break;
				case ConsoleStreamType.ErrorOutput:
					mutableString.Append("<STDERR>");
					break;
				case null:
					mutableString.Append("fd ").Append(self.GetFileDescriptor().ToString(CultureInfo.InvariantCulture));
					break;
				}
			}
			else
			{
				RubyUtils.AppendFormatHexObjectId(mutableString, RubyUtils.GetObjectId(self.Context, self));
			}
			mutableString.Append('>');
			return mutableString;
		}

		[RubyMethod("isatty", BuildConfig = "!SILVERLIGHT")]
		[RubyMethod("tty?", BuildConfig = "!SILVERLIGHT")]
		public static bool IsAtty(RubyIO self)
		{
			ConsoleStreamType? consoleStreamType = self.ConsoleStreamType;
			if (!consoleStreamType.HasValue)
			{
				return self.GetStream().BaseStream == Stream.Null;
			}
			int stdHandleFd = GetStdHandleFd(consoleStreamType.Value);
			switch (Environment.OSVersion.Platform)
			{
			case PlatformID.Win32S:
			case PlatformID.Win32Windows:
			case PlatformID.Win32NT:
			case PlatformID.WinCE:
			{
				IntPtr stdHandle = GetStdHandle(stdHandleFd);
				if (stdHandle == IntPtr.Zero)
				{
					throw new Win32Exception();
				}
				return GetFileType(stdHandle) == 2;
			}
			default:
				return isatty(stdHandleFd) == 1;
			}
		}

		private static int GetStdHandleFd(ConsoleStreamType streamType)
		{
			switch (streamType)
			{
			case ConsoleStreamType.Input:
				return -10;
			case ConsoleStreamType.Output:
				return -11;
			case ConsoleStreamType.ErrorOutput:
				return -12;
			default:
				throw Assert.Unreachable;
			}
		}

		[DllImport("kernel32")]
		private static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("kernel32")]
		private static extern int GetFileType(IntPtr hFile);

		[DllImport("libc")]
		private static extern int isatty(int desc);

		[RubyMethod("external_encoding")]
		public static RubyEncoding GetExternalEncoding(RubyIO self)
		{
			return self.ExternalEncoding;
		}

		[RubyMethod("internal_encoding")]
		public static RubyEncoding GetInternalEncoding(RubyIO self)
		{
			return self.InternalEncoding;
		}

		[RubyMethod("set_encoding")]
		public static RubyIO SetEncodings(ConversionStorage<IDictionary<object, object>> toHash, ConversionStorage<MutableString> toStr, RubyIO self, object external, [Optional] object @internal, [Optional] IDictionary<object, object> options)
		{
			Protocols.TryConvertToOptions(toHash, ref options, ref external, ref @internal);
			RubyEncoding external2 = null;
			RubyEncoding internal2 = null;
			if (external != Missing.Value && external != null)
			{
				external2 = Protocols.ConvertToEncoding(toStr, external);
			}
			if (@internal != Missing.Value && external != null)
			{
				internal2 = Protocols.ConvertToEncoding(toStr, @internal);
			}
			return SetEncodings(self, external2, internal2);
		}

		[RubyMethod("set_encoding")]
		public static RubyIO SetEncodings(RubyIO self, RubyEncoding external, RubyEncoding @internal)
		{
			self.ExternalEncoding = external ?? self.Context.RubyOptions.LocaleEncoding;
			self.InternalEncoding = @internal;
			return self;
		}

		[RubyMethod("rewind")]
		public static void Rewind(RubyContext context, RubyIO self)
		{
			self.Seek(0L, SeekOrigin.Begin);
			self.LineNumber = 0;
		}

		[RubyMethod("seek")]
		public static int Seek(RubyIO self, [DefaultProtocol] IntegerValue pos, [DefaultProtocol] int seekOrigin)
		{
			self.Seek(pos.ToInt64(), RubyIO.ToSeekOrigin(seekOrigin));
			return 0;
		}

		[RubyMethod("sysseek")]
		public static object SysSeek(RubyIO self, [DefaultProtocol] IntegerValue pos, [DefaultProtocol] int seekOrigin)
		{
			self.Flush();
			self.Seek(pos.ToInt64(), RubyIO.ToSeekOrigin(seekOrigin));
			return pos.ToObject();
		}

		[RubyMethod("tell")]
		[RubyMethod("pos")]
		public static object Pos(RubyIO self)
		{
			if (self.Position <= int.MaxValue)
			{
				return (int)self.Position;
			}
			return (BigInteger)self.Position;
		}

		[RubyMethod("pos=")]
		public static void Pos(RubyIO self, [DefaultProtocol] IntegerValue pos)
		{
			self.Seek(pos.ToInt64(), SeekOrigin.Begin);
		}

		[RubyMethod("lineno")]
		public static int GetLineNumber(RubyIO self)
		{
			self.RequireOpen();
			return self.LineNumber;
		}

		[RubyMethod("lineno=")]
		public static void SetLineNumber(RubyContext context, RubyIO self, [DefaultProtocol] int value)
		{
			self.RequireOpen();
			self.LineNumber = value;
		}

		[RubyMethod("write")]
		public static int Write(RubyIO self, [NotNull] MutableString val)
		{
			int result = ((!val.IsEmpty) ? self.WriteBytes(val, 0, val.GetByteCount()) : 0);
			if (self.AutoFlush)
			{
				self.Flush();
			}
			return result;
		}

		[RubyMethod("write")]
		public static int Write(ConversionStorage<MutableString> tosConversion, RubyIO self, object obj)
		{
			return Write(self, Protocols.ConvertToString(tosConversion, obj));
		}

		[RubyMethod("syswrite")]
		public static int SysWrite(BinaryOpStorage writeStorage, ConversionStorage<MutableString> tosConversion, RubyContext context, RubyIO self, [NotNull] MutableString val)
		{
			RubyBufferedStream writableStream = self.GetWritableStream();
			if (writableStream.DataBuffered)
			{
				PrintOps.ReportWarning(writeStorage, tosConversion, MutableString.CreateAscii("syswrite for buffered IO"));
			}
			int result = Write(self, val);
			self.Flush();
			return result;
		}

		[RubyMethod("syswrite")]
		public static int SysWrite(BinaryOpStorage writeStorage, ConversionStorage<MutableString> tosConversion, RubyContext context, RubyIO self, object obj)
		{
			return SysWrite(writeStorage, tosConversion, context, self, Protocols.ConvertToString(tosConversion, obj));
		}

		[RubyMethod("write_nonblock")]
		public static int WriteNoBlock(RubyIO self, [NotNull] MutableString val)
		{
			self.RequireWritable();
			int result = -1;
			self.NonBlockingOperation(delegate
			{
				result = Write(self, val);
			}, false);
			return result;
		}

		[RubyMethod("write_nonblock")]
		public static int WriteNoBlock(ConversionStorage<MutableString> tosConversion, RubyIO self, object obj)
		{
			return Write(self, Protocols.ConvertToString(tosConversion, obj));
		}

		private static MutableString PrepareReadBuffer(RubyIO io, MutableString buffer)
		{
			if (buffer == null)
			{
				buffer = MutableString.CreateBinary();
			}
			else
			{
				buffer.Clear();
			}
			return buffer;
		}

		[RubyMethod("read")]
		public static MutableString Read(RubyIO self)
		{
			return Read(self, null);
		}

		[RubyMethod("read")]
		public static MutableString Read(RubyIO self, DynamicNull bytes, [Optional][DefaultProtocol] MutableString buffer)
		{
			buffer = PrepareReadBuffer(self, buffer);
			self.AppendBytes(buffer, int.MaxValue);
			return buffer;
		}

		[RubyMethod("read")]
		public static MutableString Read(RubyIO self, [DefaultProtocol] int bytes, [Optional][DefaultProtocol] MutableString buffer)
		{
			self.RequireReadable();
			if (bytes < 0)
			{
				throw RubyExceptions.CreateArgumentError("negative length -1 given");
			}
			buffer = PrepareReadBuffer(self, buffer);
			if (self.AppendBytes(buffer, bytes) != 0 || bytes == 0)
			{
				return buffer;
			}
			return null;
		}

		[RubyMethod("sysread")]
		public static MutableString SystemRead(RubyIO self, [DefaultProtocol] int bytes, [Optional][DefaultProtocol] MutableString buffer)
		{
			RubyBufferedStream readableStream = self.GetReadableStream();
			if (readableStream.DataBuffered)
			{
				throw RubyExceptions.CreateIOError("sysread for buffered IO");
			}
			readableStream.Flush();
			MutableString mutableString = Read(self, bytes, buffer);
			if (mutableString == null)
			{
				throw new EOFError("end of file reached");
			}
			return mutableString;
		}

		[RubyMethod("read_nonblock")]
		public static MutableString ReadNoBlock(RubyIO self, [DefaultProtocol] int bytes, [Optional][DefaultProtocol] MutableString buffer)
		{
			self.RequireReadable();
			MutableString result = null;
			self.NonBlockingOperation(delegate
			{
				result = Read(self, bytes, buffer);
			}, true);
			if (result == null)
			{
				throw new EOFError("end of file reached");
			}
			return result;
		}

		[RubyMethod("read", RubyMethodAttributes.PublicSingleton)]
		public static MutableString Read(ConversionStorage<IDictionary<object, object>> toHash, ConversionStorage<int> fixnumCast, ConversionStorage<MutableString> toPath, RubyClass self, object path, [Optional] object optionsOrLength, [Optional] object optionsOrOffset, [DefaultProtocol] IDictionary<object, object> options)
		{
			Protocols.TryConvertToOptions(toHash, ref options, ref optionsOrLength, ref optionsOrOffset);
			CallSite<Func<CallSite, object, int>> site = fixnumCast.GetSite(ProtocolConversionAction<ConvertToFixnumAction>.Make(fixnumCast.Context));
			int num = ((optionsOrLength != Missing.Value && optionsOrLength != null) ? site.Target(site, optionsOrLength) : 0);
			int num2 = ((optionsOrOffset != Missing.Value && optionsOrOffset != null) ? site.Target(site, optionsOrOffset) : 0);
			if (num2 < 0)
			{
				throw RubyExceptions.CreateEINVAL();
			}
			if (num < 0)
			{
				throw RubyExceptions.CreateArgumentError("negative length {0} given", num);
			}
			using (RubyIO rubyIO = new RubyFile(self.Context, self.Context.DecodePath(Protocols.CastToPath(toPath, path)), IOMode.ReadOnly))
			{
				if (num2 > 0)
				{
					rubyIO.Seek(num2, SeekOrigin.Begin);
				}
				if (optionsOrLength != Missing.Value && optionsOrLength != null)
				{
					return Read(rubyIO, num);
				}
				return Read(rubyIO);
			}
		}

		[RubyMethod("readchar")]
		public static int ReadChar(RubyIO self)
		{
			self.RequireReadable();
			int num = self.ReadByteNormalizeEoln();
			if (num == -1)
			{
				throw new EOFError("end of file reached");
			}
			return num;
		}

		[RubyMethod("readline")]
		public static MutableString ReadLine(RubyScope scope, RubyIO self)
		{
			return ReadLine(scope, self, scope.RubyContext.InputSeparator, -1);
		}

		[RubyMethod("readline")]
		public static MutableString ReadLine(RubyScope scope, RubyIO self, DynamicNull separator)
		{
			return ReadLine(scope, self, null, -1);
		}

		[RubyMethod("readline")]
		public static MutableString ReadLine(RubyScope scope, RubyIO self, [NotNull][DefaultProtocol] Union<MutableString, int> separatorOrLimit)
		{
			if (separatorOrLimit.IsFixnum())
			{
				return ReadLine(scope, self, scope.RubyContext.InputSeparator, separatorOrLimit.Fixnum());
			}
			return ReadLine(scope, self, separatorOrLimit.String(), -1);
		}

		[RubyMethod("readline")]
		public static MutableString ReadLine(RubyScope scope, RubyIO self, [DefaultProtocol] MutableString separator, [DefaultProtocol] int limit)
		{
			MutableString mutableString = Gets(scope, self, separator, limit);
			if (mutableString == null)
			{
				throw new EOFError("end of file reached");
			}
			return mutableString;
		}

		[RubyMethod("readlines")]
		public static RubyArray ReadLines(RubyContext context, RubyIO self)
		{
			return ReadLines(context, self, context.InputSeparator, -1);
		}

		[RubyMethod("readlines")]
		public static RubyArray ReadLines(RubyContext context, RubyIO self, DynamicNull separator)
		{
			return ReadLines(context, self, null, -1);
		}

		[RubyMethod("readlines")]
		public static RubyArray ReadLines(RubyContext context, RubyIO self, [DefaultProtocol][NotNull] Union<MutableString, int> separatorOrLimit)
		{
			if (separatorOrLimit.IsFixnum())
			{
				return ReadLines(context, self, context.InputSeparator, separatorOrLimit.Fixnum());
			}
			return ReadLines(context, self, separatorOrLimit.String(), -1);
		}

		[RubyMethod("readlines")]
		public static RubyArray ReadLines(RubyContext context, RubyIO self, [DefaultProtocol] MutableString separator, [DefaultProtocol] int limit)
		{
			RubyArray rubyArray = new RubyArray();
			MutableString item;
			while ((item = self.ReadLineOrParagraph(separator, limit)) != null)
			{
				rubyArray.Add(item);
			}
			self.LineNumber += rubyArray.Count;
			context.InputProvider.LastInputLineNumber = self.LineNumber;
			return rubyArray;
		}

		[RubyMethod("readlines", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray ReadLines(RubyClass self, [NotNull][DefaultProtocol] MutableString path, [DefaultProtocol] int limit)
		{
			return ReadLines(self, path, self.Context.InputSeparator, limit);
		}

		[RubyMethod("readlines", RubyMethodAttributes.PublicSingleton)]
		public static RubyArray ReadLines(RubyClass self, [DefaultProtocol][NotNull] MutableString path, [DefaultProtocol] MutableString separator, [DefaultProtocol] int limit)
		{
			using (RubyIO self2 = new RubyIO(self.Context, File.OpenRead(path.ConvertToString()), IOMode.ReadOnly))
			{
				return ReadLines(self.Context, self2, separator, limit);
			}
		}

		[RubyMethod("getc")]
		public static object Getc(RubyIO self)
		{
			int num = self.ReadByteNormalizeEoln();
			if (num == -1)
			{
				return null;
			}
			return ScriptingRuntimeHelpers.Int32ToObject(num);
		}

		[RubyMethod("gets")]
		public static MutableString Gets(RubyScope scope, RubyIO self)
		{
			return Gets(scope, self, scope.RubyContext.InputSeparator, -1);
		}

		[RubyMethod("gets")]
		public static MutableString Gets(RubyScope scope, RubyIO self, DynamicNull separator)
		{
			return Gets(scope, self, null, -1);
		}

		[RubyMethod("gets")]
		public static MutableString Gets(RubyScope scope, RubyIO self, [DefaultProtocol][NotNull] Union<MutableString, int> separatorOrLimit)
		{
			if (separatorOrLimit.IsFixnum())
			{
				return Gets(scope, self, scope.RubyContext.InputSeparator, separatorOrLimit.Fixnum());
			}
			return Gets(scope, self, separatorOrLimit.String(), -1);
		}

		[RubyMethod("gets")]
		public static MutableString Gets(RubyScope scope, RubyIO self, [DefaultProtocol] MutableString separator, [DefaultProtocol] int limit)
		{
			MutableString mutableString = self.ReadLineOrParagraph(separator, limit);
			if (mutableString != null)
			{
				mutableString.IsTainted = true;
			}
			scope.GetInnerMostClosureScope().LastInputLine = mutableString;
			scope.RubyContext.InputProvider.LastInputLineNumber = ++self.LineNumber;
			return mutableString;
		}

		[RubyMethod("ungetc")]
		public static void SetPreviousByte(RubyIO self, [DefaultProtocol] int b)
		{
			self.PushBack((byte)b);
		}

		[RubyMethod("foreach", RubyMethodAttributes.PublicSingleton)]
		public static void ForEach(BlockParam block, RubyClass self, [NotNull][DefaultProtocol] MutableString path, [DefaultProtocol] int limit)
		{
			ForEach(block, self, path, self.Context.InputSeparator, limit);
		}

		[RubyMethod("foreach", RubyMethodAttributes.PublicSingleton)]
		public static void ForEach(BlockParam block, RubyClass self, [DefaultProtocol][NotNull] MutableString path, [DefaultProtocol] MutableString separator, [DefaultProtocol] int limit)
		{
			using (RubyIO self2 = new RubyIO(self.Context, File.OpenRead(path.ConvertToString()), IOMode.ReadOnly))
			{
				Each(self.Context, block, self2, separator, limit);
			}
		}

		[RubyMethod("each")]
		[RubyMethod("each_line")]
		public static object Each(RubyContext context, BlockParam block, RubyIO self)
		{
			return Each(context, block, self, context.InputSeparator, -1);
		}

		[RubyMethod("each_line")]
		[RubyMethod("each")]
		public static object Each(RubyContext context, BlockParam block, RubyIO self, DynamicNull separator)
		{
			return Each(context, block, self, null, -1);
		}

		[RubyMethod("each")]
		[RubyMethod("each_line")]
		public static object Each(RubyContext context, BlockParam block, RubyIO self, [NotNull][DefaultProtocol] Union<MutableString, int> separatorOrLimit)
		{
			if (separatorOrLimit.IsFixnum())
			{
				return Each(context, block, self, context.InputSeparator, separatorOrLimit.Fixnum());
			}
			return Each(context, block, self, separatorOrLimit.String(), -1);
		}

		[RubyMethod("each_line")]
		[RubyMethod("each")]
		public static object Each(RubyContext context, BlockParam block, RubyIO self, [DefaultProtocol] MutableString separator, [DefaultProtocol] int limit)
		{
			self.RequireReadable();
			MutableString mutableString;
			while ((mutableString = self.ReadLineOrParagraph(separator, limit)) != null)
			{
				if (block == null)
				{
					throw RubyExceptions.NoBlockGiven();
				}
				mutableString.IsTainted = true;
				context.InputProvider.LastInputLineNumber = ++self.LineNumber;
				object blockResult;
				if (block.Yield(mutableString, out blockResult))
				{
					return blockResult;
				}
			}
			return self;
		}

		[RubyMethod("each_byte")]
		public static object EachByte(BlockParam block, RubyIO self)
		{
			self.RequireReadable();
			object obj;
			while ((obj = Getc(self)) != null)
			{
				if (block == null)
				{
					throw RubyExceptions.NoBlockGiven();
				}
				object blockResult;
				if (block.Yield((int)obj, out blockResult))
				{
					return blockResult;
				}
			}
			return self;
		}

		[RubyMethod("copy_stream", RubyMethodAttributes.PublicSingleton)]
		public static object CopyStream(ConversionStorage<MutableString> toPath, ConversionStorage<int> toInt, RespondToStorage respondTo, BinaryOpStorage writeStorage, CallSiteStorage<Func<CallSite, object, object, object, object>> readStorage, RubyClass self, object src, object dst, int count, int src_offset)
		{
			if (count < -1)
			{
				throw RubyExceptions.CreateArgumentError("count should be >= -1");
			}
			if (src_offset < -1)
			{
				throw RubyExceptions.CreateArgumentError("src_offset should be >= -1");
			}
			RubyIO rubyIO = src as RubyIO;
			RubyIO rubyIO2 = dst as RubyIO;
			Stream stream = null;
			Stream stream2 = null;
			RubyContext context = toPath.Context;
			CallSite<Func<CallSite, object, object, object>> callSite = null;
			CallSite<Func<CallSite, object, object, object, object>> callSite2 = null;
			try
			{
				if (rubyIO == null || rubyIO2 == null)
				{
					CallSite<Func<CallSite, object, MutableString>> site = toPath.GetSite(ProtocolConversionAction<TryConvertToPathAction>.Make(toPath.Context));
					MutableString mutableString = site.Target(site, src);
					if (mutableString != null)
					{
						stream = new FileStream(context.DecodePath(mutableString), FileMode.Open, FileAccess.Read);
					}
					else
					{
						callSite2 = readStorage.GetCallSite("read", 2);
					}
					MutableString mutableString2 = site.Target(site, dst);
					if (mutableString2 != null)
					{
						stream2 = new FileStream(context.DecodePath(mutableString2), FileMode.Truncate);
					}
					else
					{
						callSite = writeStorage.GetCallSite("write", 1);
					}
				}
				else
				{
					stream = rubyIO.GetReadableStream();
					stream2 = rubyIO2.GetWritableStream();
				}
				if (src_offset != -1)
				{
					if (stream == null)
					{
						throw RubyExceptions.CreateArgumentError("cannot specify src_offset for non-IO");
					}
					stream.Seek(src_offset, SeekOrigin.Current);
				}
				MutableString mutableString3 = null;
				byte[] array = null;
				long num = 0L;
				long num2 = ((count < 0) ? long.MaxValue : count);
				int num3 = 16384;
				if (stream != null)
				{
					array = new byte[Math.Min(num3, num2)];
				}
				while (num2 > 0)
				{
					int num4 = (int)Math.Min(num3, num2);
					int num5;
					if (stream != null)
					{
						mutableString3 = null;
						num5 = stream.Read(array, 0, num4);
					}
					else
					{
						mutableString3 = MutableString.CreateBinary();
						num5 = Protocols.CastToFixnum(toInt, callSite2.Target(callSite2, src, num4, mutableString3));
					}
					if (num5 <= 0)
					{
						break;
					}
					if (stream2 != null)
					{
						if (mutableString3 != null)
						{
							stream2.Write(mutableString3, 0, num5);
						}
						else
						{
							stream2.Write(array, 0, num5);
						}
					}
					else
					{
						if (mutableString3 == null)
						{
							mutableString3 = MutableString.CreateBinary(num5).Append(array, 0, num5);
						}
						else
						{
							mutableString3.SetByteCount(num5);
						}
						callSite.Target(callSite, dst, mutableString3);
					}
					num += num5;
					num2 -= num5;
				}
				return Protocols.Normalize(num);
			}
			finally
			{
				if (stream != null)
				{
					stream.Close();
				}
				if (stream2 != null)
				{
					stream2.Close();
				}
			}
		}

		public static IOWrapper CreateIOWrapper(RespondToStorage respondToStorage, object io, FileAccess access)
		{
			return CreateIOWrapper(respondToStorage, io, access, 4096);
		}

		public static IOWrapper CreateIOWrapper(RespondToStorage respondToStorage, object io, FileAccess access, int bufferSize)
		{
			bool canRead = (access == FileAccess.Read || access == FileAccess.ReadWrite) && Protocols.RespondTo(respondToStorage, io, "read");
			bool canWrite = (access == FileAccess.Write || access == FileAccess.ReadWrite) && Protocols.RespondTo(respondToStorage, io, "write");
			bool canSeek = Protocols.RespondTo(respondToStorage, io, "seek") && Protocols.RespondTo(respondToStorage, io, "tell");
			bool canFlush = Protocols.RespondTo(respondToStorage, io, "flush");
			bool canBeClosed = Protocols.RespondTo(respondToStorage, io, "close");
			return new IOWrapper(respondToStorage.Context, io, canRead, canWrite, canSeek, canFlush, canBeClosed, bufferSize);
		}
	}
}
