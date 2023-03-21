using System;
using System.Collections;
using System.Collections.Generic;

namespace IronRuby.Compiler.Ast
{
	public sealed class Statements : IEnumerable<Expression>, IEnumerable
	{
		private static readonly Statements _Empty = new Statements();

		private Expression[] _statements;

		private int _count;

		internal static Statements Empty
		{
			get
			{
				return _Empty;
			}
		}

		public int Count
		{
			get
			{
				return _count;
			}
		}

		public Expression First
		{
			get
			{
				return _statements[0];
			}
		}

		public Expression Last
		{
			get
			{
				return _statements[_count - 1];
			}
		}

		public IEnumerable<Expression> AllButLast
		{
			get
			{
				for (int i = 0; i < _count - 1; i++)
				{
					yield return _statements[i];
				}
			}
		}

		public Statements()
		{
		}

		public Statements(Expression statement)
		{
			AddFirst(statement);
		}

		private void AddFirst(Expression statement)
		{
			_statements = new Expression[1] { statement };
			_count = 1;
		}

		public Expression Add(Expression statement)
		{
			if (_count == 0)
			{
				AddFirst(statement);
			}
			else
			{
				if (_count == _statements.Length)
				{
					Array.Resize(ref _statements, 2 * _count);
				}
				_statements[_count] = statement;
				_count++;
			}
			return statement;
		}

		public IEnumerator<Expression> GetEnumerator()
		{
			for (int i = 0; i < _count; i++)
			{
				yield return _statements[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
