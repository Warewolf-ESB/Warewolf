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
	public class MethodDefinition : DefinitionExpression
	{
		internal const int HiddenParameterCount = 2;

		private readonly Expression _target;

		private readonly string _name;

		private readonly Parameters _parameters;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.MethodDefinition;
			}
		}

		public Expression Target
		{
			get
			{
				return _target;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public Parameters Parameters
		{
			get
			{
				return _parameters;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public MethodDefinition(LexicalScope definedScope, Expression target, string name, Parameters parameters, Body body, SourceSpan location)
			: base(definedScope, body, location)
		{
			_target = target;
			_name = name;
			_parameters = parameters ?? Parameters.Empty;
		}

		private ScopeBuilder DefineLocals(out ReadOnlyCollectionBuilder<ParameterExpression> parameters)
		{
			parameters = new ReadOnlyCollectionBuilder<ParameterExpression>(2 + _parameters.Mandatory.Length + _parameters.Optional.Length + ((_parameters.Unsplat != null) ? 1 : 0));
			int closureIndex = 0;
			int num = 1;
			parameters.Add(System.Linq.Expressions.Expression.Parameter(typeof(object), "#self"));
			if (_parameters.Block != null)
			{
				parameters.Add(System.Linq.Expressions.Expression.Parameter(typeof(Proc), _parameters.Block.Name));
				_parameters.Block.SetClosureIndex(closureIndex++);
			}
			else
			{
				parameters.Add(System.Linq.Expressions.Expression.Parameter(typeof(Proc), "#block"));
				num++;
			}
			LeftValue[] mandatory = _parameters.Mandatory;
			foreach (LeftValue leftValue in mandatory)
			{
				LocalVariable localVariable = leftValue as LocalVariable;
				if (localVariable != null)
				{
					parameters.Add(System.Linq.Expressions.Expression.Parameter(typeof(object), localVariable.Name));
					localVariable.SetClosureIndex(closureIndex++);
					continue;
				}
				throw new NotSupportedException("TODO: compound parameters");
			}
			SimpleAssignmentExpression[] optional = _parameters.Optional;
			foreach (SimpleAssignmentExpression simpleAssignmentExpression in optional)
			{
				LocalVariable localVariable2 = (LocalVariable)simpleAssignmentExpression.Left;
				parameters.Add(System.Linq.Expressions.Expression.Parameter(typeof(object), localVariable2.Name));
				localVariable2.SetClosureIndex(closureIndex++);
			}
			if (_parameters.Unsplat != null)
			{
				LocalVariable localVariable3 = (LocalVariable)_parameters.Unsplat;
				parameters.Add(System.Linq.Expressions.Expression.Parameter(typeof(object), localVariable3.Name));
				localVariable3.SetClosureIndex(closureIndex++);
			}
			int localCount = base.DefinedScope.AllocateClosureSlotsForLocals(closureIndex);
			return new ScopeBuilder(parameters, num, localCount, null, base.DefinedScope);
		}

		internal LambdaExpression TransformBody(AstGenerator gen, RubyScope declaringScope, RubyModule declaringModule)
		{
			string name = RubyStackTraceBuilder.EncodeMethodName(_name, gen.SourcePath, base.Location, gen.DebugMode);
			ReadOnlyCollectionBuilder<ParameterExpression> parameters;
			ScopeBuilder scopeBuilder = DefineLocals(out parameters);
			ParameterExpression parameterExpression = scopeBuilder.DefineHiddenVariable("#scope", typeof(RubyMethodScope));
			ParameterExpression parameterExpression2 = parameters[0];
			ParameterExpression parameterExpression3 = parameters[1];
			int num = parameters.Count - 2 << 2;
			if (_parameters.Block != null)
			{
				num |= 1;
			}
			if (_parameters.Unsplat != null)
			{
				num |= 2;
			}
			gen.EnterMethodDefinition(scopeBuilder, parameterExpression2, parameterExpression, parameterExpression3, _name, _parameters);
			System.Linq.Expressions.Expression expr;
			System.Linq.Expressions.Expression expression;
			if (gen.Profiler != null)
			{
				int tickIndex = gen.Profiler.GetTickIndex(name);
				ParameterExpression parameterExpression4 = scopeBuilder.DefineHiddenVariable("#stamp", typeof(long));
				expr = System.Linq.Expressions.Expression.Assign(parameterExpression4, Methods.Stopwatch_GetTimestamp.OpCall());
				expression = Methods.UpdateProfileTicks.OpCall(Microsoft.Scripting.Ast.Utils.Constant(tickIndex), parameterExpression4);
			}
			else
			{
				expr = (expression = Microsoft.Scripting.Ast.Utils.Empty());
			}
			System.Linq.Expressions.Expression expr2;
			System.Linq.Expressions.Expression expression2;
			if (gen.TraceEnabled)
			{
				expr2 = Methods.TraceMethodCall.OpCall(parameterExpression, gen.SourcePathConstant, Microsoft.Scripting.Ast.Utils.Constant(base.Location.Start.Line));
				expression2 = Methods.TraceMethodReturn.OpCall(gen.CurrentScopeVariable, gen.SourcePathConstant, Microsoft.Scripting.Ast.Utils.Constant(base.Location.End.Line));
			}
			else
			{
				expr2 = (expression2 = Microsoft.Scripting.Ast.Utils.Empty());
			}
			ParameterExpression parameterExpression5;
			System.Linq.Expressions.Expression body = Microsoft.Scripting.Ast.Utils.Try(expr, _parameters.TransformOptionalsInitialization(gen), expr2, base.Body.TransformResult(gen, ResultOperation.Return)).Filter(parameterExpression5 = System.Linq.Expressions.Expression.Parameter(typeof(Exception), "#u"), Methods.IsMethodUnwinderTargetFrame.OpCall(parameterExpression, parameterExpression5), System.Linq.Expressions.Expression.Return(gen.ReturnLabel, Methods.GetMethodUnwinderReturnValue.OpCall(parameterExpression5))).Finally(Methods.LeaveMethodFrame.OpCall(parameterExpression), System.Linq.Expressions.Expression.Empty(), expression, expression2);
			body = gen.AddReturnTarget(scopeBuilder.CreateScope(parameterExpression, Methods.CreateMethodScope.OpCall(new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>
			{
				scopeBuilder.MakeLocalsStorage(),
				scopeBuilder.GetVariableNamesExpression(),
				System.Linq.Expressions.Expression.Constant(num),
				System.Linq.Expressions.Expression.Constant(declaringScope, typeof(RubyScope)),
				System.Linq.Expressions.Expression.Constant(declaringModule, typeof(RubyModule)),
				System.Linq.Expressions.Expression.Constant(_name),
				parameterExpression2,
				parameterExpression3,
				EnterInterpretedFrameExpression.Instance
			}), body));
			gen.LeaveMethodDefinition();
			return CreateLambda(name, parameters, body);
		}

		private static LambdaExpression CreateLambda(string name, ReadOnlyCollectionBuilder<ParameterExpression> parameters, System.Linq.Expressions.Expression body)
		{
			LambdaExpression lambdaExpression = MemberDispatcher.CreateRubyMethodLambda(body, name, parameters);
			if (lambdaExpression != null)
			{
				return lambdaExpression;
			}
			ParameterExpression parameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(object[]), "#params");
			ReadOnlyCollectionBuilder<ParameterExpression> readOnlyCollectionBuilder = new ReadOnlyCollectionBuilder<ParameterExpression>();
			readOnlyCollectionBuilder.Add(parameters[0]);
			readOnlyCollectionBuilder.Add(parameters[1]);
			readOnlyCollectionBuilder.Add(parameterExpression);
			ReadOnlyCollectionBuilder<ParameterExpression> parameters2 = readOnlyCollectionBuilder;
			parameters.RemoveAt(0);
			parameters.RemoveAt(1);
			ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression> readOnlyCollectionBuilder2 = new ReadOnlyCollectionBuilder<System.Linq.Expressions.Expression>(parameters.Count + 1);
			for (int i = 0; i < parameters.Count; i++)
			{
				readOnlyCollectionBuilder2[i] = System.Linq.Expressions.Expression.Assign(parameters[i], System.Linq.Expressions.Expression.ArrayIndex(parameterExpression, Microsoft.Scripting.Ast.Utils.Constant(i)));
			}
			readOnlyCollectionBuilder2[parameters.Count] = body;
			return System.Linq.Expressions.Expression.Lambda<Func<object, Proc, object[], object>>(System.Linq.Expressions.Expression.Block(parameters, readOnlyCollectionBuilder2), name, parameters2);
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			return Methods.DefineMethod.OpCall((_target != null) ? Microsoft.Scripting.Ast.Utils.Box(_target.TransformRead(gen)) : Microsoft.Scripting.Ast.Utils.Constant(null), gen.CurrentScopeVariable, System.Linq.Expressions.Expression.Constant(new RubyMethodBody(this, gen.Document, gen.Encoding)));
		}
	}
}
