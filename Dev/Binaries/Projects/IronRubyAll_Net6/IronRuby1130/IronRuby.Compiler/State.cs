using System.Collections.Generic;

namespace IronRuby.Compiler
{
	internal sealed class State
	{
		private readonly Dictionary<int, int> _actions;

		private readonly Dictionary<int, int> _gotos;

		private readonly int _defaultAction;

		public int DefaultAction
		{
			get
			{
				return _defaultAction;
			}
		}

		public Dictionary<int, int> GotoStates
		{
			get
			{
				return _gotos;
			}
		}

		public Dictionary<int, int> Actions
		{
			get
			{
				return _actions;
			}
		}

		public State(Dictionary<int, int> actions, Dictionary<int, int> gotos, int defaultAction)
		{
			_actions = actions;
			_gotos = gotos;
			_defaultAction = defaultAction;
		}
	}
}
