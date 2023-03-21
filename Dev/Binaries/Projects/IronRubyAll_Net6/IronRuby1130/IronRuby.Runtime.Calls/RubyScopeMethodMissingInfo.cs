using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Builtins;
using IronRuby.Compiler;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Runtime;

namespace IronRuby.Runtime.Calls
{
	public sealed class RubyScopeMethodMissingInfo : RubyMemberInfo
	{
		internal RubyScopeMethodMissingInfo(RubyMemberFlags flags, RubyModule declaringModule)
			: base(flags, declaringModule)
		{
		}

		protected internal override RubyMemberInfo Copy(RubyMemberFlags flags, RubyModule module)
		{
			return new RubyScopeMethodMissingInfo(flags, module);
		}

		public override int GetArity()
		{
			return -1;
		}

		public override MemberInfo[] GetMembers()
		{
			return new MemberInfo[0];
		}

		internal override void BuildCallNoFlow(MetaObjectBuilder metaBuilder, CallArguments args, string name)
		{
			RubyGlobalScope globalScope = args.TargetClass.GlobalScope;
			metaBuilder.Result = Microsoft.Scripting.Ast.Utils.LightDynamic(new RubyCallAction(globalScope.Context, Symbols.MethodMissing, new RubyCallSignature(args.Signature.ArgumentCount, args.Signature.Flags | RubyCallFlags.HasImplicitSelf | RubyCallFlags.IsSuperCall)), typeof(object), args.GetCallSiteArguments(args.TargetExpression));
		}

		internal override void BuildMethodMissingCallNoFlow(MetaObjectBuilder metaBuilder, CallArguments args, string name)
		{
			RubyGlobalScope globalScope = args.TargetClass.GlobalScope;
			RubyContext context = globalScope.Context;
			if (name.LastCharacter() == 61)
			{
				IList<DynamicMetaObject> list = RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 1, 1);
				if (!metaBuilder.Error)
				{
					ParameterExpression temporary = metaBuilder.GetTemporary(typeof(Scope), "#scope");
					metaBuilder.AddInitialization(Expression.Assign(temporary, Methods.GetGlobalScopeFromScope.OpCall(Microsoft.Scripting.Ast.Utils.Convert(args.MetaScope.Expression, typeof(RubyScope)))));
					InteropBinder.SetMember setMember = context.MetaBinderFactory.InteropSetMember(name.Substring(0, name.Length - 1));
					metaBuilder.SetMetaResult(setMember.Bind(new DynamicMetaObject(temporary, BindingRestrictions.Empty, globalScope.Scope), new DynamicMetaObject[1] { list[0] }), true);
				}
				return;
			}
			RubyOverloadResolver.NormalizeArguments(metaBuilder, args, 0, 0);
			Expression expression = (metaBuilder.Error ? Expression.Throw(metaBuilder.Result, typeof(object)) : null);
			ParameterExpression temporary2 = metaBuilder.GetTemporary(typeof(Scope), "#scope");
			ParameterExpression temporary3 = metaBuilder.GetTemporary(typeof(object), "#result");
			metaBuilder.AddInitialization(Expression.Assign(temporary2, Methods.GetGlobalScopeFromScope.OpCall(Microsoft.Scripting.Ast.Utils.Convert(args.MetaScope.Expression, typeof(RubyScope)))));
			Expression ifTrue = expression ?? temporary3;
			Expression ifFalse;
			if (name == "scope")
			{
				ifFalse = expression ?? args.TargetExpression;
			}
			else
			{
				args.InsertMethodName(name);
				ifFalse = Microsoft.Scripting.Ast.Utils.LightDynamic(context.MetaBinderFactory.Call(Symbols.MethodMissing, new RubyCallSignature(args.Signature.ArgumentCount + 1, args.Signature.Flags | RubyCallFlags.HasImplicitSelf | RubyCallFlags.IsSuperCall)), typeof(object), args.GetCallSiteArguments(args.TargetExpression));
			}
			BinaryExpression binaryExpression = Expression.NotEqual(Expression.Assign(temporary3, Microsoft.Scripting.Ast.Utils.LightDynamic(context.MetaBinderFactory.InteropTryGetMemberExact(name), typeof(object), temporary2)), Expression.Constant(OperationFailed.Value));
			string text = RubyUtils.TryUnmangleMethodName(name);
			if (text != null)
			{
				binaryExpression = Expression.OrElse(binaryExpression, Expression.NotEqual(Expression.Assign(temporary3, Microsoft.Scripting.Ast.Utils.LightDynamic(context.MetaBinderFactory.InteropTryGetMemberExact(text), typeof(object), temporary2)), Expression.Constant(OperationFailed.Value)));
			}
			metaBuilder.Result = Expression.Condition(binaryExpression, ifTrue, ifFalse);
		}
	}
}
