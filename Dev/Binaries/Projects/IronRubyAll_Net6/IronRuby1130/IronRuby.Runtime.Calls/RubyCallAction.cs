using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Compiler.Ast;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls
{
	public class RubyCallAction : RubyMetaBinder
	{
		private enum MethodMissingBinding
		{
			Error,
			Fallback,
			Custom
		}

		private readonly RubyCallSignature _signature;

		private readonly string _methodName;

		public override RubyCallSignature Signature
		{
			get
			{
				return _signature;
			}
		}

		public string MethodName
		{
			get
			{
				return _methodName;
			}
		}

		public override Type ReturnType
		{
			get
			{
				if (!_signature.ResolveOnly)
				{
					return typeof(object);
				}
				return typeof(bool);
			}
		}

		protected internal RubyCallAction(RubyContext context, string methodName, RubyCallSignature signature)
			: base(context)
		{
			_methodName = methodName;
			_signature = signature;
		}

		public static RubyCallAction Make(RubyContext context, string methodName, int argumentCount)
		{
			return Make(context, methodName, RubyCallSignature.Simple(argumentCount));
		}

		public static RubyCallAction Make(RubyContext context, string methodName, RubyCallSignature signature)
		{
			ContractUtils.RequiresNotNull(context, "context");
			ContractUtils.RequiresNotNull(methodName, "methodName");
			return context.MetaBinderFactory.Call(methodName, signature);
		}

		public static RubyCallAction MakeShared(string methodName, RubyCallSignature signature)
		{
			return RubyMetaBinderFactory.Shared.Call(methodName, signature);
		}

		public override string ToString()
		{
			string methodName = _methodName;
			RubyCallSignature signature = _signature;
			return methodName + signature.ToString() + ((base.Context != null) ? (" @" + base.Context.RuntimeId) : null);
		}

		public override System.Linq.Expressions.Expression CreateExpression()
		{
			return System.Linq.Expressions.Expression.Call(Methods.GetMethod(typeof(RubyCallAction), "MakeShared", typeof(string), typeof(RubyCallSignature)), Microsoft.Scripting.Ast.Utils.Constant(_methodName), _signature.CreateExpression());
		}

		protected override object BindPrecompiled(Type delegateType, object[] args)
		{
			if (base.Context == null || Signature.ResolveOnly || ((uint)Signature.Flags & 0xFFFFFFE2u) != 0)
			{
				return null;
			}
			RubyScope scope;
			object obj;
			if (Signature.HasScope)
			{
				scope = (RubyScope)args[0];
				obj = args[1];
			}
			else
			{
				scope = base.Context.EmptyScope;
				obj = args[0];
			}
			RubyClass immediateClassOf = base.Context.GetImmediateClassOf(obj);
			int method;
			MethodResolutionResult methodResolutionResult;
			using (immediateClassOf.Context.ClassHierarchyLocker())
			{
				method = immediateClassOf.Version.Method;
				methodResolutionResult = immediateClassOf.ResolveMethodForSiteNoLock(_methodName, GetVisibilityContext(Signature, scope));
			}
			if (!methodResolutionResult.Found || (methodResolutionResult.Info.IsProtected && !Signature.HasImplicitSelf))
			{
				return null;
			}
			MemberDispatcher dispatcher = methodResolutionResult.Info.GetDispatcher(delegateType, Signature, obj, method);
			if (dispatcher != null)
			{
				return dispatcher.CreateDelegate(MemberDispatcher.UntypedFuncs.Contains(delegateType));
			}
			return null;
		}

		protected override bool Build(MetaObjectBuilder metaBuilder, CallArguments args, bool defaultFallback)
		{
			return BuildCall(metaBuilder, _methodName, args, defaultFallback, true);
		}

		internal static bool BuildCall(MetaObjectBuilder metaBuilder, string methodName, CallArguments args, bool defaultFallback, bool callClrMethods)
		{
			RubyMemberInfo methodMissing;
			MethodResolutionResult methodResolutionResult = Resolve(metaBuilder, methodName, args, out methodMissing);
			if (methodResolutionResult.Found)
			{
				if (!callClrMethods && !methodResolutionResult.Info.IsRubyMember)
				{
					return false;
				}
				if (args.Signature.ResolveOnly)
				{
					metaBuilder.Result = AstFactory.True;
					return true;
				}
				if (args.Signature.IsVirtualCall && !methodResolutionResult.Info.IsRubyMember)
				{
					metaBuilder.Result = System.Linq.Expressions.Expression.Field(null, Fields.ForwardToBase);
					return true;
				}
				methodResolutionResult.Info.BuildCall(metaBuilder, args, methodName);
				return true;
			}
			if (args.Signature.ResolveOnly)
			{
				metaBuilder.Result = AstFactory.False;
				return true;
			}
			return BuildMethodMissingCall(metaBuilder, args, methodName, methodMissing, methodResolutionResult.IncompatibleVisibility, false, defaultFallback);
		}

		internal static bool BuildAccess(MetaObjectBuilder metaBuilder, string methodName, CallArguments args, bool defaultFallback, bool callClrMethods)
		{
			RubyMemberInfo methodMissing;
			MethodResolutionResult methodResolutionResult = Resolve(metaBuilder, methodName, args, out methodMissing);
			if (methodResolutionResult.Found)
			{
				if (!callClrMethods && !methodResolutionResult.Info.IsRubyMember)
				{
					return false;
				}
				if (methodResolutionResult.Info.IsDataMember)
				{
					methodResolutionResult.Info.BuildCall(metaBuilder, args, methodName);
				}
				else
				{
					metaBuilder.Result = Methods.CreateBoundMember.OpCall(Microsoft.Scripting.Ast.Utils.Convert(args.TargetExpression, typeof(object)), System.Linq.Expressions.Expression.Constant(methodResolutionResult.Info, typeof(RubyMemberInfo)), System.Linq.Expressions.Expression.Constant(methodName));
				}
				return true;
			}
			return BuildMethodMissingAccess(metaBuilder, args, methodName, methodMissing, methodResolutionResult.IncompatibleVisibility, false, defaultFallback);
		}

		private static VisibilityContext GetVisibilityContext(RubyCallSignature callSignature, RubyScope scope)
		{
			if (!callSignature.HasImplicitSelf && callSignature.HasScope)
			{
				return new VisibilityContext(scope.SelfImmediateClass);
			}
			return new VisibilityContext(callSignature.IsInteropCall ? RubyMethodAttributes.Public : RubyMethodAttributes.VisibilityMask);
		}

		internal static MethodResolutionResult Resolve(MetaObjectBuilder metaBuilder, string methodName, CallArguments args, out RubyMemberInfo methodMissing)
		{
			RubyClass targetClass = args.TargetClass;
			VisibilityContext visibilityContext = GetVisibilityContext(args.Signature, args.Scope);
			MethodResolutionResult result;
			using (targetClass.Context.ClassHierarchyLocker())
			{
				metaBuilder.AddTargetTypeTest(args.Target, targetClass, args.TargetExpression, args.MetaContext, new string[2]
				{
					methodName,
					Symbols.MethodMissing
				});
				if (args.Signature.IsSuperCall)
				{
					result = targetClass.ResolveSuperMethodNoLock(methodName, targetClass).InvalidateSitesOnOverride();
				}
				else
				{
					MethodLookup options = (args.Signature.IsVirtualCall ? MethodLookup.Virtual : MethodLookup.Default);
					result = targetClass.ResolveMethodForSiteNoLock(methodName, visibilityContext, options);
				}
				if (!result.Found)
				{
					methodMissing = targetClass.ResolveMethodMissingForSite(methodName, result.IncompatibleVisibility);
				}
				else
				{
					methodMissing = null;
				}
			}
			if (result.Info != null && result.Info.IsProtected && visibilityContext.Class != null)
			{
				metaBuilder.AddCondition(System.Linq.Expressions.Expression.Equal(Methods.GetSelfClassVersionHandle.OpCall(Microsoft.Scripting.Ast.Utils.Convert(args.MetaScope.Expression, typeof(RubyScope))), System.Linq.Expressions.Expression.Constant(visibilityContext.Class.Version)));
			}
			return result;
		}

		internal static bool BuildMethodMissingCall(MetaObjectBuilder metaBuilder, CallArguments args, string methodName, RubyMemberInfo methodMissing, RubyMethodVisibility incompatibleVisibility, bool isSuperCall, bool defaultFallback)
		{
			switch (BindToKernelMethodMissing(metaBuilder, args, methodName, methodMissing, incompatibleVisibility, isSuperCall))
			{
			case MethodMissingBinding.Custom:
				methodMissing.BuildMethodMissingCall(metaBuilder, args, methodName);
				return true;
			case MethodMissingBinding.Error:
				return true;
			case MethodMissingBinding.Fallback:
				if (defaultFallback)
				{
					metaBuilder.SetError(Methods.MakeMissingMethodError.OpCall(args.MetaContext.Expression, Microsoft.Scripting.Ast.Utils.Convert(args.TargetExpression, typeof(object)), System.Linq.Expressions.Expression.Constant(methodName)));
					return true;
				}
				return false;
			default:
				throw Assert.Unreachable;
			}
		}

		internal static bool BuildMethodMissingAccess(MetaObjectBuilder metaBuilder, CallArguments args, string methodName, RubyMemberInfo methodMissing, RubyMethodVisibility incompatibleVisibility, bool isSuperCall, bool defaultFallback)
		{
			switch (BindToKernelMethodMissing(metaBuilder, args, methodName, methodMissing, incompatibleVisibility, isSuperCall))
			{
			case MethodMissingBinding.Custom:
				metaBuilder.Result = Methods.CreateBoundMissingMember.OpCall(Microsoft.Scripting.Ast.Utils.Convert(args.TargetExpression, typeof(object)), System.Linq.Expressions.Expression.Constant(methodMissing, typeof(RubyMemberInfo)), System.Linq.Expressions.Expression.Constant(methodName));
				return true;
			case MethodMissingBinding.Error:
				return true;
			case MethodMissingBinding.Fallback:
				if (defaultFallback)
				{
					metaBuilder.SetError(Methods.MakeMissingMemberError.OpCall(System.Linq.Expressions.Expression.Constant(methodName)));
					return true;
				}
				return false;
			default:
				throw Assert.Unreachable;
			}
		}

		private static MethodMissingBinding BindToKernelMethodMissing(MetaObjectBuilder metaBuilder, CallArguments args, string methodName, RubyMemberInfo methodMissing, RubyMethodVisibility incompatibleVisibility, bool isSuperCall)
		{
			if (methodMissing == null || (methodMissing.DeclaringModule == methodMissing.Context.BasicObjectClass && methodMissing is RubyLibraryMethodInfo))
			{
				if (isSuperCall)
				{
					metaBuilder.SetError(Methods.MakeMissingSuperException.OpCall(Microsoft.Scripting.Ast.Utils.Constant(methodName)));
				}
				else
				{
					switch (incompatibleVisibility)
					{
					case RubyMethodVisibility.Private:
						metaBuilder.SetError(Methods.MakePrivateMethodCalledError.OpCall(Microsoft.Scripting.Ast.Utils.Convert(args.MetaContext.Expression, typeof(RubyContext)), args.TargetExpression, Microsoft.Scripting.Ast.Utils.Constant(methodName)));
						break;
					case RubyMethodVisibility.Protected:
						metaBuilder.SetError(Methods.MakeProtectedMethodCalledError.OpCall(Microsoft.Scripting.Ast.Utils.Convert(args.MetaContext.Expression, typeof(RubyContext)), args.TargetExpression, Microsoft.Scripting.Ast.Utils.Constant(methodName)));
						break;
					default:
						return MethodMissingBinding.Fallback;
					}
				}
				return MethodMissingBinding.Error;
			}
			return MethodMissingBinding.Custom;
		}

		protected override DynamicMetaObjectBinder GetInteropBinder(RubyContext context, IList<DynamicMetaObject> args, out MethodInfo postConverter)
		{
			postConverter = null;
			ExpressionType op;
			int num = RubyUtils.TryMapOperator(_methodName, out op);
			if (num == 1 + args.Count)
			{
				switch (num)
				{
				case 1:
					return context.MetaBinderFactory.InteropUnaryOperation(op);
				case 2:
					return context.MetaBinderFactory.InteropBinaryOperation(op);
				}
			}
			switch (_methodName)
			{
			case "new":
				return context.MetaBinderFactory.InteropCreateInstance(new CallInfo(args.Count));
			case "call":
				return context.MetaBinderFactory.InteropInvoke(new CallInfo(args.Count));
			case "to_s":
				if (args.Count == 0)
				{
					postConverter = Methods.ObjectToMutableString;
					return context.MetaBinderFactory.InteropInvokeMember("ToString", new CallInfo(0));
				}
				break;
			case "to_str":
				if (args.Count == 0)
				{
					postConverter = Methods.StringToMutableString;
					return context.MetaBinderFactory.InteropConvert(typeof(string), false);
				}
				break;
			case "[]":
				return context.MetaBinderFactory.InteropGetIndex(new CallInfo(args.Count));
			case "[]=":
				return context.MetaBinderFactory.InteropSetIndex(new CallInfo(args.Count));
			}
			if (_methodName.LastCharacter() == 61)
			{
				string name = _methodName.Substring(0, _methodName.Length - 1);
				if (args.Count == 1)
				{
					return context.MetaBinderFactory.InteropSetMember(name);
				}
				return context.MetaBinderFactory.InteropSetIndexedProperty(name, new CallInfo(args.Count));
			}
			return context.MetaBinderFactory.InteropInvokeMember(_methodName, new CallInfo(args.Count));
		}
	}
}
