using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Utils;

namespace IronRuby.Compiler.Ast
{
	public class RegexMatchReference : Expression
	{
		internal const int EntireMatch = 0;

		internal const string EntireMatchName = "&";

		internal const int MatchData = -1;

		internal const string MatchDataName = "~";

		internal const int MatchLastGroup = -2;

		internal const string MatchLastGroupName = "+";

		internal const int PreMatch = -3;

		internal const string PreMatchName = "`";

		internal const int PostMatch = -4;

		internal const string PostMatchName = "'";

		private readonly int _index;

		public int Index
		{
			get
			{
				if (_index <= 0)
				{
					return 0;
				}
				return _index;
			}
		}

		public string VariableName
		{
			get
			{
				switch (_index)
				{
				case 0:
					return "&";
				case -1:
					return "~";
				case -2:
					return "+";
				case -3:
					return "`";
				case -4:
					return "'";
				default:
					return _index.ToString();
				}
			}
		}

		public string FullName
		{
			get
			{
				return "$" + VariableName;
			}
		}

		internal bool CanAlias
		{
			get
			{
				return _index <= 0;
			}
		}

		public override NodeTypes NodeType
		{
			get
			{
				return NodeTypes.RegexMatchReference;
			}
		}

		internal RegexMatchReference(int index, SourceSpan location)
			: base(location)
		{
			_index = index;
		}

		public RegexMatchReference CreateGroupReference(int index, SourceSpan location)
		{
			ContractUtils.Requires(index >= 0);
			return new RegexMatchReference(index, location);
		}

		public RegexMatchReference CreateLastGroupReference(SourceSpan location)
		{
			return new RegexMatchReference(-2, location);
		}

		public RegexMatchReference CreatePrefixReference(SourceSpan location)
		{
			return new RegexMatchReference(-3, location);
		}

		public RegexMatchReference CreateSuffixReference(SourceSpan location)
		{
			return new RegexMatchReference(-4, location);
		}

		public RegexMatchReference CreateMatchReference(SourceSpan location)
		{
			return new RegexMatchReference(-1, location);
		}

		internal override System.Linq.Expressions.Expression TransformRead(AstGenerator gen)
		{
			switch (_index)
			{
			case -1:
				return Methods.GetCurrentMatchData.OpCall(gen.CurrentScopeVariable);
			case -2:
				return Methods.GetCurrentMatchLastGroup.OpCall(gen.CurrentScopeVariable);
			case -3:
				return Methods.GetCurrentPreMatch.OpCall(gen.CurrentScopeVariable);
			case -4:
				return Methods.GetCurrentPostMatch.OpCall(gen.CurrentScopeVariable);
			default:
				return Methods.GetCurrentMatchGroup.OpCall(gen.CurrentScopeVariable, Utils.Constant(_index));
			}
		}

		internal override System.Linq.Expressions.Expression TransformDefinedCondition(AstGenerator gen)
		{
			return System.Linq.Expressions.Expression.NotEqual(TransformRead(gen), Utils.Constant(null));
		}

		internal override string GetNodeName(AstGenerator gen)
		{
			switch (_index)
			{
			case -1:
				return "$~";
			case -2:
				return "$+";
			case -3:
				return "$`";
			case -4:
				return "$'";
			default:
				return "$" + _index;
			}
		}

		protected internal override void Walk(Walker walker)
		{
			walker.Walk(this);
		}
	}
}
