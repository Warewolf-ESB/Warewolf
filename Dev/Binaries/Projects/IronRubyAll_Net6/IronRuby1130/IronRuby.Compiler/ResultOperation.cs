using System.Linq.Expressions;

namespace IronRuby.Compiler
{
	internal struct ResultOperation
	{
		public static readonly ResultOperation Return = new ResultOperation(null, true);

		public static readonly ResultOperation Ignore = new ResultOperation(null, false);

		private Expression _variable;

		private bool _doReturn;

		public Expression Variable
		{
			get
			{
				return _variable;
			}
		}

		public bool DoReturn
		{
			get
			{
				return _doReturn;
			}
		}

		public bool IsIgnore
		{
			get
			{
				if (_variable == null)
				{
					return !_doReturn;
				}
				return false;
			}
		}

		public ResultOperation(Expression variable, bool doReturn)
		{
			_variable = variable;
			_doReturn = doReturn;
		}

		public static ResultOperation Store(Expression variable)
		{
			return new ResultOperation(variable, false);
		}
	}
}
