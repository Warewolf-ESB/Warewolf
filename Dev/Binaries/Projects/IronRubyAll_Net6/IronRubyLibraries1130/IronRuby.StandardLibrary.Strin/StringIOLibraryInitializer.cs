using System;
using System.Collections;
using IronRuby.Builtins;
using IronRuby.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronRuby.StandardLibrary.StringIO
{
	public sealed class StringIOLibraryInitializer : LibraryInitializer
	{
		protected override void LoadModules()
		{
			RubyModule module = GetModule(typeof(Enumerable));
			RubyClass @class = GetClass(typeof(object));
			DefineGlobalClass("StringIO", typeof(StringIO), 8, @class, LoadStringIO_Instance, LoadStringIO_Class, null, new RubyModule[1] { module }, new Func<RubyClass, StringIO>(StringIO.Create), new Func<RubyClass, MutableString, MutableString, StringIO>(StringIO.Create), new Func<RubyClass, MutableString, int, StringIO>(StringIO.Create));
		}

		private static void LoadStringIO_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "<<", 17, 0u, new Func<BinaryOpStorage, object, object, object>(StringIO.Output));
			LibraryInitializer.DefineLibraryMethod(module, "binmode", 17, 0u, new Func<StringIO, StringIO>(StringIO.SetBinaryMode));
			LibraryInitializer.DefineLibraryMethod(module, "close", 17, 0u, new Action<StringIO>(StringIO.Close));
			LibraryInitializer.DefineLibraryMethod(module, "close_read", 17, 0u, new Action<StringIO>(StringIO.CloseRead));
			LibraryInitializer.DefineLibraryMethod(module, "close_write", 17, 0u, new Action<StringIO>(StringIO.CloseWrite));
			LibraryInitializer.DefineLibraryMethod(module, "closed?", 17, 0u, new Func<StringIO, bool>(StringIO.IsClosed));
			LibraryInitializer.DefineLibraryMethod(module, "closed_read?", 17, 0u, new Func<StringIO, bool>(StringIO.IsClosedRead));
			LibraryInitializer.DefineLibraryMethod(module, "closed_write?", 17, 0u, new Func<StringIO, bool>(StringIO.IsClosedWrite));
			LibraryInitializer.DefineLibraryMethod(module, "each", 17, 0u, 0u, 262152u, 393216u, new Func<RubyContext, BlockParam, StringIO, object>(StringIO.EachLine), new Func<RubyContext, BlockParam, StringIO, DynamicNull, object>(StringIO.EachLine), new Func<RubyContext, BlockParam, StringIO, Union<MutableString, int>, object>(StringIO.EachLine), new Func<BlockParam, StringIO, MutableString, int, object>(StringIO.EachLine));
			LibraryInitializer.DefineLibraryMethod(module, "each_byte", 17, 0u, new Func<BlockParam, StringIO, object>(StringIO.EachByte));
			LibraryInitializer.DefineLibraryMethod(module, "each_line", 17, 0u, 0u, 262152u, 393216u, new Func<RubyContext, BlockParam, StringIO, object>(StringIO.EachLine), new Func<RubyContext, BlockParam, StringIO, DynamicNull, object>(StringIO.EachLine), new Func<RubyContext, BlockParam, StringIO, Union<MutableString, int>, object>(StringIO.EachLine), new Func<BlockParam, StringIO, MutableString, int, object>(StringIO.EachLine));
			LibraryInitializer.DefineLibraryMethod(module, "eof", 17, 0u, new Func<StringIO, bool>(StringIO.Eof));
			LibraryInitializer.DefineLibraryMethod(module, "eof?", 17, 0u, new Func<StringIO, bool>(StringIO.Eof));
			LibraryInitializer.DefineLibraryMethod(module, "fcntl", 17, 0u, new Action<StringIO>(StringIO.FileControl));
			LibraryInitializer.DefineLibraryMethod(module, "fileno", 17, 0u, new Func<StringIO, object>(StringIO.GetDescriptor));
			LibraryInitializer.DefineLibraryMethod(module, "flush", 17, 0u, new Func<StringIO, StringIO>(StringIO.Flush));
			LibraryInitializer.DefineLibraryMethod(module, "fsync", 17, 0u, new Func<StringIO, int>(StringIO.FSync));
			LibraryInitializer.DefineLibraryMethod(module, "getc", 17, 0u, new Func<StringIO, object>(StringIO.GetByte));
			LibraryInitializer.DefineLibraryMethod(module, "gets", 17, 0u, 0u, 131076u, 393216u, new Func<RubyScope, StringIO, MutableString>(StringIO.Gets), new Func<RubyScope, StringIO, DynamicNull, MutableString>(StringIO.Gets), new Func<RubyScope, StringIO, Union<MutableString, int>, MutableString>(StringIO.Gets), new Func<RubyScope, StringIO, MutableString, int, MutableString>(StringIO.Gets));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 18, 0u, 196614u, 65538u, new Func<StringIO, StringIO>(StringIO.Reinitialize), new Func<StringIO, MutableString, MutableString, StringIO>(StringIO.Reinitialize), new Func<StringIO, MutableString, int, StringIO>(StringIO.Reinitialize));
			LibraryInitializer.DefineLibraryMethod(module, "initialize_copy", 18, 8u, 6u, new Func<RespondToStorage, UnaryOpStorage, StringIO, object, StringIO>(StringIO.Reopen), new Func<RubyContext, StringIO, StringIO, StringIO>(StringIO.Reopen));
			LibraryInitializer.DefineLibraryMethod(module, "isatty", 17, 0u, new Func<StringIO, bool>(StringIO.IsConsole));
			LibraryInitializer.DefineLibraryMethod(module, "length", 17, 0u, new Func<StringIO, int>(StringIO.GetLength));
			LibraryInitializer.DefineLibraryMethod(module, "lineno", 17, 0u, new Func<StringIO, int>(StringIO.GetLineNo));
			LibraryInitializer.DefineLibraryMethod(module, "lineno=", 17, 65536u, new Action<StringIO, int>(StringIO.SetLineNo));
			LibraryInitializer.DefineLibraryMethod(module, "pid", 17, 0u, new Func<StringIO, object>(StringIO.GetDescriptor));
			LibraryInitializer.DefineLibraryMethod(module, "pos", 17, 0u, new Func<StringIO, int>(StringIO.GetPosition));
			LibraryInitializer.DefineLibraryMethod(module, "pos=", 17, 65536u, new Action<StringIO, int>(StringIO.Pos));
			LibraryInitializer.DefineLibraryMethod(module, "print", 17, 0u, 2147483648u, 0u, new Action<BinaryOpStorage, RubyScope, object>(StringIO.Print), new Action<BinaryOpStorage, object, object[]>(StringIO.Print), new Action<BinaryOpStorage, object, object>(StringIO.Print));
			LibraryInitializer.DefineLibraryMethod(module, "printf", 17, 2148007952u, new Action<StringFormatterSiteStorage, ConversionStorage<MutableString>, BinaryOpStorage, StringIO, MutableString, object[]>(StringIO.PrintFormatted));
			LibraryInitializer.DefineLibraryMethod(module, "putc", 17, 4u, 131072u, new Func<BinaryOpStorage, object, MutableString, MutableString>(StringIO.Putc), new Func<BinaryOpStorage, object, int, int>(StringIO.Putc));
			LibraryInitializer.DefineLibraryMethod(module, "puts", 17, 0u, 4u, 16u, 2147483648u, new Action<BinaryOpStorage, object>(StringIO.PutsEmptyLine), new Action<BinaryOpStorage, object, MutableString>(StringIO.Puts), new Action<BinaryOpStorage, ConversionStorage<MutableString>, ConversionStorage<IList>, object, object>(StringIO.Puts), new Action<BinaryOpStorage, ConversionStorage<MutableString>, ConversionStorage<IList>, object, object[]>(StringIO.Puts));
			LibraryInitializer.DefineLibraryMethod(module, "read", 17, 0u, 131076u, 196612u, new Func<StringIO, DynamicNull, MutableString>(StringIO.Read), new Func<StringIO, DynamicNull, MutableString, MutableString>(StringIO.Read), new Func<StringIO, int, MutableString, MutableString>(StringIO.Read));
			LibraryInitializer.DefineLibraryMethod(module, "readchar", 17, 0u, new Func<StringIO, int>(StringIO.ReadChar));
			LibraryInitializer.DefineLibraryMethod(module, "readline", 17, 0u, 0u, 131076u, 393216u, new Func<RubyScope, StringIO, MutableString>(StringIO.ReadLine), new Func<RubyScope, StringIO, DynamicNull, MutableString>(StringIO.ReadLine), new Func<RubyScope, StringIO, Union<MutableString, int>, MutableString>(StringIO.ReadLine), new Func<RubyScope, StringIO, MutableString, int, MutableString>(StringIO.ReadLine));
			LibraryInitializer.DefineLibraryMethod(module, "readlines", 17, 0u, 0u, 131076u, 196608u, new Func<RubyContext, StringIO, RubyArray>(StringIO.ReadLines), new Func<RubyContext, StringIO, DynamicNull, RubyArray>(StringIO.ReadLines), new Func<RubyContext, StringIO, Union<MutableString, int>, RubyArray>(StringIO.ReadLines), new Func<StringIO, MutableString, int, RubyArray>(StringIO.ReadLines));
			LibraryInitializer.DefineLibraryMethod(module, "reopen", 17, new uint[6] { 0u, 8u, 6u, 2u, 196614u, 65538u }, new Func<StringIO, StringIO>(StringIO.Reopen), new Func<RespondToStorage, UnaryOpStorage, StringIO, object, StringIO>(StringIO.Reopen), new Func<RubyContext, StringIO, StringIO, StringIO>(StringIO.Reopen), new Func<StringIO, MutableString, StringIO>(StringIO.Reopen), new Func<StringIO, MutableString, MutableString, StringIO>(StringIO.Reopen), new Func<StringIO, MutableString, int, StringIO>(StringIO.Reopen));
			LibraryInitializer.DefineLibraryMethod(module, "rewind", 17, 0u, new Func<StringIO, int>(StringIO.Rewind));
			LibraryInitializer.DefineLibraryMethod(module, "seek", 17, 196608u, new Func<StringIO, int, int, int>(StringIO.Seek));
			LibraryInitializer.DefineLibraryMethod(module, "size", 17, 0u, new Func<StringIO, int>(StringIO.GetLength));
			LibraryInitializer.DefineLibraryMethod(module, "string", 17, 0u, new Func<StringIO, MutableString>(StringIO.GetString));
			LibraryInitializer.DefineLibraryMethod(module, "string=", 17, 65538u, new Func<StringIO, MutableString, MutableString>(StringIO.SetString));
			LibraryInitializer.DefineLibraryMethod(module, "sync", 17, 0u, new Func<StringIO, bool>(StringIO.Sync));
			LibraryInitializer.DefineLibraryMethod(module, "sync=", 17, 0u, new Func<StringIO, bool, bool>(StringIO.SetSync));
			LibraryInitializer.DefineLibraryMethod(module, "sysread", 17, 0u, 131076u, 196612u, new Func<StringIO, DynamicNull, MutableString>(StringIO.SystemRead), new Func<StringIO, DynamicNull, MutableString, MutableString>(StringIO.SystemRead), new Func<StringIO, int, MutableString, MutableString>(StringIO.SystemRead));
			LibraryInitializer.DefineLibraryMethod(module, "syswrite", 17, 2u, 0u, new Func<StringIO, MutableString, int>(StringIO.Write), new Func<ConversionStorage<MutableString>, StringIO, object, int>(StringIO.Write));
			LibraryInitializer.DefineLibraryMethod(module, "tell", 17, 0u, new Func<StringIO, int>(StringIO.GetPosition));
			LibraryInitializer.DefineLibraryMethod(module, "truncate", 17, 0u, new Func<ConversionStorage<int>, StringIO, object, object>(StringIO.SetLength));
			LibraryInitializer.DefineLibraryMethod(module, "tty?", 17, 0u, new Func<StringIO, bool>(StringIO.IsConsole));
			LibraryInitializer.DefineLibraryMethod(module, "ungetc", 17, 65536u, new Action<StringIO, int>(StringIO.SetPreviousByte));
			LibraryInitializer.DefineLibraryMethod(module, "write", 17, 2u, 0u, new Func<StringIO, MutableString, int>(StringIO.Write), new Func<ConversionStorage<MutableString>, StringIO, object, int>(StringIO.Write));
		}

		private static void LoadStringIO_Class(RubyModule module)
		{
			LibraryInitializer.DefineRuleGenerator(module, "open", 33, StringIO.Open());
		}
	}
}
