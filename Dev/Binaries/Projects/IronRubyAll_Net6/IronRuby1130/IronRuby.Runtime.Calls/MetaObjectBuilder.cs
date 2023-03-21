using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Compiler;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Runtime.Calls
{
	public sealed class MetaObjectBuilder
	{
		private readonly RubyContext _siteContext;

		private Expression _condition;

		private BindingRestrictions _restrictions;

		private Expression _result;

		private ReadOnlyCollectionBuilder<Expression> _initializations;

		private List<ParameterExpression> _temps;

		private bool _error;

		private bool _treatRestrictionsAsConditions;

		public bool Error
		{
			get
			{
				return _error;
			}
		}

		public Expression Result
		{
			get
			{
				return _result;
			}
			set
			{
				_result = value;
				_error = false;
			}
		}

		public ParameterExpression BfcVariable { get; set; }

		public Action<MetaObjectBuilder, CallArguments> ControlFlowBuilder { get; set; }

		public bool TreatRestrictionsAsConditions
		{
			get
			{
				return _treatRestrictionsAsConditions;
			}
			set
			{
				_treatRestrictionsAsConditions = value;
			}
		}

		internal MetaObjectBuilder(RubyMetaBinder binder, DynamicMetaObject[] arguments)
			: this(binder.Context, null, arguments)
		{
		}

		internal MetaObjectBuilder(IInteropBinder binder, DynamicMetaObject target, params DynamicMetaObject[] arguments)
			: this(binder.Context, target, arguments)
		{
		}

		internal MetaObjectBuilder(DynamicMetaObject target, params DynamicMetaObject[] arguments)
			: this((RubyContext)null, target, arguments)
		{
		}

		private MetaObjectBuilder(RubyContext siteContext, DynamicMetaObject target, DynamicMetaObject[] arguments)
		{
			BindingRestrictions restrictions = BindingRestrictions.Combine(arguments);
			if (target != null)
			{
				restrictions = target.Restrictions.Merge(restrictions);
			}
			_restrictions = restrictions;
			_siteContext = siteContext;
		}

		private void Clear()
		{
			_condition = null;
			_restrictions = BindingRestrictions.Empty;
			_result = null;
			_initializations = null;
			_error = false;
			_treatRestrictionsAsConditions = false;
		}

		internal DynamicMetaObject CreateMetaObject(DynamicMetaObjectBinder action)
		{
			return CreateMetaObject(action, action.ReturnType);
		}

		internal DynamicMetaObject CreateMetaObject(DynamicMetaObjectBinder binder, Type returnType)
		{
			BindingRestrictions restrictions = _restrictions;
			Expression expression = (_error ? Expression.Throw(_result, returnType) : Microsoft.Scripting.Ast.Utils.Convert(_result, returnType));
			if (_condition != null)
			{
				Expression updateExpression = binder.GetUpdateExpression(returnType);
				expression = Expression.Condition(_condition, expression, updateExpression);
			}
			if (_temps != null || _initializations != null)
			{
				AddInitialization(expression);
				expression = ((_temps == null) ? Expression.Block(_initializations) : Expression.Block(_temps, _initializations));
			}
			Clear();
			return new DynamicMetaObject(expression, restrictions);
		}

		public ParameterExpression GetTemporary(Type type, string name)
		{
			return AddTemporary(Expression.Variable(type, name));
		}

		private ParameterExpression AddTemporary(ParameterExpression variable)
		{
			if (_temps == null)
			{
				_temps = new List<ParameterExpression>();
			}
			_temps.Add(variable);
			return variable;
		}

		internal void AddInitialization(Expression expression)
		{
			if (_initializations == null)
			{
				_initializations = new ReadOnlyCollectionBuilder<Expression>();
			}
			_initializations.Add(expression);
		}

		public void BuildControlFlow(CallArguments args)
		{
			if (ControlFlowBuilder != null)
			{
				ControlFlowBuilder(this, args);
				ControlFlowBuilder = null;
			}
		}

		public void SetError(Expression expression)
		{
			_result = expression;
			_error = true;
		}

		public void SetWrongNumberOfArgumentsError(int actual, int expected)
		{
			SetError(Methods.MakeWrongNumberOfArgumentsError.OpCall(Microsoft.Scripting.Ast.Utils.Constant(actual), Microsoft.Scripting.Ast.Utils.Constant(expected)));
		}

		public void SetMetaResult(DynamicMetaObject metaResult, CallArguments args)
		{
			SetMetaResult(metaResult, args.Signature.HasSplattedArgument);
		}

		public void SetMetaResult(DynamicMetaObject metaResult, bool treatRestrictionsAsConditions)
		{
			Result = metaResult.Expression;
			if (treatRestrictionsAsConditions || _treatRestrictionsAsConditions)
			{
				AddCondition(metaResult.Restrictions.ToExpression());
			}
			else
			{
				Add(metaResult.Restrictions);
			}
		}

		public void AddObjectTypeRestriction(object value, Expression expression)
		{
			if (value == null)
			{
				AddRestriction(Expression.Equal(expression, Microsoft.Scripting.Ast.Utils.Constant(null)));
			}
			else
			{
				AddTypeRestriction(value.GetType(), expression);
			}
		}

		public void AddTypeRestriction(Type type, Expression expression)
		{
			if (_treatRestrictionsAsConditions)
			{
				AddCondition(Expression.TypeEqual(expression, type));
			}
			else if (expression.Type != type || !type.IsSealed)
			{
				Add(BindingRestrictions.GetTypeRestriction(expression, type));
			}
		}

		public void AddRestriction(Expression restriction)
		{
			if (_treatRestrictionsAsConditions)
			{
				AddCondition(restriction);
			}
			else
			{
				Add(BindingRestrictions.GetExpressionRestriction(restriction));
			}
		}

		public void AddRestriction(BindingRestrictions restriction)
		{
			if (_treatRestrictionsAsConditions)
			{
				AddCondition(restriction.ToExpression());
			}
			else
			{
				Add(restriction);
			}
		}

		private void Add(BindingRestrictions restriction)
		{
			_restrictions = _restrictions.Merge(restriction);
		}

		public void AddCondition(Expression condition)
		{
			_condition = ((_condition != null) ? Expression.AndAlso(_condition, condition) : condition);
		}

		public void AddTargetTypeTest(object target, RubyClass targetClass, Expression targetParameter, DynamicMetaObject metaContext, IEnumerable<string> resolvedNames)
		{
			targetClass.InitializeMethodsNoLock();
			RubyContext rubyContext = (RubyContext)metaContext.Value;
			if (target is IRubyObject)
			{
				Type type = target.GetType();
				AddTypeRestriction(type, targetParameter);
				MethodInfo method = type.GetMethod(Methods.IRubyObject_get_ImmediateClass.Name, BindingFlags.Instance | BindingFlags.Public);
				if (type.IsVisible && method != null && method.ReturnType == typeof(RubyClass))
				{
					AddCondition(Expression.Equal(Expression.Field(Expression.Field(Expression.Call(Expression.Convert(targetParameter, type), method), Fields.RubyModule_Version), Fields.VersionHandle_Method), Microsoft.Scripting.Ast.Utils.Constant(targetClass.Version.Method)));
					return;
				}
				throw new NotSupportedException("Type implementing IRubyObject should be visible and have ImmediateClass getter");
			}
			AddRuntimeTest(metaContext);
			if (target == null)
			{
				AddRestriction(Expression.Equal(targetParameter, Microsoft.Scripting.Ast.Utils.Constant(null)));
				AddVersionTest(rubyContext.NilClass);
				return;
			}
			if (target is bool)
			{
				AddRestriction(Expression.AndAlso(Expression.TypeIs(targetParameter, typeof(bool)), Expression.Equal(Expression.Convert(targetParameter, typeof(bool)), Microsoft.Scripting.Ast.Utils.Constant(target))));
				AddVersionTest(((bool)target) ? rubyContext.TrueClass : rubyContext.FalseClass);
				return;
			}
			RubyClass nominalClass = targetClass.NominalClass;
			if (nominalClass.ClrSingletonMethods == null || CollectionUtils.TrueForAll(resolvedNames, (string methodName) => !nominalClass.ClrSingletonMethods.ContainsKey(methodName)))
			{
				AddTypeRestriction(target.GetType(), targetParameter);
				AddVersionTest(targetClass);
			}
			else if (targetClass.IsSingletonClass)
			{
				AddTypeRestriction(target.GetType(), targetParameter);
				AddCondition(Methods.IsClrSingletonRuleValid.OpCall(metaContext.Expression, targetParameter, Microsoft.Scripting.Ast.Utils.Constant(targetClass.Version.Method)));
			}
			else
			{
				AddTypeRestriction(target.GetType(), targetParameter);
				AddCondition(Methods.IsClrNonSingletonRuleValid.OpCall(metaContext.Expression, targetParameter, Expression.Constant(targetClass.Version), Microsoft.Scripting.Ast.Utils.Constant(targetClass.Version.Method)));
			}
		}

		private void AddRuntimeTest(DynamicMetaObject metaContext)
		{
			if (_siteContext == null)
			{
				AddRestriction(Expression.Equal(metaContext.Expression, Microsoft.Scripting.Ast.Utils.Constant(metaContext.Value)));
			}
			else if (_siteContext != metaContext.Value)
			{
				throw new InvalidOperationException("Runtime-bound site called from a different runtime");
			}
		}

		internal void AddVersionTest(RubyClass cls)
		{
			AddCondition(Expression.Equal(Expression.Field(Microsoft.Scripting.Ast.Utils.Constant(cls.Version), Fields.VersionHandle_Method), Microsoft.Scripting.Ast.Utils.Constant(cls.Version.Method)));
		}

		internal void AddSplattedArgumentTest(IList value, Expression expression, out int listLength, out ParameterExpression listVariable)
		{
			listVariable = expression as ParameterExpression;
			Expression expression2;
			if (listVariable != null && typeof(IList).IsAssignableFrom(expression.Type))
			{
				expression2 = expression;
			}
			else
			{
				listVariable = GetTemporary(typeof(IList), "#list");
				expression2 = Expression.Assign(listVariable, Microsoft.Scripting.Ast.Utils.Convert(expression, typeof(IList)));
			}
			listLength = value.Count;
			AddCondition(Expression.Equal(Expression.Property(expression2, typeof(ICollection).GetProperty("Count")), Microsoft.Scripting.Ast.Utils.Constant(value.Count)));
		}
	}
}
