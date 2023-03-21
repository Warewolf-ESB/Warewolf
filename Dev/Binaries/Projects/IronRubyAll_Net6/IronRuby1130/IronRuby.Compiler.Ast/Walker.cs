using System.Collections.Generic;

namespace IronRuby.Compiler.Ast
{
	public class Walker
	{
		public virtual bool Enter(SourceUnitTree node)
		{
			return true;
		}

		public virtual void Exit(SourceUnitTree node)
		{
		}

		public virtual bool Enter(BlockDefinition node)
		{
			return true;
		}

		public virtual void Exit(BlockDefinition node)
		{
		}

		public virtual bool Enter(BlockReference node)
		{
			return true;
		}

		public virtual void Exit(BlockReference node)
		{
		}

		public virtual bool Enter(Body node)
		{
			return true;
		}

		public virtual void Exit(Body node)
		{
		}

		public virtual bool Enter(Maplet node)
		{
			return true;
		}

		public virtual void Exit(Maplet node)
		{
		}

		public virtual bool Enter(Parameters node)
		{
			return true;
		}

		public virtual void Exit(Parameters node)
		{
		}

		public virtual bool Enter(Arguments node)
		{
			return true;
		}

		public virtual void Exit(Arguments node)
		{
		}

		public virtual bool Enter(SplattedArgument node)
		{
			return true;
		}

		public virtual void Exit(SplattedArgument node)
		{
		}

		public virtual bool Enter(ClassDefinition node)
		{
			return true;
		}

		public virtual void Exit(ClassDefinition node)
		{
		}

		public virtual bool Enter(ModuleDefinition node)
		{
			return true;
		}

		public virtual void Exit(ModuleDefinition node)
		{
		}

		public virtual bool Enter(SingletonDefinition node)
		{
			return true;
		}

		public virtual void Exit(SingletonDefinition node)
		{
		}

		public virtual bool Enter(MethodDefinition node)
		{
			return true;
		}

		public virtual void Exit(MethodDefinition node)
		{
		}

		public virtual bool Enter(LambdaDefinition node)
		{
			return true;
		}

		public virtual void Exit(LambdaDefinition node)
		{
		}

		public virtual bool Enter(AndExpression node)
		{
			return true;
		}

		public virtual void Exit(AndExpression node)
		{
		}

		public virtual bool Enter(ArrayConstructor node)
		{
			return true;
		}

		public virtual void Exit(ArrayConstructor node)
		{
		}

		public virtual bool Enter(AssignmentExpression node)
		{
			return true;
		}

		public virtual void Exit(AssignmentExpression node)
		{
		}

		public virtual bool Enter(IsDefinedExpression node)
		{
			return true;
		}

		public virtual void Exit(IsDefinedExpression node)
		{
		}

		public virtual bool Enter(BlockExpression node)
		{
			return true;
		}

		public virtual void Exit(BlockExpression node)
		{
		}

		public virtual bool Enter(CaseExpression node)
		{
			return true;
		}

		public virtual void Exit(CaseExpression node)
		{
		}

		public virtual bool Enter(ConditionalExpression node)
		{
			return true;
		}

		public virtual void Exit(ConditionalExpression node)
		{
		}

		public virtual bool Enter(ConditionalJumpExpression node)
		{
			return true;
		}

		public virtual void Exit(ConditionalJumpExpression node)
		{
		}

		public virtual bool Enter(ErrorExpression node)
		{
			return true;
		}

		public virtual void Exit(ErrorExpression node)
		{
		}

		public virtual bool Enter(ForLoopExpression node)
		{
			return true;
		}

		public virtual void Exit(ForLoopExpression node)
		{
		}

		public virtual bool Enter(HashConstructor node)
		{
			return true;
		}

		public virtual void Exit(HashConstructor node)
		{
		}

		public virtual bool Enter(IfExpression node)
		{
			return true;
		}

		public virtual void Exit(IfExpression node)
		{
		}

		public virtual bool Enter(Literal node)
		{
			return true;
		}

		public virtual void Exit(Literal node)
		{
		}

		public virtual bool Enter(StringLiteral node)
		{
			return true;
		}

		public virtual void Exit(StringLiteral node)
		{
		}

		public virtual bool Enter(SymbolLiteral node)
		{
			return true;
		}

		public virtual void Exit(SymbolLiteral node)
		{
		}

		public virtual bool Enter(FileLiteral node)
		{
			return true;
		}

		public virtual void Exit(FileLiteral node)
		{
		}

		public virtual bool Enter(EncodingExpression node)
		{
			return true;
		}

		public virtual void Exit(EncodingExpression node)
		{
		}

		public virtual bool Enter(MethodCall node)
		{
			return true;
		}

		public virtual void Exit(MethodCall node)
		{
		}

		public virtual bool Enter(MatchExpression node)
		{
			return true;
		}

		public virtual void Exit(MatchExpression node)
		{
		}

		public virtual bool Enter(NotExpression node)
		{
			return true;
		}

		public virtual void Exit(NotExpression node)
		{
		}

		public virtual bool Enter(OrExpression node)
		{
			return true;
		}

		public virtual void Exit(OrExpression node)
		{
		}

		public virtual bool Enter(RangeExpression node)
		{
			return true;
		}

		public virtual void Exit(RangeExpression node)
		{
		}

		public virtual bool Enter(RangeCondition node)
		{
			return true;
		}

		public virtual void Exit(RangeCondition node)
		{
		}

		public virtual bool Enter(RegularExpression node)
		{
			return true;
		}

		public virtual void Exit(RegularExpression node)
		{
		}

		public virtual bool Enter(RegularExpressionCondition node)
		{
			return true;
		}

		public virtual void Exit(RegularExpressionCondition node)
		{
		}

		public virtual bool Enter(RescueExpression node)
		{
			return true;
		}

		public virtual void Exit(RescueExpression node)
		{
		}

		public virtual bool Enter(SelfReference node)
		{
			return true;
		}

		public virtual void Exit(SelfReference node)
		{
		}

		public virtual bool Enter(StringConstructor node)
		{
			return true;
		}

		public virtual void Exit(StringConstructor node)
		{
		}

		public virtual bool Enter(SuperCall node)
		{
			return true;
		}

		public virtual void Exit(SuperCall node)
		{
		}

		public virtual bool Enter(UnlessExpression node)
		{
			return true;
		}

		public virtual void Exit(UnlessExpression node)
		{
		}

		public virtual bool Enter(WhileLoopExpression node)
		{
			return true;
		}

		public virtual void Exit(WhileLoopExpression node)
		{
		}

		public virtual bool Enter(YieldCall node)
		{
			return true;
		}

		public virtual void Exit(YieldCall node)
		{
		}

		public virtual bool Enter(ArrayItemAccess node)
		{
			return true;
		}

		public virtual void Exit(ArrayItemAccess node)
		{
		}

		public virtual bool Enter(AttributeAccess node)
		{
			return true;
		}

		public virtual void Exit(AttributeAccess node)
		{
		}

		public virtual bool Enter(ClassVariable node)
		{
			return true;
		}

		public virtual void Exit(ClassVariable node)
		{
		}

		public virtual bool Enter(CompoundLeftValue node)
		{
			return true;
		}

		public virtual void Exit(CompoundLeftValue node)
		{
		}

		public virtual bool Enter(ConstantVariable node)
		{
			return true;
		}

		public virtual void Exit(ConstantVariable node)
		{
		}

		public virtual bool Enter(GlobalVariable node)
		{
			return true;
		}

		public virtual void Exit(GlobalVariable node)
		{
		}

		public virtual bool Enter(InstanceVariable node)
		{
			return true;
		}

		public virtual void Exit(InstanceVariable node)
		{
		}

		public virtual bool Enter(LocalVariable node)
		{
			return true;
		}

		public virtual void Exit(LocalVariable node)
		{
		}

		public virtual bool Enter(Placeholder node)
		{
			return true;
		}

		public virtual void Exit(Placeholder node)
		{
		}

		public virtual bool Enter(RegexMatchReference node)
		{
			return true;
		}

		public virtual void Exit(RegexMatchReference node)
		{
		}

		public virtual bool Enter(MemberAssignmentExpression node)
		{
			return true;
		}

		public virtual void Exit(MemberAssignmentExpression node)
		{
		}

		public virtual bool Enter(ParallelAssignmentExpression node)
		{
			return true;
		}

		public virtual void Exit(ParallelAssignmentExpression node)
		{
		}

		public virtual bool Enter(SimpleAssignmentExpression node)
		{
			return true;
		}

		public virtual void Exit(SimpleAssignmentExpression node)
		{
		}

		public virtual bool Enter(AliasStatement node)
		{
			return true;
		}

		public virtual void Exit(AliasStatement node)
		{
		}

		public virtual bool Enter(ConditionalStatement node)
		{
			return true;
		}

		public virtual void Exit(ConditionalStatement node)
		{
		}

		public virtual bool Enter(ShutdownHandlerStatement node)
		{
			return true;
		}

		public virtual void Exit(ShutdownHandlerStatement node)
		{
		}

		public virtual bool Enter(FileInitializerStatement node)
		{
			return true;
		}

		public virtual void Exit(FileInitializerStatement node)
		{
		}

		public virtual bool Enter(UndefineStatement node)
		{
			return true;
		}

		public virtual void Exit(UndefineStatement node)
		{
		}

		public virtual bool Enter(BreakStatement node)
		{
			return true;
		}

		public virtual void Exit(BreakStatement node)
		{
		}

		public virtual bool Enter(NextStatement node)
		{
			return true;
		}

		public virtual void Exit(NextStatement node)
		{
		}

		public virtual bool Enter(RedoStatement node)
		{
			return true;
		}

		public virtual void Exit(RedoStatement node)
		{
		}

		public virtual bool Enter(RetryStatement node)
		{
			return true;
		}

		public virtual void Exit(RetryStatement node)
		{
		}

		public virtual bool Enter(ReturnStatement node)
		{
			return true;
		}

		public virtual void Exit(ReturnStatement node)
		{
		}

		public virtual bool Enter(RescueClause node)
		{
			return true;
		}

		public virtual void Exit(RescueClause node)
		{
		}

		public virtual bool Enter(WhenClause node)
		{
			return true;
		}

		public virtual void Exit(WhenClause node)
		{
		}

		public virtual bool Enter(ElseIfClause node)
		{
			return true;
		}

		public virtual void Exit(ElseIfClause node)
		{
		}

		public void VisitOptionalList<T>(IEnumerable<T> list) where T : Node
		{
			if (list == null)
			{
				return;
			}
			foreach (T item in list)
			{
				T current = item;
				current.Walk(this);
			}
		}

		public void VisitList<T>(IEnumerable<T> list) where T : Node
		{
			foreach (T item in list)
			{
				T current = item;
				current.Walk(this);
			}
		}

		public void Walk(Node node)
		{
			node.Walk(this);
		}

		protected internal virtual void Walk(Arguments node)
		{
			if (Enter(node))
			{
				VisitList(node.Expressions);
			}
			Exit(node);
		}

		protected internal virtual void Walk(SplattedArgument node)
		{
			if (Enter(node))
			{
				node.Argument.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(BlockReference node)
		{
			if (Enter(node))
			{
				node.Expression.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(BlockDefinition node)
		{
			if (Enter(node))
			{
				node.Parameters.Walk(this);
				VisitList(node.Body);
			}
			Exit(node);
		}

		protected internal virtual void Walk(Body node)
		{
			if (Enter(node))
			{
				VisitList(node.Statements);
				VisitOptionalList(node.RescueClauses);
				VisitOptionalList(node.ElseStatements);
				VisitOptionalList(node.EnsureStatements);
			}
			Exit(node);
		}

		protected internal virtual void Walk(Maplet node)
		{
			if (Enter(node))
			{
				node.Key.Walk(this);
				node.Value.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(Parameters node)
		{
			if (Enter(node))
			{
				VisitList(node.Mandatory);
				VisitList(node.Optional);
				if (node.Unsplat != null)
				{
					node.Unsplat.Walk(this);
				}
				if (node.Block != null)
				{
					node.Block.Walk(this);
				}
			}
			Exit(node);
		}

		protected internal virtual void Walk(RegexMatchReference node)
		{
			Enter(node);
			Exit(node);
		}

		protected internal virtual void Walk(SourceUnitTree node)
		{
			if (Enter(node))
			{
				VisitOptionalList(node.Initializers);
				VisitOptionalList(node.Statements);
			}
			Exit(node);
		}

		protected internal virtual void Walk(ParallelAssignmentExpression node)
		{
			if (Enter(node))
			{
				node.Left.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(MemberAssignmentExpression node)
		{
			if (Enter(node))
			{
				node.LeftTarget.Walk(this);
				node.Right.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(SimpleAssignmentExpression node)
		{
			if (Enter(node))
			{
				node.Left.Walk(this);
				node.Right.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(ElseIfClause node)
		{
			if (Enter(node))
			{
				if (node.Condition != null)
				{
					node.Condition.Walk(this);
				}
				VisitOptionalList(node.Statements);
			}
			Exit(node);
		}

		protected internal virtual void Walk(WhenClause node)
		{
			if (Enter(node))
			{
				VisitList(node.Comparisons);
				VisitOptionalList(node.Statements);
			}
			Exit(node);
		}

		protected internal virtual void Walk(RescueClause node)
		{
			if (Enter(node))
			{
				if (node.Types != null)
				{
					VisitOptionalList(node.Types);
				}
				if (node.Target != null)
				{
					node.Target.Walk(this);
				}
				if (node.Statements != null)
				{
					VisitOptionalList(node.Statements);
				}
			}
			Exit(node);
		}

		protected internal virtual void Walk(ClassDefinition node)
		{
			if (Enter(node))
			{
				if (node.QualifiedName != null)
				{
					node.QualifiedName.Walk(this);
				}
				if (node.SuperClass != null)
				{
					node.SuperClass.Walk(this);
				}
				node.Body.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(ModuleDefinition node)
		{
			if (Enter(node))
			{
				if (node.QualifiedName != null)
				{
					node.QualifiedName.Walk(this);
				}
				node.Body.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(SingletonDefinition node)
		{
			if (Enter(node))
			{
				if (node.QualifiedName != null)
				{
					node.QualifiedName.Walk(this);
				}
				node.Singleton.Walk(this);
				node.Body.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(MethodDefinition node)
		{
			if (Enter(node))
			{
				if (node.Target != null)
				{
					node.Target.Walk(this);
				}
				node.Parameters.Walk(this);
				node.Body.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(LambdaDefinition node)
		{
			if (Enter(node))
			{
				node.Block.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(AndExpression node)
		{
			if (Enter(node))
			{
				node.Left.Walk(this);
				node.Right.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(IsDefinedExpression node)
		{
			if (Enter(node))
			{
				node.Expression.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(BlockExpression node)
		{
			if (Enter(node))
			{
				VisitList(node.Statements);
			}
			Exit(node);
		}

		protected internal virtual void Walk(ArrayConstructor node)
		{
			if (Enter(node))
			{
				VisitList(node.Arguments.Expressions);
			}
			Exit(node);
		}

		protected internal virtual void Walk(CaseExpression node)
		{
			if (Enter(node))
			{
				if (node.Value != null)
				{
					node.Value.Walk(this);
				}
				VisitOptionalList(node.WhenClauses);
				VisitOptionalList(node.ElseStatements);
			}
			Exit(node);
		}

		protected internal virtual void Walk(ConditionalExpression node)
		{
			if (Enter(node))
			{
				node.Condition.Walk(this);
				node.TrueExpression.Walk(this);
				node.FalseExpression.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(ConditionalJumpExpression node)
		{
			if (Enter(node))
			{
				node.Condition.Walk(this);
				node.JumpStatement.Walk(this);
				if (node.Value != null)
				{
					node.Value.Walk(this);
				}
			}
			Exit(node);
		}

		protected internal virtual void Walk(ErrorExpression node)
		{
			Enter(node);
			Exit(node);
		}

		protected internal virtual void Walk(ForLoopExpression node)
		{
			if (Enter(node))
			{
				if (node.Block != null)
				{
					node.Block.Walk(this);
				}
				if (node.List != null)
				{
					node.List.Walk(this);
				}
			}
			Exit(node);
		}

		protected internal virtual void Walk(HashConstructor node)
		{
			if (Enter(node))
			{
				VisitList(node.Maplets);
			}
			Exit(node);
		}

		protected internal virtual void Walk(IfExpression node)
		{
			if (Enter(node))
			{
				node.Condition.Walk(this);
				VisitOptionalList(node.Body);
				VisitOptionalList(node.ElseIfClauses);
			}
			Exit(node);
		}

		protected internal virtual void Walk(Literal node)
		{
			Enter(node);
			Exit(node);
		}

		protected internal virtual void Walk(StringLiteral node)
		{
			Enter(node);
			Exit(node);
		}

		protected internal virtual void Walk(SymbolLiteral node)
		{
			Enter(node);
			Exit(node);
		}

		protected internal virtual void Walk(FileLiteral node)
		{
			Enter(node);
			Exit(node);
		}

		protected internal virtual void Walk(EncodingExpression node)
		{
			Enter(node);
			Exit(node);
		}

		protected internal virtual void Walk(MethodCall node)
		{
			if (Enter(node))
			{
				if (node.Target != null)
				{
					node.Target.Walk(this);
				}
				if (node.Arguments != null)
				{
					VisitList(node.Arguments.Expressions);
				}
			}
			Exit(node);
		}

		protected internal virtual void Walk(MatchExpression node)
		{
			if (Enter(node))
			{
				node.Regex.Walk(this);
				node.Expression.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(NotExpression node)
		{
			if (Enter(node))
			{
				node.Expression.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(OrExpression node)
		{
			if (Enter(node))
			{
				node.Left.Walk(this);
				node.Right.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(RangeExpression node)
		{
			if (Enter(node))
			{
				node.Begin.Walk(this);
				node.End.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(RangeCondition node)
		{
			if (Enter(node))
			{
				node.Range.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(RegularExpression node)
		{
			if (Enter(node))
			{
				VisitList(node.Pattern);
			}
			Exit(node);
		}

		protected internal virtual void Walk(RegularExpressionCondition node)
		{
			if (Enter(node))
			{
				node.RegularExpression.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(RescueExpression node)
		{
			if (Enter(node))
			{
				node.GuardedExpression.Walk(this);
				node.RescueClauseStatement.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(StringConstructor node)
		{
			if (Enter(node))
			{
				VisitList(node.Parts);
			}
			Exit(node);
		}

		protected internal virtual void Walk(SelfReference node)
		{
			Enter(node);
			Exit(node);
		}

		protected internal virtual void Walk(UnlessExpression node)
		{
			if (Enter(node))
			{
				node.Condition.Walk(this);
				VisitOptionalList(node.Statements);
				if (node.ElseClause != null)
				{
					node.ElseClause.Walk(this);
				}
			}
			Exit(node);
		}

		protected internal virtual void Walk(SuperCall node)
		{
			if (Enter(node) && node.Arguments != null)
			{
				VisitList(node.Arguments.Expressions);
			}
			Exit(node);
		}

		protected internal virtual void Walk(WhileLoopExpression node)
		{
			if (Enter(node))
			{
				if (node.Condition != null)
				{
					node.Condition.Walk(this);
				}
				VisitOptionalList(node.Statements);
			}
			Exit(node);
		}

		protected internal virtual void Walk(YieldCall node)
		{
			if (Enter(node) && node.Arguments != null)
			{
				VisitList(node.Arguments.Expressions);
			}
			Exit(node);
		}

		protected internal virtual void Walk(BreakStatement node)
		{
			if (Enter(node) && node.Arguments != null)
			{
				VisitList(node.Arguments.Expressions);
			}
			Exit(node);
		}

		protected internal virtual void Walk(NextStatement node)
		{
			if (Enter(node) && node.Arguments != null)
			{
				VisitList(node.Arguments.Expressions);
			}
			Exit(node);
		}

		protected internal virtual void Walk(RedoStatement node)
		{
			if (Enter(node) && node.Arguments != null)
			{
				VisitList(node.Arguments.Expressions);
			}
			Exit(node);
		}

		protected internal virtual void Walk(RetryStatement node)
		{
			if (Enter(node) && node.Arguments != null)
			{
				VisitList(node.Arguments.Expressions);
			}
			Exit(node);
		}

		protected internal virtual void Walk(ReturnStatement node)
		{
			if (Enter(node) && node.Arguments != null)
			{
				VisitList(node.Arguments.Expressions);
			}
			Exit(node);
		}

		protected internal virtual void Walk(ArrayItemAccess node)
		{
			if (Enter(node))
			{
				node.Array.Walk(this);
				VisitList(node.Arguments.Expressions);
				if (node.Block != null)
				{
					node.Block.Walk(this);
				}
			}
			Exit(node);
		}

		protected internal virtual void Walk(AttributeAccess node)
		{
			if (Enter(node))
			{
				node.Qualifier.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(ConstantVariable node)
		{
			if (Enter(node) && node.Qualifier != null)
			{
				node.Qualifier.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(ClassVariable node)
		{
			Enter(node);
			Exit(node);
		}

		protected internal virtual void Walk(CompoundLeftValue node)
		{
			if (Enter(node))
			{
				VisitList(node.LeftValues);
			}
			Exit(node);
		}

		protected internal virtual void Walk(GlobalVariable node)
		{
			Enter(node);
			Exit(node);
		}

		protected internal virtual void Walk(InstanceVariable node)
		{
			Enter(node);
			Exit(node);
		}

		protected internal virtual void Walk(LocalVariable node)
		{
			Enter(node);
			Exit(node);
		}

		protected internal virtual void Walk(Placeholder node)
		{
			Enter(node);
			Exit(node);
		}

		protected internal virtual void Walk(ConditionalStatement node)
		{
			if (Enter(node))
			{
				node.Condition.Walk(this);
				node.Body.Walk(this);
				if (node.ElseStatement != null)
				{
					node.ElseStatement.Walk(this);
				}
			}
			Exit(node);
		}

		protected internal virtual void Walk(AliasStatement node)
		{
			Enter(node);
			Exit(node);
		}

		protected internal virtual void Walk(FileInitializerStatement node)
		{
			if (Enter(node))
			{
				VisitOptionalList(node.Statements);
			}
			Exit(node);
		}

		protected internal virtual void Walk(ShutdownHandlerStatement node)
		{
			if (Enter(node))
			{
				node.Block.Walk(this);
			}
			Exit(node);
		}

		protected internal virtual void Walk(UndefineStatement node)
		{
			Enter(node);
			Exit(node);
		}
	}
}
