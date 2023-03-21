using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Generation;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls
{
	public abstract class RubyMethodGroupBase : RubyMemberInfo
	{
		private OverloadInfo[] _methodBases;

		protected internal virtual OverloadInfo[] MethodBases
		{
			get
			{
				return _methodBases;
			}
		}

		internal abstract SelfCallConvention CallConvention { get; }

		internal abstract bool ImplicitProtocolConversions { get; }

		protected RubyMethodGroupBase(OverloadInfo[] methods, RubyMemberFlags flags, RubyModule declaringModule)
			: base(flags, declaringModule)
		{
			if (methods != null)
			{
				SetMethodBasesNoLock(methods);
			}
		}

		protected abstract RubyMemberInfo Copy(OverloadInfo[] methods);

		internal OverloadInfo[] SetMethodBasesNoLock(OverloadInfo[] methods)
		{
			return _methodBases = methods;
		}

		public override MemberInfo[] GetMembers()
		{
			return ArrayUtils.ToArray((ICollection<OverloadInfo>)MethodBases, (Func<OverloadInfo, MemberInfo>)((OverloadInfo o) => o.ReflectionInfo));
		}

		public override int GetArity()
		{
			int num = int.MaxValue;
			int num2 = 0;
			bool flag = false;
			OverloadInfo[] methodBases = MethodBases;
			foreach (OverloadInfo method in methodBases)
			{
				int mandatory;
				int optional;
				RubyOverloadResolver.GetParameterCount(method, CallConvention, out mandatory, out optional);
				if (mandatory < num)
				{
					num = mandatory;
				}
				if (mandatory > num2)
				{
					num2 = mandatory;
				}
				if (!flag && optional > 0)
				{
					flag = true;
				}
			}
			if (flag || num2 > num)
			{
				return -num - 1;
			}
			return num;
		}

		public override RubyMemberInfo TryBindGenericParameters(Type[] typeArguments)
		{
			List<OverloadInfo> list = new List<OverloadInfo>();
			OverloadInfo[] methodBases = MethodBases;
			foreach (OverloadInfo overloadInfo in methodBases)
			{
				if (overloadInfo.IsGenericMethodDefinition)
				{
					if (typeArguments.Length == overloadInfo.GenericArguments.Count)
					{
						list.Add(overloadInfo.MakeGenericMethod(typeArguments));
					}
				}
				else if (typeArguments.Length == 0)
				{
					list.Add(overloadInfo);
				}
			}
			if (list.Count == 0)
			{
				return null;
			}
			return Copy(list.ToArray());
		}

		public override RubyMemberInfo TrySelectOverload(Type[] parameterTypes)
		{
			List<OverloadInfo> list = new List<OverloadInfo>();
			OverloadInfo[] methodBases = MethodBases;
			foreach (OverloadInfo overloadInfo in methodBases)
			{
				if (IsOverloadSignature(overloadInfo, parameterTypes))
				{
					list.Add(overloadInfo);
				}
			}
			if (list.Count == 0)
			{
				return null;
			}
			return Copy(list.ToArray());
		}

		private bool IsOverloadSignature(OverloadInfo method, Type[] parameterTypes)
		{
			int hiddenParameterCount = RubyOverloadResolver.GetHiddenParameterCount(method, CallConvention);
			IList<ParameterInfo> parameters = method.Parameters;
			if (parameters.Count - hiddenParameterCount != parameterTypes.Length)
			{
				return false;
			}
			for (int i = 0; i < parameterTypes.Length; i++)
			{
				if (parameters[hiddenParameterCount + i].ParameterType != parameterTypes[i])
				{
					return false;
				}
			}
			return true;
		}

		private static Type GetAssociatedSystemType(RubyModule module)
		{
			if (module.IsClass)
			{
				Type underlyingSystemType = ((RubyClass)module).GetUnderlyingSystemType();
				if (underlyingSystemType != null)
				{
					return underlyingSystemType;
				}
			}
			return typeof(SuperCallAction);
		}

		internal override void BuildSuperCallNoFlow(MetaObjectBuilder metaBuilder, CallArguments args, string name, RubyModule declaringModule)
		{
			IList<OverloadInfo> visibleOverloads = GetVisibleOverloads(args, MethodBases, true);
			if (visibleOverloads.Count == 0)
			{
				metaBuilder.SetError(Methods.MakeClrVirtualMethodCalledError.OpCall(args.MetaContext.Expression, args.MetaTarget.Expression, Expression.Constant(name)));
			}
			else
			{
				BuildCallNoFlow(metaBuilder, args, name, visibleOverloads, CallConvention, ImplicitProtocolConversions);
			}
		}

		internal static IList<OverloadInfo> GetVisibleOverloads(CallArguments args, IList<OverloadInfo> overloads, bool isSuperCall)
		{
			IList<OverloadInfo> list = null;
			if (isSuperCall || !args.RubyContext.DomainManager.Configuration.PrivateBinding)
			{
				Type type = null;
				BindingFlags bindingFlags = BindingFlags.Default;
				for (int i = 0; i < overloads.Count; i++)
				{
					OverloadInfo overloadInfo = overloads[i];
					if ((isSuperCall && overloadInfo.IsVirtual && !overloadInfo.IsFinal) || overloadInfo.IsProtected)
					{
						if (list == null)
						{
							list = CollectionUtils.GetRange(overloads, 0, i);
							IRubyType rubyType = args.Target as IRubyType;
							RubyClass rubyClass;
							if (rubyType != null)
							{
								bindingFlags = BindingFlags.Instance;
								type = args.Target.GetType();
							}
							else if ((rubyClass = args.Target as RubyClass) != null && rubyClass.IsRubyClass && !rubyClass.IsSingletonClass)
							{
								bindingFlags = BindingFlags.Static;
								type = rubyClass.GetUnderlyingSystemType();
							}
						}
						if (type != null)
						{
							IList<Type> genericParameterTypes = (overloadInfo.IsGenericMethod ? overloadInfo.GenericArguments : null);
							OverloadInfo methodOverload = GetMethodOverload(ArrayUtils.ToArray(overloadInfo.Parameters, (ParameterInfo pi) => pi.ParameterType), genericParameterTypes, type, "#base#" + overloadInfo.Name, BindingFlags.Public | bindingFlags | BindingFlags.InvokeMethod);
							list.Add(methodOverload);
						}
					}
					else if (list != null)
					{
						list.Add(overloadInfo);
					}
				}
			}
			return list ?? overloads;
		}

		private static OverloadInfo GetMethodOverload(Type[] parameterTypes, IList<Type> genericParameterTypes, Type type, string name, BindingFlags bindingFlags)
		{
			MemberInfo[] member = type.GetMember(name, MemberTypes.Method, bindingFlags);
			for (int i = 0; i < member.Length; i++)
			{
				MethodInfo methodInfo = (MethodInfo)member[i];
				MethodInfo method = methodInfo;
				if (genericParameterTypes != null != methodInfo.IsGenericMethod)
				{
					continue;
				}
				if (methodInfo.IsGenericMethod)
				{
					if (methodInfo.GetGenericArguments().Length != genericParameterTypes.Count)
					{
						continue;
					}
					methodInfo = methodInfo.MakeGenericMethod(ArrayUtils.ToArray(genericParameterTypes));
				}
				if (ArrayUtils.ToArray(methodInfo.GetParameters(), (ParameterInfo pi) => pi.ParameterType).ValueEquals(parameterTypes))
				{
					return new ReflectionOverloadInfo(method);
				}
			}
			return null;
		}

		internal static BindingTarget ResolveOverload(MetaObjectBuilder metaBuilder, CallArguments args, string name, IList<OverloadInfo> overloads, SelfCallConvention callConvention, bool implicitProtocolConversions, out RubyOverloadResolver resolver)
		{
			resolver = new RubyOverloadResolver(metaBuilder, args, callConvention, implicitProtocolConversions);
			BindingTarget bindingTarget = resolver.ResolveOverload(name, overloads, NarrowingLevel.None, NarrowingLevel.All);
			bool flag = bindingTarget.Success && HasBlockParameter(bindingTarget.Overload);
			if (args.Signature.HasBlock && flag)
			{
				metaBuilder.ControlFlowBuilder = RuleControlFlowBuilder;
			}
			resolver.AddArgumentRestrictions(metaBuilder, bindingTarget);
			return bindingTarget;
		}

		internal static void BuildCallNoFlow(MetaObjectBuilder metaBuilder, CallArguments args, string name, IList<OverloadInfo> overloads, SelfCallConvention callConvention, bool implicitProtocolConversions)
		{
			RubyOverloadResolver resolver;
			BindingTarget bindingTarget = ResolveOverload(metaBuilder, args, name, overloads, callConvention, implicitProtocolConversions, out resolver);
			if (bindingTarget.Success)
			{
				if (object.ReferenceEquals(bindingTarget.Overload.ReflectionInfo, Methods.CreateDefaultInstance))
				{
					metaBuilder.Result = Expression.New(args.TargetClass.TypeTracker.Type);
				}
				else if (args.Signature.IsVirtualCall && bindingTarget.Overload.IsVirtual)
				{
					metaBuilder.Result = Expression.Field(null, Fields.ForwardToBase);
				}
				else
				{
					metaBuilder.Result = bindingTarget.MakeExpression();
				}
			}
			else
			{
				metaBuilder.SetError(resolver.MakeInvalidParametersError(bindingTarget).Expression);
			}
		}

		public static void RuleControlFlowBuilder(MetaObjectBuilder metaBuilder, CallArguments args)
		{
			if (!metaBuilder.Error)
			{
				DynamicMetaObject metaBlock = args.GetMetaBlock();
				if (metaBlock.Value == null)
				{
					metaBuilder.AddRestriction(Expression.Equal(metaBlock.Expression, Microsoft.Scripting.Ast.Utils.Constant(null)));
					return;
				}
				metaBuilder.AddRestriction(Expression.NotEqual(metaBlock.Expression, Microsoft.Scripting.Ast.Utils.Constant(null)));
				Expression bfcVariable = metaBuilder.BfcVariable;
				Expression temporary = metaBuilder.GetTemporary(typeof(object), "#result");
				ParameterExpression parameterExpression;
				metaBuilder.Result = Expression.Block(Expression.Assign(bfcVariable, Methods.CreateBfcForLibraryMethod.OpCall(Microsoft.Scripting.Ast.Utils.Convert(args.GetBlockExpression(), typeof(Proc)))), Microsoft.Scripting.Ast.Utils.Try(Expression.Assign(temporary, Microsoft.Scripting.Ast.Utils.Convert(metaBuilder.Result, typeof(object)))).Filter(parameterExpression = Expression.Parameter(typeof(MethodUnwinder), "#unwinder"), Methods.IsProcConverterTarget.OpCall(bfcVariable, parameterExpression), Expression.Assign(temporary, Expression.Field(parameterExpression, StackUnwinder.ReturnValueField)), Microsoft.Scripting.Ast.Utils.Default(typeof(object))).Finally(Methods.LeaveProcConverter.OpCall(bfcVariable)), temporary);
			}
		}

		private static bool HasBlockParameter(OverloadInfo method)
		{
			foreach (ParameterInfo parameter in method.Parameters)
			{
				if (parameter.ParameterType == typeof(BlockParam))
				{
					return true;
				}
			}
			return false;
		}
	}
}
