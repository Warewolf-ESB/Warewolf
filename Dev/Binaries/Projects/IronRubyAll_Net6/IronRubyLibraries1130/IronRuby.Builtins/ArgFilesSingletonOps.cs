using System;
using System.Runtime.InteropServices;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubySingleton(BuildConfig = "!SILVERLIGHT")]
	[RubyConstant("ARGF")]
	[Includes(new Type[] { typeof(Enumerable) })]
	public static class ArgFilesSingletonOps
	{
		[RubyMethod("fileno")]
		[RubyMethod("to_i")]
		public static int FileNo(RubyContext context, object self)
		{
			return RubyIOOps.FileNo(context.InputProvider.GetCurrentStream());
		}

		[RubyMethod("file")]
		[RubyMethod("to_io")]
		public static RubyIO ToIO(RubyContext context, object self)
		{
			return RubyIOOps.ToIO(context.InputProvider.GetCurrentStream());
		}

		[RubyMethod("to_s")]
		public static MutableString ToS(RubyContext context, object self)
		{
			return MutableString.CreateAscii("ARGF");
		}

		[RubyMethod("to_a")]
		public static RubyArray TOA(RubyContext context, object self)
		{
			RubyArray rubyArray = new RubyArray();
			while (context.InputProvider.HasMoreFiles())
			{
				RubyArray rubyArray2 = RubyIOOps.ReadLines(context, context.InputProvider.GetOrResetCurrentStream(), -1);
				foreach (object item in rubyArray2)
				{
					rubyArray.Add(item);
				}
			}
			return rubyArray;
		}

		[RubyMethod("tell")]
		[RubyMethod("pos")]
		public static object Pos(RubyContext context, object self)
		{
			return RubyIOOps.Pos(context.InputProvider.GetCurrentStream());
		}

		[RubyMethod("pos=")]
		public static void Pos(RubyContext context, object self, [DefaultProtocol] IntegerValue pos)
		{
			RubyIOOps.Pos(context.InputProvider.GetCurrentStream(), pos);
		}

		[RubyMethod("lineno=")]
		public static void SetLineNumber(RubyContext context, object self, [DefaultProtocol] int value)
		{
			RubyIOOps.SetLineNumber(context, context.InputProvider.GetCurrentStream(), value);
		}

		[RubyMethod("lineno")]
		public static int GetLineNumber(RubyContext context, object self)
		{
			return RubyIOOps.GetLineNumber(context.InputProvider.GetCurrentStream());
		}

		[RubyMethod("rewind")]
		public static void Rewind(RubyContext context, object self)
		{
			RubyIOOps.Rewind(context, context.InputProvider.GetCurrentStream());
		}

		[RubyMethod("seek")]
		public static int Seek(RubyContext context, object self, [DefaultProtocol] IntegerValue pos, [DefaultProtocol] int seekOrigin)
		{
			return RubyIOOps.Seek(context.InputProvider.GetCurrentStream(), pos, seekOrigin);
		}

		[RubyMethod("skip")]
		public static void Skip(RubyContext context, object self)
		{
			context.InputProvider.IncrementCurrentFileIndex();
		}

		[RubyMethod("each")]
		[RubyMethod("each_line")]
		public static object Each(RubyContext context, BlockParam block, object self)
		{
			RubyIOOps.Each(context, block, context.InputProvider.GetOrResetCurrentStream());
			return self;
		}

		[RubyMethod("each")]
		[RubyMethod("each_line")]
		public static object Each(RubyContext context, BlockParam block, object self, DynamicNull separator)
		{
			RubyIOOps.Each(context, block, context.InputProvider.GetOrResetCurrentStream(), separator);
			return self;
		}

		[RubyMethod("each_line")]
		[RubyMethod("each")]
		public static object Each(RubyContext context, BlockParam block, object self, [NotNull][DefaultProtocol] Union<MutableString, int> separatorOrLimit)
		{
			RubyIOOps.Each(context, block, context.InputProvider.GetOrResetCurrentStream(), separatorOrLimit);
			return self;
		}

		[RubyMethod("each")]
		[RubyMethod("each_line")]
		public static object Each(RubyContext context, BlockParam block, object self, [DefaultProtocol] MutableString separator, [DefaultProtocol] int limit)
		{
			RubyIOOps.Each(context, block, context.InputProvider.GetOrResetCurrentStream(), separator, limit);
			return self;
		}

		[RubyMethod("each_byte")]
		public static object EachByte(RubyContext context, BlockParam block, object self)
		{
			RubyIOOps.EachByte(block, context.InputProvider.GetOrResetCurrentStream());
			return self;
		}

		[RubyMethod("readline")]
		public static MutableString ReadLine(RubyScope scope, object self)
		{
			return RubyIOOps.ReadLine(scope, scope.RubyContext.InputProvider.GetOrResetCurrentStream());
		}

		[RubyMethod("readline")]
		public static MutableString ReadLine(RubyScope scope, object self, DynamicNull separator)
		{
			return RubyIOOps.ReadLine(scope, scope.RubyContext.InputProvider.GetOrResetCurrentStream(), separator);
		}

		[RubyMethod("readline")]
		public static MutableString ReadLine(RubyScope scope, object self, [NotNull][DefaultProtocol] Union<MutableString, int> separatorOrLimit)
		{
			return RubyIOOps.ReadLine(scope, scope.RubyContext.InputProvider.GetOrResetCurrentStream(), separatorOrLimit);
		}

		[RubyMethod("readline")]
		public static MutableString ReadLine(RubyScope scope, object self, [DefaultProtocol] MutableString separator, [DefaultProtocol] int limit)
		{
			return RubyIOOps.ReadLine(scope, scope.RubyContext.InputProvider.GetOrResetCurrentStream(), separator, limit);
		}

		[RubyMethod("read")]
		public static MutableString Read(RubyContext context, object self)
		{
			return RubyIOOps.Read(context.InputProvider.GetOrResetCurrentStream());
		}

		[RubyMethod("read")]
		public static MutableString Read(RubyContext context, DynamicNull bytes, [Optional][DefaultProtocol] MutableString buffer)
		{
			return RubyIOOps.Read(context.InputProvider.GetOrResetCurrentStream(), bytes, buffer);
		}

		[RubyMethod("read")]
		public static MutableString Read(RubyContext context, [DefaultProtocol] int bytes, [Optional][DefaultProtocol] MutableString buffer)
		{
			return RubyIOOps.Read(context.InputProvider.GetOrResetCurrentStream(), bytes, buffer);
		}

		[RubyMethod("readchar")]
		public static int ReadChar(RubyContext context, object self)
		{
			return RubyIOOps.ReadChar(context.InputProvider.GetOrResetCurrentStream());
		}

		[RubyMethod("readlines")]
		public static RubyArray ReadLines(RubyContext context, object self)
		{
			return RubyIOOps.ReadLines(context, context.InputProvider.GetOrResetCurrentStream());
		}

		[RubyMethod("readlines")]
		public static RubyArray ReadLines(RubyContext context, object self, DynamicNull separator)
		{
			return RubyIOOps.ReadLines(context, context.InputProvider.GetOrResetCurrentStream(), separator);
		}

		[RubyMethod("readlines")]
		public static RubyArray ReadLines(RubyContext context, object self, [NotNull][DefaultProtocol] Union<MutableString, int> separatorOrLimit)
		{
			return RubyIOOps.ReadLines(context, context.InputProvider.GetOrResetCurrentStream(), separatorOrLimit);
		}

		[RubyMethod("readlines")]
		public static RubyArray ReadLines(RubyContext context, object self, [DefaultProtocol] MutableString separator, [DefaultProtocol] int limit)
		{
			return RubyIOOps.ReadLines(context, context.InputProvider.GetOrResetCurrentStream(), separator, limit);
		}

		[RubyMethod("eof")]
		[RubyMethod("eof?")]
		public static bool EoF(RubyContext context, object self)
		{
			return RubyIOOps.Eof(context.InputProvider.GetCurrentStream());
		}

		[RubyMethod("getc")]
		public static object Getc(RubyContext context, object self)
		{
			return RubyIOOps.Getc(context.InputProvider.GetOrResetCurrentStream());
		}

		[RubyMethod("gets")]
		public static MutableString Gets(RubyScope scope, object self)
		{
			return RubyIOOps.Gets(scope, scope.RubyContext.InputProvider.GetOrResetCurrentStream());
		}

		[RubyMethod("gets")]
		public static MutableString Gets(RubyScope scope, object self, DynamicNull separator)
		{
			return RubyIOOps.Gets(scope, scope.RubyContext.InputProvider.GetOrResetCurrentStream(), separator);
		}

		[RubyMethod("gets")]
		public static MutableString Gets(RubyScope scope, object self, [DefaultProtocol][NotNull] Union<MutableString, int> separatorOrLimit)
		{
			return RubyIOOps.Gets(scope, scope.RubyContext.InputProvider.GetOrResetCurrentStream(), separatorOrLimit);
		}

		[RubyMethod("gets")]
		public static MutableString Gets(RubyScope scope, object self, [DefaultProtocol] MutableString separator, [DefaultProtocol] int limit)
		{
			return RubyIOOps.Gets(scope, scope.RubyContext.InputProvider.GetOrResetCurrentStream(), separator, limit);
		}

		[RubyMethod("filename")]
		[RubyMethod("path")]
		public static MutableString GetCurrentFileName(RubyContext context, object self)
		{
			return context.InputProvider.CurrentFileName;
		}

		[RubyMethod("close")]
		public static object Close(RubyContext context, object self)
		{
			RubyIOOps.Close(context.InputProvider.GetOrResetCurrentStream());
			return self;
		}

		[RubyMethod("closed?")]
		public static bool Closed(RubyContext context, object self)
		{
			return RubyIOOps.Closed(context.InputProvider.GetCurrentStream());
		}

		[RubyMethod("binmode")]
		public static object BinMode(RubyContext context, object self)
		{
			RubyIOOps.Binmode(context.InputProvider.GetCurrentStream());
			context.InputProvider.DefaultMode = context.InputProvider.DefaultMode | IOMode.PreserveEndOfLines;
			return self;
		}
	}
}
