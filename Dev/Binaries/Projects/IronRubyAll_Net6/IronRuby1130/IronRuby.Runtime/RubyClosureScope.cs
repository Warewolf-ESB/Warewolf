using IronRuby.Builtins;

namespace IronRuby.Runtime
{
	public abstract class RubyClosureScope : RubyScope
	{
		private MatchData _currentMatch;

		private object _lastInputLine;

		protected override bool IsClosureScope
		{
			get
			{
				return true;
			}
		}

		public MatchData CurrentMatch
		{
			get
			{
				return _currentMatch;
			}
			set
			{
				_currentMatch = value;
			}
		}

		public object LastInputLine
		{
			get
			{
				return _lastInputLine;
			}
			set
			{
				_lastInputLine = value;
			}
		}

		internal RubyClosureScope()
		{
		}

		internal MutableString GetCurrentMatchGroup(int index)
		{
			MatchData currentMatch = _currentMatch;
			if (currentMatch == null)
			{
				return null;
			}
			return currentMatch.GetGroupValue(index);
		}

		internal MutableString GetCurrentPreMatch()
		{
			MatchData currentMatch = _currentMatch;
			if (currentMatch == null)
			{
				return null;
			}
			return currentMatch.GetPreMatch();
		}

		internal MutableString GetCurrentPostMatch()
		{
			MatchData currentMatch = _currentMatch;
			if (currentMatch == null)
			{
				return null;
			}
			return currentMatch.GetPostMatch();
		}

		internal MutableString GetCurrentMatchLastGroup()
		{
			MatchData currentMatch = _currentMatch;
			if (currentMatch != null)
			{
				for (int num = currentMatch.GroupCount - 1; num >= 0; num--)
				{
					MutableString groupValue = currentMatch.GetGroupValue(num);
					if (groupValue != null)
					{
						return groupValue;
					}
				}
			}
			return null;
		}
	}
}
