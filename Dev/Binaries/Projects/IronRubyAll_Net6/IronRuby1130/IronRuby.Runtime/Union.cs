namespace IronRuby.Runtime
{
	public struct Union<TFirst, TSecond>
	{
		private TFirst _first;

		private TSecond _second;

		public TFirst First
		{
			get
			{
				return _first;
			}
		}

		public TSecond Second
		{
			get
			{
				return _second;
			}
		}

		public Union(TFirst first, TSecond second)
		{
			_first = first;
			_second = second;
		}

		public static implicit operator Union<TFirst, TSecond>(TFirst value)
		{
			return new Union<TFirst, TSecond>(value, default(TSecond));
		}

		public static implicit operator Union<TFirst, TSecond>(TSecond value)
		{
			return new Union<TFirst, TSecond>(default(TFirst), value);
		}
	}
}
