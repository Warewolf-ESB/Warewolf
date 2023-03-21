using System.Collections.Generic;
using System.Reflection;

namespace IronRuby.Runtime
{
	internal sealed class RubyMethodDebugInfo
	{
		private static readonly Dictionary<string, RubyMethodDebugInfo> _Infos = new Dictionary<string, RubyMethodDebugInfo>();

		private readonly List<int> _offsets = new List<int>();

		private readonly List<int> _lines = new List<int>();

		public static bool TryGet(MethodBase method, out RubyMethodDebugInfo info)
		{
			lock (_Infos)
			{
				return _Infos.TryGetValue(method.Name, out info);
			}
		}

		public static RubyMethodDebugInfo GetOrCreate(string methodName)
		{
			lock (_Infos)
			{
				RubyMethodDebugInfo value;
				if (!_Infos.TryGetValue(methodName, out value))
				{
					value = new RubyMethodDebugInfo();
					_Infos.Add(methodName, value);
				}
				return value;
			}
		}

		public void AddMapping(int ilOffset, int line)
		{
			_offsets.Add(ilOffset);
			_lines.Add(line);
		}

		public int Map(int ilOffset)
		{
			int num = _offsets.BinarySearch(ilOffset);
			if (num >= 0)
			{
				return _lines[num];
			}
			num = ~num;
			if (num > 0)
			{
				return _lines[num - 1];
			}
			if (_lines.Count > 0)
			{
				return _lines[0];
			}
			return 0;
		}
	}
}
