using System;
using System.Collections.Generic;

namespace IronRuby.Compiler
{
	public class ParserStack<TState, TValue, TLocation>
	{
		private const int InitialSize = 50;

		private TState[] _states = new TState[50];

		private TLocation[] _locations = new TLocation[50];

		private TValue[] _values = new TValue[50];

		private int _top;

		public bool IsEmpty
		{
			get
			{
				return _top == 0;
			}
		}

		public void Push(TState state, TValue value, TLocation location)
		{
			int top = _top;
			if (top == _states.Length)
			{
				TState[] array = new TState[top * 2];
				TValue[] array2 = new TValue[top * 2];
				TLocation[] array3 = new TLocation[top * 2];
				Array.Copy(_states, array, top);
				Array.Copy(_values, array2, top);
				Array.Copy(_locations, array3, top);
				_states = array;
				_values = array2;
				_locations = array3;
			}
			_states[top] = state;
			_values[top] = value;
			_locations[top] = location;
			_top = top + 1;
		}

		public void Pop()
		{
			_top--;
		}

		public void Pop(int depth)
		{
			_top -= depth;
		}

		public TState PeekState(int depth)
		{
			return _states[_top - depth];
		}

		public TLocation PeekLocation(int depth)
		{
			return _locations[_top - depth];
		}

		public TValue PeekValue(int depth)
		{
			return _values[_top - depth];
		}

		public IEnumerable<TState> GetStates()
		{
			return _states;
		}
	}
}
