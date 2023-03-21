using System.Collections.Generic;
using System.Threading;
using IronRuby.Runtime;

namespace IronRuby.Compiler.Generation
{
	public sealed class Profiler
	{
		public struct MethodCounter
		{
			public readonly string Name;

			public readonly string File;

			public readonly int Line;

			public readonly long Ticks;

			public string Id
			{
				get
				{
					return string.Format("{0};{1};{2}", Name, File, Line);
				}
			}

			public MethodCounter(string name, string file, int line, long ticks)
			{
				Name = name;
				File = file;
				Line = line;
				Ticks = ticks;
			}
		}

		public static readonly Profiler Instance = new Profiler();

		internal static long[] _ProfileTicks = new long[100];

		private readonly Dictionary<string, int> _counters;

		private readonly List<long[]> _profiles;

		private static int _Index;

		private Profiler()
		{
			_counters = new Dictionary<string, int>();
			_profiles = new List<long[]>();
		}

		public int GetTickIndex(string name)
		{
			lock (_counters)
			{
				int value;
				if (!_counters.TryGetValue(name, out value))
				{
					value = _Index++;
					_counters.Add(name, value);
				}
				if (value >= _ProfileTicks.Length)
				{
					long[] value2 = new long[value * 2];
					_profiles.Add(Interlocked.Exchange(ref _ProfileTicks, value2));
					return value;
				}
				return value;
			}
		}

		public List<MethodCounter> GetProfile()
		{
			List<MethodCounter> list = new List<MethodCounter>();
			lock (_counters)
			{
				long[] value = new long[_ProfileTicks.Length];
				long[] array = Interlocked.Exchange(ref _ProfileTicks, value);
				for (int i = 0; i < _profiles.Count; i++)
				{
					for (int j = 0; j < array.Length; j++)
					{
						if (j < _profiles[i].Length)
						{
							array[j] += _profiles[i][j];
						}
					}
				}
				foreach (KeyValuePair<string, int> counter in _counters)
				{
					string methodName = counter.Key;
					string fileName = null;
					int line = 0;
					if (RubyStackTraceBuilder.TryParseRubyMethodName(ref methodName, ref fileName, ref line))
					{
						list.Add(new MethodCounter(methodName, fileName, line, array[counter.Value]));
					}
				}
				return list;
			}
		}
	}
}
