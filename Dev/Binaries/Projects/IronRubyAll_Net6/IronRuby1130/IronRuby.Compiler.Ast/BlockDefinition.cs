using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronRuby.Builtins;
using IronRuby.Runtime;
using IronRuby.Runtime.Calls;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class BlockDefinition : Block
	{
		internal const int HiddenParameterCount = 2;

		internal const int MaxBlockArity = 4;

		private readonly Parameters _parameters;

		private readonly Statements _body;

		private readonly LexicalScope _definedScope;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.BlockDefinition;
			}
		}

		public Parameters Parameters
		{
			get
			{
				return _parameters;
			}
		}

		public Statements Body
		{
			get
			{
				return _body;
			}
		}

		public sealed override bool IsDefinition
		{
			get
			{
				return true;
			}
		}

		private int ParameterCount
		{
			get
			{
				return _parameters.Mandatory.Length + _parameters.Optional.Length;
			}
		}

		private bool HasFormalParametersInArray
		{
			get
			{
				if (ParameterCount <= 4 && !HasUnsplatParameter)
				{
					return HasProcParameter;
				}
				return true;
			}
		}

		private bool HasUnsplatParameter
		{
			get
			{
				return _parameters.Unsplat != null;
			}
		}

		private bool HasProcParameter
		{
			get
			{
				return _parameters.Block != null;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public BlockDefinition(LexicalScope definedScope, Parameters parameters, Statements body, SourceSpan location)
			: base(location)
		{
			_definedScope = definedScope;
			_body = body;
			_parameters = parameters ?? Parameters.Empty;
		}

		private ReadOnlyCollectionBuilder<ParameterExpression> DefineParameters(out ParameterExpression selfVariable, out ParameterExpression blockParamVariable)
		{
			ReadOnlyCollectionBuilder<ParameterExpression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<ParameterExpression>(2 + (HasFormalParametersInArray ? 1 : ParameterCount) + (HasUnsplatParameter ? 1 : 0) + (HasProcParameter ? 1 : 0));
			readOnlyCollectionBuilder.Add(blockParamVariable = System.Linq.Expressions.Expression.Parameter(typeof(BlockParam), "#bp"));
			readOnlyCollectionBuilder.Add(selfVariable = System.Linq.Expressions.Expression.Parameter(typeof(object), "#self"));
			if (HasFormalParametersInArray)
			{
				readOnlyCollectionBuilder.Add(System.Linq.Expressions.Expression.Parameter(typeof(object[]), "#parameters"));
			}
			else
			{
				for (int i = 0; i < ParameterCount; i++)
				{
					readOnlyCollectionBuilder.Add(System.Linq.Expressions.Expression.Parameter(typeof(object), "#" + i));
				}
			}
			if (HasUnsplatParameter)
			{
				readOnlyCollectionBuilder.Add(System.Linq.Expressions.Expression.Parameter(typeof(RubyArray), "#array"));
			}
			if (HasProcParameter)
			{
				readOnlyCollectionBuilder.Add(System.Linq.Expressions.Expression.Parameter(typeof(Proc), "#proc"));
			}
			return readOnlyCollectionBuilder;
		}

		private ScopeBuilder DefineLocals(ScopeBuilder parentBuilder)
		{
			return new ScopeBuilder(_definedScope.AllocateClosureSlotsForLocals(0), parentBuilder, _definedScope);
		}

		internal override System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			return Transform(gen, false);
		}

		internal System.Linq.Expressions.Expression Transform(AstGenerator gen, bool isLambda)
		{
			ScopeBuilder scopeBuilder = DefineLocals(gen.CurrentScope);
			ParameterExpression selfVariable;
			ParameterExpression blockParamVariable;
			ReadOnlyCollectionBuilder<ParameterExpression> parameters = DefineParameters(out selfVariable, out blockParamVariable);
			ParameterExpression parameterExpression = scopeBuilder.DefineHiddenVariable("#scope", typeof(RubyBlockScope));
			LabelTarget labelTarget = System.Linq.Expressions.Expression.Label();
			gen.EnterBlockDefinition(scopeBuilder, blockParamVariable, selfVariable, parameterExpression, labelTarget);
			System.Linq.Expressions.Expression expr = MakeParametersInitialization(gen, parameters);
			System.Linq.Expressions.Expression expr2;
			System.Linq.Expressions.Expression expression;
			if (gen.TraceEnabled)
			{
				int num = ((_body.Count > 0) ? _body.First.Location.Start.Line : base.Location.End.Line);
				int num2 = ((_body.Count > 0) ? _body.Last.Location.End.Line : base.Location.End.Line);
				expr2 = Methods.TraceBlockCall.OpCall(parameterExpression, blockParamVariable, gen.SourcePathConstant, Microsoft.Scripting.Ast.Utils.Constant(num));
				expression = Methods.TraceBlockReturn.OpCall(parameterExpression, blockParamVariable, gen.SourcePathConstant, Microsoft.Scripting.Ast.Utils.Constant(num2));
			}
			else
			{
				expr2 = (expression = System.Linq.Expressions.Expression.Empty());
			}
			ParameterExpression expression2;
			ParameterExpression arg;
			System.Linq.Expressions.Expression body = Microsoft.Scripting.Ast.Utils.Try(expr, expr2, System.Linq.Expressions.Expression.Label(labelTarget), Microsoft.Scripting.Ast.Utils.Try(gen.TransformStatements(_body, ResultOperation.Return)).Catch(expression2 = System.Linq.Expressions.Expression.Parameter(typeof(BlockUnwinder), "#u"), Microsoft.Scripting.Ast.Utils.IfThen(System.Linq.Expressions.Expression.Field(expression2, BlockUnwinder.IsRedoField), System.Linq.Expressions.Expression.Goto(labelTarget)), gen.Return(System.Linq.Expressions.Expression.Field(expression2, StackUnwinder.ReturnValueField)))).Filter(arg = System.Linq.Expressions.Expression.Parameter(typeof(Exception), "#e"), Methods.FilterBlockException.OpCall(parameterExpression, arg)).Finally(expression, System.Linq.Expressions.Expression.Empty());
			body = gen.AddReturnTarget(scopeBuilder.CreateScope(parameterExpression, Methods.CreateBlockScope.OpCall(new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>
			{
				scopeBuilder.MakeLocalsStorage(),
				scopeBuilder.GetVariableNamesExpression(),
				blockParamVariable,
				selfVariable,
				EnterInterpretedFrameExpression.Instance
			}), body));
			gen.LeaveBlockDefinition();
			int parameterCount = ParameterCount;
			BlockSignatureAttributes blockSignatureAttributes = _parameters.GetBlockSignatureAttributes();
			ConstantExpression arg2 = System.Linq.Expressions.Expression.Constant(BlockDispatcher.Create(parameterCount, blockSignatureAttributes, gen.SourcePath, base.Location.Start.Line), typeof(BlockDispatcher));
			return System.Linq.Expressions.Expression.Coalesce((isLambda ? Methods.InstantiateLambda : Methods.InstantiateBlock).OpCall(gen.CurrentScopeVariable, gen.CurrentSelfVariable, arg2), (isLambda ? Methods.DefineLambda : Methods.DefineBlock).OpCall(gen.CurrentScopeVariable, gen.CurrentSelfVariable, arg2, BlockDispatcher.CreateLambda(body, RubyStackTraceBuilder.EncodeMethodName(gen.CurrentMethod.MethodName, gen.SourcePath, base.Location, gen.DebugMode), parameters, parameterCount, blockSignatureAttributes)));
		}

		private System.Linq.Expressions.Expression GetParameterAccess(ReadOnlyCollectionBuilder<ParameterExpression> parameters, System.Linq.Expressions.Expression paramsArray, int i)
		{
			if (paramsArray == null)
			{
				return parameters[2 + i];
			}
			return System.Linq.Expressions.Expression.ArrayAccess(paramsArray, Microsoft.Scripting.Ast.Utils.Constant(i));
		}

		private System.Linq.Expressions.Expression MakeParametersInitialization(AstGenerator gen, ReadOnlyCollectionBuilder<ParameterExpression> parameters)
		{
			ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>(ParameterCount + ((_parameters.Optional.Length > 0) ? 1 : 0) + ((_parameters.Unsplat != null) ? 1 : 0) + ((_parameters.Block != null) ? 1 : 0) + 1);
			ParameterExpression paramsArray = (HasFormalParametersInArray ? parameters[2] : null);
			int num = 0;
			for (int i = 0; i < _parameters.Mandatory.Length; i++)
			{
				readOnlyCollectionBuilder.Add(_parameters.Mandatory[i].TransformWrite(gen, GetParameterAccess(parameters, paramsArray, num)));
				num++;
			}
			if (_parameters.Optional.Length > 0)
			{
				for (int j = 0; j < _parameters.Optional.Length; j++)
				{
					readOnlyCollectionBuilder.Add(_parameters.Optional[j].Left.TransformWrite(gen, GetParameterAccess(parameters, paramsArray, num)));
					num++;
				}
				readOnlyCollectionBuilder.Add(_parameters.TransformOptionalsInitialization(gen));
			}
			if (_parameters.Unsplat != null)
			{
				readOnlyCollectionBuilder.Add(_parameters.Unsplat.TransformWrite(gen, parameters[parameters.Count - ((_parameters.Block == null) ? 1 : 2)]));
			}
			if (_parameters.Block != null)
			{
				readOnlyCollectionBuilder.Add(_parameters.Block.TransformWrite(gen, parameters[parameters.Count - 1]));
				num++;
			}
			readOnlyCollectionBuilder.Add(Microsoft.Scripting.Ast.Utils.Empty());
			return System.Linq.Expressions.Expression.Block(readOnlyCollectionBuilder);
		}
	}
}
