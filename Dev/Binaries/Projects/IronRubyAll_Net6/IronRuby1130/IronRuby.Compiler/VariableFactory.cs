using IronRuby.Compiler.Ast;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler
{
	internal static class VariableFactory
	{
		public const int Identifier = 0;

		public const int Instance = 1;

		public const int Global = 2;

		public const int Constant = 3;

		public const int Class = 4;

		public const int Nil = 5;

		public const int Self = 6;

		public const int True = 7;

		public const int False = 8;

		public const int File = 9;

		public const int Line = 10;

		public const int Encoding = 11;

		internal static Expression MakeRead(int kind, Parser parser, string name, SourceSpan location)
		{
			switch (kind)
			{
			case 0:
				return (Expression)(((object)parser.CurrentScope.ResolveVariable(name)) ?? ((object)new MethodCall(null, name, null, null, location)));
			case 1:
				return new InstanceVariable(name, location);
			case 2:
				return new GlobalVariable(name, location);
			case 3:
				return new ConstantVariable(name, location);
			case 4:
				return new ClassVariable(name, location);
			case 5:
				return Literal.Nil(location);
			case 6:
				return new SelfReference(location);
			case 7:
				return Literal.True(location);
			case 8:
				return Literal.False(location);
			case 9:
				return new FileLiteral(location);
			case 10:
				return Literal.Integer(parser.Tokenizer.TokenSpan.Start.Line, location);
			case 11:
				return new EncodingExpression(location);
			default:
				throw Assert.Unreachable;
			}
		}

		internal static LeftValue MakeLeftValue(int kind, Parser parser, string name, SourceSpan location)
		{
			switch (kind)
			{
			case 0:
				return parser.CurrentScope.ResolveOrAddVariable(name, location);
			case 1:
				return new InstanceVariable(name, location);
			case 2:
				return new GlobalVariable(name, location);
			case 3:
				return new ConstantVariable(name, location);
			case 4:
				return new ClassVariable(name, location);
			case 5:
				return parser.CannotAssignError("nil", location);
			case 6:
				return parser.CannotAssignError("self", location);
			case 7:
				return parser.CannotAssignError("true", location);
			case 8:
				return parser.CannotAssignError("false", location);
			case 9:
				return parser.CannotAssignError("__FILE__", location);
			case 10:
				return parser.CannotAssignError("__LINE__", location);
			case 11:
				return parser.CannotAssignError("__ENCODING__", location);
			default:
				return null;
			}
		}
	}
}
