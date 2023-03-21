using System;
using System.Collections.Generic;
using IronRuby.Builtins;
using IronRuby.Runtime;

namespace IronRuby.StandardLibrary.Zlib
{
	public sealed class ZlibLibraryInitializer : LibraryInitializer
	{
		protected override void LoadModules()
		{
			RubyClass @class = GetClass(typeof(SystemException));
			RubyClass class2 = GetClass(typeof(object));
			RubyClass class3 = GetClass(typeof(RuntimeError));
			RubyModule module = DefineGlobalModule("Zlib", typeof(Zlib), 8, null, LoadZlib_Class, LoadZlib_Constants, RubyModule.EmptyArray);
			RubyClass rubyClass = DefineClass("Zlib::Error", typeof(Zlib.Error), 8, @class, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__Zlib__Error));
			RubyClass rubyClass2 = DefineClass("Zlib::GzipFile", typeof(Zlib.GZipFile), 8, class2, LoadZlib__GzipFile_Instance, LoadZlib__GzipFile_Class, null, RubyModule.EmptyArray);
			RubyClass value = DefineClass("Zlib::GzipFile::Error", typeof(Zlib.GZipFile.Error), 8, class3, null, null, null, RubyModule.EmptyArray);
			RubyClass rubyClass3 = DefineClass("Zlib::ZStream", typeof(Zlib.ZStream), 8, class2, LoadZlib__ZStream_Instance, null, null, RubyModule.EmptyArray);
			RubyClass value2 = DefineClass("Zlib::BufError", typeof(Zlib.BufError), 8, rubyClass, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__Zlib__BufError));
			RubyClass value3 = DefineClass("Zlib::DataError", typeof(Zlib.DataError), 8, rubyClass, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__Zlib__DataError));
			RubyClass value4 = DefineClass("Zlib::Deflate", typeof(Zlib.Deflate), 8, rubyClass3, LoadZlib__Deflate_Instance, LoadZlib__Deflate_Class, null, RubyModule.EmptyArray);
			RubyClass value5 = DefineClass("Zlib::GzipReader", typeof(Zlib.GZipReader), 8, rubyClass2, LoadZlib__GzipReader_Instance, LoadZlib__GzipReader_Class, LoadZlib__GzipReader_Constants, RubyModule.EmptyArray, new Func<RespondToStorage, RubyClass, object, Zlib.GZipReader>(Zlib.GZipReader.Create));
			RubyClass value6 = DefineClass("Zlib::GzipWriter", typeof(Zlib.GzipWriter), 8, rubyClass2, LoadZlib__GzipWriter_Instance, LoadZlib__GzipWriter_Class, null, RubyModule.EmptyArray, new Func<RespondToStorage, RubyClass, object, int, int, Zlib.GzipWriter>(Zlib.GzipWriter.Create));
			RubyClass value7 = DefineClass("Zlib::Inflate", typeof(Zlib.Inflate), 8, rubyClass3, LoadZlib__Inflate_Instance, LoadZlib__Inflate_Class, null, RubyModule.EmptyArray);
			RubyClass value8 = DefineClass("Zlib::StreamError", typeof(Zlib.StreamError), 8, rubyClass, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__Zlib__StreamError));
			LibraryInitializer.SetConstant(module, "Error", rubyClass);
			LibraryInitializer.SetConstant(module, "GzipFile", rubyClass2);
			LibraryInitializer.SetConstant(rubyClass2, "Error", value);
			LibraryInitializer.SetConstant(module, "ZStream", rubyClass3);
			LibraryInitializer.SetConstant(module, "BufError", value2);
			LibraryInitializer.SetConstant(module, "DataError", value3);
			LibraryInitializer.SetConstant(module, "Deflate", value4);
			LibraryInitializer.SetConstant(module, "GzipReader", value5);
			LibraryInitializer.SetConstant(module, "GzipWriter", value6);
			LibraryInitializer.SetConstant(module, "Inflate", value7);
			LibraryInitializer.SetConstant(module, "StreamError", value8);
		}

		private static void LoadZlib_Constants(RubyModule module)
		{
			LibraryInitializer.SetConstant(module, "ASCII", 1);
			LibraryInitializer.SetConstant(module, "BEST_COMPRESSION", 9);
			LibraryInitializer.SetConstant(module, "BEST_SPEED", 1);
			LibraryInitializer.SetConstant(module, "BINARY", 0);
			LibraryInitializer.SetConstant(module, "DEFAULT_COMPRESSION", -1);
			LibraryInitializer.SetConstant(module, "DEFAULT_STRATEGY", 0);
			LibraryInitializer.SetConstant(module, "FILTERED", 1);
			LibraryInitializer.SetConstant(module, "FINISH", 4);
			LibraryInitializer.SetConstant(module, "FIXLCODES", 288);
			LibraryInitializer.SetConstant(module, "FULL_FLUSH", 3);
			LibraryInitializer.SetConstant(module, "HUFFMAN_ONLY", 2);
			LibraryInitializer.SetConstant(module, "MAX_WBITS", 15);
			LibraryInitializer.SetConstant(module, "MAXBITS", 15);
			LibraryInitializer.SetConstant(module, "MAXCODES", 316);
			LibraryInitializer.SetConstant(module, "MAXDCODES", 30);
			LibraryInitializer.SetConstant(module, "MAXLCODES", 286);
			LibraryInitializer.SetConstant(module, "NO_COMPRESSION", 0);
			LibraryInitializer.SetConstant(module, "NO_FLUSH", 0);
			LibraryInitializer.SetConstant(module, "SYNC_FLUSH", 2);
			LibraryInitializer.SetConstant(module, "UNKNOWN", 2);
			LibraryInitializer.SetConstant(module, "VERSION", Zlib.VERSION);
			LibraryInitializer.SetConstant(module, "Z_DEFLATED", 8);
			LibraryInitializer.SetConstant(module, "ZLIB_VERSION", Zlib.ZLIB_VERSION);
		}

		private static void LoadZlib_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "crc32", 33, 0u, 65536u, new Func<RubyModule, int>(Zlib.GetCrc), new Func<RubyModule, MutableString, int, object>(Zlib.GetCrc));
		}

		private static void LoadZlib__Deflate_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "deflate", 17, 65538u, new Func<Zlib.Deflate, MutableString, int, MutableString>(Zlib.Deflate.DeflateString));
		}

		private static void LoadZlib__Deflate_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "deflate", 33, 65538u, new Func<RubyClass, MutableString, MutableString>(Zlib.Deflate.DeflateString));
		}

		private static void LoadZlib__GzipFile_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "closed?", 17, 0u, new Func<Zlib.GZipFile, bool>(Zlib.GZipFile.IsClosed));
			LibraryInitializer.DefineLibraryMethod(module, "comment", 17, 0u, new Func<Zlib.GZipFile, MutableString>(Zlib.GZipFile.Comment));
			LibraryInitializer.DefineLibraryMethod(module, "orig_name", 17, 0u, new Func<Zlib.GZipFile, MutableString>(Zlib.GZipFile.OriginalName));
			LibraryInitializer.DefineLibraryMethod(module, "original_name", 17, 0u, new Func<Zlib.GZipFile, MutableString>(Zlib.GZipFile.OriginalName));
		}

		private static void LoadZlib__GzipFile_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "wrap", 33, 0u, new Func<BinaryOpStorage, UnaryOpStorage, UnaryOpStorage, BlockParam, RubyClass, object, object>(Zlib.GZipFile.Wrap));
		}

		private static void LoadZlib__GzipReader_Constants(RubyModule module)
		{
			LibraryInitializer.SetConstant(module, "OSES", Zlib.GZipReader.OSES);
		}

		private static void LoadZlib__GzipReader_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "close", 17, 0u, new Func<UnaryOpStorage, RubyContext, Zlib.GZipReader, object>(Zlib.GZipReader.Close));
			LibraryInitializer.DefineLibraryMethod(module, "finish", 17, 0u, new Func<UnaryOpStorage, RubyContext, Zlib.GZipReader, object>(Zlib.GZipReader.Finish));
			LibraryInitializer.DefineLibraryMethod(module, "open", 18, 0u, new Func<Zlib.GZipReader, Zlib.GZipReader>(Zlib.GZipReader.Open));
			LibraryInitializer.DefineLibraryMethod(module, "read", 17, 0u, new Func<Zlib.GZipReader, MutableString>(Zlib.GZipReader.Read));
			LibraryInitializer.DefineLibraryMethod(module, "xtra_field", 17, 0u, new Func<Zlib.GZipReader, MutableString>(Zlib.GZipReader.ExtraField));
		}

		private static void LoadZlib__GzipReader_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "open", 33, 131076u, 262154u, new Func<RespondToStorage, RubyClass, MutableString, Zlib.GZipReader>(Zlib.GZipReader.Open), new Func<RespondToStorage, BlockParam, RubyClass, MutableString, object>(Zlib.GZipReader.Open));
		}

		private static void LoadZlib__GzipWriter_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "<<", 17, 262152u, new Func<ConversionStorage<MutableString>, RubyContext, Zlib.GzipWriter, MutableString, Zlib.GzipWriter>(Zlib.GzipWriter.Output));
			LibraryInitializer.DefineLibraryMethod(module, "close", 17, 0u, new Func<UnaryOpStorage, RubyContext, Zlib.GzipWriter, object>(Zlib.GzipWriter.Close));
			LibraryInitializer.DefineLibraryMethod(module, "comment=", 17, 2u, new Func<Zlib.GzipWriter, MutableString, MutableString>(Zlib.GzipWriter.Comment));
			LibraryInitializer.DefineLibraryMethod(module, "finish", 17, 0u, new Func<UnaryOpStorage, RubyContext, Zlib.GzipWriter, object>(Zlib.GzipWriter.Finish));
			LibraryInitializer.DefineLibraryMethod(module, "flush", 17, 0u, 0u, new Func<UnaryOpStorage, RubyContext, Zlib.GzipWriter, object, Zlib.GzipWriter>(Zlib.GzipWriter.Flush), new Func<UnaryOpStorage, RubyContext, Zlib.GzipWriter, int, Zlib.GzipWriter>(Zlib.GzipWriter.Flush));
			LibraryInitializer.DefineLibraryMethod(module, "orig_name=", 17, 2u, new Func<Zlib.GzipWriter, MutableString, MutableString>(Zlib.GzipWriter.OriginalName));
			LibraryInitializer.DefineLibraryMethod(module, "write", 17, 262152u, new Func<ConversionStorage<MutableString>, RubyContext, Zlib.GzipWriter, MutableString, int>(Zlib.GzipWriter.Write));
		}

		private static void LoadZlib__GzipWriter_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "open", 33, 16u, 16u, new Func<RespondToStorage, UnaryOpStorage, BlockParam, RubyClass, MutableString, int, int, object>(Zlib.GzipWriter.Open), new Func<RespondToStorage, UnaryOpStorage, BlockParam, RubyClass, MutableString, object, object, object>(Zlib.GzipWriter.Open));
		}

		private static void LoadZlib__Inflate_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "close", 17, 0u, new Func<Zlib.Inflate, MutableString>(Zlib.Inflate.Close));
			LibraryInitializer.DefineLibraryMethod(module, "inflate", 17, 65538u, new Func<Zlib.Inflate, MutableString, MutableString>(Zlib.Inflate.InflateString));
		}

		private static void LoadZlib__Inflate_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "inflate", 33, 65538u, new Func<RubyClass, MutableString, MutableString>(Zlib.Inflate.InflateString));
		}

		private static void LoadZlib__ZStream_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "adler", 17, 0u, new Func<Zlib.ZStream, int>(Zlib.ZStream.Adler));
			LibraryInitializer.DefineLibraryMethod(module, "avail_in", 17, 0u, new Func<Zlib.ZStream, int>(Zlib.ZStream.AvailIn));
			LibraryInitializer.DefineLibraryMethod(module, "avail_out", 17, 0u, new Func<Zlib.ZStream, int>(Zlib.ZStream.GetAvailOut));
			LibraryInitializer.DefineLibraryMethod(module, "avail_out=", 17, 0u, new Func<Zlib.ZStream, int, int>(Zlib.ZStream.SetAvailOut));
			LibraryInitializer.DefineLibraryMethod(module, "close", 17, 0u, new Func<Zlib.ZStream, bool>(Zlib.ZStream.Close));
			LibraryInitializer.DefineLibraryMethod(module, "closed?", 17, 0u, new Func<Zlib.ZStream, bool>(Zlib.ZStream.IsClosed));
			LibraryInitializer.DefineLibraryMethod(module, "data_type", 17, 0u, new Action<Zlib.ZStream>(Zlib.ZStream.DataType));
			LibraryInitializer.DefineLibraryMethod(module, "finish", 17, 0u, new Func<Zlib.ZStream, bool>(Zlib.ZStream.Close));
			LibraryInitializer.DefineLibraryMethod(module, "finished?", 17, 0u, new Func<Zlib.ZStream, bool>(Zlib.ZStream.IsClosed));
			LibraryInitializer.DefineLibraryMethod(module, "flush_next_in", 17, 0u, new Func<Zlib.ZStream, List<byte>>(Zlib.ZStream.FlushNextIn));
			LibraryInitializer.DefineLibraryMethod(module, "flush_next_out", 17, 0u, new Func<Zlib.ZStream, List<byte>>(Zlib.ZStream.FlushNextOut));
			LibraryInitializer.DefineLibraryMethod(module, "reset", 17, 0u, new Action<Zlib.ZStream>(Zlib.ZStream.Reset));
			LibraryInitializer.DefineLibraryMethod(module, "stream_end?", 17, 0u, new Func<Zlib.ZStream, bool>(Zlib.ZStream.IsClosed));
			LibraryInitializer.DefineLibraryMethod(module, "total_in", 17, 0u, new Func<Zlib.ZStream, int>(Zlib.ZStream.TotalIn));
			LibraryInitializer.DefineLibraryMethod(module, "total_out", 17, 0u, new Func<Zlib.ZStream, int>(Zlib.ZStream.TotalOut));
		}

		public static Exception ExceptionFactory__Zlib__BufError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new Zlib.BufError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__Zlib__DataError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new Zlib.DataError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__Zlib__Error(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new Zlib.Error(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__Zlib__StreamError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new Zlib.StreamError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}
	}
}
