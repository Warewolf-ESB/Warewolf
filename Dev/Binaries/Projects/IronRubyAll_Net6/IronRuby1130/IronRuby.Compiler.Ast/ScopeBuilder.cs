using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	internal sealed class ScopeBuilder
	{
		private readonly ReadOnlyCollectionBuilder<ParameterExpression> _parameters;

		private readonly int _firstClosureParam;

		private readonly int _localCount;

		private readonly LexicalScope _lexicalScope;

		private readonly ScopeBuilder _parent;

		private readonly ReadOnlyCollectionBuilder<ParameterExpression> _hiddenVariables;

		private ParameterExpression _csCacheVariable;

		private ParameterExpression _csValueVariable;

		private ParameterExpression _dcsCacheVariable;

		private List<ParameterExpression> _closures;

		private ScopeBuilder _outermostClosureReferredTo;

		private readonly ParameterExpression _localsTuple;

		internal ScopeBuilder Parent
		{
			get
			{
				return _parent;
			}
		}

		internal LexicalScope LexicalScope
		{
			get
			{
				return _lexicalScope;
			}
		}

		private int LiftedVisibleVariableCount
		{
			get
			{
				return ((_parameters != null) ? (_parameters.Count - _firstClosureParam) : 0) + _localCount;
			}
		}

		public ScopeBuilder(int localCount, ScopeBuilder parent, LexicalScope lexicalScope)
			: this(null, -1, localCount, parent, lexicalScope)
		{
		}

		public ScopeBuilder(ReadOnlyCollectionBuilder<ParameterExpression> parameters, int firstClosureParam, int localCount, ScopeBuilder parent, LexicalScope lexicalScope)
		{
			_parent = parent;
			_parameters = parameters;
			_localCount = localCount;
			_firstClosureParam = firstClosureParam;
			_lexicalScope = lexicalScope;
			_hiddenVariables = new ReadOnlyCollectionBuilder<ParameterExpression>();
			_localsTuple = DefineHiddenVariable("#locals", MakeLocalsTupleType());
			_outermostClosureReferredTo = this;
		}

		private Type MakeLocalsTupleType()
		{
			return RubyOps.MakeObjectTupleType(LiftedVisibleVariableCount);
		}

		internal ParameterExpression GetClosure(int definitionDepth)
		{
			int num = _lexicalScope.Depth - definitionDepth - 1;
			if (num == -1)
			{
				return _localsTuple;
			}
			if (_closures == null)
			{
				_closures = new List<ParameterExpression>();
			}
			while (num >= _closures.Count)
			{
				_outermostClosureReferredTo = _outermostClosureReferredTo.Parent;
				_closures.Add(DefineHiddenVariable("#closure" + num, _outermostClosureReferredTo._localsTuple.Type));
			}
			return _closures[num];
		}

		public ParameterExpression DefineHiddenVariable(string name, Type type)
		{
			return AddHidden(System.Linq.Expressions.Expression.Variable(type, name));
		}

		public ParameterExpression AddHidden(ParameterExpression variable)
		{
			_hiddenVariables.Add(variable);
			return variable;
		}

		public void GetConstantSiteCacheVariables(out ParameterExpression cacheVar, out ParameterExpression valueVar)
		{
			if (_csCacheVariable == null)
			{
				_csCacheVariable = DefineHiddenVariable("c_site", typeof(ConstantSiteCache));
				_csValueVariable = DefineHiddenVariable("c_value", typeof(object));
			}
			cacheVar = _csCacheVariable;
			valueVar = _csValueVariable;
		}

		public void GetIsDefinedConstantSiteCacheVariables(out ParameterExpression cacheVar)
		{
			if (_dcsCacheVariable == null)
			{
				_dcsCacheVariable = DefineHiddenVariable("dc_site", typeof(IsDefinedConstantSiteCache));
			}
			cacheVar = _dcsCacheVariable;
		}

		public System.Linq.Expressions.Expression GetVariableAccessor(int definitionLexicalDepth, int closureIndex)
		{
			return GetVariableAccessor(GetClosure(definitionLexicalDepth), closureIndex);
		}

		public static System.Linq.Expressions.Expression GetVariableAccessor(System.Linq.Expressions.Expression tupleVariable, int tupleFieldIndex)
		{
			System.Linq.Expressions.Expression expression = tupleVariable;
			foreach (PropertyInfo item in MutableTuple.GetAccessPath(tupleVariable.Type, tupleFieldIndex))
			{
				expression = System.Linq.Expressions.Expression.Property(expression, item);
			}
			return expression;
		}

		public System.Linq.Expressions.Expression MakeLocalsStorage()
		{
			System.Linq.Expressions.Expression expression = System.Linq.Expressions.Expression.Assign(_localsTuple, (LiftedVisibleVariableCount == 0) ? ((System.Linq.Expressions.Expression)System.Linq.Expressions.Expression.Constant(null, _localsTuple.Type)) : ((System.Linq.Expressions.Expression)System.Linq.Expressions.Expression.New(_localsTuple.Type)));
			if (_parameters != null)
			{
				ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>();
				readOnlyCollectionBuilder.Add(expression);
				int num = _firstClosureParam;
				int num2 = 0;
				while (num < _parameters.Count)
				{
					readOnlyCollectionBuilder.Add(System.Linq.Expressions.Expression.Assign(GetVariableAccessor(_localsTuple, num2), _parameters[num]));
					num++;
					num2++;
				}
				readOnlyCollectionBuilder.Add(_localsTuple);
				expression = System.Linq.Expressions.Expression.Block(readOnlyCollectionBuilder);
			}
			return expression;
		}

		public System.Linq.Expressions.Expression GetVariableNamesExpression()
		{
			if (LiftedVisibleVariableCount == 0)
			{
				return System.Linq.Expressions.Expression.Constant(null, typeof(string[]));
			}
			string[] array = new string[LiftedVisibleVariableCount];
			foreach (KeyValuePair<string, LocalVariable> item in (IEnumerable<KeyValuePair<string, LocalVariable>>)_lexicalScope)
			{
				array[item.Value.ClosureIndex] = item.Value.Name;
			}
			return System.Linq.Expressions.Expression.Constant(array);
		}

		public System.Linq.Expressions.Expression CreateScope(System.Linq.Expressions.Expression body)
		{
			return System.Linq.Expressions.Expression.Block(_hiddenVariables, body);
		}

		public System.Linq.Expressions.Expression CreateScope(System.Linq.Expressions.Expression scopeVariable, System.Linq.Expressions.Expression scopeInitializer, System.Linq.Expressions.Expression body)
		{
			if (_closures != null)
			{
				if (_closures.Count == 1)
				{
					return System.Linq.Expressions.Expression.Block(_hiddenVariables, System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Assign(_closures[0], Microsoft.Scripting.Ast.Utils.Convert(Methods.GetParentLocals.OpCall(System.Linq.Expressions.Expression.Assign(scopeVariable, scopeInitializer)), _closures[0].Type)), body));
				}
				ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>();
				System.Linq.Expressions.Expression expression = DefineHiddenVariable("#s", typeof(RubyScope));
				readOnlyCollectionBuilder.Add(System.Linq.Expressions.Expression.Assign(scopeVariable, scopeInitializer));
				for (int i = 0; i < _closures.Count; i++)
				{
					readOnlyCollectionBuilder.Add(System.Linq.Expressions.Expression.Assign(_closures[i], Microsoft.Scripting.Ast.Utils.Convert(Methods.GetLocals.OpCall(System.Linq.Expressions.Expression.Assign(expression, Methods.GetParentScope.OpCall((i == 0) ? scopeVariable : expression))), _closures[i].Type)));
				}
				readOnlyCollectionBuilder.Add(body);
				return System.Linq.Expressions.Expression.Block(_hiddenVariables, readOnlyCollectionBuilder);
			}
			return System.Linq.Expressions.Expression.Block(_hiddenVariables, System.Linq.Expressions.Expression.Block(System.Linq.Expressions.Expression.Assign(scopeVariable, scopeInitializer), body));
		}
	}
}
