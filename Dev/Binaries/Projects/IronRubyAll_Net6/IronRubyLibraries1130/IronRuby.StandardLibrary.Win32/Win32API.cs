using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.StandardLibrary.Win32API
{
	[RubyClass("Win32API", BuildConfig = "!SILVERLIGHT")]
	public class Win32API : RubyObject
	{
		private enum ArgType : byte
		{
			None,
			Buffer,
			Int32
		}

		private static int _Version = 1;

		private int _version;

		private IntPtr _function;

		private ArgType[] _signature;

		private ArgType _returnType;

		private MethodInfo _calliStub;

		private readonly RubyContext _context;

		private static readonly PropertyInfo VersionProperty = typeof(Win32API).GetProperty("Version");

		private static readonly PropertyInfo FunctionProperty = typeof(Win32API).GetProperty("Function");

		private static ModuleBuilder _dynamicModule;

		private static readonly object _lock = new object();

		public int Version
		{
			get
			{
				return _version;
			}
		}

		public IntPtr Function
		{
			get
			{
				return _function;
			}
		}

		private static ModuleBuilder DynamicModule
		{
			get
			{
				if (_dynamicModule == null)
				{
					lock (_lock)
					{
						if (_dynamicModule == null)
						{
							CustomAttributeBuilder[] assemblyAttributes = new CustomAttributeBuilder[2]
							{
								new CustomAttributeBuilder(typeof(UnverifiableCodeAttribute).GetConstructor(Type.EmptyTypes), new object[0]),
								new CustomAttributeBuilder(typeof(PermissionSetAttribute).GetConstructor(new Type[1] { typeof(SecurityAction) }), new object[1] { SecurityAction.Demand }, new PropertyInfo[1] { typeof(PermissionSetAttribute).GetProperty("Unrestricted") }, new object[1] { true })
							};
							string text = typeof(Win32API).Namespace + ".DynamicAssembly";
                            //AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(text), AssemblyBuilderAccess.Run, assemblyAttributes);
                            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(text), AssemblyBuilderAccess.Run, assemblyAttributes);
							

                            //assemblyBuilder.DefineVersionInfoResource();
							_dynamicModule = assemblyBuilder.DefineDynamicModule(text);
						}
					}
				}
				return _dynamicModule;
			}
		}

		public Win32API(RubyClass cls)
			: base(cls)
		{
			_context = cls.Context;
			_version = 0;
		}

		private Win32API Reinitialize(IntPtr function, ArgType[] signature, ArgType returnType)
		{
			if (IntPtr.Size != 4)
			{
				throw new NotSupportedException("Win32API is not supported in 64-bit process");
			}
			_function = function;
			_signature = signature;
			_returnType = returnType;
			_version = Interlocked.Increment(ref _Version);
			return this;
		}

		private static ArgType ToArgType(byte b)
		{
			switch (b)
			{
			case 73:
			case 76:
			case 78:
			case 105:
			case 108:
			case 110:
				return ArgType.Int32;
			case 80:
			case 112:
				return ArgType.Buffer;
			default:
				return ArgType.None;
			}
		}

		private static Type ToNativeType(ArgType argType)
		{
			switch (argType)
			{
			case ArgType.Buffer:
				return typeof(byte[]);
			case ArgType.Int32:
				return typeof(int);
			case ArgType.None:
				return typeof(void);
			default:
				throw Assert.Unreachable;
			}
		}

		private static ArgType[] MakeSignature(int size, Func<int, byte> getByte)
		{
			ArgType[] array = new ArgType[size];
			int num = 0;
			for (int i = 0; i < size; i++)
			{
				ArgType argType = ToArgType(getByte(i));
				if (argType != 0)
				{
					array[num++] = argType;
				}
			}
			if (num != array.Length)
			{
				Array.Resize(ref array, num);
			}
			return array;
		}

		[DllImport("kernel32.dll")]
		private static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetProcAddress(IntPtr module, string lpProcName);

		private static IntPtr GetFunction(MutableString libraryName, MutableString functionName)
		{
			IntPtr intPtr = LoadLibrary(libraryName.ConvertToString());
			if (intPtr == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
			string text = functionName.ConvertToString();
			IntPtr procAddress = GetProcAddress(intPtr, text);
			if (procAddress == IntPtr.Zero)
			{
				procAddress = GetProcAddress(intPtr, text + "A");
				if (intPtr == IntPtr.Zero)
				{
					throw new Win32Exception();
				}
			}
			return procAddress;
		}

		[RubyConstructor]
		public static Win32API Create(RubyClass self, [NotNull][DefaultProtocol] MutableString libraryName, [DefaultProtocol][NotNull] MutableString functionName, [NotNull][DefaultProtocol] MutableString parameterTypes, [DefaultProtocol][NotNull] MutableString returnType)
		{
			return Reinitialize(new Win32API(self), libraryName, functionName, parameterTypes, returnType);
		}

		[RubyConstructor(Compatibility = RubyCompatibility.Default)]
		public static Win32API Create(RubyClass self, [NotNull][DefaultProtocol] MutableString libraryName, [DefaultProtocol][NotNull] MutableString functionName, [DefaultProtocol][NotNull] MutableString parameterTypes, [DefaultProtocol][NotNull] MutableString returnType, RubySymbol callingConvention)
		{
			return Reinitialize(new Win32API(self), libraryName, functionName, parameterTypes, returnType);
		}

		[RubyConstructor]
		public static Win32API Create(ConversionStorage<MutableString> toStr, RubyClass self, [NotNull][DefaultProtocol] MutableString libraryName, [DefaultProtocol][NotNull] MutableString functionName, [NotNull] IList parameterTypes, [DefaultProtocol][NotNull] MutableString returnType)
		{
			return Reinitialize(toStr, new Win32API(self), libraryName, functionName, parameterTypes, returnType);
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static Win32API Reinitialize(Win32API self, [DefaultProtocol][NotNull] MutableString libraryName, [NotNull][DefaultProtocol] MutableString functionName, [DefaultProtocol][NotNull] MutableString parameterTypes, [DefaultProtocol][NotNull] MutableString returnType)
		{
			return self.Reinitialize(GetFunction(libraryName, functionName), MakeSignature(parameterTypes.GetByteCount(), parameterTypes.GetByte), (!returnType.IsEmpty) ? ToArgType(returnType.GetByte(0)) : ArgType.None);
		}

		[RubyMethod("initialize", RubyMethodAttributes.PrivateInstance)]
		public static Win32API Reinitialize(ConversionStorage<MutableString> toStr, Win32API self, [DefaultProtocol][NotNull] MutableString libraryName, [DefaultProtocol][NotNull] MutableString functionName, [NotNull] IList parameterTypes, [NotNull][DefaultProtocol] MutableString returnType)
		{
			return self.Reinitialize(GetFunction(libraryName, functionName), MakeSignature(parameterTypes.Count, delegate(int i)
			{
				MutableString mutableString = Protocols.CastToString(toStr, parameterTypes[i]);
				return (byte)((!mutableString.IsEmpty) ? mutableString.GetByte(0) : 0);
			}), (!returnType.IsEmpty) ? ToArgType(returnType.GetByte(0)) : ArgType.None);
		}

		[RubyMethod("Call")]
		[RubyMethod("call")]
		public static RuleGenerator Call()
		{
			return delegate(MetaObjectBuilder metaBuilder, CallArguments args, string name)
			{
				((Win32API)args.Target).BuildCall(metaBuilder, args, name);
			};
		}

		private void BuildCall(MetaObjectBuilder metaBuilder, CallArguments args, string name)
		{
			IList<DynamicMetaObject> list = RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 0, int.MaxValue);
			if (metaBuilder.Error)
			{
				return;
			}
			metaBuilder.AddRestriction(Expression.Equal(Expression.Property(Expression.Convert(args.TargetExpression, typeof(Win32API)), VersionProperty), Expression.Constant(_version)));
			if (_function == IntPtr.Zero)
			{
				metaBuilder.SetError(Expression.Throw(new Func<Exception>(UninitializedFunctionError).Method.OpCall(), typeof(object)));
				return;
			}
			if (_signature.Length != list.Count)
			{
				metaBuilder.SetError(Expression.Throw(new Func<int, int, Exception>(InvalidParameterCountError).Method.OpCall(Expression.Constant(_signature.Length), Expression.Constant(list.Count)), typeof(object)));
				return;
			}
			ReadOnlyCollectionBuilder<Expression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<Expression>();
			readOnlyCollectionBuilder.Add(Expression.Property(Expression.Convert(args.TargetExpression, typeof(Win32API)), FunctionProperty));
			for (int i = 0; i < list.Count; i++)
			{
				readOnlyCollectionBuilder.Add(MarshalArgument(metaBuilder, list[i], _signature[i]));
			}
			metaBuilder.Result = Expression.Call(EmitCalliStub(), readOnlyCollectionBuilder);
			if (_returnType == ArgType.None)
			{
				metaBuilder.Result = Expression.Block(metaBuilder.Result, Microsoft.Scripting.Ast.Utils.Constant(0));
			}
		}

		private Expression MarshalArgument(MetaObjectBuilder metaBuilder, DynamicMetaObject arg, ArgType parameterType)
		{
			object value = arg.Value;
			if (value == null)
			{
				metaBuilder.AddRestriction(Expression.Equal(arg.Expression, Microsoft.Scripting.Ast.Utils.Constant(null)));
			}
			else
			{
				metaBuilder.AddTypeRestriction(value.GetType(), arg.Expression);
			}
			switch (parameterType)
			{
			case ArgType.Buffer:
				if (value == null)
				{
					return Microsoft.Scripting.Ast.Utils.Constant(null, typeof(byte[]));
				}
				if (value is int && (int)value == 0)
				{
					metaBuilder.AddRestriction(Expression.Equal(Microsoft.Scripting.Ast.Utils.Convert(arg.Expression, typeof(int)), Microsoft.Scripting.Ast.Utils.Constant(0)));
					return Microsoft.Scripting.Ast.Utils.Constant(null, typeof(byte[]));
				}
				if (value.GetType() == typeof(MutableString))
				{
					return Methods.GetMutableStringBytes.OpCall(Microsoft.Scripting.Ast.Utils.Convert(arg.Expression, typeof(MutableString)));
				}
				return Methods.GetMutableStringBytes.OpCall(Microsoft.Scripting.Ast.Utils.LightDynamic(ProtocolConversionAction<ConvertToStrAction>.Make(_context), typeof(MutableString), arg.Expression));
			case ArgType.Int32:
				if (value is int)
				{
					return Microsoft.Scripting.Ast.Utils.Convert(arg.Expression, typeof(int));
				}
				return Expression.Convert(Expression.Call(Microsoft.Scripting.Ast.Utils.LightDynamic(ProtocolConversionAction<ConvertToIntAction>.Make(_context), typeof(IntegerValue), arg.Expression), Methods.IntegerValue_ToUInt32Unchecked), typeof(int));
			default:
				throw Assert.Unreachable;
			}
		}

		public static Exception UninitializedFunctionError()
		{
			return RubyExceptions.CreateRuntimeError("uninitialized Win32 function");
		}

		public static Exception InvalidParameterCountError(int expected, int actual)
		{
			return RubyExceptions.CreateRuntimeError("wrong number of parameters: expected {0}, got {1}", expected, actual);
		}

		private MethodInfo EmitCalliStub()
		{
			if (_calliStub != null)
			{
				return _calliStub;
			}
			Type returnType = ToNativeType(_returnType);
			Type[] array = new Type[1 + _signature.Length];
			array[0] = typeof(IntPtr);
			for (int i = 0; i < _signature.Length; i++)
			{
				array[1 + i] = ToNativeType(_signature[i]);
			}
			DynamicMethod dynamicMethod = new DynamicMethod("calli", returnType, array, DynamicModule);
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			SignatureHelper methodSigHelper = SignatureHelper.GetMethodSigHelper(System.Reflection.CallingConventions.Standard, returnType);
			for (int j = 1; j < array.Length; j++)
			{
				iLGenerator.Emit(OpCodes.Ldarg, j);
				methodSigHelper.AddArgument(array[j]);
			}
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Calli, methodSigHelper);
			iLGenerator.Emit(OpCodes.Ret);
			return _calliStub = dynamicMethod;
		}
	}
}
