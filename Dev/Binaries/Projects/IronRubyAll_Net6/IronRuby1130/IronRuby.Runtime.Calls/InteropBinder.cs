using System;
using System.Collections;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Compiler;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.ComInterop;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls
{
	internal static class InteropBinder
	{
		internal sealed class CreateInstance : CreateInstanceBinder, IInteropBinder
		{
			private readonly RubyContext _context;

			public RubyContext Context
			{
				get
				{
					return _context;
				}
			}

			internal CreateInstance(RubyContext context, CallInfo callInfo)
				: base(callInfo)
			{
				_context = context;
			}

			public override T BindDelegate<T>(CallSite<T> site, object[] args)
			{
				if (_context.Options.NoAdaptiveCompilation)
				{
					return null;
				}
				T val = this.LightBind<T>(args, _context.Options.CompilationThreshold);
				CacheTarget(val);
				return val;
			}

			public override DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
			{
				return InvokeMember.FallbackInvokeMember(this, "new", base.CallInfo, target, args, errorSuggestion, null);
			}

			public override string ToString()
			{
				return string.Format("Interop.CreateInstance({0}){1}", base.CallInfo.ArgumentCount, (_context != null) ? (" @" + Context.RuntimeId) : null);
			}
		}

		internal sealed class Return : InvokeBinder, IInteropBinder
		{
			private readonly RubyContext _context;

			public RubyContext Context
			{
				get
				{
					return _context;
				}
			}

			internal Return(RubyContext context, CallInfo callInfo)
				: base(callInfo)
			{
				_context = context;
			}

			public override T BindDelegate<T>(CallSite<T> site, object[] args)
			{
				if (_context.Options.NoAdaptiveCompilation)
				{
					return null;
				}
				T val = this.LightBind<T>(args, _context.Options.CompilationThreshold);
				CacheTarget(val);
				return val;
			}

			public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
			{
				MetaObjectBuilder metaObjectBuilder = new MetaObjectBuilder(this, target, args);
				CallArguments args2 = new CallArguments(_context, target, args, base.CallInfo);
				metaObjectBuilder.AddTypeRestriction(target.GetLimitType(), target.Expression);
				RubyOverloadResolver.NormalizeArguments(metaObjectBuilder, args2, 0, 0);
				if (!metaObjectBuilder.Error)
				{
					metaObjectBuilder.Result = target.Expression;
				}
				else
				{
					metaObjectBuilder.SetMetaResult(errorSuggestion, false);
				}
				return metaObjectBuilder.CreateMetaObject(this);
			}
		}

		internal sealed class Invoke : InvokeBinder, IInteropBinder
		{
			private readonly RubyContext _context;

			public Type ResultType
			{
				get
				{
					return typeof(object);
				}
			}

			public RubyContext Context
			{
				get
				{
					return _context;
				}
			}

			internal Invoke(RubyContext context, CallInfo callInfo)
				: base(callInfo)
			{
				_context = context;
			}

			public override T BindDelegate<T>(CallSite<T> site, object[] args)
			{
				if (_context.Options.NoAdaptiveCompilation)
				{
					return null;
				}
				T val = this.LightBind<T>(args, _context.Options.CompilationThreshold);
				CacheTarget(val);
				return val;
			}

			public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
			{
				DynamicMetaObject result;
				if (ComBinder.TryBindInvoke(this, target, InplaceConvertComArguments(args), out result))
				{
					return result;
				}
				return InvokeMember.FallbackInvokeMember(this, "call", base.CallInfo, target, args, errorSuggestion, null);
			}

			public static DynamicMetaObject Bind(InvokeBinder binder, RubyMetaObject target, DynamicMetaObject[] args, Action<MetaObjectBuilder, CallArguments> buildInvoke)
			{
				RubyCallSignature callSignature;
				if (RubyCallSignature.TryCreate(binder.CallInfo, out callSignature))
				{
					return binder.FallbackInvoke(target, args);
				}
				CallArguments arg = new CallArguments(target.CreateMetaContext(), target, args, callSignature);
				MetaObjectBuilder metaObjectBuilder = new MetaObjectBuilder(target, args);
				buildInvoke(metaObjectBuilder, arg);
				return metaObjectBuilder.CreateMetaObject(binder);
			}

			public override string ToString()
			{
				return string.Format("Interop.Invoke({0}){1}", base.CallInfo.ArgumentCount, (_context != null) ? (" @" + Context.RuntimeId) : null);
			}
		}

		internal class InvokeMember : InvokeMemberBinder, IInteropBinder
		{
			private readonly RubyContext _context;

			private readonly InvokeMember _unmangled;

			private readonly string _originalName;

			public RubyContext Context
			{
				get
				{
					return _context;
				}
			}

			internal InvokeMember(RubyContext context, string name, CallInfo callInfo, string originalName)
				: base(name, false, callInfo)
			{
				_context = context;
				_originalName = originalName;
				if (originalName == null)
				{
					string text = RubyUtils.TryUnmangleMethodName(base.Name);
					if (text != null)
					{
						_unmangled = new InvokeMember(_context, text, base.CallInfo, base.Name);
					}
				}
			}

			public override T BindDelegate<T>(CallSite<T> site, object[] args)
			{
				if (_context.Options.NoAdaptiveCompilation)
				{
					return null;
				}
				T val = this.LightBind<T>(args, _context.Options.CompilationThreshold);
				CacheTarget(val);
				return val;
			}

			public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
			{
				DynamicMetaObject result;
				if (ComBinder.TryBindInvokeMember(this, target, InplaceConvertComArguments(args), out result))
				{
					return result;
				}
				return FallbackInvokeMember(this, _originalName ?? base.Name, base.CallInfo, target, args, errorSuggestion, _unmangled);
			}

			internal static DynamicMetaObject FallbackInvokeMember(IInteropBinder binder, string methodName, CallInfo callInfo, DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion, InvokeMember alternateBinder)
			{
				MetaObjectBuilder metaObjectBuilder = new MetaObjectBuilder(binder, target, args);
				CallArguments args2 = new CallArguments(binder.Context, target, args, callInfo);
				bool flag = alternateBinder != null && errorSuggestion == null;
				if (!RubyCallAction.BuildCall(metaObjectBuilder, methodName, args2, errorSuggestion == null && !flag, true))
				{
					if (flag)
					{
						metaObjectBuilder.SetMetaResult(target.BindInvokeMember(alternateBinder, args), true);
					}
					else
					{
						metaObjectBuilder.SetMetaResult(errorSuggestion, false);
					}
				}
				return metaObjectBuilder.CreateMetaObject((DynamicMetaObjectBinder)binder);
			}

			public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
			{
				ExpressionCollectionBuilder expressionCollectionBuilder = new ExpressionCollectionBuilder();
				expressionCollectionBuilder.Add(target.Expression);
				expressionCollectionBuilder.Add(args.ToExpressions());
				return new DynamicMetaObject(Microsoft.Scripting.Ast.Utils.LightDynamic(_context.MetaBinderFactory.InteropReturn(base.CallInfo), typeof(object), expressionCollectionBuilder), target.Restrictions.Merge(BindingRestrictions.Combine(args)));
			}

			public static DynamicMetaObject Bind(DynamicMetaObject context, CreateInstanceBinder binder, DynamicMetaObject target, DynamicMetaObject[] args, Func<DynamicMetaObject, DynamicMetaObject[], DynamicMetaObject> fallback)
			{
				return Bind(context, "new", binder.CallInfo, binder, target, args, fallback);
			}

			public static DynamicMetaObject Bind(DynamicMetaObject context, InvokeMemberBinder binder, DynamicMetaObject target, DynamicMetaObject[] args, Func<DynamicMetaObject, DynamicMetaObject[], DynamicMetaObject> fallback)
			{
				return Bind(context, binder.Name, binder.CallInfo, binder, target, args, fallback);
			}

			public static DynamicMetaObject Bind(DynamicMetaObject context, string methodName, CallInfo callInfo, DynamicMetaObjectBinder binder, DynamicMetaObject target, DynamicMetaObject[] args, Func<DynamicMetaObject, DynamicMetaObject[], DynamicMetaObject> fallback)
			{
				CallArguments args2 = new CallArguments(context, target, args, RubyCallSignature.Interop(callInfo.ArgumentCount));
				MetaObjectBuilder metaObjectBuilder = new MetaObjectBuilder(target, args);
				if (!RubyCallAction.BuildCall(metaObjectBuilder, methodName, args2, false, false))
				{
					metaObjectBuilder.SetMetaResult(fallback(target, args), false);
				}
				return metaObjectBuilder.CreateMetaObject(binder);
			}

			public override string ToString()
			{
				return string.Format("Interop.InvokeMember({0}, {1}){2}", base.Name, base.CallInfo.ArgumentCount, (_context != null) ? (" @" + Context.RuntimeId) : null);
			}
		}

		internal sealed class GetMember : GetMemberBinder, IInteropBinder, IInvokeOnGetBinder
		{
			private static readonly CallInfo _CallInfo = new CallInfo(0);

			private readonly RubyContext _context;

			private readonly GetMember _unmangled;

			private readonly string _originalName;

			public RubyContext Context
			{
				get
				{
					return _context;
				}
			}

			bool IInvokeOnGetBinder.InvokeOnGet
			{
				get
				{
					return false;
				}
			}

			internal GetMember(RubyContext context, string name, string originalName)
				: base(name, false)
			{
				_context = context;
				_originalName = originalName;
				if (originalName == null)
				{
					string text = RubyUtils.TryUnmangleMethodName(name);
					if (text != null)
					{
						_unmangled = new GetMember(_context, text, originalName);
					}
				}
			}

			public override T BindDelegate<T>(CallSite<T> site, object[] args)
			{
				if (_context.Options.NoAdaptiveCompilation)
				{
					return null;
				}
				T val = this.LightBind<T>(args, _context.Options.CompilationThreshold);
				CacheTarget(val);
				return val;
			}

			public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
			{
				DynamicMetaObject result;
				if (ComBinder.TryBindGetMember(this, target, out result))
				{
					return result;
				}
				MetaObjectBuilder metaObjectBuilder = new MetaObjectBuilder(target);
				CallArguments args = new CallArguments(_context, target, DynamicMetaObject.EmptyMetaObjects, _CallInfo);
				bool flag = _unmangled != null && errorSuggestion == null;
				if (!RubyCallAction.BuildAccess(metaObjectBuilder, _originalName ?? base.Name, args, errorSuggestion == null && !flag, true))
				{
					if (flag)
					{
						metaObjectBuilder.SetMetaResult(target.BindGetMember(_unmangled), true);
					}
					else
					{
						metaObjectBuilder.SetMetaResult(errorSuggestion, false);
					}
				}
				return metaObjectBuilder.CreateMetaObject(this);
			}

			public static DynamicMetaObject Bind(DynamicMetaObject context, GetMemberBinder binder, DynamicMetaObject target, Func<DynamicMetaObject, DynamicMetaObject> fallback)
			{
				CallArguments args = new CallArguments(context, target, DynamicMetaObject.EmptyMetaObjects, RubyCallSignature.Interop(0));
				MetaObjectBuilder metaObjectBuilder = new MetaObjectBuilder(target);
				if (!RubyCallAction.BuildAccess(metaObjectBuilder, binder.Name, args, false, false))
				{
					metaObjectBuilder.SetMetaResult(fallback(target), false);
				}
				return metaObjectBuilder.CreateMetaObject(binder);
			}

			public override string ToString()
			{
				return string.Format("Interop.GetMember({0}){1}", base.Name, (_context != null) ? (" @" + Context.RuntimeId) : null);
			}
		}

		internal sealed class TryGetMemberExact : GetMemberBinder, IInvokeOnGetBinder
		{
			private readonly RubyContext _context;

			bool IInvokeOnGetBinder.InvokeOnGet
			{
				get
				{
					return false;
				}
			}

			internal TryGetMemberExact(RubyContext context, string name)
				: base(name, false)
			{
				_context = context;
			}

			public override T BindDelegate<T>(CallSite<T> site, object[] args)
			{
				if (_context.Options.NoAdaptiveCompilation)
				{
					return null;
				}
				T val = this.LightBind<T>(args, _context.Options.CompilationThreshold);
				CacheTarget(val);
				return val;
			}

			public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
			{
				DynamicMetaObject result;
				if (ComBinder.TryBindGetMember(this, target, out result))
				{
					return result;
				}
				return errorSuggestion ?? new DynamicMetaObject(Expression.Constant(OperationFailed.Value, typeof(object)), target.Restrict(CompilerHelpers.GetType(target.Value)).Restrictions);
			}

			public override string ToString()
			{
				return string.Format("Interop.TryGetMemberExact({0})", base.Name);
			}
		}

		internal sealed class SetMember : DynamicMetaObjectBinder, IInteropBinder
		{
			private readonly RubyContext _context;

			private readonly string _name;

			private readonly TryGetMemberExact _tryGetMember;

			private readonly SetMemberExact _setMember;

			private readonly SetMemberExact _setMemberUnmangled;

			private readonly TryGetMemberExact _tryGetMemberUnmangled;

			public RubyContext Context
			{
				get
				{
					return _context;
				}
			}

			internal SetMember(RubyContext context, string name)
			{
				_name = name;
				_context = context;
				_tryGetMember = context.MetaBinderFactory.InteropTryGetMemberExact(name);
				_setMember = context.MetaBinderFactory.InteropSetMemberExact(name);
				string text = RubyUtils.TryUnmangleMethodName(name);
				if (text != null)
				{
					_setMemberUnmangled = context.MetaBinderFactory.InteropSetMemberExact(text);
					_tryGetMemberUnmangled = context.MetaBinderFactory.InteropTryGetMemberExact(text);
				}
			}

			public override T BindDelegate<T>(CallSite<T> site, object[] args)
			{
				if (_context.Options.NoAdaptiveCompilation)
				{
					return null;
				}
				T val = this.LightBind<T>(args, _context.Options.CompilationThreshold);
				CacheTarget(val);
				return val;
			}

			public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
			{
				if (_setMemberUnmangled == null)
				{
					return _setMember.Bind(target, args);
				}
				return new DynamicMetaObject(Expression.Condition(Expression.AndAlso(Expression.Equal(Microsoft.Scripting.Ast.Utils.LightDynamic(_tryGetMember, typeof(object), target.Expression), Expression.Constant(OperationFailed.Value)), Expression.NotEqual(Microsoft.Scripting.Ast.Utils.LightDynamic(_tryGetMemberUnmangled, typeof(object), target.Expression), Expression.Constant(OperationFailed.Value))), Microsoft.Scripting.Ast.Utils.LightDynamic(_setMemberUnmangled, typeof(object), target.Expression, args[0].Expression), Microsoft.Scripting.Ast.Utils.LightDynamic(_setMember, typeof(object), target.Expression, args[0].Expression)), target.Restrict(CompilerHelpers.GetType(target.Value)).Restrictions);
			}

			public static DynamicMetaObject Bind(DynamicMetaObject context, SetMemberBinder binder, DynamicMetaObject target, DynamicMetaObject value, Func<DynamicMetaObject, DynamicMetaObject, DynamicMetaObject> fallback)
			{
				DynamicMetaObject[] array = new DynamicMetaObject[1] { value };
				CallArguments args = new CallArguments(context, target, array, RubyCallSignature.Interop(1));
				MetaObjectBuilder metaObjectBuilder = new MetaObjectBuilder(target, array);
				if (!RubyCallAction.BuildCall(metaObjectBuilder, binder.Name + "=", args, false, false))
				{
					metaObjectBuilder.SetMetaResult(fallback(target, value), false);
				}
				return metaObjectBuilder.CreateMetaObject(binder);
			}

			public override string ToString()
			{
				return string.Format("Interop.SetMember({0}){1}", _name, (_context != null) ? (" @" + Context.RuntimeId) : null);
			}
		}

		internal sealed class SetMemberExact : SetMemberBinder
		{
			private readonly RubyContext _context;

			internal SetMemberExact(RubyContext context, string name)
				: base(name, false)
			{
				_context = context;
			}

			public override T BindDelegate<T>(CallSite<T> site, object[] args)
			{
				if (_context.Options.NoAdaptiveCompilation)
				{
					return null;
				}
				T val = this.LightBind<T>(args, _context.Options.CompilationThreshold);
				CacheTarget(val);
				return val;
			}

			public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
			{
				DynamicMetaObject result;
				if (ComBinder.TryBindSetMember(this, target, ConvertComArgument(value), out result))
				{
					return result;
				}
				return errorSuggestion ?? new DynamicMetaObject(Expression.Throw(Expression.New(typeof(MissingMemberException).GetConstructor(new Type[1] { typeof(string) }), Expression.Constant(string.Format("unknown member: {0}", base.Name))), typeof(object)), target.Restrict(CompilerHelpers.GetType(target.Value)).Restrictions);
			}

			public override string ToString()
			{
				return string.Format("Interop.SetMemberExact({0})", base.Name);
			}
		}

		internal sealed class GetIndex : GetIndexBinder, IInteropBinder
		{
			private readonly RubyContext _context;

			public RubyContext Context
			{
				get
				{
					return _context;
				}
			}

			internal GetIndex(RubyContext context, CallInfo callInfo)
				: base(callInfo)
			{
				_context = context;
			}

			public override T BindDelegate<T>(CallSite<T> site, object[] args)
			{
				if (_context.Options.NoAdaptiveCompilation)
				{
					return null;
				}
				T val = this.LightBind<T>(args, _context.Options.CompilationThreshold);
				CacheTarget(val);
				return val;
			}

			public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
			{
				DynamicMetaObject result;
				if (ComBinder.TryBindGetIndex(this, target, InplaceConvertComArguments(indexes), out result))
				{
					return result;
				}
				return InvokeMember.FallbackInvokeMember(this, "[]", base.CallInfo, target, indexes, errorSuggestion, null);
			}

			public static DynamicMetaObject Bind(DynamicMetaObject context, GetIndexBinder binder, DynamicMetaObject target, DynamicMetaObject[] indexes, Func<DynamicMetaObject, DynamicMetaObject[], DynamicMetaObject> fallback)
			{
				CallArguments args = new CallArguments(context, target, indexes, RubyCallSignature.Interop(indexes.Length));
				MetaObjectBuilder metaObjectBuilder = new MetaObjectBuilder(target, indexes);
				if (!RubyCallAction.BuildCall(metaObjectBuilder, "[]", args, false, false))
				{
					metaObjectBuilder.SetMetaResult(fallback(target, indexes), false);
				}
				return metaObjectBuilder.CreateMetaObject(binder);
			}

			public override string ToString()
			{
				return string.Format("Interop.GetIndex({0}){1}", base.CallInfo.ArgumentCount, (_context != null) ? (" @" + Context.RuntimeId) : null);
			}
		}

		internal sealed class SetIndex : SetIndexBinder, IInteropBinder
		{
			private readonly RubyContext _context;

			public RubyContext Context
			{
				get
				{
					return _context;
				}
			}

			internal SetIndex(RubyContext context, CallInfo callInfo)
				: base(callInfo)
			{
				_context = context;
			}

			public override T BindDelegate<T>(CallSite<T> site, object[] args)
			{
				if (_context.Options.NoAdaptiveCompilation)
				{
					return null;
				}
				T val = this.LightBind<T>(args, _context.Options.CompilationThreshold);
				CacheTarget(val);
				return val;
			}

			public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
			{
				DynamicMetaObject result;
				if (ComBinder.TryBindSetIndex(this, target, InplaceConvertComArguments(indexes), ConvertComArgument(value), out result))
				{
					return result;
				}
				return InvokeMember.FallbackInvokeMember(this, "[]=", base.CallInfo, target, ArrayUtils.Append(indexes, value), errorSuggestion, null);
			}

			public static DynamicMetaObject Bind(DynamicMetaObject context, SetIndexBinder binder, DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, Func<DynamicMetaObject, DynamicMetaObject[], DynamicMetaObject, DynamicMetaObject> fallback)
			{
				DynamicMetaObject[] array = ArrayUtils.Append(indexes, value);
				CallArguments args = new CallArguments(context, target, array, new RubyCallSignature(indexes.Length, (RubyCallFlags)36));
				MetaObjectBuilder metaObjectBuilder = new MetaObjectBuilder(target, array);
				if (!RubyCallAction.BuildCall(metaObjectBuilder, "[]=", args, false, false))
				{
					metaObjectBuilder.SetMetaResult(fallback(target, indexes, value), false);
				}
				return metaObjectBuilder.CreateMetaObject(binder);
			}

			public override string ToString()
			{
				return string.Format("Interop.SetIndex({0}){1}", base.CallInfo.ArgumentCount, (_context != null) ? (" @" + Context.RuntimeId) : null);
			}
		}

		internal sealed class SetIndexedProperty : DynamicMetaObjectBinder, IInteropBinder
		{
			private readonly RubyContext _context;

			private readonly GetMember _getMember;

			private readonly SetIndex _setIndex;

			public RubyContext Context
			{
				get
				{
					return _context;
				}
			}

			internal SetIndexedProperty(RubyContext context, string name, CallInfo callInfo)
			{
				_context = context;
				_getMember = context.MetaBinderFactory.InteropGetMember(name);
				_setIndex = context.MetaBinderFactory.InteropSetIndex(callInfo);
			}

			public override T BindDelegate<T>(CallSite<T> site, object[] args)
			{
				if (_context.Options.NoAdaptiveCompilation)
				{
					return null;
				}
				T val = this.LightBind<T>(args, _context.Options.CompilationThreshold);
				CacheTarget(val);
				return val;
			}

			public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
			{
				ExpressionCollectionBuilder expressionCollectionBuilder = new ExpressionCollectionBuilder();
				expressionCollectionBuilder.Add(Microsoft.Scripting.Ast.Utils.LightDynamic(_getMember, typeof(object), target.Expression));
				expressionCollectionBuilder.Add(args.ToExpressions());
				return new DynamicMetaObject(Microsoft.Scripting.Ast.Utils.LightDynamic(_setIndex, typeof(object), expressionCollectionBuilder), BindingRestrictions.Empty);
			}

			public override string ToString()
			{
				return string.Format("Interop.SetIndexedProperty({0}, {1}){2}", _getMember.Name, _setIndex.CallInfo.ArgumentCount, (_context != null) ? (" @" + Context.RuntimeId) : null);
			}
		}

		internal sealed class BinaryOperation : BinaryOperationBinder, IInteropBinder
		{
			internal static readonly CallInfo _CallInfo = new CallInfo(1);

			private readonly RubyContext _context;

			public RubyContext Context
			{
				get
				{
					return _context;
				}
			}

			internal BinaryOperation(RubyContext context, ExpressionType operation)
				: base(operation)
			{
				_context = context;
			}

			public override T BindDelegate<T>(CallSite<T> site, object[] args)
			{
				if (_context.Options.NoAdaptiveCompilation)
				{
					return null;
				}
				T val = this.LightBind<T>(args, _context.Options.CompilationThreshold);
				CacheTarget(val);
				return val;
			}

			public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
			{
				string methodName = RubyUtils.MapOperator(base.Operation);
				return InvokeMember.FallbackInvokeMember(this, methodName, _CallInfo, target, new DynamicMetaObject[1] { arg }, errorSuggestion, null);
			}

			public static DynamicMetaObject Bind(DynamicMetaObject context, BinaryOperationBinder binder, DynamicMetaObject target, DynamicMetaObject arg, Func<DynamicMetaObject, DynamicMetaObject, DynamicMetaObject> fallback)
			{
				string methodName = RubyUtils.MapOperator(binder.Operation);
				return InvokeMember.Bind(context, methodName, _CallInfo, binder, target, new DynamicMetaObject[1] { arg }, (DynamicMetaObject trgt, DynamicMetaObject[] args) => fallback(trgt, args[0]));
			}

			public override string ToString()
			{
				return string.Format("Interop.BinaryOperation({0}){1}", base.Operation, (_context != null) ? (" @" + Context.RuntimeId) : null);
			}
		}

		internal sealed class UnaryOperation : UnaryOperationBinder, IInteropBinder
		{
			internal static readonly CallInfo _CallInfo = new CallInfo(0);

			private static DynamicMetaObject[] _MetaArgumentOne;

			private readonly RubyContext _context;

			public RubyContext Context
			{
				get
				{
					return _context;
				}
			}

			private static DynamicMetaObject[] MetaArgumentOne
			{
				get
				{
					return _MetaArgumentOne ?? (_MetaArgumentOne = new DynamicMetaObject[1]
					{
						new DynamicMetaObject(Expression.Constant(ScriptingRuntimeHelpers.Int32ToObject(1), typeof(int)), BindingRestrictions.Empty, ScriptingRuntimeHelpers.Int32ToObject(1))
					});
				}
			}

			internal UnaryOperation(RubyContext context, ExpressionType operation)
				: base(operation)
			{
				_context = context;
			}

			public override T BindDelegate<T>(CallSite<T> site, object[] args)
			{
				if (_context.Options.NoAdaptiveCompilation)
				{
					return null;
				}
				T val = this.LightBind<T>(args, _context.Options.CompilationThreshold);
				CacheTarget(val);
				return val;
			}

			public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
			{
				string methodName = RubyUtils.MapOperator(base.Operation);
				return InvokeMember.FallbackInvokeMember(this, methodName, _CallInfo, target, DynamicMetaObject.EmptyMetaObjects, errorSuggestion, null);
			}

			public static DynamicMetaObject Bind(DynamicMetaObject context, UnaryOperationBinder binder, DynamicMetaObject target, Func<DynamicMetaObject, DynamicMetaObject> fallback)
			{
				if (binder.Operation == ExpressionType.Decrement)
				{
					return InvokeMember.Bind(context, "-", BinaryOperation._CallInfo, binder, target, MetaArgumentOne, (DynamicMetaObject trgt, DynamicMetaObject[] _) => fallback(trgt));
				}
				if (binder.Operation == ExpressionType.Increment)
				{
					return InvokeMember.Bind(context, "+", BinaryOperation._CallInfo, binder, target, MetaArgumentOne, (DynamicMetaObject trgt, DynamicMetaObject[] _) => fallback(trgt));
				}
				string methodName = RubyUtils.MapOperator(binder.Operation);
				return InvokeMember.Bind(context, methodName, _CallInfo, binder, target, DynamicMetaObject.EmptyMetaObjects, (DynamicMetaObject trgt, DynamicMetaObject[] _) => fallback(trgt));
			}

			public override string ToString()
			{
				return string.Format("Interop.UnaryOperation({0}){1}", base.Operation, (_context != null) ? (" @" + Context.RuntimeId) : null);
			}
		}

		internal sealed class Convert : ConvertBinder, IInteropBinder
		{
			private readonly RubyContext _context;

			public RubyContext Context
			{
				get
				{
					return _context;
				}
			}

			internal Convert(RubyContext context, Type type, bool isExplicit)
				: base(type, isExplicit)
			{
				_context = context;
			}

			public override T BindDelegate<T>(CallSite<T> site, object[] args)
			{
				if (_context.Options.NoAdaptiveCompilation)
				{
					return null;
				}
				T val = this.LightBind<T>(args, _context.Options.CompilationThreshold);
				CacheTarget(val);
				return val;
			}

			public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
			{
				DynamicMetaObject result;
				if (ComBinder.TryConvert(this, target, out result))
				{
					return result;
				}
				MetaObjectBuilder metaObjectBuilder = new MetaObjectBuilder(this, target, DynamicMetaObject.EmptyMetaObjects);
				if (!GenericConversionAction.BuildConversion(metaObjectBuilder, target, Expression.Constant(_context), base.Type, errorSuggestion == null))
				{
					metaObjectBuilder.SetMetaResult(errorSuggestion, false);
				}
				return metaObjectBuilder.CreateMetaObject(this);
			}

			public override string ToString()
			{
				return string.Format("Interop.Convert({0}, {1}){2}", base.Type.Name, base.Explicit ? "explicit" : "implicit", (_context != null) ? (" @" + Context.RuntimeId) : null);
			}
		}

		internal sealed class Splat : ConvertBinder, IInteropBinder
		{
			private readonly RubyContext _context;

			public RubyContext Context
			{
				get
				{
					return _context;
				}
			}

			internal Splat(RubyContext context)
				: base(typeof(IList), true)
			{
				_context = context;
			}

			public override T BindDelegate<T>(CallSite<T> site, object[] args)
			{
				if (_context.Options.NoAdaptiveCompilation)
				{
					return null;
				}
				T val = this.LightBind<T>(args, _context.Options.CompilationThreshold);
				CacheTarget(val);
				return val;
			}

			public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
			{
				DynamicMetaObject result;
				if (ComBinder.TryConvert(this, target, out result))
				{
					return result;
				}
				return target.Clone(Expression.NewArrayInit(typeof(object), target.Expression));
			}

			public override string ToString()
			{
				return "Interop.Splat" + ((_context != null) ? (" @" + Context.RuntimeId) : null);
			}
		}

		internal static DynamicMetaObject TryBindCovertToDelegate(RubyMetaObject target, ConvertBinder binder, MethodInfo delegateFactory)
		{
			MetaObjectBuilder metaObjectBuilder = new MetaObjectBuilder(target);
			if (!TryBuildConversionToDelegate(metaObjectBuilder, target, binder.Type, delegateFactory))
			{
				return null;
			}
			return metaObjectBuilder.CreateMetaObject(binder);
		}

		internal static bool TryBuildConversionToDelegate(MetaObjectBuilder metaBuilder, RubyMetaObject target, Type delegateType, MethodInfo delegateFactory)
		{
			if (!typeof(Delegate).IsAssignableFrom(delegateType) || delegateType.GetMethod("Invoke") == null)
			{
				return false;
			}
			Type type = target.Value.GetType();
			metaBuilder.AddTypeRestriction(type, target.Expression);
			metaBuilder.Result = delegateFactory.OpCall(Microsoft.Scripting.Ast.Utils.Constant(delegateType), Expression.Convert(target.Expression, type));
			return true;
		}

		internal static DynamicMetaObject[] InplaceConvertComArguments(DynamicMetaObject[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				args[i] = ConvertComArgument(args[i]);
			}
			return args;
		}

		internal static DynamicMetaObject ConvertComArgument(DynamicMetaObject arg)
		{
			Expression newExpression = arg.Expression;
			BindingRestrictions newRestrictions;
			if (arg.Value != null)
			{
				Type type = arg.Value.GetType();
				if (type == typeof(BigInteger))
				{
					newExpression = Expression.Convert(Microsoft.Scripting.Ast.Utils.Convert(arg.Expression, typeof(BigInteger)), typeof(double));
				}
				else if (type == typeof(MutableString))
				{
					newExpression = Expression.Convert(Microsoft.Scripting.Ast.Utils.Convert(arg.Expression, typeof(MutableString)), typeof(string));
				}
				else if (type == typeof(RubySymbol))
				{
					newExpression = Expression.Convert(Microsoft.Scripting.Ast.Utils.Convert(arg.Expression, typeof(RubySymbol)), typeof(string));
				}
				newRestrictions = BindingRestrictions.GetTypeRestriction(arg.Expression, type);
			}
			else
			{
				newRestrictions = BindingRestrictions.GetExpressionRestriction(Expression.Equal(arg.Expression, Microsoft.Scripting.Ast.Utils.Constant(null)));
			}
			return arg.Clone(newExpression, newRestrictions);
		}
	}
}
