using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Generation
{
	public abstract class ClsTypeEmitter
	{
		public const string VtableNamesField = "#VTableNames#";

		public const string BaseMethodPrefix = "#base#";

		public const string FieldGetterPrefix = "#field_get#";

		public const string FieldSetterPrefix = "#field_set#";

		protected const MethodAttributes MethodAttributesToEraseInOveride = MethodAttributes.ReservedMask | MethodAttributes.Abstract;

		private ILGen _cctor;

		private readonly TypeBuilder _tb;

		private readonly Type _baseType;

		private int _site;

		private readonly List<Expression> _dynamicSiteFactories;

		protected abstract Type ContextType { get; }

		protected Type BaseType
		{
			get
			{
				return _baseType;
			}
		}

		protected ClsTypeEmitter(TypeBuilder tb)
		{
			_tb = tb;
			_baseType = tb.BaseType;
			_dynamicSiteFactories = new List<Expression>();
		}

		private static bool CanOverrideMethod(MethodInfo mi)
		{
			return true;
		}

		protected abstract MethodInfo EventHelper();

		protected abstract MethodInfo MissingInvokeMethodException();

		protected abstract void EmitImplicitContext(ILGen il);

		protected abstract void EmitMakeCallAction(string name, int nargs, bool isList);

		protected abstract FieldInfo GetConversionSiteField(Type toType);

		protected abstract MethodInfo GetGenericConversionSiteFactory(Type toType);

		protected abstract void EmitClassObjectFromInstance(ILGen il);

		protected abstract Type[] MakeSiteSignature(int nargs);

		protected ILGen GetCCtor()
		{
			if (_cctor == null)
			{
				ConstructorBuilder constructorBuilder = _tb.DefineTypeInitializer();
				_cctor = CreateILGen(constructorBuilder.GetILGenerator());
			}
			return _cctor;
		}

		internal void OverrideMethods(Type type)
		{
			Dictionary<Key<string, MethodSignatureInfo>, MethodInfo> dictionary = new Dictionary<Key<string, MethodSignatureInfo>, MethodInfo>();
			object[] customAttributes = type.GetCustomAttributes(typeof(DefaultMemberAttribute), false);
			string text;
			string text2;
			if (customAttributes.Length == 1)
			{
				string memberName = ((DefaultMemberAttribute)customAttributes[0]).MemberName;
				text = "get_" + memberName;
				text2 = "set_" + memberName;
			}
			else
			{
				text = (text2 = null);
			}
			MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			MethodInfo[] array = methods;
			foreach (MethodInfo methodInfo in array)
			{
				Key<string, MethodSignatureInfo> key = Key.Create(methodInfo.Name, new MethodSignatureInfo(methodInfo));
				MethodInfo value;
				if (!dictionary.TryGetValue(key, out value))
				{
					dictionary[key] = methodInfo;
				}
				else if (value.DeclaringType.IsAssignableFrom(methodInfo.DeclaringType))
				{
					dictionary[key] = methodInfo;
				}
			}
			foreach (MethodInfo value2 in dictionary.Values)
			{
				if ((value2.DeclaringType == typeof(object) && value2.Name == "Finalize") || (!value2.IsPublic && !value2.IsProtected()))
				{
					continue;
				}
				if (value2.IsVirtual && !value2.IsFinal)
				{
					if (!CanOverrideMethod(value2))
					{
						continue;
					}
					string text3 = value2.Name;
					if (value2.IsSpecialName)
					{
						if (text3 == text)
						{
							text3 = "[]";
						}
						else if (text3 == text2)
						{
							text3 = "[]=";
						}
						else if (text3.StartsWith("get_", StringComparison.Ordinal))
						{
							text3 = text3.Substring(4);
						}
						else if (text3.StartsWith("set_", StringComparison.Ordinal))
						{
							text3 = text3.Substring(4) + "=";
						}
					}
					CreateVTableMethodOverride(value2, text3);
					CreateSuperCallHelper(value2);
				}
				else if (value2.IsProtected())
				{
					CreateSuperCallHelper(value2);
				}
			}
		}

		private void EmitBaseMethodDispatch(MethodInfo mi, ILGen il)
		{
			if (!mi.IsAbstract)
			{
				int num = 0;
				if (!mi.IsStatic)
				{
					il.EmitLoadArg(0);
					num = 1;
				}
				ParameterInfo[] parameters = mi.GetParameters();
				for (int i = 0; i < parameters.Length; i++)
				{
					il.EmitLoadArg(i + num);
				}
				il.EmitCall(OpCodes.Call, mi, null);
				il.Emit(OpCodes.Ret);
			}
			else
			{
				il.EmitLoadArg(0);
				il.EmitString(mi.Name);
				il.EmitCall(MissingInvokeMethodException());
				il.Emit(OpCodes.Throw);
			}
		}

		public FieldInfo AllocateDynamicSite(Type[] signature, Func<FieldInfo, Expression> factory)
		{
			FieldInfo fieldInfo = _tb.DefineField("site$" + _site++, CompilerHelpers.MakeCallSiteType(signature), FieldAttributes.Private | FieldAttributes.Static);
			_dynamicSiteFactories.Add(factory(fieldInfo));
			return fieldInfo;
		}

		public void EmitConvertFromObject(ILGen il, Type toType)
		{
			if (toType == typeof(object))
			{
				return;
			}
			if (toType == typeof(void))
			{
				il.Emit(OpCodes.Pop);
				return;
			}
			LocalBuilder local = il.DeclareLocal(typeof(object));
			il.Emit(OpCodes.Stloc, local);
			if (toType.IsGenericParameter && toType.DeclaringMethod != null)
			{
				MethodInfo genericConversionSiteFactory = GetGenericConversionSiteFactory(toType);
				LocalBuilder localBuilder = il.DeclareLocal(genericConversionSiteFactory.ReturnType);
				il.Emit(OpCodes.Call, genericConversionSiteFactory);
				il.Emit(OpCodes.Stloc, localBuilder);
				il.Emit(OpCodes.Ldloc, localBuilder);
				FieldInfo field = localBuilder.LocalType.GetField("Target");
				il.EmitFieldGet(field);
				il.Emit(OpCodes.Ldloc, localBuilder);
				EmitContext(il, false);
				il.Emit(OpCodes.Ldloc, local);
				il.EmitCall(field.FieldType, "Invoke");
			}
			else
			{
				FieldInfo conversionSiteField = GetConversionSiteField(toType);
				il.EmitFieldGet(conversionSiteField);
				FieldInfo field2 = conversionSiteField.FieldType.GetField("Target");
				il.EmitFieldGet(field2);
				il.EmitFieldGet(conversionSiteField);
				EmitContext(il, false);
				il.Emit(OpCodes.Ldloc, local);
				il.EmitCall(field2.FieldType, "Invoke");
			}
		}

		private MethodBuilder CreateVTableMethodOverride(MethodInfo mi, string name)
		{
			MethodBuilder impl;
			ILGen il = DefineMethodOverride(MethodAttributes.Public, mi, out impl);
			EmitVirtualSiteCall(il, mi, name);
			_tb.DefineMethodOverride(impl, mi);
			return impl;
		}

		public void EmitVirtualSiteCall(ILGen il, MethodInfo mi, string name)
		{
			Label label = il.DefineLabel();
			LocalBuilder local = il.DeclareLocal(typeof(object));
			EmitClrCallStub(il, mi, name);
			il.Emit(OpCodes.Stloc, local);
			il.Emit(OpCodes.Ldloc, local);
			il.Emit(OpCodes.Ldsfld, Fields.ForwardToBase);
			il.Emit(OpCodes.Ceq);
			il.Emit(OpCodes.Brtrue, label);
			if (mi.ReturnType != typeof(void))
			{
				il.Emit(OpCodes.Ldloc, local);
				EmitConvertFromObject(il, mi.ReturnType);
			}
			il.Emit(OpCodes.Ret);
			il.MarkLabel(label);
			EmitBaseMethodDispatch(mi, il);
		}

		private MethodBuilder CreateSuperCallHelper(MethodInfo mi)
		{
			MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName;
			if (mi.IsStatic)
			{
				methodAttributes |= MethodAttributes.Static;
			}
			MethodBuilder methodBuilder = _tb.DefineMethod("#base#" + mi.Name, methodAttributes, mi.CallingConvention);
			ReflectionUtils.CopyMethodSignature(mi, methodBuilder, true);
			EmitBaseMethodDispatch(mi, CreateILGen(methodBuilder.GetILGenerator()));
			return methodBuilder;
		}

		public Type FinishType()
		{
			if (_dynamicSiteFactories.Count > 0)
			{
				GetCCtor();
			}
			if (_cctor != null)
			{
				if (_dynamicSiteFactories.Count > 0)
				{
					MethodBuilder methodBuilder = _tb.DefineMethod("<create_dynamic_sites>", MethodAttributes.Private | MethodAttributes.Static, typeof(void), Type.EmptyTypes);
					_dynamicSiteFactories.Add(Expression.Empty());
					//Expression.Lambda(Expression.Block(_dynamicSiteFactories)).CompileToMethod(methodBuilder);

					//throw new PlatformNotSupportedException("Expression.Lambda(Expression.Block(_dynamicSiteFactories)).CompileToMethod(methodBuilder)");
					//_cctor.EmitCall(methodBuilder);
					_dynamicSiteFactories.Clear();
				}
				_cctor.Emit(OpCodes.Ret);
			}
			var finishedType = _tb.CreateType();
			//var mb = _tb.GetDeclaredMethod("<create_dynamic_sites>");
			//if (mb != null)
			//{
			//	mb.
			//}
			return finishedType;

        }

		protected internal ILGen CreateILGen(ILGenerator il)
		{
			return new ILGen(il);
		}

		protected ILGen DefineMethodOverride(MethodAttributes extra, MethodInfo decl, out MethodBuilder impl)
		{
			impl = ReflectionUtils.DefineMethodOverride(_tb, extra, decl);
			return CreateILGen(impl.GetILGenerator());
		}

		public static ParameterBuilder DefineParameterCopy(ConstructorBuilder builder, int paramIndex, ParameterInfo info)
		{
			ParameterBuilder parameterBuilder = builder.DefineParameter(1 + paramIndex, info.Attributes, info.Name);
			CopyParameterAttributes(info, parameterBuilder);
			return parameterBuilder;
		}

		public static void CopyParameterAttributes(ParameterInfo from, ParameterBuilder to)
		{
			if (from.IsDefined(typeof(ParamArrayAttribute), false))
			{
				to.SetCustomAttribute(new CustomAttributeBuilder(typeof(ParamArrayAttribute).GetConstructor(Type.EmptyTypes), ArrayUtils.EmptyObjects));
			}
			else if (from.IsDefined(typeof(ParamDictionaryAttribute), false))
			{
				to.SetCustomAttribute(new CustomAttributeBuilder(typeof(ParamDictionaryAttribute).GetConstructor(Type.EmptyTypes), ArrayUtils.EmptyObjects));
			}
			if (from.HasDefaultValue())
			{
				to.SetConstant(from.GetDefaultValue());
			}
		}

		internal void EmitClrCallStub(ILGen il, MethodInfo mi, string name)
		{
			int num = 0;
			bool isList = false;
			bool context = false;
			ParameterInfo[] parameters = mi.GetParameters();
			if (parameters.Length > 0)
			{
				if (parameters[0].ParameterType == ContextType)
				{
					num = 1;
					context = true;
				}
				if (parameters[parameters.Length - 1].IsDefined(typeof(ParamArrayAttribute), false))
				{
					isList = true;
				}
			}
			ParameterInfo[] array = parameters;
			int nargs = array.Length - num;
			ILGen cCtor = GetCCtor();
			EmitMakeCallAction(name, nargs, isList);
			Type type = CompilerHelpers.MakeCallSiteType(MakeSiteSignature(nargs));
			FieldBuilder fi = _tb.DefineField("site$" + _site++, type, FieldAttributes.Private | FieldAttributes.Static);
			cCtor.EmitCall(type.GetMethod("Create"));
			cCtor.EmitFieldSet(fi);
			il.EmitFieldGet(fi);
			FieldInfo field = type.GetField("Target");
			il.EmitFieldGet(field);
			il.EmitFieldGet(fi);
			EmitContext(il, context);
			il.Emit(OpCodes.Ldarg_0);
			List<ReturnFixer> list = new List<ReturnFixer>(0);
			for (int i = num; i < array.Length; i++)
			{
				ReturnFixer returnFixer = ReturnFixer.EmitArgument(il, array[i], i + 1);
				if (returnFixer != null)
				{
					list.Add(returnFixer);
				}
			}
			il.EmitCall(field.FieldType, "Invoke");
			foreach (ReturnFixer item in list)
			{
				item.FixReturn(il);
			}
		}

		private void EmitContext(ILGen il, bool context)
		{
			if (context)
			{
				il.EmitLoadArg(1);
			}
			else
			{
				EmitImplicitContext(il);
			}
		}
	}
}
