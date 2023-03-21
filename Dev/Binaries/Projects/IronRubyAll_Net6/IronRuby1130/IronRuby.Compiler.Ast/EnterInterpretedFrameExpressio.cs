using System;
using System.Linq.Expressions;
using Microsoft.Scripting.Interpreter;

namespace IronRuby.Compiler.Ast
{
	internal sealed class EnterInterpretedFrameExpression : ReducibleEmptyExpression, IInstructionProvider
	{
		private sealed class _Instruction : Instruction
		{
			internal static readonly Instruction Instance = new _Instruction();

			public override int ProducedStack
			{
				get
				{
					return 1;
				}
			}

			public override string InstructionName
			{
				get
				{
					return "Ruby:EnterInterpretedFrame";
				}
			}

			public override int Run(InterpretedFrame frame)
			{
				frame.Push(InterpretedFrame.CurrentFrame.Value);
				return 1;
			}
		}

		internal static readonly System.Linq.Expressions.Expression Instance = new EnterInterpretedFrameExpression();

		public sealed override Type Type
		{
			get
			{
				return typeof(InterpretedFrame);
			}
		}

		public override bool CanReduce
		{
			get
			{
				return true;
			}
		}

		public void AddInstructions(LightCompiler compiler)
		{
			compiler.Instructions.Emit(_Instruction.Instance);
		}

		public override System.Linq.Expressions.Expression Reduce()
		{
			return System.Linq.Expressions.Expression.Constant(null, typeof(InterpretedFrame));
		}

		protected override System.Linq.Expressions.Expression VisitChildren(ExpressionVisitor visitor)
		{
			return this;
		}
	}
}
