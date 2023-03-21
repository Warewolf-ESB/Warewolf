using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using IronRuby.Runtime;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Builtins
{
	public sealed class BuiltinsLibraryInitializer : LibraryInitializer
	{
		protected override void LoadModules()
		{
			base.Context.RegisterPrimitives(Load__MainSingleton_Instance, LoadBasicObject_Instance, LoadBasicObject_Class, null, LoadKernel_Instance, LoadKernel_Class, null, LoadObject_Instance, LoadObject_Class, LoadObject_Constants, LoadModule_Instance, LoadModule_Class, null, LoadClass_Instance, LoadClass_Class, null);
			RubyModule rubyModule = DefineGlobalModule("Comparable", typeof(Comparable), 15, LoadComparable_Instance, null, null, RubyModule.EmptyArray);
			RubyModule rubyModule2 = DefineGlobalModule("Enumerable", typeof(Enumerable), 15, LoadEnumerable_Instance, null, null, RubyModule.EmptyArray);
			RubyModule module = DefineGlobalModule("Errno", typeof(Errno), 15, null, null, null, RubyModule.EmptyArray);
			RubyModule rubyModule3 = DefineModule("File::Constants", typeof(RubyFileOps.Constants), 15, null, null, LoadFile__Constants_Constants, RubyModule.EmptyArray);
			DefineGlobalModule("FileTest", typeof(FileTest), 15, LoadFileTest_Instance, LoadFileTest_Class, null, RubyModule.EmptyArray);
			DefineGlobalModule("GC", typeof(RubyGC), 15, LoadGC_Instance, LoadGC_Class, null, RubyModule.EmptyArray);
			RubyModule value = DefineModule("IO::WaitReadable", typeof(RubyIOOps.WaitReadable), 15, null, null, null, RubyModule.EmptyArray);
			RubyModule value2 = DefineModule("IO::WaitWritable", typeof(RubyIOOps.WaitWritable), 15, null, null, null, RubyModule.EmptyArray);
			RubyModule module2 = DefineGlobalModule("IronRuby", typeof(Ruby), 4, null, LoadIronRuby_Class, null, RubyModule.EmptyArray);
			RubyModule rubyModule4 = DefineModule("IronRuby::Clr", typeof(IronRubyOps.Clr), 8, null, LoadIronRuby__Clr_Class, null, RubyModule.EmptyArray);
			RubyModule value3 = DefineModule("IronRuby::Clr::BigInteger", typeof(ClrBigInteger), 15, LoadIronRuby__Clr__BigInteger_Instance, null, null, RubyModule.EmptyArray);
			RubyModule value4 = DefineModule("IronRuby::Clr::FlagEnumeration", typeof(FlagEnumeration), 8, LoadIronRuby__Clr__FlagEnumeration_Instance, null, null, RubyModule.EmptyArray);
			RubyModule value5 = DefineModule("IronRuby::Clr::Float", typeof(ClrFloat), 15, LoadIronRuby__Clr__Float_Instance, LoadIronRuby__Clr__Float_Class, null, RubyModule.EmptyArray);
			RubyModule value6 = DefineModule("IronRuby::Clr::Integer", typeof(ClrInteger), 15, LoadIronRuby__Clr__Integer_Instance, null, null, RubyModule.EmptyArray);
			RubyModule value7 = DefineModule("IronRuby::Clr::MultiDimensionalArray", typeof(MultiDimensionalArray), 8, null, null, null, RubyModule.EmptyArray);
			RubyModule rubyModule5 = DefineModule("IronRuby::Clr::String", typeof(ClrString), 15, LoadIronRuby__Clr__String_Instance, null, null, RubyModule.EmptyArray);
			RubyModule value8 = DefineModule("IronRuby::Print", typeof(PrintOps), 15, LoadIronRuby__Print_Instance, null, null, RubyModule.EmptyArray);
			DefineGlobalModule("Marshal", typeof(RubyMarshal), 15, null, LoadMarshal_Class, LoadMarshal_Constants, RubyModule.EmptyArray);
			DefineGlobalModule("Math", typeof(RubyMath), 15, LoadMath_Instance, LoadMath_Class, LoadMath_Constants, RubyModule.EmptyArray);
			ExtendClass(typeof(TypeTracker), 0, null, LoadMicrosoft__Scripting__Actions__TypeTracker_Instance, null, null, RubyModule.EmptyArray);
			DefineGlobalModule("ObjectSpace", typeof(ObjectSpace), 15, null, LoadObjectSpace_Class, null, RubyModule.EmptyArray);
			RubyModule rubyModule6 = DefineGlobalModule("Precision", typeof(Precision), 15, LoadPrecision_Instance, LoadPrecision_Class, null, RubyModule.EmptyArray);
			RubyModule module3 = DefineGlobalModule("Process", typeof(RubyProcess), 15, LoadProcess_Instance, LoadProcess_Class, null, RubyModule.EmptyArray);
			DefineGlobalModule("Signal", typeof(Signal), 15, null, LoadSignal_Class, null, RubyModule.EmptyArray);
			ExtendClass(typeof(Type), 0, null, LoadSystem__Type_Instance, null, null, RubyModule.EmptyArray);
			object value9 = DefineSingleton(Load__Singleton_ArgFilesSingletonOps_Instance, null, null, rubyModule2);
			object value10 = DefineSingleton(Load__Singleton_EnvironmentSingletonOps_Instance, null, null, rubyModule2);
			ExtendClass(typeof(TypeGroup), 0, null, LoadMicrosoft__Scripting__Actions__TypeGroup_Instance, null, null, new RubyModule[1] { rubyModule2 });
			ExtendClass(typeof(char), 0, null, LoadSystem__Char_Instance, null, null, new RubyModule[3] { rubyModule5, rubyModule2, rubyModule }, new Func<RubyClass, int, char>(CharOps.Create), new Func<RubyClass, char, char>(CharOps.Create), new Func<RubyClass, char[], char>(CharOps.Create), new Func<RubyClass, string, char>(CharOps.Create), new Func<RubyClass, MutableString, char>(CharOps.Create));
			ExtendModule(typeof(IDictionary<object, object>), 0, LoadSystem__Collections__Generic__IDictionary_Instance, null, null, rubyModule2);
			ExtendModule(typeof(IEnumerable), 0, LoadSystem__Collections__IEnumerable_Instance, null, null, rubyModule2);
			ExtendModule(typeof(IList), 0, LoadSystem__Collections__IList_Instance, null, null, rubyModule2);
			ExtendModule(typeof(IComparable), 0, LoadSystem__IComparable_Instance, null, null, rubyModule);
			ExtendClass(typeof(string), 0, null, LoadSystem__String_Instance, null, null, new RubyModule[3] { rubyModule5, rubyModule2, rubyModule }, new Func<RubyClass, MutableString, string>(ClrStringOps.Create), new Func<RubyClass, char, int, string>(ClrStringOps.Create), new Func<RubyClass, char[], string>(ClrStringOps.Create), new Func<RubyClass, char[], int, int, string>(ClrStringOps.Create));
			DefineGlobalClass("Array", typeof(RubyArray), 7, base.Context.ObjectClass, LoadArray_Instance, LoadArray_Class, LoadArray_Constants, new RubyModule[1] { rubyModule2 }, new Func<RubyClass, RubyArray>(ArrayOps.CreateArray), new Func<ConversionStorage<Union<IList, int>>, BlockParam, RubyClass, object, object>(ArrayOps.CreateArray), new Func<BlockParam, RubyClass, int, object, RubyArray>(ArrayOps.CreateArray), new Func<RubyClass, int, object, RubyArray>(ArrayOps.CreateArray));
			DefineGlobalClass("Binding", typeof(Binding), 7, base.Context.ObjectClass, LoadBinding_Instance, LoadBinding_Class, null, RubyModule.EmptyArray);
			DefineGlobalClass("Dir", typeof(RubyDir), 15, base.Context.ObjectClass, LoadDir_Instance, LoadDir_Class, null, new RubyModule[1] { rubyModule2 }, new Func<RubyClass, MutableString, RubyDir>(RubyDir.Create));
			RubyClass module4 = DefineGlobalClass("Encoding", typeof(RubyEncoding), 7, base.Context.ObjectClass, LoadEncoding_Instance, LoadEncoding_Class, LoadEncoding_Constants, RubyModule.EmptyArray);
			DefineGlobalClass("Enumerator", typeof(Enumerator), 15, base.Context.ObjectClass, LoadEnumerator_Instance, null, null, new RubyModule[1] { rubyModule2 }, new Func<RubyClass, object, string, object[], Enumerator>(Enumerator.Create));
			RubyClass rubyClass2 = (base.Context.ExceptionClass = DefineGlobalClass("Exception", typeof(Exception), 7, base.Context.ObjectClass, LoadException_Instance, LoadException_Class, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__Exception)));
			RubyClass super = rubyClass2;
			base.Context.FalseClass = DefineGlobalClass("FalseClass", typeof(FalseClass), 15, base.Context.ObjectClass, LoadFalseClass_Instance, null, null, RubyModule.EmptyArray);
			RubyClass value11 = DefineClass("File::Stat", typeof(FileSystemInfo), 7, base.Context.ObjectClass, LoadFile__Stat_Instance, null, null, new RubyModule[1] { rubyModule }, new Func<ConversionStorage<MutableString>, RubyClass, object, FileSystemInfo>(RubyFileOps.RubyStatOps.Create));
			DefineGlobalClass("Hash", typeof(Hash), 7, base.Context.ObjectClass, LoadHash_Instance, LoadHash_Class, LoadHash_Constants, new RubyModule[1] { rubyModule2 }, new Func<RubyClass, Hash>(HashOps.CreateHash), new Func<BlockParam, RubyClass, object, Hash>(HashOps.CreateHash), new Func<BlockParam, RubyClass, Hash>(HashOps.CreateHash));
			RubyClass rubyClass3 = DefineGlobalClass("IO", typeof(RubyIO), 7, base.Context.ObjectClass, LoadIO_Instance, LoadIO_Class, LoadIO_Constants, new RubyModule[2] { rubyModule3, rubyModule2 }, new Func<ConversionStorage<int?>, ConversionStorage<IDictionary<object, object>>, ConversionStorage<MutableString>, RubyClass, object, object, IDictionary<object, object>, RubyIO>(RubyIOOps.CreateFile));
			RubyClass value12 = DefineClass("IronRuby::Clr::Name", typeof(ClrName), 7, base.Context.ObjectClass, LoadIronRuby__Clr__Name_Instance, LoadIronRuby__Clr__Name_Class, null, RubyModule.EmptyArray);
			DefineGlobalClass("MatchData", typeof(MatchData), 7, base.Context.ObjectClass, LoadMatchData_Instance, LoadMatchData_Class, null, RubyModule.EmptyArray);
			DefineGlobalClass("Method", typeof(RubyMethod), 7, base.Context.ObjectClass, LoadMethod_Instance, null, null, RubyModule.EmptyArray);
			base.Context.NilClass = DefineGlobalClass("NilClass", typeof(DynamicNull), 7, base.Context.ObjectClass, LoadNilClass_Instance, null, null, RubyModule.EmptyArray);
			RubyClass super2 = DefineGlobalClass("Numeric", typeof(Numeric), 15, base.Context.ObjectClass, LoadNumeric_Instance, null, null, new RubyModule[1] { rubyModule });
			DefineGlobalClass("Proc", typeof(Proc), 7, base.Context.ObjectClass, LoadProc_Instance, LoadProc_Class, null, RubyModule.EmptyArray, new Action<RubyClass, object[]>(ProcOps.Error));
			RubyClass value13 = DefineClass("Process::Status", typeof(RubyProcess.Status), 15, base.Context.ObjectClass, LoadProcess__Status_Instance, LoadProcess__Status_Class, null, RubyModule.EmptyArray);
			DefineGlobalClass("Range", typeof(Range), 7, base.Context.ObjectClass, LoadRange_Instance, null, null, new RubyModule[1] { rubyModule2 }, new Func<BinaryOpStorage, RubyClass, object, object, bool, Range>(RangeOps.CreateRange));
			DefineGlobalClass("Regexp", typeof(RubyRegex), 7, base.Context.ObjectClass, LoadRegexp_Instance, LoadRegexp_Class, LoadRegexp_Constants, new RubyModule[1] { rubyModule2 }, new Func<RubyClass, RubyRegex, RubyRegex>(RegexpOps.Create), new Func<RubyClass, RubyRegex, int, object, RubyRegex>(RegexpOps.Create), new Func<RubyClass, RubyRegex, object, object, RubyRegex>(RegexpOps.Create), new Func<RubyClass, MutableString, int, MutableString, RubyRegex>(RegexpOps.Create), new Func<RubyClass, MutableString, bool, MutableString, RubyRegex>(RegexpOps.Create));
			DefineGlobalClass("String", typeof(MutableString), 7, base.Context.ObjectClass, LoadString_Instance, null, null, new RubyModule[1] { rubyModule }, new Func<RubyClass, MutableString>(MutableStringOps.Create), new Func<RubyClass, MutableString, MutableString>(MutableStringOps.Create), new Func<RubyClass, byte[], MutableString>(MutableStringOps.Create));
			DefineGlobalClass("Struct", typeof(RubyStruct), 7, base.Context.ObjectClass, LoadStruct_Instance, LoadStruct_Class, LoadStruct_Constants, new RubyModule[1] { rubyModule2 }, new Action<RubyClass, object[]>(RubyStructOps.AllocatorUndefined));
			DefineGlobalClass("Symbol", typeof(RubySymbol), 7, base.Context.ObjectClass, LoadSymbol_Instance, LoadSymbol_Class, null, RubyModule.EmptyArray);
			DefineGlobalClass("Thread", typeof(Thread), 7, base.Context.ObjectClass, LoadThread_Instance, LoadThread_Class, null, RubyModule.EmptyArray);
			DefineGlobalClass("ThreadGroup", typeof(ThreadGroup), 15, base.Context.ObjectClass, LoadThreadGroup_Instance, null, LoadThreadGroup_Constants, RubyModule.EmptyArray);
			DefineGlobalClass("Time", typeof(RubyTime), 7, base.Context.ObjectClass, LoadTime_Instance, LoadTime_Class, null, new RubyModule[1] { rubyModule }, new Func<RubyClass, RubyTime>(RubyTimeOps.Create));
			base.Context.TrueClass = DefineGlobalClass("TrueClass", typeof(TrueClass), 15, base.Context.ObjectClass, LoadTrueClass_Instance, null, null, RubyModule.EmptyArray);
			DefineGlobalClass("UnboundMethod", typeof(UnboundMethod), 15, base.Context.ObjectClass, LoadUnboundMethod_Instance, null, null, RubyModule.EmptyArray);
			RubyClass module5 = DefineGlobalClass("File", typeof(RubyFile), 7, rubyClass3, LoadFile_Instance, LoadFile_Class, LoadFile_Constants, RubyModule.EmptyArray, new Func<ConversionStorage<int?>, ConversionStorage<IDictionary<object, object>>, ConversionStorage<MutableString>, ConversionStorage<MutableString>, RubyClass, object, object, object, IDictionary<object, object>, RubyFile>(RubyFileOps.CreateFile));
			DefineGlobalClass("Float", typeof(double), 7, super2, LoadFloat_Instance, LoadFloat_Class, LoadFloat_Constants, new RubyModule[1] { rubyModule6 });
			RubyClass super3 = DefineGlobalClass("Integer", typeof(Integer), 15, super2, LoadInteger_Instance, LoadInteger_Class, null, new RubyModule[1] { rubyModule6 });
			DefineGlobalClass("NoMemoryError", typeof(NoMemoryError), 15, super, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__NoMemoryError));
			RubyClass super4 = DefineGlobalClass("ScriptError", typeof(ScriptError), 7, super, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__ScriptError));
			RubyClass super5 = DefineGlobalClass("SignalException", typeof(SignalException), 15, super, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__SignalException));
			rubyClass2 = (base.Context.StandardErrorClass = DefineGlobalClass("StandardError", typeof(SystemException), 7, super, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__StandardError)));
			RubyClass super6 = rubyClass2;
			ExtendClass(typeof(decimal), 0, super2, LoadSystem__Decimal_Instance, LoadSystem__Decimal_Class, null, RubyModule.EmptyArray, new Func<RubyModule, double, decimal>(DecimalOps.InducedFrom));
			ExtendClass(typeof(float), 0, super2, LoadSystem__Single_Instance, LoadSystem__Single_Class, LoadSystem__Single_Constants, new RubyModule[1] { rubyModule6 }, new Func<RubyClass, double, float>(SingleOps.Create));
			DefineGlobalClass("SystemExit", typeof(SystemExit), 7, super, LoadSystemExit_Instance, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, SystemExit>(SystemExitOps.Factory), new Func<RubyClass, int, object, SystemExit>(SystemExitOps.Factory));
			DefineGlobalClass("ArgumentError", typeof(ArgumentException), 7, super6, LoadArgumentError_Instance, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__ArgumentError));
			DefineGlobalClass("Bignum", typeof(BigInteger), 7, super3, LoadBignum_Instance, LoadBignum_Class, LoadBignum_Constants, RubyModule.EmptyArray);
			RubyClass super7 = DefineGlobalClass("EncodingError", typeof(EncodingError), 7, super6, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__EncodingError));
			DefineGlobalClass("Fixnum", typeof(int), 7, super3, LoadFixnum_Instance, LoadFixnum_Class, LoadFixnum_Constants, RubyModule.EmptyArray);
			DefineGlobalClass("IndexError", typeof(IndexOutOfRangeException), 7, super6, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__IndexError));
			DefineGlobalClass("Interrupt", typeof(Interrupt), 15, super5, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__Interrupt));
			RubyClass super8 = DefineGlobalClass("IOError", typeof(IOException), 7, super6, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__IOError));
			DefineGlobalClass("LoadError", typeof(LoadError), 7, super4, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__LoadError));
			DefineGlobalClass("LocalJumpError", typeof(LocalJumpError), 7, super6, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__LocalJumpError));
			RubyClass super9 = DefineGlobalClass("NameError", typeof(MemberAccessException), 7, super6, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__NameError));
			DefineGlobalClass("NotImplementedError", typeof(NotImplementedError), 7, super4, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__NotImplementedError));
			RubyClass super10 = DefineGlobalClass("RangeError", typeof(ArgumentOutOfRangeException), 7, super6, LoadRangeError_Instance, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__RangeError));
			DefineGlobalClass("RegexpError", typeof(RegexpError), 7, super6, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__RegexpError));
			DefineGlobalClass("RuntimeError", typeof(RuntimeError), 7, super6, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__RuntimeError));
			DefineGlobalClass("SecurityError", typeof(SecurityException), 7, super6, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__SecurityError));
			DefineGlobalClass("SyntaxError", typeof(SyntaxError), 7, super4, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__SyntaxError));
			ExtendClass(typeof(byte), 0, super3, LoadSystem__Byte_Instance, LoadSystem__Byte_Class, LoadSystem__Byte_Constants, RubyModule.EmptyArray, new Func<RubyClass, int, byte>(ByteOps.InducedFrom), new Func<RubyClass, BigInteger, byte>(ByteOps.InducedFrom), new Func<RubyClass, double, byte>(ByteOps.InducedFrom));
			ExtendClass(typeof(short), 0, super3, LoadSystem__Int16_Instance, LoadSystem__Int16_Class, LoadSystem__Int16_Constants, RubyModule.EmptyArray, new Func<RubyClass, int, short>(Int16Ops.InducedFrom), new Func<RubyClass, BigInteger, short>(Int16Ops.InducedFrom), new Func<RubyClass, double, short>(Int16Ops.InducedFrom));
			ExtendClass(typeof(long), 0, super3, LoadSystem__Int64_Instance, LoadSystem__Int64_Class, LoadSystem__Int64_Constants, RubyModule.EmptyArray, new Func<RubyClass, int, long>(Int64Ops.InducedFrom), new Func<RubyClass, BigInteger, long>(Int64Ops.InducedFrom), new Func<RubyClass, double, long>(Int64Ops.InducedFrom));
			ExtendClass(typeof(sbyte), 0, super3, LoadSystem__SByte_Instance, LoadSystem__SByte_Class, LoadSystem__SByte_Constants, RubyModule.EmptyArray, new Func<RubyClass, int, sbyte>(SByteOps.InducedFrom), new Func<RubyClass, BigInteger, sbyte>(SByteOps.InducedFrom), new Func<RubyClass, double, sbyte>(SByteOps.InducedFrom));
			ExtendClass(typeof(ushort), 0, super3, LoadSystem__UInt16_Instance, LoadSystem__UInt16_Class, LoadSystem__UInt16_Constants, RubyModule.EmptyArray, new Func<RubyClass, int, ushort>(UInt16Ops.InducedFrom), new Func<RubyClass, BigInteger, ushort>(UInt16Ops.InducedFrom), new Func<RubyClass, double, ushort>(UInt16Ops.InducedFrom));
			ExtendClass(typeof(uint), 0, super3, LoadSystem__UInt32_Instance, LoadSystem__UInt32_Class, LoadSystem__UInt32_Constants, RubyModule.EmptyArray, new Func<RubyClass, int, uint>(UInt32Ops.InducedFrom), new Func<RubyClass, BigInteger, uint>(UInt32Ops.InducedFrom), new Func<RubyClass, double, uint>(UInt32Ops.InducedFrom));
			ExtendClass(typeof(ulong), 0, super3, LoadSystem__UInt64_Instance, LoadSystem__UInt64_Class, LoadSystem__UInt64_Constants, RubyModule.EmptyArray, new Func<RubyClass, int, ulong>(UInt64Ops.InducedFrom), new Func<RubyClass, BigInteger, ulong>(UInt64Ops.InducedFrom), new Func<RubyClass, double, ulong>(UInt64Ops.InducedFrom));
			RubyClass super11 = DefineGlobalClass("SystemCallError", typeof(ExternalException), 7, super6, LoadSystemCallError_Instance, null, null, RubyModule.EmptyArray, new Func<RubyClass, MutableString, ExternalException>(SystemCallErrorOps.Factory), new Func<RubyClass, int, ExternalException>(SystemCallErrorOps.Factory));
			DefineGlobalClass("SystemStackError", typeof(SystemStackError), 7, super6, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__SystemStackError));
			DefineGlobalClass("ThreadError", typeof(ThreadError), 15, super6, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__ThreadError));
			DefineGlobalClass("TypeError", typeof(InvalidOperationException), 7, super6, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__TypeError));
			DefineGlobalClass("ZeroDivisionError", typeof(DivideByZeroException), 7, super6, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__ZeroDivisionError));
			RubyClass value14 = DefineClass("Encoding::CompatibilityError", typeof(EncodingCompatibilityError), 7, super7, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__Encoding__CompatibilityError));
			RubyClass value15 = DefineClass("Encoding::ConverterNotFoundError", typeof(ConverterNotFoundError), 7, super7, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__Encoding__ConverterNotFoundError));
			RubyClass value16 = DefineClass("Encoding::InvalidByteSequenceError", typeof(InvalidByteSequenceError), 7, super7, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__Encoding__InvalidByteSequenceError));
			RubyClass value17 = DefineClass("Encoding::UndefinedConversionError", typeof(UndefinedConversionError), 7, super7, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__Encoding__UndefinedConversionError));
			DefineGlobalClass("EOFError", typeof(EOFError), 15, super8, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__EOFError));
			RubyClass value18 = DefineClass("Errno::EACCES", typeof(UnauthorizedAccessException), 7, super11, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, MutableString, UnauthorizedAccessException>(Errno.UnauthorizedAccessExceptionOps.Create));
			RubyClass value19 = DefineClass("Errno::EADDRINUSE", typeof(Errno.AddressInUseError), 15, super11, null, null, null, RubyModule.EmptyArray);
			RubyClass value20 = DefineClass("Errno::EAGAIN", typeof(Errno.ResourceTemporarilyUnavailableError), 15, super11, null, null, null, RubyModule.EmptyArray);
			RubyClass value21 = DefineClass("Errno::EBADF", typeof(BadFileDescriptorError), 7, super11, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, MutableString, BadFileDescriptorError>(Errno.BadFileDescriptorErrorOps.Create));
			RubyClass value22 = DefineClass("Errno::ECHILD", typeof(Errno.ChildError), 15, super11, null, null, null, RubyModule.EmptyArray);
			RubyClass value23 = DefineClass("Errno::ECONNABORTED", typeof(Errno.ConnectionAbortedError), 15, super11, null, null, null, RubyModule.EmptyArray);
			RubyClass value24 = DefineClass("Errno::ECONNREFUSED", typeof(Errno.ConnectionRefusedError), 15, super11, null, null, null, RubyModule.EmptyArray);
			RubyClass value25 = DefineClass("Errno::ECONNRESET", typeof(Errno.ConnectionResetError), 15, super11, null, null, null, RubyModule.EmptyArray);
			RubyClass value26 = DefineClass("Errno::EDOM", typeof(Errno.DomainError), 15, super11, null, null, null, RubyModule.EmptyArray);
			RubyClass value27 = DefineClass("Errno::EEXIST", typeof(ExistError), 7, super11, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, MutableString, ExistError>(Errno.ExistErrorOps.Create));
			RubyClass value28 = DefineClass("Errno::EHOSTDOWN", typeof(Errno.HostDownError), 15, super11, null, null, null, RubyModule.EmptyArray);
			RubyClass value29 = DefineClass("Errno::EINTR", typeof(Errno.InterruptedError), 15, super11, null, null, null, RubyModule.EmptyArray);
			RubyClass value30 = DefineClass("Errno::EINVAL", typeof(InvalidError), 7, super11, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, MutableString, InvalidError>(Errno.InvalidErrorOps.Create));
			RubyClass value31 = DefineClass("Errno::ENOENT", typeof(FileNotFoundException), 7, super11, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, MutableString, FileNotFoundException>(Errno.FileNotFoundExceptionOps.Create));
			RubyClass value32 = DefineClass("Errno::ENOEXEC", typeof(ExecFormatError), 7, super11, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, MutableString, BadFileDescriptorError>(Errno.ExecFormatErrorOps.Create));
			RubyClass value33 = DefineClass("Errno::ENOTCONN", typeof(Errno.NotConnectedError), 15, super11, null, null, null, RubyModule.EmptyArray);
			RubyClass value34 = DefineClass("Errno::ENOTDIR", typeof(DirectoryNotFoundException), 7, super11, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, MutableString, DirectoryNotFoundException>(Errno.DirectoryNotFoundExceptionOps.Create));
			RubyClass value35 = DefineClass("Errno::EPIPE", typeof(Errno.PipeError), 15, super11, null, null, null, RubyModule.EmptyArray);
			RubyClass value36 = DefineClass("Errno::ESPIPE", typeof(Errno.InvalidSeekError), 15, super11, null, null, null, RubyModule.EmptyArray);
			RubyClass value37 = DefineClass("Errno::EWOULDBLOCK", typeof(Errno.WouldBlockError), 15, super11, null, null, null, RubyModule.EmptyArray);
			RubyClass value38 = DefineClass("Errno::EXDEV", typeof(Errno.ImproperLinkError), 15, super11, null, null, null, RubyModule.EmptyArray);
			DefineGlobalClass("FloatDomainError", typeof(FloatDomainError), 15, super10, null, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, Exception>(ExceptionFactory__FloatDomainError));
			DefineGlobalClass("NoMethodError", typeof(MissingMethodException), 7, super9, LoadNoMethodError_Instance, null, null, RubyModule.EmptyArray, new Func<RubyClass, object, object, object, MissingMethodException>(NoMethodErrorOps.Factory));
			LibraryInitializer.SetBuiltinConstant(module5, "Constants", rubyModule3);
			LibraryInitializer.SetBuiltinConstant(rubyClass3, "WaitReadable", value);
			LibraryInitializer.SetBuiltinConstant(rubyClass3, "WaitWritable", value2);
			LibraryInitializer.SetBuiltinConstant(module2, "Clr", rubyModule4);
			LibraryInitializer.SetBuiltinConstant(rubyModule4, "BigInteger", value3);
			LibraryInitializer.SetBuiltinConstant(rubyModule4, "FlagEnumeration", value4);
			LibraryInitializer.SetBuiltinConstant(rubyModule4, "Float", value5);
			LibraryInitializer.SetBuiltinConstant(rubyModule4, "Integer", value6);
			LibraryInitializer.SetBuiltinConstant(rubyModule4, "MultiDimensionalArray", value7);
			LibraryInitializer.SetBuiltinConstant(rubyModule4, "String", rubyModule5);
			LibraryInitializer.SetBuiltinConstant(module2, "Print", value8);
			LibraryInitializer.SetBuiltinConstant(base.Context.ObjectClass, "ARGF", value9);
			LibraryInitializer.SetBuiltinConstant(base.Context.ObjectClass, "ENV", value10);
			LibraryInitializer.SetBuiltinConstant(module5, "Stat", value11);
			LibraryInitializer.SetBuiltinConstant(rubyModule4, "Name", value12);
			LibraryInitializer.SetBuiltinConstant(module3, "Status", value13);
			LibraryInitializer.SetBuiltinConstant(module4, "CompatibilityError", value14);
			LibraryInitializer.SetBuiltinConstant(module4, "ConverterNotFoundError", value15);
			LibraryInitializer.SetBuiltinConstant(module4, "InvalidByteSequenceError", value16);
			LibraryInitializer.SetBuiltinConstant(module4, "UndefinedConversionError", value17);
			LibraryInitializer.SetBuiltinConstant(module, "EACCES", value18);
			LibraryInitializer.SetBuiltinConstant(module, "EADDRINUSE", value19);
			LibraryInitializer.SetBuiltinConstant(module, "EAGAIN", value20);
			LibraryInitializer.SetBuiltinConstant(module, "EBADF", value21);
			LibraryInitializer.SetBuiltinConstant(module, "ECHILD", value22);
			LibraryInitializer.SetBuiltinConstant(module, "ECONNABORTED", value23);
			LibraryInitializer.SetBuiltinConstant(module, "ECONNREFUSED", value24);
			LibraryInitializer.SetBuiltinConstant(module, "ECONNRESET", value25);
			LibraryInitializer.SetBuiltinConstant(module, "EDOM", value26);
			LibraryInitializer.SetBuiltinConstant(module, "EEXIST", value27);
			LibraryInitializer.SetBuiltinConstant(module, "EHOSTDOWN", value28);
			LibraryInitializer.SetBuiltinConstant(module, "EINTR", value29);
			LibraryInitializer.SetBuiltinConstant(module, "EINVAL", value30);
			LibraryInitializer.SetBuiltinConstant(module, "ENOENT", value31);
			LibraryInitializer.SetBuiltinConstant(module, "ENOEXEC", value32);
			LibraryInitializer.SetBuiltinConstant(module, "ENOTCONN", value33);
			LibraryInitializer.SetBuiltinConstant(module, "ENOTDIR", value34);
			LibraryInitializer.SetBuiltinConstant(module, "EPIPE", value35);
			LibraryInitializer.SetBuiltinConstant(module, "ESPIPE", value36);
			LibraryInitializer.SetBuiltinConstant(module, "EWOULDBLOCK", value37);
			LibraryInitializer.SetBuiltinConstant(module, "EXDEV", value38);
		}

		private static void Load__MainSingleton_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "include", 81, 2147483648u, new Func<RubyContext, object, RubyModule[], RubyClass>(MainSingletonOps.Include));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 82, 0u, new Func<object, object>(MainSingletonOps.Initialize));
			LibraryInitializer.DefineLibraryMethod(module, "private", 81, 2147614724u, new Func<RubyScope, object, string[], RubyModule>(MainSingletonOps.SetPrivateVisibility));
			LibraryInitializer.DefineLibraryMethod(module, "public", 81, 2147614724u, new Func<RubyScope, object, string[], RubyModule>(MainSingletonOps.SetPublicVisibility));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<object, MutableString>(MainSingletonOps.ToS));
		}

		private static void Load__MainSingleton_Class(RubyModule module)
		{
		}

		private static void Load__Singleton_ArgFilesSingletonOps_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "binmode", 81, 0u, new Func<RubyContext, object, object>(ArgFilesSingletonOps.BinMode));
			LibraryInitializer.DefineLibraryMethod(module, "close", 81, 0u, new Func<RubyContext, object, object>(ArgFilesSingletonOps.Close));
			LibraryInitializer.DefineLibraryMethod(module, "closed?", 81, 0u, new Func<RubyContext, object, bool>(ArgFilesSingletonOps.Closed));
			LibraryInitializer.DefineLibraryMethod(module, "each", 81, 0u, 0u, 262152u, 786432u, new Func<RubyContext, BlockParam, object, object>(ArgFilesSingletonOps.Each), new Func<RubyContext, BlockParam, object, DynamicNull, object>(ArgFilesSingletonOps.Each), new Func<RubyContext, BlockParam, object, Union<MutableString, int>, object>(ArgFilesSingletonOps.Each), new Func<RubyContext, BlockParam, object, MutableString, int, object>(ArgFilesSingletonOps.Each));
			LibraryInitializer.DefineLibraryMethod(module, "each_byte", 81, 0u, new Func<RubyContext, BlockParam, object, object>(ArgFilesSingletonOps.EachByte));
			LibraryInitializer.DefineLibraryMethod(module, "each_line", 81, 0u, 0u, 262152u, 786432u, new Func<RubyContext, BlockParam, object, object>(ArgFilesSingletonOps.Each), new Func<RubyContext, BlockParam, object, DynamicNull, object>(ArgFilesSingletonOps.Each), new Func<RubyContext, BlockParam, object, Union<MutableString, int>, object>(ArgFilesSingletonOps.Each), new Func<RubyContext, BlockParam, object, MutableString, int, object>(ArgFilesSingletonOps.Each));
			LibraryInitializer.DefineLibraryMethod(module, "eof", 81, 0u, new Func<RubyContext, object, bool>(ArgFilesSingletonOps.EoF));
			LibraryInitializer.DefineLibraryMethod(module, "eof?", 81, 0u, new Func<RubyContext, object, bool>(ArgFilesSingletonOps.EoF));
			LibraryInitializer.DefineLibraryMethod(module, "file", 81, 0u, new Func<RubyContext, object, RubyIO>(ArgFilesSingletonOps.ToIO));
			LibraryInitializer.DefineLibraryMethod(module, "filename", 81, 0u, new Func<RubyContext, object, MutableString>(ArgFilesSingletonOps.GetCurrentFileName));
			LibraryInitializer.DefineLibraryMethod(module, "fileno", 81, 0u, new Func<RubyContext, object, int>(ArgFilesSingletonOps.FileNo));
			LibraryInitializer.DefineLibraryMethod(module, "getc", 81, 0u, new Func<RubyContext, object, object>(ArgFilesSingletonOps.Getc));
			LibraryInitializer.DefineLibraryMethod(module, "gets", 81, 0u, 0u, 131076u, 393216u, new Func<RubyScope, object, MutableString>(ArgFilesSingletonOps.Gets), new Func<RubyScope, object, DynamicNull, MutableString>(ArgFilesSingletonOps.Gets), new Func<RubyScope, object, Union<MutableString, int>, MutableString>(ArgFilesSingletonOps.Gets), new Func<RubyScope, object, MutableString, int, MutableString>(ArgFilesSingletonOps.Gets));
			LibraryInitializer.DefineLibraryMethod(module, "lineno", 81, 0u, new Func<RubyContext, object, int>(ArgFilesSingletonOps.GetLineNumber));
			LibraryInitializer.DefineLibraryMethod(module, "lineno=", 81, 131072u, new Action<RubyContext, object, int>(ArgFilesSingletonOps.SetLineNumber));
			LibraryInitializer.DefineLibraryMethod(module, "path", 81, 0u, new Func<RubyContext, object, MutableString>(ArgFilesSingletonOps.GetCurrentFileName));
			LibraryInitializer.DefineLibraryMethod(module, "pos", 81, 0u, new Func<RubyContext, object, object>(ArgFilesSingletonOps.Pos));
			LibraryInitializer.DefineLibraryMethod(module, "pos=", 81, 131072u, new Action<RubyContext, object, IntegerValue>(ArgFilesSingletonOps.Pos));
			LibraryInitializer.DefineLibraryMethod(module, "read", 81, 0u, 131072u, 196608u, new Func<RubyContext, object, MutableString>(ArgFilesSingletonOps.Read), new Func<RubyContext, DynamicNull, MutableString, MutableString>(ArgFilesSingletonOps.Read), new Func<RubyContext, int, MutableString, MutableString>(ArgFilesSingletonOps.Read));
			LibraryInitializer.DefineLibraryMethod(module, "readchar", 81, 0u, new Func<RubyContext, object, int>(ArgFilesSingletonOps.ReadChar));
			LibraryInitializer.DefineLibraryMethod(module, "readline", 81, 0u, 0u, 131076u, 393216u, new Func<RubyScope, object, MutableString>(ArgFilesSingletonOps.ReadLine), new Func<RubyScope, object, DynamicNull, MutableString>(ArgFilesSingletonOps.ReadLine), new Func<RubyScope, object, Union<MutableString, int>, MutableString>(ArgFilesSingletonOps.ReadLine), new Func<RubyScope, object, MutableString, int, MutableString>(ArgFilesSingletonOps.ReadLine));
			LibraryInitializer.DefineLibraryMethod(module, "readlines", 81, 0u, 0u, 131076u, 393216u, new Func<RubyContext, object, RubyArray>(ArgFilesSingletonOps.ReadLines), new Func<RubyContext, object, DynamicNull, RubyArray>(ArgFilesSingletonOps.ReadLines), new Func<RubyContext, object, Union<MutableString, int>, RubyArray>(ArgFilesSingletonOps.ReadLines), new Func<RubyContext, object, MutableString, int, RubyArray>(ArgFilesSingletonOps.ReadLines));
			LibraryInitializer.DefineLibraryMethod(module, "rewind", 81, 0u, new Action<RubyContext, object>(ArgFilesSingletonOps.Rewind));
			LibraryInitializer.DefineLibraryMethod(module, "seek", 81, 393216u, new Func<RubyContext, object, IntegerValue, int, int>(ArgFilesSingletonOps.Seek));
			LibraryInitializer.DefineLibraryMethod(module, "skip", 81, 0u, new Action<RubyContext, object>(ArgFilesSingletonOps.Skip));
			LibraryInitializer.DefineLibraryMethod(module, "tell", 81, 0u, new Func<RubyContext, object, object>(ArgFilesSingletonOps.Pos));
			LibraryInitializer.DefineLibraryMethod(module, "to_a", 81, 0u, new Func<RubyContext, object, RubyArray>(ArgFilesSingletonOps.TOA));
			LibraryInitializer.DefineLibraryMethod(module, "to_i", 81, 0u, new Func<RubyContext, object, int>(ArgFilesSingletonOps.FileNo));
			LibraryInitializer.DefineLibraryMethod(module, "to_io", 81, 0u, new Func<RubyContext, object, RubyIO>(ArgFilesSingletonOps.ToIO));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<RubyContext, object, MutableString>(ArgFilesSingletonOps.ToS));
		}

		private static void LoadArgumentError_Instance(RubyModule module)
		{
			module.HideMethod("message");
		}

		private static void LoadArray_Constants(RubyModule module)
		{
		}

		private static void LoadArray_Instance(RubyModule module)
		{
			LoadSystem__Collections__IList_Instance(module);
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 82, 0u, 8u, 0u, 131072u, new Func<RubyContext, RubyArray, RubyArray>(ArrayOps.Reinitialize), new Func<ConversionStorage<Union<IList, int>>, BlockParam, RubyArray, object, object>(ArrayOps.Reinitialize), new Func<BlockParam, RubyArray, int, object, RubyArray>(ArrayOps.Reinitialize), new Func<RubyContext, RubyArray, int, object, RubyArray>(ArrayOps.ReinitializeByRepeatedValue));
			LibraryInitializer.DefineLibraryMethod(module, "pack", 81, 1048608u, new Func<ConversionStorage<IntegerValue>, ConversionStorage<double>, ConversionStorage<MutableString>, ConversionStorage<MutableString>, RubyArray, MutableString, MutableString>(ArrayOps.Pack));
			LibraryInitializer.DefineLibraryMethod(module, "reverse!", 81, 0u, new Func<RubyContext, RubyArray, RubyArray>(ArrayOps.InPlaceReverse));
			LibraryInitializer.DefineLibraryMethod(module, "sort", 81, 0u, new Func<ComparisonStorage, BlockParam, RubyArray, object>(ArrayOps.Sort));
			LibraryInitializer.DefineLibraryMethod(module, "sort!", 81, 0u, new Func<ComparisonStorage, BlockParam, RubyArray, object>(ArrayOps.SortInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "to_a", 81, 0u, new Func<RubyArray, RubyArray>(ArrayOps.ToArray));
			LibraryInitializer.DefineLibraryMethod(module, "to_ary", 81, 0u, new Func<RubyArray, RubyArray>(ArrayOps.ToExplicitArray));
		}

		private static void LoadArray_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "[]", 97, 2147483648u, new Func<RubyClass, object[], RubyArray>(ArrayOps.MakeArray));
			LibraryInitializer.DefineLibraryMethod(module, "try_convert", 97, 0u, new Func<ConversionStorage<IList>, RubyClass, object, IList>(ArrayOps.TryConvert));
		}

		private static void LoadBasicObject_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "!", 81, 0u, new Func<object, bool>(BasicObjectOps.Not));
			LibraryInitializer.DefineLibraryMethod(module, "!=", 81, 0u, new Func<BinaryOpStorage, object, object, bool>(BasicObjectOps.ValueNotEquals));
			LibraryInitializer.DefineLibraryMethod(module, "__send__", 81, new uint[11]
			{
				0u, 131076u, 262152u, 131076u, 262152u, 131076u, 262152u, 131076u, 262152u, 2147614724u,
				2147745800u
			}, new Func<RubyScope, object, object>(BasicObjectOps.SendMessage), new Func<RubyScope, object, string, object>(BasicObjectOps.SendMessage), new Func<RubyScope, BlockParam, object, string, object>(BasicObjectOps.SendMessage), new Func<RubyScope, object, string, object, object>(BasicObjectOps.SendMessage), new Func<RubyScope, BlockParam, object, string, object, object>(BasicObjectOps.SendMessage), new Func<RubyScope, object, string, object, object, object>(BasicObjectOps.SendMessage), new Func<RubyScope, BlockParam, object, string, object, object, object>(BasicObjectOps.SendMessage), new Func<RubyScope, object, string, object, object, object, object>(BasicObjectOps.SendMessage), new Func<RubyScope, BlockParam, object, string, object, object, object, object>(BasicObjectOps.SendMessage), new Func<RubyScope, object, string, object[], object>(BasicObjectOps.SendMessage), new Func<RubyScope, BlockParam, object, string, object[], object>(BasicObjectOps.SendMessage));
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 1u, 0u, new Func<IRubyObject, object, bool>(BasicObjectOps.ValueEquals), new Func<object, object, bool>(BasicObjectOps.ValueEquals));
			LibraryInitializer.DefineLibraryMethod(module, "equal?", 81, 0u, new Func<object, object, bool>(BasicObjectOps.IsEqual));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 90, 2147483648u, new Func<object, object[], object>(BasicObjectOps.Reinitialize));
			LibraryInitializer.DefineLibraryMethod(module, "instance_eval", 81, 12u, 1u, new Func<RubyScope, object, MutableString, MutableString, int, object>(BasicObjectOps.Evaluate), new Func<BlockParam, object, object>(BasicObjectOps.InstanceEval));
			LibraryInitializer.DefineLibraryMethod(module, "instance_exec", 81, 2147483649u, new Func<BlockParam, object, object[], object>(BasicObjectOps.InstanceExec));
			LibraryInitializer.DefineLibraryMethod(module, "method_missing", 82, 2147483652u, new Func<RubyContext, object, RubySymbol, object[], object>(BasicObjectOps.MethodMissing));
			LibraryInitializer.DefineLibraryMethod(module, "singleton_method_added", 90, 0u, new Action<object, object>(BasicObjectOps.MethodAdded));
			LibraryInitializer.DefineLibraryMethod(module, "singleton_method_removed", 90, 0u, new Action<object, object>(BasicObjectOps.MethodRemoved));
			LibraryInitializer.DefineLibraryMethod(module, "singleton_method_undefined", 90, 0u, new Action<object, object>(BasicObjectOps.MethodUndefined));
		}

		private static void LoadBasicObject_Class(RubyModule module)
		{
		}

		private static void LoadBignum_Constants(RubyModule module)
		{
		}

		private static void LoadBignum_Instance(RubyModule module)
		{
			LoadIronRuby__Clr__BigInteger_Instance(module);
			module.HideMethod("<");
			module.HideMethod("<=");
			module.HideMethod(">");
			module.HideMethod(">=");
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<BigInteger, int>(BignumOps.Size));
		}

		private static void LoadBignum_Class(RubyModule module)
		{
			module.UndefineMethodNoEvent("new");
		}

		private static void LoadBinding_Instance(RubyModule module)
		{
			module.UndefineMethodNoEvent("LocalScope");
		}

		private static void LoadBinding_Class(RubyModule module)
		{
			module.UndefineMethodNoEvent("new");
		}

		private static void LoadClass_Instance(RubyModule module)
		{
			module.UndefineMethodNoEvent("append_features");
			module.UndefineMethodNoEvent("extend_object");
			module.UndefineMethodNoEvent("module_function");
			LibraryInitializer.DefineRuleGenerator(module, "allocate", 81, ClassOps.Allocate());
			LibraryInitializer.DefineLibraryMethod(module, "clr_constructor", 81, 0u, new Func<RubyClass, RubyMethod>(ClassOps.GetClrConstructor));
			LibraryInitializer.DefineLibraryMethod(module, "clr_ctor", 81, 0u, new Func<RubyClass, RubyMethod>(ClassOps.GetClrConstructor));
			LibraryInitializer.DefineRuleGenerator(module, "clr_new", 81, ClassOps.ClrNew());
			LibraryInitializer.DefineLibraryMethod(module, "inherited", 90, 0u, new Action<object, object>(ClassOps.Inherited));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 82, 0u, new Action<BlockParam, RubyClass, RubyClass>(ClassOps.Reinitialize));
			LibraryInitializer.DefineLibraryMethod(module, "initialize_copy", 82, 2u, new Action<RubyClass, RubyClass>(ClassOps.InitializeCopy));
			LibraryInitializer.DefineRuleGenerator(module, "new", 81, ClassOps.New());
			LibraryInitializer.DefineLibraryMethod(module, "superclass", 81, 0u, new Func<RubyClass, RubyClass>(ClassOps.GetSuperclass));
		}

		private static void LoadClass_Class(RubyModule module)
		{
		}

		private static void LoadComparable_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "<", 81, 0u, new Func<ComparisonStorage, object, object, bool>(Comparable.Less));
			LibraryInitializer.DefineLibraryMethod(module, "<=", 81, 0u, new Func<ComparisonStorage, object, object, bool>(Comparable.LessOrEqual));
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 0u, new Func<BinaryOpStorage, object, object, bool>(Comparable.Equal));
			LibraryInitializer.DefineLibraryMethod(module, ">", 81, 0u, new Func<ComparisonStorage, object, object, bool>(Comparable.Greater));
			LibraryInitializer.DefineLibraryMethod(module, ">=", 81, 0u, new Func<ComparisonStorage, object, object, bool>(Comparable.GreaterOrEqual));
			LibraryInitializer.DefineLibraryMethod(module, "between?", 81, 0u, new Func<ComparisonStorage, object, object, object, bool>(Comparable.Between));
		}

		private static void LoadDir_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "close", 81, 0u, new Action<RubyDir>(RubyDir.Close));
			LibraryInitializer.DefineLibraryMethod(module, "each", 81, 0u, new Func<RubyContext, BlockParam, RubyDir, object>(RubyDir.Each));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 82, 2u, new Func<RubyDir, MutableString, RubyDir>(RubyDir.Reinitialize));
			LibraryInitializer.DefineLibraryMethod(module, "path", 81, 0u, new Func<RubyContext, RubyDir, MutableString>(RubyDir.GetPath));
			LibraryInitializer.DefineLibraryMethod(module, "pos", 81, 0u, new Func<RubyDir, int>(RubyDir.GetCurrentPosition));
			LibraryInitializer.DefineLibraryMethod(module, "pos=", 81, 0u, new Func<RubyDir, int, int>(RubyDir.SetPosition));
			LibraryInitializer.DefineLibraryMethod(module, "read", 81, 0u, new Func<RubyContext, RubyDir, MutableString>(RubyDir.Read));
			LibraryInitializer.DefineLibraryMethod(module, "rewind", 81, 0u, new Func<RubyDir, RubyDir>(RubyDir.Rewind));
			LibraryInitializer.DefineLibraryMethod(module, "seek", 81, 0u, new Func<RubyDir, int, RubyDir>(RubyDir.Seek));
			LibraryInitializer.DefineLibraryMethod(module, "tell", 81, 0u, new Func<RubyDir, int>(RubyDir.GetCurrentPosition));
			LibraryInitializer.DefineLibraryMethod(module, "to_path", 81, 0u, new Func<RubyContext, RubyDir, MutableString>(RubyDir.GetPath));
		}

		private static void LoadDir_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "[]", 97, 2147549186u, new Func<RubyClass, MutableString[], RubyArray>(RubyDir.Glob));
			LibraryInitializer.DefineLibraryMethod(module, "chdir", 97, 0u, 0u, new Func<ConversionStorage<MutableString>, BlockParam, RubyClass, object, object>(RubyDir.ChangeDirectory), new Func<BlockParam, RubyClass, object>(RubyDir.ChangeDirectory));
			LibraryInitializer.DefineLibraryMethod(module, "chroot", 97, 0u, new Func<object, int>(RubyDir.ChangeRoot));
			LibraryInitializer.DefineLibraryMethod(module, "delete", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, int>(RubyDir.RemoveDirectory));
			LibraryInitializer.DefineLibraryMethod(module, "entries", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, IDictionary<object, object>, RubyArray>(RubyDir.GetEntries));
			LibraryInitializer.DefineLibraryMethod(module, "exist?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyDir.Exists));
			LibraryInitializer.DefineLibraryMethod(module, "exists?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyDir.Exists));
			LibraryInitializer.DefineLibraryMethod(module, "foreach", 97, 0u, new Func<ConversionStorage<MutableString>, BlockParam, RubyClass, object, object>(RubyDir.ForEach));
			LibraryInitializer.DefineLibraryMethod(module, "getwd", 97, 0u, new Func<RubyClass, MutableString>(RubyDir.GetCurrentDirectory));
			LibraryInitializer.DefineLibraryMethod(module, "glob", 97, 393221u, 196610u, new Func<BlockParam, RubyClass, MutableString, int, object>(RubyDir.Glob), new Func<RubyClass, MutableString, int, object>(RubyDir.Glob));
			LibraryInitializer.DefineLibraryMethod(module, "mkdir", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, object, int>(RubyDir.MakeDirectory));
			LibraryInitializer.DefineLibraryMethod(module, "open", 97, 0u, 0u, new Func<ConversionStorage<MutableString>, BlockParam, RubyClass, object, object>(RubyDir.Open), new Func<ConversionStorage<MutableString>, RubyClass, object, RubyDir>(RubyDir.Open));
			LibraryInitializer.DefineLibraryMethod(module, "pwd", 97, 0u, new Func<RubyClass, MutableString>(RubyDir.GetCurrentDirectory));
			LibraryInitializer.DefineLibraryMethod(module, "rmdir", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, int>(RubyDir.RemoveDirectory));
			LibraryInitializer.DefineLibraryMethod(module, "unlink", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, int>(RubyDir.RemoveDirectory));
		}

		private static void LoadEncoding_Constants(RubyModule module)
		{
			LibraryInitializer.SetBuiltinConstant(module, "ANSI_X3_4_1968", RubyEncodingOps.US_ASCII);
			LibraryInitializer.SetBuiltinConstant(module, "ASCII", RubyEncodingOps.US_ASCII);
			LibraryInitializer.SetBuiltinConstant(module, "ASCII_8BIT", RubyEncodingOps.ASCII_8BIT);
			LibraryInitializer.SetBuiltinConstant(module, "Big5", RubyEncodingOps.Big5);
			LibraryInitializer.SetBuiltinConstant(module, "BIG5", RubyEncodingOps.Big5);
			LibraryInitializer.SetBuiltinConstant(module, "BINARY", RubyEncodingOps.BINARY);
			LibraryInitializer.SetBuiltinConstant(module, "EUC_JP", RubyEncodingOps.EUC_JP);
			LibraryInitializer.SetBuiltinConstant(module, "ISO_8859_15", RubyEncodingOps.ISO_8859_15);
			LibraryInitializer.SetBuiltinConstant(module, "ISO_8859_9", RubyEncodingOps.ISO_8859_9);
			LibraryInitializer.SetBuiltinConstant(module, "ISO8859_15", RubyEncodingOps.ISO_8859_15);
			LibraryInitializer.SetBuiltinConstant(module, "ISO8859_9", RubyEncodingOps.ISO_8859_9);
			LibraryInitializer.SetBuiltinConstant(module, "KOI8_R", RubyEncodingOps.KOI8_R);
			LibraryInitializer.SetBuiltinConstant(module, "Shift_JIS", RubyEncodingOps.SHIFT_JIS);
			LibraryInitializer.SetBuiltinConstant(module, "SHIFT_JIS", RubyEncodingOps.SHIFT_JIS);
			LibraryInitializer.SetBuiltinConstant(module, "TIS_620", RubyEncodingOps.TIS_620);
			LibraryInitializer.SetBuiltinConstant(module, "US_ASCII", RubyEncodingOps.US_ASCII);
			LibraryInitializer.SetBuiltinConstant(module, "UTF_16BE", RubyEncodingOps.UTF_16BE);
			LibraryInitializer.SetBuiltinConstant(module, "UTF_16LE", RubyEncodingOps.UTF_16LE);
			LibraryInitializer.SetBuiltinConstant(module, "UTF_32BE", RubyEncodingOps.UTF_32BE);
			LibraryInitializer.SetBuiltinConstant(module, "UTF_32LE", RubyEncodingOps.UTF_32LE);
			LibraryInitializer.SetBuiltinConstant(module, "UTF_7", RubyEncodingOps.UTF_7);
			LibraryInitializer.SetBuiltinConstant(module, "UTF_8", RubyEncodingOps.UTF_8);
		}

		private static void LoadEncoding_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "ascii_compatible?", 81, 0u, new Func<RubyEncoding, bool>(RubyEncodingOps.IsAsciiCompatible));
			LibraryInitializer.DefineLibraryMethod(module, "based_encoding", 81, 0u, new Func<RubyEncoding, RubyEncoding>(RubyEncodingOps.BasedEncoding));
			LibraryInitializer.DefineLibraryMethod(module, "dummy?", 81, 0u, new Func<RubyEncoding, bool>(RubyEncodingOps.IsDummy));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<RubyContext, RubyEncoding, MutableString>(RubyEncodingOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "name", 81, 0u, new Func<RubyEncoding, MutableString>(RubyEncodingOps.ToS));
			LibraryInitializer.DefineLibraryMethod(module, "names", 81, 0u, new Func<RubyContext, RubyEncoding, RubyArray>(RubyEncodingOps.GetAllNames));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<RubyEncoding, MutableString>(RubyEncodingOps.ToS));
		}

		private static void LoadEncoding_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "aliases", 97, 0u, new Func<RubyClass, Hash>(RubyEncodingOps.GetAliases));
			LibraryInitializer.DefineLibraryMethod(module, "compatible?", 97, new uint[10] { 6u, 6u, 6u, 6u, 6u, 6u, 6u, 6u, 6u, 0u }, new Func<RubyClass, MutableString, MutableString, RubyEncoding>(RubyEncodingOps.GetCompatible), new Func<RubyClass, RubyEncoding, RubyEncoding, RubyEncoding>(RubyEncodingOps.GetCompatible), new Func<RubyClass, RubyEncoding, MutableString, RubyEncoding>(RubyEncodingOps.GetCompatible), new Func<RubyClass, MutableString, RubyEncoding, RubyEncoding>(RubyEncodingOps.GetCompatible), new Func<RubyClass, RubyEncoding, RubySymbol, RubyEncoding>(RubyEncodingOps.GetCompatible), new Func<RubyClass, MutableString, RubySymbol, RubyEncoding>(RubyEncodingOps.GetCompatible), new Func<RubyClass, RubySymbol, RubyEncoding, RubyEncoding>(RubyEncodingOps.GetCompatible), new Func<RubyClass, RubySymbol, MutableString, RubyEncoding>(RubyEncodingOps.GetCompatible), new Func<RubyClass, RubySymbol, RubySymbol, RubyEncoding>(RubyEncodingOps.GetCompatible), new Func<RubyClass, object, object, RubyEncoding>(RubyEncodingOps.GetCompatible));
			LibraryInitializer.DefineLibraryMethod(module, "default_external", 97, 0u, new Func<RubyClass, RubyEncoding>(RubyEncodingOps.GetDefaultExternalEncoding));
			LibraryInitializer.DefineLibraryMethod(module, "default_external=", 97, 0u, 65538u, new Func<RubyClass, RubyEncoding, RubyEncoding>(RubyEncodingOps.SetDefaultExternalEncoding), new Func<RubyClass, MutableString, RubyEncoding>(RubyEncodingOps.SetDefaultExternalEncoding));
			LibraryInitializer.DefineLibraryMethod(module, "default_internal", 97, 0u, new Func<RubyClass, RubyEncoding>(RubyEncodingOps.GetDefaultInternalEncoding));
			LibraryInitializer.DefineLibraryMethod(module, "default_internal=", 97, 0u, 65538u, new Func<RubyClass, RubyEncoding, RubyEncoding>(RubyEncodingOps.SetDefaultInternalEncoding), new Func<RubyClass, MutableString, RubyEncoding>(RubyEncodingOps.SetDefaultInternalEncoding));
			LibraryInitializer.DefineLibraryMethod(module, "find", 97, 65538u, new Func<RubyClass, MutableString, RubyEncoding>(RubyEncodingOps.GetEncoding));
			LibraryInitializer.DefineLibraryMethod(module, "list", 97, 0u, new Func<RubyClass, RubyArray>(RubyEncodingOps.GetAvailableEncodings));
			LibraryInitializer.DefineLibraryMethod(module, "locale_charmap", 97, 0u, new Func<RubyClass, MutableString>(RubyEncodingOps.GetDefaultCharmap));
			LibraryInitializer.DefineLibraryMethod(module, "name_list", 97, 0u, new Func<RubyClass, RubyArray>(RubyEncodingOps.GetNameList));
		}

		private static void LoadEnumerable_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "all?", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, object>(Enumerable.TrueForAll));
			LibraryInitializer.DefineLibraryMethod(module, "any?", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, object>(Enumerable.TrueForAny));
			LibraryInitializer.DefineLibraryMethod(module, "collect", 81, 0u, 2u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, Enumerator>(Enumerable.GetMapEnumerator), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, object>(Enumerable.Map));
			LibraryInitializer.DefineLibraryMethod(module, "count", 81, 0u, 0u, 4u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, object, int>(Enumerable.Count), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BinaryOpStorage, BlockParam, object, object, int>(Enumerable.Count), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BinaryOpStorage, BlockParam, object, object>(Enumerable.Count));
			LibraryInitializer.DefineLibraryMethod(module, "cycle", 81, 262144u, 0u, 262146u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, int, Enumerator>(Enumerable.GetCycleEnumerator), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, DynamicNull, object>(Enumerable.Cycle), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, int, object>(Enumerable.Cycle));
			LibraryInitializer.DefineLibraryMethod(module, "detect", 81, 0u, 4u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, CallSiteStorage<Func<CallSite, object, object>>, BlockParam, object, object, Enumerator>(Enumerable.GetFindEnumerator), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, CallSiteStorage<Func<CallSite, object, object>>, BlockParam, object, object, object>(Enumerable.Find));
			LibraryInitializer.DefineLibraryMethod(module, "drop", 81, 131072u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, object, int, RubyArray>(Enumerable.Drop));
			LibraryInitializer.DefineLibraryMethod(module, "drop_while", 81, 0u, 2u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, Enumerator>(Enumerable.GetDropWhileEnumerator), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, object>(Enumerable.DropWhile));
			LibraryInitializer.DefineLibraryMethod(module, "each_cons", 81, 262144u, 262146u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, int, Enumerator>(Enumerable.GetEachConsEnumerator), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, int, object>(Enumerable.EachCons));
			LibraryInitializer.DefineLibraryMethod(module, "each_slice", 81, 262144u, 262146u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, int, Enumerator>(Enumerable.GetEachSliceEnumerator), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, int, object>(Enumerable.EachSlice));
			LibraryInitializer.DefineLibraryMethod(module, "each_with_index", 81, 0u, 2u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, Enumerator>(Enumerable.GetEachWithIndexEnumerator), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, object>(Enumerable.EachWithIndex));
			LibraryInitializer.DefineLibraryMethod(module, "entries", 81, 0u, 2147483648u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, object, RubyArray>(Enumerable.ToArray), new Func<CallSiteStorage<Func<CallSite, object, Proc, IList, object>>, object, object[], RubyArray>(Enumerable.ToArray));
			LibraryInitializer.DefineLibraryMethod(module, "enum_cons", 81, 65536u, new Func<object, int, Enumerator>(Enumerable.GetConsEnumerator));
			LibraryInitializer.DefineLibraryMethod(module, "enum_slice", 81, 65536u, new Func<object, int, Enumerator>(Enumerable.GetSliceEnumerator));
			LibraryInitializer.DefineLibraryMethod(module, "enum_with_index", 81, 0u, new Func<object, Enumerator>(Enumerable.GetEnumeratorWithIndex));
			LibraryInitializer.DefineLibraryMethod(module, "find", 81, 0u, 4u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, CallSiteStorage<Func<CallSite, object, object>>, BlockParam, object, object, Enumerator>(Enumerable.GetFindEnumerator), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, CallSiteStorage<Func<CallSite, object, object>>, BlockParam, object, object, object>(Enumerable.Find));
			LibraryInitializer.DefineLibraryMethod(module, "find_all", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, object>(Enumerable.Select));
			LibraryInitializer.DefineLibraryMethod(module, "find_index", 81, 0u, 2u, 0u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, Enumerator>(Enumerable.GetFindIndexEnumerator), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, object>(Enumerable.FindIndex), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BinaryOpStorage, BlockParam, object, object, object>(Enumerable.FindIndex));
			LibraryInitializer.DefineLibraryMethod(module, "first", 81, 0u, 131072u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, object, object>(Enumerable.First), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, object, int, RubyArray>(Enumerable.Take));
			LibraryInitializer.DefineLibraryMethod(module, "grep", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BinaryOpStorage, BlockParam, object, object, object>(Enumerable.Grep));
			LibraryInitializer.DefineLibraryMethod(module, "include?", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BinaryOpStorage, object, object, object>(Enumerable.Contains));
			LibraryInitializer.DefineLibraryMethod(module, "inject", 81, 2u, 524304u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, object, object>(Enumerable.Inject), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, RubyScope, object, object, string, object>(Enumerable.Inject));
			LibraryInitializer.DefineLibraryMethod(module, "map", 81, 0u, 2u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, Enumerator>(Enumerable.GetMapEnumerator), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, object>(Enumerable.Map));
			LibraryInitializer.DefineLibraryMethod(module, "max", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, ComparisonStorage, BlockParam, object, object>(Enumerable.GetMaximum));
			LibraryInitializer.DefineLibraryMethod(module, "member?", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BinaryOpStorage, object, object, object>(Enumerable.Contains));
			LibraryInitializer.DefineLibraryMethod(module, "min", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, ComparisonStorage, BlockParam, object, object>(Enumerable.GetMinimum));
			LibraryInitializer.DefineLibraryMethod(module, "minmax", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, ComparisonStorage, BlockParam, object, object>(Enumerable.GetExtremes));
			LibraryInitializer.DefineLibraryMethod(module, "none?", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, object>(Enumerable.TrueForNone));
			LibraryInitializer.DefineLibraryMethod(module, "one?", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BinaryOpStorage, BlockParam, object, object>(Enumerable.One));
			LibraryInitializer.DefineLibraryMethod(module, "partition", 81, 0u, 2u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, Enumerator>(Enumerable.GetPartitionEnumerator), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, object>(Enumerable.Partition));
			LibraryInitializer.DefineLibraryMethod(module, "reduce", 81, 2u, 524304u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, object, object>(Enumerable.Inject), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, RubyScope, object, object, string, object>(Enumerable.Inject));
			LibraryInitializer.DefineLibraryMethod(module, "reject", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, object>(Enumerable.Reject));
			LibraryInitializer.DefineLibraryMethod(module, "select", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, object>(Enumerable.Select));
			LibraryInitializer.DefineLibraryMethod(module, "sort", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, ComparisonStorage, BlockParam, object, object>(Enumerable.Sort));
			LibraryInitializer.DefineLibraryMethod(module, "sort_by", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, ComparisonStorage, BlockParam, object, object>(Enumerable.SortBy));
			LibraryInitializer.DefineLibraryMethod(module, "take", 81, 131072u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, object, int, RubyArray>(Enumerable.Take));
			LibraryInitializer.DefineLibraryMethod(module, "take_while", 81, 0u, 2u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, Enumerator>(Enumerable.GetTakeWhileEnumerator), new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, object>(Enumerable.TakeWhile));
			LibraryInitializer.DefineLibraryMethod(module, "to_a", 81, 0u, 2147483648u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, object, RubyArray>(Enumerable.ToArray), new Func<CallSiteStorage<Func<CallSite, object, Proc, IList, object>>, object, object[], RubyArray>(Enumerable.ToArray));
			LibraryInitializer.DefineLibraryMethod(module, "zip", 81, 2147745800u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, object, IList[], object>(Enumerable.Zip));
		}

		private static void LoadEnumerator_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "each", 81, 0u, new Func<RubyScope, BlockParam, Enumerator, object>(Enumerator.Each));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 82, 2147614720u, new Func<Enumerator, object, string, object[], Enumerator>(Enumerator.Reinitialize));
		}

		private static void Load__Singleton_EnvironmentSingletonOps_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "[]", 81, 131076u, new Func<RubyContext, object, MutableString, MutableString>(EnvironmentSingletonOps.GetVariable));
			LibraryInitializer.DefineLibraryMethod(module, "[]=", 81, 393220u, new Func<RubyContext, object, MutableString, MutableString, MutableString>(EnvironmentSingletonOps.SetVariable));
			LibraryInitializer.DefineLibraryMethod(module, "clear", 81, 0u, new Func<RubyContext, object, object>(EnvironmentSingletonOps.Clear));
			LibraryInitializer.DefineLibraryMethod(module, "delete", 81, 131076u, new Func<RubyContext, object, MutableString, object>(EnvironmentSingletonOps.Delete));
			LibraryInitializer.DefineLibraryMethod(module, "delete_if", 81, 0u, new Func<RubyContext, BlockParam, object, object>(EnvironmentSingletonOps.DeleteIf));
			LibraryInitializer.DefineLibraryMethod(module, "each", 81, 0u, new Func<RubyContext, BlockParam, object, object>(EnvironmentSingletonOps.Each));
			LibraryInitializer.DefineLibraryMethod(module, "each_key", 81, 0u, new Func<RubyContext, BlockParam, object, object>(EnvironmentSingletonOps.EachKey));
			LibraryInitializer.DefineLibraryMethod(module, "each_pair", 81, 0u, new Func<RubyContext, BlockParam, object, object>(EnvironmentSingletonOps.Each));
			LibraryInitializer.DefineLibraryMethod(module, "each_value", 81, 0u, new Func<RubyContext, BlockParam, object, object>(EnvironmentSingletonOps.EachValue));
			LibraryInitializer.DefineLibraryMethod(module, "empty?", 81, 0u, new Func<RubyContext, object, bool>(EnvironmentSingletonOps.IsEmpty));
			LibraryInitializer.DefineLibraryMethod(module, "fetch", 81, 131076u, new Func<RubyContext, object, MutableString, MutableString>(EnvironmentSingletonOps.GetVariable));
			LibraryInitializer.DefineLibraryMethod(module, "has_key?", 81, 131076u, new Func<RubyContext, object, MutableString, bool>(EnvironmentSingletonOps.HasKey));
			LibraryInitializer.DefineLibraryMethod(module, "has_value?", 81, 0u, new Func<RubyContext, object, object, bool>(EnvironmentSingletonOps.HasValue));
			LibraryInitializer.DefineLibraryMethod(module, "include?", 81, 131076u, new Func<RubyContext, object, MutableString, bool>(EnvironmentSingletonOps.HasKey));
			LibraryInitializer.DefineLibraryMethod(module, "index", 81, 131076u, new Func<RubyContext, object, MutableString, MutableString>(EnvironmentSingletonOps.Index));
			LibraryInitializer.DefineLibraryMethod(module, "indexes", 81, 2147614724u, new Func<RubyContext, object, MutableString[], RubyArray>(EnvironmentSingletonOps.Index));
			LibraryInitializer.DefineLibraryMethod(module, "indices", 81, 2147614724u, new Func<RubyContext, object, MutableString[], RubyArray>(EnvironmentSingletonOps.Indices));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<RubyContext, object, MutableString>(EnvironmentSingletonOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "invert", 81, 0u, new Func<RubyContext, object, Hash>(EnvironmentSingletonOps.Invert));
			LibraryInitializer.DefineLibraryMethod(module, "key?", 81, 131076u, new Func<RubyContext, object, MutableString, bool>(EnvironmentSingletonOps.HasKey));
			LibraryInitializer.DefineLibraryMethod(module, "keys", 81, 0u, new Func<RubyContext, object, RubyArray>(EnvironmentSingletonOps.Keys));
			LibraryInitializer.DefineLibraryMethod(module, "length", 81, 0u, new Func<RubyContext, object, int>(EnvironmentSingletonOps.Length));
			LibraryInitializer.DefineLibraryMethod(module, "rehash", 81, 0u, new Func<object, object>(EnvironmentSingletonOps.Rehash));
			LibraryInitializer.DefineLibraryMethod(module, "reject!", 81, 0u, new Func<RubyContext, BlockParam, object, object>(EnvironmentSingletonOps.DeleteIf));
			LibraryInitializer.DefineLibraryMethod(module, "replace", 81, 4u, new Func<ConversionStorage<MutableString>, object, Hash, object>(EnvironmentSingletonOps.Replace));
			LibraryInitializer.DefineLibraryMethod(module, "shift", 81, 0u, new Func<RubyContext, object, object>(EnvironmentSingletonOps.Shift));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<RubyContext, object, int>(EnvironmentSingletonOps.Length));
			LibraryInitializer.DefineLibraryMethod(module, "store", 81, 393220u, new Func<RubyContext, object, MutableString, MutableString, MutableString>(EnvironmentSingletonOps.SetVariable));
			LibraryInitializer.DefineLibraryMethod(module, "to_hash", 81, 0u, new Func<RubyContext, object, Hash>(EnvironmentSingletonOps.ToHash));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<object, MutableString>(EnvironmentSingletonOps.ToString));
			LibraryInitializer.DefineLibraryMethod(module, "update", 81, 4u, new Func<ConversionStorage<MutableString>, object, Hash, object>(EnvironmentSingletonOps.Update));
			LibraryInitializer.DefineLibraryMethod(module, "value?", 81, 0u, new Func<RubyContext, object, object, bool>(EnvironmentSingletonOps.HasValue));
			LibraryInitializer.DefineLibraryMethod(module, "values", 81, 0u, new Func<RubyContext, object, RubyArray>(EnvironmentSingletonOps.Values));
			LibraryInitializer.DefineLibraryMethod(module, "values_at", 81, 2147614724u, new Func<RubyContext, object, MutableString[], RubyArray>(EnvironmentSingletonOps.ValuesAt));
		}

		private static void LoadException_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "backtrace", 81, 0u, new Func<Exception, RubyArray>(ExceptionOps.GetBacktrace));
			LibraryInitializer.DefineRuleGenerator(module, "exception", 81, ExceptionOps.GetException());
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 82, 0u, new Func<RubyContext, Exception, object, Exception>(ExceptionOps.ReinitializeException));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<UnaryOpStorage, ConversionStorage<MutableString>, Exception, MutableString>(ExceptionOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "message", 81, 0u, new Func<UnaryOpStorage, Exception, object>(ExceptionOps.GetMessage));
			LibraryInitializer.DefineLibraryMethod(module, "set_backtrace", 81, 2u, 0u, new Func<Exception, MutableString, RubyArray>(ExceptionOps.SetBacktrace), new Func<Exception, RubyArray, RubyArray>(ExceptionOps.SetBacktrace));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<Exception, object>(ExceptionOps.StringRepresentation));
			LibraryInitializer.DefineLibraryMethod(module, "to_str", 81, 0u, new Func<Exception, object>(ExceptionOps.StringRepresentation));
		}

		private static void LoadException_Class(RubyModule module)
		{
			LibraryInitializer.DefineRuleGenerator(module, "exception", 97, ExceptionOps.CreateException());
		}

		private static void LoadFalseClass_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "&", 81, 0u, new Func<bool, object, bool>(FalseClass.And));
			LibraryInitializer.DefineLibraryMethod(module, "^", 81, 0u, 0u, new Func<bool, object, bool>(FalseClass.Xor), new Func<bool, bool, bool>(FalseClass.Xor));
			LibraryInitializer.DefineLibraryMethod(module, "|", 81, 0u, 0u, new Func<bool, object, bool>(FalseClass.Or), new Func<bool, bool, bool>(FalseClass.Or));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<bool, MutableString>(FalseClass.ToString));
		}

		private static void LoadFile_Constants(RubyModule module)
		{
			LibraryInitializer.SetBuiltinConstant(module, "ALT_SEPARATOR", RubyFileOps.ALT_SEPARATOR);
			LibraryInitializer.SetBuiltinConstant(module, "PATH_SEPARATOR", RubyFileOps.PATH_SEPARATOR);
			LibraryInitializer.SetBuiltinConstant(module, "Separator", RubyFileOps.Separator);
			LibraryInitializer.SetBuiltinConstant(module, "SEPARATOR", RubyFileOps.SEPARATOR);
		}

		private static void LoadFile_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "atime", 81, 0u, new Func<RubyContext, RubyFile, RubyTime>(RubyFileOps.AccessTime));
			LibraryInitializer.DefineLibraryMethod(module, "chmod", 81, 65536u, new Func<RubyFile, int, int>(RubyFileOps.Chmod));
			LibraryInitializer.DefineLibraryMethod(module, "chown", 81, 196608u, 0u, new Func<RubyFile, int, int, int>(RubyFileOps.ChangeOwner), new Func<RubyContext, RubyFile, object, object, int>(RubyFileOps.ChangeOwner));
			LibraryInitializer.DefineLibraryMethod(module, "ctime", 81, 0u, new Func<RubyContext, RubyFile, RubyTime>(RubyFileOps.CreateTime));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 82, 8388608u, new Func<ConversionStorage<int?>, ConversionStorage<IDictionary<object, object>>, ConversionStorage<MutableString>, ConversionStorage<MutableString>, RubyFile, object, object, object, IDictionary<object, object>, RubyFile>(RubyFileOps.Reinitialize));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<RubyFile, MutableString>(RubyFileOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "lstat", 81, 0u, new Func<RubyFile, FileSystemInfo>(RubyFileOps.Stat));
			LibraryInitializer.DefineLibraryMethod(module, "mtime", 81, 0u, new Func<RubyContext, RubyFile, RubyTime>(RubyFileOps.ModifiedTime));
			LibraryInitializer.DefineLibraryMethod(module, "path", 81, 0u, new Func<RubyFile, MutableString>(RubyFileOps.GetPath));
			LibraryInitializer.DefineLibraryMethod(module, "stat", 81, 0u, new Func<RubyFile, FileSystemInfo>(RubyFileOps.Stat));
			LibraryInitializer.DefineLibraryMethod(module, "to_path", 81, 0u, new Func<RubyFile, MutableString>(RubyFileOps.GetPath));
			LibraryInitializer.DefineLibraryMethod(module, "truncate", 81, 65536u, new Func<RubyFile, int, int>(RubyFileOps.Truncate));
		}

		private static void LoadFile_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "absolute_path", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, object, MutableString>(RubyFileOps.AbsolutePath));
			LibraryInitializer.DefineLibraryMethod(module, "atime", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, RubyTime>(RubyFileOps.AccessTime));
			LibraryInitializer.DefineLibraryMethod(module, "basename", 97, 262152u, new Func<ConversionStorage<MutableString>, RubyClass, object, MutableString, MutableString>(RubyFileOps.BaseName));
			LibraryInitializer.DefineLibraryMethod(module, "blockdev?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.IsBlockDevice));
			LibraryInitializer.DefineLibraryMethod(module, "chardev?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.IsCharDevice));
			LibraryInitializer.DefineLibraryMethod(module, "chmod", 97, 131072u, new Func<ConversionStorage<MutableString>, RubyClass, int, object, int>(RubyFileOps.Chmod));
			LibraryInitializer.DefineLibraryMethod(module, "chown", 97, 458760u, 524304u, new Func<RubyClass, int, int, MutableString, int>(RubyFileOps.ChangeOwner), new Func<RubyContext, RubyClass, object, object, MutableString, int>(RubyFileOps.ChangeOwner));
			LibraryInitializer.DefineLibraryMethod(module, "ctime", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, RubyTime>(RubyFileOps.CreateTime));
			LibraryInitializer.DefineLibraryMethod(module, "delete", 97, 0u, 2147483648u, new Func<ConversionStorage<MutableString>, RubyClass, object, int>(RubyFileOps.Delete), new Func<ConversionStorage<MutableString>, RubyClass, object[], int>(RubyFileOps.Delete));
			LibraryInitializer.DefineLibraryMethod(module, "directory?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.IsDirectory));
			LibraryInitializer.DefineLibraryMethod(module, "dirname", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, MutableString>(RubyFileOps.DirName));
			LibraryInitializer.DefineLibraryMethod(module, "executable?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.IsExecutable));
			LibraryInitializer.DefineLibraryMethod(module, "executable_real?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.IsExecutable));
			LibraryInitializer.DefineLibraryMethod(module, "exist?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.Exists));
			LibraryInitializer.DefineLibraryMethod(module, "exists?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.Exists));
			LibraryInitializer.DefineLibraryMethod(module, "expand_path", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, object, MutableString>(RubyFileOps.ExpandPath));
			LibraryInitializer.DefineLibraryMethod(module, "extname", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, MutableString>(RubyFileOps.GetExtension));
			LibraryInitializer.DefineLibraryMethod(module, "file?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.IsFile));
			LibraryInitializer.DefineLibraryMethod(module, "fnmatch", 97, 131076u, new Func<ConversionStorage<MutableString>, object, MutableString, object, int, bool>(RubyFileOps.FnMatch));
			LibraryInitializer.DefineLibraryMethod(module, "fnmatch?", 97, 131076u, new Func<ConversionStorage<MutableString>, object, MutableString, object, int, bool>(RubyFileOps.FnMatch));
			LibraryInitializer.DefineLibraryMethod(module, "ftype", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, MutableString>(RubyFileOps.FileType));
			LibraryInitializer.DefineLibraryMethod(module, "grpowned?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.IsGroupOwned));
			LibraryInitializer.DefineLibraryMethod(module, "identical?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, object, bool>(RubyFileOps.AreIdentical));
			LibraryInitializer.DefineLibraryMethod(module, "join", 97, 2147483648u, new Func<ConversionStorage<MutableString>, RubyClass, object[], MutableString>(RubyFileOps.Join));
			LibraryInitializer.DefineLibraryMethod(module, "link", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, object, int>(RubyFileOps.Link));
			LibraryInitializer.DefineLibraryMethod(module, "lstat", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, FileSystemInfo>(RubyFileOps.Stat));
			LibraryInitializer.DefineLibraryMethod(module, "mtime", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, RubyTime>(RubyFileOps.ModifiedTime));
			LibraryInitializer.DefineRuleGenerator(module, "open", 97, RubyFileOps.Open());
			LibraryInitializer.DefineLibraryMethod(module, "owned?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.IsUserOwned));
			LibraryInitializer.DefineLibraryMethod(module, "path", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, MutableString>(RubyFileOps.ToPath));
			LibraryInitializer.DefineLibraryMethod(module, "pipe?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.IsPipe));
			LibraryInitializer.DefineLibraryMethod(module, "readable?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.IsReadable));
			LibraryInitializer.DefineLibraryMethod(module, "readable_real?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.IsReadable));
			LibraryInitializer.DefineLibraryMethod(module, "readlink", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, bool>(RubyFileOps.Readlink));
			LibraryInitializer.DefineLibraryMethod(module, "rename", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, object, int>(RubyFileOps.Rename));
			LibraryInitializer.DefineLibraryMethod(module, "setgid?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.IsSetGid));
			LibraryInitializer.DefineLibraryMethod(module, "setuid?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.IsSetUid));
			LibraryInitializer.DefineLibraryMethod(module, "size", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, int>(RubyFileOps.Size));
			LibraryInitializer.DefineLibraryMethod(module, "size?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, object>(RubyFileOps.NullableSize));
			LibraryInitializer.DefineLibraryMethod(module, "socket?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.IsSocket));
			LibraryInitializer.DefineLibraryMethod(module, "split", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, RubyArray>(RubyFileOps.Split));
			LibraryInitializer.DefineLibraryMethod(module, "stat", 97, 0u, new Func<ConversionStorage<MutableString>, RubyClass, object, FileSystemInfo>(RubyFileOps.Stat));
			LibraryInitializer.DefineLibraryMethod(module, "sticky?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, object>(RubyFileOps.IsSticky));
			LibraryInitializer.DefineLibraryMethod(module, "symlink", 97, 65538u, new Func<RubyClass, MutableString, object>(RubyFileOps.SymLink));
			LibraryInitializer.DefineLibraryMethod(module, "symlink?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.IsSymLink));
			LibraryInitializer.DefineLibraryMethod(module, "truncate", 97, 262144u, new Func<ConversionStorage<MutableString>, RubyClass, object, int, int>(RubyFileOps.Truncate));
			LibraryInitializer.DefineLibraryMethod(module, "umask", 97, 65536u, 0u, new Func<RubyClass, int, int>(RubyFileOps.GetUmask), new Func<RubyClass, int>(RubyFileOps.GetUmask));
			LibraryInitializer.DefineLibraryMethod(module, "unlink", 97, 0u, 2147483648u, new Func<ConversionStorage<MutableString>, RubyClass, object, int>(RubyFileOps.Delete), new Func<ConversionStorage<MutableString>, RubyClass, object[], int>(RubyFileOps.Delete));
			LibraryInitializer.DefineLibraryMethod(module, "utime", 97, 12u, 2147483648u, new Func<ConversionStorage<MutableString>, RubyClass, RubyTime, RubyTime, object, int>(RubyFileOps.UpdateTimes), new Func<ConversionStorage<MutableString>, RubyClass, object, object, object[], int>(RubyFileOps.UpdateTimes));
			LibraryInitializer.DefineLibraryMethod(module, "writable?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.IsWritable));
			LibraryInitializer.DefineLibraryMethod(module, "writable_real?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.IsWritable));
			LibraryInitializer.DefineLibraryMethod(module, "zero?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(RubyFileOps.IsZeroLength));
		}

		private static void LoadFile__Constants_Constants(RubyModule module)
		{
			LibraryInitializer.SetBuiltinConstant(module, "APPEND", RubyFileOps.Constants.APPEND);
			LibraryInitializer.SetBuiltinConstant(module, "BINARY", RubyFileOps.Constants.BINARY);
			LibraryInitializer.SetBuiltinConstant(module, "CREAT", RubyFileOps.Constants.CREAT);
			LibraryInitializer.SetBuiltinConstant(module, "EXCL", RubyFileOps.Constants.EXCL);
			LibraryInitializer.SetBuiltinConstant(module, "FNM_CASEFOLD", RubyFileOps.Constants.FNM_CASEFOLD);
			LibraryInitializer.SetBuiltinConstant(module, "FNM_DOTMATCH", RubyFileOps.Constants.FNM_DOTMATCH);
			LibraryInitializer.SetBuiltinConstant(module, "FNM_NOESCAPE", RubyFileOps.Constants.FNM_NOESCAPE);
			LibraryInitializer.SetBuiltinConstant(module, "FNM_PATHNAME", RubyFileOps.Constants.FNM_PATHNAME);
			LibraryInitializer.SetBuiltinConstant(module, "FNM_SYSCASE", RubyFileOps.Constants.FNM_SYSCASE);
			LibraryInitializer.SetBuiltinConstant(module, "LOCK_EX", RubyFileOps.Constants.LOCK_EX);
			LibraryInitializer.SetBuiltinConstant(module, "LOCK_NB", RubyFileOps.Constants.LOCK_NB);
			LibraryInitializer.SetBuiltinConstant(module, "LOCK_SH", RubyFileOps.Constants.LOCK_SH);
			LibraryInitializer.SetBuiltinConstant(module, "LOCK_UN", RubyFileOps.Constants.LOCK_UN);
			LibraryInitializer.SetBuiltinConstant(module, "NONBLOCK", RubyFileOps.Constants.NONBLOCK);
			LibraryInitializer.SetBuiltinConstant(module, "RDONLY", RubyFileOps.Constants.RDONLY);
			LibraryInitializer.SetBuiltinConstant(module, "RDWR", RubyFileOps.Constants.RDWR);
			LibraryInitializer.SetBuiltinConstant(module, "TRUNC", RubyFileOps.Constants.TRUNC);
			LibraryInitializer.SetBuiltinConstant(module, "WRONLY", RubyFileOps.Constants.WRONLY);
		}

		private static void LoadFile__Stat_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "<=>", 81, 2u, 0u, new Func<FileSystemInfo, FileSystemInfo, int>(RubyFileOps.RubyStatOps.Compare), new Func<FileSystemInfo, object, object>(RubyFileOps.RubyStatOps.Compare));
			LibraryInitializer.DefineLibraryMethod(module, "atime", 81, 0u, new Func<FileSystemInfo, RubyTime>(RubyFileOps.RubyStatOps.AccessTime));
			LibraryInitializer.DefineLibraryMethod(module, "blksize", 81, 0u, new Func<FileSystemInfo, object>(RubyFileOps.RubyStatOps.BlockSize));
			LibraryInitializer.DefineLibraryMethod(module, "blockdev?", 81, 0u, new Func<FileSystemInfo, bool>(RubyFileOps.RubyStatOps.IsBlockDevice));
			LibraryInitializer.DefineLibraryMethod(module, "blocks", 81, 0u, new Func<FileSystemInfo, object>(RubyFileOps.RubyStatOps.Blocks));
			LibraryInitializer.DefineLibraryMethod(module, "chardev?", 81, 0u, new Func<FileSystemInfo, bool>(RubyFileOps.RubyStatOps.IsCharDevice));
			LibraryInitializer.DefineLibraryMethod(module, "ctime", 81, 0u, new Func<FileSystemInfo, RubyTime>(RubyFileOps.RubyStatOps.CreateTime));
			LibraryInitializer.DefineLibraryMethod(module, "dev", 81, 0u, new Func<FileSystemInfo, object>(RubyFileOps.RubyStatOps.DeviceId));
			LibraryInitializer.DefineLibraryMethod(module, "dev_major", 81, 0u, new Func<FileSystemInfo, object>(RubyFileOps.RubyStatOps.DeviceIdMajor));
			LibraryInitializer.DefineLibraryMethod(module, "dev_minor", 81, 0u, new Func<FileSystemInfo, object>(RubyFileOps.RubyStatOps.DeviceIdMinor));
			LibraryInitializer.DefineLibraryMethod(module, "directory?", 81, 0u, new Func<FileSystemInfo, bool>(RubyFileOps.RubyStatOps.IsDirectory));
			LibraryInitializer.DefineLibraryMethod(module, "executable?", 81, 0u, new Func<FileSystemInfo, bool>(RubyFileOps.RubyStatOps.IsExecutable));
			LibraryInitializer.DefineLibraryMethod(module, "executable_real?", 81, 0u, new Func<FileSystemInfo, bool>(RubyFileOps.RubyStatOps.IsExecutable));
			LibraryInitializer.DefineLibraryMethod(module, "file?", 81, 0u, new Func<FileSystemInfo, bool>(RubyFileOps.RubyStatOps.IsFile));
			LibraryInitializer.DefineLibraryMethod(module, "ftype", 81, 0u, new Func<FileSystemInfo, MutableString>(RubyFileOps.RubyStatOps.FileType));
			LibraryInitializer.DefineLibraryMethod(module, "gid", 81, 0u, new Func<FileSystemInfo, int>(RubyFileOps.RubyStatOps.GroupId));
			LibraryInitializer.DefineLibraryMethod(module, "grpowned?", 81, 0u, new Func<FileSystemInfo, bool>(RubyFileOps.RubyStatOps.IsGroupOwned));
			LibraryInitializer.DefineLibraryMethod(module, "identical?", 81, 4u, new Func<RubyContext, FileSystemInfo, FileSystemInfo, bool>(RubyFileOps.RubyStatOps.AreIdentical));
			LibraryInitializer.DefineLibraryMethod(module, "ino", 81, 0u, new Func<FileSystemInfo, int>(RubyFileOps.RubyStatOps.Inode));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<RubyContext, FileSystemInfo, MutableString>(RubyFileOps.RubyStatOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "mode", 81, 0u, new Func<FileSystemInfo, int>(RubyFileOps.RubyStatOps.Mode));
			LibraryInitializer.DefineLibraryMethod(module, "mtime", 81, 0u, new Func<FileSystemInfo, RubyTime>(RubyFileOps.RubyStatOps.ModifiedTime));
			LibraryInitializer.DefineLibraryMethod(module, "nlink", 81, 0u, new Func<FileSystemInfo, int>(RubyFileOps.RubyStatOps.NumberOfLinks));
			LibraryInitializer.DefineLibraryMethod(module, "owned?", 81, 0u, new Func<FileSystemInfo, bool>(RubyFileOps.RubyStatOps.IsUserOwned));
			LibraryInitializer.DefineLibraryMethod(module, "pipe?", 81, 0u, new Func<FileSystemInfo, bool>(RubyFileOps.RubyStatOps.IsPipe));
			LibraryInitializer.DefineLibraryMethod(module, "rdev", 81, 0u, new Func<FileSystemInfo, object>(RubyFileOps.RubyStatOps.DeviceId));
			LibraryInitializer.DefineLibraryMethod(module, "rdev_major", 81, 0u, new Func<FileSystemInfo, object>(RubyFileOps.RubyStatOps.DeviceIdMajor));
			LibraryInitializer.DefineLibraryMethod(module, "rdev_minor", 81, 0u, new Func<FileSystemInfo, object>(RubyFileOps.RubyStatOps.DeviceIdMinor));
			LibraryInitializer.DefineLibraryMethod(module, "readable?", 81, 0u, new Func<FileSystemInfo, bool>(RubyFileOps.RubyStatOps.IsReadable));
			LibraryInitializer.DefineLibraryMethod(module, "readable_real?", 81, 0u, new Func<FileSystemInfo, bool>(RubyFileOps.RubyStatOps.IsReadable));
			LibraryInitializer.DefineLibraryMethod(module, "setgid?", 81, 0u, new Func<FileSystemInfo, bool>(RubyFileOps.RubyStatOps.IsSetGid));
			LibraryInitializer.DefineLibraryMethod(module, "setuid?", 81, 0u, new Func<FileSystemInfo, bool>(RubyFileOps.RubyStatOps.IsSetUid));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<FileSystemInfo, int>(RubyFileOps.RubyStatOps.Size));
			LibraryInitializer.DefineLibraryMethod(module, "size?", 81, 0u, new Func<FileSystemInfo, object>(RubyFileOps.RubyStatOps.NullableSize));
			LibraryInitializer.DefineLibraryMethod(module, "socket?", 81, 0u, new Func<FileSystemInfo, bool>(RubyFileOps.RubyStatOps.IsSocket));
			LibraryInitializer.DefineLibraryMethod(module, "sticky?", 81, 0u, new Func<FileSystemInfo, object>(RubyFileOps.RubyStatOps.IsSticky));
			LibraryInitializer.DefineLibraryMethod(module, "symlink?", 81, 0u, new Func<FileSystemInfo, bool>(RubyFileOps.RubyStatOps.IsSymLink));
			LibraryInitializer.DefineLibraryMethod(module, "uid", 81, 0u, new Func<FileSystemInfo, int>(RubyFileOps.RubyStatOps.UserId));
			LibraryInitializer.DefineLibraryMethod(module, "writable?", 81, 0u, new Func<FileSystemInfo, bool>(RubyFileOps.RubyStatOps.IsWritable));
			LibraryInitializer.DefineLibraryMethod(module, "writable_real?", 81, 0u, new Func<FileSystemInfo, bool>(RubyFileOps.RubyStatOps.IsWritable));
			LibraryInitializer.DefineLibraryMethod(module, "zero?", 81, 0u, new Func<FileSystemInfo, bool>(RubyFileOps.RubyStatOps.IsZeroLength));
		}

		private static void LoadFileTest_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "blockdev?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsBlockDevice));
			LibraryInitializer.DefineLibraryMethod(module, "chardev?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsCharDevice));
			LibraryInitializer.DefineLibraryMethod(module, "directory?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsDirectory));
			LibraryInitializer.DefineLibraryMethod(module, "executable?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsExecutable));
			LibraryInitializer.DefineLibraryMethod(module, "executable_real?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsExecutable));
			LibraryInitializer.DefineLibraryMethod(module, "exist?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.Exists));
			LibraryInitializer.DefineLibraryMethod(module, "exists?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.Exists));
			LibraryInitializer.DefineLibraryMethod(module, "file?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsFile));
			LibraryInitializer.DefineLibraryMethod(module, "grpowned?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsGroupOwned));
			LibraryInitializer.DefineLibraryMethod(module, "identical?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, object, bool>(FileTest.AreIdentical));
			LibraryInitializer.DefineLibraryMethod(module, "owned?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsUserOwned));
			LibraryInitializer.DefineLibraryMethod(module, "pipe?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsPipe));
			LibraryInitializer.DefineLibraryMethod(module, "readable?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsReadable));
			LibraryInitializer.DefineLibraryMethod(module, "readable_real?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsReadable));
			LibraryInitializer.DefineLibraryMethod(module, "setgid?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsSetGid));
			LibraryInitializer.DefineLibraryMethod(module, "setuid?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsSetUid));
			LibraryInitializer.DefineLibraryMethod(module, "size", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, int>(FileTest.Size));
			LibraryInitializer.DefineLibraryMethod(module, "size?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, object>(FileTest.NullableSize));
			LibraryInitializer.DefineLibraryMethod(module, "socket?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsSocket));
			LibraryInitializer.DefineLibraryMethod(module, "sticky?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, object>(FileTest.IsSticky));
			LibraryInitializer.DefineLibraryMethod(module, "symlink?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsSymLink));
			LibraryInitializer.DefineLibraryMethod(module, "writable?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsWritable));
			LibraryInitializer.DefineLibraryMethod(module, "writable_real?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsWritable));
			LibraryInitializer.DefineLibraryMethod(module, "zero?", 82, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsZeroLength));
		}

		private static void LoadFileTest_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "blockdev?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsBlockDevice));
			LibraryInitializer.DefineLibraryMethod(module, "chardev?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsCharDevice));
			LibraryInitializer.DefineLibraryMethod(module, "directory?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsDirectory));
			LibraryInitializer.DefineLibraryMethod(module, "executable?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsExecutable));
			LibraryInitializer.DefineLibraryMethod(module, "executable_real?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsExecutable));
			LibraryInitializer.DefineLibraryMethod(module, "exist?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.Exists));
			LibraryInitializer.DefineLibraryMethod(module, "exists?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.Exists));
			LibraryInitializer.DefineLibraryMethod(module, "file?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsFile));
			LibraryInitializer.DefineLibraryMethod(module, "grpowned?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsGroupOwned));
			LibraryInitializer.DefineLibraryMethod(module, "identical?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, object, bool>(FileTest.AreIdentical));
			LibraryInitializer.DefineLibraryMethod(module, "owned?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsUserOwned));
			LibraryInitializer.DefineLibraryMethod(module, "pipe?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsPipe));
			LibraryInitializer.DefineLibraryMethod(module, "readable?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsReadable));
			LibraryInitializer.DefineLibraryMethod(module, "readable_real?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsReadable));
			LibraryInitializer.DefineLibraryMethod(module, "setgid?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsSetGid));
			LibraryInitializer.DefineLibraryMethod(module, "setuid?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsSetUid));
			LibraryInitializer.DefineLibraryMethod(module, "size", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, int>(FileTest.Size));
			LibraryInitializer.DefineLibraryMethod(module, "size?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, object>(FileTest.NullableSize));
			LibraryInitializer.DefineLibraryMethod(module, "socket?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsSocket));
			LibraryInitializer.DefineLibraryMethod(module, "sticky?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, object>(FileTest.IsSticky));
			LibraryInitializer.DefineLibraryMethod(module, "symlink?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsSymLink));
			LibraryInitializer.DefineLibraryMethod(module, "writable?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsWritable));
			LibraryInitializer.DefineLibraryMethod(module, "writable_real?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsWritable));
			LibraryInitializer.DefineLibraryMethod(module, "zero?", 97, 0u, new Func<ConversionStorage<MutableString>, RubyModule, object, bool>(FileTest.IsZeroLength));
		}

		private static void LoadFixnum_Constants(RubyModule module)
		{
		}

		private static void LoadFixnum_Instance(RubyModule module)
		{
			LoadIronRuby__Clr__Integer_Instance(module);
			LibraryInitializer.DefineLibraryMethod(module, "id2name", 81, 0u, new Func<RubyContext, int, object>(Int32Ops.Id2Name));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<int, int>(Int32Ops.Size));
			LibraryInitializer.DefineLibraryMethod(module, "to_sym", 81, 0u, new Func<RubyContext, int, object>(Int32Ops.ToSymbol));
		}

		private static void LoadFixnum_Class(RubyModule module)
		{
			module.UndefineMethodNoEvent("new");
			LibraryInitializer.DefineLibraryMethod(module, "induced_from", 97, 65536u, 0u, new Func<RubyClass, int, int>(Int32Ops.InducedFrom), new Func<RubyClass, double, int>(Int32Ops.InducedFrom));
		}

		private static void LoadFloat_Constants(RubyModule module)
		{
			LibraryInitializer.SetBuiltinConstant(module, "DIG", 15);
			LibraryInitializer.SetBuiltinConstant(module, "EPSILON", 2.2204460492503131E-16);
			LibraryInitializer.SetBuiltinConstant(module, "MANT_DIG", 53);
			LibraryInitializer.SetBuiltinConstant(module, "MAX", double.MaxValue);
			LibraryInitializer.SetBuiltinConstant(module, "MAX_10_EXP", 308.0);
			LibraryInitializer.SetBuiltinConstant(module, "MAX_EXP", 1024);
			LibraryInitializer.SetBuiltinConstant(module, "MIN", 2.2250738585072014E-308);
			LibraryInitializer.SetBuiltinConstant(module, "MIN_10_EXP", -307.0);
			LibraryInitializer.SetBuiltinConstant(module, "MIN_EXP", -1021);
			LibraryInitializer.SetBuiltinConstant(module, "RADIX", 2);
			LibraryInitializer.SetBuiltinConstant(module, "ROUNDS", 1);
		}

		private static void LoadFloat_Instance(RubyModule module)
		{
			LoadIronRuby__Clr__Float_Instance(module);
		}

		private static void LoadFloat_Class(RubyModule module)
		{
			LoadIronRuby__Clr__Float_Class(module);
			module.UndefineMethodNoEvent("new");
		}

		private static void LoadGC_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "garbage_collect", 81, 0u, new Action<object>(RubyGC.GarbageCollect));
		}

		private static void LoadGC_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "disable", 97, 0u, new Func<object, bool>(RubyGC.Disable));
			LibraryInitializer.DefineLibraryMethod(module, "enable", 97, 0u, new Func<object, bool>(RubyGC.Enable));
			LibraryInitializer.DefineLibraryMethod(module, "start", 97, 0u, new Action<object>(RubyGC.GarbageCollect));
		}

		private static void LoadHash_Constants(RubyModule module)
		{
		}

		private static void LoadHash_Instance(RubyModule module)
		{
			LoadSystem__Collections__Generic__IDictionary_Instance(module);
			LibraryInitializer.DefineLibraryMethod(module, "[]", 81, 0u, new Func<BinaryOpStorage, IDictionary<object, object>, object, object>(HashOps.GetElement));
			LibraryInitializer.DefineLibraryMethod(module, "default", 81, 0u, 0u, new Func<RubyContext, Hash, object>(HashOps.GetDefaultValue), new Func<CallSiteStorage<Func<CallSite, Proc, Hash, object, object>>, Hash, object, object>(HashOps.GetDefaultValue));
			LibraryInitializer.DefineLibraryMethod(module, "default_proc", 81, 0u, new Func<Hash, Proc>(HashOps.GetDefaultProc));
			LibraryInitializer.DefineLibraryMethod(module, "default=", 81, 0u, new Func<RubyContext, Hash, object, object>(HashOps.SetDefaultValue));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 82, 0u, 0u, 1u, new Func<Hash, Hash>(HashOps.Initialize), new Func<BlockParam, Hash, object, Hash>(HashOps.Initialize), new Func<BlockParam, Hash, Hash>(HashOps.Initialize));
			LibraryInitializer.DefineLibraryMethod(module, "initialize_copy", 82, 4u, new Func<RubyContext, Hash, Hash, Hash>(HashOps.InitializeCopy));
			LibraryInitializer.DefineLibraryMethod(module, "replace", 81, 131076u, new Func<RubyContext, Hash, IDictionary<object, object>, Hash>(HashOps.Replace));
			LibraryInitializer.DefineLibraryMethod(module, "shift", 81, 0u, new Func<CallSiteStorage<Func<CallSite, Hash, object, object>>, Hash, object>(HashOps.Shift));
		}

		private static void LoadHash_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "[]", 97, new uint[5] { 0u, 0u, 4u, 2u, 2147483648u }, new Func<RubyClass, Hash>(HashOps.CreateSubclass), new Func<ConversionStorage<IDictionary<object, object>>, ConversionStorage<IList>, RubyClass, object, Hash>(HashOps.CreateSubclass), new Func<ConversionStorage<IList>, RubyClass, IList, Hash>(HashOps.CreateSubclass), new Func<RubyClass, IDictionary<object, object>, Hash>(HashOps.CreateSubclass), new Func<RubyClass, object[], Hash>(HashOps.CreateSubclass));
			LibraryInitializer.DefineLibraryMethod(module, "try_convert", 97, 0u, new Func<ConversionStorage<IDictionary<object, object>>, RubyClass, object, IDictionary<object, object>>(HashOps.TryConvert));
		}

		private static void LoadInteger_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "ceil", 81, 0u, new Func<object, object>(Integer.ToInteger));
			LibraryInitializer.DefineLibraryMethod(module, "chr", 81, 65536u, new Func<ConversionStorage<MutableString>, int, object, MutableString>(Integer.ToChr));
			LibraryInitializer.DefineLibraryMethod(module, "denominator", 81, 0u, new Func<object, object>(Integer.Denominator));
			LibraryInitializer.DefineLibraryMethod(module, "downto", 81, 0u, 0u, new Func<BlockParam, int, int, object>(Integer.DownTo), new Func<BinaryOpStorage, BinaryOpStorage, BlockParam, object, object, object>(Integer.DownTo));
			LibraryInitializer.DefineLibraryMethod(module, "even?", 81, 0u, 0u, new Func<int, bool>(Integer.IsEven), new Func<BigInteger, bool>(Integer.IsEven));
			LibraryInitializer.DefineLibraryMethod(module, "floor", 81, 0u, new Func<object, object>(Integer.ToInteger));
			LibraryInitializer.DefineLibraryMethod(module, "gcd", 81, 0u, 0u, 0u, new Func<int, int, object>(Integer.Gcd), new Func<BigInteger, BigInteger, object>(Integer.Gcd), new Func<object, object, object>(Integer.Gcd));
			LibraryInitializer.DefineLibraryMethod(module, "gcdlcm", 81, 0u, 0u, 0u, new Func<int, int, RubyArray>(Integer.GcdLcm), new Func<BigInteger, BigInteger, RubyArray>(Integer.GcdLcm), new Func<object, object, RubyArray>(Integer.GcdLcm));
			LibraryInitializer.DefineLibraryMethod(module, "integer?", 81, 0u, new Func<object, bool>(Integer.IsInteger));
			LibraryInitializer.DefineLibraryMethod(module, "lcm", 81, 0u, 0u, 0u, new Func<int, int, object>(Integer.Lcm), new Func<BigInteger, BigInteger, object>(Integer.Lcm), new Func<object, object, object>(Integer.Lcm));
			LibraryInitializer.DefineLibraryMethod(module, "next", 81, 0u, 0u, new Func<int, object>(Integer.Next), new Func<BinaryOpStorage, object, object>(Integer.Next));
			LibraryInitializer.DefineLibraryMethod(module, "numerator", 81, 0u, new Func<object, object>(Integer.Numerator));
			LibraryInitializer.DefineLibraryMethod(module, "odd?", 81, 0u, 0u, new Func<int, bool>(Integer.IsOdd), new Func<BigInteger, bool>(Integer.IsOdd));
			LibraryInitializer.DefineLibraryMethod(module, "ord", 81, 0u, new Func<object, object>(Integer.Numerator));
			LibraryInitializer.DefineLibraryMethod(module, "pred", 81, 0u, 0u, new Func<int, object>(Integer.Pred), new Func<BinaryOpStorage, object, object>(Integer.Pred));
			LibraryInitializer.DefineLibraryMethod(module, "rationalize", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, object, object, object>>, RubyScope, object, object>(Integer.ToRational));
			LibraryInitializer.DefineLibraryMethod(module, "round", 81, 0u, new Func<object, object>(Integer.ToInteger));
			LibraryInitializer.DefineLibraryMethod(module, "succ", 81, 0u, 0u, new Func<int, object>(Integer.Next), new Func<BinaryOpStorage, object, object>(Integer.Next));
			LibraryInitializer.DefineLibraryMethod(module, "times", 81, 0u, 0u, new Func<BlockParam, int, object>(Integer.Times), new Func<BinaryOpStorage, BinaryOpStorage, BlockParam, object, object>(Integer.Times));
			LibraryInitializer.DefineLibraryMethod(module, "to_i", 81, 0u, new Func<object, object>(Integer.ToInteger));
			LibraryInitializer.DefineLibraryMethod(module, "to_int", 81, 0u, new Func<object, object>(Integer.ToInteger));
			LibraryInitializer.DefineLibraryMethod(module, "to_r", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, object, object, object>>, RubyScope, object, object>(Integer.ToRational));
			LibraryInitializer.DefineLibraryMethod(module, "truncate", 81, 0u, new Func<object, object>(Integer.ToInteger));
			LibraryInitializer.DefineLibraryMethod(module, "upto", 81, 0u, 0u, new Func<BlockParam, int, int, object>(Integer.UpTo), new Func<BinaryOpStorage, BinaryOpStorage, BlockParam, object, object, object>(Integer.UpTo));
		}

		private static void LoadInteger_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "induced_from", 97, 0u, 2u, 0u, 0u, new Func<RubyClass, int, object>(Integer.InducedFrom), new Func<RubyClass, BigInteger, object>(Integer.InducedFrom), new Func<UnaryOpStorage, RubyClass, double, object>(Integer.InducedFrom), new Func<RubyClass, object, int>(Integer.InducedFrom));
		}

		private static void LoadIO_Constants(RubyModule module)
		{
			LibraryInitializer.SetBuiltinConstant(module, "SEEK_CUR", 1);
			LibraryInitializer.SetBuiltinConstant(module, "SEEK_END", 2);
			LibraryInitializer.SetBuiltinConstant(module, "SEEK_SET", 0);
		}

		private static void LoadIO_Instance(RubyModule module)
		{
			LoadIronRuby__Print_Instance(module);
			LibraryInitializer.DefineLibraryMethod(module, "binmode", 81, 0u, new Func<RubyIO, RubyIO>(RubyIOOps.Binmode));
			LibraryInitializer.DefineLibraryMethod(module, "close", 81, 0u, new Action<RubyIO>(RubyIOOps.Close));
			LibraryInitializer.DefineLibraryMethod(module, "close_read", 81, 0u, new Action<RubyIO>(RubyIOOps.CloseReader));
			LibraryInitializer.DefineLibraryMethod(module, "close_write", 81, 0u, new Action<RubyIO>(RubyIOOps.CloseWriter));
			LibraryInitializer.DefineLibraryMethod(module, "closed?", 81, 0u, new Func<RubyIO, bool>(RubyIOOps.Closed));
			LibraryInitializer.DefineLibraryMethod(module, "each", 81, 0u, 0u, 262152u, 786432u, new Func<RubyContext, BlockParam, RubyIO, object>(RubyIOOps.Each), new Func<RubyContext, BlockParam, RubyIO, DynamicNull, object>(RubyIOOps.Each), new Func<RubyContext, BlockParam, RubyIO, Union<MutableString, int>, object>(RubyIOOps.Each), new Func<RubyContext, BlockParam, RubyIO, MutableString, int, object>(RubyIOOps.Each));
			LibraryInitializer.DefineLibraryMethod(module, "each_byte", 81, 0u, new Func<BlockParam, RubyIO, object>(RubyIOOps.EachByte));
			LibraryInitializer.DefineLibraryMethod(module, "each_line", 81, 0u, 0u, 262152u, 786432u, new Func<RubyContext, BlockParam, RubyIO, object>(RubyIOOps.Each), new Func<RubyContext, BlockParam, RubyIO, DynamicNull, object>(RubyIOOps.Each), new Func<RubyContext, BlockParam, RubyIO, Union<MutableString, int>, object>(RubyIOOps.Each), new Func<RubyContext, BlockParam, RubyIO, MutableString, int, object>(RubyIOOps.Each));
			LibraryInitializer.DefineLibraryMethod(module, "eof", 81, 0u, new Func<RubyIO, bool>(RubyIOOps.Eof));
			LibraryInitializer.DefineLibraryMethod(module, "eof?", 81, 0u, new Func<RubyIO, bool>(RubyIOOps.Eof));
			LibraryInitializer.DefineLibraryMethod(module, "external_encoding", 81, 0u, new Func<RubyIO, RubyEncoding>(RubyIOOps.GetExternalEncoding));
			LibraryInitializer.DefineLibraryMethod(module, "fcntl", 81, 65536u, 65536u, new Func<RubyIO, int, MutableString, int>(RubyIOOps.FileControl), new Func<RubyIO, int, int, int>(RubyIOOps.FileControl));
			LibraryInitializer.DefineLibraryMethod(module, "fileno", 81, 0u, new Func<RubyIO, int>(RubyIOOps.FileNo));
			LibraryInitializer.DefineLibraryMethod(module, "flush", 81, 0u, new Action<RubyIO>(RubyIOOps.Flush));
			LibraryInitializer.DefineLibraryMethod(module, "fsync", 81, 0u, new Action<RubyIO>(RubyIOOps.Flush));
			LibraryInitializer.DefineLibraryMethod(module, "getc", 81, 0u, new Func<RubyIO, object>(RubyIOOps.Getc));
			LibraryInitializer.DefineLibraryMethod(module, "gets", 81, 0u, 0u, 131076u, 393216u, new Func<RubyScope, RubyIO, MutableString>(RubyIOOps.Gets), new Func<RubyScope, RubyIO, DynamicNull, MutableString>(RubyIOOps.Gets), new Func<RubyScope, RubyIO, Union<MutableString, int>, MutableString>(RubyIOOps.Gets), new Func<RubyScope, RubyIO, MutableString, int, MutableString>(RubyIOOps.Gets));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 82, 2097152u, new Func<ConversionStorage<int?>, ConversionStorage<IDictionary<object, object>>, ConversionStorage<MutableString>, RubyIO, object, object, IDictionary<object, object>, RubyIO>(RubyIOOps.Reinitialize));
			LibraryInitializer.DefineLibraryMethod(module, "initialize_copy", 82, 2u, new Func<RubyIO, RubyIO, RubyIO>(RubyIOOps.InitializeCopy));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<RubyIO, MutableString>(RubyIOOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "internal_encoding", 81, 0u, new Func<RubyIO, RubyEncoding>(RubyIOOps.GetInternalEncoding));
			LibraryInitializer.DefineLibraryMethod(module, "ioctl", 81, 65536u, 65536u, new Func<RubyIO, int, MutableString, int>(RubyIOOps.FileControl), new Func<RubyIO, int, int, int>(RubyIOOps.FileControl));
			LibraryInitializer.DefineLibraryMethod(module, "isatty", 81, 0u, new Func<RubyIO, bool>(RubyIOOps.IsAtty));
			LibraryInitializer.DefineLibraryMethod(module, "lineno", 81, 0u, new Func<RubyIO, int>(RubyIOOps.GetLineNumber));
			LibraryInitializer.DefineLibraryMethod(module, "lineno=", 81, 131072u, new Action<RubyContext, RubyIO, int>(RubyIOOps.SetLineNumber));
			LibraryInitializer.DefineLibraryMethod(module, "pid", 81, 0u, new Func<RubyIO, object>(RubyIOOps.Pid));
			LibraryInitializer.DefineLibraryMethod(module, "pos", 81, 0u, new Func<RubyIO, object>(RubyIOOps.Pos));
			LibraryInitializer.DefineLibraryMethod(module, "pos=", 81, 65536u, new Action<RubyIO, IntegerValue>(RubyIOOps.Pos));
			LibraryInitializer.DefineLibraryMethod(module, "read", 81, 0u, 131072u, 196608u, new Func<RubyIO, MutableString>(RubyIOOps.Read), new Func<RubyIO, DynamicNull, MutableString, MutableString>(RubyIOOps.Read), new Func<RubyIO, int, MutableString, MutableString>(RubyIOOps.Read));
			LibraryInitializer.DefineLibraryMethod(module, "read_nonblock", 81, 196608u, new Func<RubyIO, int, MutableString, MutableString>(RubyIOOps.ReadNoBlock));
			LibraryInitializer.DefineLibraryMethod(module, "readchar", 81, 0u, new Func<RubyIO, int>(RubyIOOps.ReadChar));
			LibraryInitializer.DefineLibraryMethod(module, "readline", 81, 0u, 0u, 131076u, 393216u, new Func<RubyScope, RubyIO, MutableString>(RubyIOOps.ReadLine), new Func<RubyScope, RubyIO, DynamicNull, MutableString>(RubyIOOps.ReadLine), new Func<RubyScope, RubyIO, Union<MutableString, int>, MutableString>(RubyIOOps.ReadLine), new Func<RubyScope, RubyIO, MutableString, int, MutableString>(RubyIOOps.ReadLine));
			LibraryInitializer.DefineLibraryMethod(module, "readlines", 81, 393216u, 0u, 0u, 131076u, new Func<RubyContext, RubyIO, MutableString, int, RubyArray>(RubyIOOps.ReadLines), new Func<RubyContext, RubyIO, RubyArray>(RubyIOOps.ReadLines), new Func<RubyContext, RubyIO, DynamicNull, RubyArray>(RubyIOOps.ReadLines), new Func<RubyContext, RubyIO, Union<MutableString, int>, RubyArray>(RubyIOOps.ReadLines));
			LibraryInitializer.DefineLibraryMethod(module, "reopen", 81, 2u, 262152u, 0u, new Func<RubyIO, RubyIO, RubyIO>(RubyIOOps.Reopen), new Func<ConversionStorage<MutableString>, RubyIO, object, MutableString, RubyIO>(RubyIOOps.Reopen), new Func<ConversionStorage<MutableString>, RubyIO, object, int, RubyIO>(RubyIOOps.Reopen));
			LibraryInitializer.DefineLibraryMethod(module, "rewind", 81, 0u, new Action<RubyContext, RubyIO>(RubyIOOps.Rewind));
			LibraryInitializer.DefineLibraryMethod(module, "seek", 81, 196608u, new Func<RubyIO, IntegerValue, int, int>(RubyIOOps.Seek));
			LibraryInitializer.DefineLibraryMethod(module, "set_encoding", 81, 0u, 0u, new Func<ConversionStorage<IDictionary<object, object>>, ConversionStorage<MutableString>, RubyIO, object, object, IDictionary<object, object>, RubyIO>(RubyIOOps.SetEncodings), new Func<RubyIO, RubyEncoding, RubyEncoding, RubyIO>(RubyIOOps.SetEncodings));
			LibraryInitializer.DefineLibraryMethod(module, "sync", 81, 0u, new Func<RubyIO, bool>(RubyIOOps.Sync));
			LibraryInitializer.DefineLibraryMethod(module, "sync=", 81, 0u, new Func<RubyIO, bool, bool>(RubyIOOps.Sync));
			LibraryInitializer.DefineLibraryMethod(module, "sysread", 81, 196608u, new Func<RubyIO, int, MutableString, MutableString>(RubyIOOps.SystemRead));
			LibraryInitializer.DefineLibraryMethod(module, "sysseek", 81, 196608u, new Func<RubyIO, IntegerValue, int, object>(RubyIOOps.SysSeek));
			LibraryInitializer.DefineLibraryMethod(module, "syswrite", 81, 16u, 0u, new Func<BinaryOpStorage, ConversionStorage<MutableString>, RubyContext, RubyIO, MutableString, int>(RubyIOOps.SysWrite), new Func<BinaryOpStorage, ConversionStorage<MutableString>, RubyContext, RubyIO, object, int>(RubyIOOps.SysWrite));
			LibraryInitializer.DefineLibraryMethod(module, "tell", 81, 0u, new Func<RubyIO, object>(RubyIOOps.Pos));
			LibraryInitializer.DefineLibraryMethod(module, "to_i", 81, 0u, new Func<RubyIO, int>(RubyIOOps.FileNo));
			LibraryInitializer.DefineLibraryMethod(module, "to_io", 81, 0u, new Func<RubyIO, RubyIO>(RubyIOOps.ToIO));
			LibraryInitializer.DefineLibraryMethod(module, "tty?", 81, 0u, new Func<RubyIO, bool>(RubyIOOps.IsAtty));
			LibraryInitializer.DefineLibraryMethod(module, "ungetc", 81, 65536u, new Action<RubyIO, int>(RubyIOOps.SetPreviousByte));
			LibraryInitializer.DefineLibraryMethod(module, "write", 81, 2u, 0u, new Func<RubyIO, MutableString, int>(RubyIOOps.Write), new Func<ConversionStorage<MutableString>, RubyIO, object, int>(RubyIOOps.Write));
			LibraryInitializer.DefineLibraryMethod(module, "write_nonblock", 81, 2u, 0u, new Func<RubyIO, MutableString, int>(RubyIOOps.WriteNoBlock), new Func<ConversionStorage<MutableString>, RubyIO, object, int>(RubyIOOps.WriteNoBlock));
		}

		private static void LoadIO_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "copy_stream", 97, 0u, new Func<ConversionStorage<MutableString>, ConversionStorage<int>, RespondToStorage, BinaryOpStorage, CallSiteStorage<Func<CallSite, object, object, object, object>>, RubyClass, object, object, int, int, object>(RubyIOOps.CopyStream));
			LibraryInitializer.DefineRuleGenerator(module, "for_fd", 97, RubyIOOps.ForFileDescriptor());
			LibraryInitializer.DefineLibraryMethod(module, "foreach", 97, 393220u, 917508u, new Action<BlockParam, RubyClass, MutableString, int>(RubyIOOps.ForEach), new Action<BlockParam, RubyClass, MutableString, MutableString, int>(RubyIOOps.ForEach));
			LibraryInitializer.DefineRuleGenerator(module, "open", 97, RubyIOOps.Open());
			LibraryInitializer.DefineLibraryMethod(module, "pipe", 97, 0u, new Func<RubyClass, RubyArray>(RubyIOOps.OpenPipe));
			LibraryInitializer.DefineLibraryMethod(module, "popen", 97, 786456u, 393228u, new Func<RubyContext, BlockParam, RubyClass, MutableString, MutableString, object>(RubyIOOps.OpenPipe), new Func<RubyContext, RubyClass, MutableString, MutableString, RubyIO>(RubyIOOps.OpenPipe));
			LibraryInitializer.DefineLibraryMethod(module, "read", 97, 4194304u, new Func<ConversionStorage<IDictionary<object, object>>, ConversionStorage<int>, ConversionStorage<MutableString>, RubyClass, object, object, object, IDictionary<object, object>, MutableString>(RubyIOOps.Read));
			LibraryInitializer.DefineLibraryMethod(module, "readlines", 97, 196610u, 458754u, new Func<RubyClass, MutableString, int, RubyArray>(RubyIOOps.ReadLines), new Func<RubyClass, MutableString, MutableString, int, RubyArray>(RubyIOOps.ReadLines));
			LibraryInitializer.DefineLibraryMethod(module, "select", 97, 0u, 0u, 0u, new Func<RubyContext, object, RubyArray, RubyArray, RubyArray, RubyArray>(RubyIOOps.Select), new Func<RubyContext, object, RubyArray, RubyArray, RubyArray, int, RubyArray>(RubyIOOps.Select), new Func<RubyContext, object, RubyArray, RubyArray, RubyArray, double, RubyArray>(RubyIOOps.Select));
			LibraryInitializer.DefineLibraryMethod(module, "sysopen", 97, 2u, new Func<RubyClass, MutableString, MutableString, int, int>(RubyIOOps.SysOpen));
		}

		private static void LoadIronRuby_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "configuration", 97, 0u, new Func<RubyContext, RubyModule, DlrConfiguration>(IronRubyOps.GetConfiguration));
			LibraryInitializer.DefineLibraryMethod(module, "globals", 97, 0u, new Func<RubyContext, RubyModule, Scope>(IronRubyOps.GetGlobalScope));
			LibraryInitializer.DefineLibraryMethod(module, "load", 97, 0u, new Func<RubyScope, RubyModule, MutableString, object>(IronRubyOps.Load));
			LibraryInitializer.DefineLibraryMethod(module, "loaded_assemblies", 97, 0u, new Func<RubyContext, RubyModule, RubyArray>(IronRubyOps.GetLoadedAssemblies));
			LibraryInitializer.DefineLibraryMethod(module, "loaded_scripts", 97, 0u, new Func<RubyContext, RubyModule, IDictionary<string, Scope>>(IronRubyOps.GetLoadedScripts));
			LibraryInitializer.DefineLibraryMethod(module, "require", 97, 0u, new Func<RubyScope, RubyModule, MutableString, object>(IronRubyOps.Require));
		}

		private static void LoadIronRuby__Clr_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "profile", 97, 0u, 0u, new Func<RubyContext, object, Hash>(IronRubyOps.Clr.GetProfile), new Func<RubyContext, BlockParam, object, object>(IronRubyOps.Clr.GetProfile));
		}

		private static void LoadIronRuby__Clr__BigInteger_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "-", 81, 2u, 0u, 0u, 0u, new Func<BigInteger, BigInteger, object>(ClrBigInteger.Subtract), new Func<BigInteger, int, object>(ClrBigInteger.Subtract), new Func<BigInteger, double, object>(ClrBigInteger.Subtract), new Func<BinaryOpStorage, BinaryOpStorage, object, object, object>(ClrBigInteger.Subtract));
			LibraryInitializer.DefineLibraryMethod(module, "%", 81, 2u, 0u, 0u, 0u, new Func<BigInteger, BigInteger, object>(ClrBigInteger.Modulo), new Func<BigInteger, int, object>(ClrBigInteger.Modulo), new Func<BigInteger, double, object>(ClrBigInteger.Modulo), new Func<BinaryOpStorage, BinaryOpStorage, object, object, object>(ClrBigInteger.ModuloOp));
			LibraryInitializer.DefineLibraryMethod(module, "&", 81, 0u, 2u, 131072u, new Func<BigInteger, int, object>(ClrBigInteger.And), new Func<BigInteger, BigInteger, object>(ClrBigInteger.And), new Func<RubyContext, BigInteger, IntegerValue, object>(ClrBigInteger.And));
			LibraryInitializer.DefineLibraryMethod(module, "*", 81, 2u, 0u, 0u, 0u, new Func<BigInteger, BigInteger, object>(ClrBigInteger.Multiply), new Func<BigInteger, int, object>(ClrBigInteger.Multiply), new Func<BigInteger, double, object>(ClrBigInteger.Multiply), new Func<BinaryOpStorage, BinaryOpStorage, object, object, object>(ClrBigInteger.Multiply));
			LibraryInitializer.DefineLibraryMethod(module, "**", 81, 4u, 0u, 0u, 0u, new Func<RubyContext, BigInteger, BigInteger, object>(ClrBigInteger.Power), new Func<BigInteger, int, object>(ClrBigInteger.Power), new Func<BigInteger, double, object>(ClrBigInteger.Power), new Func<BinaryOpStorage, BinaryOpStorage, object, object, object>(ClrBigInteger.Power));
			LibraryInitializer.DefineLibraryMethod(module, "/", 81, 2u, 0u, 0u, 0u, new Func<BigInteger, BigInteger, object>(ClrBigInteger.Divide), new Func<BigInteger, int, object>(ClrBigInteger.Divide), new Func<BigInteger, double, object>(ClrBigInteger.DivideOp), new Func<BinaryOpStorage, BinaryOpStorage, object, object, object>(ClrBigInteger.Divide));
			LibraryInitializer.DefineLibraryMethod(module, "-@", 81, 0u, new Func<BigInteger, object>(ClrBigInteger.Negate));
			LibraryInitializer.DefineLibraryMethod(module, "[]", 81, 65536u, 2u, new Func<BigInteger, int, int>(ClrBigInteger.Bit), new Func<BigInteger, BigInteger, int>(ClrBigInteger.Bit));
			LibraryInitializer.DefineLibraryMethod(module, "^", 81, 0u, 2u, 131072u, new Func<BigInteger, int, object>(ClrBigInteger.Xor), new Func<BigInteger, BigInteger, object>(ClrBigInteger.Xor), new Func<RubyContext, BigInteger, IntegerValue, object>(ClrBigInteger.Xor));
			LibraryInitializer.DefineLibraryMethod(module, "|", 81, 0u, 2u, 131072u, new Func<BigInteger, int, object>(ClrBigInteger.BitwiseOr), new Func<BigInteger, BigInteger, object>(ClrBigInteger.BitwiseOr), new Func<RubyContext, BigInteger, IntegerValue, object>(ClrBigInteger.BitwiseOr));
			LibraryInitializer.DefineLibraryMethod(module, "~", 81, 0u, new Func<BigInteger, object>(ClrBigInteger.Invert));
			LibraryInitializer.DefineLibraryMethod(module, "+", 81, 2u, 0u, 0u, 0u, new Func<BigInteger, BigInteger, object>(ClrBigInteger.Add), new Func<BigInteger, int, object>(ClrBigInteger.Add), new Func<BigInteger, double, object>(ClrBigInteger.Add), new Func<BinaryOpStorage, BinaryOpStorage, object, object, object>(ClrBigInteger.Add));
			LibraryInitializer.DefineLibraryMethod(module, "<<", 81, 0u, 2u, 131072u, new Func<BigInteger, int, object>(ClrBigInteger.LeftShift), new Func<BigInteger, BigInteger, object>(ClrBigInteger.LeftShift), new Func<RubyContext, BigInteger, IntegerValue, object>(ClrBigInteger.LeftShift));
			LibraryInitializer.DefineLibraryMethod(module, "<=>", 81, 2u, 0u, 0u, 0u, new Func<BigInteger, BigInteger, int>(ClrBigInteger.Compare), new Func<BigInteger, int, int>(ClrBigInteger.Compare), new Func<RubyContext, BigInteger, double, object>(ClrBigInteger.Compare), new Func<BinaryOpStorage, BinaryOpStorage, BigInteger, object, object>(ClrBigInteger.Compare));
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 2u, 0u, 0u, 0u, new Func<BigInteger, BigInteger, bool>(ClrBigInteger.Equal), new Func<BigInteger, int, bool>(ClrBigInteger.Equal), new Func<RubyContext, BigInteger, double, bool>(ClrBigInteger.Equal), new Func<BinaryOpStorage, BigInteger, object, bool>(ClrBigInteger.Equal));
			LibraryInitializer.DefineLibraryMethod(module, ">>", 81, 0u, 2u, 131072u, new Func<BigInteger, int, object>(ClrBigInteger.RightShift), new Func<BigInteger, BigInteger, object>(ClrBigInteger.RightShift), new Func<RubyContext, BigInteger, IntegerValue, object>(ClrBigInteger.RightShift));
			LibraryInitializer.DefineLibraryMethod(module, "abs", 81, 0u, new Func<BigInteger, object>(ClrBigInteger.Abs));
			LibraryInitializer.DefineLibraryMethod(module, "coerce", 81, 2u, 0u, new Func<BigInteger, BigInteger, RubyArray>(ClrBigInteger.Coerce), new Func<RubyContext, BigInteger, object, RubyArray>(ClrBigInteger.Coerce));
			LibraryInitializer.DefineLibraryMethod(module, "div", 81, 2u, 0u, 0u, 0u, new Func<BigInteger, BigInteger, object>(ClrBigInteger.Divide), new Func<BigInteger, int, object>(ClrBigInteger.Divide), new Func<BigInteger, double, object>(ClrBigInteger.Divide), new Func<BinaryOpStorage, BinaryOpStorage, object, object, object>(ClrBigInteger.Div));
			LibraryInitializer.DefineLibraryMethod(module, "divmod", 81, 2u, 0u, 0u, 0u, new Func<BigInteger, BigInteger, RubyArray>(ClrBigInteger.DivMod), new Func<BigInteger, int, RubyArray>(ClrBigInteger.DivMod), new Func<BigInteger, double, RubyArray>(ClrBigInteger.DivMod), new Func<BinaryOpStorage, BinaryOpStorage, object, object, object>(ClrBigInteger.DivMod));
			LibraryInitializer.DefineLibraryMethod(module, "eql?", 81, 2u, 0u, 0u, new Func<BigInteger, BigInteger, bool>(ClrBigInteger.Eql), new Func<BigInteger, int, bool>(ClrBigInteger.Eql), new Func<BigInteger, object, bool>(ClrBigInteger.Eql));
			LibraryInitializer.DefineLibraryMethod(module, "fdiv", 81, 2u, new Func<BigInteger, BigInteger, double>(ClrBigInteger.FDiv));
			LibraryInitializer.DefineLibraryMethod(module, "hash", 81, 0u, new Func<BigInteger, int>(ClrBigInteger.Hash));
			LibraryInitializer.DefineLibraryMethod(module, "modulo", 81, 2u, 0u, 0u, 0u, new Func<BigInteger, BigInteger, object>(ClrBigInteger.Modulo), new Func<BigInteger, int, object>(ClrBigInteger.Modulo), new Func<BigInteger, double, object>(ClrBigInteger.Modulo), new Func<BinaryOpStorage, BinaryOpStorage, object, object, object>(ClrBigInteger.Modulo));
			LibraryInitializer.DefineLibraryMethod(module, "quo", 81, 2u, 0u, 0u, 0u, new Func<BigInteger, BigInteger, object>(ClrBigInteger.Quotient), new Func<BigInteger, int, object>(ClrBigInteger.Quotient), new Func<BigInteger, double, object>(ClrBigInteger.Quotient), new Func<BinaryOpStorage, BinaryOpStorage, object, object, object>(ClrBigInteger.Quotient));
			LibraryInitializer.DefineLibraryMethod(module, "remainder", 81, 2u, 0u, 0u, 0u, new Func<BigInteger, BigInteger, object>(ClrBigInteger.Remainder), new Func<BigInteger, int, object>(ClrBigInteger.Remainder), new Func<BigInteger, double, double>(ClrBigInteger.Remainder), new Func<BinaryOpStorage, BinaryOpStorage, object, object, object>(ClrBigInteger.Remainder));
			LibraryInitializer.DefineLibraryMethod(module, "to_f", 81, 0u, new Func<RubyContext, BigInteger, double>(ClrBigInteger.ToFloat));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, 0u, new Func<BigInteger, MutableString>(ClrBigInteger.ToString), new Func<BigInteger, int, MutableString>(ClrBigInteger.ToString));
		}

		private static void LoadIronRuby__Clr__FlagEnumeration_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "&", 81, 4u, new Func<RubyContext, object, object, object>(FlagEnumerationOps.BitwiseAnd));
			LibraryInitializer.DefineLibraryMethod(module, "^", 81, 4u, new Func<RubyContext, object, object, object>(FlagEnumerationOps.Xor));
			LibraryInitializer.DefineLibraryMethod(module, "|", 81, 4u, new Func<RubyContext, object, object, object>(FlagEnumerationOps.BitwiseOr));
			LibraryInitializer.DefineLibraryMethod(module, "~", 81, 0u, new Func<RubyContext, object, object>(FlagEnumerationOps.OnesComplement));
		}

		private static void LoadIronRuby__Clr__Float_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "-", 81, 0u, 4u, 0u, 0u, new Func<double, int, double>(ClrFloat.Subtract), new Func<RubyContext, double, BigInteger, double>(ClrFloat.Subtract), new Func<double, double, double>(ClrFloat.Subtract), new Func<BinaryOpStorage, BinaryOpStorage, double, object, object>(ClrFloat.Subtract));
			LibraryInitializer.DefineLibraryMethod(module, "%", 81, 0u, 4u, 0u, 0u, new Func<double, int, double>(ClrFloat.Modulo), new Func<RubyContext, double, BigInteger, double>(ClrFloat.Modulo), new Func<double, double, double>(ClrFloat.Modulo), new Func<BinaryOpStorage, BinaryOpStorage, double, object, object>(ClrFloat.ModuloOp));
			LibraryInitializer.DefineLibraryMethod(module, "*", 81, 0u, 4u, 0u, 0u, new Func<double, int, double>(ClrFloat.Multiply), new Func<RubyContext, double, BigInteger, double>(ClrFloat.Multiply), new Func<double, double, double>(ClrFloat.Multiply), new Func<BinaryOpStorage, BinaryOpStorage, double, object, object>(ClrFloat.Multiply));
			LibraryInitializer.DefineLibraryMethod(module, "**", 81, 0u, 4u, 0u, 0u, new Func<double, int, double>(ClrFloat.Power), new Func<RubyContext, double, BigInteger, double>(ClrFloat.Power), new Func<double, double, double>(ClrFloat.Power), new Func<BinaryOpStorage, BinaryOpStorage, double, object, object>(ClrFloat.Power));
			LibraryInitializer.DefineLibraryMethod(module, "/", 81, 0u, 4u, 0u, 0u, new Func<double, int, double>(ClrFloat.Divide), new Func<RubyContext, double, BigInteger, double>(ClrFloat.Divide), new Func<double, double, double>(ClrFloat.Divide), new Func<BinaryOpStorage, BinaryOpStorage, double, object, object>(ClrFloat.Divide));
			LibraryInitializer.DefineLibraryMethod(module, "+", 81, 0u, 4u, 0u, 0u, new Func<double, int, double>(ClrFloat.Add), new Func<RubyContext, double, BigInteger, double>(ClrFloat.Add), new Func<double, double, double>(ClrFloat.Add), new Func<BinaryOpStorage, BinaryOpStorage, double, object, object>(ClrFloat.Add));
			LibraryInitializer.DefineLibraryMethod(module, "<", 81, 0u, 0u, 4u, 0u, new Func<double, double, bool>(ClrFloat.LessThan), new Func<double, int, bool>(ClrFloat.LessThan), new Func<RubyContext, double, BigInteger, bool>(ClrFloat.LessThan), new Func<BinaryOpStorage, BinaryOpStorage, double, object, bool>(ClrFloat.LessThan));
			LibraryInitializer.DefineLibraryMethod(module, "<=", 81, 0u, 0u, 4u, 0u, new Func<double, double, bool>(ClrFloat.LessThanOrEqual), new Func<double, int, bool>(ClrFloat.LessThanOrEqual), new Func<RubyContext, double, BigInteger, bool>(ClrFloat.LessThanOrEqual), new Func<BinaryOpStorage, BinaryOpStorage, double, object, bool>(ClrFloat.LessThanOrEqual));
			LibraryInitializer.DefineLibraryMethod(module, "<=>", 81, 0u, 0u, 4u, 0u, new Func<double, double, object>(ClrFloat.Compare), new Func<double, int, object>(ClrFloat.Compare), new Func<RubyContext, double, BigInteger, object>(ClrFloat.Compare), new Func<BinaryOpStorage, BinaryOpStorage, double, object, object>(ClrFloat.Compare));
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 0u, 0u, new Func<double, double, bool>(ClrFloat.Equal), new Func<BinaryOpStorage, double, object, bool>(ClrFloat.Equal));
			LibraryInitializer.DefineLibraryMethod(module, ">", 81, 0u, 0u, 4u, 0u, new Func<double, double, bool>(ClrFloat.GreaterThan), new Func<double, int, bool>(ClrFloat.GreaterThan), new Func<RubyContext, double, BigInteger, bool>(ClrFloat.GreaterThan), new Func<BinaryOpStorage, BinaryOpStorage, double, object, bool>(ClrFloat.GreaterThan));
			LibraryInitializer.DefineLibraryMethod(module, ">=", 81, 0u, 0u, 4u, 0u, new Func<double, double, bool>(ClrFloat.GreaterThanOrEqual), new Func<double, int, bool>(ClrFloat.GreaterThanOrEqual), new Func<RubyContext, double, BigInteger, bool>(ClrFloat.GreaterThanOrEqual), new Func<BinaryOpStorage, BinaryOpStorage, double, object, bool>(ClrFloat.GreaterThanOrEqual));
			LibraryInitializer.DefineLibraryMethod(module, "abs", 81, 0u, new Func<double, double>(ClrFloat.Abs));
			LibraryInitializer.DefineLibraryMethod(module, "ceil", 81, 0u, new Func<double, object>(ClrFloat.Ceil));
			LibraryInitializer.DefineLibraryMethod(module, "coerce", 81, 65536u, new Func<double, double, RubyArray>(ClrFloat.Coerce));
			LibraryInitializer.DefineLibraryMethod(module, "divmod", 81, 0u, 4u, 0u, 0u, new Func<double, int, RubyArray>(ClrFloat.DivMod), new Func<RubyContext, double, BigInteger, RubyArray>(ClrFloat.DivMod), new Func<double, double, RubyArray>(ClrFloat.DivMod), new Func<BinaryOpStorage, BinaryOpStorage, double, object, object>(ClrFloat.DivMod));
			LibraryInitializer.DefineLibraryMethod(module, "finite?", 81, 0u, new Func<double, bool>(ClrFloat.IsFinite));
			LibraryInitializer.DefineLibraryMethod(module, "floor", 81, 0u, new Func<double, object>(ClrFloat.Floor));
			LibraryInitializer.DefineLibraryMethod(module, "hash", 81, 0u, new Func<double, int>(ClrFloat.Hash));
			LibraryInitializer.DefineLibraryMethod(module, "infinite?", 81, 0u, new Func<double, object>(ClrFloat.IsInfinite));
			LibraryInitializer.DefineLibraryMethod(module, "modulo", 81, 0u, 4u, 0u, 0u, new Func<double, int, double>(ClrFloat.Modulo), new Func<RubyContext, double, BigInteger, double>(ClrFloat.Modulo), new Func<double, double, double>(ClrFloat.Modulo), new Func<BinaryOpStorage, BinaryOpStorage, double, object, object>(ClrFloat.Modulo));
			LibraryInitializer.DefineLibraryMethod(module, "nan?", 81, 0u, new Func<double, bool>(ClrFloat.IsNan));
			LibraryInitializer.DefineLibraryMethod(module, "round", 81, 0u, new Func<double, object>(ClrFloat.Round));
			LibraryInitializer.DefineLibraryMethod(module, "to_f", 81, 0u, new Func<double, double>(ClrFloat.ToFloat));
			LibraryInitializer.DefineLibraryMethod(module, "to_i", 81, 0u, new Func<double, object>(ClrFloat.ToInt));
			LibraryInitializer.DefineLibraryMethod(module, "to_int", 81, 0u, new Func<double, object>(ClrFloat.ToInt));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<RubyContext, double, MutableString>(ClrFloat.ToS));
			LibraryInitializer.DefineLibraryMethod(module, "truncate", 81, 0u, new Func<double, object>(ClrFloat.ToInt));
			LibraryInitializer.DefineLibraryMethod(module, "zero?", 81, 0u, new Func<double, bool>(ClrFloat.IsZero));
		}

		private static void LoadIronRuby__Clr__Float_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "induced_from", 97, 0u, 0u, 4u, 0u, new Func<RubyModule, double, double>(ClrFloat.InducedFrom), new Func<UnaryOpStorage, RubyModule, int, object>(ClrFloat.InducedFrom), new Func<UnaryOpStorage, RubyModule, BigInteger, object>(ClrFloat.InducedFrom), new Func<RubyModule, object, double>(ClrFloat.InducedFrom));
		}

		private static void LoadIronRuby__Clr__Integer_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "-", 81, 0u, 0u, 0u, 0u, new Func<int, int, object>(ClrInteger.Subtract), new Func<int, BigInteger, object>(ClrInteger.Subtract), new Func<int, double, double>(ClrInteger.Subtract), new Func<BinaryOpStorage, BinaryOpStorage, RubyContext, object, object, object>(ClrInteger.Subtract));
			LibraryInitializer.DefineLibraryMethod(module, "%", 81, 0u, 0u, new Func<int, int, int>(ClrInteger.Modulo), new Func<BinaryOpStorage, BinaryOpStorage, object, object, object>(ClrInteger.ModuloOp));
			LibraryInitializer.DefineLibraryMethod(module, "&", 81, 0u, 2u, 131072u, new Func<int, int, int>(ClrInteger.BitwiseAnd), new Func<int, BigInteger, object>(ClrInteger.BitwiseAnd), new Func<RubyContext, int, IntegerValue, object>(ClrInteger.BitwiseAnd));
			LibraryInitializer.DefineLibraryMethod(module, "*", 81, 0u, 2u, 0u, 0u, new Func<int, int, object>(ClrInteger.Multiply), new Func<int, BigInteger, BigInteger>(ClrInteger.Multiply), new Func<int, double, double>(ClrInteger.Multiply), new Func<BinaryOpStorage, BinaryOpStorage, object, object, object>(ClrInteger.Multiply));
			LibraryInitializer.DefineLibraryMethod(module, "**", 81, 0u, 0u, 0u, new Func<int, int, object>(ClrInteger.Power), new Func<int, double, double>(ClrInteger.Power), new Func<BinaryOpStorage, BinaryOpStorage, RubyContext, int, object, object>(ClrInteger.Power));
			LibraryInitializer.DefineLibraryMethod(module, "/", 81, 0u, 0u, new Func<int, int, object>(ClrInteger.Divide), new Func<BinaryOpStorage, BinaryOpStorage, object, object, object>(ClrInteger.DivideOp));
			LibraryInitializer.DefineLibraryMethod(module, "-@", 81, 0u, new Func<int, object>(ClrInteger.Minus));
			LibraryInitializer.DefineLibraryMethod(module, "[]", 81, 65536u, 2u, new Func<int, int, int>(ClrInteger.Bit), new Func<int, BigInteger, int>(ClrInteger.Bit));
			LibraryInitializer.DefineLibraryMethod(module, "^", 81, 0u, 2u, 131072u, new Func<int, int, object>(ClrInteger.BitwiseXor), new Func<int, BigInteger, object>(ClrInteger.BitwiseXor), new Func<RubyContext, int, IntegerValue, object>(ClrInteger.BitwiseXor));
			LibraryInitializer.DefineLibraryMethod(module, "|", 81, 0u, 2u, 131072u, new Func<int, int, int>(ClrInteger.BitwiseOr), new Func<int, BigInteger, object>(ClrInteger.BitwiseOr), new Func<RubyContext, int, IntegerValue, object>(ClrInteger.BitwiseOr));
			LibraryInitializer.DefineLibraryMethod(module, "~", 81, 0u, new Func<int, int>(ClrInteger.OnesComplement));
			LibraryInitializer.DefineLibraryMethod(module, "+", 81, 0u, 2u, 0u, 0u, new Func<int, int, object>(ClrInteger.Add), new Func<int, BigInteger, object>(ClrInteger.Add), new Func<int, double, double>(ClrInteger.Add), new Func<BinaryOpStorage, BinaryOpStorage, object, object, object>(ClrInteger.Add));
			LibraryInitializer.DefineLibraryMethod(module, "<", 81, 0u, 0u, new Func<int, int, bool>(ClrInteger.LessThan), new Func<BinaryOpStorage, BinaryOpStorage, object, object, bool>(ClrInteger.LessThan));
			LibraryInitializer.DefineLibraryMethod(module, "<<", 81, 0u, 131072u, new Func<int, int, object>(ClrInteger.LeftShift), new Func<RubyContext, int, IntegerValue, object>(ClrInteger.LeftShift));
			LibraryInitializer.DefineLibraryMethod(module, "<=", 81, 0u, 0u, new Func<int, int, bool>(ClrInteger.LessThanOrEqual), new Func<BinaryOpStorage, BinaryOpStorage, object, object, bool>(ClrInteger.LessThanOrEqual));
			LibraryInitializer.DefineLibraryMethod(module, "<=>", 81, 0u, 0u, new Func<int, int, int>(ClrInteger.Compare), new Func<BinaryOpStorage, BinaryOpStorage, object, object, object>(ClrInteger.Compare));
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 0u, 0u, new Func<int, int, bool>(ClrInteger.Equal), new Func<BinaryOpStorage, int, object, bool>(ClrInteger.Equal));
			LibraryInitializer.DefineLibraryMethod(module, ">", 81, 0u, 0u, new Func<int, int, bool>(ClrInteger.GreaterThan), new Func<BinaryOpStorage, BinaryOpStorage, object, object, bool>(ClrInteger.GreaterThan));
			LibraryInitializer.DefineLibraryMethod(module, ">=", 81, 0u, 0u, new Func<int, int, bool>(ClrInteger.GreaterThanOrEqual), new Func<BinaryOpStorage, BinaryOpStorage, object, object, bool>(ClrInteger.GreaterThanOrEqual));
			LibraryInitializer.DefineLibraryMethod(module, ">>", 81, 0u, 131072u, new Func<int, int, object>(ClrInteger.RightShift), new Func<RubyContext, int, IntegerValue, object>(ClrInteger.RightShift));
			LibraryInitializer.DefineLibraryMethod(module, "abs", 81, 0u, new Func<int, object>(ClrInteger.Abs));
			LibraryInitializer.DefineLibraryMethod(module, "div", 81, 0u, 0u, new Func<int, int, object>(ClrInteger.Divide), new Func<BinaryOpStorage, BinaryOpStorage, object, object, object>(ClrInteger.Divide));
			LibraryInitializer.DefineLibraryMethod(module, "divmod", 81, 0u, 0u, new Func<int, int, RubyArray>(ClrInteger.DivMod), new Func<BinaryOpStorage, BinaryOpStorage, int, object, object>(ClrInteger.DivMod));
			LibraryInitializer.DefineLibraryMethod(module, "fdiv", 81, 65536u, new Func<int, int, double>(ClrInteger.FDiv));
			LibraryInitializer.DefineLibraryMethod(module, "modulo", 81, 0u, 0u, new Func<int, int, int>(ClrInteger.Modulo), new Func<BinaryOpStorage, BinaryOpStorage, object, object, object>(ClrInteger.Modulo));
			LibraryInitializer.DefineLibraryMethod(module, "quo", 81, 0u, 0u, new Func<int, int, double>(ClrInteger.Quotient), new Func<BinaryOpStorage, BinaryOpStorage, int, object, object>(ClrInteger.Quotient));
			LibraryInitializer.DefineLibraryMethod(module, "to_f", 81, 0u, new Func<int, double>(ClrInteger.ToFloat));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, 1u, new Func<object, object>(ClrInteger.ToString), new Func<BigInteger, int, object>(ClrInteger.ToString));
			LibraryInitializer.DefineLibraryMethod(module, "zero?", 81, 0u, new Func<int, bool>(ClrInteger.IsZero));
		}

		private static void LoadIronRuby__Clr__Name_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "<=>", 81, new uint[5] { 65538u, 2u, 4u, 4u, 0u }, new Func<ClrName, string, int>(ClrNameOps.Compare), new Func<ClrName, ClrName, int>(ClrNameOps.Compare), new Func<RubyContext, ClrName, MutableString, int>(ClrNameOps.Compare), new Func<RubyContext, ClrName, RubySymbol, int>(ClrNameOps.Compare), new Func<BinaryOpStorage, RespondToStorage, ClrName, object, object>(ClrNameOps.Compare));
			LibraryInitializer.DefineLibraryMethod(module, "=~", 81, 4u, 2u, 0u, new Func<RubyScope, ClrName, RubyRegex, object>(ClrNameOps.Match), new Func<ClrName, ClrName, object>(ClrNameOps.Match), new Func<CallSiteStorage<Func<CallSite, RubyScope, object, object, object>>, RubyScope, ClrName, object, object>(ClrNameOps.Match));
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 65538u, 2u, 4u, 2u, new Func<ClrName, string, bool>(ClrNameOps.IsEqual), new Func<ClrName, MutableString, bool>(ClrNameOps.IsEqual), new Func<RubyContext, ClrName, RubySymbol, bool>(ClrNameOps.IsEqual), new Func<ClrName, ClrName, bool>(ClrNameOps.IsEqual));
			LibraryInitializer.DefineLibraryMethod(module, "clr_name", 81, 0u, new Func<RubyContext, ClrName, MutableString>(ClrNameOps.GetClrName));
			LibraryInitializer.DefineLibraryMethod(module, "dump", 81, 0u, new Func<ClrName, MutableString>(ClrNameOps.Dump));
			LibraryInitializer.DefineLibraryMethod(module, "empty?", 81, 0u, new Func<ClrName, bool>(ClrNameOps.IsEmpty));
			LibraryInitializer.DefineLibraryMethod(module, "encoding", 81, 0u, new Func<ClrName, RubyEncoding>(ClrNameOps.GetEncoding));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<ClrName, MutableString>(ClrNameOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "intern", 81, 0u, new Func<RubyContext, ClrName, RubySymbol>(ClrNameOps.ToSymbol));
			LibraryInitializer.DefineLibraryMethod(module, "length", 81, 0u, new Func<ClrName, int>(ClrNameOps.GetLength));
			LibraryInitializer.DefineLibraryMethod(module, "match", 81, 8u, 262152u, new Func<CallSiteStorage<Func<CallSite, RubyScope, object, object, object>>, RubyScope, ClrName, RubyRegex, object>(ClrNameOps.Match), new Func<CallSiteStorage<Func<CallSite, RubyScope, object, object, object>>, RubyScope, ClrName, MutableString, object>(ClrNameOps.Match));
			LibraryInitializer.DefineLibraryMethod(module, "ruby_name", 81, 0u, new Func<RubyContext, ClrName, MutableString>(ClrNameOps.GetRubyName));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<ClrName, int>(ClrNameOps.GetLength));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<RubyContext, ClrName, MutableString>(ClrNameOps.GetRubyName));
			LibraryInitializer.DefineLibraryMethod(module, "to_str", 81, 0u, new Func<RubyContext, ClrName, MutableString>(ClrNameOps.GetRubyName));
			LibraryInitializer.DefineLibraryMethod(module, "to_sym", 81, 0u, new Func<RubyContext, ClrName, RubySymbol>(ClrNameOps.ToSymbol));
		}

		private static void LoadIronRuby__Clr__Name_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "clr_to_ruby", 97, 65536u, new Func<RubyClass, string, MutableString>(ClrNameOps.Mangle));
			LibraryInitializer.DefineLibraryMethod(module, "mangle", 97, 65536u, new Func<RubyClass, string, MutableString>(ClrNameOps.Mangle));
			LibraryInitializer.DefineLibraryMethod(module, "ruby_to_clr", 97, 65536u, new Func<RubyClass, string, MutableString>(ClrNameOps.Unmangle));
			LibraryInitializer.DefineLibraryMethod(module, "unmangle", 97, 65536u, new Func<RubyClass, string, MutableString>(ClrNameOps.Unmangle));
		}

		private static void LoadIronRuby__Clr__String_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "%", 81, 4u, 0u, new Func<StringFormatterSiteStorage, string, IList, string>(ClrString.Format), new Func<StringFormatterSiteStorage, ConversionStorage<IList>, string, object, string>(ClrString.Format));
			LibraryInitializer.DefineLibraryMethod(module, "*", 81, 65536u, new Func<string, int, string>(ClrString.Repeat));
			LibraryInitializer.DefineLibraryMethod(module, "[]", 81, new uint[6] { 65536u, 196608u, 4u, 2u, 4u, 262148u }, new Func<string, int, object>(ClrString.GetChar), new Func<string, int, int, string>(ClrString.GetSubstring), new Func<ConversionStorage<int>, string, Range, string>(ClrString.GetSubstring), new Func<string, string, string>(ClrString.GetSubstring), new Func<RubyScope, string, RubyRegex, string>(ClrString.GetSubstring), new Func<RubyScope, string, RubyRegex, int, string>(ClrString.GetSubstring));
			LibraryInitializer.DefineLibraryMethod(module, "+", 81, 65538u, new Func<string, MutableString, string>(ClrString.Concatenate));
			LibraryInitializer.DefineLibraryMethod(module, "<=>", 81, 2u, 2u, 0u, new Func<string, string, int>(ClrString.Compare), new Func<string, MutableString, int>(ClrString.Compare), new Func<BinaryOpStorage, RespondToStorage, string, object, object>(ClrString.Compare));
			LibraryInitializer.DefineLibraryMethod(module, "=~", 81, 4u, 2u, 0u, new Func<RubyScope, string, RubyRegex, object>(ClrString.Match), new Func<string, string, object>(ClrString.Match), new Func<CallSiteStorage<Func<CallSite, RubyScope, object, string, object>>, RubyScope, string, object, object>(ClrString.Match));
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 2u, 2u, 0u, new Func<string, string, bool>(ClrString.StringEquals), new Func<string, MutableString, bool>(ClrString.StringEquals), new Func<RespondToStorage, BinaryOpStorage, string, object, bool>(ClrString.Equals));
			LibraryInitializer.DefineLibraryMethod(module, "===", 81, 2u, 2u, 0u, new Func<string, string, bool>(ClrString.StringEquals), new Func<string, MutableString, bool>(ClrString.StringEquals), new Func<RespondToStorage, BinaryOpStorage, string, object, bool>(ClrString.Equals));
			LibraryInitializer.DefineLibraryMethod(module, "dump", 81, 0u, new Func<string, MutableString>(ClrString.Dump));
			LibraryInitializer.DefineLibraryMethod(module, "empty?", 81, 0u, new Func<string, bool>(ClrString.IsEmpty));
			LibraryInitializer.DefineLibraryMethod(module, "encoding", 81, 0u, new Func<string, RubyEncoding>(ClrString.GetEncoding));
			LibraryInitializer.DefineLibraryMethod(module, "eql?", 81, 2u, 2u, 0u, new Func<string, string, bool>(ClrString.Eql), new Func<string, MutableString, bool>(ClrString.Eql), new Func<string, object, bool>(ClrString.Eql));
			LibraryInitializer.DefineLibraryMethod(module, "hex", 81, 0u, new Func<string, object>(ClrString.ToIntegerHex));
			LibraryInitializer.DefineLibraryMethod(module, "include?", 81, 65538u, new Func<string, string, bool>(ClrString.Include));
			LibraryInitializer.DefineLibraryMethod(module, "insert", 81, 196612u, new Func<string, int, string, string>(ClrString.Insert));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<string, MutableString>(ClrString.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "intern", 81, 0u, new Func<RubyContext, string, RubySymbol>(ClrString.ToSymbol));
			LibraryInitializer.DefineLibraryMethod(module, "method_missing", 82, 2147483656u, new Func<RubyScope, BlockParam, string, RubySymbol, object[], object>(ClrString.MethodMissing));
			LibraryInitializer.DefineLibraryMethod(module, "oct", 81, 0u, new Func<string, object>(ClrString.ToIntegerOctal));
			LibraryInitializer.DefineLibraryMethod(module, "reverse", 81, 0u, new Func<string, string>(ClrString.GetReversed));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<string, int>(ClrString.GetLength));
			LibraryInitializer.DefineLibraryMethod(module, "slice", 81, new uint[6] { 65536u, 196608u, 4u, 2u, 4u, 262148u }, new Func<string, int, object>(ClrString.GetChar), new Func<string, int, int, string>(ClrString.GetSubstring), new Func<ConversionStorage<int>, string, Range, string>(ClrString.GetSubstring), new Func<string, string, string>(ClrString.GetSubstring), new Func<RubyScope, string, RubyRegex, string>(ClrString.GetSubstring), new Func<RubyScope, string, RubyRegex, int, string>(ClrString.GetSubstring));
			LibraryInitializer.DefineLibraryMethod(module, "split", 81, 0u, 393216u, 262148u, new Func<ConversionStorage<MutableString>, string, RubyArray>(ClrString.Split), new Func<ConversionStorage<MutableString>, string, string, int, RubyArray>(ClrString.Split), new Func<ConversionStorage<MutableString>, string, RubyRegex, int, RubyArray>(ClrString.Split));
			LibraryInitializer.DefineLibraryMethod(module, "to_clr_string", 81, 0u, new Func<string, string>(ClrString.ToClrString));
			LibraryInitializer.DefineLibraryMethod(module, "to_f", 81, 0u, new Func<string, double>(ClrString.ToDouble));
			LibraryInitializer.DefineLibraryMethod(module, "to_i", 81, 65536u, new Func<string, int, object>(ClrString.ToInteger));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<string, MutableString>(ClrString.ToStr));
			LibraryInitializer.DefineLibraryMethod(module, "to_str", 81, 0u, new Func<string, MutableString>(ClrString.ToStr));
			LibraryInitializer.DefineLibraryMethod(module, "to_sym", 81, 0u, new Func<RubyContext, string, RubySymbol>(ClrString.ToSymbol));
		}

		private static void LoadIronRuby__Print_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "<<", 81, 0u, new Func<BinaryOpStorage, object, object, object>(PrintOps.Output));
			LibraryInitializer.DefineLibraryMethod(module, "print", 81, 0u, 2147483648u, 0u, new Action<BinaryOpStorage, RubyScope, object>(PrintOps.Print), new Action<BinaryOpStorage, object, object[]>(PrintOps.Print), new Action<BinaryOpStorage, object, object>(PrintOps.Print));
			LibraryInitializer.DefineLibraryMethod(module, "printf", 81, 2148007952u, new Action<StringFormatterSiteStorage, ConversionStorage<MutableString>, BinaryOpStorage, object, MutableString, object[]>(PrintOps.PrintFormatted));
			LibraryInitializer.DefineLibraryMethod(module, "putc", 81, 4u, 131072u, new Func<BinaryOpStorage, object, MutableString, MutableString>(PrintOps.Putc), new Func<BinaryOpStorage, object, int, int>(PrintOps.Putc));
			LibraryInitializer.DefineLibraryMethod(module, "puts", 81, 0u, 4u, 16u, 2147483648u, new Action<BinaryOpStorage, object>(PrintOps.PutsEmptyLine), new Action<BinaryOpStorage, object, MutableString>(PrintOps.Puts), new Action<BinaryOpStorage, ConversionStorage<MutableString>, ConversionStorage<IList>, object, object>(PrintOps.Puts), new Action<BinaryOpStorage, ConversionStorage<MutableString>, ConversionStorage<IList>, object, object[]>(PrintOps.Puts));
		}

		private static void LoadKernel_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "!~", 81, 0u, new Func<BinaryOpStorage, object, object, bool>(KernelOps.NotMatch));
			LibraryInitializer.DefineLibraryMethod(module, "__id__", 81, 0u, new Func<RubyContext, object, object>(KernelOps.GetObjectId));
			LibraryInitializer.DefineLibraryMethod(module, "`", 82, 131076u, new Func<RubyContext, object, MutableString, MutableString>(KernelOps.ExecuteCommand));
			LibraryInitializer.DefineLibraryMethod(module, "<=>", 81, 0u, new Func<BinaryOpStorage, object, object, object>(KernelOps.Compare));
			LibraryInitializer.DefineLibraryMethod(module, "=~", 81, 0u, new Func<object, object, object>(KernelOps.Match));
			LibraryInitializer.DefineLibraryMethod(module, "===", 81, 0u, new Func<BinaryOpStorage, object, object, bool>(KernelOps.CaseEquals));
			LibraryInitializer.DefineLibraryMethod(module, "abort", 82, 0u, 131076u, new Action<object>(KernelOps.Abort), new Action<BinaryOpStorage, object, MutableString>(KernelOps.Abort));
			LibraryInitializer.DefineLibraryMethod(module, "Array", 82, 0u, new Func<ConversionStorage<IList>, ConversionStorage<IList>, object, object, IList>(KernelOps.ToArray));
			LibraryInitializer.DefineLibraryMethod(module, "at_exit", 82, 0u, new Func<BlockParam, object, Proc>(KernelOps.AtExit));
			LibraryInitializer.DefineLibraryMethod(module, "autoload", 82, 393228u, new Action<RubyScope, object, string, MutableString>(KernelOps.SetAutoloadedConstant));
			LibraryInitializer.DefineLibraryMethod(module, "autoload?", 82, 131076u, new Func<RubyScope, object, string, MutableString>(KernelOps.GetAutoloadedConstantPath));
			LibraryInitializer.DefineLibraryMethod(module, "binding", 82, 0u, new Func<RubyScope, object, Binding>(KernelOps.GetLocalScope));
			LibraryInitializer.DefineLibraryMethod(module, "block_given?", 82, 0u, new Func<RubyScope, object, bool>(KernelOps.HasBlock));
			LibraryInitializer.DefineLibraryMethod(module, "caller", 82, 0u, new Func<RubyContext, object, int, RubyArray>(KernelOps.GetStackTrace));
			LibraryInitializer.DefineLibraryMethod(module, "catch", 82, 0u, new Func<BlockParam, object, object, object>(KernelOps.Catch));
			LibraryInitializer.DefineLibraryMethod(module, "class", 81, 0u, new Func<RubyContext, object, RubyClass>(KernelOps.GetClass));
			LibraryInitializer.DefineLibraryMethod(module, "clone", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, object, object>>, CallSiteStorage<Func<CallSite, RubyClass, object>>, object, object>(KernelOps.Clone));
			LibraryInitializer.DefineLibraryMethod(module, "clr_member", 81, 262156u, new Func<RubyContext, object, object, string, RubyMethod>(KernelOps.GetClrMember));
			LibraryInitializer.DefineLibraryMethod(module, "Complex", 82, 0u, new Func<CallSiteStorage<Func<CallSite, object, object, object, object>>, RubyScope, object, object, object, object>(KernelOps.ToComplex));
			LibraryInitializer.DefineLibraryMethod(module, "define_singleton_method", 81, new uint[8] { 12u, 131084u, 12u, 131084u, 12u, 262154u, 10u, 131084u }, new Func<RubyScope, object, ClrName, Proc, Proc>(KernelOps.DefineSingletonMethod), new Func<RubyScope, object, string, RubyMethod, RubyMethod>(KernelOps.DefineSingletonMethod), new Func<RubyScope, object, ClrName, RubyMethod, RubyMethod>(KernelOps.DefineSingletonMethod), new Func<RubyScope, object, string, UnboundMethod, UnboundMethod>(KernelOps.DefineSingletonMethod), new Func<RubyScope, object, ClrName, UnboundMethod, UnboundMethod>(KernelOps.DefineSingletonMethod), new Func<RubyScope, BlockParam, object, string, Proc>(KernelOps.DefineSingletonMethod), new Func<RubyScope, BlockParam, object, ClrName, Proc>(KernelOps.DefineSingletonMethod), new Func<RubyScope, object, string, Proc, Proc>(KernelOps.DefineSingletonMethod));
			LibraryInitializer.DefineLibraryMethod(module, "display", 81, 0u, new Action<BinaryOpStorage, object>(KernelOps.Display));
			LibraryInitializer.DefineLibraryMethod(module, "dup", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, object, object>>, CallSiteStorage<Func<CallSite, RubyClass, object>>, object, object>(KernelOps.Duplicate));
			LibraryInitializer.DefineLibraryMethod(module, "enum_for", 81, 2147549186u, new Func<object, string, object[], Enumerator>(KernelOps.Create));
			LibraryInitializer.DefineLibraryMethod(module, "eql?", 81, 1u, 0u, new Func<IRubyObject, object, bool>(KernelOps.ValueEquals), new Func<object, object, bool>(KernelOps.ValueEquals));
			LibraryInitializer.DefineLibraryMethod(module, "eval", 82, 20u, 28u, new Func<RubyScope, object, MutableString, Binding, MutableString, int, object>(KernelOps.Evaluate), new Func<RubyScope, object, MutableString, Proc, MutableString, int, object>(KernelOps.Evaluate));
			LibraryInitializer.DefineLibraryMethod(module, "exec", 82, 131076u, 2147876876u, new Action<RubyContext, object, MutableString>(KernelOps.Execute), new Action<RubyContext, object, MutableString, MutableString[]>(KernelOps.Execute));
			LibraryInitializer.DefineLibraryMethod(module, "exit", 82, 0u, 2u, 65536u, new Action<object>(KernelOps.Exit), new Action<object, bool>(KernelOps.Exit), new Action<object, int>(KernelOps.Exit));
			LibraryInitializer.DefineLibraryMethod(module, "exit!", 82, 0u, 0u, 0u, new Action<RubyContext, object>(KernelOps.TerminateExecution), new Action<RubyContext, object, bool>(KernelOps.TerminateExecution), new Action<RubyContext, object, int>(KernelOps.TerminateExecution));
			LibraryInitializer.DefineLibraryMethod(module, "extend", 81, 2147483672u, new Func<CallSiteStorage<Func<CallSite, RubyModule, object, object>>, CallSiteStorage<Func<CallSite, RubyModule, object, object>>, object, RubyModule, RubyModule[], object>(KernelOps.Extend));
			LibraryInitializer.DefineLibraryMethod(module, "fail", 82, 0u, 2u, 0u, new Action<RubyContext, object>(KernelOps.RaiseException), new Action<object, MutableString>(KernelOps.RaiseException), new Action<RespondToStorage, UnaryOpStorage, BinaryOpStorage, CallSiteStorage<Action<CallSite, Exception, RubyArray>>, object, object, object, RubyArray>(KernelOps.RaiseException));
			LibraryInitializer.DefineLibraryMethod(module, "Float", 82, 65536u, new Func<object, double, double>(KernelOps.ToFloat));
			LibraryInitializer.DefineLibraryMethod(module, "format", 82, 2147614724u, new Func<StringFormatterSiteStorage, object, MutableString, object[], MutableString>(KernelOps.Sprintf));
			LibraryInitializer.DefineLibraryMethod(module, "freeze", 81, 0u, new Func<RubyContext, object, object>(KernelOps.Freeze));
			LibraryInitializer.DefineLibraryMethod(module, "frozen?", 81, 1u, 0u, new Func<MutableString, bool>(KernelOps.Frozen), new Func<RubyContext, object, bool>(KernelOps.Frozen));
			LibraryInitializer.DefineLibraryMethod(module, "gets", 82, 0u, 4u, 262148u, new Func<CallSiteStorage<Func<CallSite, object, object>>, object, object>(KernelOps.ReadInputLine), new Func<CallSiteStorage<Func<CallSite, object, object, object>>, object, MutableString, object>(KernelOps.ReadInputLine), new Func<CallSiteStorage<Func<CallSite, object, object, object, object>>, object, MutableString, int, object>(KernelOps.ReadInputLine));
			LibraryInitializer.DefineLibraryMethod(module, "global_variables", 82, 0u, new Func<RubyContext, object, RubyArray>(KernelOps.GetGlobalVariableNames));
			LibraryInitializer.DefineLibraryMethod(module, "hash", 81, 1u, 0u, new Func<IRubyObject, int>(KernelOps.Hash), new Func<object, int>(KernelOps.Hash));
			LibraryInitializer.DefineLibraryMethod(module, "id", 81, 0u, new Func<RubyContext, object, object>(KernelOps.GetId));
			LibraryInitializer.DefineLibraryMethod(module, "initialize_copy", 82, 0u, new Func<RubyContext, object, object, object>(KernelOps.InitializeCopy));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<UnaryOpStorage, ConversionStorage<MutableString>, object, MutableString>(KernelOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "instance_of?", 81, 0u, new Func<object, RubyModule, bool>(KernelOps.IsOfClass));
			LibraryInitializer.DefineLibraryMethod(module, "instance_variable_defined?", 81, 131076u, new Func<RubyContext, object, string, bool>(KernelOps.InstanceVariableDefined));
			LibraryInitializer.DefineLibraryMethod(module, "instance_variable_get", 81, 131076u, new Func<RubyContext, object, string, object>(KernelOps.InstanceVariableGet));
			LibraryInitializer.DefineLibraryMethod(module, "instance_variable_set", 81, 131076u, new Func<RubyContext, object, string, object, object>(KernelOps.InstanceVariableSet));
			LibraryInitializer.DefineLibraryMethod(module, "instance_variables", 81, 0u, new Func<RubyContext, object, RubyArray>(KernelOps.GetInstanceVariableNames));
			LibraryInitializer.DefineLibraryMethod(module, "Integer", 82, 2u, 0u, new Func<object, MutableString, object>(KernelOps.ToInteger), new Func<ConversionStorage<IntegerValue>, object, object, object>(KernelOps.ToInteger));
			LibraryInitializer.DefineLibraryMethod(module, "is_a?", 81, 0u, new Func<object, RubyModule, bool>(KernelOps.IsKindOf));
			LibraryInitializer.DefineLibraryMethod(module, "iterator?", 82, 0u, new Func<RubyScope, object, bool>(KernelOps.HasBlock));
			LibraryInitializer.DefineLibraryMethod(module, "kind_of?", 81, 0u, new Func<object, RubyModule, bool>(KernelOps.IsKindOf));
			LibraryInitializer.DefineLibraryMethod(module, "lambda", 82, 0u, new Func<BlockParam, object, Proc>(KernelOps.CreateLambda));
			LibraryInitializer.DefineLibraryMethod(module, "load", 82, 0u, new Func<ConversionStorage<MutableString>, RubyScope, object, object, bool, bool>(KernelOps.Load));
			LibraryInitializer.DefineLibraryMethod(module, "load_assembly", 82, 393228u, new Func<RubyContext, object, MutableString, MutableString, bool>(KernelOps.LoadAssembly));
			LibraryInitializer.DefineLibraryMethod(module, "local_variables", 82, 0u, new Func<RubyScope, object, RubyArray>(KernelOps.GetLocalVariableNames));
			LibraryInitializer.DefineLibraryMethod(module, "loop", 82, 0u, new Func<BlockParam, object, object>(KernelOps.Loop));
			LibraryInitializer.DefineLibraryMethod(module, "method", 81, 131076u, new Func<RubyContext, object, string, RubyMethod>(KernelOps.GetMethod));
			LibraryInitializer.DefineLibraryMethod(module, "methods", 81, 0u, new Func<RubyContext, object, bool, RubyArray>(KernelOps.GetMethods));
			LibraryInitializer.DefineLibraryMethod(module, "nil?", 81, 0u, new Func<object, bool>(KernelOps.IsNil));
			LibraryInitializer.DefineLibraryMethod(module, "object_id", 81, 0u, new Func<RubyContext, object, object>(KernelOps.GetObjectId));
			LibraryInitializer.DefineLibraryMethod(module, "open", 82, 917508u, 1835018u, 655364u, 1310730u, new Func<RubyContext, object, MutableString, MutableString, int, RubyIO>(KernelOps.Open), new Func<RubyContext, BlockParam, object, MutableString, MutableString, int, object>(KernelOps.Open), new Func<RubyContext, object, MutableString, int, int, RubyIO>(KernelOps.Open), new Func<RubyContext, BlockParam, object, MutableString, int, int, object>(KernelOps.Open));
			LibraryInitializer.DefineLibraryMethod(module, "p", 82, 2147483648u, new Func<BinaryOpStorage, UnaryOpStorage, ConversionStorage<MutableString>, object, object[], object>(KernelOps.PrintInspect));
			LibraryInitializer.DefineLibraryMethod(module, "print", 82, 0u, 0u, 2147483648u, new Action<BinaryOpStorage, RubyScope, object>(KernelOps.Print), new Action<BinaryOpStorage, object, object>(KernelOps.Print), new Action<BinaryOpStorage, object, object[]>(KernelOps.Print));
			LibraryInitializer.DefineLibraryMethod(module, "printf", 82, 2147483664u, 2147483680u, new Action<StringFormatterSiteStorage, ConversionStorage<MutableString>, BinaryOpStorage, object, MutableString, object[]>(KernelOps.PrintFormatted), new Action<StringFormatterSiteStorage, ConversionStorage<MutableString>, BinaryOpStorage, object, object, object, object[]>(KernelOps.PrintFormatted));
			LibraryInitializer.DefineLibraryMethod(module, "private_methods", 81, 0u, new Func<RubyContext, object, bool, RubyArray>(KernelOps.GetPrivateMethods));
			LibraryInitializer.DefineLibraryMethod(module, "proc", 82, 0u, new Func<BlockParam, object, Proc>(KernelOps.CreateProc));
			LibraryInitializer.DefineLibraryMethod(module, "protected_methods", 81, 0u, new Func<RubyContext, object, bool, RubyArray>(KernelOps.GetProtectedMethods));
			LibraryInitializer.DefineLibraryMethod(module, "public_methods", 81, 0u, new Func<RubyContext, object, bool, RubyArray>(KernelOps.GetPublicMethods));
			LibraryInitializer.DefineLibraryMethod(module, "putc", 82, 4u, 131072u, new Func<BinaryOpStorage, object, MutableString, MutableString>(KernelOps.Putc), new Func<BinaryOpStorage, object, int, int>(KernelOps.Putc));
			LibraryInitializer.DefineLibraryMethod(module, "puts", 82, 0u, 0u, 4u, 2147483648u, new Action<BinaryOpStorage, object>(KernelOps.PutsEmptyLine), new Action<BinaryOpStorage, ConversionStorage<MutableString>, ConversionStorage<IList>, object, object>(KernelOps.PutString), new Action<BinaryOpStorage, object, MutableString>(KernelOps.PutString), new Action<BinaryOpStorage, ConversionStorage<MutableString>, ConversionStorage<IList>, object, object[]>(KernelOps.PutString));
			LibraryInitializer.DefineLibraryMethod(module, "raise", 82, 0u, 2u, 0u, new Action<RubyContext, object>(KernelOps.RaiseException), new Action<object, MutableString>(KernelOps.RaiseException), new Action<RespondToStorage, UnaryOpStorage, BinaryOpStorage, CallSiteStorage<Action<CallSite, Exception, RubyArray>>, object, object, object, RubyArray>(KernelOps.RaiseException));
			LibraryInitializer.DefineLibraryMethod(module, "rand", 82, 0u, 0u, 0u, new Func<RubyContext, object, double>(KernelOps.Random), new Func<RubyContext, object, int, object>(KernelOps.Random), new Func<ConversionStorage<IntegerValue>, RubyContext, object, object, object>(KernelOps.Random));
			LibraryInitializer.DefineLibraryMethod(module, "Rational", 82, 0u, new Func<CallSiteStorage<Func<CallSite, object, object, object, object>>, RubyScope, object, object, object, object>(KernelOps.ToRational));
			LibraryInitializer.DefineLibraryMethod(module, "remove_instance_variable", 82, 131076u, new Func<RubyContext, object, string, object>(KernelOps.RemoveInstanceVariable));
			LibraryInitializer.DefineLibraryMethod(module, "require", 82, 0u, new Func<ConversionStorage<MutableString>, RubyScope, object, object, bool>(KernelOps.Require));
			LibraryInitializer.DefineLibraryMethod(module, "respond_to?", 81, 131076u, new Func<RubyContext, object, string, bool, bool>(KernelOps.RespondTo));
			LibraryInitializer.DefineLibraryMethod(module, "select", 82, 0u, 0u, 0u, new Func<RubyContext, object, RubyArray, RubyArray, RubyArray, RubyArray>(KernelOps.Select), new Func<RubyContext, object, RubyArray, RubyArray, RubyArray, int, RubyArray>(KernelOps.Select), new Func<RubyContext, object, RubyArray, RubyArray, RubyArray, double, RubyArray>(KernelOps.Select));
			LibraryInitializer.DefineLibraryMethod(module, "send", 81, new uint[11]
			{
				0u, 131076u, 262152u, 131076u, 262152u, 131076u, 262152u, 131076u, 262152u, 2147614724u,
				2147745800u
			}, new Func<RubyScope, object, object>(KernelOps.SendMessage), new Func<RubyScope, object, string, object>(KernelOps.SendMessage), new Func<RubyScope, BlockParam, object, string, object>(KernelOps.SendMessage), new Func<RubyScope, object, string, object, object>(KernelOps.SendMessage), new Func<RubyScope, BlockParam, object, string, object, object>(KernelOps.SendMessage), new Func<RubyScope, object, string, object, object, object>(KernelOps.SendMessage), new Func<RubyScope, BlockParam, object, string, object, object, object>(KernelOps.SendMessage), new Func<RubyScope, object, string, object, object, object, object>(KernelOps.SendMessage), new Func<RubyScope, BlockParam, object, string, object, object, object, object>(KernelOps.SendMessage), new Func<RubyScope, object, string, object[], object>(KernelOps.SendMessage), new Func<RubyScope, BlockParam, object, string, object[], object>(KernelOps.SendMessage));
			LibraryInitializer.DefineLibraryMethod(module, "set_trace_func", 82, 0u, new Func<RubyContext, object, Proc, Proc>(KernelOps.SetTraceListener));
			LibraryInitializer.DefineLibraryMethod(module, "singleton_methods", 81, 0u, new Func<RubyContext, object, bool, RubyArray>(KernelOps.GetSingletonMethods));
			LibraryInitializer.DefineLibraryMethod(module, "sleep", 82, 0u, 0u, 0u, new Action<object>(KernelOps.Sleep), new Func<object, int, int>(KernelOps.Sleep), new Func<object, double, int>(KernelOps.Sleep));
			LibraryInitializer.DefineLibraryMethod(module, "sprintf", 82, 2147614724u, new Func<StringFormatterSiteStorage, object, MutableString, object[], MutableString>(KernelOps.Sprintf));
			LibraryInitializer.DefineLibraryMethod(module, "srand", 82, 0u, 131072u, new Func<RubyContext, object, object>(KernelOps.SeedRandomNumberGenerator), new Func<RubyContext, object, IntegerValue, object>(KernelOps.SeedRandomNumberGenerator));
			LibraryInitializer.DefineLibraryMethod(module, "String", 82, 0u, new Func<ConversionStorage<MutableString>, object, object, object>(KernelOps.ToString));
			LibraryInitializer.DefineLibraryMethod(module, "system", 82, 131076u, 2147876876u, new Func<RubyContext, object, MutableString, bool>(KernelOps.System), new Func<RubyContext, object, MutableString, MutableString[], bool>(KernelOps.System));
			LibraryInitializer.DefineLibraryMethod(module, "taint", 81, 0u, new Func<RubyContext, object, object>(KernelOps.Taint));
			LibraryInitializer.DefineLibraryMethod(module, "tainted?", 81, 0u, new Func<RubyContext, object, bool>(KernelOps.Tainted));
			LibraryInitializer.DefineLibraryMethod(module, "tap", 81, 2u, new Func<RubyScope, BlockParam, object, object>(KernelOps.Tap));
			LibraryInitializer.DefineLibraryMethod(module, "test", 82, 4u, 131072u, 786456u, new Func<ConversionStorage<MutableString>, object, MutableString, object, object>(KernelOps.Test), new Func<ConversionStorage<MutableString>, object, int, object, object>(KernelOps.Test), new Func<RubyContext, object, int, MutableString, MutableString, object>(KernelOps.Test));
			LibraryInitializer.DefineLibraryMethod(module, "throw", 82, 0u, new Action<RubyContext, object, object, object>(KernelOps.Throw));
			LibraryInitializer.DefineLibraryMethod(module, "to_enum", 81, 2147549186u, new Func<object, string, object[], Enumerator>(KernelOps.Create));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 1u, 0u, new Func<IRubyObject, MutableString>(KernelOps.ToS), new Func<object, MutableString>(KernelOps.ToS));
			LibraryInitializer.DefineLibraryMethod(module, "trap", 82, 0u, 2u, new Func<RubyContext, object, object, Proc, object>(KernelOps.Trap), new Func<RubyContext, BlockParam, object, object, object>(KernelOps.Trap));
			LibraryInitializer.DefineLibraryMethod(module, "trust", 81, 0u, new Func<RubyContext, object, object>(KernelOps.Trust));
			LibraryInitializer.DefineLibraryMethod(module, "type", 81, 0u, new Func<RubyContext, object, RubyClass>(KernelOps.GetClassObsolete));
			LibraryInitializer.DefineLibraryMethod(module, "untaint", 81, 0u, new Func<RubyContext, object, object>(KernelOps.Untaint));
			LibraryInitializer.DefineLibraryMethod(module, "untrust", 81, 0u, new Func<RubyContext, object, object>(KernelOps.Untrust));
			LibraryInitializer.DefineLibraryMethod(module, "untrusted?", 81, 0u, new Func<RubyContext, object, bool>(KernelOps.Untrusted));
			LibraryInitializer.DefineLibraryMethod(module, "using_clr_extensions", 82, 0u, new Action<RubyContext, object, RubyModule>(KernelOps.UsingClrExtensions));
			LibraryInitializer.DefineLibraryMethod(module, "warn", 82, 0u, new Action<BinaryOpStorage, ConversionStorage<MutableString>, object, object>(KernelOps.ReportWarning));
		}

		private static void LoadKernel_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "`", 97, 131076u, new Func<RubyContext, object, MutableString, MutableString>(KernelOps.ExecuteCommand));
			LibraryInitializer.DefineLibraryMethod(module, "abort", 97, 0u, 131076u, new Action<object>(KernelOps.Abort), new Action<BinaryOpStorage, object, MutableString>(KernelOps.Abort));
			LibraryInitializer.DefineLibraryMethod(module, "Array", 97, 0u, new Func<ConversionStorage<IList>, ConversionStorage<IList>, object, object, IList>(KernelOps.ToArray));
			LibraryInitializer.DefineLibraryMethod(module, "at_exit", 97, 0u, new Func<BlockParam, object, Proc>(KernelOps.AtExit));
			LibraryInitializer.DefineLibraryMethod(module, "autoload", 97, 393228u, new Action<RubyScope, object, string, MutableString>(KernelOps.SetAutoloadedConstant));
			LibraryInitializer.DefineLibraryMethod(module, "autoload?", 97, 131076u, new Func<RubyScope, object, string, MutableString>(KernelOps.GetAutoloadedConstantPath));
			LibraryInitializer.DefineLibraryMethod(module, "binding", 97, 0u, new Func<RubyScope, object, Binding>(KernelOps.GetLocalScope));
			LibraryInitializer.DefineLibraryMethod(module, "block_given?", 97, 0u, new Func<RubyScope, object, bool>(KernelOps.HasBlock));
			LibraryInitializer.DefineLibraryMethod(module, "caller", 97, 0u, new Func<RubyContext, object, int, RubyArray>(KernelOps.GetStackTrace));
			LibraryInitializer.DefineLibraryMethod(module, "catch", 97, 0u, new Func<BlockParam, object, object, object>(KernelOps.Catch));
			LibraryInitializer.DefineLibraryMethod(module, "Complex", 97, 0u, new Func<CallSiteStorage<Func<CallSite, object, object, object, object>>, RubyScope, object, object, object, object>(KernelOps.ToComplex));
			LibraryInitializer.DefineLibraryMethod(module, "eval", 97, 20u, 28u, new Func<RubyScope, object, MutableString, Binding, MutableString, int, object>(KernelOps.Evaluate), new Func<RubyScope, object, MutableString, Proc, MutableString, int, object>(KernelOps.Evaluate));
			LibraryInitializer.DefineLibraryMethod(module, "exec", 97, 131076u, 2147876876u, new Action<RubyContext, object, MutableString>(KernelOps.Execute), new Action<RubyContext, object, MutableString, MutableString[]>(KernelOps.Execute));
			LibraryInitializer.DefineLibraryMethod(module, "exit", 97, 0u, 2u, 65536u, new Action<object>(KernelOps.Exit), new Action<object, bool>(KernelOps.Exit), new Action<object, int>(KernelOps.Exit));
			LibraryInitializer.DefineLibraryMethod(module, "exit!", 97, 0u, 0u, 0u, new Action<RubyContext, object>(KernelOps.TerminateExecution), new Action<RubyContext, object, bool>(KernelOps.TerminateExecution), new Action<RubyContext, object, int>(KernelOps.TerminateExecution));
			LibraryInitializer.DefineLibraryMethod(module, "fail", 97, 0u, 2u, 0u, new Action<RubyContext, object>(KernelOps.RaiseException), new Action<object, MutableString>(KernelOps.RaiseException), new Action<RespondToStorage, UnaryOpStorage, BinaryOpStorage, CallSiteStorage<Action<CallSite, Exception, RubyArray>>, object, object, object, RubyArray>(KernelOps.RaiseException));
			LibraryInitializer.DefineLibraryMethod(module, "Float", 97, 65536u, new Func<object, double, double>(KernelOps.ToFloat));
			LibraryInitializer.DefineLibraryMethod(module, "format", 97, 2147614724u, new Func<StringFormatterSiteStorage, object, MutableString, object[], MutableString>(KernelOps.Sprintf));
			LibraryInitializer.DefineLibraryMethod(module, "gets", 97, 0u, 4u, 262148u, new Func<CallSiteStorage<Func<CallSite, object, object>>, object, object>(KernelOps.ReadInputLine), new Func<CallSiteStorage<Func<CallSite, object, object, object>>, object, MutableString, object>(KernelOps.ReadInputLine), new Func<CallSiteStorage<Func<CallSite, object, object, object, object>>, object, MutableString, int, object>(KernelOps.ReadInputLine));
			LibraryInitializer.DefineLibraryMethod(module, "global_variables", 97, 0u, new Func<RubyContext, object, RubyArray>(KernelOps.GetGlobalVariableNames));
			LibraryInitializer.DefineLibraryMethod(module, "Integer", 97, 2u, 0u, new Func<object, MutableString, object>(KernelOps.ToInteger), new Func<ConversionStorage<IntegerValue>, object, object, object>(KernelOps.ToInteger));
			LibraryInitializer.DefineLibraryMethod(module, "iterator?", 97, 0u, new Func<RubyScope, object, bool>(KernelOps.HasBlock));
			LibraryInitializer.DefineLibraryMethod(module, "lambda", 97, 0u, new Func<BlockParam, object, Proc>(KernelOps.CreateLambda));
			LibraryInitializer.DefineLibraryMethod(module, "load", 97, 0u, new Func<ConversionStorage<MutableString>, RubyScope, object, object, bool, bool>(KernelOps.Load));
			LibraryInitializer.DefineLibraryMethod(module, "load_assembly", 97, 393228u, new Func<RubyContext, object, MutableString, MutableString, bool>(KernelOps.LoadAssembly));
			LibraryInitializer.DefineLibraryMethod(module, "local_variables", 97, 0u, new Func<RubyScope, object, RubyArray>(KernelOps.GetLocalVariableNames));
			LibraryInitializer.DefineLibraryMethod(module, "loop", 97, 0u, new Func<BlockParam, object, object>(KernelOps.Loop));
			LibraryInitializer.DefineLibraryMethod(module, "open", 97, 917508u, 1835018u, 655364u, 1310730u, new Func<RubyContext, object, MutableString, MutableString, int, RubyIO>(KernelOps.Open), new Func<RubyContext, BlockParam, object, MutableString, MutableString, int, object>(KernelOps.Open), new Func<RubyContext, object, MutableString, int, int, RubyIO>(KernelOps.Open), new Func<RubyContext, BlockParam, object, MutableString, int, int, object>(KernelOps.Open));
			LibraryInitializer.DefineLibraryMethod(module, "p", 97, 2147483648u, new Func<BinaryOpStorage, UnaryOpStorage, ConversionStorage<MutableString>, object, object[], object>(KernelOps.PrintInspect));
			LibraryInitializer.DefineLibraryMethod(module, "print", 97, 0u, 0u, 2147483648u, new Action<BinaryOpStorage, RubyScope, object>(KernelOps.Print), new Action<BinaryOpStorage, object, object>(KernelOps.Print), new Action<BinaryOpStorage, object, object[]>(KernelOps.Print));
			LibraryInitializer.DefineLibraryMethod(module, "printf", 97, 2147483664u, 2147483680u, new Action<StringFormatterSiteStorage, ConversionStorage<MutableString>, BinaryOpStorage, object, MutableString, object[]>(KernelOps.PrintFormatted), new Action<StringFormatterSiteStorage, ConversionStorage<MutableString>, BinaryOpStorage, object, object, object, object[]>(KernelOps.PrintFormatted));
			LibraryInitializer.DefineLibraryMethod(module, "proc", 97, 0u, new Func<BlockParam, object, Proc>(KernelOps.CreateProc));
			LibraryInitializer.DefineLibraryMethod(module, "putc", 97, 4u, 131072u, new Func<BinaryOpStorage, object, MutableString, MutableString>(KernelOps.Putc), new Func<BinaryOpStorage, object, int, int>(KernelOps.Putc));
			LibraryInitializer.DefineLibraryMethod(module, "puts", 97, 0u, 0u, 4u, 2147483648u, new Action<BinaryOpStorage, object>(KernelOps.PutsEmptyLine), new Action<BinaryOpStorage, ConversionStorage<MutableString>, ConversionStorage<IList>, object, object>(KernelOps.PutString), new Action<BinaryOpStorage, object, MutableString>(KernelOps.PutString), new Action<BinaryOpStorage, ConversionStorage<MutableString>, ConversionStorage<IList>, object, object[]>(KernelOps.PutString));
			LibraryInitializer.DefineLibraryMethod(module, "raise", 97, 0u, 2u, 0u, new Action<RubyContext, object>(KernelOps.RaiseException), new Action<object, MutableString>(KernelOps.RaiseException), new Action<RespondToStorage, UnaryOpStorage, BinaryOpStorage, CallSiteStorage<Action<CallSite, Exception, RubyArray>>, object, object, object, RubyArray>(KernelOps.RaiseException));
			LibraryInitializer.DefineLibraryMethod(module, "rand", 97, 0u, 0u, 0u, new Func<RubyContext, object, double>(KernelOps.Random), new Func<RubyContext, object, int, object>(KernelOps.Random), new Func<ConversionStorage<IntegerValue>, RubyContext, object, object, object>(KernelOps.Random));
			LibraryInitializer.DefineLibraryMethod(module, "Rational", 97, 0u, new Func<CallSiteStorage<Func<CallSite, object, object, object, object>>, RubyScope, object, object, object, object>(KernelOps.ToRational));
			LibraryInitializer.DefineLibraryMethod(module, "require", 97, 0u, new Func<ConversionStorage<MutableString>, RubyScope, object, object, bool>(KernelOps.Require));
			LibraryInitializer.DefineLibraryMethod(module, "select", 97, 0u, 0u, 0u, new Func<RubyContext, object, RubyArray, RubyArray, RubyArray, RubyArray>(KernelOps.Select), new Func<RubyContext, object, RubyArray, RubyArray, RubyArray, int, RubyArray>(KernelOps.Select), new Func<RubyContext, object, RubyArray, RubyArray, RubyArray, double, RubyArray>(KernelOps.Select));
			LibraryInitializer.DefineLibraryMethod(module, "set_trace_func", 97, 0u, new Func<RubyContext, object, Proc, Proc>(KernelOps.SetTraceListener));
			LibraryInitializer.DefineLibraryMethod(module, "sleep", 97, 0u, 0u, 0u, new Action<object>(KernelOps.Sleep), new Func<object, int, int>(KernelOps.Sleep), new Func<object, double, int>(KernelOps.Sleep));
			LibraryInitializer.DefineLibraryMethod(module, "sprintf", 97, 2147614724u, new Func<StringFormatterSiteStorage, object, MutableString, object[], MutableString>(KernelOps.Sprintf));
			LibraryInitializer.DefineLibraryMethod(module, "srand", 97, 0u, 131072u, new Func<RubyContext, object, object>(KernelOps.SeedRandomNumberGenerator), new Func<RubyContext, object, IntegerValue, object>(KernelOps.SeedRandomNumberGenerator));
			LibraryInitializer.DefineLibraryMethod(module, "String", 97, 0u, new Func<ConversionStorage<MutableString>, object, object, object>(KernelOps.ToString));
			LibraryInitializer.DefineLibraryMethod(module, "system", 97, 131076u, 2147876876u, new Func<RubyContext, object, MutableString, bool>(KernelOps.System), new Func<RubyContext, object, MutableString, MutableString[], bool>(KernelOps.System));
			LibraryInitializer.DefineLibraryMethod(module, "test", 97, 4u, 131072u, 786456u, new Func<ConversionStorage<MutableString>, object, MutableString, object, object>(KernelOps.Test), new Func<ConversionStorage<MutableString>, object, int, object, object>(KernelOps.Test), new Func<RubyContext, object, int, MutableString, MutableString, object>(KernelOps.Test));
			LibraryInitializer.DefineLibraryMethod(module, "throw", 97, 0u, new Action<RubyContext, object, object, object>(KernelOps.Throw));
			LibraryInitializer.DefineLibraryMethod(module, "trap", 97, 0u, 2u, new Func<RubyContext, object, object, Proc, object>(KernelOps.Trap), new Func<RubyContext, BlockParam, object, object, object>(KernelOps.Trap));
			LibraryInitializer.DefineLibraryMethod(module, "using_clr_extensions", 97, 0u, new Action<RubyContext, object, RubyModule>(KernelOps.UsingClrExtensions));
			LibraryInitializer.DefineLibraryMethod(module, "warn", 97, 0u, new Action<BinaryOpStorage, ConversionStorage<MutableString>, object, object>(KernelOps.ReportWarning));
		}

		private static void LoadMarshal_Constants(RubyModule module)
		{
			LibraryInitializer.SetBuiltinConstant(module, "MAJOR_VERSION", 4);
			LibraryInitializer.SetBuiltinConstant(module, "MINOR_VERSION", 8);
		}

		private static void LoadMarshal_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "dump", 97, 0u, 0u, 8u, 0u, new Func<RubyMarshal.WriterSites, RubyModule, object, MutableString>(RubyMarshal.Dump), new Func<RubyMarshal.WriterSites, RubyModule, object, int, MutableString>(RubyMarshal.Dump), new Func<RubyMarshal.WriterSites, RubyModule, object, RubyIO, int?, object>(RubyMarshal.Dump), new Func<RubyMarshal.WriterSites, RespondToStorage, RubyModule, object, object, int?, object>(RubyMarshal.Dump));
			LibraryInitializer.DefineLibraryMethod(module, "load", 97, 8u, 8u, 0u, new Func<RubyMarshal.ReaderSites, RubyScope, RubyModule, MutableString, Proc, object>(RubyMarshal.Load), new Func<RubyMarshal.ReaderSites, RubyScope, RubyModule, RubyIO, Proc, object>(RubyMarshal.Load), new Func<RubyMarshal.ReaderSites, RespondToStorage, RubyScope, RubyModule, object, Proc, object>(RubyMarshal.Load));
			LibraryInitializer.DefineLibraryMethod(module, "restore", 97, 8u, 8u, 0u, new Func<RubyMarshal.ReaderSites, RubyScope, RubyModule, MutableString, Proc, object>(RubyMarshal.Load), new Func<RubyMarshal.ReaderSites, RubyScope, RubyModule, RubyIO, Proc, object>(RubyMarshal.Load), new Func<RubyMarshal.ReaderSites, RespondToStorage, RubyScope, RubyModule, object, Proc, object>(RubyMarshal.Load));
		}

		private static void LoadMatchData_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "[]", 81, 65536u, 196608u, 4u, new Func<MatchData, int, MutableString>(MatchDataOps.GetGroup), new Func<MatchData, int, int, RubyArray>(MatchDataOps.GetGroup), new Func<ConversionStorage<int>, MatchData, Range, RubyArray>(MatchDataOps.GetGroup));
			LibraryInitializer.DefineLibraryMethod(module, "begin", 81, 65536u, new Func<MatchData, int, object>(MatchDataOps.Begin));
			LibraryInitializer.DefineLibraryMethod(module, "captures", 81, 0u, new Func<MatchData, RubyArray>(MatchDataOps.Captures));
			LibraryInitializer.DefineLibraryMethod(module, "end", 81, 65536u, new Func<MatchData, int, object>(MatchDataOps.End));
			LibraryInitializer.DefineLibraryMethod(module, "initialize_copy", 82, 2u, new Func<MatchData, MatchData, MatchData>(MatchDataOps.InitializeCopy));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<RubyContext, MatchData, MutableString>(MatchDataOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "length", 81, 0u, new Func<MatchData, int>(MatchDataOps.Length));
			LibraryInitializer.DefineLibraryMethod(module, "offset", 81, 65536u, new Func<MatchData, int, RubyArray>(MatchDataOps.Offset));
			LibraryInitializer.DefineLibraryMethod(module, "post_match", 81, 0u, new Func<MatchData, MutableString>(MatchDataOps.PostMatch));
			LibraryInitializer.DefineLibraryMethod(module, "pre_match", 81, 0u, new Func<MatchData, MutableString>(MatchDataOps.PreMatch));
			LibraryInitializer.DefineLibraryMethod(module, "select", 81, 1u, new Func<BlockParam, MatchData, object>(MatchDataOps.Select));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<MatchData, int>(MatchDataOps.Length));
			LibraryInitializer.DefineLibraryMethod(module, "string", 81, 0u, new Func<RubyContext, MatchData, MutableString>(MatchDataOps.ReturnFrozenString));
			LibraryInitializer.DefineLibraryMethod(module, "to_a", 81, 0u, new Func<MatchData, RubyArray>(MatchDataOps.ToArray));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<MatchData, MutableString>(MatchDataOps.ToS));
			LibraryInitializer.DefineLibraryMethod(module, "values_at", 81, 2147614720u, new Func<ConversionStorage<int>, MatchData, int[], RubyArray>(MatchDataOps.ValuesAt));
		}

		private static void LoadMatchData_Class(RubyModule module)
		{
			module.UndefineMethodNoEvent("new");
		}

		private static void LoadMath_Constants(RubyModule module)
		{
			LibraryInitializer.SetBuiltinConstant(module, "E", Math.E);
			LibraryInitializer.SetBuiltinConstant(module, "PI", Math.PI);
		}

		private static void LoadMath_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "acos", 82, 65536u, new Func<object, double, double>(RubyMath.Acos));
			LibraryInitializer.DefineLibraryMethod(module, "acosh", 82, 65536u, new Func<object, double, double>(RubyMath.Acosh));
			LibraryInitializer.DefineLibraryMethod(module, "asin", 82, 65536u, new Func<object, double, double>(RubyMath.Asin));
			LibraryInitializer.DefineLibraryMethod(module, "asinh", 82, 65536u, new Func<object, double, double>(RubyMath.Asinh));
			LibraryInitializer.DefineLibraryMethod(module, "atan", 82, 65536u, new Func<object, double, double>(RubyMath.Atan));
			LibraryInitializer.DefineLibraryMethod(module, "atan2", 82, 196608u, new Func<object, double, double, double>(RubyMath.Atan2));
			LibraryInitializer.DefineLibraryMethod(module, "atanh", 82, 65536u, new Func<object, double, double>(RubyMath.Atanh));
			LibraryInitializer.DefineLibraryMethod(module, "cbrt", 82, 65536u, new Func<object, double, double>(RubyMath.CubeRoot));
			LibraryInitializer.DefineLibraryMethod(module, "cos", 82, 65536u, new Func<object, double, double>(RubyMath.Cos));
			LibraryInitializer.DefineLibraryMethod(module, "cosh", 82, 65536u, new Func<object, double, double>(RubyMath.Cosh));
			LibraryInitializer.DefineLibraryMethod(module, "erf", 82, 65536u, new Func<object, double, double>(RubyMath.Erf));
			LibraryInitializer.DefineLibraryMethod(module, "erfc", 82, 65536u, new Func<object, double, double>(RubyMath.Erfc));
			LibraryInitializer.DefineLibraryMethod(module, "exp", 82, 65536u, new Func<object, double, double>(RubyMath.Exp));
			LibraryInitializer.DefineLibraryMethod(module, "frexp", 82, 65536u, new Func<object, double, RubyArray>(RubyMath.Frexp));
			LibraryInitializer.DefineLibraryMethod(module, "gamma", 82, 65536u, new Func<object, double, double>(RubyMath.Gamma));
			LibraryInitializer.DefineLibraryMethod(module, "hypot", 82, 196608u, new Func<object, double, double, double>(RubyMath.Hypot));
			LibraryInitializer.DefineLibraryMethod(module, "ldexp", 82, 196608u, new Func<object, double, IntegerValue, double>(RubyMath.Ldexp));
			LibraryInitializer.DefineLibraryMethod(module, "lgamma", 82, 65536u, new Func<object, double, double>(RubyMath.LogGamma));
			LibraryInitializer.DefineLibraryMethod(module, "log", 82, 65536u, new Func<object, double, double>(RubyMath.Log));
			LibraryInitializer.DefineLibraryMethod(module, "log10", 82, 65536u, new Func<object, double, double>(RubyMath.Log10));
			LibraryInitializer.DefineLibraryMethod(module, "log2", 82, 65536u, new Func<object, double, double>(RubyMath.Log2));
			LibraryInitializer.DefineLibraryMethod(module, "sin", 82, 65536u, new Func<object, double, double>(RubyMath.Sin));
			LibraryInitializer.DefineLibraryMethod(module, "sinh", 82, 65536u, new Func<object, double, double>(RubyMath.Sinh));
			LibraryInitializer.DefineLibraryMethod(module, "sqrt", 82, 65536u, new Func<object, double, double>(RubyMath.Sqrt));
			LibraryInitializer.DefineLibraryMethod(module, "tan", 82, 65536u, new Func<object, double, double>(RubyMath.Tan));
			LibraryInitializer.DefineLibraryMethod(module, "tanh", 82, 65536u, new Func<object, double, double>(RubyMath.Tanh));
		}

		private static void LoadMath_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "acos", 97, 65536u, new Func<object, double, double>(RubyMath.Acos));
			LibraryInitializer.DefineLibraryMethod(module, "acosh", 97, 65536u, new Func<object, double, double>(RubyMath.Acosh));
			LibraryInitializer.DefineLibraryMethod(module, "asin", 97, 65536u, new Func<object, double, double>(RubyMath.Asin));
			LibraryInitializer.DefineLibraryMethod(module, "asinh", 97, 65536u, new Func<object, double, double>(RubyMath.Asinh));
			LibraryInitializer.DefineLibraryMethod(module, "atan", 97, 65536u, new Func<object, double, double>(RubyMath.Atan));
			LibraryInitializer.DefineLibraryMethod(module, "atan2", 97, 196608u, new Func<object, double, double, double>(RubyMath.Atan2));
			LibraryInitializer.DefineLibraryMethod(module, "atanh", 97, 65536u, new Func<object, double, double>(RubyMath.Atanh));
			LibraryInitializer.DefineLibraryMethod(module, "cbrt", 97, 65536u, new Func<object, double, double>(RubyMath.CubeRoot));
			LibraryInitializer.DefineLibraryMethod(module, "cos", 97, 65536u, new Func<object, double, double>(RubyMath.Cos));
			LibraryInitializer.DefineLibraryMethod(module, "cosh", 97, 65536u, new Func<object, double, double>(RubyMath.Cosh));
			LibraryInitializer.DefineLibraryMethod(module, "erf", 97, 65536u, new Func<object, double, double>(RubyMath.Erf));
			LibraryInitializer.DefineLibraryMethod(module, "erfc", 97, 65536u, new Func<object, double, double>(RubyMath.Erfc));
			LibraryInitializer.DefineLibraryMethod(module, "exp", 97, 65536u, new Func<object, double, double>(RubyMath.Exp));
			LibraryInitializer.DefineLibraryMethod(module, "frexp", 97, 65536u, new Func<object, double, RubyArray>(RubyMath.Frexp));
			LibraryInitializer.DefineLibraryMethod(module, "gamma", 97, 65536u, new Func<object, double, double>(RubyMath.Gamma));
			LibraryInitializer.DefineLibraryMethod(module, "hypot", 97, 196608u, new Func<object, double, double, double>(RubyMath.Hypot));
			LibraryInitializer.DefineLibraryMethod(module, "ldexp", 97, 196608u, new Func<object, double, IntegerValue, double>(RubyMath.Ldexp));
			LibraryInitializer.DefineLibraryMethod(module, "lgamma", 97, 65536u, new Func<object, double, double>(RubyMath.LogGamma));
			LibraryInitializer.DefineLibraryMethod(module, "log", 97, 65536u, new Func<object, double, double>(RubyMath.Log));
			LibraryInitializer.DefineLibraryMethod(module, "log10", 97, 65536u, new Func<object, double, double>(RubyMath.Log10));
			LibraryInitializer.DefineLibraryMethod(module, "log2", 97, 65536u, new Func<object, double, double>(RubyMath.Log2));
			LibraryInitializer.DefineLibraryMethod(module, "sin", 97, 65536u, new Func<object, double, double>(RubyMath.Sin));
			LibraryInitializer.DefineLibraryMethod(module, "sinh", 97, 65536u, new Func<object, double, double>(RubyMath.Sinh));
			LibraryInitializer.DefineLibraryMethod(module, "sqrt", 97, 65536u, new Func<object, double, double>(RubyMath.Sqrt));
			LibraryInitializer.DefineLibraryMethod(module, "tan", 97, 65536u, new Func<object, double, double>(RubyMath.Tan));
			LibraryInitializer.DefineLibraryMethod(module, "tanh", 97, 65536u, new Func<object, double, double>(RubyMath.Tanh));
		}

		private static void LoadMethod_Instance(RubyModule module)
		{
			LibraryInitializer.DefineRuleGenerator(module, "[]", 81, MethodOps.Call());
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 2u, 0u, new Func<RubyMethod, RubyMethod, bool>(MethodOps.Equal), new Func<RubyMethod, object, bool>(MethodOps.Equal));
			LibraryInitializer.DefineLibraryMethod(module, "arity", 81, 0u, new Func<RubyMethod, int>(MethodOps.GetArity));
			LibraryInitializer.DefineRuleGenerator(module, "call", 81, MethodOps.Call());
			LibraryInitializer.DefineLibraryMethod(module, "clone", 81, 0u, new Func<RubyMethod, RubyMethod>(MethodOps.Clone));
			LibraryInitializer.DefineLibraryMethod(module, "clr_members", 81, 0u, new Func<RubyMethod, RubyArray>(MethodOps.GetClrMembers));
			LibraryInitializer.DefineLibraryMethod(module, "of", 81, 2147483652u, new Func<RubyContext, RubyMethod, object[], RubyMethod>(MethodOps.BindGenericParameters));
			LibraryInitializer.DefineLibraryMethod(module, "overload", 81, 2147483652u, new Func<RubyContext, RubyMethod, object[], RubyMethod>(MethodOps.SelectOverload));
			LibraryInitializer.DefineLibraryMethod(module, "overloads", 81, 2147483652u, new Func<RubyContext, RubyMethod, object[], RubyMethod>(MethodOps.SelectOverload_old));
			LibraryInitializer.DefineLibraryMethod(module, "parameters", 81, 0u, new Func<RubyMethod, RubyArray>(MethodOps.GetParameters));
			LibraryInitializer.DefineLibraryMethod(module, "source_location", 81, 0u, new Func<RubyMethod, RubyArray>(MethodOps.GetSourceLocation));
			LibraryInitializer.DefineLibraryMethod(module, "to_proc", 81, 0u, new Func<RubyScope, RubyMethod, Proc>(MethodOps.ToProc));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<RubyContext, RubyMethod, MutableString>(MethodOps.ToS));
			LibraryInitializer.DefineLibraryMethod(module, "unbind", 81, 0u, new Func<RubyMethod, UnboundMethod>(MethodOps.Unbind));
		}

		private static void LoadMicrosoft__Scripting__Actions__TypeGroup_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "[]", 81, 2147483652u, 0u, new Func<RubyContext, TypeGroup, object[], RubyModule>(TypeGroupOps.Of), new Func<RubyContext, TypeGroup, int, RubyModule>(TypeGroupOps.Of));
			LibraryInitializer.DefineLibraryMethod(module, "clr_constructor", 81, 0u, new Func<RubyContext, TypeGroup, RubyMethod>(TypeGroupOps.GetClrConstructor));
			LibraryInitializer.DefineLibraryMethod(module, "clr_ctor", 81, 0u, new Func<RubyContext, TypeGroup, RubyMethod>(TypeGroupOps.GetClrConstructor));
			LibraryInitializer.DefineLibraryMethod(module, "clr_new", 81, 2147483648u, 2147483648u, new Func<CallSiteStorage<Func<CallSite, object, object, object>>, TypeGroup, object[], object>(TypeGroupOps.ClrNew), new Func<CallSiteStorage<Func<CallSite, object, object, object, object>>, BlockParam, TypeGroup, object[], object>(TypeGroupOps.ClrNew));
			LibraryInitializer.DefineLibraryMethod(module, "each", 81, 0u, new Func<RubyContext, BlockParam, TypeGroup, object>(TypeGroupOps.EachType));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<RubyContext, TypeGroup, MutableString>(TypeGroupOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "name", 81, 0u, new Func<RubyContext, TypeGroup, MutableString>(TypeGroupOps.GetName));
			LibraryInitializer.DefineLibraryMethod(module, "new", 81, 2147483648u, 2147483648u, new Func<CallSiteStorage<Func<CallSite, object, object, object>>, TypeGroup, object[], object>(TypeGroupOps.New), new Func<CallSiteStorage<Func<CallSite, object, object, object, object>>, BlockParam, TypeGroup, object[], object>(TypeGroupOps.New));
			LibraryInitializer.DefineLibraryMethod(module, "of", 81, 2147483652u, 0u, new Func<RubyContext, TypeGroup, object[], RubyModule>(TypeGroupOps.Of), new Func<RubyContext, TypeGroup, int, RubyModule>(TypeGroupOps.Of));
			LibraryInitializer.DefineLibraryMethod(module, "superclass", 81, 0u, new Func<RubyContext, TypeGroup, RubyClass>(TypeGroupOps.GetSuperclass));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<RubyContext, TypeGroup, MutableString>(TypeGroupOps.GetName));
		}

		private static void LoadMicrosoft__Scripting__Actions__TypeTracker_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "to_class", 81, 0u, new Func<RubyContext, TypeTracker, RubyClass>(TypeTrackerOps.ToClass));
			LibraryInitializer.DefineLibraryMethod(module, "to_module", 81, 0u, new Func<RubyContext, TypeTracker, RubyModule>(TypeTrackerOps.ToModule));
		}

		private static void LoadModule_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "[]", 81, 2147483650u, 0u, new Func<RubyModule, object[], RubyModule>(ModuleOps.Of), new Func<RubyModule, int, RubyModule>(ModuleOps.Of));
			LibraryInitializer.DefineLibraryMethod(module, "<", 81, 2u, 0u, new Func<RubyModule, RubyModule, object>(ModuleOps.IsSubclassOrIncluded), new Func<RubyModule, object, object>(ModuleOps.InvalidComparison));
			LibraryInitializer.DefineLibraryMethod(module, "<=", 81, 2u, 0u, new Func<RubyModule, RubyModule, object>(ModuleOps.IsSubclassSameOrIncluded), new Func<RubyModule, object, object>(ModuleOps.InvalidComparison));
			LibraryInitializer.DefineLibraryMethod(module, "<=>", 81, 2u, 0u, new Func<RubyModule, RubyModule, object>(ModuleOps.Comparison), new Func<RubyModule, object, object>(ModuleOps.Comparison));
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 0u, new Func<RubyModule, object, bool>(ModuleOps.Equals));
			LibraryInitializer.DefineLibraryMethod(module, "===", 81, 0u, new Func<RubyModule, object, bool>(ModuleOps.CaseEquals));
			LibraryInitializer.DefineLibraryMethod(module, ">", 81, 2u, 0u, new Func<RubyModule, RubyModule, object>(ModuleOps.IsNotSubclassOrIncluded), new Func<RubyModule, object, object>(ModuleOps.InvalidComparison));
			LibraryInitializer.DefineLibraryMethod(module, ">=", 81, 2u, 0u, new Func<RubyModule, RubyModule, object>(ModuleOps.IsNotSubclassSameOrIncluded), new Func<RubyModule, object, object>(ModuleOps.InvalidComparison));
			LibraryInitializer.DefineLibraryMethod(module, "alias_method", 82, 393228u, new Func<RubyContext, RubyModule, string, string, RubyModule>(ModuleOps.AliasMethod));
			LibraryInitializer.DefineLibraryMethod(module, "ancestors", 81, 0u, new Func<RubyModule, RubyArray>(ModuleOps.Ancestors));
			LibraryInitializer.DefineLibraryMethod(module, "append_features", 82, 2u, new Func<RubyModule, RubyModule, RubyModule>(ModuleOps.AppendFeatures));
			LibraryInitializer.DefineLibraryMethod(module, "attr", 82, 131076u, 2147614724u, new Action<RubyScope, RubyModule, string, bool>(ModuleOps.Attr), new Action<RubyScope, RubyModule, string[]>(ModuleOps.Attr));
			LibraryInitializer.DefineLibraryMethod(module, "attr_accessor", 82, 131076u, 2147614724u, new Action<RubyScope, RubyModule, string>(ModuleOps.AttrAccessor), new Action<RubyScope, RubyModule, string[]>(ModuleOps.AttrAccessor));
			LibraryInitializer.DefineLibraryMethod(module, "attr_reader", 82, 131076u, 2147614724u, new Action<RubyScope, RubyModule, string>(ModuleOps.AttrReader), new Action<RubyScope, RubyModule, string[]>(ModuleOps.AttrReader));
			LibraryInitializer.DefineLibraryMethod(module, "attr_writer", 82, 131076u, 2147614724u, new Action<RubyScope, RubyModule, string>(ModuleOps.AttrWriter), new Action<RubyScope, RubyModule, string[]>(ModuleOps.AttrWriter));
			LibraryInitializer.DefineLibraryMethod(module, "autoload", 81, 196614u, new Action<RubyModule, string, MutableString>(ModuleOps.SetAutoloadedConstant));
			LibraryInitializer.DefineLibraryMethod(module, "autoload?", 81, 65538u, new Func<RubyModule, string, MutableString>(ModuleOps.GetAutoloadedConstantPath));
			LibraryInitializer.DefineLibraryMethod(module, "class_eval", 81, 262168u, 1u, new Func<RubyScope, BlockParam, RubyModule, MutableString, MutableString, int, object>(ModuleOps.Evaluate), new Func<BlockParam, RubyModule, object>(ModuleOps.Evaluate));
			LibraryInitializer.DefineLibraryMethod(module, "class_exec", 81, 2147483649u, new Func<BlockParam, RubyModule, object[], object>(ModuleOps.Execute));
			LibraryInitializer.DefineLibraryMethod(module, "class_variable_defined?", 81, 65538u, new Func<RubyModule, string, bool>(ModuleOps.IsClassVariableDefined));
			LibraryInitializer.DefineLibraryMethod(module, "class_variable_get", 82, 65538u, new Func<RubyModule, string, object>(ModuleOps.GetClassVariable));
			LibraryInitializer.DefineLibraryMethod(module, "class_variable_set", 82, 65538u, new Func<RubyModule, string, object, object>(ModuleOps.ClassVariableSet));
			LibraryInitializer.DefineLibraryMethod(module, "class_variables", 81, 0u, new Func<RubyModule, RubyArray>(ModuleOps.ClassVariables));
			LibraryInitializer.DefineLibraryMethod(module, "const_defined?", 81, 65538u, new Func<RubyModule, string, bool>(ModuleOps.IsConstantDefined));
			LibraryInitializer.DefineLibraryMethod(module, "const_get", 81, 131076u, new Func<RubyScope, RubyModule, string, object>(ModuleOps.GetConstantValue));
			LibraryInitializer.DefineLibraryMethod(module, "const_missing", 81, 65538u, new Func<RubyModule, string, object>(ModuleOps.ConstantMissing));
			LibraryInitializer.DefineLibraryMethod(module, "const_set", 81, 65538u, new Func<RubyModule, string, object, object>(ModuleOps.SetConstantValue));
			LibraryInitializer.DefineLibraryMethod(module, "constants", 81, 0u, new Func<RubyModule, bool, RubyArray>(ModuleOps.GetDefinedConstants));
			LibraryInitializer.DefineLibraryMethod(module, "define_method", 82, new uint[8] { 131084u, 12u, 131084u, 12u, 262154u, 10u, 131084u, 12u }, new Func<RubyScope, RubyModule, string, RubyMethod, RubyMethod>(ModuleOps.DefineMethod), new Func<RubyScope, RubyModule, ClrName, RubyMethod, RubyMethod>(ModuleOps.DefineMethod), new Func<RubyScope, RubyModule, string, UnboundMethod, UnboundMethod>(ModuleOps.DefineMethod), new Func<RubyScope, RubyModule, ClrName, UnboundMethod, UnboundMethod>(ModuleOps.DefineMethod), new Func<RubyScope, BlockParam, RubyModule, string, Proc>(ModuleOps.DefineMethod), new Func<RubyScope, BlockParam, RubyModule, ClrName, Proc>(ModuleOps.DefineMethod), new Func<RubyScope, RubyModule, string, Proc, Proc>(ModuleOps.DefineMethod), new Func<RubyScope, RubyModule, ClrName, Proc, Proc>(ModuleOps.DefineMethod));
			LibraryInitializer.DefineLibraryMethod(module, "extend_object", 82, 0u, new Func<RubyModule, object, object>(ModuleOps.ExtendObject));
			LibraryInitializer.DefineLibraryMethod(module, "extended", 82, 0u, new Action<RubyModule, object>(ModuleOps.ObjectExtended));
			LibraryInitializer.DefineLibraryMethod(module, "freeze", 81, 0u, new Func<RubyContext, RubyModule, RubyModule>(ModuleOps.Freeze));
			LibraryInitializer.DefineLibraryMethod(module, "include", 82, 2147483656u, new Func<CallSiteStorage<Func<CallSite, RubyModule, RubyModule, object>>, CallSiteStorage<Func<CallSite, RubyModule, RubyModule, object>>, RubyModule, RubyModule[], RubyModule>(ModuleOps.Include));
			LibraryInitializer.DefineLibraryMethod(module, "include?", 81, 2u, new Func<RubyModule, RubyModule, bool>(ModuleOps.IncludesModule));
			LibraryInitializer.DefineLibraryMethod(module, "included", 82, 0u, new Action<RubyModule, RubyModule>(ModuleOps.Included));
			LibraryInitializer.DefineLibraryMethod(module, "included_modules", 81, 0u, new Func<RubyModule, RubyArray>(ModuleOps.GetIncludedModules));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 82, 0u, new Func<BlockParam, RubyModule, object>(ModuleOps.Reinitialize));
			LibraryInitializer.DefineLibraryMethod(module, "initialize_copy", 82, 0u, new Func<RubyModule, object, RubyModule>(ModuleOps.InitializeCopy));
			LibraryInitializer.DefineLibraryMethod(module, "instance_method", 81, 65538u, new Func<RubyModule, string, UnboundMethod>(ModuleOps.GetInstanceMethod));
			LibraryInitializer.DefineLibraryMethod(module, "instance_methods", 81, 0u, 0u, new Func<RubyModule, RubyArray>(ModuleOps.GetInstanceMethods), new Func<RubyModule, bool, RubyArray>(ModuleOps.GetInstanceMethods));
			LibraryInitializer.DefineLibraryMethod(module, "method_added", 90, 0u, new Action<object, object>(ModuleOps.MethodAdded));
			LibraryInitializer.DefineLibraryMethod(module, "method_defined?", 81, 65538u, new Func<RubyModule, string, bool>(ModuleOps.MethodDefined));
			LibraryInitializer.DefineLibraryMethod(module, "method_removed", 90, 0u, new Action<object, object>(ModuleOps.MethodRemoved));
			LibraryInitializer.DefineLibraryMethod(module, "method_undefined", 90, 0u, new Action<object, object>(ModuleOps.MethodUndefined));
			LibraryInitializer.DefineLibraryMethod(module, "module_eval", 81, 262168u, 1u, new Func<RubyScope, BlockParam, RubyModule, MutableString, MutableString, int, object>(ModuleOps.Evaluate), new Func<BlockParam, RubyModule, object>(ModuleOps.Evaluate));
			LibraryInitializer.DefineLibraryMethod(module, "module_exec", 81, 2147483649u, new Func<BlockParam, RubyModule, object[], object>(ModuleOps.Execute));
			LibraryInitializer.DefineLibraryMethod(module, "module_function", 82, 2147614724u, new Func<RubyScope, RubyModule, string[], RubyModule>(ModuleOps.CopyMethodsToModuleSingleton));
			LibraryInitializer.DefineLibraryMethod(module, "name", 81, 0u, new Func<RubyContext, RubyModule, MutableString>(ModuleOps.GetName));
			LibraryInitializer.DefineLibraryMethod(module, "of", 81, 2147483650u, 0u, new Func<RubyModule, object[], RubyModule>(ModuleOps.Of), new Func<RubyModule, int, RubyModule>(ModuleOps.Of));
			LibraryInitializer.DefineLibraryMethod(module, "private", 82, 2147614724u, new Func<RubyScope, RubyModule, string[], RubyModule>(ModuleOps.SetPrivateVisibility));
			LibraryInitializer.DefineLibraryMethod(module, "private_class_method", 81, 2147549186u, new Func<RubyModule, string[], RubyModule>(ModuleOps.MakeClassMethodsPrivate));
			LibraryInitializer.DefineLibraryMethod(module, "private_instance_methods", 81, 0u, 0u, new Func<RubyModule, RubyArray>(ModuleOps.GetPrivateInstanceMethods), new Func<RubyModule, bool, RubyArray>(ModuleOps.GetPrivateInstanceMethods));
			LibraryInitializer.DefineLibraryMethod(module, "private_method_defined?", 81, 65538u, new Func<RubyModule, string, bool>(ModuleOps.PrivateMethodDefined));
			LibraryInitializer.DefineLibraryMethod(module, "protected", 82, 2147614724u, new Func<RubyScope, RubyModule, string[], RubyModule>(ModuleOps.SetProtectedVisibility));
			LibraryInitializer.DefineLibraryMethod(module, "protected_instance_methods", 81, 0u, 0u, new Func<RubyModule, RubyArray>(ModuleOps.GetProtectedInstanceMethods), new Func<RubyModule, bool, RubyArray>(ModuleOps.GetProtectedInstanceMethods));
			LibraryInitializer.DefineLibraryMethod(module, "protected_method_defined?", 81, 65538u, new Func<RubyModule, string, bool>(ModuleOps.ProtectedMethodDefined));
			LibraryInitializer.DefineLibraryMethod(module, "public", 82, 2147614724u, new Func<RubyScope, RubyModule, string[], RubyModule>(ModuleOps.SetPublicVisibility));
			LibraryInitializer.DefineLibraryMethod(module, "public_class_method", 81, 2147549186u, new Func<RubyModule, string[], RubyModule>(ModuleOps.MakeClassMethodsPublic));
			LibraryInitializer.DefineLibraryMethod(module, "public_instance_methods", 81, 0u, 0u, new Func<RubyModule, RubyArray>(ModuleOps.GetPublicInstanceMethods), new Func<RubyModule, bool, RubyArray>(ModuleOps.GetPublicInstanceMethods));
			LibraryInitializer.DefineLibraryMethod(module, "public_method_defined?", 81, 65538u, new Func<RubyModule, string, bool>(ModuleOps.PublicMethodDefined));
			LibraryInitializer.DefineLibraryMethod(module, "remove_class_variable", 82, 65538u, new Func<RubyModule, string, object>(ModuleOps.RemoveClassVariable));
			LibraryInitializer.DefineLibraryMethod(module, "remove_const", 82, 65538u, new Func<RubyModule, string, object>(ModuleOps.RemoveConstant));
			LibraryInitializer.DefineLibraryMethod(module, "remove_method", 82, 2147549186u, new Func<RubyModule, string[], RubyModule>(ModuleOps.RemoveMethod));
			LibraryInitializer.DefineLibraryMethod(module, "to_clr_ref", 81, 0u, new Func<RubyModule, RubyModule>(ModuleOps.ToClrRef));
			LibraryInitializer.DefineLibraryMethod(module, "to_clr_type", 81, 0u, new Func<RubyModule, Type>(ModuleOps.ToClrType));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<RubyContext, RubyModule, MutableString>(ModuleOps.ToS));
			LibraryInitializer.DefineLibraryMethod(module, "undef_method", 82, 2147549186u, new Func<RubyModule, string[], RubyModule>(ModuleOps.UndefineMethod));
		}

		private static void LoadModule_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "constants", 97, 0u, new Func<RubyModule, bool, RubyArray>(ModuleOps.GetGlobalConstants));
			LibraryInitializer.DefineLibraryMethod(module, "nesting", 97, 0u, new Func<RubyScope, RubyModule, RubyArray>(ModuleOps.GetLexicalModuleNesting));
		}

		private static void LoadNilClass_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "&", 81, 0u, new Func<object, object, bool>(NilClassOps.And));
			LibraryInitializer.DefineLibraryMethod(module, "^", 81, 0u, 0u, new Func<object, object, bool>(NilClassOps.Xor), new Func<object, bool, bool>(NilClassOps.Xor));
			LibraryInitializer.DefineLibraryMethod(module, "|", 81, 0u, 0u, new Func<object, object, bool>(NilClassOps.Or), new Func<object, bool, bool>(NilClassOps.Or));
			LibraryInitializer.DefineLibraryMethod(module, "GetHashCode", 81, 0u, new Func<object, int>(NilClassOps.GetClrHashCode));
			LibraryInitializer.DefineLibraryMethod(module, "GetType", 81, 0u, new Func<object, Type>(NilClassOps.GetClrType));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<object, MutableString>(NilClassOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "nil?", 81, 0u, new Func<object, bool>(NilClassOps.IsNil));
			LibraryInitializer.DefineLibraryMethod(module, "to_a", 81, 0u, new Func<object, RubyArray>(NilClassOps.ToArray));
			LibraryInitializer.DefineLibraryMethod(module, "to_f", 81, 0u, new Func<object, double>(NilClassOps.ToDouble));
			LibraryInitializer.DefineLibraryMethod(module, "to_i", 81, 0u, new Func<object, int>(NilClassOps.ToInteger));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<object, MutableString>(NilClassOps.ToString));
			LibraryInitializer.DefineLibraryMethod(module, "ToString", 81, 0u, new Func<object, string>(NilClassOps.ToClrString));
		}

		private static void LoadNoMethodError_Instance(RubyModule module)
		{
			module.HideMethod("message");
			LibraryInitializer.DefineLibraryMethod(module, "args", 81, 0u, new Func<MissingMethodException, object>(NoMethodErrorOps.GetArguments));
		}

		private static void LoadNumeric_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "-@", 81, 0u, new Func<BinaryOpStorage, BinaryOpStorage, object, object>(Numeric.UnaryMinus));
			LibraryInitializer.DefineLibraryMethod(module, "+@", 81, 0u, new Func<object, object>(Numeric.UnaryPlus));
			LibraryInitializer.DefineLibraryMethod(module, "<=>", 81, 0u, new Func<object, object, object>(Numeric.Compare));
			LibraryInitializer.DefineLibraryMethod(module, "abs", 81, 0u, new Func<BinaryOpStorage, UnaryOpStorage, object, object>(Numeric.Abs));
			LibraryInitializer.DefineLibraryMethod(module, "ceil", 81, 32768u, new Func<double, object>(Numeric.Ceil));
			LibraryInitializer.DefineLibraryMethod(module, "coerce", 81, 0u, 0u, 0u, new Func<int, int, RubyArray>(Numeric.Coerce), new Func<double, double, RubyArray>(Numeric.Coerce), new Func<ConversionStorage<double>, ConversionStorage<double>, object, object, RubyArray>(Numeric.Coerce));
			LibraryInitializer.DefineLibraryMethod(module, "div", 81, 0u, new Func<BinaryOpStorage, ConversionStorage<double>, object, object, object>(Numeric.Div));
			LibraryInitializer.DefineLibraryMethod(module, "divmod", 81, 0u, new Func<BinaryOpStorage, BinaryOpStorage, ConversionStorage<double>, object, object, RubyArray>(Numeric.DivMod));
			LibraryInitializer.DefineLibraryMethod(module, "eql?", 81, 0u, new Func<BinaryOpStorage, object, object, bool>(Numeric.Eql));
			LibraryInitializer.DefineLibraryMethod(module, "floor", 81, 32768u, new Func<double, object>(Numeric.Floor));
			LibraryInitializer.DefineLibraryMethod(module, "integer?", 81, 0u, new Func<object, bool>(Numeric.IsInteger));
			LibraryInitializer.DefineLibraryMethod(module, "modulo", 81, 0u, new Func<BinaryOpStorage, object, object, object>(Numeric.Modulo));
			LibraryInitializer.DefineLibraryMethod(module, "nonzero?", 81, 0u, new Func<UnaryOpStorage, object, object>(Numeric.IsNonZero));
			LibraryInitializer.DefineLibraryMethod(module, "quo", 81, 0u, new Func<BinaryOpStorage, object, object, object>(Numeric.Quo));
			LibraryInitializer.DefineLibraryMethod(module, "remainder", 81, 0u, new Func<BinaryOpStorage, BinaryOpStorage, BinaryOpStorage, BinaryOpStorage, BinaryOpStorage, object, object, object>(Numeric.Remainder));
			LibraryInitializer.DefineLibraryMethod(module, "round", 81, 32768u, new Func<double, object>(Numeric.Round));
			LibraryInitializer.DefineLibraryMethod(module, "step", 81, 0u, 0u, 0u, 0u, new Func<BlockParam, int, int, object>(Numeric.Step), new Func<BlockParam, int, int, int, object>(Numeric.Step), new Func<BlockParam, double, double, double, object>(Numeric.Step), new Func<BinaryOpStorage, BinaryOpStorage, BinaryOpStorage, BinaryOpStorage, ConversionStorage<double>, BlockParam, object, object, object, object>(Numeric.Step));
			LibraryInitializer.DefineLibraryMethod(module, "to_int", 81, 0u, new Func<UnaryOpStorage, object, object>(Numeric.ToInt));
			LibraryInitializer.DefineLibraryMethod(module, "truncate", 81, 32768u, new Func<double, object>(Numeric.Truncate));
			LibraryInitializer.DefineLibraryMethod(module, "zero?", 81, 0u, new Func<BinaryOpStorage, object, bool>(Numeric.IsZero));
		}

		private static void LoadObject_Constants(RubyModule module)
		{
			LibraryInitializer.SetBuiltinConstant(module, "___Numerics__", ObjectOps.Numerics(module));
			LibraryInitializer.SetBuiltinConstant(module, "FALSE", ObjectOps.FALSE);
			LibraryInitializer.SetBuiltinConstant(module, "NIL", ObjectOps.NIL);
			LibraryInitializer.SetBuiltinConstant(module, "TRUE", ObjectOps.TRUE);
		}

		private static void LoadObject_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 90, 2147483648u, new Func<object, object[], object>(ObjectOps.Reinitialize));
		}

		private static void LoadObject_Class(RubyModule module)
		{
		}

		private static void LoadObjectSpace_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "define_finalizer", 97, 0u, new Func<RespondToStorage, BinaryOpStorage, RubyModule, object, object, object>(ObjectSpace.DefineFinalizer));
			LibraryInitializer.DefineLibraryMethod(module, "each_object", 97, 4u, new Func<BlockParam, RubyModule, RubyClass, object>(ObjectSpace.EachObject));
			LibraryInitializer.DefineLibraryMethod(module, "garbage_collect", 97, 0u, new Action<RubyModule>(ObjectSpace.GarbageCollect));
			LibraryInitializer.DefineLibraryMethod(module, "undefine_finalizer", 97, 0u, new Func<RubyContext, RubyModule, object, object>(ObjectSpace.UndefineFinalizer));
		}

		private static void LoadPrecision_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "prec", 81, 4u, new Func<CallSiteStorage<Func<CallSite, RubyClass, object, object>>, object, RubyClass, object>(Precision.Prec));
			LibraryInitializer.DefineLibraryMethod(module, "prec_f", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, RubyClass, object>>, object, object>(Precision.PrecFloat));
			LibraryInitializer.DefineLibraryMethod(module, "prec_i", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, RubyClass, object>>, object, object>(Precision.PrecInteger));
		}

		private static void LoadPrecision_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "included", 97, 0u, new Func<RubyContext, RubyModule, RubyModule, object>(Precision.Included));
		}

		private static void LoadProc_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "[]", 81, new uint[6] { 0u, 0u, 0u, 0u, 0u, 2147483648u }, new Func<BlockParam, Proc, object>(ProcOps.Call), new Func<BlockParam, Proc, object, object>(ProcOps.Call), new Func<BlockParam, Proc, object, object, object>(ProcOps.Call), new Func<BlockParam, Proc, object, object, object, object>(ProcOps.Call), new Func<BlockParam, Proc, object, object, object, object, object>(ProcOps.Call), new Func<BlockParam, Proc, object[], object>(ProcOps.Call));
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 2u, 0u, new Func<Proc, Proc, bool>(ProcOps.Equal), new Func<Proc, object, bool>(ProcOps.Equal));
			LibraryInitializer.DefineLibraryMethod(module, "===", 81, new uint[6] { 0u, 0u, 0u, 0u, 0u, 2147483648u }, new Func<BlockParam, Proc, object>(ProcOps.Call), new Func<BlockParam, Proc, object, object>(ProcOps.Call), new Func<BlockParam, Proc, object, object, object>(ProcOps.Call), new Func<BlockParam, Proc, object, object, object, object>(ProcOps.Call), new Func<BlockParam, Proc, object, object, object, object, object>(ProcOps.Call), new Func<BlockParam, Proc, object[], object>(ProcOps.Call));
			LibraryInitializer.DefineLibraryMethod(module, "arity", 81, 0u, new Func<Proc, int>(ProcOps.GetArity));
			LibraryInitializer.DefineLibraryMethod(module, "binding", 81, 0u, new Func<Proc, Binding>(ProcOps.GetLocalScope));
			LibraryInitializer.DefineLibraryMethod(module, "call", 81, new uint[6] { 0u, 0u, 0u, 0u, 0u, 2147483648u }, new Func<BlockParam, Proc, object>(ProcOps.Call), new Func<BlockParam, Proc, object, object>(ProcOps.Call), new Func<BlockParam, Proc, object, object, object>(ProcOps.Call), new Func<BlockParam, Proc, object, object, object, object>(ProcOps.Call), new Func<BlockParam, Proc, object, object, object, object, object>(ProcOps.Call), new Func<BlockParam, Proc, object[], object>(ProcOps.Call));
			LibraryInitializer.DefineLibraryMethod(module, "clone", 81, 0u, new Func<Proc, Proc>(ProcOps.Clone));
			LibraryInitializer.DefineLibraryMethod(module, "dup", 81, 0u, new Func<Proc, Proc>(ProcOps.Clone));
			LibraryInitializer.DefineLibraryMethod(module, "eql?", 81, 2u, 0u, new Func<Proc, Proc, bool>(ProcOps.Equal), new Func<Proc, object, bool>(ProcOps.Equal));
			LibraryInitializer.DefineLibraryMethod(module, "hash", 81, 0u, new Func<Proc, int>(ProcOps.GetHash));
			LibraryInitializer.DefineLibraryMethod(module, "lambda?", 81, 0u, new Func<Proc, bool>(ProcOps.IsLambda));
			LibraryInitializer.DefineLibraryMethod(module, "source_location", 81, 0u, new Func<Proc, RubyArray>(ProcOps.GetSourceLocation));
			LibraryInitializer.DefineLibraryMethod(module, "to_proc", 81, 0u, new Func<Proc, Proc>(ProcOps.ToProc));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<Proc, MutableString>(ProcOps.ToS));
			LibraryInitializer.DefineLibraryMethod(module, "yield", 81, new uint[6] { 0u, 0u, 0u, 0u, 0u, 2147483648u }, new Func<BlockParam, Proc, object>(ProcOps.Call), new Func<BlockParam, Proc, object, object>(ProcOps.Call), new Func<BlockParam, Proc, object, object, object>(ProcOps.Call), new Func<BlockParam, Proc, object, object, object, object>(ProcOps.Call), new Func<BlockParam, Proc, object, object, object, object, object>(ProcOps.Call), new Func<BlockParam, Proc, object[], object>(ProcOps.Call));
		}

		private static void LoadProc_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "new", 97, 0u, 0u, new Func<CallSiteStorage<Func<CallSite, object, object>>, RubyScope, RubyClass, Proc>(ProcOps.CreateNew), new Func<CallSiteStorage<Func<CallSite, object, object, object>>, BlockParam, RubyClass, object>(ProcOps.CreateNew));
		}

		private static void LoadProcess_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "kill", 82, 0u, new Func<RubyModule, object, object, object>(RubyProcess.Kill));
		}

		private static void LoadProcess_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "euid", 97, 0u, new Func<RubyModule, int>(RubyProcess.EffectiveUserId));
			LibraryInitializer.DefineLibraryMethod(module, "kill", 97, 0u, new Func<RubyModule, object, object, object>(RubyProcess.Kill));
			LibraryInitializer.DefineLibraryMethod(module, "pid", 97, 0u, new Func<RubyModule, int>(RubyProcess.GetPid));
			LibraryInitializer.DefineLibraryMethod(module, "ppid", 97, 0u, new Func<RubyModule, int>(RubyProcess.GetParentPid));
			LibraryInitializer.DefineLibraryMethod(module, "times", 97, 0u, new Func<RubyModule, RubyStruct>(RubyProcess.GetTimes));
			LibraryInitializer.DefineLibraryMethod(module, "uid", 97, 0u, new Func<RubyModule, int>(RubyProcess.UserId));
			LibraryInitializer.DefineLibraryMethod(module, "uid=", 97, 0u, new Action<RubyModule, object>(RubyProcess.SetUserId));
			LibraryInitializer.DefineLibraryMethod(module, "wait", 97, 0u, new Action<RubyModule>(RubyProcess.Wait));
			LibraryInitializer.DefineLibraryMethod(module, "wait2", 97, 0u, new Action<RubyModule>(RubyProcess.Wait2));
			LibraryInitializer.DefineLibraryMethod(module, "waitall", 97, 0u, new Func<RubyModule, RubyArray>(RubyProcess.Waitall));
		}

		private static void LoadProcess__Status_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "coredump?", 81, 0u, new Func<RubyProcess.Status, bool>(RubyProcess.Status.CoreDump));
			LibraryInitializer.DefineLibraryMethod(module, "exited?", 81, 0u, new Func<RubyProcess.Status, bool>(RubyProcess.Status.Exited));
			LibraryInitializer.DefineLibraryMethod(module, "exitstatus", 81, 0u, new Func<RubyProcess.Status, int>(RubyProcess.Status.ExitStatus));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<RubyProcess.Status, MutableString>(RubyProcess.Status.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "pid", 81, 0u, new Func<RubyProcess.Status, int>(RubyProcess.Status.Pid));
			LibraryInitializer.DefineLibraryMethod(module, "stopped?", 81, 0u, new Func<RubyProcess.Status, bool>(RubyProcess.Status.Stopped));
			LibraryInitializer.DefineLibraryMethod(module, "stopsig", 81, 0u, new Func<RubyProcess.Status, object>(RubyProcess.Status.StopSig));
			LibraryInitializer.DefineLibraryMethod(module, "success?", 81, 0u, new Func<RubyProcess.Status, bool>(RubyProcess.Status.Success));
			LibraryInitializer.DefineLibraryMethod(module, "termsig", 81, 0u, new Func<RubyProcess.Status, object>(RubyProcess.Status.TermSig));
		}

		private static void LoadProcess__Status_Class(RubyModule module)
		{
			module.HideMethod("new");
		}

		private static void LoadRange_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 0u, 4u, new Func<Range, object, bool>(RangeOps.Equals), new Func<BinaryOpStorage, Range, Range, bool>(RangeOps.Equals));
			LibraryInitializer.DefineLibraryMethod(module, "===", 81, 2u, new Func<ComparisonStorage, Range, object, bool>(RangeOps.CaseEquals));
			LibraryInitializer.DefineLibraryMethod(module, "begin", 81, 1u, new Func<Range, object>(RangeOps.Begin));
			LibraryInitializer.DefineLibraryMethod(module, "each", 81, 0u, 2u, new Func<RangeOps.EachStorage, Range, Enumerator>(RangeOps.GetEachEnumerator), new Func<RangeOps.EachStorage, BlockParam, Range, object>(RangeOps.Each));
			LibraryInitializer.DefineLibraryMethod(module, "end", 81, 1u, new Func<Range, object>(RangeOps.End));
			LibraryInitializer.DefineLibraryMethod(module, "eql?", 81, 0u, 4u, new Func<Range, object, bool>(RangeOps.Equals), new Func<BinaryOpStorage, Range, Range, bool>(RangeOps.Eql));
			LibraryInitializer.DefineLibraryMethod(module, "exclude_end?", 81, 1u, new Func<Range, bool>(RangeOps.ExcludeEnd));
			LibraryInitializer.DefineLibraryMethod(module, "first", 81, 1u, new Func<Range, object>(RangeOps.Begin));
			LibraryInitializer.DefineLibraryMethod(module, "hash", 81, 0u, new Func<UnaryOpStorage, Range, int>(RangeOps.GetHashCode));
			LibraryInitializer.DefineLibraryMethod(module, "include?", 81, 2u, new Func<ComparisonStorage, Range, object, bool>(RangeOps.CaseEquals));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 82, 0u, new Func<BinaryOpStorage, RubyContext, Range, object, object, bool, Range>(RangeOps.Reinitialize));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<RubyContext, Range, MutableString>(RangeOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "last", 81, 1u, new Func<Range, object>(RangeOps.End));
			LibraryInitializer.DefineLibraryMethod(module, "member?", 81, 2u, new Func<ComparisonStorage, Range, object, bool>(RangeOps.CaseEquals));
			LibraryInitializer.DefineLibraryMethod(module, "step", 81, 0u, 2u, new Func<RangeOps.StepStorage, Range, object, Enumerator>(RangeOps.GetStepEnumerator), new Func<RangeOps.StepStorage, BlockParam, Range, object, object>(RangeOps.Step));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<ConversionStorage<MutableString>, Range, MutableString>(RangeOps.ToS));
		}

		private static void LoadRangeError_Instance(RubyModule module)
		{
			module.HideMethod("message");
		}

		private static void LoadRegexp_Constants(RubyModule module)
		{
			LibraryInitializer.SetBuiltinConstant(module, "EXTENDED", 2);
			LibraryInitializer.SetBuiltinConstant(module, "IGNORECASE", 1);
			LibraryInitializer.SetBuiltinConstant(module, "MULTILINE", 4);
		}

		private static void LoadRegexp_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "~", 81, 0u, new Func<ConversionStorage<MutableString>, RubyScope, RubyRegex, object>(RegexpOps.ImplicitMatch));
			LibraryInitializer.DefineLibraryMethod(module, "=~", 81, 131072u, new Func<RubyScope, RubyRegex, MutableString, object>(RegexpOps.MatchIndex));
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 0u, 4u, new Func<RubyRegex, object, bool>(RegexpOps.Equals), new Func<RubyContext, RubyRegex, RubyRegex, bool>(RegexpOps.Equals));
			LibraryInitializer.DefineLibraryMethod(module, "===", 81, 0u, new Func<ConversionStorage<MutableString>, RubyScope, RubyRegex, object, bool>(RegexpOps.CaseCompare));
			LibraryInitializer.DefineLibraryMethod(module, "casefold?", 81, 0u, new Func<RubyRegex, bool>(RegexpOps.IsCaseInsensitive));
			LibraryInitializer.DefineLibraryMethod(module, "encoding", 81, 0u, new Func<RubyRegex, RubyEncoding>(RegexpOps.GetEncoding));
			LibraryInitializer.DefineLibraryMethod(module, "eql?", 81, 0u, 4u, new Func<RubyRegex, object, bool>(RegexpOps.Equals), new Func<RubyContext, RubyRegex, RubyRegex, bool>(RegexpOps.Equals));
			LibraryInitializer.DefineLibraryMethod(module, "hash", 81, 0u, new Func<RubyRegex, int>(RegexpOps.GetHash));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 82, new uint[5] { 2u, 4u, 4u, 327682u, 327682u }, new Func<RubyRegex, RubyRegex, RubyRegex>(RegexpOps.Reinitialize), new Func<RubyContext, RubyRegex, RubyRegex, int, object, RubyRegex>(RegexpOps.Reinitialize), new Func<RubyContext, RubyRegex, RubyRegex, object, object, RubyRegex>(RegexpOps.Reinitialize), new Func<RubyRegex, MutableString, int, MutableString, RubyRegex>(RegexpOps.Reinitialize), new Func<RubyRegex, MutableString, bool, MutableString, RubyRegex>(RegexpOps.Reinitialize));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<RubyRegex, MutableString>(RegexpOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "match", 81, 131072u, new Func<RubyScope, RubyRegex, MutableString, MatchData>(RegexpOps.Match));
			LibraryInitializer.DefineLibraryMethod(module, "options", 81, 0u, new Func<RubyRegex, int>(RegexpOps.GetOptions));
			LibraryInitializer.DefineLibraryMethod(module, "source", 81, 0u, new Func<RubyRegex, MutableString>(RegexpOps.Source));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<RubyRegex, MutableString>(RegexpOps.ToS));
		}

		private static void LoadRegexp_Class(RubyModule module)
		{
			LibraryInitializer.DefineRuleGenerator(module, "compile", 97, RegexpOps.Compile());
			LibraryInitializer.DefineLibraryMethod(module, "escape", 97, 65538u, new Func<RubyClass, MutableString, MutableString>(RegexpOps.Escape));
			LibraryInitializer.DefineLibraryMethod(module, "last_match", 97, 0u, 131072u, new Func<RubyScope, RubyClass, MatchData>(RegexpOps.LastMatch), new Func<RubyScope, RubyClass, int, MutableString>(RegexpOps.LastMatch));
			LibraryInitializer.DefineLibraryMethod(module, "quote", 97, 65538u, new Func<RubyClass, MutableString, MutableString>(RegexpOps.Escape));
			LibraryInitializer.DefineLibraryMethod(module, "union", 97, 8u, 4u, 2147483652u, new Func<ConversionStorage<MutableString>, ConversionStorage<IList>, RubyClass, object, RubyRegex>(RegexpOps.Union), new Func<ConversionStorage<MutableString>, RubyClass, IList, RubyRegex>(RegexpOps.Union), new Func<ConversionStorage<MutableString>, RubyClass, object[], RubyRegex>(RegexpOps.Union));
		}

		private static void LoadSignal_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "list", 97, 0u, new Func<RubyContext, RubyModule, Hash>(Signal.List));
			LibraryInitializer.DefineLibraryMethod(module, "trap", 97, 0u, 0u, new Func<RubyContext, object, object, Proc, object>(Signal.Trap), new Func<RubyContext, BlockParam, object, object, object>(Signal.Trap));
		}

		private static void LoadString_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "%", 81, 4u, 0u, new Func<StringFormatterSiteStorage, MutableString, IList, MutableString>(MutableStringOps.Format), new Func<StringFormatterSiteStorage, ConversionStorage<IList>, MutableString, object, MutableString>(MutableStringOps.Format));
			LibraryInitializer.DefineLibraryMethod(module, "*", 81, 65536u, new Func<MutableString, int, MutableString>(MutableStringOps.Repeat));
			LibraryInitializer.DefineLibraryMethod(module, "[]", 81, new uint[6] { 65536u, 196608u, 4u, 2u, 4u, 262148u }, new Func<MutableString, int, MutableString>(MutableStringOps.GetChar), new Func<MutableString, int, int, MutableString>(MutableStringOps.GetSubstring), new Func<ConversionStorage<int>, MutableString, Range, MutableString>(MutableStringOps.GetSubstring), new Func<MutableString, MutableString, MutableString>(MutableStringOps.GetSubstring), new Func<RubyScope, MutableString, RubyRegex, MutableString>(MutableStringOps.GetSubstring), new Func<RubyScope, MutableString, RubyRegex, int, MutableString>(MutableStringOps.GetSubstring));
			LibraryInitializer.DefineLibraryMethod(module, "[]=", 81, new uint[6] { 196612u, 65536u, 458760u, 262156u, 131078u, 786452u }, new Func<MutableString, int, MutableString, MutableString>(MutableStringOps.ReplaceCharacter), new Func<MutableString, int, int, int>(MutableStringOps.SetCharacter), new Func<MutableString, int, int, MutableString, MutableString>(MutableStringOps.ReplaceSubstring), new Func<ConversionStorage<int>, MutableString, Range, MutableString, MutableString>(MutableStringOps.ReplaceSubstring), new Func<MutableString, MutableString, MutableString, MutableString>(MutableStringOps.ReplaceSubstring), new Func<RubyContext, MutableString, RubyRegex, int, MutableString, MutableString>(MutableStringOps.ReplaceSubstring));
			LibraryInitializer.DefineLibraryMethod(module, "+", 81, 65538u, 2u, new Func<MutableString, MutableString, MutableString>(MutableStringOps.Concatenate), new Func<MutableString, RubySymbol, MutableString>(MutableStringOps.Concatenate));
			LibraryInitializer.DefineLibraryMethod(module, "<<", 81, 65538u, 0u, new Func<MutableString, MutableString, MutableString>(MutableStringOps.Append), new Func<MutableString, int, MutableString>(MutableStringOps.Append));
			LibraryInitializer.DefineLibraryMethod(module, "<=>", 81, 2u, 2u, 0u, new Func<MutableString, MutableString, int>(MutableStringOps.Compare), new Func<MutableString, string, int>(MutableStringOps.Compare), new Func<BinaryOpStorage, RespondToStorage, object, object, object>(MutableStringOps.Compare));
			LibraryInitializer.DefineLibraryMethod(module, "=~", 81, 4u, 2u, 0u, new Func<RubyScope, MutableString, RubyRegex, object>(MutableStringOps.Match), new Func<MutableString, MutableString, object>(MutableStringOps.Match), new Func<CallSiteStorage<Func<CallSite, RubyScope, object, object, object>>, RubyScope, MutableString, object, object>(MutableStringOps.Match));
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 2u, 2u, 0u, new Func<MutableString, MutableString, bool>(MutableStringOps.StringEquals), new Func<MutableString, string, bool>(MutableStringOps.StringEquals), new Func<RespondToStorage, BinaryOpStorage, object, object, bool>(MutableStringOps.Equals));
			LibraryInitializer.DefineLibraryMethod(module, "===", 81, 2u, 2u, 0u, new Func<MutableString, MutableString, bool>(MutableStringOps.StringEquals), new Func<MutableString, string, bool>(MutableStringOps.StringEquals), new Func<RespondToStorage, BinaryOpStorage, object, object, bool>(MutableStringOps.Equals));
			LibraryInitializer.DefineLibraryMethod(module, "ascii_only?", 81, 0u, new Func<MutableString, bool>(MutableStringOps.IsAscii));
			LibraryInitializer.DefineLibraryMethod(module, "bytes", 81, 0u, 1u, new Func<MutableString, Enumerator>(MutableStringOps.EachByte), new Func<BlockParam, MutableString, object>(MutableStringOps.EachByte));
			LibraryInitializer.DefineLibraryMethod(module, "bytesize", 81, 0u, new Func<MutableString, int>(MutableStringOps.GetByteCount));
			LibraryInitializer.DefineLibraryMethod(module, "capitalize", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.Capitalize));
			LibraryInitializer.DefineLibraryMethod(module, "capitalize!", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.CapitalizeInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "casecmp", 81, 65538u, new Func<MutableString, MutableString, int>(MutableStringOps.Casecmp));
			LibraryInitializer.DefineLibraryMethod(module, "center", 81, 196608u, new Func<MutableString, int, MutableString, MutableString>(MutableStringOps.Center));
			LibraryInitializer.DefineLibraryMethod(module, "chars", 81, 0u, 1u, new Func<MutableString, Enumerator>(MutableStringOps.EachChar), new Func<BlockParam, MutableString, object>(MutableStringOps.EachChar));
			LibraryInitializer.DefineLibraryMethod(module, "chomp", 81, 0u, 65536u, new Func<RubyContext, MutableString, MutableString>(MutableStringOps.Chomp), new Func<MutableString, MutableString, MutableString>(MutableStringOps.Chomp));
			LibraryInitializer.DefineLibraryMethod(module, "chomp!", 81, 0u, 65536u, new Func<RubyContext, MutableString, MutableString>(MutableStringOps.ChompInPlace), new Func<MutableString, MutableString, MutableString>(MutableStringOps.ChompInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "chop", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.Chop));
			LibraryInitializer.DefineLibraryMethod(module, "chop!", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.ChopInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "chr", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.FirstChar));
			LibraryInitializer.DefineLibraryMethod(module, "clear", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.Clear));
			LibraryInitializer.DefineLibraryMethod(module, "codepoints", 81, 0u, 1u, new Func<MutableString, Enumerator>(MutableStringOps.EachCodePoint), new Func<BlockParam, MutableString, object>(MutableStringOps.EachCodePoint));
			LibraryInitializer.DefineLibraryMethod(module, "concat", 81, 65538u, 0u, new Func<MutableString, MutableString, MutableString>(MutableStringOps.Append), new Func<MutableString, int, MutableString>(MutableStringOps.Append));
			LibraryInitializer.DefineLibraryMethod(module, "count", 81, 2147614724u, new Func<RubyContext, MutableString, MutableString[], object>(MutableStringOps.Count));
			LibraryInitializer.DefineLibraryMethod(module, "delete", 81, 2147549186u, new Func<MutableString, MutableString[], MutableString>(MutableStringOps.Delete));
			LibraryInitializer.DefineLibraryMethod(module, "delete!", 81, 2147549186u, new Func<MutableString, MutableString[], MutableString>(MutableStringOps.DeleteInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "downcase", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.DownCase));
			LibraryInitializer.DefineLibraryMethod(module, "downcase!", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.DownCaseInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "dump", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.Dump));
			LibraryInitializer.DefineLibraryMethod(module, "each_byte", 81, 0u, 1u, new Func<MutableString, Enumerator>(MutableStringOps.EachByte), new Func<BlockParam, MutableString, object>(MutableStringOps.EachByte));
			LibraryInitializer.DefineLibraryMethod(module, "each_char", 81, 0u, 1u, new Func<MutableString, Enumerator>(MutableStringOps.EachChar), new Func<BlockParam, MutableString, object>(MutableStringOps.EachChar));
			LibraryInitializer.DefineLibraryMethod(module, "each_codepoint", 81, 0u, 1u, new Func<MutableString, Enumerator>(MutableStringOps.EachCodePoint), new Func<BlockParam, MutableString, object>(MutableStringOps.EachCodePoint));
			LibraryInitializer.DefineLibraryMethod(module, "each_line", 81, 2u, 65536u, 131073u, 0u, new Func<RubyContext, BlockParam, MutableString, object>(MutableStringOps.EachLine), new Func<MutableString, MutableString, Enumerator>(MutableStringOps.EachLine), new Func<BlockParam, MutableString, MutableString, object>(MutableStringOps.EachLine), new Func<RubyContext, MutableString, Enumerator>(MutableStringOps.EachLine));
			LibraryInitializer.DefineLibraryMethod(module, "empty?", 81, 0u, new Func<MutableString, bool>(MutableStringOps.IsEmpty));
			LibraryInitializer.DefineLibraryMethod(module, "encode", 81, 1048576u, new Func<ConversionStorage<IDictionary<object, object>>, ConversionStorage<MutableString>, MutableString, object, object, IDictionary<object, object>, MutableString>(MutableStringOps.Encode));
			LibraryInitializer.DefineLibraryMethod(module, "encode!", 81, 1048576u, new Func<ConversionStorage<IDictionary<object, object>>, ConversionStorage<MutableString>, MutableString, object, object, IDictionary<object, object>, MutableString>(MutableStringOps.EncodeInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "encoding", 81, 0u, new Func<MutableString, RubyEncoding>(MutableStringOps.GetEncoding));
			LibraryInitializer.DefineLibraryMethod(module, "end_with?", 81, 131072u, new Func<RubyScope, MutableString, MutableString, bool>(MutableStringOps.EndsWith));
			LibraryInitializer.DefineLibraryMethod(module, "eql?", 81, 2u, 2u, 0u, new Func<MutableString, MutableString, bool>(MutableStringOps.Eql), new Func<MutableString, string, bool>(MutableStringOps.Eql), new Func<MutableString, object, bool>(MutableStringOps.Eql));
			LibraryInitializer.DefineLibraryMethod(module, "force_encoding", 81, 2u, 131076u, new Func<MutableString, RubyEncoding, MutableString>(MutableStringOps.ForceEncoding), new Func<RubyContext, MutableString, MutableString, MutableString>(MutableStringOps.ForceEncoding));
			LibraryInitializer.DefineLibraryMethod(module, "getbyte", 81, 65536u, new Func<MutableString, int, object>(MutableStringOps.GetByte));
			LibraryInitializer.DefineLibraryMethod(module, "gsub", 81, 20u, 20u, 131084u, 1572912u, new Func<ConversionStorage<MutableString>, RubyScope, BlockParam, MutableString, RubyRegex, object>(MutableStringOps.BlockReplaceAll), new Func<ConversionStorage<MutableString>, RubyScope, BlockParam, MutableString, MutableString, object>(MutableStringOps.BlockReplaceAll), new Func<RubyScope, MutableString, RubyRegex, MutableString, MutableString>(MutableStringOps.ReplaceAll), new Func<ConversionStorage<MutableString>, BinaryOpStorage, RubyScope, MutableString, RubyRegex, Union<IDictionary<object, object>, MutableString>, MutableString>(MutableStringOps.ReplaceAll));
			LibraryInitializer.DefineLibraryMethod(module, "gsub!", 81, 524308u, 393228u, 1572912u, new Func<ConversionStorage<MutableString>, RubyScope, BlockParam, MutableString, RubyRegex, object>(MutableStringOps.BlockReplaceAllInPlace), new Func<RubyScope, MutableString, RubyRegex, MutableString, MutableString>(MutableStringOps.ReplaceAllInPlace), new Func<ConversionStorage<MutableString>, BinaryOpStorage, RubyScope, MutableString, RubyRegex, Union<IDictionary<object, object>, MutableString>, MutableString>(MutableStringOps.ReplaceAllInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "hex", 81, 0u, new Func<MutableString, object>(MutableStringOps.ToIntegerHex));
			LibraryInitializer.DefineLibraryMethod(module, "include?", 81, 65538u, 0u, new Func<MutableString, MutableString, bool>(MutableStringOps.Include), new Func<MutableString, int, bool>(MutableStringOps.Include));
			LibraryInitializer.DefineLibraryMethod(module, "index", 81, 196610u, 262148u, new Func<MutableString, MutableString, int, object>(MutableStringOps.Index), new Func<RubyScope, MutableString, RubyRegex, int, object>(MutableStringOps.Index));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 82, 0u, 65538u, 2u, new Func<MutableString, MutableString>(MutableStringOps.Reinitialize), new Func<MutableString, MutableString, MutableString>(MutableStringOps.Reinitialize), new Func<MutableString, byte[], MutableString>(MutableStringOps.Reinitialize));
			LibraryInitializer.DefineLibraryMethod(module, "initialize_copy", 82, 65538u, new Func<MutableString, MutableString, MutableString>(MutableStringOps.Reinitialize));
			LibraryInitializer.DefineLibraryMethod(module, "insert", 81, 196612u, new Func<MutableString, int, MutableString, MutableString>(MutableStringOps.Insert));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<RubyContext, MutableString, MutableString>(MutableStringOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "intern", 81, 0u, new Func<RubyContext, MutableString, RubySymbol>(MutableStringOps.ToSymbol));
			LibraryInitializer.DefineLibraryMethod(module, "length", 81, 0u, new Func<MutableString, int>(MutableStringOps.GetCharacterCount));
			LibraryInitializer.DefineLibraryMethod(module, "lines", 81, 2u, 65536u, 131073u, 0u, new Func<RubyContext, BlockParam, MutableString, object>(MutableStringOps.EachLine), new Func<MutableString, MutableString, Enumerator>(MutableStringOps.EachLine), new Func<BlockParam, MutableString, MutableString, object>(MutableStringOps.EachLine), new Func<RubyContext, MutableString, Enumerator>(MutableStringOps.EachLine));
			LibraryInitializer.DefineLibraryMethod(module, "ljust", 81, 65536u, 196612u, new Func<MutableString, int, MutableString>(MutableStringOps.LeftJustify), new Func<MutableString, int, MutableString, MutableString>(MutableStringOps.LeftJustify));
			LibraryInitializer.DefineLibraryMethod(module, "lstrip", 81, 0u, new Func<RubyContext, MutableString, MutableString>(MutableStringOps.StripLeft));
			LibraryInitializer.DefineLibraryMethod(module, "lstrip!", 81, 0u, new Func<RubyContext, MutableString, MutableString>(MutableStringOps.StripLeftInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "match", 81, 8u, 262152u, new Func<CallSiteStorage<Func<CallSite, RubyScope, object, object, object>>, RubyScope, MutableString, RubyRegex, object>(MutableStringOps.Match), new Func<CallSiteStorage<Func<CallSite, RubyScope, object, object, object>>, RubyScope, MutableString, MutableString, object>(MutableStringOps.Match));
			LibraryInitializer.DefineLibraryMethod(module, "next", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.Succ));
			LibraryInitializer.DefineLibraryMethod(module, "next!", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.SuccInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "oct", 81, 0u, new Func<MutableString, object>(MutableStringOps.ToIntegerOctal));
			LibraryInitializer.DefineLibraryMethod(module, "ord", 81, 0u, new Func<MutableString, int>(MutableStringOps.Ord));
			LibraryInitializer.DefineLibraryMethod(module, "replace", 81, 65538u, new Func<MutableString, MutableString, MutableString>(MutableStringOps.Replace));
			LibraryInitializer.DefineLibraryMethod(module, "reverse", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.GetReversed));
			LibraryInitializer.DefineLibraryMethod(module, "reverse!", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.Reverse));
			LibraryInitializer.DefineLibraryMethod(module, "rindex", 81, 65538u, 196610u, 262148u, new Func<MutableString, MutableString, object>(MutableStringOps.LastIndexOf), new Func<MutableString, MutableString, int, object>(MutableStringOps.LastIndexOf), new Func<RubyScope, MutableString, RubyRegex, int, object>(MutableStringOps.LastIndexOf));
			LibraryInitializer.DefineLibraryMethod(module, "rjust", 81, 65536u, 196612u, new Func<MutableString, int, MutableString>(MutableStringOps.RightJustify), new Func<MutableString, int, MutableString, MutableString>(MutableStringOps.RightJustify));
			LibraryInitializer.DefineLibraryMethod(module, "rstrip", 81, 0u, new Func<RubyContext, MutableString, MutableString>(MutableStringOps.StripRight));
			LibraryInitializer.DefineLibraryMethod(module, "rstrip!", 81, 0u, new Func<RubyContext, MutableString, MutableString>(MutableStringOps.StripRightInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "scan", 81, 131076u, 262154u, new Func<RubyScope, MutableString, RubyRegex, RubyArray>(MutableStringOps.Scan), new Func<RubyScope, BlockParam, MutableString, RubyRegex, object>(MutableStringOps.Scan));
			LibraryInitializer.DefineLibraryMethod(module, "setbyte", 81, 196608u, new Func<MutableString, int, int, MutableString>(MutableStringOps.SetByte));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<MutableString, int>(MutableStringOps.GetCharacterCount));
			LibraryInitializer.DefineLibraryMethod(module, "slice", 81, new uint[6] { 65536u, 196608u, 4u, 2u, 4u, 262148u }, new Func<MutableString, int, MutableString>(MutableStringOps.GetChar), new Func<MutableString, int, int, MutableString>(MutableStringOps.GetSubstring), new Func<ConversionStorage<int>, MutableString, Range, MutableString>(MutableStringOps.GetSubstring), new Func<MutableString, MutableString, MutableString>(MutableStringOps.GetSubstring), new Func<RubyScope, MutableString, RubyRegex, MutableString>(MutableStringOps.GetSubstring), new Func<RubyScope, MutableString, RubyRegex, int, MutableString>(MutableStringOps.GetSubstring));
			LibraryInitializer.DefineLibraryMethod(module, "slice!", 81, new uint[6] { 131072u, 196608u, 4u, 4u, 262148u, 2u }, new Func<RubyContext, MutableString, int, object>(MutableStringOps.RemoveCharInPlace), new Func<MutableString, int, int, MutableString>(MutableStringOps.RemoveSubstringInPlace), new Func<ConversionStorage<int>, MutableString, Range, MutableString>(MutableStringOps.RemoveSubstringInPlace), new Func<RubyScope, MutableString, RubyRegex, MutableString>(MutableStringOps.RemoveSubstringInPlace), new Func<RubyScope, MutableString, RubyRegex, int, MutableString>(MutableStringOps.RemoveSubstringInPlace), new Func<MutableString, MutableString, MutableString>(MutableStringOps.RemoveSubstringInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "split", 81, 0u, 393216u, 262148u, new Func<ConversionStorage<MutableString>, MutableString, RubyArray>(MutableStringOps.Split), new Func<ConversionStorage<MutableString>, MutableString, MutableString, int, RubyArray>(MutableStringOps.Split), new Func<ConversionStorage<MutableString>, MutableString, RubyRegex, int, RubyArray>(MutableStringOps.Split));
			LibraryInitializer.DefineLibraryMethod(module, "squeeze", 81, 2147614724u, new Func<RubyContext, MutableString, MutableString[], MutableString>(MutableStringOps.Squeeze));
			LibraryInitializer.DefineLibraryMethod(module, "squeeze!", 81, 2147614724u, new Func<RubyContext, MutableString, MutableString[], MutableString>(MutableStringOps.SqueezeInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "start_with?", 81, 131072u, new Func<RubyScope, MutableString, MutableString, bool>(MutableStringOps.StartsWith));
			LibraryInitializer.DefineLibraryMethod(module, "strip", 81, 0u, new Func<RubyContext, MutableString, MutableString>(MutableStringOps.Strip));
			LibraryInitializer.DefineLibraryMethod(module, "strip!", 81, 0u, new Func<RubyContext, MutableString, MutableString>(MutableStringOps.StripInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "sub", 81, 20u, 20u, 131084u, 1572912u, new Func<ConversionStorage<MutableString>, RubyScope, BlockParam, MutableString, RubyRegex, object>(MutableStringOps.BlockReplaceFirst), new Func<ConversionStorage<MutableString>, RubyScope, BlockParam, MutableString, MutableString, object>(MutableStringOps.BlockReplaceFirst), new Func<RubyScope, MutableString, RubyRegex, MutableString, MutableString>(MutableStringOps.ReplaceFirst), new Func<ConversionStorage<MutableString>, BinaryOpStorage, RubyScope, MutableString, RubyRegex, Union<IDictionary<object, object>, MutableString>, MutableString>(MutableStringOps.ReplaceFirst));
			LibraryInitializer.DefineLibraryMethod(module, "sub!", 81, 524308u, 393228u, 1572912u, new Func<ConversionStorage<MutableString>, RubyScope, BlockParam, MutableString, RubyRegex, object>(MutableStringOps.BlockReplaceFirstInPlace), new Func<RubyScope, MutableString, RubyRegex, MutableString, MutableString>(MutableStringOps.ReplaceFirstInPlace), new Func<ConversionStorage<MutableString>, BinaryOpStorage, RubyScope, MutableString, RubyRegex, Union<IDictionary<object, object>, MutableString>, MutableString>(MutableStringOps.ReplaceFirstInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "succ", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.Succ));
			LibraryInitializer.DefineLibraryMethod(module, "succ!", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.SuccInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "sum", 81, 65536u, new Func<MutableString, int, object>(MutableStringOps.GetChecksum));
			LibraryInitializer.DefineLibraryMethod(module, "swapcase", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.SwapCase));
			LibraryInitializer.DefineLibraryMethod(module, "swapcase!", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.SwapCaseInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "to_clr_string", 81, 0u, new Func<MutableString, string>(MutableStringOps.ToClrString));
			LibraryInitializer.DefineLibraryMethod(module, "to_f", 81, 0u, new Func<MutableString, double>(MutableStringOps.ToDouble));
			LibraryInitializer.DefineLibraryMethod(module, "to_i", 81, 65536u, new Func<MutableString, int, object>(MutableStringOps.ToInteger));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.ToS));
			LibraryInitializer.DefineLibraryMethod(module, "to_str", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.ToS));
			LibraryInitializer.DefineLibraryMethod(module, "to_sym", 81, 0u, new Func<RubyContext, MutableString, RubySymbol>(MutableStringOps.ToSymbol));
			LibraryInitializer.DefineLibraryMethod(module, "tr", 81, 196614u, new Func<MutableString, MutableString, MutableString, MutableString>(MutableStringOps.GetTranslated));
			LibraryInitializer.DefineLibraryMethod(module, "tr!", 81, 196614u, new Func<MutableString, MutableString, MutableString, MutableString>(MutableStringOps.Translate));
			LibraryInitializer.DefineLibraryMethod(module, "tr_s", 81, 196614u, new Func<MutableString, MutableString, MutableString, MutableString>(MutableStringOps.TrSqueeze));
			LibraryInitializer.DefineLibraryMethod(module, "tr_s!", 81, 196614u, new Func<MutableString, MutableString, MutableString, MutableString>(MutableStringOps.TrSqueezeInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "unpack", 81, 65538u, new Func<MutableString, MutableString, RubyArray>(MutableStringOps.Unpack));
			LibraryInitializer.DefineLibraryMethod(module, "upcase", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.UpCase));
			LibraryInitializer.DefineLibraryMethod(module, "upcase!", 81, 0u, new Func<MutableString, MutableString>(MutableStringOps.UpCaseInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "upto", 81, 131076u, 262154u, new Func<RangeOps.EachStorage, MutableString, MutableString, Enumerator>(MutableStringOps.UpTo), new Func<RangeOps.EachStorage, BlockParam, MutableString, MutableString, object>(MutableStringOps.UpTo));
			LibraryInitializer.DefineLibraryMethod(module, "valid_encoding?", 81, 0u, new Func<MutableString, bool>(MutableStringOps.ValidEncoding));
		}

		private static void LoadStruct_Constants(RubyModule module)
		{
			LibraryInitializer.SetBuiltinConstant(module, "Tms", RubyStructOps.CreateTmsClass(module));
		}

		private static void LoadStruct_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "[]", 81, 0u, 2u, 2u, 0u, new Func<RubyStruct, int, object>(RubyStructOps.GetValue), new Func<RubyStruct, RubySymbol, object>(RubyStructOps.GetValue), new Func<RubyStruct, MutableString, object>(RubyStructOps.GetValue), new Func<ConversionStorage<int>, RubyStruct, object, object>(RubyStructOps.GetValue));
			LibraryInitializer.DefineLibraryMethod(module, "[]=", 81, 0u, 2u, 2u, 0u, new Func<RubyStruct, int, object, object>(RubyStructOps.SetValue), new Func<RubyStruct, RubySymbol, object, object>(RubyStructOps.SetValue), new Func<RubyStruct, MutableString, object, object>(RubyStructOps.SetValue), new Func<ConversionStorage<int>, RubyStruct, object, object, object>(RubyStructOps.SetValue));
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 0u, new Func<BinaryOpStorage, RubyStruct, object, bool>(RubyStructOps.Equals));
			LibraryInitializer.DefineLibraryMethod(module, "each", 81, 0u, new Func<BlockParam, RubyStruct, object>(RubyStructOps.Each));
			LibraryInitializer.DefineLibraryMethod(module, "each_pair", 81, 0u, new Func<BlockParam, RubyStruct, object>(RubyStructOps.EachPair));
			LibraryInitializer.DefineLibraryMethod(module, "eql?", 81, 0u, new Func<BinaryOpStorage, RubyStruct, object, bool>(RubyStructOps.Equal));
			LibraryInitializer.DefineLibraryMethod(module, "hash", 81, 0u, new Func<UnaryOpStorage, ConversionStorage<int>, RubyStruct, int>(RubyStructOps.Hash));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 82, 2147483648u, new Action<RubyStruct, object[]>(RubyStructOps.Reinitialize));
			LibraryInitializer.DefineLibraryMethod(module, "initialize_copy", 82, 2u, new Func<RubyStruct, RubyStruct, RubyStruct>(RubyStructOps.InitializeCopy));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<RubyStruct, MutableString>(RubyStructOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "length", 81, 0u, new Func<RubyStruct, int>(RubyStructOps.GetSize));
			LibraryInitializer.DefineLibraryMethod(module, "members", 81, 0u, new Func<RubyStruct, RubyArray>(RubyStructOps.GetMembers));
			LibraryInitializer.DefineLibraryMethod(module, "select", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, BlockParam, RubyStruct, object>(RubyStructOps.Select));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<RubyStruct, int>(RubyStructOps.GetSize));
			LibraryInitializer.DefineLibraryMethod(module, "to_a", 81, 0u, new Func<RubyStruct, RubyArray>(RubyStructOps.Values));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<RubyStruct, MutableString>(RubyStructOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "values", 81, 0u, new Func<RubyStruct, RubyArray>(RubyStructOps.Values));
			LibraryInitializer.DefineLibraryMethod(module, "values_at", 81, 2147483648u, new Func<ConversionStorage<int>, RubyStruct, object[], RubyArray>(RubyStructOps.ValuesAt));
		}

		private static void LoadStruct_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "new", 97, 2147745804u, 2147745804u, 2147876872u, new Func<BlockParam, RubyClass, RubySymbol, string[], object>(RubyStructOps.NewAnonymousStruct), new Func<BlockParam, RubyClass, string, string[], object>(RubyStructOps.NewAnonymousStruct), new Func<BlockParam, RubyClass, MutableString, string[], object>(RubyStructOps.NewStruct));
		}

		private static void LoadSymbol_Instance(RubyModule module)
		{
			module.HideMethod("==");
			LibraryInitializer.DefineLibraryMethod(module, "[]", 81, new uint[6] { 65536u, 196608u, 4u, 2u, 4u, 262148u }, new Func<RubySymbol, int, MutableString>(SymbolOps.GetChar), new Func<RubySymbol, int, int, MutableString>(SymbolOps.GetSubstring), new Func<ConversionStorage<int>, RubySymbol, Range, MutableString>(SymbolOps.GetSubstring), new Func<RubySymbol, MutableString, MutableString>(SymbolOps.GetSubstring), new Func<RubyScope, RubySymbol, RubyRegex, MutableString>(SymbolOps.GetSubstring), new Func<RubyScope, RubySymbol, RubyRegex, int, MutableString>(SymbolOps.GetSubstring));
			LibraryInitializer.DefineLibraryMethod(module, "<=>", 81, 2u, 4u, 0u, new Func<RubySymbol, RubySymbol, int>(SymbolOps.Compare), new Func<RubyContext, RubySymbol, ClrName, int>(SymbolOps.Compare), new Func<RubySymbol, object, object>(SymbolOps.Compare));
			LibraryInitializer.DefineLibraryMethod(module, "=~", 81, 4u, 2u, 0u, new Func<RubyScope, RubySymbol, RubyRegex, object>(SymbolOps.Match), new Func<ClrName, RubySymbol, object>(SymbolOps.Match), new Func<CallSiteStorage<Func<CallSite, RubyScope, object, object, object>>, RubyScope, RubySymbol, object, object>(SymbolOps.Match));
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 2u, 4u, 0u, new Func<RubySymbol, RubySymbol, bool>(SymbolOps.Equals), new Func<RubyContext, RubySymbol, ClrName, bool>(SymbolOps.Equals), new Func<RubySymbol, object, bool>(SymbolOps.Equals));
			LibraryInitializer.DefineLibraryMethod(module, "===", 81, 2u, 4u, 0u, new Func<RubySymbol, RubySymbol, bool>(SymbolOps.Equals), new Func<RubyContext, RubySymbol, ClrName, bool>(SymbolOps.Equals), new Func<RubySymbol, object, bool>(SymbolOps.Equals));
			LibraryInitializer.DefineLibraryMethod(module, "capitalize", 81, 0u, new Func<RubyContext, RubySymbol, RubySymbol>(SymbolOps.Capitalize));
			LibraryInitializer.DefineLibraryMethod(module, "casecmp", 81, 2u, 65538u, new Func<RubySymbol, RubySymbol, int>(SymbolOps.Casecmp), new Func<RubySymbol, MutableString, int>(SymbolOps.Casecmp));
			LibraryInitializer.DefineLibraryMethod(module, "downcase", 81, 0u, new Func<RubyContext, RubySymbol, RubySymbol>(SymbolOps.DownCase));
			LibraryInitializer.DefineLibraryMethod(module, "empty?", 81, 0u, new Func<RubySymbol, bool>(SymbolOps.IsEmpty));
			LibraryInitializer.DefineLibraryMethod(module, "encoding", 81, 0u, new Func<RubySymbol, RubyEncoding>(SymbolOps.GetEncoding));
			LibraryInitializer.DefineLibraryMethod(module, "id2name", 81, 0u, new Func<RubySymbol, MutableString>(SymbolOps.ToString));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<RubyContext, RubySymbol, MutableString>(SymbolOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "intern", 81, 0u, new Func<RubySymbol, RubySymbol>(SymbolOps.ToSymbol));
			LibraryInitializer.DefineLibraryMethod(module, "length", 81, 0u, new Func<RubySymbol, int>(SymbolOps.GetLength));
			LibraryInitializer.DefineLibraryMethod(module, "match", 81, 8u, 262152u, new Func<CallSiteStorage<Func<CallSite, RubyScope, object, object, object>>, RubyScope, RubySymbol, RubyRegex, object>(SymbolOps.Match), new Func<CallSiteStorage<Func<CallSite, RubyScope, object, object, object>>, RubyScope, RubySymbol, MutableString, object>(SymbolOps.Match));
			LibraryInitializer.DefineLibraryMethod(module, "next", 81, 0u, new Func<RubyContext, RubySymbol, RubySymbol>(SymbolOps.Succ));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<RubySymbol, int>(SymbolOps.GetLength));
			LibraryInitializer.DefineLibraryMethod(module, "slice", 81, new uint[6] { 65536u, 196608u, 4u, 2u, 4u, 262148u }, new Func<RubySymbol, int, MutableString>(SymbolOps.GetChar), new Func<RubySymbol, int, int, MutableString>(SymbolOps.GetSubstring), new Func<ConversionStorage<int>, RubySymbol, Range, MutableString>(SymbolOps.GetSubstring), new Func<RubySymbol, MutableString, MutableString>(SymbolOps.GetSubstring), new Func<RubyScope, RubySymbol, RubyRegex, MutableString>(SymbolOps.GetSubstring), new Func<RubyScope, RubySymbol, RubyRegex, int, MutableString>(SymbolOps.GetSubstring));
			LibraryInitializer.DefineLibraryMethod(module, "succ", 81, 0u, new Func<RubyContext, RubySymbol, RubySymbol>(SymbolOps.Succ));
			LibraryInitializer.DefineLibraryMethod(module, "swapcase", 81, 0u, new Func<RubyContext, RubySymbol, RubySymbol>(SymbolOps.SwapCase));
			LibraryInitializer.DefineLibraryMethod(module, "to_clr_string", 81, 0u, new Func<RubySymbol, string>(SymbolOps.ToClrString));
			LibraryInitializer.DefineLibraryMethod(module, "to_proc", 81, 0u, new Func<RubyScope, RubySymbol, Proc>(SymbolOps.ToProc));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<RubySymbol, MutableString>(SymbolOps.ToString));
			LibraryInitializer.DefineLibraryMethod(module, "to_sym", 81, 0u, new Func<RubySymbol, RubySymbol>(SymbolOps.ToSymbol));
			LibraryInitializer.DefineLibraryMethod(module, "upcase", 81, 0u, new Func<RubyContext, RubySymbol, RubySymbol>(SymbolOps.UpCase));
		}

		private static void LoadSymbol_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "all_symbols", 97, 0u, new Func<RubyClass, RubyArray>(SymbolOps.GetAllSymbols));
		}

		private static void LoadSystem__Byte_Constants(RubyModule module)
		{
		}

		private static void LoadSystem__Byte_Instance(RubyModule module)
		{
			LoadIronRuby__Clr__Integer_Instance(module);
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<object, MutableString>(ByteOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "next", 81, 0u, new Func<byte, object>(ByteOps.Next));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<byte, int>(ByteOps.Size));
			LibraryInitializer.DefineLibraryMethod(module, "succ", 81, 0u, new Func<byte, object>(ByteOps.Next));
		}

		private static void LoadSystem__Byte_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "induced_from", 97, 65536u, 2u, 0u, new Func<RubyClass, int, byte>(ByteOps.InducedFrom), new Func<RubyClass, BigInteger, byte>(ByteOps.InducedFrom), new Func<RubyClass, double, byte>(ByteOps.InducedFrom));
		}

		private static void LoadSystem__Char_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "dump", 81, 0u, new Func<char, MutableString>(CharOps.Dump));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<RubyContext, char, MutableString>(CharOps.Inspect));
		}

		private static void LoadSystem__Collections__Generic__IDictionary_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "[]", 81, 0u, new Func<RubyContext, IDictionary<object, object>, object, object>(IDictionaryOps.GetElement));
			LibraryInitializer.DefineLibraryMethod(module, "[]=", 81, 0u, 0u, new Func<RubyContext, Hash, object, object, object>(IDictionaryOps.SetElement), new Func<RubyContext, IDictionary<object, object>, object, object, object>(IDictionaryOps.SetElement));
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 0u, 4u, new Func<RespondToStorage, BinaryOpStorage, IDictionary<object, object>, object, bool>(IDictionaryOps.Equals), new Func<BinaryOpStorage, IDictionary<object, object>, IDictionary<object, object>, bool>(IDictionaryOps.Equals));
			LibraryInitializer.DefineLibraryMethod(module, "clear", 81, 0u, 0u, new Func<Hash, IDictionary<object, object>>(IDictionaryOps.Clear), new Func<IDictionary<object, object>, IDictionary<object, object>>(IDictionaryOps.Clear));
			LibraryInitializer.DefineLibraryMethod(module, "default", 81, 0u, new Func<RubyContext, IDictionary<object, object>, object, object>(IDictionaryOps.GetDefaultValue));
			LibraryInitializer.DefineLibraryMethod(module, "default_proc", 81, 0u, new Func<IDictionary<object, object>, Proc>(IDictionaryOps.GetDefaultProc));
			LibraryInitializer.DefineLibraryMethod(module, "delete", 81, 0u, 0u, new Func<BlockParam, Hash, object, object>(IDictionaryOps.Delete), new Func<BlockParam, IDictionary<object, object>, object, object>(IDictionaryOps.Delete));
			LibraryInitializer.DefineLibraryMethod(module, "delete_if", 81, 0u, 0u, new Func<BlockParam, Hash, object>(IDictionaryOps.DeleteIf), new Func<BlockParam, IDictionary<object, object>, object>(IDictionaryOps.DeleteIf));
			LibraryInitializer.DefineLibraryMethod(module, "each", 81, 0u, 2u, new Func<RubyContext, IDictionary<object, object>, Enumerator>(IDictionaryOps.Each), new Func<RubyContext, BlockParam, IDictionary<object, object>, object>(IDictionaryOps.Each));
			LibraryInitializer.DefineLibraryMethod(module, "each_key", 81, 0u, 2u, new Func<RubyContext, IDictionary<object, object>, Enumerator>(IDictionaryOps.EachKey), new Func<RubyContext, BlockParam, IDictionary<object, object>, object>(IDictionaryOps.EachKey));
			LibraryInitializer.DefineLibraryMethod(module, "each_pair", 81, 0u, 2u, new Func<RubyContext, IDictionary<object, object>, Enumerator>(IDictionaryOps.Each), new Func<RubyContext, BlockParam, IDictionary<object, object>, object>(IDictionaryOps.Each));
			LibraryInitializer.DefineLibraryMethod(module, "each_value", 81, 0u, 2u, new Func<RubyContext, IDictionary<object, object>, Enumerator>(IDictionaryOps.EachValue), new Func<RubyContext, BlockParam, IDictionary<object, object>, object>(IDictionaryOps.EachValue));
			LibraryInitializer.DefineLibraryMethod(module, "empty?", 81, 0u, new Func<IDictionary<object, object>, bool>(IDictionaryOps.Empty));
			LibraryInitializer.DefineLibraryMethod(module, "fetch", 81, 0u, new Func<RubyContext, BlockParam, IDictionary<object, object>, object, object, object>(IDictionaryOps.Fetch));
			LibraryInitializer.DefineLibraryMethod(module, "flatten", 81, 131072u, new Func<ConversionStorage<IList>, IDictionary<object, object>, int, IList>(IDictionaryOps.Flatten));
			LibraryInitializer.DefineLibraryMethod(module, "has_key?", 81, 0u, new Func<IDictionary<object, object>, object, bool>(IDictionaryOps.HasKey));
			LibraryInitializer.DefineLibraryMethod(module, "has_value?", 81, 0u, new Func<BinaryOpStorage, IDictionary<object, object>, object, bool>(IDictionaryOps.HasValue));
			LibraryInitializer.DefineLibraryMethod(module, "include?", 81, 0u, new Func<IDictionary<object, object>, object, bool>(IDictionaryOps.HasKey));
			LibraryInitializer.DefineLibraryMethod(module, "index", 81, 0u, new Func<BinaryOpStorage, IDictionary<object, object>, object, object>(IDictionaryOps.Index));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<RubyContext, IDictionary<object, object>, MutableString>(IDictionaryOps.ToMutableString));
			LibraryInitializer.DefineLibraryMethod(module, "invert", 81, 0u, new Func<RubyContext, IDictionary<object, object>, Hash>(IDictionaryOps.Invert));
			LibraryInitializer.DefineLibraryMethod(module, "key?", 81, 0u, new Func<IDictionary<object, object>, object, bool>(IDictionaryOps.HasKey));
			LibraryInitializer.DefineLibraryMethod(module, "keys", 81, 0u, new Func<IDictionary<object, object>, RubyArray>(IDictionaryOps.GetKeys));
			LibraryInitializer.DefineLibraryMethod(module, "length", 81, 0u, new Func<IDictionary<object, object>, int>(IDictionaryOps.Length));
			LibraryInitializer.DefineLibraryMethod(module, "member?", 81, 0u, new Func<IDictionary<object, object>, object, bool>(IDictionaryOps.HasKey));
			LibraryInitializer.DefineLibraryMethod(module, "merge", 81, 524304u, new Func<CallSiteStorage<Func<CallSite, object, object, object>>, CallSiteStorage<Func<CallSite, RubyClass, object>>, BlockParam, IDictionary<object, object>, IDictionary<object, object>, object>(IDictionaryOps.Merge));
			LibraryInitializer.DefineLibraryMethod(module, "merge!", 81, 131076u, 131076u, new Func<BlockParam, Hash, IDictionary<object, object>, object>(IDictionaryOps.Update), new Func<BlockParam, IDictionary<object, object>, IDictionary<object, object>, object>(IDictionaryOps.Update));
			LibraryInitializer.DefineLibraryMethod(module, "rehash", 81, 0u, 0u, new Func<Hash, IDictionary<object, object>>(IDictionaryOps.Rehash), new Func<IDictionary<object, object>, IDictionary<object, object>>(IDictionaryOps.Rehash));
			LibraryInitializer.DefineLibraryMethod(module, "reject", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, object, object>>, CallSiteStorage<Func<CallSite, RubyClass, object>>, BlockParam, IDictionary<object, object>, object>(IDictionaryOps.Reject));
			LibraryInitializer.DefineLibraryMethod(module, "reject!", 81, 0u, 0u, new Func<BlockParam, Hash, object>(IDictionaryOps.RejectMutate), new Func<BlockParam, IDictionary<object, object>, object>(IDictionaryOps.RejectMutate));
			LibraryInitializer.DefineLibraryMethod(module, "replace", 81, 131076u, new Func<RubyContext, Hash, IDictionary<object, object>, Hash>(IDictionaryOps.Replace));
			LibraryInitializer.DefineLibraryMethod(module, "select", 81, 0u, 2u, new Func<RubyContext, IDictionary<object, object>, Enumerator>(IDictionaryOps.Select), new Func<RubyContext, BlockParam, IDictionary<object, object>, object>(IDictionaryOps.Select));
			LibraryInitializer.DefineLibraryMethod(module, "shift", 81, 0u, 0u, new Func<Hash, object>(IDictionaryOps.Shift), new Func<IDictionary<object, object>, object>(IDictionaryOps.Shift));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<IDictionary<object, object>, int>(IDictionaryOps.Length));
			LibraryInitializer.DefineLibraryMethod(module, "sort", 81, 0u, new Func<ComparisonStorage, BlockParam, IDictionary<object, object>, object>(IDictionaryOps.Sort));
			LibraryInitializer.DefineLibraryMethod(module, "store", 81, 0u, 0u, new Func<RubyContext, Hash, object, object, object>(IDictionaryOps.SetElement), new Func<RubyContext, IDictionary<object, object>, object, object, object>(IDictionaryOps.SetElement));
			LibraryInitializer.DefineLibraryMethod(module, "to_a", 81, 0u, new Func<IDictionary<object, object>, RubyArray>(IDictionaryOps.ToArray));
			LibraryInitializer.DefineLibraryMethod(module, "to_hash", 81, 0u, new Func<IDictionary<object, object>, IDictionary<object, object>>(IDictionaryOps.ToHash));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<RubyContext, IDictionary<object, object>, MutableString>(IDictionaryOps.ToMutableString));
			LibraryInitializer.DefineLibraryMethod(module, "update", 81, 131076u, 131076u, new Func<BlockParam, Hash, IDictionary<object, object>, object>(IDictionaryOps.Update), new Func<BlockParam, IDictionary<object, object>, IDictionary<object, object>, object>(IDictionaryOps.Update));
			LibraryInitializer.DefineLibraryMethod(module, "value?", 81, 0u, new Func<BinaryOpStorage, IDictionary<object, object>, object, bool>(IDictionaryOps.HasValue));
			LibraryInitializer.DefineLibraryMethod(module, "values", 81, 0u, new Func<IDictionary<object, object>, RubyArray>(IDictionaryOps.GetValues));
			LibraryInitializer.DefineLibraryMethod(module, "values_at", 81, 2147483648u, new Func<RubyContext, IDictionary<object, object>, object[], RubyArray>(IDictionaryOps.ValuesAt));
		}

		private static void LoadSystem__Collections__IEnumerable_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "each", 81, 0u, new Func<BlockParam, IEnumerable, object>(IEnumerableOps.Each));
		}

		private static void LoadSystem__Collections__IList_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "-", 81, 262152u, new Func<UnaryOpStorage, BinaryOpStorage, IList, IList, RubyArray>(IListOps.Difference));
			LibraryInitializer.DefineLibraryMethod(module, "&", 81, 262144u, new Func<UnaryOpStorage, BinaryOpStorage, IList, IList, RubyArray>(IListOps.Intersection));
			LibraryInitializer.DefineLibraryMethod(module, "*", 81, 0u, 4u, 262152u, new Func<UnaryOpStorage, IList, int, IList>(IListOps.Repeat), new Func<JoinConversionStorage, IList, MutableString, MutableString>(IListOps.Repeat), new Func<UnaryOpStorage, JoinConversionStorage, IList, Union<MutableString, int>, object>(IListOps.Repeat));
			LibraryInitializer.DefineLibraryMethod(module, "[]", 81, 65536u, 393216u, 8u, new Func<IList, int, object>(IListOps.GetElement), new Func<UnaryOpStorage, IList, int, int, IList>(IListOps.GetElements), new Func<ConversionStorage<int>, UnaryOpStorage, IList, Range, IList>(IListOps.GetElements));
			LibraryInitializer.DefineLibraryMethod(module, "[]=", 81, 65536u, 65536u, 393216u, 8u, new Func<RubyArray, int, object, object>(IListOps.SetElement), new Func<IList, int, object, object>(IListOps.SetElement), new Func<ConversionStorage<IList>, IList, int, int, object, object>(IListOps.SetElement), new Func<ConversionStorage<IList>, ConversionStorage<int>, IList, Range, object, object>(IListOps.SetElement));
			LibraryInitializer.DefineLibraryMethod(module, "|", 81, 262144u, new Func<UnaryOpStorage, BinaryOpStorage, IList, IList, RubyArray>(IListOps.Union));
			LibraryInitializer.DefineLibraryMethod(module, "+", 81, 65538u, new Func<IList, IList, RubyArray>(IListOps.Concatenate));
			LibraryInitializer.DefineLibraryMethod(module, "<<", 81, 0u, new Func<IList, object, IList>(IListOps.Append));
			LibraryInitializer.DefineLibraryMethod(module, "<=>", 81, 0u, 4u, new Func<BinaryOpStorage, ConversionStorage<IList>, IList, object, object>(IListOps.Compare), new Func<BinaryOpStorage, IList, IList, object>(IListOps.Compare));
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 0u, 4u, new Func<RespondToStorage, BinaryOpStorage, IList, object, bool>(IListOps.Equals), new Func<BinaryOpStorage, IList, IList, bool>(IListOps.Equals));
			LibraryInitializer.DefineLibraryMethod(module, "assoc", 81, 0u, new Func<BinaryOpStorage, IList, object, IList>(IListOps.GetContainerOfFirstItem));
			LibraryInitializer.DefineLibraryMethod(module, "at", 81, 65536u, new Func<IList, int, object>(IListOps.At));
			LibraryInitializer.DefineLibraryMethod(module, "clear", 81, 0u, new Func<IList, IList>(IListOps.Clear));
			LibraryInitializer.DefineLibraryMethod(module, "collect!", 81, 0u, new Func<BlockParam, IList, object>(IListOps.CollectInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "combination", 81, 131072u, new Func<BlockParam, IList, int?, object>(IListOps.GetCombinations));
			LibraryInitializer.DefineLibraryMethod(module, "compact", 81, 0u, new Func<UnaryOpStorage, IList, IList>(IListOps.Compact));
			LibraryInitializer.DefineLibraryMethod(module, "compact!", 81, 0u, new Func<IList, IList>(IListOps.CompactInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "concat", 81, 65538u, new Func<IList, IList, IList>(IListOps.Concat));
			LibraryInitializer.DefineLibraryMethod(module, "count", 81, 0u, new Func<IList, int>(IListOps.Length));
			LibraryInitializer.DefineLibraryMethod(module, "delete", 81, 0u, 0u, new Func<BinaryOpStorage, IList, object, object>(IListOps.Delete), new Func<BinaryOpStorage, BlockParam, IList, object, object>(IListOps.Delete));
			LibraryInitializer.DefineLibraryMethod(module, "delete_at", 81, 65536u, new Func<IList, int, object>(IListOps.DeleteAt));
			LibraryInitializer.DefineLibraryMethod(module, "delete_if", 81, 0u, new Func<BlockParam, IList, object>(IListOps.DeleteIf));
			LibraryInitializer.DefineLibraryMethod(module, "each", 81, 0u, 1u, new Func<IList, Enumerator>(IListOps.Each), new Func<BlockParam, IList, object>(IListOps.Each));
			LibraryInitializer.DefineLibraryMethod(module, "each_index", 81, 0u, 1u, new Func<IList, Enumerator>(IListOps.EachIndex), new Func<BlockParam, IList, object>(IListOps.EachIndex));
			LibraryInitializer.DefineLibraryMethod(module, "empty?", 81, 0u, new Func<IList, bool>(IListOps.Empty));
			LibraryInitializer.DefineLibraryMethod(module, "eql?", 81, 0u, new Func<BinaryOpStorage, IList, object, bool>(IListOps.HashEquals));
			LibraryInitializer.DefineLibraryMethod(module, "fetch", 81, 0u, new Func<ConversionStorage<int>, BlockParam, IList, object, object, object>(IListOps.Fetch));
			LibraryInitializer.DefineLibraryMethod(module, "fill", 81, new uint[8] { 0u, 0u, 0u, 8u, 1u, 1u, 2u, 10u }, new Func<IList, object, int, IList>(IListOps.Fill), new Func<IList, object, int, int, IList>(IListOps.Fill), new Func<ConversionStorage<int>, IList, object, object, object, IList>(IListOps.Fill), new Func<ConversionStorage<int>, IList, object, Range, IList>(IListOps.Fill), new Func<BlockParam, IList, int, object>(IListOps.Fill), new Func<BlockParam, IList, int, int, object>(IListOps.Fill), new Func<ConversionStorage<int>, BlockParam, IList, object, object, object>(IListOps.Fill), new Func<ConversionStorage<int>, BlockParam, IList, Range, object>(IListOps.Fill));
			LibraryInitializer.DefineLibraryMethod(module, "find_index", 81, 0u, 1u, 0u, new Func<BlockParam, IList, Enumerator>(IListOps.GetFindIndexEnumerator), new Func<BlockParam, IList, object>(IListOps.FindIndex), new Func<BinaryOpStorage, BlockParam, IList, object, object>(IListOps.FindIndex));
			LibraryInitializer.DefineLibraryMethod(module, "first", 81, 0u, 65536u, new Func<IList, object>(IListOps.First), new Func<IList, int, IList>(IListOps.First));
			LibraryInitializer.DefineLibraryMethod(module, "flatten", 81, 262144u, new Func<UnaryOpStorage, ConversionStorage<IList>, IList, int, IList>(IListOps.Flatten));
			LibraryInitializer.DefineLibraryMethod(module, "flatten!", 81, 131072u, 131072u, new Func<ConversionStorage<IList>, RubyArray, int, IList>(IListOps.FlattenInPlace), new Func<ConversionStorage<IList>, IList, int, IList>(IListOps.FlattenInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "hash", 81, 0u, new Func<UnaryOpStorage, ConversionStorage<int>, IList, int>(IListOps.GetHashCode));
			LibraryInitializer.DefineLibraryMethod(module, "include?", 81, 0u, new Func<BinaryOpStorage, IList, object, bool>(IListOps.Include));
			LibraryInitializer.DefineLibraryMethod(module, "index", 81, 0u, 1u, 0u, new Func<BlockParam, IList, Enumerator>(IListOps.GetFindIndexEnumerator), new Func<BlockParam, IList, object>(IListOps.FindIndex), new Func<BinaryOpStorage, BlockParam, IList, object, object>(IListOps.FindIndex));
			LibraryInitializer.DefineLibraryMethod(module, "indexes", 81, 2147483648u, new Func<ConversionStorage<int>, UnaryOpStorage, IList, object[], object>(IListOps.Indexes));
			LibraryInitializer.DefineLibraryMethod(module, "indices", 81, 2147483648u, new Func<ConversionStorage<int>, UnaryOpStorage, IList, object[], object>(IListOps.Indexes));
			LibraryInitializer.DefineLibraryMethod(module, "initialize_copy", 82, 65538u, new Func<IList, IList, IList>(IListOps.Replace));
			LibraryInitializer.DefineLibraryMethod(module, "insert", 81, 2147549184u, new Func<IList, int, object[], IList>(IListOps.Insert));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<RubyContext, IList, MutableString>(IListOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "join", 81, 0u, 0u, new Func<JoinConversionStorage, IList, MutableString>(IListOps.Join), new Func<JoinConversionStorage, ConversionStorage<MutableString>, IList, object, MutableString>(IListOps.JoinWithLazySeparatorConversion));
			LibraryInitializer.DefineLibraryMethod(module, "last", 81, 0u, 65536u, new Func<IList, object>(IListOps.Last), new Func<IList, int, IList>(IListOps.Last));
			LibraryInitializer.DefineLibraryMethod(module, "length", 81, 0u, new Func<IList, int>(IListOps.Length));
			LibraryInitializer.DefineLibraryMethod(module, "map!", 81, 0u, new Func<BlockParam, IList, object>(IListOps.CollectInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "nitems", 81, 0u, new Func<IList, int>(IListOps.NumberOfNonNilItems));
			LibraryInitializer.DefineLibraryMethod(module, "none?", 81, 0u, new Func<IList, bool>(IListOps.Empty));
			LibraryInitializer.DefineLibraryMethod(module, "permutation", 81, 131072u, new Func<BlockParam, IList, int?, object>(IListOps.GetPermutations));
			LibraryInitializer.DefineLibraryMethod(module, "pop", 81, 0u, 131072u, new Func<IList, object>(IListOps.Pop), new Func<RubyContext, IList, int, object>(IListOps.Pop));
			LibraryInitializer.DefineLibraryMethod(module, "product", 81, 2147549186u, new Func<IList, IList[], RubyArray>(IListOps.Product));
			LibraryInitializer.DefineLibraryMethod(module, "push", 81, 2147483648u, new Func<IList, object[], IList>(IListOps.Push));
			LibraryInitializer.DefineLibraryMethod(module, "rassoc", 81, 0u, new Func<BinaryOpStorage, IList, object, IList>(IListOps.GetContainerOfSecondItem));
			LibraryInitializer.DefineLibraryMethod(module, "reject", 81, 0u, new Func<CallSiteStorage<Func<CallSite, object, Proc, object>>, UnaryOpStorage, BlockParam, IList, object>(IListOps.Reject));
			LibraryInitializer.DefineLibraryMethod(module, "reject!", 81, 0u, new Func<BlockParam, IList, object>(IListOps.RejectInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "replace", 81, 65538u, new Func<IList, IList, IList>(IListOps.Replace));
			LibraryInitializer.DefineLibraryMethod(module, "reverse", 81, 0u, new Func<UnaryOpStorage, IList, IList>(IListOps.Reverse));
			LibraryInitializer.DefineLibraryMethod(module, "reverse!", 81, 0u, new Func<IList, IList>(IListOps.InPlaceReverse));
			LibraryInitializer.DefineLibraryMethod(module, "reverse_each", 81, 0u, 1u, new Func<RubyArray, Enumerator>(IListOps.ReverseEach), new Func<BlockParam, RubyArray, object>(IListOps.ReverseEach));
			LibraryInitializer.DefineLibraryMethod(module, "rindex", 81, 1u, 0u, new Func<BlockParam, IList, object>(IListOps.ReverseIndex), new Func<BinaryOpStorage, BlockParam, IList, object, object>(IListOps.ReverseIndex));
			LibraryInitializer.DefineLibraryMethod(module, "shift", 81, 0u, new Func<IList, object>(IListOps.Shift));
			LibraryInitializer.DefineLibraryMethod(module, "shuffle", 81, 0u, new Func<UnaryOpStorage, RubyArray, IList>(IListOps.Shuffle));
			LibraryInitializer.DefineLibraryMethod(module, "shuffle!", 81, 0u, new Func<RubyContext, RubyArray, RubyArray>(IListOps.ShuffleInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<IList, int>(IListOps.Length));
			LibraryInitializer.DefineLibraryMethod(module, "slice", 81, 65536u, 393216u, 8u, new Func<IList, int, object>(IListOps.GetElement), new Func<UnaryOpStorage, IList, int, int, IList>(IListOps.GetElements), new Func<ConversionStorage<int>, UnaryOpStorage, IList, Range, IList>(IListOps.GetElements));
			LibraryInitializer.DefineLibraryMethod(module, "slice!", 81, 65536u, 8u, 393216u, new Func<IList, int, object>(IListOps.SliceInPlace), new Func<ConversionStorage<int>, UnaryOpStorage, IList, Range, IList>(IListOps.SliceInPlace), new Func<UnaryOpStorage, IList, int, int, IList>(IListOps.SliceInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "sort", 81, 0u, new Func<UnaryOpStorage, ComparisonStorage, BlockParam, IList, object>(IListOps.Sort));
			LibraryInitializer.DefineLibraryMethod(module, "sort!", 81, 0u, new Func<ComparisonStorage, BlockParam, IList, object>(IListOps.SortInPlace));
			LibraryInitializer.DefineLibraryMethod(module, "to_a", 81, 0u, new Func<IList, RubyArray>(IListOps.ToArray));
			LibraryInitializer.DefineLibraryMethod(module, "to_ary", 81, 0u, new Func<IList, RubyArray>(IListOps.ToArray));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<RubyContext, IList, MutableString>(IListOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "transpose", 81, 0u, new Func<ConversionStorage<IList>, IList, RubyArray>(IListOps.Transpose));
			LibraryInitializer.DefineLibraryMethod(module, "uniq", 81, 0u, new Func<UnaryOpStorage, IList, IList>(IListOps.Unique));
			LibraryInitializer.DefineLibraryMethod(module, "uniq!", 81, 0u, 0u, new Func<UnaryOpStorage, BinaryOpStorage, RubyArray, IList>(IListOps.UniqueSelf), new Func<UnaryOpStorage, BinaryOpStorage, IList, IList>(IListOps.UniqueSelf));
			LibraryInitializer.DefineLibraryMethod(module, "unshift", 81, 0u, 2147483648u, new Func<IList, object, IList>(IListOps.Unshift), new Func<IList, object[], IList>(IListOps.Unshift));
			LibraryInitializer.DefineLibraryMethod(module, "values_at", 81, 2147483648u, new Func<ConversionStorage<int>, UnaryOpStorage, IList, object[], RubyArray>(IListOps.ValuesAt));
		}

		private static void LoadSystem__Decimal_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 0u, 0u, new Func<decimal, double, bool>(DecimalOps.Equal), new Func<BinaryOpStorage, decimal, object, bool>(DecimalOps.Equal));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<object, MutableString>(DecimalOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<object, int>(DecimalOps.Size));
			LibraryInitializer.DefineLibraryMethod(module, "to_f", 81, 0u, new Func<decimal, double>(DecimalOps.ToDouble));
			LibraryInitializer.DefineLibraryMethod(module, "to_i", 81, 0u, new Func<decimal, object>(DecimalOps.ToInt));
			LibraryInitializer.DefineLibraryMethod(module, "to_int", 81, 0u, new Func<decimal, object>(DecimalOps.ToInt));
		}

		private static void LoadSystem__Decimal_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "induced_from", 97, new uint[5] { 0u, 0u, 0u, 2u, 0u }, new Func<RubyModule, double, decimal>(DecimalOps.InducedFrom), new Func<RubyModule, decimal, decimal>(DecimalOps.InducedFrom), new Func<RubyModule, int, decimal>(DecimalOps.InducedFrom), new Func<RubyModule, BigInteger, decimal>(DecimalOps.InducedFrom), new Func<RubyModule, object, double>(DecimalOps.InducedFrom));
		}

		private static void LoadSystem__IComparable_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "<=>", 81, 0u, new Func<IComparable, object, int>(IComparableOps.Compare));
		}

		private static void LoadSystem__Int16_Constants(RubyModule module)
		{
		}

		private static void LoadSystem__Int16_Instance(RubyModule module)
		{
			LoadIronRuby__Clr__Integer_Instance(module);
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<object, MutableString>(Int16Ops.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "next", 81, 0u, new Func<short, object>(Int16Ops.Next));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<short, int>(Int16Ops.Size));
			LibraryInitializer.DefineLibraryMethod(module, "succ", 81, 0u, new Func<short, object>(Int16Ops.Next));
		}

		private static void LoadSystem__Int16_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "induced_from", 97, 65536u, 2u, 0u, new Func<RubyClass, int, short>(Int16Ops.InducedFrom), new Func<RubyClass, BigInteger, short>(Int16Ops.InducedFrom), new Func<RubyClass, double, short>(Int16Ops.InducedFrom));
		}

		private static void LoadSystem__Int64_Constants(RubyModule module)
		{
		}

		private static void LoadSystem__Int64_Instance(RubyModule module)
		{
			LoadIronRuby__Clr__BigInteger_Instance(module);
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<object, MutableString>(Int64Ops.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "next", 81, 0u, new Func<long, object>(Int64Ops.Next));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<long, int>(Int64Ops.Size));
			LibraryInitializer.DefineLibraryMethod(module, "succ", 81, 0u, new Func<long, object>(Int64Ops.Next));
		}

		private static void LoadSystem__Int64_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "induced_from", 97, 65536u, 2u, 0u, new Func<RubyClass, int, long>(Int64Ops.InducedFrom), new Func<RubyClass, BigInteger, long>(Int64Ops.InducedFrom), new Func<RubyClass, double, long>(Int64Ops.InducedFrom));
		}

		private static void LoadSystem__SByte_Constants(RubyModule module)
		{
		}

		private static void LoadSystem__SByte_Instance(RubyModule module)
		{
			LoadIronRuby__Clr__Integer_Instance(module);
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<object, MutableString>(SByteOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "next", 81, 0u, new Func<sbyte, object>(SByteOps.Next));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<sbyte, int>(SByteOps.Size));
			LibraryInitializer.DefineLibraryMethod(module, "succ", 81, 0u, new Func<sbyte, object>(SByteOps.Next));
		}

		private static void LoadSystem__SByte_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "induced_from", 97, 65536u, 2u, 0u, new Func<RubyClass, int, sbyte>(SByteOps.InducedFrom), new Func<RubyClass, BigInteger, sbyte>(SByteOps.InducedFrom), new Func<RubyClass, double, sbyte>(SByteOps.InducedFrom));
		}

		private static void LoadSystem__Single_Constants(RubyModule module)
		{
		}

		private static void LoadSystem__Single_Instance(RubyModule module)
		{
			LoadIronRuby__Clr__Float_Instance(module);
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<object, MutableString>(SingleOps.Inspect));
		}

		private static void LoadSystem__Single_Class(RubyModule module)
		{
			LoadIronRuby__Clr__Float_Class(module);
		}

		private static void LoadSystem__String_Instance(RubyModule module)
		{
			module.HideMethod("[]");
			module.HideMethod("==");
			module.HideMethod("clone");
			module.HideMethod("insert");
			module.HideMethod("split");
		}

		private static void LoadSystem__Type_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "to_class", 81, 0u, new Func<RubyContext, Type, RubyClass>(TypeOps.ToClass));
			LibraryInitializer.DefineLibraryMethod(module, "to_module", 81, 0u, new Func<RubyContext, Type, RubyModule>(TypeOps.ToModule));
		}

		private static void LoadSystem__UInt16_Constants(RubyModule module)
		{
		}

		private static void LoadSystem__UInt16_Instance(RubyModule module)
		{
			LoadIronRuby__Clr__Integer_Instance(module);
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<object, MutableString>(UInt16Ops.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "next", 81, 0u, new Func<ushort, object>(UInt16Ops.Next));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<ushort, int>(UInt16Ops.Size));
			LibraryInitializer.DefineLibraryMethod(module, "succ", 81, 0u, new Func<ushort, object>(UInt16Ops.Next));
		}

		private static void LoadSystem__UInt16_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "induced_from", 97, 65536u, 2u, 0u, new Func<RubyClass, int, ushort>(UInt16Ops.InducedFrom), new Func<RubyClass, BigInteger, ushort>(UInt16Ops.InducedFrom), new Func<RubyClass, double, ushort>(UInt16Ops.InducedFrom));
		}

		private static void LoadSystem__UInt32_Constants(RubyModule module)
		{
		}

		private static void LoadSystem__UInt32_Instance(RubyModule module)
		{
			LoadIronRuby__Clr__BigInteger_Instance(module);
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<object, MutableString>(UInt32Ops.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "next", 81, 0u, new Func<uint, object>(UInt32Ops.Next));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<uint, int>(UInt32Ops.Size));
			LibraryInitializer.DefineLibraryMethod(module, "succ", 81, 0u, new Func<uint, object>(UInt32Ops.Next));
		}

		private static void LoadSystem__UInt32_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "induced_from", 97, 65536u, 2u, 0u, new Func<RubyClass, int, uint>(UInt32Ops.InducedFrom), new Func<RubyClass, BigInteger, uint>(UInt32Ops.InducedFrom), new Func<RubyClass, double, uint>(UInt32Ops.InducedFrom));
		}

		private static void LoadSystem__UInt64_Constants(RubyModule module)
		{
		}

		private static void LoadSystem__UInt64_Instance(RubyModule module)
		{
			LoadIronRuby__Clr__BigInteger_Instance(module);
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<object, MutableString>(UInt64Ops.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "next", 81, 0u, new Func<ulong, object>(UInt64Ops.Next));
			LibraryInitializer.DefineLibraryMethod(module, "size", 81, 0u, new Func<ulong, int>(UInt64Ops.Size));
			LibraryInitializer.DefineLibraryMethod(module, "succ", 81, 0u, new Func<ulong, object>(UInt64Ops.Next));
		}

		private static void LoadSystem__UInt64_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "induced_from", 97, 65536u, 2u, 0u, new Func<RubyClass, int, ulong>(UInt64Ops.InducedFrom), new Func<RubyClass, BigInteger, ulong>(UInt64Ops.InducedFrom), new Func<RubyClass, double, ulong>(UInt64Ops.InducedFrom));
		}

		private static void LoadSystemCallError_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "errno", 81, 0u, new Func<ExternalException, int>(SystemCallErrorOps.Errno));
		}

		private static void LoadSystemExit_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "status", 81, 0u, new Func<SystemExit, int>(SystemExitOps.GetStatus));
			LibraryInitializer.DefineLibraryMethod(module, "success?", 81, 0u, new Func<SystemExit, bool>(SystemExitOps.IsSuccessful));
		}

		private static void LoadThread_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "[]", 81, 2u, 4u, 0u, new Func<Thread, RubySymbol, object>(ThreadOps.GetElement), new Func<RubyContext, Thread, MutableString, object>(ThreadOps.GetElement), new Func<RubyContext, Thread, object, object>(ThreadOps.GetElement));
			LibraryInitializer.DefineLibraryMethod(module, "[]=", 81, 2u, 4u, 0u, new Func<Thread, RubySymbol, object, object>(ThreadOps.SetElement), new Func<RubyContext, Thread, MutableString, object, object>(ThreadOps.SetElement), new Func<RubyContext, Thread, object, object, object>(ThreadOps.SetElement));
			LibraryInitializer.DefineLibraryMethod(module, "abort_on_exception", 81, 0u, new Func<Thread, object>(ThreadOps.AbortOnException));
			LibraryInitializer.DefineLibraryMethod(module, "abort_on_exception=", 81, 0u, new Func<Thread, bool, object>(ThreadOps.AbortOnException));
			LibraryInitializer.DefineLibraryMethod(module, "alive?", 81, 0u, new Func<Thread, bool>(ThreadOps.IsAlive));
			LibraryInitializer.DefineLibraryMethod(module, "exit", 81, 0u, new Func<Thread, Thread>(ThreadOps.Kill));
			LibraryInitializer.DefineLibraryMethod(module, "group", 81, 0u, new Func<Thread, ThreadGroup>(ThreadOps.Group));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<RubyContext, Thread, MutableString>(ThreadOps.Inspect));
			LibraryInitializer.DefineLibraryMethod(module, "join", 81, 0u, 0u, new Func<Thread, Thread>(ThreadOps.Join), new Func<Thread, double, Thread>(ThreadOps.Join));
			LibraryInitializer.DefineLibraryMethod(module, "key?", 81, 2u, 4u, 0u, new Func<Thread, RubySymbol, object>(ThreadOps.HasKey), new Func<RubyContext, Thread, MutableString, object>(ThreadOps.HasKey), new Func<RubyContext, Thread, object, object>(ThreadOps.HasKey));
			LibraryInitializer.DefineLibraryMethod(module, "keys", 81, 0u, new Func<RubyContext, Thread, object>(ThreadOps.Keys));
			LibraryInitializer.DefineLibraryMethod(module, "kill", 81, 0u, new Func<Thread, Thread>(ThreadOps.Kill));
			LibraryInitializer.DefineLibraryMethod(module, "priority", 81, 0u, new Func<Thread, object>(ThreadOps.Priority));
			LibraryInitializer.DefineLibraryMethod(module, "priority=", 81, 0u, new Func<Thread, int, Thread>(ThreadOps.Priority));
			LibraryInitializer.DefineLibraryMethod(module, "raise", 81, 0u, 2u, 0u, new Action<RubyContext, Thread>(ThreadOps.RaiseException), new Action<Thread, MutableString>(ThreadOps.RaiseException), new Action<RespondToStorage, UnaryOpStorage, BinaryOpStorage, CallSiteStorage<Action<CallSite, Exception, RubyArray>>, Thread, object, object, RubyArray>(ThreadOps.RaiseException));
			LibraryInitializer.DefineLibraryMethod(module, "run", 81, 0u, new Func<Thread, Thread>(ThreadOps.Run));
			LibraryInitializer.DefineLibraryMethod(module, "status", 81, 0u, new Func<Thread, object>(ThreadOps.Status));
			LibraryInitializer.DefineLibraryMethod(module, "stop?", 81, 0u, new Func<Thread, bool>(ThreadOps.IsStopped));
			LibraryInitializer.DefineLibraryMethod(module, "terminate", 81, 0u, new Func<Thread, Thread>(ThreadOps.Kill));
			LibraryInitializer.DefineLibraryMethod(module, "value", 81, 0u, new Func<Thread, object>(ThreadOps.Value));
			LibraryInitializer.DefineLibraryMethod(module, "wakeup", 81, 0u, new Func<Thread, Thread>(ThreadOps.Run));
		}

		private static void LoadThread_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "abort_on_exception", 97, 0u, new Func<object, object>(ThreadOps.GlobalAbortOnException));
			LibraryInitializer.DefineLibraryMethod(module, "abort_on_exception=", 97, 0u, new Func<object, bool, object>(ThreadOps.GlobalAbortOnException));
			LibraryInitializer.DefineLibraryMethod(module, "critical", 97, 0u, new Func<RubyContext, object, bool>(ThreadOps.Critical));
			LibraryInitializer.DefineLibraryMethod(module, "critical=", 97, 0u, new Action<RubyContext, object, bool>(ThreadOps.Critical));
			LibraryInitializer.DefineLibraryMethod(module, "current", 97, 0u, new Func<object, Thread>(ThreadOps.Current));
			LibraryInitializer.DefineLibraryMethod(module, "list", 97, 0u, new Func<object, RubyArray>(ThreadOps.List));
			LibraryInitializer.DefineLibraryMethod(module, "main", 97, 0u, new Func<RubyContext, RubyClass, Thread>(ThreadOps.GetMainThread));
			LibraryInitializer.DefineLibraryMethod(module, "new", 97, 2147483648u, new Func<RubyContext, BlockParam, object, object[], Thread>(ThreadOps.CreateThread));
			LibraryInitializer.DefineLibraryMethod(module, "pass", 97, 0u, new Action<object>(ThreadOps.Yield));
			LibraryInitializer.DefineLibraryMethod(module, "start", 97, 2147483648u, new Func<RubyContext, BlockParam, object, object[], Thread>(ThreadOps.CreateThread));
			LibraryInitializer.DefineLibraryMethod(module, "stop", 97, 0u, new Action<RubyContext, object>(ThreadOps.Stop));
		}

		private static void LoadThreadGroup_Constants(RubyModule module)
		{
			LibraryInitializer.SetBuiltinConstant(module, "Default", ThreadGroup.Default);
		}

		private static void LoadThreadGroup_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "add", 81, 3u, new Func<ThreadGroup, Thread, ThreadGroup>(ThreadGroup.Add));
			LibraryInitializer.DefineLibraryMethod(module, "list", 81, 1u, new Func<ThreadGroup, RubyArray>(ThreadGroup.List));
		}

		private static void LoadTime_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "-", 81, 65536u, 2u, 0u, new Func<RubyTime, double, RubyTime>(RubyTimeOps.SubtractSeconds), new Func<RubyTime, RubyTime, double>(RubyTimeOps.SubtractTime), new Func<RubyTime, DateTime, double>(RubyTimeOps.SubtractTime));
			LibraryInitializer.DefineLibraryMethod(module, "_dump", 81, 0u, new Func<RubyContext, RubyTime, int, MutableString>(RubyTimeOps.Dump));
			LibraryInitializer.DefineLibraryMethod(module, "+", 81, 65536u, 2u, new Func<RubyTime, double, RubyTime>(RubyTimeOps.AddSeconds), new Func<RubyTime, RubyTime, RubyTime>(RubyTimeOps.AddSeconds));
			LibraryInitializer.DefineLibraryMethod(module, "<=>", 81, 2u, 0u, new Func<RubyTime, RubyTime, int>(RubyTimeOps.CompareTo), new Func<RubyTime, object, object>(RubyTimeOps.CompareSeconds));
			LibraryInitializer.DefineLibraryMethod(module, "asctime", 81, 0u, new Func<RubyTime, MutableString>(RubyTimeOps.CTime));
			LibraryInitializer.DefineLibraryMethod(module, "ctime", 81, 0u, new Func<RubyTime, MutableString>(RubyTimeOps.CTime));
			LibraryInitializer.DefineLibraryMethod(module, "day", 81, 0u, new Func<RubyTime, int>(RubyTimeOps.Day));
			LibraryInitializer.DefineLibraryMethod(module, "dst?", 81, 0u, new Func<RubyContext, RubyTime, object>(RubyTimeOps.IsDst));
			LibraryInitializer.DefineLibraryMethod(module, "eql?", 81, 2u, 0u, new Func<RubyTime, RubyTime, bool>(RubyTimeOps.Eql), new Func<RubyTime, object, bool>(RubyTimeOps.Eql));
			LibraryInitializer.DefineLibraryMethod(module, "getgm", 81, 0u, new Func<RubyTime, RubyTime>(RubyTimeOps.GetUTC));
			LibraryInitializer.DefineLibraryMethod(module, "getlocal", 81, 0u, new Func<RubyTime, RubyTime>(RubyTimeOps.ToLocalTime));
			LibraryInitializer.DefineLibraryMethod(module, "getutc", 81, 0u, new Func<RubyTime, RubyTime>(RubyTimeOps.GetUTC));
			LibraryInitializer.DefineLibraryMethod(module, "gmt?", 81, 0u, new Func<RubyTime, bool>(RubyTimeOps.IsUts));
			LibraryInitializer.DefineLibraryMethod(module, "gmt_offset", 81, 0u, new Func<RubyTime, object>(RubyTimeOps.Offset));
			LibraryInitializer.DefineLibraryMethod(module, "gmtime", 81, 0u, new Func<RubyTime, RubyTime>(RubyTimeOps.SwitchToUtc));
			LibraryInitializer.DefineLibraryMethod(module, "gmtoff", 81, 0u, new Func<RubyTime, object>(RubyTimeOps.Offset));
			LibraryInitializer.DefineLibraryMethod(module, "hash", 81, 0u, new Func<RubyTime, int>(RubyTimeOps.GetHash));
			LibraryInitializer.DefineLibraryMethod(module, "hour", 81, 0u, new Func<RubyTime, int>(RubyTimeOps.Hour));
			LibraryInitializer.DefineLibraryMethod(module, "initialize", 82, 0u, new Func<RubyTime, RubyTime>(RubyTimeOps.Reinitialize));
			LibraryInitializer.DefineLibraryMethod(module, "initialize_copy", 82, 2u, new Func<RubyTime, RubyTime, RubyTime>(RubyTimeOps.InitializeCopy));
			LibraryInitializer.DefineLibraryMethod(module, "inspect", 81, 0u, new Func<RubyContext, RubyTime, MutableString>(RubyTimeOps.ToString));
			LibraryInitializer.DefineLibraryMethod(module, "isdst", 81, 0u, new Func<RubyContext, RubyTime, object>(RubyTimeOps.IsDst));
			LibraryInitializer.DefineLibraryMethod(module, "localtime", 81, 0u, new Func<RubyTime, RubyTime>(RubyTimeOps.ToLocalTime));
			LibraryInitializer.DefineLibraryMethod(module, "mday", 81, 0u, new Func<RubyTime, int>(RubyTimeOps.Day));
			LibraryInitializer.DefineLibraryMethod(module, "min", 81, 0u, new Func<RubyTime, int>(RubyTimeOps.Minute));
			LibraryInitializer.DefineLibraryMethod(module, "mon", 81, 0u, new Func<RubyTime, int>(RubyTimeOps.Month));
			LibraryInitializer.DefineLibraryMethod(module, "month", 81, 0u, new Func<RubyTime, int>(RubyTimeOps.Month));
			LibraryInitializer.DefineLibraryMethod(module, "sec", 81, 0u, new Func<RubyTime, int>(RubyTimeOps.Second));
			LibraryInitializer.DefineLibraryMethod(module, "strftime", 81, 131076u, new Func<RubyContext, RubyTime, MutableString, MutableString>(RubyTimeOps.FormatTime));
			LibraryInitializer.DefineLibraryMethod(module, "succ", 81, 0u, new Func<RubyTime, RubyTime>(RubyTimeOps.SuccessiveSecond));
			LibraryInitializer.DefineLibraryMethod(module, "to_a", 81, 0u, new Func<RubyContext, RubyTime, RubyArray>(RubyTimeOps.ToArray));
			LibraryInitializer.DefineLibraryMethod(module, "to_f", 81, 0u, new Func<RubyTime, double>(RubyTimeOps.ToFloatSeconds));
			LibraryInitializer.DefineLibraryMethod(module, "to_i", 81, 0u, new Func<RubyTime, object>(RubyTimeOps.ToSeconds));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<RubyContext, RubyTime, MutableString>(RubyTimeOps.ToString));
			LibraryInitializer.DefineLibraryMethod(module, "tv_sec", 81, 0u, new Func<RubyTime, object>(RubyTimeOps.ToSeconds));
			LibraryInitializer.DefineLibraryMethod(module, "tv_usec", 81, 0u, new Func<RubyTime, int>(RubyTimeOps.GetMicroSeconds));
			LibraryInitializer.DefineLibraryMethod(module, "usec", 81, 0u, new Func<RubyTime, int>(RubyTimeOps.GetMicroSeconds));
			LibraryInitializer.DefineLibraryMethod(module, "utc", 81, 0u, new Func<RubyTime, RubyTime>(RubyTimeOps.SwitchToUtc));
			LibraryInitializer.DefineLibraryMethod(module, "utc?", 81, 0u, new Func<RubyTime, bool>(RubyTimeOps.IsUts));
			LibraryInitializer.DefineLibraryMethod(module, "utc_offset", 81, 0u, new Func<RubyTime, object>(RubyTimeOps.Offset));
			LibraryInitializer.DefineLibraryMethod(module, "wday", 81, 0u, new Func<RubyTime, int>(RubyTimeOps.DayOfWeek));
			LibraryInitializer.DefineLibraryMethod(module, "yday", 81, 0u, new Func<RubyTime, int>(RubyTimeOps.DayOfYear));
			LibraryInitializer.DefineLibraryMethod(module, "year", 81, 0u, new Func<RubyTime, int>(RubyTimeOps.Year));
			LibraryInitializer.DefineLibraryMethod(module, "zone", 81, 0u, new Func<RubyContext, RubyTime, MutableString>(RubyTimeOps.GetZone));
		}

		private static void LoadTime_Class(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "_load", 97, 4u, new Func<RubyContext, RubyClass, MutableString, RubyTime>(RubyTimeOps.Load));
			LibraryInitializer.DefineLibraryMethod(module, "at", 97, 2u, 0u, 0u, new Func<RubyClass, RubyTime, RubyTime>(RubyTimeOps.Create), new Func<RubyClass, double, RubyTime>(RubyTimeOps.Create), new Func<RubyClass, int, int, RubyTime>(RubyTimeOps.Create));
			LibraryInitializer.DefineLibraryMethod(module, "gm", 97, 0u, 2147483648u, new Func<object, int, int, int, int, int, int, int, RubyTime>(RubyTimeOps.CreateGmtTime), new Func<ConversionStorage<int>, ConversionStorage<MutableString>, RubyClass, object[], RubyTime>(RubyTimeOps.CreateGmtTime));
			LibraryInitializer.DefineLibraryMethod(module, "local", 97, 0u, 2147483648u, new Func<object, int, int, int, int, int, int, int, RubyTime>(RubyTimeOps.CreateLocalTime), new Func<ConversionStorage<int>, ConversionStorage<MutableString>, RubyClass, object[], RubyTime>(RubyTimeOps.CreateLocalTime));
			LibraryInitializer.DefineLibraryMethod(module, "mktime", 97, 0u, 2147483648u, new Func<object, int, int, int, int, int, int, int, RubyTime>(RubyTimeOps.CreateLocalTime), new Func<ConversionStorage<int>, ConversionStorage<MutableString>, RubyClass, object[], RubyTime>(RubyTimeOps.CreateLocalTime));
			LibraryInitializer.DefineLibraryMethod(module, "now", 97, 0u, new Func<RubyClass, RubyTime>(RubyTimeOps.Now));
			LibraryInitializer.DefineLibraryMethod(module, "times", 12189793, 0u, new Func<RubyClass, RubyStruct>(RubyTimeOps.Times));
			LibraryInitializer.DefineLibraryMethod(module, "utc", 97, 0u, 2147483648u, new Func<object, int, int, int, int, int, int, int, RubyTime>(RubyTimeOps.CreateGmtTime), new Func<ConversionStorage<int>, ConversionStorage<MutableString>, RubyClass, object[], RubyTime>(RubyTimeOps.CreateGmtTime));
		}

		private static void LoadTrueClass_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "&", 81, 0u, 0u, new Func<bool, object, bool>(TrueClass.And), new Func<bool, bool, bool>(TrueClass.And));
			LibraryInitializer.DefineLibraryMethod(module, "^", 81, 0u, 0u, new Func<bool, object, bool>(TrueClass.Xor), new Func<bool, bool, bool>(TrueClass.Xor));
			LibraryInitializer.DefineLibraryMethod(module, "|", 81, 0u, new Func<bool, object, bool>(TrueClass.Or));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<bool, MutableString>(TrueClass.ToString));
		}

		private static void LoadUnboundMethod_Instance(RubyModule module)
		{
			LibraryInitializer.DefineLibraryMethod(module, "==", 81, 2u, 0u, new Func<UnboundMethod, UnboundMethod, bool>(UnboundMethod.Equal), new Func<UnboundMethod, object, bool>(UnboundMethod.Equal));
			LibraryInitializer.DefineLibraryMethod(module, "arity", 81, 0u, new Func<UnboundMethod, int>(UnboundMethod.GetArity));
			LibraryInitializer.DefineLibraryMethod(module, "bind", 81, 0u, new Func<UnboundMethod, object, RubyMethod>(UnboundMethod.Bind));
			LibraryInitializer.DefineLibraryMethod(module, "clone", 81, 0u, new Func<UnboundMethod, UnboundMethod>(UnboundMethod.Clone));
			LibraryInitializer.DefineLibraryMethod(module, "clr_members", 81, 0u, new Func<UnboundMethod, RubyArray>(UnboundMethod.GetClrMembers));
			LibraryInitializer.DefineLibraryMethod(module, "of", 81, 2147483652u, new Func<RubyContext, UnboundMethod, object[], UnboundMethod>(UnboundMethod.BingGenericParameters));
			LibraryInitializer.DefineLibraryMethod(module, "overload", 81, 2147483652u, new Func<RubyContext, UnboundMethod, object[], UnboundMethod>(UnboundMethod.SelectOverload));
			LibraryInitializer.DefineLibraryMethod(module, "overloads", 81, 2147483652u, new Func<RubyContext, RubyMethod, object[], RubyMethod>(UnboundMethod.SelectOverload_old));
			LibraryInitializer.DefineLibraryMethod(module, "parameters", 81, 0u, new Func<UnboundMethod, RubyArray>(UnboundMethod.GetParameters));
			LibraryInitializer.DefineLibraryMethod(module, "source_location", 81, 0u, new Func<UnboundMethod, RubyArray>(UnboundMethod.GetSourceLocation));
			LibraryInitializer.DefineLibraryMethod(module, "to_s", 81, 0u, new Func<RubyContext, UnboundMethod, MutableString>(UnboundMethod.ToS));
		}

		public static Exception ExceptionFactory__Encoding__ConverterNotFoundError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new ConverterNotFoundError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__Encoding__CompatibilityError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new EncodingCompatibilityError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__EncodingError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new EncodingError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__EOFError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new EOFError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__FloatDomainError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new FloatDomainError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__Interrupt(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new Interrupt(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__Encoding__InvalidByteSequenceError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new InvalidByteSequenceError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__LoadError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new LoadError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__LocalJumpError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new LocalJumpError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__NoMemoryError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new NoMemoryError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__NotImplementedError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new NotImplementedError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__RegexpError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new RegexpError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__RuntimeError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new RuntimeError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__ScriptError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new ScriptError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__SignalException(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new SignalException(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__SyntaxError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new SyntaxError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__SystemExit(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new SystemExit(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__SystemStackError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new SystemStackError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__ThreadError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new ThreadError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__Encoding__UndefinedConversionError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new UndefinedConversionError(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__ArgumentError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new ArgumentException(RubyExceptionData.GetClrMessage(self, message), (Exception)null), message);
		}

		public static Exception ExceptionFactory__RangeError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new ArgumentOutOfRangeException(RubyExceptionData.GetClrMessage(self, message), (Exception)null), message);
		}

		public static Exception ExceptionFactory__ZeroDivisionError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new DivideByZeroException(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__Exception(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new Exception(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__IndexError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new IndexOutOfRangeException(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__TypeError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new InvalidOperationException(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__IOError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new IOException(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__NameError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new MemberAccessException(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__NoMethodError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new MissingMethodException(RubyExceptionData.GetClrMessage(self, message), (Exception)null), message);
		}

		public static Exception ExceptionFactory__SystemCallError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new ExternalException(RubyExceptionData.GetClrMessage(self, message), null), message);
		}

		public static Exception ExceptionFactory__SecurityError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new SecurityException(RubyExceptionData.GetClrMessage(self, message), (Exception)null), message);
		}

		public static Exception ExceptionFactory__StandardError(RubyClass self, object message)
		{
			return RubyExceptionData.InitializeException(new SystemException(RubyExceptionData.GetClrMessage(self, message), null), message);
		}
	}
}
