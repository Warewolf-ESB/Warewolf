using System;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Iconv
{
	public sealed class IconvLibraryInitializer : LibraryInitializer
	{
		protected override void LoadModules()
		{
			RubyClass @class = GetClass(typeof(object));
			RubyClass class2 = GetClass(typeof(RuntimeError));
			RubyClass class3 = GetClass(typeof(ArgumentException));
			RubyClass module = DefineGlobalClass("Iconv", typeof(Iconv), 8, @class, LoadIconv_Instance, LoadIconv_Class, null, RubyModule.EmptyArray, new Func<RubyClass, MutableString, MutableString, Iconv>(Iconv.Create));
			RubyModule rubyModule = DefineModule("Iconv::Failure", typeof(Iconv.Failure), 8, null, null, null, RubyModule.EmptyArray);
			RubyClass value = DefineClass("Iconv::BrokenLibrary", typeof(Iconv.BrokenLibrary), 8, class2, null, null, null, new RubyModule[1] { rubyModule }, new Func<RubyClass, object, object, object, Iconv.BrokenLibrary>(Iconv.BrokenLibrary.Factory));
			RubyClass value2 = DefineClass("Iconv::IllegalSequence", typeof(Iconv.IllegalSequence), 8, class3, null, null, null, new RubyModule[1] { rubyModule }, new Func<RubyClass, object, object, object, Iconv.IllegalSequence>(Iconv.IllegalSequence.Factory));
			RubyClass value3 = DefineClass("Iconv::InvalidCharacter", typeof(Iconv.InvalidCharacter), 8, class3, null, null, null, new RubyModule[1] { rubyModule }, new Func<RubyClass, object, object, object, Iconv.InvalidCharacter>(Iconv.InvalidCharacter.Factory));
			RubyClass value4 = DefineClass("Iconv::InvalidEncoding", typeof(Iconv.InvalidEncoding), 8, class3, null, null, null, new RubyModule[1] { rubyModule }, new Func<RubyClass, object, object, object, Iconv.InvalidEncoding>(Iconv.InvalidEncoding.Factory));
			RubyClass value5 = DefineClass("Iconv::OutOfRange", typeof(Iconv.OutOfRange), 8, class2, null, null, null, new RubyModule[1] { rubyModule }, new Func<RubyClass, object, object, object, Iconv.OutOfRange>(Iconv.OutOfRange.Factory));
			LibraryInitializer.SetConstant(module, "Failure", rubyModule);
			LibraryInitializer.SetConstant(module, "BrokenLibrary", value);
			LibraryInitializer.SetConstant(module, "IllegalSequence", value2);
			LibraryInitializer.SetConstant(module, "InvalidCharacter", value3);
			LibraryInitializer.SetConstant(module, "InvalidEncoding", value4);
			LibraryInitializer.SetConstant(module, "OutOfRange", value5);
		}

		private static void LoadIconv_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "close", 17, 0u, new Func<Iconv, MutableString>(Iconv.Close));
			LibraryInitializer.DefineLibraryMethod(module, "iconv", 17, 196608u, 458760u, new Func<Iconv, MutableString, int, object, MutableString>(Iconv.iconv), new Func<Iconv, MutableString, int, int, MutableString>(Iconv.iconv));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 18, 393228u, new Func<RubyContext, Iconv, MutableString, MutableString, Iconv>(Iconv.Initialize));
		}

		private static void LoadIconv_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "charset_map", 33, 0u, new Func<RubyClass, Hash>(Iconv.CharsetMap));
			LibraryInitializer.DefineLibraryMethod(module, "conv", 33, 458758u, new Func<RubyClass, MutableString, MutableString, MutableString, MutableString>(Iconv.Convert));
			LibraryInitializer.DefineLibraryMethod(module, "iconv", 33, 2147680262u, new Func<RubyClass, MutableString, MutableString, MutableString[], RubyArray>(Iconv.iconv));
			LibraryInitializer.DefineLibraryMethod(module, "open", 33, 196614u, 393229u, new Func<RubyClass, MutableString, MutableString, Iconv>(Iconv.Create), new Func<BlockParam, RubyClass, MutableString, MutableString, object>(Iconv.Open));
		}

		public static Exception ExceptionFactory__Iconv__BrokenLibrary(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new Iconv.BrokenLibrary(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__Iconv__IllegalSequence(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new Iconv.IllegalSequence(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__Iconv__InvalidCharacter(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new Iconv.InvalidCharacter(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__Iconv__InvalidEncoding(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new Iconv.InvalidEncoding(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__Iconv__OutOfRange(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new Iconv.OutOfRange(RubyExceptionData.GetClrMessage(self, message), null), message);
		}
	}
}
