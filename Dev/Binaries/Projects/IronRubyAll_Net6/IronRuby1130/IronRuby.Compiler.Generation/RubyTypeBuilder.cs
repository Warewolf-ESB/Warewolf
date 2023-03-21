using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Generation
{
	internal class RubyTypeBuilder : IFeatureBuilder
	{
		private enum SignatureAdjustment
		{
			None,
			ConvertClassToContext,
			InsertClass
		}

		private sealed class ConstructorBuilderInfo
		{
			public ConstructorInfo BaseCtor;

			public ParameterInfo[] BaseParameters;

			public Type[] ParameterTypes;

			public int ContextArgIndex;

			public int ClassParamIndex;

			public SignatureAdjustment Adjustment;
		}

		protected readonly TypeBuilder _tb;

		private RubyTypeEmitter _emitter;

		private static readonly Type[] _deserializerSignature = new Type[2]
		{
			typeof(SerializationInfo),
			typeof(StreamingContext)
		};

		private static readonly Type[] _exceptionMessageSignature = new Type[1] { typeof(string) };

		internal bool IsDerivedRubyType
		{
			get
			{
				return _emitter.ImmediateClassField == null;
			}
		}

		internal FieldBuilder ImmediateClassField
		{
			get
			{
				return _emitter.ImmediateClassField;
			}
		}

		internal FieldBuilder InstanceDataField
		{
			get
			{
				return _emitter.InstanceDataField;
			}
		}

		internal RubyTypeBuilder(TypeBuilder tb)
		{
			_tb = tb;
		}

		public void Implement(ClsTypeEmitter emitter)
		{
			_emitter = (RubyTypeEmitter)emitter;
			DefineConstructors();
			if (!IsDerivedRubyType)
			{
				DefineRubyObjectImplementation();
				DefineSerializer();
				DefineDynamicObjectImplementation();
				DefineCustomTypeDescriptor();
				DefineRubyTypeImplementation();
			}
		}

		private static bool IsAvailable(MethodBase method)
		{
			if (method != null && !method.IsPrivate && !method.IsAssembly)
			{
				return !method.IsFamilyAndAssembly;
			}
			return false;
		}

		private void DefineConstructors()
		{
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			List<ConstructorBuilderInfo> ctors = new List<ConstructorBuilderInfo>();
			ConstructorInfo[] constructors = _tb.BaseType.GetConstructors(bindingAttr);
			foreach (ConstructorInfo constructorInfo in constructors)
			{
				if (constructorInfo.IsPublic || constructorInfo.IsProtected())
				{
					ParameterInfo[] parameters = constructorInfo.GetParameters();
					if (parameters.Length == 2 && parameters[0].ParameterType == typeof(SerializationInfo) && parameters[1].ParameterType == typeof(StreamingContext))
					{
						OverrideDeserializer(constructorInfo);
					}
					else
					{
						AddConstructor(ctors, MakeConstructor(constructorInfo, parameters));
					}
				}
			}
			BuildConstructors(ctors);
		}

		private static void AddConstructor(List<ConstructorBuilderInfo> ctors, ConstructorBuilderInfo ctor)
		{
			int num = ctors.FindIndex((ConstructorBuilderInfo c) => c.ParameterTypes.ValueEquals(ctor.ParameterTypes));
			if (num != -1)
			{
				if (ctors[num].Adjustment > ctor.Adjustment)
				{
					ctors[num] = ctor;
				}
			}
			else
			{
				ctors.Add(ctor);
			}
		}

		private ConstructorBuilderInfo MakeConstructor(ConstructorInfo baseCtor, ParameterInfo[] baseParams)
		{
			int num = -1;
			int num2 = -1;
			for (int i = 0; i < baseParams.Length; i++)
			{
				if (baseParams[i].ParameterType == typeof(RubyContext))
				{
					num = i;
					break;
				}
				if (baseParams[i].ParameterType == typeof(RubyClass))
				{
					num2 = i;
					break;
				}
			}
			SignatureAdjustment signatureAdjustment;
			int num3;
			if (num2 == -1)
			{
				if (num == -1)
				{
					signatureAdjustment = SignatureAdjustment.InsertClass;
					num3 = 0;
				}
				else
				{
					signatureAdjustment = SignatureAdjustment.ConvertClassToContext;
					num3 = num;
				}
			}
			else
			{
				signatureAdjustment = SignatureAdjustment.None;
				num3 = num2;
			}
			Type[] array = new Type[((signatureAdjustment == SignatureAdjustment.InsertClass) ? 1 : 0) + baseParams.Length];
			int num4 = 0;
			int num5 = 0;
			if (signatureAdjustment == SignatureAdjustment.InsertClass)
			{
				num4++;
			}
			while (num4 < array.Length)
			{
				array[num4++] = baseParams[num5++].ParameterType;
			}
			array[num3] = typeof(RubyClass);
			ConstructorBuilderInfo constructorBuilderInfo = new ConstructorBuilderInfo();
			constructorBuilderInfo.BaseCtor = baseCtor;
			constructorBuilderInfo.BaseParameters = baseParams;
			constructorBuilderInfo.ParameterTypes = array;
			constructorBuilderInfo.ContextArgIndex = num;
			constructorBuilderInfo.ClassParamIndex = num3;
			constructorBuilderInfo.Adjustment = signatureAdjustment;
			return constructorBuilderInfo;
		}

		private void BuildConstructors(IList<ConstructorBuilderInfo> ctors)
		{
			foreach (ConstructorBuilderInfo ctor in ctors)
			{
				ConstructorBuilder constructorBuilder = _tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, ctor.ParameterTypes);
				ILGen iLGen = new ILGen(constructorBuilder.GetILGenerator());
				int i = 0;
				int num = 0;
				if (!IsDerivedRubyType)
				{
					iLGen.EmitLoadArg(0);
					iLGen.EmitLoadArg(1 + ctor.ClassParamIndex);
					iLGen.EmitFieldSet(ImmediateClassField);
				}
				iLGen.EmitLoadArg(0);
				ConstructorInfo constructor;
				if (ctor.ParameterTypes.Length == 1 && ctor.Adjustment == SignatureAdjustment.InsertClass && _tb.IsSubclassOf(typeof(Exception)) && IsAvailable(constructor = _tb.BaseType.GetConstructor(_exceptionMessageSignature)))
				{
					iLGen.EmitLoadArg(1);
					iLGen.EmitCall(Methods.GetDefaultExceptionMessage);
					iLGen.Emit(OpCodes.Call, constructor);
				}
				else
				{
					if (ctor.Adjustment == SignatureAdjustment.InsertClass)
					{
						i++;
					}
					for (; i < ctor.ParameterTypes.Length; i++)
					{
						if (ctor.Adjustment == SignatureAdjustment.ConvertClassToContext && num == ctor.ContextArgIndex)
						{
							iLGen.EmitLoadArg(1 + ctor.ClassParamIndex);
							iLGen.EmitCall(Methods.GetContextFromModule);
						}
						else
						{
							ClsTypeEmitter.DefineParameterCopy(constructorBuilder, i, ctor.BaseParameters[num]);
							iLGen.EmitLoadArg(1 + i);
						}
						num++;
					}
					iLGen.Emit(OpCodes.Call, ctor.BaseCtor);
				}
				iLGen.Emit(OpCodes.Ret);
			}
		}

		private void OverrideDeserializer(ConstructorInfo baseCtor)
		{
			ConstructorBuilder constructorBuilder = _tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, _deserializerSignature);
			ILGen iLGen = new ILGen(constructorBuilder.GetILGenerator());
			iLGen.EmitLoadArg(0);
			iLGen.EmitLoadArg(1);
			iLGen.EmitLoadArg(2);
			iLGen.Emit(OpCodes.Call, baseCtor);
			iLGen.EmitLoadArg(0);
			iLGen.EmitFieldAddress(InstanceDataField);
			iLGen.EmitLoadArg(0);
			iLGen.EmitFieldAddress(ImmediateClassField);
			iLGen.EmitLoadArg(1);
			iLGen.EmitCall(Methods.DeserializeObject);
			iLGen.Emit(OpCodes.Ret);
		}

		private void DefineRubyObjectImplementation()
		{
			_tb.AddInterfaceImplementation(typeof(IRubyObject));
			_tb.SetCustomAttribute(new CustomAttributeBuilder(typeof(DebuggerTypeProxyAttribute).GetConstructor(new Type[1] { typeof(Type) }), new Type[1] { typeof(RubyObjectDebugView) }));
			_tb.SetCustomAttribute(new CustomAttributeBuilder(typeof(DebuggerDisplayAttribute).GetConstructor(new Type[1] { typeof(string) }), new string[1] { "{_immediateClass.GetDebuggerDisplayValue(this),nq}" }, new PropertyInfo[1] { typeof(DebuggerDisplayAttribute).GetProperty("Type") }, new string[1] { "{_immediateClass.GetDebuggerDisplayType(),nq}" }));
			ILGen iLGen = DefineMethodOverride(_tb, Methods.IRubyObject_get_ImmediateClass);
			iLGen.EmitLoadArg(0);
			iLGen.EmitFieldGet(ImmediateClassField);
			iLGen.Emit(OpCodes.Ret);
			iLGen = DefinePrivateInterfaceMethodOverride(_tb, Methods.IRubyObject_set_ImmediateClass);
			iLGen.EmitLoadArg(0);
			iLGen.EmitLoadArg(1);
			iLGen.EmitFieldSet(ImmediateClassField);
			iLGen.Emit(OpCodes.Ret);
			iLGen = DefinePrivateInterfaceMethodOverride(_tb, Methods.IRubyObject_TryGetInstanceData);
			iLGen.EmitLoadArg(0);
			iLGen.EmitFieldGet(InstanceDataField);
			iLGen.Emit(OpCodes.Ret);
			iLGen = DefinePrivateInterfaceMethodOverride(_tb, Methods.IRubyObject_GetInstanceData);
			iLGen.EmitLoadArg(0);
			iLGen.EmitFieldAddress(InstanceDataField);
			iLGen.EmitCall(Methods.GetInstanceData);
			iLGen.Emit(OpCodes.Ret);
			iLGen = DefinePrivateInterfaceMethodOverride(_tb, Methods.IRubyObjectState_get_IsFrozen);
			iLGen.EmitLoadArg(0);
			iLGen.EmitFieldGet(InstanceDataField);
			iLGen.EmitCall(Methods.IsObjectFrozen);
			iLGen.Emit(OpCodes.Ret);
			iLGen = DefinePrivateInterfaceMethodOverride(_tb, Methods.IRubyObjectState_Freeze);
			iLGen.EmitLoadArg(0);
			iLGen.EmitFieldAddress(InstanceDataField);
			iLGen.EmitCall(Methods.FreezeObject);
			iLGen.Emit(OpCodes.Ret);
			iLGen = DefinePrivateInterfaceMethodOverride(_tb, Methods.IRubyObjectState_get_IsTainted);
			iLGen.EmitLoadArg(0);
			iLGen.EmitFieldGet(InstanceDataField);
			iLGen.EmitCall(Methods.IsObjectTainted);
			iLGen.Emit(OpCodes.Ret);
			iLGen = DefinePrivateInterfaceMethodOverride(_tb, Methods.IRubyObjectState_set_IsTainted);
			iLGen.EmitLoadArg(0);
			iLGen.EmitFieldAddress(InstanceDataField);
			iLGen.EmitLoadArg(1);
			iLGen.EmitCall(Methods.SetObjectTaint);
			iLGen.Emit(OpCodes.Ret);
			iLGen = DefinePrivateInterfaceMethodOverride(_tb, Methods.IRubyObjectState_get_IsUntrusted);
			iLGen.EmitLoadArg(0);
			iLGen.EmitFieldGet(InstanceDataField);
			iLGen.EmitCall(Methods.IsObjectUntrusted);
			iLGen.Emit(OpCodes.Ret);
			iLGen = DefinePrivateInterfaceMethodOverride(_tb, Methods.IRubyObjectState_set_IsUntrusted);
			iLGen.EmitLoadArg(0);
			iLGen.EmitFieldAddress(InstanceDataField);
			iLGen.EmitLoadArg(1);
			iLGen.EmitCall(Methods.SetObjectTrustiness);
			iLGen.Emit(OpCodes.Ret);
			iLGen = DefinePrivateInterfaceMethodOverride(_tb, Methods.IRubyObject_BaseGetHashCode);
			iLGen.EmitLoadArg(0);
			iLGen.EmitCall(_tb.BaseType.GetMethod("GetHashCode", Type.EmptyTypes));
			iLGen.Emit(OpCodes.Ret);
			iLGen = DefinePrivateInterfaceMethodOverride(_tb, Methods.IRubyObject_BaseEquals);
			iLGen.EmitLoadArg(0);
			iLGen.EmitLoadArg(1);
			iLGen.EmitCall(_tb.BaseType.GetMethod("Equals", new Type[1] { typeof(object) }));
			iLGen.Emit(OpCodes.Ret);
			iLGen = DefinePrivateInterfaceMethodOverride(_tb, Methods.IRubyObject_BaseToString);
			iLGen.EmitLoadArg(0);
			MethodInfo method = _tb.BaseType.GetMethod("ToString", Type.EmptyTypes);
			if (method.DeclaringType == typeof(object))
			{
				iLGen.EmitCall(Methods.ObjectToString);
			}
			else
			{
				iLGen.EmitCall(method);
			}
			iLGen.Emit(OpCodes.Ret);
		}

		private void DefineDynamicObjectImplementation()
		{
			_tb.AddInterfaceImplementation(typeof(IRubyDynamicMetaObjectProvider));
			ILGen iLGen = DefinePrivateInterfaceMethodOverride(_tb, typeof(IDynamicMetaObjectProvider).GetMethod("GetMetaObject"));
			iLGen.EmitLoadArg(0);
			iLGen.EmitLoadArg(1);
			iLGen.EmitCall(Methods.GetMetaObject);
			iLGen.Emit(OpCodes.Ret);
		}

		private void DefineRubyTypeImplementation()
		{
			_tb.AddInterfaceImplementation(typeof(IRubyType));
		}

		private void DefineSerializer()
		{
			_tb.AddInterfaceImplementation(typeof(ISerializable));
			MethodInfo methodInfo = ((!typeof(ISerializable).IsAssignableFrom(_tb.BaseType)) ? null : _tb.BaseType.GetInterfaceMap(typeof(ISerializable)).TargetMethods[0]);
			ILGen iLGen = DefinePrivateInterfaceMethodOverride(_tb, typeof(ISerializable).GetMethod("GetObjectData"));
			if (methodInfo != null)
			{
				iLGen.EmitLoadArg(0);
				iLGen.EmitLoadArg(1);
				iLGen.EmitLoadArg(2);
				iLGen.Emit(OpCodes.Call, methodInfo);
			}
			iLGen.EmitLoadArg(0);
			iLGen.EmitFieldGet(InstanceDataField);
			iLGen.EmitLoadArg(0);
			iLGen.EmitFieldGet(ImmediateClassField);
			iLGen.EmitLoadArg(1);
			iLGen.EmitCall(Methods.SerializeObject);
			iLGen.Emit(OpCodes.Ret);
		}

		private void DefineCustomTypeDescriptor()
		{
			_tb.AddInterfaceImplementation(typeof(ICustomTypeDescriptor));
			MethodInfo[] methods = typeof(ICustomTypeDescriptor).GetMethods();
			foreach (MethodInfo m in methods)
			{
				ImplementCTDOverride(m);
			}
		}

		private void ImplementCTDOverride(MethodInfo m)
		{
			MethodBuilder impl;
			ILGen iLGen = DefinePrivateInterfaceMethodOverride(_tb, m, out impl);
			iLGen.EmitLoadArg(0);
			ParameterInfo[] parameters = m.GetParameters();
			Type[] array = new Type[parameters.Length + 1];
			array[0] = typeof(object);
			for (int i = 0; i < parameters.Length; i++)
			{
				iLGen.EmitLoadArg(i + 1);
				array[i + 1] = parameters[i].ParameterType;
			}
			iLGen.EmitCall(typeof(CustomTypeDescHelpers), m.Name, array);
			iLGen.EmitBoxing(m.ReturnType);
			iLGen.Emit(OpCodes.Ret);
		}

		private static ILGen DefineMethodOverride(TypeBuilder tb, MethodInfo decl)
		{
			MethodBuilder impl;
			return DefineMethodOverride(tb, decl, out impl);
		}

		private static ILGen DefineMethodOverride(TypeBuilder tb, MethodInfo decl, out MethodBuilder impl)
		{
			impl = tb.DefineMethod(decl.Name, decl.Attributes & ~(MethodAttributes.ReservedMask | MethodAttributes.Abstract), decl.ReturnType, ReflectionUtils.GetParameterTypes(decl.GetParameters()));
			tb.DefineMethodOverride(impl, decl);
			return new ILGen(impl.GetILGenerator());
		}

		private static ILGen DefinePrivateInterfaceMethodOverride(TypeBuilder tb, MethodInfo decl)
		{
			MethodBuilder impl;
			return DefinePrivateInterfaceMethodOverride(tb, decl, out impl);
		}

		private static ILGen DefinePrivateInterfaceMethodOverride(TypeBuilder tb, MethodInfo decl, out MethodBuilder impl)
		{
			string name = decl.DeclaringType.Name + "." + decl.Name;
			MethodAttributes methodAttributes = decl.Attributes & ~(MethodAttributes.Public | MethodAttributes.Abstract);
			methodAttributes |= MethodAttributes.Final | MethodAttributes.VtableLayoutMask;
			impl = tb.DefineMethod(name, methodAttributes, decl.ReturnType, ReflectionUtils.GetParameterTypes(decl.GetParameters()));
			tb.DefineMethodOverride(impl, decl);
			return new ILGen(impl.GetILGenerator());
		}
	}
}
