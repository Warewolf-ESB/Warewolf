using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast
{
	public abstract class AssignmentExpression : Expression
	{
		private string _operation;

		public string Operation
		{
			get
			{
				return _operation;
			}
			internal set
			{
				_operation = value;
			}
		}

		public AssignmentExpression(string operation, SourceSpan location)
			: base(location)
		{
			_operation = operation;
		}

		internal override string GetNodeName(AstGenerator gen)
		{
			return "assignment";
		}
	}
}
