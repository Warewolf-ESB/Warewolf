using System;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using IronRuby.Compiler;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace IronRuby.Builtins
{
	[DebuggerDisplay("{GetDebugView(), nq}")]
	public class RubyMethod : IRubyDynamicMetaObjectProvider, IDynamicMetaObjectProvider
	{
		[DebuggerDisplay("{GetCurriedDebugView(), nq}")]
		public sealed class Curried : RubyMethod
		{
			private readonly string _methodNameArg;

			internal Curried(object target, RubyMemberInfo info, string methodNameArg)
				: base(target, info, "method_missing")
			{
				_methodNameArg = methodNameArg;
			}

			internal override void BuildInvoke(MetaObjectBuilder metaBuilder, CallArguments args)
			{
				args.InsertMethodName(_methodNameArg);
				base.BuildInvoke(metaBuilder, args);
			}

			public override Proc ToProc(RubyScope scope)
			{
				throw new NotSupportedException();
			}

			private string GetCurriedDebugView()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("missing ");
				stringBuilder.Append(GetTargetClass().Name);
				stringBuilder.Append('#');
				stringBuilder.Append(_methodNameArg);
				stringBuilder.Append("(?)");
				return stringBuilder.ToString();
			}
		}

		internal sealed class Meta : RubyMetaObject<RubyMethod>
		{
			public override RubyContext Context
			{
				get
				{
					return base.Value.Info.Context;
				}
			}

			protected override MethodInfo ContextConverter
			{
				get
				{
					return Methods.GetContextFromMethod;
				}
			}

			public Meta(Expression expression, BindingRestrictions restrictions, RubyMethod value)
				: base(expression, restrictions, value)
			{
			}

			public override DynamicMetaObject BindConvert(ConvertBinder binder)
			{
				return InteropBinder.TryBindCovertToDelegate(this, binder, Methods.CreateDelegateFromMethod) ?? base.BindConvert(binder);
			}

			public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
			{
				return InteropBinder.Invoke.Bind(binder, this, args, base.Value.BuildInvoke);
			}
		}

		private readonly object _target;

		private readonly string _name;

		private readonly RubyMemberInfo _info;

		private BlockDispatcherUnsplatN _procDispatcher;

		public object Target
		{
			get
			{
				return _target;
			}
		}

		public RubyMemberInfo Info
		{
			get
			{
				return _info;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public RubyMethod(object target, RubyMemberInfo info, string name)
		{
			ContractUtils.RequiresNotNull(info, "info");
			ContractUtils.RequiresNotNull(name, "name");
			_target = target;
			_info = info;
			_name = name;
		}

		public RubyClass GetTargetClass()
		{
			return _info.Context.GetClassOf(_target);
		}

		public virtual Proc ToProc(RubyScope scope)
		{
			ContractUtils.RequiresNotNull(scope, "scope");
			if (_procDispatcher == null)
			{
				CallSite<Func<CallSite, object, object, object>> site = CallSite<Func<CallSite, object, object, object>>.Create(RubyCallAction.Make(scope.RubyContext, "call", new RubyCallSignature(1, (RubyCallFlags)18)));
				Func<BlockParam, object, object[], RubyArray, object> method = (BlockParam blockParam, object self, object[] args, RubyArray unsplat) => site.Target(site, this, unsplat);
				_procDispatcher = new BlockDispatcherUnsplatN(0, BlockDispatcher.MakeAttributes(BlockSignatureAttributes.HasUnsplatParameter, _info.GetArity()), null, 0);
				_procDispatcher.SetMethod(method);
			}
			return new Proc(ProcKind.Block, scope.SelfObject, scope, _procDispatcher);
		}

		internal virtual void BuildInvoke(MetaObjectBuilder metaBuilder, CallArguments args)
		{
			metaBuilder.AddRestriction(Expression.Equal(args.TargetExpression, Microsoft.Scripting.Ast.Utils.Constant(this)));
			args.SetTarget(Microsoft.Scripting.Ast.Utils.Constant(_target, CompilerHelpers.GetVisibleType(_target)), _target);
			_info.BuildCall(metaBuilder, args, _name);
		}

		private string GetDebugView()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(_info.Visibility.ToString().ToLowerInvariant());
			stringBuilder.Append(' ');
			stringBuilder.Append(GetTargetClass().Name);
			stringBuilder.Append('#');
			stringBuilder.Append(_name);
			stringBuilder.Append("()");
			return stringBuilder.ToString();
		}

		public DynamicMetaObject GetMetaObject(Expression parameter)
		{
			return new Meta(parameter, BindingRestrictions.Empty, this);
		}
	}
}
