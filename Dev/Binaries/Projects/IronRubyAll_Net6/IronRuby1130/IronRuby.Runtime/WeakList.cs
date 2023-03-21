using System;
using System.Collections;
using System.Collections.Generic;

namespace IronRuby.Runtime
{
	internal sealed class WeakList<T> : IEnumerable<T>, IEnumerable where T : class
	{
		private readonly List<WeakReference> _list = new List<WeakReference>();

		public IEnumerator<T> GetEnumerator()
		{
			int deadCount = 0;
			for (int i = 0; i < _list.Count; i++)
			{
				object item = _list[i].Target;
				if (item != null)
				{
					yield return (T)item;
				}
				else
				{
					deadCount++;
				}
			}
			if (deadCount > 5 && deadCount > _list.Count / 5)
			{
				Prune();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(WeakReference item)
		{
			_list.Add(item);
		}

		private void Prune()
		{
			int i = 0;
			int num = 0;
			for (; i < _list.Count; i++)
			{
				if (_list[i].IsAlive)
				{
					if (num != i)
					{
						_list[num] = _list[i];
					}
					num++;
				}
			}
			if (num < i)
			{
				_list.RemoveRange(num, i - num);
				_list.TrimExcess();
			}
		}
	}
}
