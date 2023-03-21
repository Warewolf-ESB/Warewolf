using System;
using System.Collections;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	[RubyModule("Print", DefineIn = typeof(IronRubyOps))]
	public static class PrintOps
	{
		[RubyMethod("<<")]
		public static object Output(BinaryOpStorage writeStorage, object self, object value)
		{
			Protocols.Write(writeStorage, self, value);
			return self;
		}

		[RubyMethod("print")]
		public static void Print(BinaryOpStorage writeStorage, RubyScope scope, object self)
		{
			Print(writeStorage, self, scope.GetInnerMostClosureScope().LastInputLine);
		}

		[RubyMethod("print")]
		public static void Print(BinaryOpStorage writeStorage, object self, params object[] args)
		{
			foreach (object value in args)
			{
				Print(writeStorage, self, value);
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
			if (val.IsEmpty)
			{
				throw RubyExceptions.CreateTypeError("can't convert String into Integer");
			}
			MutableString value = MutableString.CreateBinary(val.GetBinarySlice(0, 1));
			Protocols.Write(writeStorage, self, value);
			return val;
		}

		[RubyMethod("putc")]
		public static int Putc(BinaryOpStorage writeStorage, object self, [DefaultProtocol] int c)
		{
			MutableString value = MutableString.CreateBinary(1).Append((byte)c);
			Protocols.Write(writeStorage, self, value);
			return c;
		}

		public static MutableString ToPrintedString(ConversionStorage<MutableString> tosConversion, object obj)
		{
			if (obj == null)
			{
				return MutableString.CreateAscii("nil");
			}
			return Protocols.ConvertToString(tosConversion, obj);
		}

		[RubyMethod("puts")]
		public static void PutsEmptyLine(BinaryOpStorage writeStorage, object self)
		{
			Protocols.Write(writeStorage, self, MutableString.CreateAscii("\n"));
		}

		[RubyMethod("puts")]
		public static void Puts(BinaryOpStorage writeStorage, object self, [NotNull] MutableString str)
		{
			Protocols.Write(writeStorage, self, str);
			if (!str.EndsWith('\n'))
			{
				PutsEmptyLine(writeStorage, self);
			}
		}

		[RubyMethod("puts")]
		public static void Puts(BinaryOpStorage writeStorage, ConversionStorage<MutableString> tosConversion, ConversionStorage<IList> tryToAry, object self, [NotNull] object val)
		{
			IList list = Protocols.TryCastToArray(tryToAry, val);
			if (list != null)
			{
				IEnumerable enumerable = IListOps.EnumerateRecursively(tryToAry, list, -1, (IList _) => MutableString.CreateAscii("[...]"));
				{
					foreach (object item in enumerable ?? list)
					{
						Puts(writeStorage, self, ToPrintedString(tosConversion, item));
					}
					return;
				}
			}
			Puts(writeStorage, self, ToPrintedString(tosConversion, val));
		}

		[RubyMethod("puts")]
		public static void Puts(BinaryOpStorage writeStorage, ConversionStorage<MutableString> tosConversion, ConversionStorage<IList> tryToAry, object self, params object[] vals)
		{
			for (int i = 0; i < vals.Length; i++)
			{
				Puts(writeStorage, tosConversion, tryToAry, self, vals[i]);
			}
		}

		[RubyMethod("printf")]
		public static void PrintFormatted(StringFormatterSiteStorage storage, ConversionStorage<MutableString> stringCast, BinaryOpStorage writeStorage, object self, [DefaultProtocol][NotNull] MutableString format, params object[] args)
		{
			KernelOps.PrintFormatted(storage, stringCast, writeStorage, null, self, format, args);
		}

		internal static void ReportWarning(BinaryOpStorage writeStorage, ConversionStorage<MutableString> tosConversion, object message)
		{
			if (writeStorage.Context.Verbose != null)
			{
				object standardErrorOutput = writeStorage.Context.StandardErrorOutput;
				CallSite<Func<CallSite, object, object, object>> callSite = writeStorage.GetCallSite("write", 1);
				callSite.Target(callSite, standardErrorOutput, ToPrintedString(tosConversion, message));
				PutsEmptyLine(writeStorage, standardErrorOutput);
			}
		}
	}
}
