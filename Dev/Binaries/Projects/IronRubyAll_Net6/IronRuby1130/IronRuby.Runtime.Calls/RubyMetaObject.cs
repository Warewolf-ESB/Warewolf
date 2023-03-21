using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Compiler;
using IronRuby.Runtime.Conversions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls
{
	public abstract class RubyMetaObject : DynamicMetaObject
	{
		public abstract RubyContext Context { get; }

		public abstract Expression ContextExpression { get; }

		internal RubyMetaObject(Expression expression, BindingRestrictions restrictions, object value)
			: base(expression, restrictions, value)
		{
			ContractUtils.RequiresNotNull(value, "value");
		}

		internal DynamicMetaObject CreateMetaContext()
		{
			return new DynamicMetaObject(ContextExpression, BindingRestrictions.Empty, Context);
		}

		public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
		{
			return InteropBinder.InvokeMember.Bind(CreateMetaContext(), binder, this, args, binder.FallbackInvokeMember);
		}

		public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
		{
			return InteropBinder.GetMember.Bind(CreateMetaContext(), binder, this, binder.FallbackGetMember);
		}

		public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
		{
			return InteropBinder.SetMember.Bind(CreateMetaContext(), binder, this, value, binder.FallbackSetMember);
		}

		public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
		{
			return InteropBinder.GetIndex.Bind(CreateMetaContext(), binder, this, indexes, binder.FallbackGetIndex);
		}

		public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
		{
			return InteropBinder.SetIndex.Bind(CreateMetaContext(), binder, this, indexes, value, binder.FallbackSetIndex);
		}

		public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
		{
			return InteropBinder.UnaryOperation.Bind(CreateMetaContext(), binder, this, binder.FallbackUnaryOperation);
		}

		public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
		{
			return InteropBinder.BinaryOperation.Bind(CreateMetaContext(), binder, this, arg, binder.FallbackBinaryOperation);
		}

		public override DynamicMetaObject BindConvert(ConvertBinder binder)
		{
			RubyConversionAction rubyConversionAction = RubyConversionAction.TryGetDefaultConversionAction(Context, binder.Type);
			if (rubyConversionAction != null)
			{
				return rubyConversionAction.Bind(this, DynamicMetaObject.EmptyMetaObjects);
			}
			return binder.FallbackConvert(this);
		}
	}
	public abstract class RubyMetaObject<T> : RubyMetaObject
	{
		protected abstract MethodInfo ContextConverter { get; }

		public new T Value
		{
			get
			{
				return (T)base.Value;
			}
		}

		public sealed override Expression ContextExpression
		{
			get
			{
				return ContextConverter.OpCall(Microsoft.Scripting.Ast.Utils.Convert(base.Expression, typeof(T)));
			}
		}

		public RubyMetaObject(Expression expression, BindingRestrictions restrictions, T value)
			: base(expression, restrictions, value)
		{
		}
	}
}
