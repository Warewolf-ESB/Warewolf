using Microsoft.Scripting;

namespace IronRuby.Compiler.Ast
{
	public abstract class Node
	{
		private readonly SourceSpan _location;

		public abstract NodeTypes NodeType { get; }

		public SourceSpan Location
		{
			get
			{
				return _location;
			}
		}

		protected internal abstract void Walk(Walker walker);

		protected Node(SourceSpan location)
		{
			_location = location;
		}
	}
}
