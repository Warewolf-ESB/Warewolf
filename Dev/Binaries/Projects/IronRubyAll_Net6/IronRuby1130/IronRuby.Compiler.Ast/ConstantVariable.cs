using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class ConstantVariable : Variable
	{
		private const int OpGet = 0;

		private const int OpIsDefined = 1;

		private readonly bool _explicitlyBound;

		private readonly Expression _qualifier;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.ConstantVariable;
			}
		}

		public Expression Qualifier
		{
			get
			{
				return _qualifier;
			}
		}

		public bool IsGlobal
		{
			get
			{
				if (_explicitlyBound)
				{
					return _qualifier == null;
				}
				return false;
			}
		}

		public bool IsBound
		{
			get
			{
				return _explicitlyBound;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public ConstantVariable(string name, SourceSpan location)
			: base(name, location)
		{
			_qualifier = null;
			_explicitlyBound = false;
		}

		public ConstantVariable(Expression qualifier, string name, SourceSpan location)
			: base(name, location)
		{
			_qualifier = qualifier;
			_explicitlyBound = true;
		}

		internal StaticScopeKind TransformQualifier(AstGenerator gen, out System.Linq.Expressions.Expression transformedQualifier)
		{
			if (_qualifier != null)
			{
				transformedQualifier = _qualifier.TransformRead(gen);
				return StaticScopeKind.Explicit;
			}
			if (_explicitlyBound)
			{
				transformedQualifier = null;
				return StaticScopeKind.Global;
			}
			transformedQualifier = null;
			return StaticScopeKind.EnclosingModule;
		}

		internal override System.Linq.Expressions.Expression TransformReadVariable(AstGenerator gen, bool tryRead)
		{
			return TransformRead(gen, 0);
		}

		private System.Linq.Expressions.Expression TransformRead(AstGenerator gen, int opKind)
		{
			ConstantVariable constantVariable = _qualifier as ConstantVariable;
			if (constantVariable != null)
			{
				List<string> list = new List<string>();
				list.Add(base.Name);
				ConstantVariable constantVariable2;
				do
				{
					list.Add(constantVariable.Name);
					constantVariable2 = constantVariable;
					constantVariable = constantVariable.Qualifier as ConstantVariable;
				}
				while (constantVariable != null);
				if (constantVariable2.Qualifier != null)
				{
					return constantVariable2.MakeExpressionQualifiedRead(gen, opKind, list.ToReverseArray());
				}
				return MakeCachedRead(gen, opKind, constantVariable2.IsGlobal, true, System.Linq.Expressions.Expression.Constant(list.ToReverseArray()));
			}
			if (_qualifier != null)
			{
				return MakeExpressionQualifiedRead(gen, opKind, new string[1] { base.Name });
			}
			return MakeCachedRead(gen, opKind, IsGlobal, false, System.Linq.Expressions.Expression.Constant(base.Name));
		}

		private System.Linq.Expressions.Expression MakeExpressionQualifiedRead(AstGenerator gen, int opKind, string[] names)
		{
			object value;
			MethodInfo method;
			if (opKind == 1)
			{
				value = new ExpressionQualifiedIsDefinedConstantSiteCache();
				method = Methods.IsDefinedExpressionQualifiedConstant;
			}
			else
			{
				value = new ExpressionQualifiedConstantSiteCache();
				method = Methods.GetExpressionQualifiedConstant;
			}
			System.Linq.Expressions.Expression expression = method.OpCall(Microsoft.Scripting.Ast.Utils.Box(_qualifier.TransformRead(gen)), gen.CurrentScopeVariable, System.Linq.Expressions.Expression.Constant(value), System.Linq.Expressions.Expression.Constant(names));
			if (opKind != 1)
			{
				return expression;
			}
			return System.Linq.Expressions.Expression.TryCatch(expression, System.Linq.Expressions.Expression.Catch(typeof(Exception), AstFactory.False));
		}

		private static System.Linq.Expressions.Expression MakeCachedRead(AstGenerator gen, int opKind, bool isGlobal, bool isQualified, System.Linq.Expressions.Expression name)
		{
			object value;
			ParameterExpression cacheVar;
			FieldInfo field;
			System.Linq.Expressions.Expression ifTrue;
			System.Linq.Expressions.Expression ifFalse;
			if (opKind == 1)
			{
				value = new IsDefinedConstantSiteCache();
				gen.CurrentScope.GetIsDefinedConstantSiteCacheVariables(out cacheVar);
				field = Fields.IsDefinedConstantSiteCache_Version;
				FieldInfo isDefinedConstantSiteCache_Value = Fields.IsDefinedConstantSiteCache_Value;
				ifTrue = System.Linq.Expressions.Expression.Field(cacheVar, isDefinedConstantSiteCache_Value);
				ifFalse = (isQualified ? Methods.IsDefinedQualifiedConstant.OpCall(gen.CurrentScopeVariable, cacheVar, name, Microsoft.Scripting.Ast.Utils.Constant(isGlobal)) : (isGlobal ? Methods.IsDefinedGlobalConstant : Methods.IsDefinedUnqualifiedConstant).OpCall(gen.CurrentScopeVariable, cacheVar, name));
			}
			else
			{
				value = new ConstantSiteCache();
				ParameterExpression valueVar;
				gen.CurrentScope.GetConstantSiteCacheVariables(out cacheVar, out valueVar);
				field = Fields.ConstantSiteCache_Version;
				FieldInfo isDefinedConstantSiteCache_Value = Fields.ConstantSiteCache_Value;
				System.Linq.Expressions.Expression expression = System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Convert(valueVar, typeof(WeakReference)), Methods.WeakReference_get_Target);
				if (!isQualified)
				{
					expression = System.Linq.Expressions.Expression.Condition(System.Linq.Expressions.Expression.Equal(valueVar, Microsoft.Scripting.Ast.Utils.Constant(ConstantSiteCache.WeakMissingConstant)), (isGlobal ? Methods.GetGlobalMissingConstant : Methods.GetMissingConstant).OpCall(gen.CurrentScopeVariable, cacheVar, name), expression);
				}
				ifTrue = System.Linq.Expressions.Expression.Condition(System.Linq.Expressions.Expression.TypeEqual(System.Linq.Expressions.Expression.Assign(valueVar, System.Linq.Expressions.Expression.Field(cacheVar, isDefinedConstantSiteCache_Value)), typeof(WeakReference)), expression, valueVar);
				ifFalse = (isQualified ? Methods.GetQualifiedConstant : Methods.GetUnqualifiedConstant).OpCall(gen.CurrentScopeVariable, cacheVar, name, Microsoft.Scripting.Ast.Utils.Constant(isGlobal));
			}
			return System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Condition(System.Linq.Expressions.Expression.Equal(System.Linq.Expressions.Expression.Field(System.Linq.Expressions.Expression.Assign(cacheVar, System.Linq.Expressions.Expression.Constant(value)), field), System.Linq.Expressions.Expression.Field(System.Linq.Expressions.Expression.Constant(gen.Context), Fields.RubyContext_ConstantAccessVersion)), ifTrue, ifFalse));
		}

		internal override System.Linq.Expressions.Expression TransformWriteVariable(AstGenerator gen, System.Linq.Expressions.Expression rightValue)
		{
			System.Linq.Expressions.Expression expression = TransformName(gen);
			System.Linq.Expressions.Expression transformedQualifier;
			switch (TransformQualifier(gen, out transformedQualifier))
			{
			case StaticScopeKind.Global:
				return Methods.SetGlobalConstant.OpCall(Microsoft.Scripting.Ast.Utils.Box(rightValue), gen.CurrentScopeVariable, expression);
			case StaticScopeKind.EnclosingModule:
				return Methods.SetUnqualifiedConstant.OpCall(Microsoft.Scripting.Ast.Utils.Box(rightValue), gen.CurrentScopeVariable, expression);
			case StaticScopeKind.Explicit:
				return Methods.SetQualifiedConstant.OpCall(Microsoft.Scripting.Ast.Utils.Box(rightValue), transformedQualifier, gen.CurrentScopeVariable, expression);
			default:
				throw Assert.Unreachable;
			}
		}

		internal override System.Linq.Expressions.Expression TransformDefinedCondition(AstGenerator gen)
		{
			return TransformRead(gen, 1);
		}

		internal override string GetNodeName(AstGenerator gen)
		{
			return "constant";
		}
	}
}
