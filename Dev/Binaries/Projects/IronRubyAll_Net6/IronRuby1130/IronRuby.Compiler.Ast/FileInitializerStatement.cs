using System.Linq.Expressions;
using IronRuby.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;

namespace IronRuby.Compiler.Ast
{
	public class FileInitializerStatement : Expression
	{
		private readonly LexicalScope _definedScope;

		private readonly Statements _statements;

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.Initializer;
			}
		}

		public Statements Statements
		{
			get
			{
				return _statements;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}

		public FileInitializerStatement(LexicalScope definedScope, Statements statements, SourceSpan location)
			: base(location)
		{
			_definedScope = definedScope;
			_statements = statements;
		}

		private ScopeBuilder DefineLocals()
		{
			return new ScopeBuilder(_definedScope.AllocateClosureSlotsForLocals(0), null, _definedScope);
		}

		private void TransformBody(AstGenerator gen)
		{
			ScopeBuilder scopeBuilder = DefineLocals();
			ParameterExpression parameterExpression = gen.TopLevelScope.Builder.DefineHiddenVariable("#scope", typeof(RubyScope));
			gen.EnterFileInitializer(scopeBuilder, gen.TopLevelScope.SelfVariable, parameterExpression);
			System.Linq.Expressions.Expression body = gen.TransformStatements(_statements, ResultOperation.Ignore);
			gen.LeaveFileInitializer();
			gen.AddFileInitializer(scopeBuilder.CreateScope(parameterExpression, Methods.CreateFileInitializerScope.OpCall(scopeBuilder.MakeLocalsStorage(), scopeBuilder.GetVariableNamesExpression(), gen.TopLevelScope.RuntimeScopeVariable), body));
		}

		internal override System.Linq.Expressions.Expression Transform(AstGenerator gen)
		{
			TransformBody(gen);
			return System.Linq.Expressions.Expression.Empty();
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			TransformBody(gen);
			return Microsoft.Scripting.Ast.Utils.Constant(null);
		}
	}
}
